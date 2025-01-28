using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;


public class FloatListData
{
    public List<float> Values;
    public List<int> instances;
}

public class Raycast_Gizmo : MonoBehaviour
{
  

    Transform tr;
    public float distance,radius,FOV,FOV_x;
    public int multiplier, multiplier_x;
    RaycastHit hit;
    int angle;
    public int no_jobs;
    List<Quaternion> angles;
    List<RaycastHit> hits;
    NativeArray<RaycastHit> results;
    NativeArray<SpherecastCommand> commands;
    [SerializeField]
    FloatListData distances;
    string data,folder;
    ROSConnection ros;
    public string topicName = "/Sonar/Image";
    private RosMessageTypes.Std.HeaderMsg header;
    public byte[] outputRaycast;
    private void Awake()
    {
        folder = new string(Path.Join(Directory.GetCurrentDirectory(),"SONAR_Outputs", DateTime.Now.ToString("ddMMyy_hhmmss") + " " + gameObject.name.ToString()));
        Directory.CreateDirectory(folder);
        tr = transform;
        angles = new List<Quaternion>();
        hits = new List<RaycastHit>();
        distances = new FloatListData { Values = new List<float>(), instances=new List<int>() };
        data = new string("");
        for (int i = 0; i < multiplier; ++i)
        {
            float value;
            if (multiplier == 1)
            {
                value = 0;
            }
            else
            {
                value = -FOV / 2 + i * (FOV / (multiplier - 1));
            }
            for (int j = 0; j < multiplier_x; ++j)
            {
                float value_i = -FOV_x / 2 + j * (FOV_x / (multiplier_x - 1));
                Quaternion direction = Quaternion.AngleAxis(value_i, tr.up);
                direction = direction * Quaternion.AngleAxis(value, tr.right);
                angles.Add(direction);
               
            }
        }
        results = new NativeArray<RaycastHit>(Convert.ToInt32(multiplier * multiplier_x), Allocator.Persistent);
        commands = new NativeArray<SpherecastCommand>(Convert.ToInt32(multiplier * multiplier_x), Allocator.Persistent);
    }
    private void OnDrawGizmosSelected()
    {
        if (hits != null)
        {
            for (int i = 0; i < hits.Count; ++i)
            {
                draw_raycasts(i);
            }
        }

    }
    void draw_raycasts(int i)
    {
        if (hits[i].distance == 0)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(tr.position, angles[i]*tr.forward * distance);
            
        }
        else
        {
            //print("hit");
            Gizmos.color = Color.red;
            Gizmos.DrawRay(tr.position, tr.forward * hits[i].distance);
            Gizmos.DrawWireSphere(tr.position+ angles[i] * tr.forward * hits[i].distance, radius);
        }

    }
    [BurstCompile]
    private void Update()
    {

        hits.Clear();
        distances.Values.Clear();
        distances.instances.Clear();

        Vector3 position = transform.position;
        NativeArray<Vector3> directions = PopulateDirections();

        NativeArray<RaycastHit> results = new(this.multiplier*this.multiplier_x, Allocator.TempJob);
        NativeArray<SpherecastCommand> commands = new(this.multiplier*this.multiplier_x, Allocator.TempJob);

        for (int i = 0; i < angles.Count; ++i)

        {
            commands[i] = new SpherecastCommand(tr.position, radius, angles[i] * tr.forward,distance);
            
            //Physics.SphereCast(tr.position, radius, angles[i] * tr.forward, out hit, distance);
           // hits.Add(hit);
            
        }

        PrepareSpherecastCommandJob prepareCommandsJob = new() {
            position = transform.position,
            directions = directions,
            commands = commands
        };
        
        JobHandle handle = prepareCommandsJob.ScheduleParallel(commands.Length, 64, default);
        
        handle.Complete();
       
        for (int i = 0; i < angles.Count; ++i)

        {
            //Physics.SphereCast(tr.position, radius, angles[i] * tr.forward, out hit, distance);
            hits.Add(results[i]);
            if (results[i].distance > 0)
            {
                distances.Values.Add(results[i].distance);
                distances.instances.Add(i);
            }
        }

        
            data= JsonUtility.ToJson(distances);

        File.WriteAllText(Path.Join(folder, Time.frameCount.ToString().PadLeft(6, '0')+" .json"), data);

        //  results.Dispose();
        //commands.Dispose();

    }
    private void OnDestroy()
    {
        results.Dispose();
        commands.Dispose();
    }

    private NativeArray<Vector3> PopulateDirections()
    {
        NativeArray<Vector3> directions = new (this.multiplier*this.multiplier_x,Allocator.TempJob);
        for (int height = 0; height < multiplier; ++height)
        {
            float value;
            if (multiplier == 1)
            {
                value = 0;
            }
            else
            {
                value = -FOV / 2 + height * (FOV / (multiplier - 1));
            }
            for (int width = 0; width < multiplier_x; ++width)
            {
                float value_i = -FOV_x / 2 + width * (FOV_x / (multiplier_x - 1));
                Quaternion direction = Quaternion.AngleAxis(value_i, tr.up);
                direction = direction * Quaternion.AngleAxis(value, tr.right);
                directions[height*multiplier_x + width] = direction*tr.forward;
               
            }
        }
        return directions;
    }

    private void ProcessRaycastHit(NativeArray<RaycastHit> hit)
    {

    }

/*
    private void PublishROSMessage()
    {
        ImageMsg msg = PopulateImageMsg();
        ros.Publish(topicName, msg);
    }
*/
/*    private ImageMsg PopulateImageMsg()
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
    }*/


}

// [BurstCompile]
// public struct PrepareSpherecastCommandJob : IJobFor
// {
//     [ReadOnly]
//     public NativeArray<Vector3> directions;
//     [ReadOnly]
//     public Vector3 position;
//     [NativeDisableParallelForRestriction]
//     public NativeArray<SpherecastCommand> commands;
//     public float radius;
//     public void Execute(int index)
//     {
//         Vector3 direction = this.directions[index];
//         this.commands[index] = new SpherecastCommand(
//             position, radius, direction, QueryParameters.Default, 30
//         );
//     }
// }
