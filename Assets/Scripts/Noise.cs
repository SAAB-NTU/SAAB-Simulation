using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

public class Noise : MonoBehaviour
{
    ROSConnection ros; 

    public string topicName = "imu_noise";
    public string topicName2 = "pos_noise";

    public Vector3 prev_velocity = Vector3.zero;
    Vector3 prev_acceleration = Vector3.zero;
    Vector3 prev_angular_velocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {	
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<ImuMsg>(topicName,ImuCallback);
        ros.RegisterPublisher<PointCloudMsg>(topicName2);
    }

    void ImuCallback(ImuMsg imu_msg)
    {
        PointCloudMsg msg = new PointCloudMsg();

        Debug.Log(imu_msg);
        Rigidbody rb = GetComponent<Rigidbody>();
        //calculate position from IMU acceleration readings
        Vector3 acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z);
        rb.velocity += ((acceleration + prev_acceleration)/2) * Time.deltaTime;
        //rb.velocity = velocity;
        // Vector3 translation = ((velocity + prev_velocity)/2) * Time.deltaTime; 
        // transform.Translate(translation);

        
        // //calculate rotation from IMU angular_velocity readings
        Vector3 angular_velocity = new Vector3 (imu_msg.w_x,imu_msg.w_y,imu_msg.w_z);
        rb.angularVelocity = angular_velocity;
        //Vector3 rotation = ((angular_velocity-prev_angular_velocity)/2) * Time.deltaTime; //slow update of angle
        //transform.Rotate(rotation);

        // //update prev_readings;
        //prev_velocity = velocity;
        prev_acceleration = acceleration;
        // prev_angular_velocity = angular_velocity;

        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        //Debug.Log("x: "+msg.x);

        ros.Publish(topicName2,msg);
    }
}
