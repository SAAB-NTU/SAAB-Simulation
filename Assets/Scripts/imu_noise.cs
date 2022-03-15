using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

public class imu_noise : MonoBehaviour
{

    ROSConnection ros; 
    string topicName = "imu_noise";

    int fs = 1000;
    int scale = 1;
    
    Dictionary<string,Vector3> accel_err;
    Dictionary<string,Vector3> gyro_err;
    Vector3 old_accel_bias_drift = Vector3.zero;
    Vector3 old_gyro_bias_drift = Vector3.zero;
    Vector3 old_accel_drift_noise = Vector3.zero;
    Vector3 old_gyro_drift_noise  = Vector3.zero;
    Vector3 a_accel = Vector3.zero;
    Vector3 b_accel = Vector3.zero;
    Vector3 a_gyro = Vector3.zero;
    Vector3 b_gyro = Vector3.zero;

    public  GameObject cube; 
    public string imu_model;
    public Vector3 real_accel = Vector3.zero;
    public Vector3 real_gyro = Vector3.zero;

    float accel_noise_x;
    float accel_noise_y;
    float accel_noise_z;
    float gyro_noise_x;
    float gyro_noise_y;
    float gyro_noise_z;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImuMsg>(topicName);
        
        Dictionary<string,Dictionary<string,Dictionary<string,Vector3>>> imu_models = cube.GetComponent<imu_model>().getDictionary();
        accel_err = imu_models[imu_model]["accel"];
        gyro_err = imu_models[imu_model]["gyro"];

        List<Vector3> acceleration_param = set_sensor_bias_param(accel_err["b_corr"],accel_err["b_drift"]);
        a_accel = acceleration_param[0];
        b_accel = acceleration_param[1];

        List<Vector3> gyroscope_param = set_sensor_bias_param(gyro_err["b_corr"],gyro_err["b_drift"]);
        a_gyro = acceleration_param[0];
        b_gyro = acceleration_param[1];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ImuMsg msg = new ImuMsg();

        Vector3 true_acceleration = cube.GetComponent<Cube>().imu_true_accel(); //get true acceleration
        Vector3 true_angular_velocity = cube.GetComponent<Cube>().imu_true_gyro(); //get true angular velocity
        real_accel = accel_gen(true_acceleration,accel_err);
        real_gyro = gyro_gen(true_angular_velocity,gyro_err);

        msg.a_x = real_accel[0];
        msg.a_y = real_accel[1];
        msg.a_z = real_accel[2];
        msg.w_x = real_gyro[0];
        msg.w_y = real_gyro[1];
        msg.w_z = real_gyro[2];

