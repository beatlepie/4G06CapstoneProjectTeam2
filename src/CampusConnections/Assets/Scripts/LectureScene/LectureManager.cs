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
using UnityEngine.Serialization;

public class LectureManager : MonoBehaviour
{
    [Header("List View")] public static string DefaultSearchString;
    public static string DefaultSearchOption;
    [FormerlySerializedAs("FilterDropdown")] [SerializeField] private TMP_Dropdown filterDropdown;
    [FormerlySerializedAs("SearchString")] [SerializeField] public TMP_InputField searchString;
    [FormerlySerializedAs("NewLectureIcon")] [SerializeField] private GameObject newLectureIcon;
    private Transform _tableTitleTemplate;
    private Transform _entryContainer;
    private Transform _entryTemplate;
    private Pagination<Lecture> _lectures;
    private List<Transform> _lectureEntryTransformList;
    public TMP_Text pgNum;
    private const int PageCount = 10;
    [FormerlySerializedAs("BookmarkButton")] public GameObject bookmarkButton;

    [Header("Detail View")] public static List<string> MyLectures;
    public static Lecture CurrentLecture; //The one we want to see details

    [Header("Detail View View")] [SerializeField]
    private TMP_Text lecCodeView;

    [Header("Edit Page")] [SerializeField] private TMP_InputField lecCodeEdit;
    [SerializeField] private TMP_InputField lecNameEdit;
    [SerializeField] private TMP_InputField lecInstructorEdit;
    [SerializeField] private TMP_InputField lecLocationEdit;
    [SerializeField] private TMP_InputField lecTimesEdit;

    private void Awake()
    {
        Debug.Log("lecture manager script running");
        //after db stuff
        pgNum.text = "1";
        _entryContainer = transform.Find("lectureEntryContainer");
        _tableTitleTemplate = _entryContainer.Find("TableTitle");
        _entryTemplate = _entryContainer.Find("lectureEntryTemplate");
        _lectureEntryTransformList = new List<Transform>();
        _entryTemplate.gameObject.SetActive(false);
        bookmarkButton.SetActive(false);
        if (AuthConnector.Instance.Perms != PermissionLevel.Admin)
        {
            newLectureIcon.SetActive(false);
            bookmarkButton.SetActive(true);
        }

        GetLectureData();
        GetPinnedData();
        if ((DefaultSearchOption != null) & (DefaultSearchString != null))
        {
            searchString.text = DefaultSearchString;
            filterDropdown.value = DefaultSearchOption == "location" ? 2 : 0;
        }
    }

    public void GetLectureData()
    {
        StartCoroutine(GetLectures());
    }

    private IEnumerator GetLectures()
    {
        //from merge conflict!
        _lectureEntryTransformList = new List<Transform>();
        var lectureList = new List<Lecture>();
        var lectureData = DatabaseConnector.Instance.Root.Child("lectures").OrderByKey().StartAt("-").GetValueAsync();

        yield return new WaitUntil(() => lectureData.IsCompleted);
        if (lectureData != null)
        {
            var snapshot = lectureData.Result;
            foreach (var x in snapshot.Children) lectureList.Add(Utilities.FormalizeDBLectureData(x));
        }

        _lectures = new Pagination<Lecture>(lectureList, DefaultSearchOption, DefaultSearchString);
        DisplayLectureList();
    }

    private void GetPinnedData()
    {
        StartCoroutine(GetPinnedLectures((data) => MyLectures = data));
    }

