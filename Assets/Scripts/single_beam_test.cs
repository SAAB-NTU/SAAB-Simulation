using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class single_beam_test : MonoBehaviour
{

    public float velocity;
    
    public float time_travelled = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(-1,0,0) * velocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time_travelled += Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided");
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 normal = collision.contacts[0].normal;
        Vector3 _velocity = Vector3.Reflect(rb.velocity,normal);
        rb.velocity = _velocity ;
    }
}
