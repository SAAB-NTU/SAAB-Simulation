using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class multi_beam : MonoBehaviour
{
    string folder;
    public float multiplier,FOV,divide,distance,multiplier_x;
    List<single_beam> beams;
    List<float> vals;
    int j;
    List<List<float>> final_vals;
    public sound_props sound_prop;
    // Start is called before the first frame update
    void Start()
    {
        j = 0;
        beams = new List<single_beam>();
        vals = new List<float>();
        folder = new string(Path.Join(Directory.GetCurrentDirectory(), DateTime.Now.ToString("ddMMyy_hhmmss")));
        Directory.CreateDirectory(folder);
        for (int i = 0; i < multiplier; ++i)
        {

            //Instantiate probe 
            print(i);
            float value = -FOV / 2 + i * (FOV / (multiplier - 1));
            vals.Add(value);
            //vals_y.Add(value);
            beams.Add(new single_beam(value,multiplier_x, divide, distance, transform, sound_prop));

        }
    }

    // Update is called once per frame
    void Update()
    {
        final_vals = new List<List<float>>();
        foreach(single_beam beam in beams)
        {
            beam.beam_hit();
            final_vals.Add(beam.rs);
        }
        for (int i = 0; i < final_vals.Count; ++i)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Join(folder, j.ToString().PadLeft(6, '0') + "_" + ".csv"), true))
            {

                file.WriteLine(string.Join(",", final_vals[i]));

            }
        }
        j++;
    }
}
