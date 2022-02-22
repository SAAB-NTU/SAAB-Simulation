using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class raycast_script2 : MonoBehaviour
{
    public GameObject prefab,coord,graph;
    public float multiplier,scale;

    //List<float> vals;
    List<GameObject> coords;
    // Start is called before the first frame update
    void Start()
    {
        //vals = new List<float>();
        coords = new List<GameObject>();
        for (int i = 0; i < 5*multiplier; ++i)
        {
            print("in");
            GameObject ray=Instantiate(prefab);
            ray.transform.parent = gameObject.transform;
            float value = -135f + i * (18f /( multiplier));
            print(495*i/(5*multiplier));
            ray.transform.rotation=(Quaternion.Euler(0, value,0));
            GameObject coordinate = Instantiate(coord);
            coordinate.transform.SetParent(graph.transform);
            coordinate.GetComponent<RectTransform>().anchoredPosition=new Vector2(i*495/ (5 * multiplier), 0);
            
            
            coords.Add(coordinate);
        }
       
    }
    private void Update()
    {
        
        for(int i=0;i<transform.childCount;++i)
        {
            //vals.Add(transform.GetChild(i).GetComponent<raycast_script>().hit_val);
            try
            {
            Vector2 ini=coords[i].GetComponent<RectTransform>().anchoredPosition;
            coords[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(ini.x, scale*transform.GetChild(i).GetComponent<raycast_script>().hit_val);
            }
            catch(System.Exception e)
            {
                print("It's ok");
            }
            
        }
       // print(vals.Count);
       // vals = new List<float>();
       // print(hits.Length);
    }
    // Update is called once per frame

}
