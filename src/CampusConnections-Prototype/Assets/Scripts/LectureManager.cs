using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LectureManager : MonoBehaviour
{
    private Transform entryContainer;
    private Transform entryTemplate;
    private List<LectureEntry> lectureEntryList;
    private List<Transform> lectureEntryTransformList;

    private void Awake()
    {
        UnityEngine.Debug.Log("lecture manager script running");
        entryContainer = transform.Find("lectureEntryContainer");
        entryTemplate = entryContainer.Find("lectureEntryTemplate");

        entryTemplate.gameObject.SetActive(false);

        lectureEntryList = new List<LectureEntry>() {
            new LectureEntry{ code = "1x03", instructor = "Dr.A", location = "ITB", name = "rounding 1", time = "3:30 - 4:30"},
            new LectureEntry{ code = "2x03", instructor = "Dr.B", location = "ITB", name = "rounding 2", time = "3:30 - 4:30"},
            new LectureEntry{ code = "3x03", instructor = "Dr.C", location = "ITB", name = "rounding 3", time = "3:30 - 4:30"},
        };

        lectureEntryTransformList = new List<Transform>();

        foreach (LectureEntry lectureEntry in lectureEntryList){
            CreateLectureEntryTransform(lectureEntry, entryContainer, lectureEntryTransformList);
        }
    }

    private void CreateLectureEntryTransform(LectureEntry lectureEntry, Transform container, List<Transform> transformList){
        float templateHeight = 90f;
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        int ind = transformList.Count + 1; //count for each entry if we want to print number later

        entryTransform.Find("nameText").GetComponent<TMP_Text>().text = lectureEntry.name;
        entryTransform.Find("codeText").GetComponent<TMP_Text>().text = lectureEntry.code;
        entryTransform.Find("instrucText").GetComponent<TMP_Text>().text = lectureEntry.instructor;
        entryTransform.Find("locText").GetComponent<TMP_Text>().text = lectureEntry.location;
        entryTransform.Find("timeText").GetComponent<TMP_Text>().text = lectureEntry.time;

        transformList.Add(entryTransform);
    }

    private class LectureEntry
    {
        public string code;
        public string instructor;
        public string location;
        public string name;
        public string time;
    }

}


