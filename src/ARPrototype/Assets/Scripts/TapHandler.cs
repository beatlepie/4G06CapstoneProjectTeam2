using TMPro;
using UnityEngine;

public class TapHandler : MonoBehaviour
{

    public Camera mainCamera;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            var ray = mainCamera.ScreenPointToRay(Input.touches[0].position);

            if (Physics.Raycast(ray, out var hit))
            {
                var result = hit.transform.parent.GetChild(0).gameObject.GetComponent<TextMeshPro>();
                if (result != null)
                {
                    TextBox.text = "<Brief description of the building>";
                }
            }
        }
    }
}
