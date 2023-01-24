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


public class FloatListData
{
    public List<float> Values;
    public List<int> instances;
}

public class Raycast_Gizmo : MonoBehaviour
{
  

    Transform tr;
    public float distance,radius,multiplier,multiplier_x,FOV,FOV_x;
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
        //print(Convert.ToInt32(multiplier * multiplier_x));


        for (int i = 0; i < angles.Count; ++i)

        {
            commands[i] = new SpherecastCommand(tr.position, radius, angles[i] * tr.forward,distance);
            
            //Physics.SphereCast(tr.position, radius, angles[i] * tr.forward, out hit, distance);
           // hits.Add(hit);
            
        }
        
        var handle = SpherecastCommand.ScheduleBatch(commands, results,no_jobs, default(JobHandle));
        
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
   
    // Start is called before the first frame update


    // Update is called once per frame

}
