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
    public Vector3 velocity = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angular_velocity = Vector3.zero;

    Vector3 last_position = Vector3.zero;
    Vector3 last_velocity = Vector3.zero;
    Vector3 last_acceleration = Vector3.zero;
    Vector3 last_angle = Vector3.zero;
    Vector3 last_angular_velocity = Vector3.zero;

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
        acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z);
        Vector3 displacement = Vector3.zero;
        //calculate position from IMU acceleration readings (roslaunch imu_noise position_analysis.launch first)
        for(int i = 0;i<3;i++)
        {
            if(Mathf.Round((cube.GetComponent<Cube>().send_acceleration()[i] * 100000f) / 100000f) != 0.00000f) //if there is acceleration
            {
                Debug.Log(1);
                //ABU// 
                //fluctuations in readings make this approach highly unstable 
                velocity[i] = last_velocity[i] + (((acceleration[i] + last_acceleration[i])/2)  * Time.fixedDeltaTime); 

                //more accurate integration also produce insignificant effect
                //velocity = new Vector3(integration(acceleration.x,prev_acceleration.x),integration(acceleration.y,prev_acceleration.y),integration(acceleration.z,prev_acceleration.z));
                
                //this approach is more like catching up with the position of the true cube
                //assumptions: (i.)  readings are late by one step
                //             (ii.) acceleration,velocity constant for dt -> step graph rather than fluctuation
                //velocity[i] = last_velocity[i] + (acceleration[i] * Time.deltaTime);
                displacement[i] = ((velocity[i] + last_velocity[i])/2)  * Time.fixedDeltaTime; 
            }
            else //if no acceleration
            {
                acceleration[i] = 0f;
                if(Mathf.Round((cube.GetComponent<Cube>().send_velocity()[i]* 100000f) / 100000f) != 0.00000f) //if constant velocity
                {
                    Debug.Log("Constant Velocity");
                    velocity[i] = last_velocity[i];
                    displacement[i] = velocity[i] * Time.deltaTime; 
                    //transform.position = prev_position + displacement;
                }
                else //if 0 velocity
                {
                    Debug.Log("Stationary");
                    velocity[i] = 0f; 
                }
            }
            
        }
        transform.position = last_position + displacement;

        //calculate rotation from IMU angular_velocity readings
        angular_velocity = new Vector3 (imu_msg.w_x,imu_msg.w_y,imu_msg.w_z);
        Vector3 angular_displacement =  ((angular_velocity+last_angular_velocity)/2) * Time.fixedDeltaTime; 
        //Vector3 angular_displacement = new Vector3(integration(angular_velocity.x,prev_angular_velocity.x),integration(angular_velocity.y,prev_angular_velocity.y),integration(angular_velocity.z,prev_angular_velocity.z));
        //Vector3 angular_displacement =  angular_velocity * Time.deltaTime; 
        transform.rotation = Quaternion.Euler(last_angle + angular_displacement); 

        //update prev_readings;
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

    // // more accurate integration -> not much effect on angular_displacement
    // float integration(float current,float prev)
    // {
    //     if(current/prev > 0)
    //   
    //         return (current+prev)/2 * Time.deltaTime; 
    //     }
    //     else
    //     {
    //         return ((current-prev)/2 * Time.deltaTime) + (prev * Time.deltaTime);
    //     }
    // }

    // float32 position_algo(float32 acceleration)
    // {
    //     if(cube.GetComponent<Cube>().send_acceleration() != Vector3.zero) //if there is acceleration
    //     {
    //         Debug.Log(1);
    //         acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z); //ABU// 
    //         //fluctuations in readings make this approach highly unstable 
    //         //velocity = prev_velocity + (((acceleration + prev_acceleration)/2)  * (Time.deltaTime +0.005f)); 

    //         //more accurate integration also produce insignificant effect
    //         //velocity = new Vector3(integration(acceleration.x,prev_acceleration.x),integration(acceleration.y,prev_acceleration.y),integration(acceleration.z,prev_acceleration.z));
            
    //         //this approach is more like catching up with the position of the true cube
    //         //assumptions: (i.)  readings are late by one step
    //         //             (ii.) acceleration,velocity constant for dt -> step graph rather than fluctuation
    //         velocity = prev_velocity + (acceleration * (Time.deltaTime));
    //         displacement = velocity  * (Time.deltaTime); 
    //         transform.position = prev_position + displacement;
    //     }
    //     else //if no acceleration
    //     {
    //         acceleration = Vector3.zero;
    //         if(cube.GetComponent<Cube>().send_velocity() != Vector3.zero) //if constant velocity
    //         {
    //             velocity = prev_velocity;
    //             displacement = velocity * Time.deltaTime; 
    //             transform.position = prev_position + displacement;
    //         }
    //         else //if 0 velocity
    //         {
    //             velocity = Vector3.zero; 
    //         }
    //     }
    // }
}
