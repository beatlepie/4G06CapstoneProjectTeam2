using System.Collections;
using System.Collections.Generic;
using System;
using Database;
using UnityEngine;
using Firebase.Database;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Auth;

public class LectureManager : MonoBehaviour
{
    [Header("List View")]
    public static string defaultSearchString;
    public static string defaultSearchOption;
    [SerializeField] TMP_Dropdown FilterDropdown;
    [SerializeField] public TMP_InputField SearchString;
    [SerializeField] GameObject NewLectureIcon;
    private Transform tabeltitleTemplate;
    private Transform entryContainer;
    private Transform entryTemplate;
    private Pagination<Lecture> lectures;
    private List<Transform> lectureEntryTransformList;
    public TMP_Text pgNum;
    public const int PAGECOUNT = 10;
    public GameObject BookmarkButton;

    [Header("Detail View")]
    public static List<string> myLectures;
    public static Lecture currentLecture; //The one we want to see details
    [Header("Detail View View")]
    [SerializeField] TMP_Text lecCodeView;

    [Header("Edit Page")]
    [SerializeField] TMP_InputField lecCodeEdit;
    [SerializeField] TMP_InputField lecNameEdit;
    [SerializeField] TMP_InputField lecInstructorEdit;
    [SerializeField] TMP_InputField lecLocationEdit;
    [SerializeField] TMP_InputField lecTimesEdit;
    
    private void Awake()
    {
        UnityEngine.Debug.Log("lecture manager script running");
        //after db stuff
        pgNum.text = "1";
        entryContainer = transform.Find("lectureEntryContainer");
        tabeltitleTemplate = entryContainer.Find("TableTitle");
        entryTemplate = entryContainer.Find("lectureEntryTemplate");
        lectureEntryTransformList = new List<Transform>();
        entryTemplate.gameObject.SetActive(false);
        BookmarkButton.SetActive(false);
        if(AuthConnector.Instance.Perms != PermissonLevel.Admin)
        {
            NewLectureIcon.SetActive(false);
            BookmarkButton.SetActive(true);
        }
        GetLectureData();
        GetPinnedData();
        if (defaultSearchOption != null & defaultSearchString != null)
        {
            SearchString.text = defaultSearchString;
            FilterDropdown.value = defaultSearchOption == "location" ? 2 : 0;
        }
    }

    public void GetLectureData()
    {
        StartCoroutine(GetLectures());
    }

    IEnumerator GetLectures()
    {
        //from merge conflict!
        lectureEntryTransformList = new List<Transform>();
        List<Lecture> lectureList = new List<Lecture>();
        var lectureData = DatabaseConnector.Instance.Root.Child("lectures").OrderByKey().StartAt("-").GetValueAsync();

        yield return new WaitUntil(predicate: () => lectureData.IsCompleted);
        if (lectureData != null)
        {
            DataSnapshot snapshot = lectureData.Result;
            foreach (var x in snapshot.Children)
            {
                lectureList.Add(Utilities.FormalizeDBLectureData(x));
            }
        }
        lectures = new Pagination<Lecture>(lectureList, defaultSearchOption, defaultSearchString, PAGECOUNT);
        DisplayLectureList();
    }

    public void GetPinnedData()
    {
        StartCoroutine(GetPinnedLectures((data) => myLectures = data));
    }

    IEnumerator GetPinnedLectures(Action<List<string>> onCallBack)
    {
        string emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);                
        var userData = DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/lectures").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if(userData != null)
        {
            List<string> pinnedLectures = new List<string>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                pinnedLectures.Add(x.Key.ToString());
            }
            onCallBack.Invoke(pinnedLectures);
        }
    }

    public void DisplayLectureList()
    {
        RectTransform titleRectTransform = tabeltitleTemplate.GetComponent<RectTransform>();
        titleRectTransform.sizeDelta = new Vector2((float)(Screen.width/1.2), titleRectTransform.sizeDelta.y);
        for (int i = ((lectures.currentPage - 1) * PAGECOUNT); i < Math.Min((lectures.currentPage) * PAGECOUNT, lectures.filteredList.Count); i++)
        {
            if (lectures.filteredList[i] != null)
            {
                Lecture lectureEntry = lectures.filteredList[i];
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
        lectures.nextPage();
        pgNum.text = lectures.currentPage.ToString();
    }

    public void prevPage()
    {
        lectures.prevPage();
        pgNum.text = lectures.currentPage.ToString();
    }

    public void lastPage()
    {
        lectures.lastPage();
        pgNum.text = lectures.currentPage.ToString();
    }

    public void firstPage()
    {
        lectures.firstPage();
        pgNum.text = lectures.currentPage.ToString();
    }

    public void onFilter()
    {
        switch (FilterDropdown.value)
        {
            case(0):
            // Code
                lectures.filterBy = "code";
                lectures.filterString = SearchString.text;
                var filteredLec = lectures.filterEntries();
                break;
            case(1):
            // Instructor
                lectures.filterBy = "instructor";
                lectures.filterString = SearchString.text;
                lectures.filterEntries();
                break;
            case(2):
            // Location
                lectures.filterBy = "location";
                lectures.filterString = SearchString.text;
                lectures.filterEntries();
                break;
            default:
                lectures.filterBy = null;
                lectures.filterString = null;
                lectures.filterEntries();
                break;
        }
        DisplayLectureList();
    }

    public void OnEntryClick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string code = template.transform.Find("codeText").GetComponent<TMP_Text>().text;
        Lecture target = lectures.entryList.Find(lecture => lecture.code == code);
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
        DatabaseConnector.Instance.Root.Child("lectures/" + lecCodeEdit.text).SetRawJsonValueAsync(lecJson);
        // Add new lecture to the rendered list, clear the filter and render the first page
        lectures.addNewEntry(lec);
        clearing();
        DisplayLectureList();
    }

    public void DeleteLec()
    {
        DatabaseConnector.Instance.Root.Child("lectures/" + lecCodeView.text).SetValueAsync(null);
        // Delete this lecture from the rendered list, clear the filter and render the first page
        var target = lectures.entryList.Find(lec => lec.code == lecCodeView.text);
        lectures.removeEntry(target);
        clearing();
        DisplayLectureList();
    }

    /// <summary>
    /// Function handling button press of bookmark button
    /// </summary>
    public void GoToBookmark()
    {
        SettingsManager.currentUser = true;
        SettingsManager.state = 1;
        SceneManager.LoadScene("SettingsScene");
    }
}