        ros.Publish(topicName,msg);
    }

    Vector3 accel_gen(Vector3 ref_a, Dictionary<string,Vector3>accel_err)
    {
        float dt = 1.0f/fs;
        // simulate sensor error
        // static bias
        Vector3 accel_bias = accel_err["b"];
        // bias drift
        List<Vector3> acceleration_return = bias_drift("accel",accel_err["b_corr"], accel_err["b_drift"],old_accel_bias_drift,old_accel_drift_noise);
        Vector3 accel_bias_drift = acceleration_return[0];
        old_accel_drift_noise = acceleration_return[1];

       // accelerometer white noise
        Vector3 accel_noise = new Vector3(random_gaussian(),random_gaussian(),random_gaussian());
        accel_noise[0] = accel_err["vrw"][0] /Mathf.Sqrt(dt) * accel_noise[0];
        accel_noise[1] = accel_err["vrw"][1] /Mathf.Sqrt(dt) * accel_noise[1];
        accel_noise[2] = accel_err["vrw"][2] /Mathf.Sqrt(dt) * accel_noise[2];
       
        Vector3 total_accel_noise = scale*accel_bias + scale*accel_bias_drift + scale*accel_noise;

        // Debug.Log("accel_bias: " + accel_bias[0]); //to see contribution of each noise component for x 
        // Debug.Log("accel_bias_drift: "+ accel_bias_drift[0]);
        // Debug.Log("accel_noise: " + accel_noise[0]);

        accel_noise_x = total_accel_noise[0]; //show noise component in debug inspector
        accel_noise_y = total_accel_noise[1];
        accel_noise_z = total_accel_noise[2];

        // true + constant_bias + bias_drift + noise
        Vector3 a_mea = ref_a + accel_noise;  //+ acc_vib
        old_accel_bias_drift = accel_bias_drift; //store current bias drift for next reading
        return a_mea;
    }

    Vector3 gyro_gen(Vector3 ref_w, Dictionary<string,Vector3>gyro_err)
    {
        float dt = 1.0f/fs;
        // simulate sensor error
        // static bias
        Vector3 gyro_bias = gyro_err["b"];

        // bias drift
        List<Vector3> gyroscope_return = bias_drift("gyro",gyro_err["b_corr"], gyro_err["b_drift"], old_gyro_bias_drift,old_gyro_drift_noise);
        Vector3 gyro_bias_drift = gyroscope_return[0];
        old_gyro_drift_noise = gyroscope_return[1];

        // gyroscope white noise
        Vector3 gyro_noise = new Vector3(random_gaussian(),random_gaussian(),random_gaussian());

        gyro_noise[0] = gyro_err["arw"][0] /Mathf.Sqrt(dt) * gyro_noise[0];
        gyro_noise[1] = gyro_err["arw"][1] /Mathf.Sqrt(dt) * gyro_noise[1];
        gyro_noise[2] = gyro_err["arw"][2] /Mathf.Sqrt(dt) * gyro_noise[2];
        
        Vector3 total_gyro_noise = scale*gyro_bias + scale*gyro_bias_drift + scale*gyro_noise;

        // Debug.Log("gyro_bias: " + gyro_bias[0]); //to see contribution of each noise component for x
        // Debug.Log("gyro_bias_drift: "+ gyro_bias_drift[0]);
        // Debug.Log("gyro_noise: " + gyro_noise[0]);

        gyro_noise_x = total_gyro_noise[0]; //show noise component in debug inspector
        gyro_noise_y = total_gyro_noise[1];
        gyro_noise_z = total_gyro_noise[2];

        // true + constant_bias + bias_drift + noise
        Vector3 w_mea = ref_w + gyro_noise;  // + gyro_vib
        old_gyro_bias_drift = gyro_bias_drift;
        return w_mea;
    }

    List<Vector3> bias_drift(string sensor_type,Vector3 corr_time,Vector3 drift,Vector3 old_sensor_bias_drift,Vector3 old_drift_noise)
    {
        var a_b_dict = new Dictionary<string,Dictionary<string,Vector3>>()
        {
            {"accel",new Dictionary<string,Vector3>(){{"a",a_accel},{"b",b_accel}}},
            {"gyro",new Dictionary<string,Vector3>(){{"a",a_gyro},{"b",b_gyro}}}
        };
        Vector3 a = a_b_dict[sensor_type]["a"];
        Vector3 b = a_b_dict[sensor_type]["b"];
        // 3 axis
        Vector3 sensor_bias_drift = Vector3.zero;
        Vector3 drift_noise = Vector3.zero;
        for(int i=0;i < 3;i++)
        {
            if (double.IsInfinity(corr_time[i]) != true)
            {
                //sensor_bias_drift[0, :] = np.random.randn(3) * drift
                drift_noise[i] = random_gaussian();
                //for j in range(1, n):
                sensor_bias_drift[i] = a[i]*old_sensor_bias_drift[i] + b[i]*old_drift_noise[i];
            }
            else
            {
                // normal distribution
                sensor_bias_drift[i] = drift[i] * random_gaussian();
            }

        }
        List<Vector3> return_list = new List<Vector3>{sensor_bias_drift,drift_noise}; 
        return return_list;
    }

    List<Vector3> set_sensor_bias_param(Vector3 corr_time,Vector3 drift)
    {
        Vector3 a = Vector3.zero;
        Vector3 b = Vector3.zero;    

        //First-order Gauss-Markov
        a[0] = 1f - 1f/fs/corr_time[0];
        a[1] = 1f - 1f/fs/corr_time[1];
        a[2] = 1f - 1f/fs/corr_time[2];

        //For the following equation, see issue #19 and
        //https://www.ncbi.nlm.nih.gov/pmc/articles/PMC3812568/ (Eq. 3).
        b[0] = drift[0] * Mathf.Sqrt(1.0f - Mathf.Exp(-2f/(fs * corr_time[0])));
        b[1] = drift[1] * Mathf.Sqrt(1.0f - Mathf.Exp(-2f/(fs * corr_time[1])));
        b[2] = drift[2] * Mathf.Sqrt(1.0f - Mathf.Exp(-2f/(fs * corr_time[2])));
        
        List<Vector3> param_return = new List<Vector3>{a,b};
        return param_return;

    }
    float random_gaussian()
    {
        float mean = 0f;
        float stdDev = 1f;
        float u1 = 1.0f-Random.Range(0f,1f); //uniform(0,1] random doubles
        float u2 = 1.0f-Random.Range(0f,1f);
        float randStdNormal = (Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2)); //random normal(0,1)
        float randNormal = (mean + stdDev * randStdNormal); //random normal(mean,stdDev^2)
        return randNormal;
    }

    public Vector3 imu_noise_accel()
    {
        return real_accel;
    }

    public Vector3 imu_noise_gyro()
    {
        return real_gyro;
    }
}
