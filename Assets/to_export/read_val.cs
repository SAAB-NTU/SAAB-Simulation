using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class read_val : MonoBehaviour
{
    public Cube IMU;
    public Rigidbody Real;
    public single_beam SONAR;
    // Start is called before the first frame update
    public float predicted_velocity, real_velocity, measured_velocity,predicted_error,measured_error;
    // Update is called once per frame
    void FixedUpdate()
    {
        predicted_velocity=Mathf.Abs(IMU.velocity.x);
        real_velocity= Mathf.Abs(Real.velocity.x);
        measured_velocity = Mathf.Abs(SONAR.tot);
        predicted_error = 100*Mathf.Abs((predicted_velocity - real_velocity) / real_velocity);
        measured_error = 100 * Mathf.Abs((measured_velocity - real_velocity)/ real_velocity);
    }
}
