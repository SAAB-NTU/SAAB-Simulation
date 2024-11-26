using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sonar_particle_system : MonoBehaviour
{
    public GameObject preFab;
    public Vector3 normal_ray;
    public float sound_speed = 5f;
    public float frequency = 10f;
    public float FOV = 30;
    public float multiplier = 3;
    public float SNR = 10f;

    [SerializeField]
    float distance;
    List<float> distance_list = new List<float>();
    float time;

    // Start is called before the first frame update
    void Start()
    {
        time = 0f;
        float angle = FOV/(2);
        for(int i = 0;i < multiplier;i++)
        {
            float x = -sound_speed * Mathf.Cos(Mathf.Deg2Rad*angle);
            float y = 0;
            float z = -sound_speed * Mathf.Sin(Mathf.Deg2Rad *angle);
            GameObject beam = Instantiate(preFab);
            beam.GetComponent<single_beam_test>().SNR = Vector3.one * SNR;
            beam.GetComponent<Rigidbody>().linearVelocity = new Vector3(x,y,z);
            angle -= FOV/(multiplier-1);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time += Time.deltaTime;
        if (time >= (1/frequency))
        {
            float angle = FOV/(2);
            for(int i = 0;i < multiplier;i++)
            {
                float x = -sound_speed * Mathf.Cos(Mathf.Deg2Rad*angle);
                float y = 0;
                float z = -sound_speed * Mathf.Sin(Mathf.Deg2Rad *angle);
                GameObject beam = Instantiate(preFab);
                beam.GetComponent<single_beam_test>().SNR = Vector3.one * SNR;
                beam.GetComponent<Rigidbody>().linearVelocity = new Vector3(x,y,z);
                angle -= FOV/(multiplier-1);
            }
            time = 0f;
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        float time_travelled = other.gameObject.GetComponent<single_beam_test>().time_travelled;
        distance = time_travelled/2 * sound_speed;
        Debug.Log(distance);
        distance_list.Add(distance);
        Destroy(other.gameObject);
    }
}
