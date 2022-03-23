//Reference: https://dosits.org/science/advanced-topics/sonar-equation/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class single_beam_test : MonoBehaviour
{

    public float velocity;
    public Vector3 normal_sph;
    public float time_travelled = 0;
    public Vector3 SNR;
    
    // Start is called before the first frame update
    void Start()
    {
        //Source Level (SL)
        transform.localScale = SNR;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time_travelled += Time.deltaTime;
        float r = time_travelled * velocity;

        //Cylindrical Transmission Loss
        SNR -=  0.000001f * Vector3.one *10*Mathf.Log(r,10);

        //Sound Absorption
        //Attenuation (dB) = alpha * R
        //SNR -= 

        //Noise Level (NL)
        //SNR -= 

        if(SNR.x > 0)
        {
            transform.localScale = SNR;
        }
        else
        {
            Destroy(gameObject);
        }
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
