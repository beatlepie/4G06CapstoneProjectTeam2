using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using UnityEngine.EventSystems;

public class LectureManager : MonoBehaviour
{
    [Header("List View")]
    [SerializeField] TMP_Dropdown FilterDropdown;
    [SerializeField] TMP_InputField SearchString;
    private Transform entryContainer;
    private Transform entryTemplate;
    private List<Lecture> lectureEntryList;
    private List<Lecture> filteredList;
    private List<Transform> lectureEntryTransformList;
    public TMP_Text lecList;
    public TMP_Text pgNum;
    public int maxPages;
    public const int PAGECOUNT = 10;

    [Header("Detail View")]
    public static Lecture currentLecture; //The one we want to see details

    [Header("Database")]
    public DatabaseReference databaseReference;
    private void Awake()
    {
        UnityEngine.Debug.Log("lecture manager script running");
        //db setup
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        //after db stuff
        pgNum.text = "1";
        entryContainer = transform.Find("lectureEntryContainer");
        entryTemplate = entryContainer.Find("lectureEntryTemplate");
        entryTemplate.gameObject.SetActive(false);
        GetLectureData();
    }

    public void GetLectureData()
    {
        StartCoroutine(GetLectures());
    }

    IEnumerator GetLectures()
    {
        lectureEntryTransformList = new List<Transform>();
        lectureEntryList = new List<Lecture>();
        var lecInfo = new List<string>();
        var lectureData = databaseReference.Child("lectures").OrderByKey().StartAt("-").GetValueAsync();
        yield return new WaitUntil(predicate: () => lectureData.IsCompleted);
        if (lectureData != null)
        {
            string result = "";
            DataSnapshot snapshot = lectureData.Result;
            foreach (var x in snapshot.Children)
            {
                foreach (var i in x.Children)
                {
                    result += i.Value + " ";
                    lecInfo.Add(i.Value.ToString());
                    //UnityEngine.Debug.Log(lecInfo);
                }
                result += "\n";
                // make a lectureEntry object with constructor  (lecInfo[0],lecInfo[1]...)
                Lecture newEntry = new Lecture(lecInfo[0], lecInfo[1], lecInfo[2], lecInfo[3], lecInfo[4]);
                lectureEntryList.Add(newEntry);
                filteredList = new List<Lecture>(lectureEntryList);
                //empty lecInfo for next iteration
                lecInfo.Clear();
                //CreateLectureEntryTransform(newEntry, entryContainer, lectureEntryTransformList);
            }
            maxPages = lectureEntryList.Count % PAGECOUNT == 0 ? lectureEntryList.Count / PAGECOUNT : (int)(lectureEntryList.Count / PAGECOUNT) + 1;
            DisplayLectureList();
        }
    }

    public void DisplayLectureList()
    {
        for (int i = ((Int32.Parse(pgNum.text) - 1) * PAGECOUNT); i < Math.Min((Int32.Parse(pgNum.text)) * PAGECOUNT, filteredList.Count); i++)
        {
            if (filteredList[i] != null)
            {
                //UnityEngine.Debug.Log(i);
                Lecture lectureEntry = filteredList[i];
                CreateLectureEntryTransform(lectureEntry, entryContainer, lectureEntryTransformList);
            }
        }
    }

    private void CreateLectureEntryTransform(Lecture lectureEntry, Transform container, List<Transform> transformList)
    {
        float templateHeight = 130f;
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        int ind = transformList.Count + 1; //count for each entry starting at 1

        //entryTransform.Find("nameText").GetComponent<TMP_Text>().text = lectureEntry.name;
        entryTransform.Find("codeText").GetComponent<TMP_Text>().text = lectureEntry.code;
        entryTransform.Find("instrucText").GetComponent<TMP_Text>().text = lectureEntry.instructor;
        entryTransform.Find("locText").GetComponent<TMP_Text>().text = lectureEntry.location;
        //entryTransform.Find("timeText").GetComponent<TMP_Text>().text = lectureEntry.time;

        entryTransform.Find("entryBG").gameObject.SetActive(ind % 2 == 1);  //alternate bg

        transformList.Add(entryTransform);
    }

    public void clearing()
    {
        foreach (Transform entryTransform in lectureEntryTransformList)
        {
            Destroy(entryTransform.gameObject);
        }
        lectureEntryTransformList.Clear();  //even after destroying size isnt 0 so we have to clear
    }

    public void nextPage()
    {
        if (Int32.Parse(pgNum.text) == maxPages)
        {
            return;
        }
        pgNum.text = (Int32.Parse(pgNum.text) + 1).ToString();
    }

    public void prevPage()
    {
        if (Int32.Parse(pgNum.text) <= 1)
        {
            return;
        }
        pgNum.text = (Int32.Parse(pgNum.text) - 1).ToString();
    }

    public void lastPage()
    {
        pgNum.text = (maxPages).ToString();
    }

    public void firstPage()
    {
        pgNum.text = "1";
    }

    public void onFilter()
    {
        filteredList.Clear();
        switch (FilterDropdown.value)
        {
            case(0):
            // Code
                foreach (Lecture l in lectureEntryList)
                {
                    if(l.code.Contains(SearchString.text))
                    {
                        filteredList.Add(l);
                    }
                }
                break;
            case(1):
            // Instructor
                foreach (Lecture l in lectureEntryList)
                {
                    if(l.instructor.Contains(SearchString.text))
                    {
                        filteredList.Add(l);
                    }
                }
                break;
            case(2):
            // Location
                foreach (Lecture l in lectureEntryList)
                {
                    if(l.location.Contains(SearchString.text))
                    {
                        filteredList.Add(l);
                    }
                }
                break;
            default:
                filteredList = lectureEntryList;
                break;
        }
        firstPage();
        maxPages = filteredList.Count % PAGECOUNT == 0 ? filteredList.Count / PAGECOUNT : (int)(filteredList.Count / PAGECOUNT) + 1;;
        DisplayLectureList();
    }

    public void OnEntryClick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string code = template.transform.Find("codeText").GetComponent<TMP_Text>().text;
        Lecture target = lectureEntryList.Find(lecture => lecture.code == code);
        currentLecture = target;
    }
}


