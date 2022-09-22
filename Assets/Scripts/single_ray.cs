using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class single_ray : MonoBehaviour
{
    public single_ray(Quaternion angle,float divide,float distance,Transform tr,sound_props sound_prop)
    {
        //print("created_ray");
        this.distance = distance;
        this.divide = divide;
        this.angle = angle;
        this.sound_prop = sound_prop;
        colors = new List<Color>();

        for (int x = 0; x < divide; ++x)
        {
            colors.Add(Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        }
        this.tr = tr;
 
    }
    float hit_val,angle_normal,distance,divide;
    List<Color> colors;
    sound_props sound_prop;
    public List<float> dists, T_L, RL_V;
    public List<float> final;
    Quaternion angle;
    Transform tr;
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

    public void ray_hit()
    {
        
        dists = new List<float>();
        T_L = new List<float>();
        RL_V = new List<float>();
        final = new List<float>();
        RaycastHit hit;
        //Does the ray intersect any objects excluding the player layer
        //print(angle * Vector3.forward);
        //Physics.Raycast(tr.position, angle*Vector3.forward, out hit, distance);
        Physics.Raycast(tr.position, angle * tr.forward, out hit, distance);
        //print(hit.distance);
        print(tr.forward);
        for (int i = 1; i < divide + 1; ++i)
        {
            //float d = (divide / (2 * distance));
            //float ag=(Mathf.Deg2Rad * angle.eulerAngles[0]);
           // print(angle.eulerAngles);
            float d = (distance * (i-0.5f))/divide;
            float d1 = distance * i / divide;
            float d2 = distance * (i-1) / divide;
           // print(hit.distance);
            if (d1 <= hit.distance||hit.distance==0)
                {
                //print(hit.distance);
                    float alphaT = ((2f * d - 1f) * sound_prop.alphaW) / 1000f;

                    float S_L = 40f * Mathf.Log10(d);
                   // print(i);
                    float transmission_loss = S_L + alphaT;
                Debug.DrawRay(tr.position +  angle * tr.forward * d2,
                      angle *  tr.forward * d1, colors[i - 1]);
                //Debug.DrawRay(tr.position + angle * Vector3.forward * d2,
                       //tr.position + angle * Vector3.forward * d1, colors[i - 1]);
                    dists.Add(d);
                    T_L.Add(transmission_loss);
                float SV = sound_prop.sp + 7 * Mathf.Log10(sound_prop.frequency);
                float reverb_strength = SV + 10 * Mathf.Log(Mathf.Abs(4 * Mathf.PI * ((Mathf.Pow(d1, 3) - Mathf.Pow(d2, 3))) / 3), 10f);
                float RL_V_val = sound_prop.source_level + reverb_strength + sound_prop.beam_pattern_r + sound_prop.beam_pattern_t - transmission_loss;
                RL_V.Add(RL_V_val);
                
                float IR = 0;
                if(2*d1-d2>hit.distance&&hit.collider!=null)
                {
                    IR = sound_prop.source_level + reverb_strength + sound_prop.beam_pattern_r + sound_prop.beam_pattern_t - transmission_loss+hit.collider.GetComponent<target_strength>().ts;
                    print("IR added at bin "+i.ToString());
                }
                final.Add(RL_V_val+IR);
            }
          
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
