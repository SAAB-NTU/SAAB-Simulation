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

public class newtest : MonoBehaviour
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

    private void Update()
    {

        hits.Clear();
        distances.Values.Clear();
        distances.instances.Clear();

         for (int i = 0; i < angles.Count; ++i)

        {

            Debug.Log(angles[i]);
            Debug.DrawRay(tr.position, angles[i]*tr.forward * distance);
        }

    }
}