using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sonar : MonoBehaviour
{
    public GameObject preFab;

    public float sound_speed = 5f;

    [SerializeField]
    float distance;

    // Start is called before the first frame update
    GameObject beam;
    void Start()
    {
        beam = Instantiate(preFab,new Vector3(-0.3251f,0,0),Quaternion.identity);
        beam.GetComponent<single_beam_test>().velocity = sound_speed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        float time_travelled = other.gameObject.GetComponent<single_beam_test>().time_travelled;
        distance = time_travelled/2 * sound_speed;
        Debug.Log(distance);
        Destroy(other.gameObject);
    }
}
