using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LectureManager : MonoBehaviour
{
    [Header("List View")]
    [SerializeField] TMP_Dropdown FilterDropdown;
    [SerializeField] TMP_InputField SearchString;
    private Transform tabeltitleTemplate;
    private Transform entryContainer;
    private Transform entryTemplate;
    private List<Lecture> lectureList;
    private List<Lecture> filteredList;
    private List<Transform> lectureEntryTransformList;
    public TMP_Text pgNum;
    public int maxPages;
    public const int PAGECOUNT = 10;

    [Header("Detail View")]
    public static Lecture currentLecture; //The one we want to see details
    [Header("Detail View View")]
    [SerializeField] TMP_Text lecCodeView;

    [Header("Edit Page")]
    [SerializeField] TMP_InputField lecCodeEdit;
    [SerializeField] TMP_InputField lecNameEdit;
    [SerializeField] TMP_InputField lecInstructorEdit;
    [SerializeField] TMP_InputField lecLocationEdit;
    [SerializeField] TMP_InputField lecTimesEdit;

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
        tabeltitleTemplate = entryContainer.Find("TableTitle");
        entryTemplate = entryContainer.Find("lectureEntryTemplate");
        entryTemplate.gameObject.SetActive(false);
        GetLectureData();
    }

    private void UpdateMaxPage()
    {
        if(filteredList.Count == 0)
        {
            maxPages = 1;
        }
        else
        {
            maxPages = filteredList.Count % PAGECOUNT == 0 ? filteredList.Count / PAGECOUNT : (int)(filteredList.Count / PAGECOUNT) + 1;
        }
    }
    public void GetLectureData()
    {
        StartCoroutine(GetLectures());
    }

    IEnumerator GetLectures()
    {
        lectureEntryTransformList = new List<Transform>();
        lectureList = new List<Lecture>();
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
                }
                result += "\n";
                // make a lectureEntry object with constructor  (lecInfo[0],lecInfo[1]...)
                Lecture newEntry = new Lecture(lecInfo[0], lecInfo[1], lecInfo[2], lecInfo[3], lecInfo[4]);
                lectureList.Add(newEntry);
                //empty lecInfo for next iteration
                lecInfo.Clear();
                //CreateLectureEntryTransform(newEntry, entryContainer, lectureEntryTransformList);
            }
            filteredList = new List<Lecture>(lectureList);
            UpdateMaxPage();
            DisplayLectureList();
        }
    }

    public void DisplayLectureList()
    {
        RectTransform titleRectTransform = tabeltitleTemplate.GetComponent<RectTransform>();
        titleRectTransform.sizeDelta = new Vector2((float)(Screen.width/1.2), titleRectTransform.sizeDelta.y);
        for (int i = ((Int32.Parse(pgNum.text) - 1) * PAGECOUNT); i < Math.Min((Int32.Parse(pgNum.text)) * PAGECOUNT, filteredList.Count); i++)
        {
            if (filteredList[i] != null)
            {
                Lecture lectureEntry = filteredList[i];
                CreateLectureEntryTransform(lectureEntry, entryContainer, lectureEntryTransformList);
            }
        }
    }

    private void CreateLectureEntryTransform(Lecture lectureEntry, Transform container, List<Transform> transformList)
    {
        // The arbitray number comes from marron header + filter + table title + footer height 
        float templateHeight = (Screen.height-690)/PAGECOUNT;
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryRectTransform.sizeDelta = new Vector2((float)(Screen.width/1.2), templateHeight);
        entryTransform.gameObject.SetActive(true);

        int ind = transformList.Count + 1; //count for each entry starting at 1

        //entryTransform.Find("nameText").GetComponent<TMP_Text>().text = lectureEntry.name;
        entryTransform.Find("codeText").GetComponent<TMP_Text>().text = lectureEntry.code;
        entryTransform.Find("instrucText").GetComponent<TMP_Text>().text = lectureEntry.instructor;
        entryTransform.Find("locText").GetComponent<TMP_Text>().text = lectureEntry.location;
        //entryTransform.Find("timeText").GetComponent<TMP_Text>().text = lectureEntry.time;

        //entryTransform.Find("entryBG").gameObject.SetActive(ind % 2 == 1);  //alternate bg
        entryTransform.Find("entryBG").gameObject.SetActive(true);  //always original bg
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
                foreach (Lecture l in lectureList)
                {
                    if(l.code.Contains(SearchString.text))
                    {
                        filteredList.Add(l);
                    }
                }
                break;
            case(1):
            // Instructor
                foreach (Lecture l in lectureList)
                {
                    if(l.instructor.Contains(SearchString.text))
                    {
                        filteredList.Add(l);
                    }
                }
                break;
            case(2):
            // Location
                foreach (Lecture l in lectureList)
                {
                    if(l.location.Contains(SearchString.text))
                    {
                        filteredList.Add(l);
                    }
                }
                break;
            default:
                filteredList = lectureList;
                break;
        }
        firstPage();
        UpdateMaxPage();
        DisplayLectureList();
    }

    public void OnEntryClick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string code = template.transform.Find("codeText").GetComponent<TMP_Text>().text;
        Lecture target = lectureList.Find(lecture => lecture.code == code);
        currentLecture = target;
    }

    public void ExitLecturePage()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void WriteNewLec()
    {
        Lecture lec = new Lecture(lecCodeEdit.text, lecInstructorEdit.text, lecLocationEdit.text, lecNameEdit.text, lecTimesEdit.text);
        string lecJson = JsonUtility.ToJson(lec);
        databaseReference.Child("lectures/" + lecCodeEdit.text).SetRawJsonValueAsync(lecJson);
        // Add new lecture to the rendered list, clear the filter and render the first page
        lectureList.Add(lec);
        filteredList = new List<Lecture>(lectureList);
        clearing();
        DisplayLectureList();
        UpdateMaxPage();
        firstPage();
    }

    public void DeleteLec()
    {
        databaseReference.Child("lectures/" + lecCodeView.text).SetValueAsync(null);
        // Delete this lecture from the rendered list, clear the filter and render the first page
        var target = lectureList.Find(lec => lec.code == lecCodeView.text);
        lectureList.Remove(target);
        filteredList = new List<Lecture>(lectureList);
        clearing();
        DisplayLectureList();
        UpdateMaxPage();
        firstPage();
    }
}