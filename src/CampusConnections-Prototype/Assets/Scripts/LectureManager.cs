using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LectureManager : MonoBehaviour
{
    private Transform entryContainer;
    private Transform entryTemplate;

    private void Start()
    {
        UnityEngine.Debug.Log("lecture manager script running");
        entryContainer = transform.Find("lectureEntryContainer");
        entryTemplate = entryContainer.Find("lectureEntryTemplate");

        entryTemplate.gameObject.SetActive(false);

        float templateHeight = 90f;
        for (int i = 0; i < 6; i++)
        {
            Transform entryTransform = Instantiate(entryTemplate, entryContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * i);
            entryTransform.gameObject.SetActive(true);
        }

    }

}