using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_props : MonoBehaviour
{
    public float sound_velocity,wavelength,pH_moles,sp,alphaW,beam_pattern_r,beam_pattern_t;
    public float A1, f1, P1, A2, f2, P2, A3, f3, P3;
    public float temp,salinity,depth,frequency,pH_level,max_depth;
    public float source_level, horizontal_len_t, vertical_len_t, horizontal_len_r, vertical_len_r, theta_r, phi_r,theta_t,phi_t;
    public enum Sp { Low, Moderate, High } // represents particle density
    public Sp Volume_Reverb;
    // Start is called before the first frame update
    void Awake()
    {
        sound_velocity = 1449.2f + 4.6f * temp - 0.055f * temp * temp + 0.00029f * Mathf.Pow(temp, 3f) + (1.34f - 0.010f * temp) * (salinity - 35f) + 0.016f * depth;
        //sonar_resolution = sound_velocity / (2 * bandwidth);
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
    alphaW = (A1 * P1 * f1 * frequency * frequency) / 
            (f1 * f1 + frequency * frequency) +
            (A2 * P2 * f2 * frequency * frequency) / (f2 * f2 + frequency * frequency) + A3 * P3 * frequency * frequency;

        float alpha_r = Mathf.Sin((Mathf.Sin(Mathf.Deg2Rad * theta_r)) * (Mathf.Cos(Mathf.Deg2Rad * phi_r)) * (horizontal_len_r / wavelength)) / ((Mathf.Sin(Mathf.Deg2Rad * theta_r)) * (Mathf.Cos(Mathf.Deg2Rad * phi_r)) * (horizontal_len_r / wavelength));

        float beta_r = Mathf.Sin((Mathf.Sin(Mathf.Deg2Rad * phi_r)) * (vertical_len_r / wavelength)) / ((Mathf.Sin(Mathf.Deg2Rad * phi_r)) * (vertical_len_r / wavelength));

        beam_pattern_r = 20 * Mathf.Log(alpha_r * beta_r, 10f);

        float alpha_t = Mathf.Sin((Mathf.Sin(Mathf.Deg2Rad * theta_t)) * (Mathf.Cos(Mathf.Deg2Rad * phi_t)) * (horizontal_len_t / wavelength)) / ((Mathf.Sin(Mathf.Deg2Rad * theta_t)) * (Mathf.Cos(Mathf.Deg2Rad * phi_t)) * (horizontal_len_t / wavelength));

        float beta_t = Mathf.Sin((Mathf.Sin(Mathf.Deg2Rad * phi_t)) * (vertical_len_t / wavelength)) / ((Mathf.Sin(Mathf.Deg2Rad * phi_t)) * (vertical_len_t / wavelength));

        beam_pattern_t = 20 * Mathf.Log(alpha_t * beta_t, 10f);
    }
}
