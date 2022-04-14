using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class single_beam : MonoBehaviour
{
    public GameObject prefab,coord,graph;
    public float FOV,multiplier,scale,offset,tot,tolerance;
    public int k;
    List<float> vals,bef,aft;
    List<GameObject> coords;
    void Start()
    {
        tot = 0;
        vals = new List<float>();
        bef = new List<float>();
        aft = new List<float>();
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
        for (int i=0;i<transform.childCount;++i)
        {
            try
            {
                Vector2 ini=coords[i].GetComponent<RectTransform>().anchoredPosition;
               
                float r = scale*transform.GetChild(i).GetComponent<raycast_script>().hit_val;
                float cos_theta = Mathf.Cos(Mathf.Deg2Rad*vals[i]);
                float sin_theta = Mathf.Sin(Mathf.Deg2Rad*vals[i]);
                aft.Add(r*cos_theta);
                coords[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(r*sin_theta,r*cos_theta);
            }

            catch(System.Exception e)
            {
                Debug.Log(e);
            }
            
        }

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
            }
            if(k!=0)
            tot /= k;
      
            tot /= Time.fixedDeltaTime;

        }
        bef = aft;
    }

}
