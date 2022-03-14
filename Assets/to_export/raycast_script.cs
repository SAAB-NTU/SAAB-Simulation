using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class raycast_script : MonoBehaviour
{
    public float hit_val;

    // Update is called once per frame
    private void FixedUpdate()
    {
        //GetComponent<LineRenderer>().SetPosition(0, transform.position);
        RaycastHit hit;
        //Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 30))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
            // print(hit.point.z);
            //GetComponent<LineRenderer>().SetPosition(1, transform.TransformDirection(Vector3.forward) * hit.distance- transform.position);
            hit_val = hit.distance;
            // Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 30, Color.white);
            // Debug.Log("Did not Hit");
            hit_val = -1;
        }
        
    }
}
