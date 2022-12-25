using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class multiraycast : MonoBehaviour
{
    public float hit_val_front,hit_val_left,hit_val_right,angle_normal;
    float distance;
    void Start()
    {
        distance=30;
    }

    private void FixedUpdate()
    {
        RaycastHit hit_front,hit_left,hit_right;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit_front, distance))
        {
            
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit_front.distance, Color.green);
            
            hit_val_front = hit_front.distance;

           if(hit_front.distance<distance)
            {
           
                Debug.DrawRay(hit_front.point, transform.TransformDirection(Vector3.Reflect(Vector3.forward,Quaternion.Inverse(transform.rotation)*hit_front.normal)) * (distance-hit_front.distance), Color.red);
          
            }
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * distance, Color.white);
            // Debug.Log("Did not Hit");
            hit_val_front = -1f;
        }

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out hit_left, distance))
        {
            
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.left) * hit_left.distance, Color.green);
            
            hit_val_left = hit_left.distance;

           if(hit_left.distance<distance)
            {
           
                Debug.DrawRay(hit_left.point, transform.TransformDirection(Vector3.Reflect(Vector3.left,Quaternion.Inverse(transform.rotation)*hit_left.normal)) * (distance-hit_left.distance), Color.red);
          
            }
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.left) * distance, Color.white);
            // Debug.Log("Did not Hit");
            hit_val_left = -1f;
        }

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out hit_right, distance))
        {
            
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right) * hit_right.distance, Color.green);
            
            hit_val_right = hit_right.distance;

           if(hit_right.distance<distance)
            {
           
                Debug.DrawRay(hit_right.point, transform.TransformDirection(Vector3.Reflect(Vector3.right,Quaternion.Inverse(transform.rotation)*hit_right.normal)) * (distance-hit_right.distance), Color.red);
          
            }
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right) * distance, Color.white);
            // Debug.Log("Did not Hit");
            hit_val_right = -1f;
        }
    }
}
