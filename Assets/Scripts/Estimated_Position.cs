using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

public class Estimated_Position : MonoBehaviour
{
    ROSConnection ros; 
    public  GameObject cube;
    string topicName = "imu_noise";
    // string topicName2 = "pos_noise";

    public Vector3 acceleration = Vector3.zero;
    public Vector3 angular_velocity = Vector3.zero;
    
    Vector3 prev_position = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    Vector3 prev_velocity = Vector3.zero;
    Vector3 prev_acceleration = Vector3.zero;
    Vector3 prev_angular_velocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {	
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<ImuMsg>(topicName,ImuCallback_estimated);
        //ros.RegisterPublisher<PointCloudMsg>(topicName2);
        //transform.Rotate(new Vector3(0,-90,0));
    }

    void ImuCallback_estimated(ImuMsg imu_msg)
    {
        //PointCloudMsg msg = new PointCloudMsg();
        //calculate position from IMU acceleration readings
        acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z);
        velocity = prev_velocity + (((acceleration + prev_acceleration)/2) * Time.deltaTime);
        Vector3 displacement = (((velocity + prev_velocity)/2) * Time.deltaTime); 
        transform.position = prev_position + displacement;
        //transform.position=new Vector3(cube.transform.position.x+displacement.x,cube.transform.position.y+displacement.y,cube.transform.position.z+displacement.z);

        //calculate rotation from IMU angular_velocity readings
        angular_velocity = new Vector3 (imu_msg.w_x,imu_msg.w_y,imu_msg.w_z);
        Vector3 rotation = ((angular_velocity+prev_angular_velocity)/2) * Time.deltaTime; //slow update of angle
        transform.Rotate(rotation*(180/Mathf.PI)); //convert to deg from rad

        //update prev_readings;
        prev_velocity = velocity;
        prev_acceleration = acceleration;
        prev_angular_velocity = angular_velocity;
        prev_position = transform.position;

        // msg.x = transform.position.x;
        // msg.y = transform.position.y;
        // msg.z = transform.position.z;
        // ros.Publish(topicName2,msg);
    }

    public Vector3 acceleration_noise()
    {
        return acceleration;
    }
}
