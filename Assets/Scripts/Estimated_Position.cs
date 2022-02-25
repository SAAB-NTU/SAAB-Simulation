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
        ros = ROSConnection.GetOrCreateInstance(); 
        ros.Subscribe<ImuMsg>(topicName,Estimated); 
        ros.RegisterPublisher<PointCloudMsg>(topicName2);
    }

    void Estimated(ImuMsg imu_msg) 
    {
        PointCloudMsg msg = new PointCloudMsg(); //to plot tragectory in rviz
        acceleration = new Vector3 (imu_msg.a_x,imu_msg.a_y,imu_msg.a_z);
        Vector3 displacement = Vector3.zero;
        //calculate position from IMU acceleration readings (roslaunch imu_noise position_analysis.launch first)
        for(int i = 0;i<3;i++)
        {
            if(Mathf.Round((cube.GetComponent<Cube>().send_acceleration()[i] * 100000f) / 100000f) != 0.00000f) //if there is acceleration
            {
                Debug.Log("Acceleration");

                //(i) Approach 1 -> Using current reading to predict next reading
                //assumptions: (i.)  readings are late by one step
                //             (ii.) acceleration,velocity constant for dt -> step graph rather than fluctuation
                //velocity[i] = last_velocity[i] + (acceleration[i] * Time.deltaTime);
                //displacement[i] = velocity[i] * Time.fixedDeltaTime;

                //(ii) Approach 2 -> Taking average of current reading and last reading.
                //                -> Able to reduce "overshooting" effect when there is sudden spike in reading
                // velocity[i] = last_velocity[i] + (((acceleration[i] + last_acceleration[i])/2)  * Time.fixedDeltaTime); 
                // displacement[i] = ((velocity[i] + last_velocity[i])/2)  * Time.fixedDeltaTime; 

                //(iii) Approach 3 -> More accurate integration approximation, area under curve with fluctuation considered.
                velocity[i] = last_velocity[i] + integration(acceleration[i],last_acceleration[i]);
                displacement[i] = integration(velocity[i],last_velocity[i]);
                
            }
            else //if no acceleration
            {
                acceleration[i] = 0f;
                if(Mathf.Round((cube.GetComponent<Cube>().send_velocity()[i]* 100000f) / 100000f) != 0.00000f) //if constant velocity
                {
                    Debug.Log("Constant Velocity");
                    velocity[i] = last_velocity[i];
                    displacement[i] = velocity[i] * Time.deltaTime; 
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
        //Vector3 angular_displacement =  angular_velocity * Time.fixedDeltaTime; 
        //Vector3 angular_displacement =  ((angular_velocity+last_angular_velocity)/2) * Time.fixedDeltaTime; 
        Vector3 angular_displacement = new Vector3(integration(angular_velocity[0],last_angular_velocity[0]),integration(angular_velocity[1],last_angular_velocity[1]),integration(angular_velocity[2],last_angular_velocity[2]));
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

    // more accurate integration -> not much effect on angular_displacement
    float integration(float current,float prev)
    {
        if(current/prev > 0)
        {
            return (current+prev)/2 * Time.fixedDeltaTime;
        }
        else
        {
            return ((current-prev)/2 * Time.fixedDeltaTime) + (prev * Time.fixedDeltaTime);;
        }
    }

}
