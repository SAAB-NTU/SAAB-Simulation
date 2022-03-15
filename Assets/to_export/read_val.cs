using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class read_val : MonoBehaviour
{
    public GameObject IMU;
    public Rigidbody Real;
    public single_beam SONAR;
    // Start is called before the first frame update
    public float predicted_velocity, real_velocity, measured_velocity,predicted_error,measured_error;

    

    // void Start() //save to csv
    // {
    //     var w = new StreamWriter("<file>.csv",true); //csv file saved to Asset folder
    //     var line = "predicted_velocity,real_velocity,measured_velocity,predicted_error,measured_error";
    //     w.WriteLine(line);
    //     w.Close();
    // }

    // Update is called once per frame
    void FixedUpdate()
    {
        predicted_velocity = Mathf.Abs((IMU.GetComponent<Estimated_Position>().estimated_velocity()).x); //for x direction only
        real_velocity= Mathf.Abs(Real.velocity.x);
        measured_velocity = Mathf.Abs(SONAR.tot);
        predicted_error = 100*Mathf.Abs((real_velocity - predicted_velocity ) / real_velocity);
        measured_error = 100 * Mathf.Abs((real_velocity - measured_velocity )/ real_velocity);

        //save to csv
        // var w = new StreamWriter("<file>",true);
        // var line = string.Format("{0},{1},{2},{3},{4}",predicted_velocity,real_velocity,measured_velocity,predicted_error,measured_error);
        // w.WriteLine(line);
        // w.Close();
    }
}
