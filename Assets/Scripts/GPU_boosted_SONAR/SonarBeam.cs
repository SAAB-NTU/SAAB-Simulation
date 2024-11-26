using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Assertions.Must;

[BurstCompile]
public class SonarBeam : MonoBehaviour
{
    // For Oculus M1200d, the horizontal range is 130deg and vertical range is 20deg
    private float horizontalAperture, verticalAperture;
    public int imageWidth, imageHeight, imageSize; // output image size in pixels
    private Vector3 upperLeftBorder, currentBeam;
    private Matrix4x4 horizontalRotation, verticalRotation;
    public byte[] outputRaycast;

    public SonarBeam() 
    {
        this.horizontalAperture = 130;
        this.verticalAperture = 20;

        //SONAR output image properties
        this.imageWidth = 516;
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

    }

    void Update()
    {
        // Profiler.BeginSample("CastBeam");
        float start_time = Time.realtimeSinceStartup;
        CastBeam();
        Debug.Log("Time taken: " + ((Time.realtimeSinceStartup - start_time)*1000f) + "ms");
        // Profiler.EndSample();
    }

    private void CastBeam() 
    {
        Vector3 position = transform.position; // Update SONAR position
        NativeArray<Vector3> directions = PopulateDirections();

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
        // Convert RaycastHit into an image grid of Raycast Distance
        // outputRaycast.Dispose(); // Dispose the previous outputRaycast
        for (int index = 0; index < results.Length; index++)
        {
            outputRaycast[index] = (byte) results[index].distance;
        }
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
            position, radius, direction, QueryParameters.Default, 10
        );
    }
}
