using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector; //ABU//
using RosMessageTypes.UnityRoboticsDemo; //ABU//

public class Estimated_Position : MonoBehaviour
{
    ROSConnection ros; 
    public  GameObject cube;
    string topicName = "imu_true"; //ABU//
    string topicName2 = "pos_noise";

    //for debugging
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angular_velocity = Vector3.zero;
    public Vector3 displacement = Vector3.zero;
    public Vector3 velocity = Vector3.zero;

    Vector3 prev_position = Vector3.zero;
    Vector3 prev_velocity = Vector3.zero;
    Vector3 prev_acceleration = Vector3.zero;
    Vector3 prev_angular_velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {	
        ros = ROSConnection.GetOrCreateInstance(); //ABU//
        ros.Subscribe<ImuMsg>(topicName,Estimated); //ABU//
        ros.RegisterPublisher<PointCloudMsg>(topicName2);
    }

    void Estimated(ImuMsg imu_msg) //ABU//
    {
        PointCloudMsg msg = new PointCloudMsg(); //to plot tragectory in rviz

        //calculate position from IMU acceleration readings (roslaunch imu_noise position_analysis.launch first)
        if(cube.GetComponent<Cube>().send_acceleration() != Vector3.zero) //if there is acceleration
        {
            
            acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z); //ABU// 
            //fluctuations in readings make this approach highly unstable 
            //velocity = prev_velocity + (((acceleration + prev_acceleration)/2)  * (Time.deltaTime +0.005f)); 

            //more accurate integration also produce insignificant effect
            //velocity = new Vector3(integration(acceleration.x,prev_acceleration.x),integration(acceleration.y,prev_acceleration.y),integration(acceleration.z,prev_acceleration.z));
            
            //this approach is more like catching up with the position of the true cube
            //assumptions: (i.)  readings are late by one step
            //             (ii.) acceleration,velocity constant for dt
            velocity = prev_velocity + (acceleration * (Time.deltaTime));
            displacement = velocity  * (Time.deltaTime); 
            transform.position = prev_position + displacement;
        }
        else //if no acceleration
        {
            acceleration = Vector3.zero;
            if(cube.GetComponent<Cube>().send_velocity() != Vector3.zero) //if constant velocity
            {
                velocity = prev_velocity;
                displacement = velocity * Time.deltaTime; 
                transform.position = prev_position + displacement;
            }
            else //if 0 velocity
            {
                velocity = Vector3.zero; 
            }
        }
        
        

        //calculate rotation from IMU angular_velocity readings
        angular_velocity = new Vector3 (imu_msg.w_x,imu_msg.w_y,imu_msg.w_z);
        Vector3 angular_displacement =  ((angular_velocity+prev_angular_velocity)/2) * Time.deltaTime; 
        //Vector3 angular_displacement = new Vector3(integration(angular_velocity.x,prev_angular_velocity.x),integration(angular_velocity.y,prev_angular_velocity.y),integration(angular_velocity.z,prev_angular_velocity.z));
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

    // // more accurate integration -> not much effect on angular_displacement
    // float integration(float current,float prev)
    // {
    //     if(current/prev > 0)
    //     {
    //         return (current+prev)/2 * Time.deltaTime; 
    //     }
    //     else
    //     {
    //         return ((current-prev)/2 * Time.deltaTime) + (prev * Time.deltaTime);
    //     }
    // }
}
