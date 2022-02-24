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

    //for debugging
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angular_velocity = Vector3.zero;
    
    public Vector3 displacement = Vector3.zero;
    Vector3 prev_position = Vector3.zero;
    public Vector3 velocity = Vector3.zero;
    Vector3 prev_velocity = Vector3.zero;
    Vector3 prev_acceleration = Vector3.zero;
    Vector3 prev_angular_velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {	
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<ImuMsg>(topicName,Estimated);
        ros.RegisterPublisher<PointCloudMsg>(topicName2);
    }

    void Estimated(ImuMsg imu_msg)
    {
        PointCloudMsg msg = new PointCloudMsg(); //to plot tragectory in rviz

        //calculate position from IMU acceleration readings (roslaunch imu_noise position_analysis.launch first)
        if(cube.GetComponent<Cube>().send_acceleration() != Vector3.zero)
        {
            
            acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z);
            velocity = prev_velocity + (((acceleration + prev_acceleration)/2)  * Time.deltaTime);
            displacement = velocity  * (Time.deltaTime + 0.005f); 
            transform.position = prev_position + displacement;
        }
        else
        {
            acceleration = Vector3.zero;
            if(cube.GetComponent<Cube>().send_velocity() != Vector3.zero)
            {
                Debug.Log(2);
                velocity = prev_velocity;
                displacement = velocity * Time.deltaTime; 
                transform.position = prev_position + displacement;
            }
            else
            {
                Debug.Log(3);
                velocity = Vector3.zero;
            }
        }
        
        

        //calculate rotation from IMU angular_velocity readings
        angular_velocity = new Vector3 (imu_msg.w_x,imu_msg.w_y,imu_msg.w_z);
        Vector3 angular_displacement =  ((angular_velocity+prev_angular_velocity)/2) * Time.deltaTime; 
        transform.Rotate(angular_displacement*(180/Mathf.PI)); //convert to deg from rad

        //update prev_readings;
        //prev_velocity = velocity;
        prev_acceleration = acceleration;
        prev_angular_velocity = angular_velocity;
        prev_position = transform.position;
        displacement = Vector3.zero;

        //publish position to rviz
        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        ros.Publish(topicName2,msg);
    }

}
