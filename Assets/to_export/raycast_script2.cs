using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class raycast_script2 : MonoBehaviour
{
    public GameObject prefab,coord,graph;
    public float multiplier,scale,offset,tot;

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
        for (int i = 0; i < 5*multiplier; ++i)
        {
            print("in");
            GameObject ray=Instantiate(prefab);
            ray.transform.parent = gameObject.transform;
            float value = -135f + i * (18f /( multiplier));
           // print(495*i/(5*multiplier));
            ray.transform.rotation=(Quaternion.Euler(0, value,0));
            GameObject coordinate = Instantiate(coord);
            coordinate.transform.SetParent(graph.transform);
            vals.Add(value);
            //coordinate.GetComponent<RectTransform>().eulerAngles = new Vector3(0,0,value);
            //coordinate.GetComponent<RectTransform>().anchoredPosition=new Vector2(i*495/ (5 * multiplier), 0);
            
            
            coords.Add(coordinate);
        }
       
    }
    private void Update()
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
                aft.Add(r);
                coords[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(offset+(r *cos_theta),Mathf.Abs(r * sin_theta));
            }
            catch(System.Exception e)
            {
                //print("It's ok");
                //do nothing
                Debug.Log(e);
            }
            
        }
        if (bef.Count>0)
        {
            tot = 0;
            for (int i = 0; i < aft.Count; ++i)
            {
                tot += (Mathf.Abs(aft[i] - bef[i]));
            }
            tot /= bef.Count;
            tot /= Time.deltaTime;

        }
        bef = aft;
        
        // print(vals.Count);
        // vals = new List<float>();
        // print(hits.Length);
    }
    // Update is called once per frame

}
