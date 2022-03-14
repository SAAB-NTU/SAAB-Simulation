using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

public class Cube : MonoBehaviour
{
    ROSConnection ros; 
    public string topicName = "imu_true";
    public string topicName2 = "pos_true";

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
        Rigidbody rb = GetComponent<Rigidbody>();

        ImuMsg msg = new ImuMsg();
        PointCloudMsg msg2 = new PointCloudMsg();

        Vector3 current_position = transform.position;
        Vector3 current_angle = transform.rotation.eulerAngles;

        velocity = rb.velocity;
        acceleration = (velocity - last_velocity)/Time.fixedDeltaTime;

        msg.a_x = acceleration[0];
        msg.a_y = acceleration[1];
        msg.a_z = acceleration[2];
        last_velocity = velocity;
        last_position = current_position;
        
        angular_velocity = rb.angularVelocity;
        msg.w_x = angular_velocity[0];
        msg.w_y = angular_velocity[1];
        msg.w_z = angular_velocity[2];
        last_angle = current_angle;

        ros.Publish(topicName,msg);
        
        msg2.x = transform.position.x;
        msg2.y = transform.position.y;
        msg2.z = transform.position.z;
        ros.Publish(topicName2,msg2);
        
    }

    public Vector3 imu_true_accel()
    {
        return acceleration;
    }

    public Vector3 imu_true_gyro()
    {
        return angular_velocity;
    }

    public Vector3 imu_true_vel()
    {
        return velocity;
    }

}
