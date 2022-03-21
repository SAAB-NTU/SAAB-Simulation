// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class kalman_filter : MonoBehaviour
// {
//     public  GameObject cube;
//     public GameObject sonar;    
//     public Vector3 estimated_velocity;
//     Vector3 velocity = Vector3.zero;
//     Vector3 acceleration = Vector3.zero;
//     Vector3 last_acceleration = Vector3.zero;
//     Vector3 posterior = Vector3.zero;

//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void FixedUpdate()
//     {
//         //predict step
//         float time = Time.fixedDeltaTime;
//         float process_var = 
//         float imu_var = 
//         Vector3 imu_noise_accel = cube.GetComponent<imu_noise>().imu_noise_accel();
//         acceleration = imu_noise_accel;
//         predicted_velocity = (last_velocity + (((acceleration + last_acceleration)/2)  * time)).x; 
//         last_acceleration = acceleration;

//         var prior = (mean:predicted_velocity,var:);

//         //update step
//         float process_var = 
//         float sonar_var = 
//         float measured_velocity = sonar.GetComponent<single_beam>().tot;
//         var likelihood = (mean:measured_velocity,var:);
//         float mean = (prior.var * likelihood.mean + likelihood.var * prior.mean) / (prior.var + likelihood.var);
//         float variance = (prior.var * lilkelihood.var) / (prior.var + lilkelihood.var);
        
//     }
// }
