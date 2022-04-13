using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class kalman_filter : MonoBehaviour
{
    public  GameObject cube;
    public GameObject sonar;  
    public Rigidbody Real;
    public float predicted_velocity,measured_velocity,estimatedVel,real_velocity;  
    Vector3 velocity,acceleration = Vector3.zero;
    Vector3 last_acceleration = Vector3.zero;

    (float mean,float var) posterior;

    // Start is called before the first frame update
    void Start()
    {
        //initialization
        posterior = (0,0.01f);
        // var w = new StreamWriter("no_water(7).csv",true); //csv file saved to Asset folder
        // var line = "predicted_velocity,measured_velocity,estimated_velocity,real_velocity";
        // w.WriteLine(line);
        // w.Close();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        real_velocity = Mathf.Abs(Real.velocity.x);
        //predict step
        float time = Time.fixedDeltaTime;
        float imu_std = 0.05f;
        Vector3 imu_noise_accel = cube.GetComponent<imu_noise>().imu_noise_accel();
        acceleration = imu_noise_accel;
        predicted_velocity = posterior.mean + (((acceleration + last_acceleration)/2)  * time).x; 
        last_acceleration = acceleration;

        float predicted_variance = imu_std * imu_std;

        (float mean,float var) prior = (predicted_velocity,predicted_variance);

        //update step
        float sonar_std = 0.05f;
        measured_velocity = Mathf.Abs(sonar.GetComponent<single_beam>().tot);
        (float mean,float var) likelihood = (measured_velocity,sonar_std * sonar_std);
        float residual = likelihood.mean - prior.mean;
        float kalmanGain = (prior.var / prior.var + likelihood.var);
        estimatedVel = Mathf.Abs(prior.mean + kalmanGain * residual);
        float estimatedError = (1-kalmanGain) * prior.var;
        posterior = (estimatedVel,estimatedError);

        //save to csv
        // var w = new StreamWriter("no_water(7).csv",true);
        // var line = string.Format("{0},{1},{2},{3}",predicted_velocity,measured_velocity,estimatedVel,real_velocity);
        // w.WriteLine(line);
        // w.Close();
    }

}
