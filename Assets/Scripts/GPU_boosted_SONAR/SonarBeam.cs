using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System;

[BurstCompile]
public class SonarBeam : MonoBehaviour
{
    // For Oculus M1200d, the horizontal range is 130deg and vertical range is 20deg
    private float horizontalAperture, verticalAperture;
    public int imageWidth, imageHeight, imageSize; // output image size in pixels
    private Vector3 upperLeftBorder, currentBeam;
    private Matrix4x4 horizontalRotation, verticalRotation;
    // public bool readyToPublish = false;
    ROSConnection ros;
    public string topicName = "/Sonar/Image";
    public float publishMessageFrequency = 1f;
    private RosMessageTypes.Std.HeaderMsg header;
    public byte[] outputRaycast;
    Transform tr;

    public SonarBeam() 
    {
        this.horizontalAperture = 130;
        this.verticalAperture = 20;

        //SONAR output image properties
        this.imageWidth = 20;
        this.imageHeight = 371;
        this.imageSize = this.imageWidth * this.imageHeight;
    }

    void Awake() 
    {
        ProcessBoundaries();
        this.currentBeam = upperLeftBorder;
    }

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);
    }

    void Update()
    {
        // Profiler.BeginSample("CastBeam");
        float start_time = Time.realtimeSinceStartup;
        CastBeam();
        Debug.Log("Time taken: " + ((Time.realtimeSinceStartup - start_time)*1000f) + "ms");
        // Profiler.EndSample();
        // Debug.DrawRay(transform.position,transform.forward, Color.yellow);
    }

    private void CastBeam() 
    {
        Vector3 position = transform.position; // Update SONAR position
        NativeArray<Vector3> directions = PopulateDirectionsNew();

        NativeArray<RaycastHit> results = new(this.imageSize, Allocator.TempJob);
        NativeArray<SpherecastCommand> commands = new(this.imageSize, Allocator.TempJob);

        PrepareSpherecastCommandJob prepareCommandsJob = new() {
            position = position,
            directions = directions,
            commands = commands
        };

        JobHandle handle = prepareCommandsJob.ScheduleParallel(commands.Length, 64, default);

        int commandsPerJob = Mathf.Max(this.imageSize / JobsUtility.JobWorkerCount, 1);
        handle = SpherecastCommand.ScheduleBatch(commands, results, commandsPerJob, handle);

        handle.Complete();

        ProcessRaycastHit(results);
        PublishROSMessage();

        // Debug.Log("Length: " + results.Length + " first index: " + results[0].distance);

        directions.Dispose();
        results.Dispose();
        commands.Dispose();
    }

    private void ProcessBoundaries() 
    {
        // Derive upper left border of the SONAR image
        Vector3 centerBeam = transform.forward;

        Matrix4x4 upperLeftBorderRotation = GetRotationMatrix(-verticalAperture/2, -horizontalAperture,0);
        upperLeftBorder = upperLeftBorderRotation.MultiplyPoint3x4(centerBeam);

        // Matrix4x4 upperRightBorderRotation = GetRotationMatrix(-verticalAperture/2, horizontalAperture,0);
        // upperRightBorder = upperRightBorderRotation.MultiplyPoint3x4(centerBeam);

        // Matrix4x4 lowerLeftBorderRotation = GetRotationMatrix(verticalAperture/2, -horizontalAperture,0);
        // lowerLeftBorder = lowerLeftBorderRotation.MultiplyPoint3x4(centerBeam);

        // Matrix4x4 lowerRightBorderRotation = GetRotationMatrix(verticalAperture/2, horizontalAperture,0);
        // lowerRightBorder = lowerRightBorderRotation.MultiplyPoint3x4(centerBeam);

        horizontalRotation = GetRotationMatrix(0,horizontalAperture/imageWidth,0);
        verticalRotation = GetRotationMatrix(-verticalAperture/imageHeight,0,0);
    }

    private void ProcessRaycastHit(NativeArray<RaycastHit> results) 
    {
        // this.readyToPublish = false;
        // Convert RaycastHit into an image grid of Raycast Distance
        // outputRaycast.Dispose(); // Dispose the previous outputRaycast
        if (outputRaycast == null || outputRaycast.Length != results.Length)
        {
            outputRaycast = new byte[results.Length];
        }
        // int counter = 0;
        int max = -10000;
        for (int index = 0; index < results.Length; index++)
        {
            outputRaycast[index] = (byte) results[index].distance;
            if (outputRaycast[index] > max)
            {
                max = outputRaycast[index];
            }
        }

        if (max != 0)
        {
            for (int i = 0; i < outputRaycast.Length; i++)
            {
                outputRaycast[i] = (byte)(outputRaycast[i] / (float)max * 255);
            }

        }


        // Debug.Log("Counter: " + counter);

        // this.readyToPublish = true;

        // while (this.readyToPublish == true)
        // {}
        // Debug.Log("readyToPublish set to false from the Publisher");
    }

    private void PublishROSMessage()
    {
        ImageMsg msg = PopulateImageMsg();
        ros.Publish(topicName, msg);
    }

    private ImageMsg PopulateImageMsg()
    {
        ImageMsg message = new();

        // Get current time from system clock (using DateTime)
        DateTime now = DateTime.UtcNow;

        // Convert DateTime to timestamp (seconds and nanoseconds)
        long seconds = new DateTimeOffset(now).ToUnixTimeSeconds();
        long nanoseconds = (now.Ticks % TimeSpan.TicksPerSecond) * 100; // Convert ticks to nanoseconds

        // Populate the header
        message.header.stamp.sec = (int) seconds;
        message.header.stamp.nanosec = (uint) nanoseconds;
        message.header.frame_id = "camera_link";  // Set frame of reference
        message.encoding = "mono8";
        message.step = (uint) this.imageWidth;
        message.width = (uint) this.imageWidth;
        message.height = (uint) this.imageHeight;
        message.is_bigendian = 0;
        message.data = this.outputRaycast;
        return message;
    }

    private Matrix4x4 GetRotationMatrix(float x, float y, float z) {
        Quaternion quat = Quaternion.Euler(x,y,z);
        Matrix4x4 rotation_matrix = Matrix4x4.Rotate(quat);
        return rotation_matrix;
    }

    private NativeArray<Vector3> PopulateDirections() {
        NativeArray<Vector3> directions = new(this.imageSize,Allocator.TempJob);
        ProcessBoundaries();

        Vector3 currentBeam = upperLeftBorder;
        Vector3 tempBeam;

        for (int height = 0; height < imageHeight; height++) {
            tempBeam = verticalRotation.MultiplyPoint3x4(currentBeam);
            for (int width = 0; width < imageWidth; width++) {
                currentBeam = horizontalRotation.MultiplyPoint3x4(tempBeam);
                // Debug.DrawRay(transform.position, currentBeam*10, Color.yellow);
                directions[height*imageWidth + width] = currentBeam;
            }
        }

        return directions;
    }

    private NativeArray<Vector3> PopulateDirectionsNew()
    {
        NativeArray<Vector3> directions = new (this.imageSize,Allocator.TempJob);
        for (int height = 0; height < imageHeight; ++height)
        {
            float value;
            if (imageHeight == 1)
            {
                value = 0;
            }
            else
            {
                value = -verticalAperture / 2 + height * (verticalAperture / (imageHeight - 1));
            }
            for (int width = 0; width < imageWidth; ++width)
            {
                float value_i = -horizontalAperture / 2 + width * (horizontalAperture / (imageWidth - 1));
                Quaternion direction = Quaternion.AngleAxis(value_i, transform.up);
                direction = direction * Quaternion.AngleAxis(value, transform.right);
                directions[height*imageWidth + width] = direction*transform.forward;
               
            }
        }
        return directions;
    }
}

[BurstCompile]
public struct PrepareSpherecastCommandJob : IJobFor
{
    [ReadOnly]
    public NativeArray<Vector3> directions;
    [ReadOnly]
    public Vector3 position;
    [NativeDisableParallelForRestriction]
    public NativeArray<SpherecastCommand> commands;
    public float radius;
    public void Execute(int index)
    {
        Vector3 direction = this.directions[index];
        this.commands[index] = new SpherecastCommand(
            position, radius, direction, QueryParameters.Default, 30
        );
    }
}
