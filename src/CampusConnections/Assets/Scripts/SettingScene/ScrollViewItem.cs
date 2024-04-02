using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewItem : MonoBehaviour
{
    [SerializeField] private GameObject item;

    [SerializeField] private List<string> favorite;

    // Start is called before the first frame update
    private void Start()
    {
        foreach (var s in favorite)
        {
            // TODO: Is this supposed to be empty?
        }
    }
}