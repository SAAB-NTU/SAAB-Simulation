using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class single_ray : sound_props
{
    public float hit_val,angle_normal,distance,divide;
    List<Color> colors;
    List<float> dists,T_L;
    void Start()
    {
       // distance=30;
        //divide = 3;
        colors = new List<Color>();
        
        for(int x=0;x<divide;++x)
        {
            colors.Add(Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        }
    }

    private void FixedUpdate()
    {
        dists = new List<float>();
        T_L = new List<float>();
        for (int i = 1; i < divide + 1; ++i)
        {
            //float d = (divide / (2 * distance));
            float d = (distance * (i-0.5f))/divide;
            float alphaT = ((2f * d - 1f) * alphaW) / 1000f;

            float S_L = 40f * Mathf.Log(d, 10f);
            //print(r / scale);
            float transmission_loss = S_L + alphaT;
            Debug.DrawRay(transform.position + transform.TransformDirection(Vector3.forward) * distance * (i - 1) / divide, transform.TransformDirection(Vector3.forward) * distance * i / divide, colors[i - 1]);
            dists.Add(d);
            T_L.Add(transmission_loss);
        }
        
        //RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        //if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distance))
        //{
            
            
        //     angle_normal=Mathf.Deg2Rad*180-Mathf.Acos(Vector3.Dot(Quaternion.Inverse(transform.rotation) * hit.normal, Vector3.forward));
        //    //Quaternion rot = Quaternion.AngleAxis(angle,Vector3.one);
        //    //print(angle.ToString("n2"));
        //    hit_val = hit.distance;
        //    //Vector3 actual_hit=Quaternion.Inverse(transform.rotation)*hit.point;
        //    // Debug.Log("Did Hit");
        //   if(hit.distance<distance)
        //    {
           
        //        Debug.DrawRay(hit.point, transform.TransformDirection(Vector3.Reflect(Vector3.forward,Quaternion.Inverse(transform.rotation)*hit.normal)) * (distance-hit.distance), Color.red);
          
        //    }
            
        //}
        //else
        //{
        //    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * distance, Color.white);
        //    // Debug.Log("Did not Hit");
        //    hit_val = -1f;
        //}
        
    }
}
