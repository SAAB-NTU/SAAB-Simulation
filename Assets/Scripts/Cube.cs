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
    float time_Update = 0f;

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
        if (Input.GetKey(KeyCode.U)) //
        {
            rb.AddForce(new Vector3(10,0,0));
            //transform.Translate(new Vector3(-1,0,0) * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.I))
        {

            rb.AddTorque(new Vector3(0,1,0));
            //transform.Rotate(new Vector3(0,10,0) * Time.deltaTime);
        }

        timeElapsed += Time.deltaTime;

        if (timeElapsed > 0f) //to publish at desired rate 
        {
            ImuMsg msg = new ImuMsg();
            PointCloudMsg msg2 = new PointCloudMsg();

            Vector3 current_position = transform.position;
            Vector3 current_angle = transform.rotation.eulerAngles;

            velocity = rb.velocity;
            // displacement = transform.position - last_position;
            // velocity = displacement / Time.deltaTime;
            acceleration = (velocity - last_velocity)/Time.fixedDeltaTime;

            msg.a_x = acceleration[0];
            msg.a_y = acceleration[1];
            msg.a_z = acceleration[2];
            last_velocity = velocity;
            last_position = current_position;
            
            angular_velocity = rb.angularVelocity * (180/Mathf.PI);
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

        time_Update += Time.fixedDeltaTime;
        
    }

    public Vector3 send_acceleration()
    {
        return acceleration;
    }

    public Vector3 send_velocity()
    {
        return velocity;
    }

    public float send_time()
    {
        return time_Update;
    }
}
