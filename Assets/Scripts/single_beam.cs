using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using System.Text;

public class single_beam : MonoBehaviour
{
    float FOV_x;
    
    //public GameObject prefab,coord,graph;
    Texture2D texture;
    public List<float> rs;
    List<List<float>> r;
    List<single_ray> rays;
    float divide;
    public Image output;
    // Start is called before the first frame update
    
    
    
    int j;
    //To update, beam pattern of receiver to vary with multiplier_x and divide, now done manually
    
    public single_beam(float value_i,float multiplier_x,float divide, float distance,Transform tr,sound_props sound_prop)
    {
        this.divide = divide;
        
        //vals_x = new List<float>();
       
        
        FOV_x = sound_prop.phi_t;

        rays = new List<single_ray>();
       
            
            for (int i = 0; i < multiplier_x; ++i)
            {
               
                
                //Instantiate probe 
                
                float value = -FOV_x / 2 + i * (FOV_x / (multiplier_x - 1));
           // print(value_i);
                Quaternion direction = Quaternion.AngleAxis(value_i, tr.up);
                direction = direction* Quaternion.AngleAxis(value, tr.right);
                //rays.Add(new single_ray(Quaternion.Euler(value, value_i, 0), divide, distance,tr,sound_prop));
                rays.Add(new single_ray(direction, divide, distance,tr,sound_prop));
            }
    
    }

    // Update is called once per frame
    public void beam_hit(StreamWriter file)
    {
        
        r = new List<List<float>>();
        rs = new List<float>();
        for(int i=0;i<divide;++i)
        {
            rs.Add(0);
        }
        
        foreach (single_ray ray in rays)
        {
            ray.ray_hit();
            //r.Add(ray.final);
            for(int i=0;i<divide;++i)
            {
                if(i<ray.final.Count)
                {
                    rs[i] += ray.final[i];
                }
            }
            //rs = ray.final;
        }

            
            //file.WriteLine(string.Join(",", rs));

        

        // using (System.IO.StreamWriter file = new System.IO.StreamWriter("array.csv"))
        // {

        //file.Write(string.Join(",", r[i]));
        // }
        //dt1 = DateTime.Now;
        //for (int i = 0; i < r.Count; ++i)
        //{


        //    //for (int j = 0; j < multiplier_x; ++j)
        //    //{

        //    //    texture.SetPixel(i, j, Color.white * (-r[i][j] / gain));
        //    //    texture.SetPixel(i, j, Color.red);
        //    //}

        //}

        ////texture.Apply();
        //j++;

    }
}
