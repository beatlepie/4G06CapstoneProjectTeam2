using System;
using TMPro;
using UnityEngine;

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

    // Update is called once per frame
    private void Update()
    {
        var newScale = new Vector3((float)Math.Ceiling(tmp.preferredWidth), (float)Math.Ceiling(tmp.preferredHeight),
            surface.transform.localScale.z);
        surface.transform.localScale = newScale;
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