    private IEnumerator GetPinnedLectures(Action<List<string>> onCallBack)
    {
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        var userData = DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/lectures").GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);
        if (userData != null)
        {
            var pinnedLectures = new List<string>();
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children) pinnedLectures.Add(x.Key);
            onCallBack.Invoke(pinnedLectures);
        }
    }

    public void DisplayLectureList()
    {
        var titleRectTransform = _tableTitleTemplate.GetComponent<RectTransform>();
        titleRectTransform.sizeDelta = new Vector2((float)(Screen.width / 1.2), titleRectTransform.sizeDelta.y);
        for (var i = (_lectures.CurrentPage - 1) * PageCount;
             i < Math.Min(_lectures.CurrentPage * PageCount, _lectures.FilteredList.Count);
             i++)
            if (_lectures.FilteredList[i] != null)
            {
                var lectureEntry = _lectures.FilteredList[i];
                CreateLectureEntryTransform(lectureEntry, _entryContainer, _lectureEntryTransformList);
            }
    }

    private void CreateLectureEntryTransform(Lecture lectureEntry, Transform container, List<Transform> transformList)
    {
        // The arbitrary number comes from marron header + filter + table title + footer height 
        float templateHeight = (Screen.height - 690) / (float) PageCount;
        var entryTransform = Instantiate(_entryTemplate, container);
        var entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryRectTransform.sizeDelta = new Vector2((float)(Screen.width / 1.2), templateHeight);
        entryTransform.gameObject.SetActive(true);

        //entryTransform.Find("nameText").GetComponent<TMP_Text>().text = lectureEntry.name;
        entryTransform.Find("codeText").GetComponent<TMP_Text>().text = lectureEntry.Code;
        entryTransform.Find("instrucText").GetComponent<TMP_Text>().text = lectureEntry.Instructor;
        entryTransform.Find("locText").GetComponent<TMP_Text>().text = lectureEntry.Location;
        //entryTransform.Find("timeText").GetComponent<TMP_Text>().text = lectureEntry.time;

        //entryTransform.Find("entryBG").gameObject.SetActive(ind % 2 == 1);  //alternate bg
        entryTransform.Find("entryBG").gameObject.SetActive(true); //always original bg
        transformList.Add(entryTransform);
    }

    public void Clearing()
    {
        foreach (var entryTransform in _lectureEntryTransformList) Destroy(entryTransform.gameObject);
        _lectureEntryTransformList.Clear(); //even after destroying size isn't 0 so we have to clear
    }

    public void NextPage()
    {
        _lectures.NextPage();
        pgNum.text = _lectures.CurrentPage.ToString();
    }

    public void PrevPage()
    {
        _lectures.PrevPage();
        pgNum.text = _lectures.CurrentPage.ToString();
    }

    public void LastPage()
    {
        _lectures.LastPage();
        pgNum.text = _lectures.CurrentPage.ToString();
    }

    public void FirstPage()
    {
        _lectures.FirstPage();
        pgNum.text = _lectures.CurrentPage.ToString();
    }

    public void OnFilter()
    {
        switch (filterDropdown.value)
        {
            case 0:
                // Code
                _lectures.FilterBy = "code";
                _lectures.FilterString = searchString.text;
                var filteredLec = _lectures.FilterEntries();
                break;
            case 1:
                // Instructor
                _lectures.FilterBy = "instructor";
                _lectures.FilterString = searchString.text;
                _lectures.FilterEntries();
                break;
            case 2:
                // Location
                _lectures.FilterBy = "location";
                _lectures.FilterString = searchString.text;
                _lectures.FilterEntries();
                break;
            default:
                _lectures.FilterBy = null;
                _lectures.FilterString = null;
                _lectures.FilterEntries();
                break;
        }

        DisplayLectureList();
    }

    public void OnEntryClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var code = template.transform.Find("codeText").GetComponent<TMP_Text>().text;
        var target = _lectures.EntryList.Find(lecture => lecture.Code == code);
        CurrentLecture = target;
    }

    public void ExitLecturePage()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void WriteNewLec()
    {
        var lec = new Lecture(lecCodeEdit.text, lecInstructorEdit.text, lecLocationEdit.text, lecNameEdit.text,
            lecTimesEdit.text);
        var lecJson = JsonUtility.ToJson(lec);
        DatabaseConnector.Instance.Root.Child("lectures/" + lecCodeEdit.text).SetRawJsonValueAsync(lecJson);
        // Add new lecture to the rendered list, clear the filter and render the first page
        _lectures.AddNewEntry(lec);
        Clearing();
        DisplayLectureList();
    }

    public void DeleteLec()
    {
        DatabaseConnector.Instance.Root.Child("lectures/" + lecCodeView.text).SetValueAsync(null);
        // Delete this lecture from the rendered list, clear the filter and render the first page
        var target = _lectures.EntryList.Find(lec => lec.Code == lecCodeView.text);
        _lectures.RemoveEntry(target);
        Clearing();
        DisplayLectureList();
    }

    /// <summary>
    /// Function handling button press of bookmark button
    /// </summary>
    public void GoToBookmark()
    {
        SettingsManager.CurrentUser = true;
        SettingsManager.State = 1;
        SceneManager.LoadScene("SettingsScene");
    }
}