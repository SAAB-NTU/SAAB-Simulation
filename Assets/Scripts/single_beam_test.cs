using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class single_beam_test : MonoBehaviour
{

    public float velocity;
    public Vector3 normal_sph;
    public float time_travelled = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        //transform.Rotate(0,45,0);
        //rb.velocity = new Vector3(-Mathf.Cos(Mathf.PI/4),0,Mathf.Cos(Mathf.PI/4)) * velocity;
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
        normal_sph = collision.contacts[0].normal;
        Vector3 _velocity = Vector3.Reflect(rb.velocity,normal_sph);
        rb.velocity = _velocity ;
        

 
        // // Print how many points are colliding with this transform
        // Debug.Log("Points colliding: " + collision.contacts.Length);

        // // Print the normal of the first point in the collision.
        // Debug.Log("Normal of the first point: " + collision.contacts[0].normal);

        // // Draw a different colored ray for every normal in the collision
        // foreach (var item in collision.contacts)
        // {
        //     Debug.DrawRay(item.point, item.normal * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 100f);
        // }
    


    }
}
