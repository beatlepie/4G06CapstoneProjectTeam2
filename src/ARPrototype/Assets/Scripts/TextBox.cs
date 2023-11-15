using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBox : MonoBehaviour
{
    public static String text;

    public TextMeshPro tmp;
    public GameObject surface;
    
    // Start is called before the first frame update
    void Start()
    {
        text = "Tap for more info";
    }

    // Update is called once per frame
    void Update()
    {
        tmp.rectTransform.sizeDelta = new Vector2((float) Math.Floor(tmp.preferredWidth), (float) Math.Floor(tmp.preferredHeight));
        var newScale = new Vector3((float) Math.Ceiling(tmp.preferredWidth), (float) Math.Ceiling(tmp.preferredHeight), surface.transform.localScale.z);
        surface.transform.localScale = newScale;

        tmp.SetText(text);
    }
}
