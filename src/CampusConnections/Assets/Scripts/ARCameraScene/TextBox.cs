using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Game object template for AR elements
/// Can add sound effect and a carousel view besides plain text
/// Author: Zihao Du
/// Date: 2024-02-04
/// </summary>
public class TextBox : MonoBehaviour
{
    public TextMeshPro tmp;
    public GameObject surface;
    private AudioSource _clickSound;
    [SerializeField] private string textBoxName;
    [SerializeField] private GameObject carousel;

    private void Start()
    {
        _clickSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        var newScale = new Vector3((float)Math.Ceiling(tmp.preferredWidth), (float)Math.Ceiling(tmp.preferredHeight),
            surface.transform.localScale.z);
        surface.transform.localScale = newScale;
        // on click event
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, Mathf.Infinity))
                    if (hit.collider.name == textBoxName)
                    {
                        _clickSound.Play();
                        if (carousel != null)
                        {
                            GameObject.Find(textBoxName).SetActive(false);
                            carousel.SetActive(true);
                        }
                    }
            }
        }
    }
}