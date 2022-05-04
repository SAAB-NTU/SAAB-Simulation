using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Graph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    // Start is called before the first frame update
    void Start()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        createCircle(new Vector2(200, 200));
    }

    void createCircle(Vector2 anchoredPosition)//Points
    {
        print("hi");
        GameObject gameobject = new GameObject(("circle"),typeof(Image));
        gameobject.transform.SetParent(graphContainer, false);
        gameobject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameobject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta=new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
