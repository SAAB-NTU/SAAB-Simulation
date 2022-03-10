using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class imu_noise : MonoBehaviour
{

    int fs;
    int scale;
    
    Vector3 accel_err;
    Vector3 gyro_err;
    Vector3 old_accel_bias_drift = Vector3.zero;
    Vector3 old_gyro_bias_drift = Vector3.zero;
    Vector3 old_accel_drift_noise = Vector3.zero;
    Vector3 old_gyro_drift_noise  = Vector3.zero;
    Vector3 a_accel = Vector3.zero;
    Vector3 b_accel = Vector3.zero;
    Vector3 a_gyro = Vector3.zero;
    Vector3 b_gyro = Vector3.zero;

    public  GameObject cube; 
    public string imu_model = "high";
    public Vector3 real_accel = Vector3.zero;
    public Vector3 real_gyro = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        imu_models = cube.GetComponent<imu_model>().getDictionary();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        true_acceleration = cube.GetComponent<Cube>().send_acceleration(); //get true acceleration
        real_accel = accel_gen(true_acceleration,imu_models[imu_model]);
        real_gyro = gyro_gen();
    }

    Vector3 accel_gen(Vector3 ref_a, Vector3 accel_err)
    {
        float dt = 1.0f/fs;
        // simulate sensor error
        // static bias
        Vector3 accel_bias = accel_err["b"];
        // bias drift
        accel_bias_drift,old_accel_drift_noise = bias_drift("accel",accel_err["b_corr"], accel_err["b_drift"],old_accel_bias_drift,old_accel_drift_noise);

       // accelerometer white noise
        Vector3 accel_noise = new Vector3(random_gaussian,random_gaussian,random_gaussian);
        accel_noise[0] = accel_err["vrw"][0] / math.sqrt(dt) * accel_noise[0];
        accel_noise[1] = accel_err["vrw"][1] / math.sqrt(dt) * accel_noise[1];
        accel_noise[2] = accel_err["vrw"][2] / math.sqrt(dt) * accel_noise[2];
       // true + constant_bias + bias_drift + noise
       //a_mea = ref_a + scale*accel_bias + scale*accel_bias_drift + scale*accel_noise  //+ acc_vib
        a_mea = ref_a;
        old_accel_bias_drift = accel_bias_drift; //store current bias drift for next reading
        return a_mea;
    }

    Vector3 gyro_gen(Vector3 ref_ws, Vector3 gyro_err)
    {
        float dt = 1.0/fs;
        // simulate sensor error
        // static bias
        gyro_bias = gyro_err["b"];

        // bias drift
        gyro_bias_drift,old_gyro_drift_noise = bias_drift("gyro",gyro_err["b_corr"], gyro_err["b_drift"], old_gyro_bias_drift,old_gyro_drift_noise);

        // gyroscope white noise
        gyro_noise = np.random.randn(3);

        gyro_noise[0] = gyro_err["arw"][0] / math.sqrt(dt) * gyro_noise[0];
        gyro_noise[1] = gyro_err["arw"][1] / math.sqrt(dt) * gyro_noise[1];
        gyro_noise[2] = gyro_err["arw"][2] / math.sqrt(dt) * gyro_noise[2];
        // true + constant_bias + bias_drift + noise
        //w_mea = ref_w + scale*gyro_bias + scale*gyro_bias_drift + scale*gyro_noise  // + gyro_vib
        w_mea = ref_w;
        old_gyro_bias_drift = gyro_bias_drift;
        return w_mea;
    }

    Vector3 bias_drift(string sensor_type,Vector3 corr_time,Vector3 drift,Vector3 old_sensor_bias_drift,Vector3 old_drift_noise)
    {
        var a_b_dict = new Dictionary<string,Dictionary<string,Vector3>>()
        {
            {"accel",new Dictionary<string,Vector3>(){{"a",a_accel},{"b",b_accel}}},
            {"gyro",new Dictionary<string,Vector3>(){{"a",a_gyro},{"b",b_gyro}}}
        };
        a = a_b_dict[sensor_type]["a"];
        b = a_b_dict[sensor_type]["b"];
        // 3 axis
        sensor_bias_drift = Vector3.zero;
        for(int i=0;i < 3;i++)
        {
            if (double.IsInfinity(corr_time[i]) != true)
            {
                //sensor_bias_drift[0, :] = np.random.randn(3) * drift
                drift_noise = np.random.randn(3);
                //for j in range(1, n):
                sensor_bias_drift[i] = a[i]*old_sensor_bias_drift[i] + b[i]*old_drift_noise[i];
            }
            else
            {
                // normal distribution
                sensor_bias_drift[i] = drift[i] * np.random.randn(1);
            }

        }
        return sensor_bias_drift,drift_noise;
    }

    void set_sensor_bias_param()
    {
        //only called once when receiving first reading
        Dictionary<string,Dictionary<string,Vector3>> params_dict = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            {"accel",new Dictionary<string,Vector3>(){{"corr",accel_err["b_corr"]}, {"drift",accel_err["b_drift"]},{"a",a_accel},{"b",b_accel}}},
            {"gyro",new Dictionary<string,Vector3>(){{"corr",accel_err["b_corr"]},{"drift",accel_err["b_drift"]},{"a",a_gyro},{"b",b_gyro}}}
        };
        foreach(KeyValuePair<string,Dictionary<string,Vector3>> param in params_dict)
            Vector3 corr_time = param.Value["corr"];
            Vector3 drift = param.Value["drift"];
            Vector3 a = param.Value["a"];
            Vector3 b = param.Value["b"];
            
            //First-order Gauss-Markov
            a[0] = 1 - 1/fs/corr_time[0];
            a[1] = 1 - 1/fs/corr_time[1];
            a[2] = 1 - 1/fs/corr_time[2];

            //For the following equation, see issue #19 and
            //https://www.ncbi.nlm.nih.gov/pmc/articles/PMC3812568/ (Eq. 3).
            b[0] = drift[0] * np.sqrt(1.0 - np.exp(-2/(fs * corr_time[0])));
            b[1] = drift[1] * np.sqrt(1.0 - np.exp(-2/(fs * corr_time[1])));
            b[2] = drift[2] * np.sqrt(1.0 - np.exp(-2/(fs * corr_time[2])));

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
}
