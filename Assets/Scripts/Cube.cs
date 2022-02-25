using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

public class Cube : MonoBehaviour
{
    ROSConnection ros; 
    string topicName = "imu_true";
    string topicName2 = "pos_true";
    float timeElapsed = 0f;

    public Vector3 displacement = Vector3.zero;
    public Vector3 velocity = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angular_velocity;

    Vector3 last_position = Vector3.zero;
    Vector3 last_velocity = Vector3.zero;
    Vector3 last_angle = Vector3.zero;
    
    
    // Start is called before the first frame update
    void Start()
    {	

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImuMsg>(topicName);
        ros.RegisterPublisher<PointCloudMsg>(topicName2);
    }
    
    // Update is called once per frame
    void FixedUpdate()
    { 
        if (Input.GetKey(KeyCode.U))
        {
            transform.Translate(new Vector3(1,0,0) * Time.deltaTime);
        }

        timeElapsed += Time.deltaTime;

        if (timeElapsed > 0f) //to publish at desired rate -> 0.005 highest rate without jamming ROS traffic
        {
            ImuMsg msg = new ImuMsg();
            PointCloudMsg msg2 = new PointCloudMsg();

            Vector3 current_position = transform.position;
            Vector3 current_angle = transform.rotation.eulerAngles;

            displacement = current_position - last_position;
            velocity = displacement / Time.deltaTime;
            acceleration = (velocity - last_velocity)/Time.deltaTime;

            //truncate acceleration to 2dp -> not much effect
            // msg.a_x = Mathf.Round(acceleration[0]*100f)/100f;
            // msg.a_y = Mathf.Round(acceleration[1]*100f)/100f;
            // msg.a_z = Mathf.Round(acceleration[2]*100f)/100f;

            msg.a_x = acceleration[0];
            msg.a_y = acceleration[1];
            msg.a_z = acceleration[2];
            last_velocity = velocity;
            last_position = current_position;

            angular_velocity = (current_angle - last_angle) / Time.deltaTime;
            msg.w_x = angular_velocity[0];
            msg.w_y = angular_velocity[1];
            msg.w_z = angular_velocity[2];
            last_angle = current_angle;

            ros.Publish(topicName,msg);
            
            msg2.x = transform.position.x;
            msg2.y = transform.position.y;
            msg2.z = transform.position.z;
            ros.Publish(topicName2,msg2);
            timeElapsed = 0;
        }
    }

    public Vector3 send_acceleration()
    {
        return acceleration;
    }

    public Vector3 send_velocity()
    {
        return velocity;
    }
}
