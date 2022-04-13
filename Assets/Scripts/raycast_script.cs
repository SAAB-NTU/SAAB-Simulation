using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class raycast_script : MonoBehaviour
{
    public float hit_val;
    private float sound_velocity,transmission_loss,SNR;
    private float pH,max_depth,A1,P1,f1,f,A2,P2,f2,A3,P3; //for transmission loss
    
    [Range(0f,35f)] 
    public float temp; //celcius

    [Range(0f,45f)]
    public float salinity; //ppt

    [Range(0f,1000f)]
    public float depth; //meters

    private void Start()
    {
        //initialize parameters for SNR calculation
        sound_velocity = 1449.2f + 4.6f*temp- 0.055f*temp*temp + 0.00029f*Mathf.Pow(temp,3f) + (1.34f-0.010f*temp)*(salinity - 35f) + 0.016f*depth;
        A1 = (8.696f/sound_velocity)*Mathf.Pow(10f,0.78f*pH -5f);
        f1 = 2.8f*Mathf.Sqrt(salinity/35f)*Mathf.Pow(10f,4f-(1245f/(temp+273f)));
        A2 = 21.44f*(salinity/sound_velocity)*(1f+0.025f*temp);
        f2 = (8.17f*Mathf.Pow(10f,8f-(1990f/(temp+273f))))/(1f+0.0018f*(salinity-35f));
        P2 = 1f-1.37f*Mathf.Pow(10f,-4f)*max_depth + 6.2f*Mathf.Pow(10f,-9f)*max_depth*max_depth;
        if(temp<=20f)
        {
            A3 = 4.937f*Mathf.Pow(10f,-4f)-2.59f*Mathf.Pow(10f,-5f)*temp + 9.11f*Mathf.Pow(10f,-7f)*temp*temp -1.5f*Mathf.Pow(10f,-8f)*Mathf.Pow(temp,3f);
        }
        else
        {
            A3 = 3.964f*Mathf.Pow(10f,-4f)-1.146f*Mathf.Pow(10f,-5f)*temp + 1.45f*Mathf.Pow(10f,-7f)*temp*temp -6.5f*Mathf.Pow(10f,-10f)*Mathf.Pow(temp,3f);
        }
        P3 = 1f - 3.83f*Mathf.Pow(10f,-5f)*max_depth + 4.9f*Mathf.Pow(10f,-10f)*max_depth*max_depth;
    }
    private void FixedUpdate()
    {
        RaycastHit hit;
        //Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 30))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);

            hit_val = hit.distance;
            // Debug.Log("Did Hit");

            //SNR calculation
            float alphaW = (A1*P1*f1*f*f)/(f1*f1 + f*f) + (A2*P2*f2*f*f)/(f2*f2 + f*f) + A3*P3*f*f;

            float alphaT = ((2f*hit_val - 1f) * alphaW)/1000f;
            
            float SL = 40f*Mathf.Log(hit_val,10f);

            transmission_loss = SL + alphaT;
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 30, Color.white);
            // Debug.Log("Did not Hit");
            hit_val = -1f;
        }
        
    }
}
