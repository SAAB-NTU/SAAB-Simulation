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
    // Start is called before the first frame update
    void Start()
    {
        tot = 0;
        vals = new List<float>();
        bef = new List<float>();
        aft = new List<float>();
        coords = new List<GameObject>();
        for (int i = 0; i < multiplier; ++i)
        {
            print("in");
            GameObject ray=Instantiate(prefab);
            
            float value = -FOV/2 + i * (FOV /( multiplier-1));
            ray.transform.SetParent(gameObject.transform);
            // print(495*i/(5*multiplier));
            print(value);
            //ray.transform.rotation = gameObject.transform.rotation;
            ray.transform.localRotation=(Quaternion.Euler(0,value,0));
            
            GameObject coordinate = Instantiate(coord);
            coordinate.transform.SetParent(graph.transform);
            vals.Add(value);
            //coordinate.GetComponent<RectTransform>().eulerAngles = new Vector3(0,0,value);
            //coordinate.GetComponent<RectTransform>().anchoredPosition=new Vector2(i*495/ (5 * multiplier), 0);
            
            
            coords.Add(coordinate);
        }
       
    }
    private void FixedUpdate()
    {
        aft = new List<float>();
        for (int i=0;i<transform.childCount;++i)
        {
            //vals.Add(transform.GetChild(i).GetComponent<raycast_script>().hit_val);
            try
            {
            Vector2 ini=coords[i].GetComponent<RectTransform>().anchoredPosition;
               
                float r = scale * transform.GetChild(i).GetComponent<raycast_script>().hit_val;
                float cos_theta = Mathf.Cos(Mathf.PI*(vals[i]/180));
                float sin_theta = Mathf.Sin(Mathf.PI * (vals[i] / 180));
                    aft.Add(r*cos_theta/scale);
                //aft.Add(r  / scale);
                coords[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(offset+(r *cos_theta),Mathf.Abs(r * sin_theta));
            }
            catch(System.Exception e)
            {
                //print("It's ok");
                //do nothing
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
      
            tot /= Time.fixedUnscaledDeltaTime;

        }
        bef = aft;
        
        // print(vals.Count);
        // vals = new List<float>();
        // print(hits.Length);
    }
    // Update is called once per frame

}
