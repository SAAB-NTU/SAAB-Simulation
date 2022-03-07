using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector; 
using RosMessageTypes.UnityRoboticsDemo; 

public class Estimated_Position : MonoBehaviour
{
    ROSConnection ros; 
    public  GameObject cube;
    string topicName = "imu_true"; 
    string topicName2 = "pos_noise";
    float last_time_elapsed = 0f;

    //for debugging
    public Vector3 velocity = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angular_velocity = Vector3.zero;
    public Vector3 true_acceleration = Vector3.zero;
    public Vector3 imu_noise = Vector3.zero;
    public Vector3 filtered_error = Vector3.zero;
    
    Vector3 last_position = Vector3.zero;
    Vector3 last_velocity = Vector3.zero;
    Vector3 last_acceleration = Vector3.zero;
    Vector3 last_angle = Vector3.zero;
    Vector3 last_angular_velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {	
        ros = ROSConnection.GetOrCreateInstance(); 
        ros.Subscribe<ImuMsg>(topicName,Estimated); 
        ros.RegisterPublisher<PointCloudMsg>(topicName2);
        
    }

    void Estimated(ImuMsg imu_msg) 
    {
        true_acceleration = cube.GetComponent<Cube>().send_acceleration(); //get true acceleration
        float current_time = cube.GetComponent<Cube>().send_time();
        float time = current_time - last_time_elapsed;
        PointCloudMsg msg = new PointCloudMsg(); //to plot tragectory in rviz
        acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z);
        imu_noise = acceleration - true_acceleration; //show noise in inspector
        Vector3 displacement = Vector3.zero;

        //filter out noise from IMU
        
        //(i)   Approach 1 -> averaging several samples 

        //(ii)  Approach 2 -> LPF (based on https://github.com/KalebKE/AccelerationExplorer/wiki/Advanced-Low-Pass-Filter
        //                 -> y[i] = y[i] + alpha * (x[i] - y[i])
        // Vector3 filtered_acceleration = Vector3.zero;
        // float alpha = time / (time + time);
        // filtered_acceleration[0] = filtered_acceleration[0] + alpha * (acceleration[0] - filtered_acceleration[0]);
        // filtered_acceleration[1] = filtered_acceleration[1] + alpha * (acceleration[1] - filtered_acceleration[1]);
        // filtered_acceleration[2] = filtered_acceleration[2] + alpha * (acceleration[2] - filtered_acceleration[2]);
        // filtered_error = filtered_acceleration - true_acceleration;
        // acceleration = filtered_acceleration;
        // Debug.Log(filtered_acceleration.x);

        //baseline-calibration
        //subtract bias from imu readings (true_value + bias + bias_drift + noise)
        //other components are random
        //***** current imu model data assumes no calibration error *****


        for(int i = 0;i<3;i++)
        {
            if(Mathf.Abs(acceleration[i]) > 0.001) //if there is acceleration
            {
                Debug.Log("Acceleration");

                //(i) Approach 1 -> Using current reading to predict next reading (right sum)
                //assumptions: (i.)  readings are late by one step
                //             (ii.) acceleration,velocity constant for dt -> step graph rather than fluctuation
                // velocity[i] = last_velocity[i] + (acceleration[i] * time);
                // displacement[i] = velocity[i] * time;

                //(ii) Approach 2 -> Trapezoidal
                //                -> fast and exact for piecewise linear curve, 
                velocity[i] = last_velocity[i] + (((acceleration[i] + last_acceleration[i])/2)  * time); 
                displacement[i] = ((velocity[i] + last_velocity[i])/2)  * time; 

                //(iii) Approach 3 -> Simpson's Rule
                //                 -> Uses quadratic approximation instead of linear approximation
                //                 -> Good for smooth function, bad for digitized due to noise and high frequency content


                //(iii) Approach 4 -> Romberg integration algorithm

                
            }
            else //if no acceleration
            {
                acceleration[i] = 0f;
                velocity[i] = last_velocity[i]; //update from sonar
                if(Mathf.Abs(velocity[i]) > 0.001) //if constant velocity //get from sonar
                {
                    Debug.Log("Constant Velocity");
                    displacement[i] = velocity[i] * time; 
                }
                
                else //if 0 velocity
                {
                    Debug.Log("stationary");
                    velocity[i] = 0f;
                }

            }

        }

        transform.position = last_position + displacement;

        //calculate rotation from IMU angular_velocity readings
        Vector3 angular_displacement =  Vector3.zero;
        angular_velocity = new Vector3 (imu_msg.w_x,imu_msg.w_y,imu_msg.w_z);
        angular_displacement =  angular_velocity * time; 
        //Vector3 angular_displacement =  ((angular_velocity+last_angular_velocity)/2) * time; 
        //Vector3 angular_displacement = new Vector3(integration(angular_velocity[0],last_angular_velocity[0],time),integration(angular_velocity[1],last_angular_velocity[1],time),integration(angular_velocity[2],last_angular_velocity[2],time));
        transform.rotation = Quaternion.Euler(last_angle + angular_displacement); 

        //update prev_readings;
        last_time_elapsed = current_time;
        last_velocity = velocity;
        last_acceleration = acceleration;
        last_angular_velocity = angular_velocity;
        last_position = transform.position;
        last_angle = transform.rotation.eulerAngles;

        //publish position to rviz
        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        ros.Publish(topicName2,msg);
    

    }

}
