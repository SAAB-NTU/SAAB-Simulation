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
        c = 1484; // m/s
        K = 100;
        L = maxRange;
        lambda = c/(f*1000);
        double f_sqr = Math.Pow(f,2);
        alpha = 0.1*f_sqr/(1+f_sqr) + 40*f_sqr/(4100 + f_sqr);
        Z_H20 = 1.48; // MRayl (megaRayleigh)
        Z_Obj = 45.7; // Steel
        rmsRoughness = 0.001;
        // alpha = 0.1*f_sqr/(1+f_sqr) + 40*f_sqr/(4100 + f_sqr) + 2.75e-4*f_sqr; // Additional term

        TL_dB_const = -alpha/1000;
        R_theta_impedance_term = (Z_Obj - Z_H20)/(Z_Obj + Z_H20);
        // R_theta_const_exp_term = 8*Math.Pow(Math.PI,2)*Math.Pow(rmsRoughness,2)/Math.Pow(lambda,2);
        R_theta_const_exp_term = 8*Math.Pow(Math.PI*rmsRoughness/lambda,2);
        sigma = K*L*angularResolution;
        Debug.Log(sigma);
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

    byte[] SonarCalculations(
        ref NativeArray<RaycastCommand> raycastCommand,
        ref NativeArray<RaycastHit> raycastMap
        )
    {   
        // Each ray holds 2 value: range, intensity
        double[] backscatteringMap = new double[beamNum*rayNum*2];
        double[] beamCentralValue = new double[beamNum];
        double[] intensityMap = new double[beamNum*rayNum];
        byte[] finalMap = new byte[beamNum*rayNum];
        
        for (int beam = 0; beam < beamNum; beam++)
        {
            double[] beamBin = new double[rayNum];
            for (int ray = 0; ray < rayNum; ray++)
            {
                RaycastHit raycastHit = raycastMap[beam*rayNum + ray];

                // Calculations
                double TL_dB = TL_dB_const*raycastHit.distance;
                double TL = Math.Pow(10,Math.Abs(TL_dB)/20);
                double incidence = Vector3.Angle(
                    raycastHit.normal,
                    raycastCommand[beam*rayNum + ray].direction
                );

                // double theta = incidence *(Math.PI/180); // GrazingAngle = IncidenceAngle - 90, converted to radians
                // double R_theta_exp_term = Math.Pow(Math.E,
                //                         -(R_theta_const_exp_term*Math.Pow(Math.Sin((incidence-90)*DEG_TO_RAD),2))
                //                         );
                // double R_theta = R_theta_impedance_term * R_theta_exp_term;

                double R_theta = Sigmoid((incidence-90)/90); // grazing angle is normalized before passing into Sigmoid
                double I_b = Math.Abs(TL*R_theta);

                backscatteringMap[beam*rayNum + ray*2]     = raycastHit.distance;
                backscatteringMap[beam*rayNum + ray*2 + 1] = I_b;
                // backscatteringMap[ray*beamNum + beam*2]     = raycastHit.distance;
                // backscatteringMap[ray*beamNum + beam*2 + 1] = I_b;

            }
        }

        for (int beam = 0; beam < beamNum; beam++)
        {
            double x_i = beamCentralValue[beam];
            for (int ray = 0; ray < rayNum; ray++)
            {
                double range = backscatteringMap[beam*rayNum + ray*2];

                if (range != 0)
                {
                    double I_b = backscatteringMap[beam*rayNum + ray*2 + 1];
                    int mapIndex = (int)(Math.Min(range/binRange, rayNum - 1));
                    if (mapIndex >= rayNum)
                    {
                        Debug.Log("MapIndex out of range at " + mapIndex + " range: " + range + " binRange: " + binRange);
                    }
                    double exp = -Math.Pow(range - x_i,2)/sigma;
                    double value = I_b*Math.Pow(Math.E, exp);
                    // intensityMap[beam*rayNum + mapIndex] += value;
                    intensityMap[mapIndex*beamNum + beam] += value;

                    // Debug.Log(mapIndex);
                    // Debug.Log("beam: " + beam + " sigma: " + sigma +" x_i: " + x_i + " range: " + range + " I_b: " + I_b + " distanceBin: " + mapIndex + " exp: " + exp + " value: " + intensityMap[beam*rayNum + mapIndex]);
                }
            }
        }

        // Buffer.BlockCopy(intensityMap, 0, finalMap, 0, intensityMap.Length);  

        for (int i = 0; i < intensityMap.Length; i++)
        {
            finalMap[i] = (byte)Math.Min(255, Math.Max(0, intensityMap[i] * 255));
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