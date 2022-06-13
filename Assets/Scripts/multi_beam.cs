using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class multi_beam : MonoBehaviour
{
    float FOV_x, FOV_y;
    public float multiplier_x, multiplier_y,distance,divide,gain;
    //public GameObject prefab,coord,graph;
    Texture2D texture;
    List<float> vals_x,vals_y,rs;
    List<List<float>> r;
    List<single_ray> rays;
    public Image output;
    // Start is called before the first frame update
    public sound_props sound_prop;
    //To update, beam pattern of receiver to vary with multiplier_x and divide, now done manually

    void Start()
    {

        
        vals_x = new List<float>();
        vals_y = new List<float>();
        //texture = new Texture2D(((int)divide),(int)multiplier_x);
       // texture.SetPixels(output.sprite.texture.GetPixels());
        //texture.Apply();
        //output.sprite = Sprite.Create(texture,output.sprite.rect,output.sprite.pivot);
        FOV_x = sound_prop.theta_t;
        FOV_y = sound_prop.phi_t;

        rays = new List<single_ray>();
        for (int j = 0; j < multiplier_y; ++j)
        {

            for (int i = 0; i < multiplier_x; ++i)
            {
                //float value_i = -FOV_y / 2 + j * (FOV_y / (multiplier_y - 1));
                float value_i = 0;
                //Instantiate probe 
                
                float value = -FOV_x / 2 + i * (FOV_x / (multiplier_x - 1));
                vals_x.Add(value_i);
                vals_y.Add(value);
                rays.Add(new single_ray(Quaternion.Euler(value_i, value, 0), divide, distance,transform,sound_prop));
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        r = new List<List<float>>();
        foreach(single_ray ray in rays)
        {
            
          ray.ray_hit();
            r.Add(ray.final);
            rs = ray.final;
        }
        using (System.IO.StreamWriter file = new System.IO.StreamWriter("array.csv"))
        {

            //file.Write(string.Join(",", r[i]));
        }
        for (int i = 0; i < divide; ++i)
        {

            using (System.IO.StreamWriter file = new System.IO.StreamWriter("array"+i.ToString()+".csv"))
            {
                
                file.Write(string.Join(",", r[i]));
  
            }
            //for (int j = 0; j < multiplier_x; ++j)
            //{

            //    texture.SetPixel(i, j, Color.white * (-r[i][j] / gain));
            //    texture.SetPixel(i, j, Color.red);
            //}

        }

        //texture.Apply();


    }
}
