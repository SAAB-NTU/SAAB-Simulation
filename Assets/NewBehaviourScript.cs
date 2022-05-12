using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    float distance;
    [SerializeField]
    float angle;
    [SerializeField]
    Vector3 Direction;
    void Start()
    {
        distance = 30;
    }
    // Start is called before the first frame update
    void FixedUpdate()
    {
        // Bit shift the index of the layer (8) to get a bit mask
       

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
       

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distance))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
           if(hit.distance<distance)
            {
           
                Debug.DrawRay(hit.point, transform.TransformDirection(Quaternion.AngleAxis(Mathf.Deg2Rad*180-Mathf.Acos(Vector3.Dot(hit.normal,Vector3.forward)),Vector3.forward)*Vector3.back) * (distance-hit.distance), Color.red);
           Debug.DrawRay(hit.point, transform.TransformDirection(Quaternion.AngleAxis(180+Mathf.Deg2Rad*180-Mathf.Acos(Vector3.Dot(hit.normal,Vector3.forward)),Vector3.forward)*Vector3.back) * (distance-hit.distance), Color.red);
            
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            
        }
    }
}
