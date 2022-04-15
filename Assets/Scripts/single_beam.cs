using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector; 
using RosMessageTypes.UnityRoboticsDemo; 

public class single_beam : MonoBehaviour
{
    public GameObject prefab,coord,graph;
    public float FOV,multiplier,scale,offset,tot,tolerance;
    public int k;
    List<float> vals,bef,aft;
    //float[] aft2;
    List<GameObject> coords;
    //ROSConnection ros; 
    //private string topicName = "sonar_measurements";
    void Start()
    {
        // ros = ROSConnection.GetOrCreateInstance(); 
        // ros.RegisterPublisher<SonarMsg>(topicName);

        tot = 0;
        vals = new List<float>();
        bef = new List<float>();
        coords = new List<GameObject>();
        for (int i = 0; i < multiplier; ++i)
        {
            //Instantiate probe 
            GameObject ray=Instantiate(prefab);
            float value = -FOV/2 + i * (FOV /( multiplier-1));
            ray.transform.SetParent(gameObject.transform);
            ray.transform.localRotation=(Quaternion.Euler(0,value,0));
            GameObject coordinate = Instantiate(coord);
            coordinate.transform.SetParent(graph.transform);
            vals.Add(value);
            Debug.Log(value);         
            coords.Add(coordinate);
        }
       
    }
    private void FixedUpdate()
    {
        aft = new List<float>();
        SNR_array = new List<float>();
        for (int i=0;i<transform.childCount;++i)
        {
            try
            {
                Vector2 ini=coords[i].GetComponent<RectTransform>().anchoredPosition;
               
                float r = transform.GetChild(i).GetComponent<raycast_script>().hit_val;
                float SNR = transform.GetChild(i).GetComponent<raycast_script>().SNR;
                float cos_theta = Mathf.Cos(Mathf.Deg2Rad*vals[i]);
                float sin_theta = Mathf.Sin(Mathf.Deg2Rad*vals[i]);
                aft.Add(r*cos_theta);
                coords[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(scale*r*sin_theta,scale*r*cos_theta);
            }

            catch(System.Exception e)
            {
                Debug.Log(e);
            }
            
        }

        //aft2 = new float[aft.Count];
        if (bef.Count>0)
        {
            tot = 0;
            k = 0;
            for (int i = 0; i < aft.Count; ++i)
            {
                float diff = aft[i] - bef[i];
                if (Mathf.Abs(diff) > tolerance)
                {
                    k += 1;
                    tot += (Mathf.Abs(diff));
                }
                //aft2[i] = aft[i];
            }
            if(k!=0)
            tot /= k;
      
            tot /= Time.fixedDeltaTime;

        }
        bef = aft;

        // SonarMsg sonar_msg = new SonarMsg();
        // sonar_msg.ranges = aft2;
        // ros.Publish(topicName,sonar_msg);
    }

}
