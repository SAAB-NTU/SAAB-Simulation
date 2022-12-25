using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Unity;

public class Noise : MonoBehaviour
{
    ROSConnection ros; 
    public  GameObject cube;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angular_velocity = Vector3.zero;
    string topicName = "imu_noise";
    Vector3 velocity = Vector3.zero;
    Vector3 prev_velocity = Vector3.zero;
    Vector3 prev_acceleration = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<ImuUnityMsg>(topicName, ImuCallback);
    }

void ImuCallback(ImuUnityMsg imu_msg)
    {

        Rigidbody rb = GetComponent<Rigidbody>();

        //calculate position from IMU acceleration readings
        acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z);
        rb.velocity += ((acceleration + prev_acceleration)/2) * Time.deltaTime;
        
        // //calculate rotation from IMU angular_velocity readings
        angular_velocity = new Vector3 (imu_msg.w_x,imu_msg.w_y,imu_msg.w_z);
        rb.angularVelocity = angular_velocity;

        // //update prev_readings;
        prev_acceleration = acceleration;

    }
}

