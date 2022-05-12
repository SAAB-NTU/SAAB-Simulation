using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class raycast_script : MonoBehaviour
{
    public float hit_val;
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
            float angle=Mathf.Deg2Rad*180-Mathf.Acos(Vector3.Dot(hit.normal,Vector3.forward));
            Quaternion rot = Quaternion.AngleAxis(angle,Vector3.one);
            print(angle.ToString("n2"));
            hit_val = hit.distance;
            Vector3 actual_hit=Quaternion.Inverse(transform.rotation)*hit.point;
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
        
    }
}
