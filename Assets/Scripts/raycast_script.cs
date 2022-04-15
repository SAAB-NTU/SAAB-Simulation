using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class raycast_script : MonoBehaviour
{
    public float hit_val,SNR,beam_pattern,transmission_loss;
    private float wavelength,sound_velocity,reverb_strength,target_strength,RL_V,IR;
    private float pH_moles,A1,P1,f1,A2,P2,f2,A3,P3; //for transmission loss
    private float sp; //volume reverberation coefficient
    public float frequency = 115; //kHz
    public float source_level = 220; //source level dB

    [Range(7.2f,7.8f)] 
    public float pH_level;// water acidity (moles per litre) source: https://sso.agc.gov.sg/SL-Rev/95-RG10/Published/20000131?DocDate=19930401

    [Range(0f,35f)] 
    public float temp; //degree celcius

    [Range(0f,45f)]
    public float salinity; //ppt

    [Range(0f,1000f)]
    public float max_depth; //meters

    [Range(0f,1000f)]
    public float depth; //meters
    public float theta = 30f; //horizontal beam angle for beam pattern
    public float phi = 2f; //vertical beam angle for beam pattern
    public float horizontal_len = 0.048f; //meters , ping sonar diameter
    public float vertical_len = 0.035f; // meters , ping sonar height
    public enum Sp{Low,Moderate,High} // represents particle density
    public Sp Volume_Reverb;
    private void Start()
    {
        
        //initialize parameters for SNR calculation
        sound_velocity = 1449.2f + 4.6f*temp- 0.055f*temp*temp + 0.00029f*Mathf.Pow(temp,3f) + (1.34f-0.010f*temp)*(salinity - 35f) + 0.016f*depth;
        wavelength = sound_velocity/(frequency*1000);
        pH_moles = Mathf.Pow(10f,-pH_level);
        A1 = (8.696f/sound_velocity)*Mathf.Pow(10f,0.78f*pH_moles -5f);
        f1 = 2.8f*Mathf.Sqrt(salinity/35f)*Mathf.Pow(10f,4f-(1245f/(temp+273f)));
        P1 = 1f;
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

        if(Volume_Reverb == Sp.Low)
        {
            sp = -50f;
        }
        else if(Volume_Reverb == Sp.Moderate)
        {
            sp = -70f;
        }
        else if(Volume_Reverb == Sp.High)
        {
            sp = -90f;
        }
    }
    private void FixedUpdate()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 30))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);

            hit_val = hit.distance;
            // Debug.Log("Did Hit");

            //SNR calculation
            float alphaW = (A1*P1*f1*frequency*frequency)/(f1*f1 + frequency*frequency) + (A2*P2*f2*frequency*frequency)/(f2*f2 + frequency*frequency) + A3*P3*frequency*frequency;

            float alphaT = ((2f*hit_val - 1f) * alphaW)/1000f;
            
            float S_L = 40f*Mathf.Log(hit_val,10f);

            transmission_loss = S_L + alphaT;


            //** sinc(x) = sin(x)/x **

            float alpha = Mathf.Sin((Mathf.Sin(Mathf.Deg2Rad*theta))*(Mathf.Cos(Mathf.Deg2Rad*phi))*(horizontal_len/wavelength))/((Mathf.Sin(Mathf.Deg2Rad*theta))*(Mathf.Cos(Mathf.Deg2Rad*phi))*(horizontal_len/wavelength));

            float beta = Mathf.Sin((Mathf.Sin(Mathf.Deg2Rad*phi))*(vertical_len/wavelength))/((Mathf.Sin(Mathf.Deg2Rad*phi))*(vertical_len/wavelength));

            beam_pattern = 20*Mathf.Log(alpha*beta,10f);

            // volume backscatter
            // float V //ensonified volume
            // float SV = sp + 7*Mathf.Log(frequency,10f);
            // reverb_strength = SV + 10*Mathf.Log(V,10f);
            // RL_V = source_level - transmission_loss + beam_pattern*2 + reverb_strength; //main equation

            // reflected intensity 
            //target_strength = 
            //IR = source_level - transmission_loss + beam_pattern*2 + target_strength;

            SNR = source_level - transmission_loss + beam_pattern*2;
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 30, Color.white);
            // Debug.Log("Did not Hit");
            hit_val = -1f;
        }
        
    }
}
