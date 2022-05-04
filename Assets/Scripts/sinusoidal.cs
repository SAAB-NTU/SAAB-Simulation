using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sinusoidal : MonoBehaviour

{
    public float frequency,amplitude;

    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
        GetComponent<Rigidbody>().velocity = new Vector3(amplitude * -Mathf.Sin(frequency * Time.unscaledTime), 0, 0);
    }
}
