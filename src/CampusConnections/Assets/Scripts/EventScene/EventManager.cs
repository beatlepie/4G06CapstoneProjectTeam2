using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Auth;
using Database;
using UnityEngine.Serialization;

/// <summary>
/// This class controls the event list view, including pagination view and search filter.
/// Author: Zihao Du
/// Date: 2024-02-20
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("List View")] public static string DefaultSearchString;
    public static string DefaultSearchOption;
    [FormerlySerializedAs("FilterDropdown")] [SerializeField] private TMP_Dropdown filterDropdown;
    [FormerlySerializedAs("SearchString")] [SerializeField] private TMP_InputField searchString;
    private Transform _tableTitleTemplate;
    private Transform _entryContainer;
    private Transform _entryTemplate;
    private Pagination<Event> _events;
    private List<Transform> _eventEntryTransformList;
    public TMP_Text pgNum;
    // Following numbers come from Figma design
    private const int PageCount = 10;
    private const int HeaderHeight = 690;
    [FormerlySerializedAs("EditButton")] [SerializeField] private Image editButton;
    [FormerlySerializedAs("BookmarkButton")] public GameObject bookmarkButton;

    [Header("Detail View")] public static List<string> MyEvents;
    public static Event CurrentEvent; //The one we want to see details

    [Header("Detail View View")] [SerializeField]
    private TMP_Text eventNameView;

    [SerializeField] private Toggle eventIsPublicView;

    [Header("Edit Page")] [SerializeField] private TMP_InputField eventNameEdit;
    [SerializeField] private TMP_InputField eventDescriptionEdit;
    [SerializeField] private TMP_InputField eventOrganizerEdit;
    [SerializeField] private TMP_InputField eventLocationEdit;
    [SerializeField] private TMP_InputField eventTimeEdit;
    [SerializeField] private TMP_InputField eventDurationEdit;
    [SerializeField] private Toggle eventIsPublicEdit;

    private void Awake()
    {
        // Init
        pgNum.text = "1";
        _entryContainer = transform.Find("eventEntryContainer");
        _tableTitleTemplate = _entryContainer.Find("TableTitle");
        _entryTemplate = _entryContainer.Find("eventEntryTemplate");

        _entryTemplate.gameObject.SetActive(false);
        bookmarkButton.SetActive(false);
        _eventEntryTransformList = new List<Transform>();

        // If they are not admin, do not show edit button
        if (AuthConnector.Instance.Perms != PermissionLevel.Admin)
        {
            editButton.gameObject.SetActive(false);
            bookmarkButton.SetActive(true);
        }

        GetEventData();
        GetBookmarkedData();
        if ((DefaultSearchOption != null) & (DefaultSearchString != null))
        {
            searchString.text = DefaultSearchString;
            // Filter by name, if not specified search option
            // There is a lookup table for dropdown table in LectureScene.unity, 2 - location, 1 - organizer, 0 - name
            filterDropdown.value = DefaultSearchOption == "location" ? 2 : 0;
        }
    }

    /// <summary>
    /// Get and store a list of all events from database
    /// </summary>
    public void GetEventData()
    {
        StartCoroutine(GetEvents());
    }

    /// <summary>
    /// Async call to retrieve event data from db
    /// </summary>
    private IEnumerator GetEvents()
    {
        _eventEntryTransformList = new List<Transform>();
        var publicEventData = DatabaseConnector.Instance.Root.Child("events/public").OrderByKey().StartAt("-")
            .GetValueAsync();
        var eventList = new List<Event>();
        yield return new WaitUntil(() => publicEventData.IsCompleted);
        if (publicEventData != null)
        {
            var snapshot = publicEventData.Result;
            foreach (var e in snapshot.Children) eventList.Add(Utilities.FormalizeDBEventData(e));
        }
        // Privacy Requirement: Private events need user or admin permission
        if (AuthConnector.Instance.Perms != PermissionLevel.Guest)
        {
            var privateEventData = DatabaseConnector.Instance.Root.Child("events/private").OrderByKey().StartAt("-")
                .GetValueAsync();
            yield return new WaitUntil(() => privateEventData.IsCompleted);
            if (privateEventData != null)
            {
                var snapshot = privateEventData.Result;
                foreach (var e in snapshot.Children) eventList.Add(Utilities.FormalizeDBEventData(e));
            }
        }

        _events = new Pagination<Event>(eventList, DefaultSearchOption, DefaultSearchString);
        DisplayEventList();
    }

    /// <summary>
    /// Retrieve bookmarked event data and store it to the state
    /// </summary>
    private void GetBookmarkedData()
    {
        StartCoroutine(GetBookmarkedEvents((data) => MyEvents = data));
    }

    /// <summary>
    /// Get and store a list of bookmarked events from database
    /// </summary>
    private IEnumerator GetBookmarkedEvents(Action<List<string>> onCallBack)
    {
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        var userData = DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/events").GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);
        if (userData != null)
        {
            var bookmarkedOnes = new List<string>();
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children) bookmarkedOnes.Add(x.Key);
            onCallBack.Invoke(bookmarkedOnes);
        }
    }

    /// <summary>
    /// Render a list of events, indices from (current page - 1) * PageCount to (current page) * PageCount
    /// </summary>
    public void DisplayEventList()
    {
        var titleRectTransform = _tableTitleTemplate.GetComponent<RectTransform>();
        // 1.2 comes from Figma Design, the entries take 5/6 of the screen width
        titleRectTransform.sizeDelta = new Vector2((float)(Screen.width / 1.2), titleRectTransform.sizeDelta.y);
        for (var i = (_events.CurrentPage - 1) * PageCount;
             i < Math.Min(_events.CurrentPage * PageCount, _events.FilteredList.Count);
             i++)
            if (_events.FilteredList[i] != null)
            {
                var eventEntry = _events.FilteredList[i];
                CreateEventEntryTransform(eventEntry, _entryContainer, _eventEntryTransformList);
            }
    }

    /// <summary>
    /// Create an event entry in the list view given a template and its corresponding event information
    /// </summary>
    private void CreateEventEntryTransform(Event eventEntry, Transform container, List<Transform> transformList)
    { 
        var templateHeight = (Screen.height - HeaderHeight) / (float) PageCount;
        var entryTransform = Instantiate(_entryTemplate, container);
        var entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        // 1.2 comes from Figma Design, the entries take 5/6 of the screen width
        entryRectTransform.sizeDelta = new Vector2((float)(Screen.width / 1.2), templateHeight);
        entryTransform.gameObject.SetActive(true);

        entryTransform.Find("nameText").GetComponent<TMP_Text>().text = eventEntry.Name;
        entryTransform.Find("organizerText").GetComponent<TMP_Text>().text = eventEntry.Organizer;
        entryTransform.Find("locText").GetComponent<TMP_Text>().text = eventEntry.Location;
        entryTransform.Find("entryBG").gameObject.SetActive(true);
        transformList.Add(entryTransform);
    }

    /// <summary>
    /// Destroy the rendered lists
    /// </summary>
    public void Clearing()
    {
        foreach (var entryTransform in _eventEntryTransformList) Destroy(entryTransform.gameObject);
        _eventEntryTransformList.Clear();
    }

    public void NextPage()
    {
        _events.NextPage();
        pgNum.text = _events.CurrentPage.ToString();
    }

    public void PrevPage()
    {
        _events.PrevPage();
        pgNum.text = _events.CurrentPage.ToString();
    }

    public void LastPage()
    {
        _events.LastPage();
        pgNum.text = _events.CurrentPage.ToString();
    }

    public void FirstPage()
    {
        _events.FirstPage();
        pgNum.text = _events.CurrentPage.ToString();
    }

    /// <summary>
    /// Apply values from dropdown to event list
    /// Use filter method from Pagniation class
    /// </summary>
    public void OnFilter()
    {
        switch (filterDropdown.value)
        {
            case 0:
                // Name
                _events.FilterBy = "name";
                _events.FilterString = searchString.text;
                _events.FilterEntries();
                break;
            case 1:
                // Organizer
                _events.FilterBy = "organizer";
                _events.FilterString = searchString.text;
                _events.FilterEntries();
                break;
            case 2:
                // Location
                _events.FilterBy = "location";
                _events.FilterString = searchString.text;
                _events.FilterEntries();
                break;
            default:
                _events.FilterBy = null;
                _events.FilterString = null;
                _events.FilterEntries();
                break;
        }

        DisplayEventList();
    }

    public void OnEntryClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var text = template.transform.Find("nameText").GetComponent<TMP_Text>().text;
        var target = _events.EntryList.Find(e => e.Name == text);
        CurrentEvent = target;
    }

    public void ExitEventPage()
    {
        SceneManager.LoadScene("MenuScene");
    }

    /// <summary>
    /// Add a new event with input information
    /// </summary>
    public void WriteNewEvent()
    {
        var startTime = DateTime.Parse(eventTimeEdit.text).ToLocalTime();
        var dto = new DateTimeOffset(startTime);
        var e = new Event(eventNameEdit.text, dto.ToUnixTimeSeconds(), int.Parse(eventDurationEdit.text),
            eventOrganizerEdit.text, eventDescriptionEdit.text, eventLocationEdit.text, eventIsPublicEdit.isOn);
        var eventJson = JsonUtility.ToJson(e);
        var prefix = eventIsPublicEdit.isOn ? "events/public/" : "events/private/";
        DatabaseConnector.Instance.Root.Child(prefix + eventNameEdit.text).SetRawJsonValueAsync(eventJson);
        // Add new event to the rendered list, clear the filter and render the first page
        _events.AddNewEntry(e);
        Clearing();
        DisplayEventList();
    }

    /// <summary>
    /// Delete target event
    /// </summary>
    public void DeleteEvent()
    {
        var prefix = eventIsPublicView.isOn ? "events/public/" : "events/private/";
        DatabaseConnector.Instance.Root.Child(prefix + eventNameView.text).SetValueAsync(null);
        // Delete this lecture from the rendered list, clear the filter and render the first page
        var target = _events.EntryList.Find(e => e.Name == eventNameView.text);
        _events.RemoveEntry(target);
        Clearing();
        DisplayEventList();
    }

    /// <summary>
    /// When the bookmark is clicked, it should redirect to bookmarkedEvents page
    /// </summary>
    public void GoToBookmark()
    {
        SettingsManager.CurrentUser = true;
        SettingsManager.State = 2;
        SceneManager.LoadScene("SettingsScene");
    }
}