using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewItem : MonoBehaviour
{
    [SerializeField]
    private GameObject item;

    [SerializeField]
    private List<string> favorite;

    // Start is called before the first frame update
    void Start()
    {
        foreach(string item in favorite)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
