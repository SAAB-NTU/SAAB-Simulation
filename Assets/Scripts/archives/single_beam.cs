using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector; 
using RosMessageTypes.UnityRoboticsDemo; 

public class single_beam : MonoBehaviour
{
    List<float>  rev_vals;
    protected float  beam_pattern, transmission_loss;
    protected float wavelength, sound_velocity, reverb_strength, target_strength,RL_V, IR;
    protected float pH_moles, A1, P1, f1, A2, P2, f2, A3, P3; //for transmission loss
    private float sp; //volume reverberation coefficient
    public float frequency = 115; //kHz
    public float source_level = 220; //source level dB

    public float RL_V_val_i, RL_V_val_n,final; //To display volume backscattering effects
    public float sonar_resolution; // c/(2*Band-Width)

    [Range(7.2f, 7.8f)]
    public float pH_level;// water acidity (moles per litre) source: https://sso.agc.gov.sg/SL-Rev/95-RG10/Published/20000131?DocDate=19930401

    [Range(0f, 35f)]
    public float temp; //degree celcius

    [Range(0f, 45f)]
    public float salinity; //ppt

    [Range(0f, 1000f)]
    public float max_depth; //meters

    [Range(0f, 1000f)]
    public float depth; //meters
    public float bandwidth; //Hz, inverse of ping interval, found to be 250ms from Ping1D protocol
    public float theta = 30f; //horizontal beam angle for beam pattern
    public float phi = 2f; //vertical beam angle for beam pattern
    public float horizontal_len = 0.048f; //meters , ping sonar diameter
    public float vertical_len = 0.035f; // meters , ping sonar height
    public enum Sp { Low, Moderate, High } // represents particle density
    public Sp Volume_Reverb;

