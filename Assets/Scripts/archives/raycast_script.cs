using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class raycast_script : MonoBehaviour
{
    public float hit_val,angle_normal;
    public int confidence;
    float timer = 0; // for noise
    float distance;
    void Start()
    {
        distance=30;
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distance))
        {
            
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
             angle_normal=Mathf.Deg2Rad*180-Mathf.Acos(Vector3.Dot(Quaternion.Inverse(transform.rotation) * hit.normal, Vector3.forward));
            //Quaternion rot = Quaternion.AngleAxis(angle,Vector3.one);
            //print(angle.ToString("n2"));

            //adding noise to specific time periods to generate simulation data
            // if (timer > 5f & timer < 8f)
            // {
            //     confidence = 30;
            //     hit_val = 1.5f;
            // }
            // else if (timer > 20f & timer < 24f)
            // {
            //     confidence = 30;
            //     hit_val = 1.5f;
            // }
            // else if (timer >35f  & timer < 38f)
            // {
            //     confidence = 30;
            //     hit_val = 1.5f;
            // }
            // else
            // {
            //     confidence = 100;
            //     hit_val = hit.distance;
            // }

            confidence = 100;
            hit_val = hit.distance;

            //Vector3 actual_hit=Quaternion.Inverse(transform.rotation)*hit.point;
            // Debug.Log("Did Hit");
           if(hit.distance<distance)
            {
           
                Debug.DrawRay(hit.point, transform.TransformDirection(Vector3.Reflect(Vector3.forward,Quaternion.Inverse(transform.rotation)*hit.normal)) * (distance-hit.distance), Color.red);
          
            }
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * distance, Color.white);
            // Debug.Log("Did not Hit");
            hit_val = -1f;
        }
        timer += Time.fixedDeltaTime;
    }
}
