using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBox : MonoBehaviour
{
    public TextMeshPro tmp;
    public GameObject surface;
    private AudioSource clickSound;
    [SerializeField] private string textBoxName;
    [SerializeField] private GameObject carousel;

    private void Start()
    {
        clickSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void Update()
    {
        var newScale = new Vector3((float)Math.Ceiling(tmp.preferredWidth), (float)Math.Ceiling(tmp.preferredHeight),
            surface.transform.localScale.z);
        surface.transform.localScale = newScale;
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                if (hit.collider.name == textBoxName)
                {
                    clickSound.Play();
                    if (carousel != null)
                    {
                        GameObject.Find(textBoxName).SetActive(false);
                        carousel.SetActive(true);
                    }
                }
        }
    }
}