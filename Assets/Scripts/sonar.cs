using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sonar : MonoBehaviour
{
    public GameObject preFab;
    public Vector3 normal_ray;
    public float sound_speed = 5f;

    [SerializeField]
    float distance;

    // Start is called before the first frame update
    GameObject beam;
    void Awake()
    {
        beam = Instantiate(preFab,new Vector3(-0.3251f,0,0),Quaternion.identity);
        beam.GetComponent<single_beam_test>().velocity = sound_speed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
         //GetComponent<LineRenderer>().SetPosition(0, transform.position);
        RaycastHit hit;
        //Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out hit, 30))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.left) * hit.distance, Color.green);
            // print(hit.point.z);
            //GetComponent<LineRenderer>().SetPosition(1, transform.TransformDirection(Vector3.forward) * hit.distance- transform.position);
            //hit_val = hit.distance;
            // Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.left) * 30, Color.white);
            // Debug.Log("Did not Hit");
            //hit_val = -1;
        }
        normal_ray=hit.normal;
    }

    void OnTriggerEnter(Collider other)
    {
        float time_travelled = other.gameObject.GetComponent<single_beam_test>().time_travelled;
        distance = time_travelled/2 * sound_speed;
        Debug.Log(distance);
        Destroy(other.gameObject);
    }
}
