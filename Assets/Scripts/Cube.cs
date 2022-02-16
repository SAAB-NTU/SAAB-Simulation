using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

public class Cube : MonoBehaviour
{
    ROSConnection ros; 
    Rigidbody rb;

    public string topicName = "imu_true";
    public Vector3 last_velocity = Vector3.zero;
    public Vector3 angular_velocity;
    public float publishMessageFrequency = 1000f;
    private float timeElapsed;
    public moveSpeed = 5;

    
    
    // Start is called before the first frame update
    void Start()
    {	
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImuMsg>(topicName);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    { 	
        float verticalInput = Input.GetAxis("Vertical");
        transform
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency) //to publish at desired rate
        {
            ImuMsg msg = new ImuMsg();

            Vector3 velocity = rb.velocity;
            Vector3 acceleration = (velocity - last_velocity)/Time.deltaTime;
            msg.a_x = acceleration[0];
            msg.a_y = acceleration[1];
            msg.a_z = acceleration[2];
            Debug.Log(acceleration);
            last_velocity = velocity;

            angular_velocity = new Vector3 (rb.angularVelocity.x,rb.angularVelocity.y,rb.angularVelocity.z);
            msg.w_x = angular_velocity[0];
            msg.w_y = angular_velocity[1];
            msg.w_z = angular_velocity[2];
            Debug.Log(angular_velocity);
            ros.Publish(topicName,msg);
            timeElapsed = 0;
        }
    }
}
