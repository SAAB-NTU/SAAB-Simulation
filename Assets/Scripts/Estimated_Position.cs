using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector; 
using RosMessageTypes.UnityRoboticsDemo; 

public class Estimated_Position : MonoBehaviour
{
    ROSConnection ros; 
    public  GameObject cube;    
    string topicName = "pos_noise";

    // int sample_size = 50; //mean filter
    // List<Vector3> window = new List<Vector3>{};
    // float RC_x = 1f/0.25f;  // LPF
    // float RC_y = 1f/8f;
    // float RC_z = 1f/1f;

    // int interval_size = 2;//Simpson's Rule
    // List<Vector3> acceleration_points = new List<Vector3>{}; 

    //for debugging
    public Vector3 velocity = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angular_velocity = Vector3.zero;
    Vector3 last_position = Vector3.zero;
    Vector3 last_velocity = Vector3.zero;
    Vector3 last_acceleration = Vector3.zero;
    Vector3 last_angular_velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {	
        ros = ROSConnection.GetOrCreateInstance(); 
        ros.RegisterPublisher<PointCloudMsg>(topicName);
    }

    // Vector3 calibrate()
    // {
    //     //baseline-calibration
    //     //subtract bias from imu readings (true_value + bias + bias_drift + noise)
    //     //other components are random
    //     //Vector3 calibrated_acceleration = imu_acceleration - new Vector3(0.05f,0.05f,0.05f);
        
    // }
    // Vector3 filter()
    // {
    //     //Digital filters -> filter out noise from IMU
    //     //(i)  Approach 1 -> mean filter
    //     //                -> averaging several samples to smooth out data
    //     //                -> can reduce noise but unable to isolate frequencies
    //     Vector3 filtered_acceleration = Vector3.zero;
    //     if(window.Count != sample_size)
    //     {
    //         window.Add(calibrated_acceleration);
    //         return;
    //     }
    //     else
    //     {
    //         Vector3 sum = Vector3.zero;
    //         foreach(Vector3 sample in window)
    //         {
    //             sum += sample;
    //         }
    //         filtered_acceleration = sum / window.Count;
    //         window = new List<Vector3>{};
    //     }

    //     //(ii)  Approach 2 -> LPF (based on https://github.com/KalebKE/AccelerationExplorer/wiki/Advanced-Low-Pass-Filter
    //     //                 -> y[i] = y[i] + alpha * (x[i] - y[i-1])
    //     //                 -> Gives general trend of trajectory
    //     Vector3 filtered_acceleration = Vector3.zero;
    //     float alpha_x = time / ( RC_x + time);
    //     float alpha_y = time / ( RC_y + time);
    //     float alpha_z = time / ( RC_z + time);
    //     filtered_acceleration[0] = filtered_acceleration[0] + alpha_x * (calibrated_acceleration[0] - filtered_acceleration[0]);
    //     filtered_acceleration[1] = filtered_acceleration[1] + alpha_y * (calibrated_acceleration[1] - filtered_acceleration[1]);
    //     filtered_acceleration[2] = filtered_acceleration[2] + alpha_z * (calibrated_acceleration[2] - filtered_acceleration[2]);
    //     filtered_error = filtered_acceleration - true_acceleration;
    //     Debug.Log(filtered_acceleration.x);
    // }

    

    //Position prediction / Dead-reckoning     
    //acceleration = calibrated_acceleration;
    //(i) Approach 1 -> Using current reading to predict next reading (right sum)
    //assumptions: (i.)  readings are late by one step
    //             (ii.) acceleration,velocity constant for dt -> step graph rather than fluctuation
    // velocity = last_velocity + (acceleration * time);
    // displacement = velocity * time;

    ///(ii) Approach 2 -> Trapezoidal
    //                 -> fast and exact for piecewise linear curve,
    // velocity = last_velocity + (((acceleration + last_acceleration)/2)  * time); 
    // displacement = ((velocity + last_velocity)/2)  * time; 

    //(iii) Approach 3 -> Simpson's Rule
    //                 -> https://www.freecodecamp.org/news/simpsons-rule/ 
    //                 -> Good for smooth function, bad for digitized due to noise and high frequency content
    //                 -> trajectory "signal" sligtly attenuated
    // acceleration_points.Add(acceleration);
    // if (acceleration_points.Count == interval_size + 1)
    // {
    //     Vector3 sum = Vector3.zero;
    //     int element_index = 1;
    //     foreach(Vector3 point in acceleration_points)
    //     {
    //         if(element_index == 1 || element_index == acceleration_points.Count)
    //         {
    //             sum += point;
    //         }
    //         else if(element_index % 2 == 0)
    //         {
    //             sum += point * 4;
    //         }
    //         else 
    //         {
    //             sum += point *2;
    //         }
    //     }
    //     velocity = last_velocity + (sum * time/3);
    //     acceleration_points = new List<Vector3>{};
    // }
    // displacement = ((velocity+last_velocity)/2) * (time);
    
    //(iii) Approach 3 -> Romberg integration algorithm


    void FixedUpdate()
    {
        float time = Time.fixedDeltaTime;
        Vector3 imu_noise_accel = cube.GetComponent<imu_noise>().imu_noise_accel();
        Vector3 imu_noise_gyro = cube.GetComponent<imu_noise>().imu_noise_gyro();

        acceleration = imu_noise_accel;
        Vector3 displacement = Vector3.zero;
        velocity = last_velocity + (((acceleration + last_acceleration)/2)  * time); 
        displacement = ((velocity + last_velocity)/2)  * time;
        transform.position = last_position + displacement;

        //calculate rotation from IMU angular_velocity readings
        Vector3 angular_displacement =  Vector3.zero;
        angular_velocity = imu_noise_gyro; //* (180/Mathf.PI);
        angular_displacement =  ((angular_velocity + last_angular_velocity)/2) * time;  
        transform.Rotate(angular_displacement * (180/Mathf.PI)); 

        last_position = transform.position;
        last_velocity = velocity;
        last_acceleration = acceleration;
        last_angular_velocity = angular_velocity;

        PointCloudMsg msg = new PointCloudMsg(); //to plot tragectory in rviz
        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        ros.Publish(topicName,msg);
    }

    public Vector3 estimated_velocity()
    {
        return velocity;
    }

}
