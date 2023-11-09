using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBox : MonoBehaviour
{
    public TextMeshPro tmp;
    public GameObject surface;
    
    // Start is called before the first frame update
    void Start()
    {
        tmp.SetText("The quick brown fox jumps over the lazy dog.");
    }

    // Update is called once per frame
    void Update()
    {
        var newScale = new Vector3((float) Math.Ceiling(tmp.preferredWidth), (float) Math.Ceiling(tmp.preferredHeight), surface.transform.localScale.z);
        surface.transform.localScale = newScale;
    }
}
