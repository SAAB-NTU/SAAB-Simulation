using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class multi_beam : MonoBehaviour
{
    string folder;
    public float multiplier,FOV,divide,distance,multiplier_x,SonarNum;
    List<single_beam> beams;
    BinaryFormatter converter = new BinaryFormatter();
    FileStream dataStream;
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
        folder = new string(Path.Join(Directory.GetCurrentDirectory(), DateTime.Now.ToString("ddMMyy_hhmmss")+" "+SonarNum.ToString()));
        Directory.CreateDirectory(folder);
        for (int i = 0; i < multiplier; ++i)
        {
            float value;
            //Instantiate probe 
            //print(i);

            if (multiplier == 1) {
                value = 0;
            }
            else
            {
                value = -FOV / 2 + i * (FOV / (multiplier - 1));
            }
            //float value = -FOV / 2 + i * (FOV / (multiplier - 1));
            vals.Add(value);
            //vals_y.Add(value);
            beams.Add(new single_beam(value,multiplier_x, divide, distance, transform, sound_prop));

        }
    }

    // Update is called once per frame   
    void Update()
    {
        if (j % 1 == 0)
        {
            //string file_path = Path.Join(folder, j.ToString().PadLeft(6, '0') + "_" + ".csv");
            //System.IO.StreamWriter file = new System.IO.StreamWriter(file_path, true, Encoding.UTF8, 65536);
            //final_vals = new List<List<float>>();
            for (int i=0;i<beams.Count; ++i)
            {
                
                beams[i].beam_hit(null);
                //final_vals.Add(beam.rs);
            }
           // file.Close();
        }

      //  for (int i = 0; i < final_vals.Count; ++i)
      //  {
            /*
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Join(folder, j.ToString().PadLeft(6, '0') + "_" + ".csv"), true))
            {

                file.WriteLine(string.Join(",", final_vals[i]));

            }
            */

            /*
            dataStream = new FileStream(Path.Join(folder, j.ToString().PadLeft(6, '0') + "_" + ".dat"), FileMode.Create);

            // Serialize GameData into binary data 
            //  and add it to an existing input stream.
            converter.Serialize(dataStream, final_vals);

            // Close stream.
            dataStream.Close();
            */
      //  }
        j++;
        
    }
}