    public GameObject prefab,coord,graph;
    float FOV_x, FOV_y,d_FOV_x, d_FOV_y;
    public float multiplier_x,multiplier_y,scale,offset,tot,tolerance;
    public int k;
    List<float> vals,bef,aft,transmission_array,TS_array,final_array;
    //float[] aft2;
    List<GameObject> coords;
    //ROSConnection ros; 
    //private string topicName = "sonar_measurements";
    void Start()
    {
        FOV_x=theta;
        FOV_y = phi;
        d_FOV_x = Mathf.Deg2Rad*FOV_x / multiplier_x;
        d_FOV_y = Mathf.Deg2Rad*FOV_y / multiplier_y;
        if (bandwidth == 0)
        {
            bandwidth = 1 / 0.25f;
        }
        
        sound_velocity = 1449.2f + 4.6f * temp - 0.055f * temp * temp + 0.00029f * Mathf.Pow(temp, 3f) + (1.34f - 0.010f * temp) * (salinity - 35f) + 0.016f * depth;
        sonar_resolution = sound_velocity / (2 * bandwidth);
        wavelength = sound_velocity / (frequency * 1000);

        pH_moles = Mathf.Pow(10f, -pH_level);

        A1 = (8.696f / sound_velocity) * Mathf.Pow(10f, 0.78f * pH_moles - 5f);
        f1 = 2.8f * Mathf.Sqrt(salinity / 35f) * Mathf.Pow(10f, 4f - (1245f / (temp + 273f)));
        P1 = 1f;

        A2 = 21.44f * (salinity / sound_velocity) * (1f + 0.025f * temp);
        f2 = (8.17f * Mathf.Pow(10f, 8f - (1990f / (temp + 273f)))) / (1f + 0.0018f * (salinity - 35f));
        P2 = 1f - 1.37f * Mathf.Pow(10f, -4f) * max_depth + 6.2f * Mathf.Pow(10f, -9f) * max_depth * max_depth;

        if (temp <= 20f)
        {
            A3 = 4.937f * Mathf.Pow(10f, -4f) - 2.59f * Mathf.Pow(10f, -5f) * temp + 9.11f * Mathf.Pow(10f, -7f) * temp * temp - 1.5f * Mathf.Pow(10f, -8f) * Mathf.Pow(temp, 3f);
        }
        else
        {
            A3 = 3.964f * Mathf.Pow(10f, -4f) - 1.146f * Mathf.Pow(10f, -5f) * temp + 1.45f * Mathf.Pow(10f, -7f) * temp * temp - 6.5f * Mathf.Pow(10f, -10f) * Mathf.Pow(temp, 3f);
        }
        P3 = 1f - 3.83f * Mathf.Pow(10f, -5f) * max_depth + 4.9f * Mathf.Pow(10f, -10f) * max_depth * max_depth;

        if (Volume_Reverb == Sp.Low)
        {
            sp = -50f;
        }
        else if (Volume_Reverb == Sp.Moderate)
        {
            sp = -70f;
        }
        else if (Volume_Reverb == Sp.High)
        {
            sp = -90f;
        }


        // ros = ROSConnection.GetOrCreateInstance(); 
        // ros.RegisterPublisher<SonarMsg>(topicName);

        //writing into csv files
        var w = new StreamWriter("sonar_115000.csv",true); //csv file saved to Asset folder
        var line = "SNR,Transmission Loss,Beam Pattern";
        w.WriteLine(line);
        w.Close();

        tot = 0;
        vals = new List<float>();
        bef = new List<float>();
        coords = new List<GameObject>();
        for (int j = 0; j < multiplier_y; ++j)
        {

            for (int i = 0; i < multiplier_x; ++i)
            {
                //float value_i = -FOV_y / 2 + j * (FOV_y / (multiplier_y - 1));
               float value_i = 0;
                //Instantiate probe 
                GameObject ray = Instantiate(prefab);
                float value = -FOV_x / 2 + i * (FOV_x / (multiplier_x - 1));
                ray.transform.SetParent(gameObject.transform);
                ray.transform.localRotation = (Quaternion.Euler(value_i, value, 0));
                GameObject coordinate = Instantiate(coord);
                coordinate.transform.SetParent(graph.transform);
                vals.Add(value);
                // Debug.Log(value);         
                coords.Add(coordinate);
            }
        }
       
    }
    private void FixedUpdate()
    {
        rev_vals = new List<float>();
        aft = new List<float>();
        TS_array = new List<float>();
        final_array = new List<float>();
        final = 0;
        transmission_array = new List<float>();
        for (int i=0;i<transform.childCount;++i)
        {
            try
            {
                Vector2 ini=coords[i].GetComponent<RectTransform>().anchoredPosition;
               
                float r = transform.GetChild(i).GetComponent<raycast_script>().hit_val;
                float angle = transform.GetChild(i).GetComponent<raycast_script>().angle_normal;//angle in rads
                if (r >= 0)
                {
                    float cos_theta = Mathf.Cos(Mathf.Deg2Rad * vals[i]);
                    float sin_theta = Mathf.Sin(Mathf.Deg2Rad * vals[i]);

                    // adding ray properties to respective lists -> distance, SNR, beam pattern, transmission loss 
                    aft.Add(r * cos_theta);
                  //print( d_FOV_x * d_FOV_y );
                    TS_array.Add(10*Mathf.Log10(0.001f*r*r * d_FOV_x * d_FOV_y * Mathf.Cos(angle)));

                    


                    float alphaW = (A1 * P1 * f1 * frequency * frequency) / (f1 * f1 + frequency * frequency) + (A2 * P2 * f2 * frequency * frequency) / (f2 * f2 + frequency * frequency) + A3 * P3 * frequency * frequency;

                    float alphaT = ((2f * r / scale - 1f) * alphaW) / 1000f;
                    
                    float S_L = 40f * Mathf.Log(r / scale, 10f);
                    //print(r / scale);
                    transmission_loss = S_L + alphaT;
                    transmission_array.Add(transmission_loss);
                    // Debug.Log("sl" + S_L);
                    //Debug.Log("alpha" + alphaT);


                    //** sinc(x) = sin(x)/x **

                    float alpha = Mathf.Sin((Mathf.Sin(Mathf.Deg2Rad * theta)) * (Mathf.Cos(Mathf.Deg2Rad * phi)) * (horizontal_len / wavelength)) / ((Mathf.Sin(Mathf.Deg2Rad * theta)) * (Mathf.Cos(Mathf.Deg2Rad * phi)) * (horizontal_len / wavelength));

                    float beta = Mathf.Sin((Mathf.Sin(Mathf.Deg2Rad * phi)) * (vertical_len / wavelength)) / ((Mathf.Sin(Mathf.Deg2Rad * phi)) * (vertical_len / wavelength));

                    beam_pattern = 20 * Mathf.Log(alpha * beta, 10f);

                   // print((Mathf.Sin(Mathf.Deg2Rad * theta)) * (Mathf.Cos(Mathf.Deg2Rad * phi)) * (horizontal_len / wavelength));
                   
                    //SNR calculation

                    //SNR_array.Add(SNR);
                    //beam_array.Add(beam_pattern);
                }

            }

            catch(System.Exception e)
            {
                //Debug.Log(e);
            }
            
        }

        // //** SAVING IN CSV FILE**
        // //getting mean of ray properties
        // float tot_SNR = 0;
        // float tot_beam = 0;
        // float tot_transmission = 0;
        // for (int i = 0; i<SNR_array.Count;i++)
        // {
        //     tot_SNR += SNR_array[i];
        //     tot_beam += beam_array[i];
        //     tot_transmission += transmission_array[i];
        // }
        // float avg_SNR = tot_SNR/SNR_array.Count; 
        // float avg_beam = tot_beam/beam_array.Count; 
        // float avg_transmission = tot_transmission/transmission_array.Count; 

        //save to csv
        

        //aft2 = new float[aft.Count];
        if (bef.Count>0)
        {
            tot = 0;
            k = 0;
            RL_V_val_i = 0;
            rev_vals = new List<float>();
            float V=0;
            for (int i = 0; i < aft.Count; ++i)
            {
                if (i == 0)
                {
                  V = (4 * Mathf.PI / 3) * (Mathf.Pow(aft[i], 3) - 0); //ensonified volume d0
                }
                else if (i<=aft.Count-2) //check for looping issues if any
                { 
                    V = (4 * Mathf.PI / 3) * (Mathf.Pow(aft[i + 1], 3) - Mathf.Pow(aft[i], 3)); //ensonified volume   
                }
                
                float SV = sp + 7 * Mathf.Log(frequency, 10f);
                reverb_strength = SV + 10 * Mathf.Log(Mathf.Abs(V), 10f);
                RL_V = source_level - transmission_array[i] + beam_pattern * 2 + reverb_strength;
                IR= source_level - transmission_array[i] + beam_pattern * 2;
                //float RL_V_pow = Mathf.Pow(10, (RL_V * (aft[i] - (i - 1) * sonar_resolution) / 10));
                final_array.Add(IR + RL_V);
                print(i);
                //print(beam_pattern);
                final = final_array[final_array.Count - 1];
                float cos_theta = Mathf.Cos(Mathf.Deg2Rad * vals[i]);
                float sin_theta = Mathf.Sin(Mathf.Deg2Rad * vals[i]);
               // coords[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(scale * final * sin_theta, scale * final * cos_theta);
                
              //  print(transmission_array[i]);
                //if (float.IsInfinity(rev_vals[rev_vals.Count - 1]) == false && float.IsNaN(rev_vals[rev_vals.Count - 1]) == false)
              //  RL_V_val_i += (rev_vals[rev_vals.Count-1]);

                float diff = aft[i] - bef[i];
                if (Mathf.Abs(diff) > tolerance)
                {
                    k += 1;
                    tot += (Mathf.Abs(diff));
                }
                //aft2[i] = aft[i];
            }

            if (k != 0)
            {
                tot /= k;
                 //Check here for potential errors
            }
           // RL_V_val_n = 10 * Mathf.Log10(RL_V_val_i);
            tot /= Time.fixedDeltaTime;

        }
        using (TextWriter writer = new StreamWriter("RLV_val.csv", true))
        {
          
            writer.WriteLine(string.Format("{0},{1}", Time.fixedTime, RL_V_val_n));
            writer.Close();
        }
        
        bef = aft;
        // volume backscatter
        // float V //ensonified volume
        // float SV = sp + 7*Mathf.Log(frequency,10f);
        // reverb_strength = SV + 10*Mathf.Log(V,10f);
        // RL_V = source_level - transmission_loss + beam_pattern*2 + reverb_strength; //main equation

        // reflected intensity 
        //target_strength = 
        //IR = source_level - transmission_loss + beam_pattern*2 + target_strength;

        //SNR = source_level - transmission_loss + beam_pattern * 2+RL_V_val_n;
        // print(vals.Count);
        // vals = new List<float>();
        // print(hits.Length);
        // SonarMsg sonar_msg = new SonarMsg();
        // sonar_msg.ranges = aft2;
        // ros.Publish(topicName,sonar_msg);
    }

}
