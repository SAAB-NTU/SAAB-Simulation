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
    public Vector3 last_velocity = Vector3.zero;
    public Vector3 angular_velocity;
    private float timeElapsed = 0;
    private float timeElapsed_start = 0;
    float num = 0f;
    
    // Start is called before the first frame update
    void Start()
    {	

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImuMsg>(topicName);
        ros.RegisterPublisher<PointCloudMsg>(topicName2);
    }
    
    // Update is called once per frame
    void Update()
    { 
        timeElapsed_start += Time.deltaTime;
        if (timeElapsed_start > 3)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            // float horizontalInput = Input.GetAxis("Horizontal");
            // transform.position += new Vector3 (horizontalInput*Time.deltaTime*5,0,0);

            // if (Input.GetKey(KeyCode.U)){
            //     num -= 0.01f;
            //     rb.velocity = new Vector3(num*5,0,0);
            // }
            
            // if (Input.GetKey(KeyCode.D)){
            //     transform.Rotate(-Vector3.up * 5 * Time.deltaTime);
            // }

            timeElapsed += Time.deltaTime;
            

            if (timeElapsed > 0) //to publish at desired rate
            {
                ImuMsg msg = new ImuMsg();
                PointCloudMsg msg2 = new PointCloudMsg();

                Vector3 velocity = rb.velocity;
                Vector3 acceleration = (velocity - last_velocity)/Time.fixedDeltaTime;
                Debug.Log(acceleration[0]);
                msg.a_x = acceleration[0];
                msg.a_y = acceleration[1];
                msg.a_z = acceleration[2];
                last_velocity = velocity;

                angular_velocity = new Vector3 (rb.angularVelocity.x,rb.angularVelocity.y,rb.angularVelocity.z);
                msg.w_x = angular_velocity[0];
                msg.w_y = angular_velocity[1];
                msg.w_z = angular_velocity[2];
                ros.Publish(topicName,msg);
                
                msg2.x = transform.position.x;
                msg2.y = transform.position.y;
                msg2.z = transform.position.z;
                ros.Publish(topicName2,msg2);
                timeElapsed = 0;
            }
        }
    }
}
