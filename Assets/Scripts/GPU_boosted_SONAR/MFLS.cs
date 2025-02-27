using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using RosMessageTypes.Sensor;

// using Beam = Unity.Collections.NativeArray<UnityEngine.Vector3>; // A beam is a collection of rays (Vector3)
// using MultiBeam = Unity.Collections.NativeArray<
//     Unity.Collections.NativeArray<UnityEngine.Vector3>
// >;

using Unity.VisualScripting;
using System;
using Unity.Robotics.ROSTCPConnector;

[BurstCompile]
public class MFLS : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float horizontalAperture, verticalAperture; // 130/20 or 60/12

    public int mode; // 1 for 1.2MHz, 2 for 2.1MHz
    private float f; // Operating Frequency, 1200 or 2100 in kHz for M1200d
    private float maxRange;
    private float minRange;
    private double rangeResolution;
    private float angularResolution;
    private float beamSeparation;
    private int rayNum;
    private int beamNum;
    private int imageHeight;
    private int imageWidth;
    private double binRange;
    private float c; // Speed of sound in water
    private float lambda; // SONAR wavelength
    private double rmsRoughness; // Surface root mean square roughness
    private double Z_H20; // acoustic water impedance
    private double Z_Obj; // collided object acoustic impedance
    private double alpha; // intrinsic attenuation
    private double TL_dB_const; // The constant term of Transmission Loss
    private double R_theta_impedance_term; // The impedance term of reflection coefficient
    private double R_theta_const_exp_term;
    // The constant term inside the exponential of reflection coefficient
    private double K; // Arbitrary parameter
    private double L; // Length L of the considered ray
    private double sigma; // Standard Deviation of the Gaussian model of the SONAR signal
    private const double DEG_TO_RAD = Math.PI / 180.0;

    private byte[] outputMap;

    // Coefficients for Sigmoid function
    private double Sigmoid_beta;
    private double Sigmoid_x0;

    // Coefficients for ray Gaussian Model
    private double EchoGaussian_mean;
    private double EchoGaussian_std;
    // private double[] GaussianKernel = {0.011f,0.135f,0.607f,1.0f,0.607f,0.135f,0.011f}; // 1D Gaussian Kernel, corresponding to 1,2,3sigma
    private double[] GaussianKernel = {1.0f,0.6907f,0.835f,0.011f}; // 1D Gaussian Kernel
    ROSConnection ros;
    [SerializeField] string topicName = "/sonar/image"; 
    

    void Start()
    {   
        InitializeMode();
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);
    }

    // Update is called once per frame
    void Update()
    {
        Cast();
    }

    void InitializeMode()
    {   
        switch (mode)
        {
            case 1: // 1.2MHz
                f = 1200f;
                maxRange = 40f;
                minRange = 0.1f;
                rangeResolution = 2.5e-3;
                horizontalAperture = 130f;
                verticalAperture = 20f;
                angularResolution = 0.6f;
                beamSeparation = 0.25f;

                beamNum = 513;
                rayNum = 261;
                imageWidth = beamNum;
                imageHeight = rayNum;
                break;
            case 2: // 2.1MHz
                f = 2100f;
                maxRange = 10f;
                minRange = 0.1f;
                rangeResolution = 2.5e-3;
                horizontalAperture = 60f;
                verticalAperture = 12f;
                angularResolution = 0.4f;
                beamSeparation = 0.16f;

                rayNum = 20;
                beamNum = 516;
                break;
            default:
                Debug.Log("No matching mode. Initializing to 1.2MHz operating frequency");
                f = 1200f;
                maxRange = 40f;
                minRange = 0.1f;
                rangeResolution = 2.5e-3;
                horizontalAperture = 130f;
                verticalAperture = 20f;
                angularResolution = 0.6f;
                beamSeparation = 0.25f;

                rayNum = 20;
                beamNum = 516;
                break;
        }
        
        binRange  = (maxRange - minRange)/imageHeight;
        K = 100;
        L = maxRange;
        lambda = c/(f*1000);
        double f_sqr = Math.Pow(f,2);
        alpha = 0.1*f_sqr/(1+f_sqr) + 40*f_sqr/(4100 + f_sqr);

        TL_dB_const = -alpha/1000;
        sigma = K*L*angularResolution;

        // Debug.Log(string.Format(
        //     "Sonar Parameters:\n" +
        //     "Bin Range: {0:F4} m\n" +
        //     "K: {1}\n" +
        //     "L (Max Range): {2:F2} m\n" +
        //     "Lambda: {3:F6} m\n" +
        //     "Frequency: {4:F2} kHz\n" +
        //     "Alpha: {5} dB/m\n" +
        //     "TL_dB_const: {6}\n" +
        //     "Sigma: {9:F6}",
        //     binRange,
        //     K,
        //     L,
        //     lambda,
        //     f,
        //     alpha,
        //     TL_dB_const,
        //     sigma
        // ));
    }

    void Cast()
    {
        Vector3 position = transform.position; // Update SONAR position
        NativeArray<Vector3> directions = PopulateMultiBeam();
        NativeArray<RaycastHit> results = new(rayNum*beamNum, Allocator.TempJob);
        NativeArray<RaycastCommand> commands = new(rayNum*beamNum, Allocator.TempJob);

        PrepareRaycastCommandJob prepareCommandsJob = new() {
            position = position,
            directions = directions,
            commands = commands,
            distance = maxRange
        };

        JobHandle handle = prepareCommandsJob.ScheduleParallel(commands.Length, 64, default);

        int commandsPerJob = Mathf.Max(beamNum*rayNum / JobsUtility.JobWorkerCount, 1);
        handle = RaycastCommand.ScheduleBatch(commands, results, commandsPerJob, handle);

        handle.Complete();

        byte[] map = SonarCalculations(ref commands, ref results);
        PublishROSMessage(ref map);

        directions.Dispose();
        results.Dispose();
        commands.Dispose();
    }

    private void PublishROSMessage(ref byte[] image)
    {
        ImageMsg message = new ImageMsg();

        DateTime now = DateTime.UtcNow;
        long seconds = new DateTimeOffset(now).ToUnixTimeSeconds();
        long nanoseconds = (now.Ticks % TimeSpan.TicksPerSecond) * 100;

        // Populate the header
        message.header.stamp.sec = (int) seconds;
        message.header.stamp.nanosec = (uint) nanoseconds;
        message.header.frame_id = "sonar_link";  // Set frame of reference
        message.encoding = "mono8";
        message.step = (uint) beamNum;
        message.width = (uint) beamNum;
        message.height = (uint) rayNum;
        message.is_bigendian = 0;
        message.data = image;

        ros.Publish(topicName, message);
    }

    NativeArray<Vector3> PopulateMultiBeam()
    {
        // Initialize NativeArray of Beams
        NativeArray<Vector3> multiBeam = new (beamNum*rayNum, Allocator.TempJob);
        /*
                Beam1   Beam2   BeamN 
        Ray1    |       |       |
        Ray2    |       |       |
        Ray3    |       |       |
        RayN    |       |       |
        */

        for (int beam = 0; beam < beamNum; beam++)
        {
            float beam_angle = horizontalAperture/2 - beam*(horizontalAperture / (beamNum -1));

            for (int ray = 0; ray < rayNum; ray++)
            {
                float ray_angle = verticalAperture/2 - ray*(verticalAperture/(rayNum -1));
                Quaternion direction = Quaternion.AngleAxis(ray_angle, transform.up);
                direction *= Quaternion.AngleAxis(beam_angle, transform.right);
                multiBeam[beam*rayNum + ray] = direction * transform.forward;
                // Debug.DrawRay(transform.position,direction*transform.forward*10, Color.yellow);
            }
        }

        return multiBeam; 
    }

    double RayGaussian(double x, double x_j)
    {
        // params:
        //   x   : current range corresponding to the bin
        //   x_j : central location distance of interest
        return Math.Pow(Math.E, (-1/2) * Math.Pow((x-x_j)/sigma,2));
    }

    double Sigmoid(double x)
    {
        return 0.5 * Math.Tanh(0.5 * Sigmoid_beta * (x - Sigmoid_x0)) + 0.5;
    }

    void ApplySpeckleNoise(ref double[] bins)
    {
        double stddev = 0.05;
        double min_value = 0.03;
        for (int i = 0; i < bins.Length; i++)
        {
            // if (bins[i] < min_value)
            // {
            //     bins[i] = min_value;
            // }
            var rand = UnityEngine.Random.Range(-5f,5f);
            bins[i] += 0.25*Math.Exp(-0.5 * Math.Pow((rand) / stddev, 2));

        }
    }

    byte[] SonarCalculations(
        ref NativeArray<RaycastCommand> raycastCommand,
        ref NativeArray<RaycastHit> raycastMap
        )
    {   
        byte[] finalMap = new byte[beamNum*rayNum];
        
        for (int beam = 0; beam < beamNum; beam++)
        {
            double[] beamBin = new double[rayNum];
            for (int ray = 0; ray < rayNum; ray++)
            {
                RaycastHit raycastHit = raycastMap[beam*rayNum + ray];

                // Calculations
                double TL_dB = TL_dB_const*raycastHit.distance;
                double TL = Math.Pow(10,TL_dB/20); // Normalized in [0,1] range
                double incidence = Vector3.Angle(
                    raycastHit.normal,
                    raycastCommand[beam*rayNum + ray].direction
                );

                double R_theta = Sigmoid((incidence-90)/90); // grazing angle is normalized before passing into Sigmoid
                double I_b = Math.Abs(TL*R_theta);

                int binIndex = (int)(Math.Min(raycastHit.distance/binRange, rayNum - 1));

                // Debug.Log(string.Format(
                //     "Beam: {0}, Ray: {1}\n" +
                //     "Distance: {2:F2}\n" +
                //     "TL_dB: {3:F2}\n" +
                //     "TL: {4:F4}\n" +
                //     "Incidence Angle: {5:F2}\n" +
                //     "R_theta: {6:F4}\n" +
                //     "I_b: {7:F4}\n" +
                //     "binIndex: {8}",
                //     beam, ray,
                //     raycastHit.distance,
                //     TL_dB,
                //     TL,
                //     incidence,
                //     R_theta,
                //     I_b,
                //     binIndex
                // ));


                for (int i = -(GaussianKernel.Length-1)/2 ; i <= (GaussianKernel.Length-1)/2; i++)
                {
                    int index = binIndex + i;
                    if ((index >= beamBin.Length) || (index < 0))
                    {
                        continue;
                    }
                    beamBin[binIndex + i] = I_b*GaussianKernel[Math.Abs(i)];
                }

            }

            ApplySpeckleNoise(ref beamBin);
                
            for (int i = 0; i < beamBin.Length; i++)
            {
                finalMap[i*beamNum + beam] = (byte) Mathf.Clamp((float)beamBin[i]*255*2, 0, 255);
                // if (finalMap[i*beamNum + beam] != 0)
                // {
                //     Debug.Log("At index: " + i*beamNum + beam + " value: " + finalMap[i*beamNum + beam]);
                // }
            }

            // for (int i = 0; i< beamBin.Length; i++)
            // {
            //     if (beamBin[i] != 0)
            //     {
            //         Debug.Log(beamBin[i]);
            //     }
            // }
        }

        return finalMap;
    }
}


[BurstCompile]
public struct PrepareRaycastCommandJob : IJobFor
{
    [ReadOnly]
    public NativeArray<Vector3> directions;
    [ReadOnly]
    public Vector3 position;
    [ReadOnly]
    public float distance;
    [NativeDisableParallelForRestriction]
    public NativeArray<RaycastCommand> commands;
    public void Execute(int index)
    {
        Vector3 direction = directions[index];
        commands[index] = new RaycastCommand(
            position, direction, QueryParameters.Default, distance
        );
    }
}