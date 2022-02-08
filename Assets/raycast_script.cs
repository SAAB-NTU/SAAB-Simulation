using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class raycast_script : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    { 
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        RaycastHit hit;
        
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
            print(hit.point.z);
           // Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
           // Debug.Log("Did not Hit");
        }
        
    }
}
