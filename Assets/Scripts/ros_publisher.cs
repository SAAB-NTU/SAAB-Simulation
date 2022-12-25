using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector; 
using RosMessageTypes.Unity; 

public class ros_publisher : MonoBehaviour
{
    ROSConnection ros;
    public GameObject cube;
    public string sonar_topic = "/unity/sonar";
    public string imu_topic = "/unity/imu";
    public string pose_topic = "/unity/pose";

    float sonar_timer = 0; float imu_timer = 0 ; 

    void Start() //save to csv
    {
        ros = ROSConnection.GetOrCreateInstance(); 
        ros.RegisterPublisher<ImuUnityMsg>(imu_topic);
        ros.RegisterPublisher<MultiSonarUnityMsg>(sonar_topic); //change message type according to sonar
        ros.RegisterPublisher<PoseUnityMsg>(pose_topic);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float time = Time.realtimeSinceStartup;
        if (sonar_timer >= 0.05)
        {
            //SonarUnityMsg sonar_msg = new SonarUnityMsg(); //single sonar
            //sonar_msg.distance = cube.GetComponent<raycast_script>().hit_val;

            MultiSonarUnityMsg sonar_msg = new MultiSonarUnityMsg();
            sonar_msg.distance_front= cube.GetComponent<multiraycast>().hit_val_front;
            sonar_msg.distance_left = cube.GetComponent<multiraycast>().hit_val_left;
            sonar_msg.distance_right = cube.GetComponent<multiraycast>().hit_val_right;
            sonar_msg.time = time;
            ros.Publish(sonar_topic,sonar_msg);
            sonar_timer = 0;
        }

        if (imu_timer >= 0.01)
        {
            ImuUnityMsg imu_msg = new ImuUnityMsg(); 
            Vector3 acceleration = cube.GetComponent<imu_noise>().real_accel;
            Vector3 angular_velocity = cube.GetComponent<imu_noise>().real_gyro;
            imu_msg.time = time;
            imu_msg.a_x = acceleration[2];
            imu_msg.a_y = -acceleration[0];
            imu_msg.a_z = acceleration[1];
            imu_msg.w_x = angular_velocity[2];
            imu_msg.w_y = -angular_velocity[0];
            imu_msg.w_z = angular_velocity[1];
            ros.Publish(imu_topic,imu_msg);
            imu_timer = 0;
        }

        PoseUnityMsg pose_msg = new PoseUnityMsg();
        pose_msg.time = time;
        pose_msg.position_x = cube.transform.position.z;
        pose_msg.position_y = -cube.transform.position.x;
        pose_msg.position_z = cube.transform.position.y;
        pose_msg.euler_x = Mathf.Deg2Rad * -cube.transform.eulerAngles.z;
        pose_msg.euler_y = Mathf.Deg2Rad * cube.transform.eulerAngles.x;
        if(cube.transform.eulerAngles.y == 0)
        {
            pose_msg.euler_z = 0;
        }
        else
        {
            pose_msg.euler_z = Mathf.Deg2Rad * (360-cube.transform.eulerAngles.y);
        }
        ros.Publish(pose_topic,pose_msg);

        imu_timer += Time.fixedDeltaTime;
        sonar_timer += Time.fixedDeltaTime;
    }
}
