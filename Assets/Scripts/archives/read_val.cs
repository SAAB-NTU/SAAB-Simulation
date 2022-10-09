using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class read_val : MonoBehaviour
{
    // Start is called before the first frame update
    
    public GameObject cube;
    public float sonar_distance,accel_x,accel_y,accel_z,angular_x,angular_y,angular_z;
    int confidence;
    float sonar_timer = 0;
    float timer = 0;
    float positionX,positionY,positionZ;

    void Start() //save to csv
    {
        var w = new StreamWriter("kalman_filter_with_noise.csv",true); //csv file saved to Asset folder
        var line = "time,sensor,sonar_distance,confidence,position x,position y,position z,accel x,accel y,angular_vel x,angular_vel y,angular_vel z";
        w.WriteLine(line);
        w.Close();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (sonar_timer >= 0.05)
        {
            sonar_distance = cube.GetComponent<raycast_script>().hit_val;
            confidence = cube.GetComponent<raycast_script>().confidence;
            positionX = cube.transform.position.x;
            positionY = cube.transform.position.y;
            positionZ = cube.transform.position.z;
            var w = new StreamWriter("kalman_filter_with_noise.csv",true);
            var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",timer,"sonar",sonar_distance,confidence,
                                      positionX,positionY,positionZ,"","","","","","");
            sonar_timer = 0;
            w.WriteLine(line);
            w.Close();
        }

        else
        {
            Vector3 acceleration = cube.GetComponent<imu_noise>().real_accel;
            Vector3 angular_velocity = cube.GetComponent<imu_noise>().real_gyro;
            accel_x = acceleration[0];
            accel_y = acceleration[1];
            accel_z = acceleration[2];
            angular_x = angular_velocity[0];
            angular_y = angular_velocity[1];
            angular_z = angular_velocity[2];

            var w = new StreamWriter("kalman_filter_with_noise.csv",true);
            var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",timer,"imu","","","","","",
                                    accel_x,accel_y,accel_z,angular_x,angular_y,angular_z);
            w.WriteLine(line);
            w.Close();
            sonar_timer += Time.fixedDeltaTime;
        }
        timer += Time.fixedDeltaTime;
    }
}
