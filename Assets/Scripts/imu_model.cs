using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
gyro_err: gyroscope error parameters.
            "b": 3x1 constant gyro bias, rad/s.
            "b_drift": 3x1 gyro bias drift, rad/s.
            "arw": 3x1 angle random walk, rad/s/root-Hz.

acc_err: accelerometer error parameters.
            "b": 3x1 acc constant bias, m/s2.
            "b_drift": 3x1 acc bias drift, m/s2.
            "vrw": 3x1 velocity random walk, m/s2/root-Hz.
**/

public class imu_model : MonoBehaviour
{
    // low accuracy, from AHRS380
    // http://www.memsic.cn/userfiles/files/Datasheets/Inertial-System-Datasheets/AHRS380SA_Datasheet.pdf

    float D2R = Mathf.PI/180;

    Dictionary<string,Vector3> gyro_low_accuracy;
    Dictionary<string,Vector3> gyro_mid_accuracy;
    Dictionary<string,Vector3> gyro_high_accuracy;
    Dictionary<string,Vector3> gyro_mti_100;

    Dictionary<string,Vector3> accel_low_accuracy;
    Dictionary<string,Vector3> accel_mid_accuracy;
    Dictionary<string,Vector3> accel_high_accuracy;
    Dictionary<string,Vector3> accel_mti_100;

    Dictionary<string,Dictionary<string,Dictionary<string,Vector3>>> IMU_dict;
    void Start()
    {
        gyro_low_accuracy = new Dictionary<string,Vector3>()
        {
            {"b",Vector3.zero},
            {"b_drift", new Vector3(10.0f,10.0f,10.0f) * D2R/3600.0f},
            {"b_corr",new Vector3(100.0f,100.0f,100.0f)},
            {"arw", new Vector3(0.75f,0.75f,0.75f) * D2R/60.0f}
        };

        accel_low_accuracy = new Dictionary<string,Vector3>()
        {
            {"b", Vector3.zero},
            {"b_drift", new Vector3(2.0e-4f, 2.0e-4f, 2.0e-4f)},
            {"b_corr", new Vector3(100.0f, 100.0f, 100.0f)},
            {"vrw", new Vector3(0.05f, 0.05f, 0.05f) / 60.0f}
        };

        // mid accuracy, partly from IMU381
        gyro_mid_accuracy = new Dictionary<string,Vector3>()
        {
            {"b", new Vector3(0.0f, 0.0f, 0.0f) * D2R},
            {"b_drift", new Vector3(3.5f, 3.5f, 3.5f) * D2R/3600.0f},
            {"b_corr",new Vector3(100.0f, 100.0f, 100.0f)},
            {"arw", new Vector3(0.25f, 0.25f, 0.25f) * D2R/60f}
        };
        accel_mid_accuracy = new Dictionary<string,Vector3>()
        {
            {"b", new Vector3(0.0e-3f, 0.0e-3f, 0.0e-3f)},
            {"b_drift", new Vector3(5.0e-5f, 5.0e-5f, 5.0e-5f)},
            {"b_corr", new Vector3(100.0f, 100.0f, 100.0f)},
            {"vrw", new Vector3(0.03f, 0.03f, 0.03f) / 60f}
        };

        // high accuracy, partly from HG9900, partly from
        // http://www.dtic.mil/get-tr-doc/pdf?AD=ADA581016
        gyro_high_accuracy = new Dictionary<string,Vector3>()
        {
            {"b", new Vector3(0.0f, 0.0f, 0.0f) * D2R},
            {"b_drift", new Vector3(0.1f, 0.1f, 0.1f) * D2R/3600.0f},
            {"b_corr",new Vector3(100.0f, 100.0f, 100.0f)},
            {"arw", new Vector3(2.0e-3f, 2.0e-3f, 2.0e-3f) * D2R/60f}
        };
        accel_high_accuracy = new Dictionary<string,Vector3>()
        {
            {"b", new Vector3(0.0e-3f, 0.0e-3f, 0.0e-3f)},
            {"b_drift", new Vector3(3.6e-6f, 3.6e-6f, 3.6e-6f)},
            {"b_corr", new Vector3(100.0f, 100.0f, 100.0f)},
            {"vrw", new Vector3(2.5e-5f, 2.5e-5f, 2.5e-5f) / 60f}
        };

        // https://mtidocs.xsens.com/output-specifications$sensor-data-performance-specifications
        // used noise density for vrw
        // uncalibrated ?? 
        gyro_mti_100= new Dictionary<string,Vector3>()
        {
            {"b", new Vector3(0.0f, 0.0f, 0.0f)* D2R} ,
            {"b_drift", new Vector3(10.0f, 10.0f, 10.0f)* D2R/3600.0f} , 
            {"b_corr",new Vector3(100.0f, 100.0f, 100.0f)},
            {"arw", new Vector3(0.01f, 0.01f, 0.01f)* D2R}
        };
        accel_mti_100 = new Dictionary<string,Vector3>()
        {
            {"b", new Vector3(0.0e-3f, 0.0e-3f, 0.0e-3f)}, 
            {"b_drift", new Vector3(1.5e-4f, 1.5e-4f, 1.5e-4f)},
            {"b_corr", new Vector3(100.0f, 100.0f, 100.0f)},
            {"vrw", new Vector3(6.0e-4f, 6.0e-4f, 6.0e-4f)} 
        };

        IMU_dict = new Dictionary<string,Dictionary<string,Dictionary<string,Vector3>>>()
        {
            {"low",new Dictionary<string,Dictionary<string,Vector3>>(){{"accel",accel_low_accuracy},{"gyro",gyro_low_accuracy}}},
            {"mid",new Dictionary<string,Dictionary<string,Vector3>>(){{"accel",accel_mid_accuracy},{"gyro",gyro_mid_accuracy}}},
            {"high",new Dictionary<string,Dictionary<string,Vector3>>(){{"accel",accel_high_accuracy},{"gyro",gyro_high_accuracy}}},
            {"mti-100",new Dictionary<string,Dictionary<string,Vector3>>(){{"accel",accel_mti_100},{"gyro",gyro_mti_100}}}
        };

    }

    public Dictionary<string,Dictionary<string,Dictionary<string,Vector3>>> getDictionary()
    {
        return IMU_dict;
    }
    
}






