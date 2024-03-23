using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase.Database;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Auth;
using Database;

public class EventManager : MonoBehaviour
{
    [Header("List View")]
    public static string defaultSearchString;
    public static string defaultSearchOption;
    [SerializeField] TMP_Dropdown FilterDropdown;
    [SerializeField] TMP_InputField SearchString;
    private Transform tabeltitleTemplate;
    private Transform entryContainer;
    private Transform entryTemplate;
    private Pagination<Event> events;
    private List<Transform> eventEntryTransformList;
    public TMP_Text pgNum;
    public const int PAGECOUNT = 10;
    [SerializeField] Image EditButton;
    public GameObject BookmarkButton;

    [Header("Detail View")]
    public static List<string> myEvents;
    public static Event currentEvent; //The one we want to see details
    [Header("Detail View View")]
    [SerializeField] TMP_Text eventNameView;
    [SerializeField] Toggle eventIsPublicView;

    [Header("Edit Page")]
    [SerializeField] TMP_InputField eventNameEdit;
    [SerializeField] TMP_InputField eventDescriptionEdit;
    [SerializeField] TMP_InputField eventOrganizerEdit;
    [SerializeField] TMP_InputField eventLocationEdit;
    [SerializeField] TMP_InputField eventTimeEdit;
    [SerializeField] TMP_InputField eventDurationEdit;
    [SerializeField] Toggle eventIsPublicEdit;
    private void Awake()
    {
        UnityEngine.Debug.Log("lecture manager script running");
        //after db stuff
        pgNum.text = "1";
        entryContainer = transform.Find("eventEntryContainer");
        tabeltitleTemplate = entryContainer.Find("TableTitle");
        entryTemplate = entryContainer.Find("eventEntryTemplate");

        entryTemplate.gameObject.SetActive(false);
        BookmarkButton.SetActive(false);
        //This may cause issues! leaving this from merge conflict!
        eventEntryTransformList = new List<Transform>();

        // If they are not admin, do not show edit button!
        if (AuthConnector.Instance.Perms != PermissonLevel.Admin)
        {
            EditButton.gameObject.SetActive(false);
            BookmarkButton.SetActive(true);
        }        
        GetEventData();
        GetPinnedData();
        if (defaultSearchOption != null & defaultSearchString != null)
        {
            SearchString.text = defaultSearchString;
            FilterDropdown.value = defaultSearchOption == "location" ? 2 : 0;
        }
    }

    public void GetEventData()
    {
        StartCoroutine(GetEvents());
    }

    IEnumerator GetEvents()
    {
        eventEntryTransformList = new List<Transform>();
        var publicEventData = DatabaseConnector.Instance.Root.Child("events/public").OrderByKey().StartAt("-").GetValueAsync();
        List<Event> eventList = new List<Event>();
        yield return new WaitUntil(predicate: () => publicEventData.IsCompleted);
        if (publicEventData != null)
        {
            DataSnapshot snapshot = publicEventData.Result;
            foreach (var e in snapshot.Children)
            {
                eventList.Add(Utilities.FormalizeDBEventData(e));    
            }
        }
        if(AuthConnector.Instance.Perms != PermissonLevel.Guest)
        {
            var privateEventData = DatabaseConnector.Instance.Root.Child("events/private").OrderByKey().StartAt("-").GetValueAsync();
            yield return new WaitUntil(predicate: () => privateEventData.IsCompleted);
            if (privateEventData != null)
            {
                DataSnapshot snapshot = privateEventData.Result;
                foreach (var e in snapshot.Children)
                {
                    eventList.Add(Utilities.FormalizeDBEventData(e));
                }
            }
        }
        events = new Pagination<Event>(eventList, defaultSearchOption, defaultSearchString, PAGECOUNT);
        DisplayEventList();
    }

    public void GetPinnedData()
    {
        StartCoroutine(GetPinnedEvents((data) => myEvents = data));
    }

    IEnumerator GetPinnedEvents(Action<List<string>> onCallBack)
    {
        string emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);                
        var userData = DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/events").GetValueAsync();
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

    public void DisplayEventList()
    {
        RectTransform titleRectTransform = tabeltitleTemplate.GetComponent<RectTransform>();
        titleRectTransform.sizeDelta = new Vector2((float)(Screen.width/1.2), titleRectTransform.sizeDelta.y);
        for (int i = ((events.currentPage - 1) * PAGECOUNT); i < Math.Min((events.currentPage) * PAGECOUNT, events.filteredList.Count); i++)
        {
            if (events.filteredList[i] != null)
            {
                Event eventEntry = events.filteredList[i];
                CreateEventEntryTransform(eventEntry, entryContainer, eventEntryTransformList);
            }
        }
    }

    private void CreateEventEntryTransform(Event eventEntry, Transform container, List<Transform> transformList)
    {
        // The arbitray number comes from marron header + filter + table title + footer height 
        float templateHeight = (Screen.height-690)/PAGECOUNT;
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryRectTransform.sizeDelta = new Vector2((float)(Screen.width/1.2), templateHeight);
        entryTransform.gameObject.SetActive(true);

        //entryTransform.Find("nameText").GetComponent<TMP_Text>().text = lectureEntry.name;
        entryTransform.Find("nameText").GetComponent<TMP_Text>().text = eventEntry.name;
        entryTransform.Find("organizerText").GetComponent<TMP_Text>().text = eventEntry.organizer;
        entryTransform.Find("locText").GetComponent<TMP_Text>().text = eventEntry.location;
        entryTransform.Find("entryBG").gameObject.SetActive(true);  //always original bg
        transformList.Add(entryTransform);
    }

    public void clearing()
    {
        foreach (Transform entryTransform in eventEntryTransformList)
        {
            Destroy(entryTransform.gameObject);
        }
        eventEntryTransformList.Clear();  //even after destroying size isnt 0 so we have to clear
    }

    public void nextPage()
    {
        events.nextPage();
        pgNum.text = events.currentPage.ToString();
    }

    public void prevPage()
    {
        events.prevPage();
        pgNum.text = events.currentPage.ToString();
    }

    public void lastPage()
    {
        events.lastPage();
        pgNum.text = events.currentPage.ToString();
    }

    public void firstPage()
    {
        events.firstPage();
        pgNum.text = events.currentPage.ToString();
    }

    public void onFilter()
    {
        switch (FilterDropdown.value)
        {
            case(0):
            // Name
                events.filterBy = "name";
                events.filterString = SearchString.text;
                events.filterEntries();
                break;
            case(1):
            // Organizer
               events.filterBy = "organizer";
                events.filterString = SearchString.text;
                events.filterEntries();
                break;
            case(2):
            // Location
                events.filterBy = "location";
                events.filterString = SearchString.text;
                events.filterEntries();
                break;
            default:
                events.filterBy = null;
                events.filterString = null;
                events.filterEntries();
                break;
        }
        DisplayEventList();
    }

    public void OnEntryClick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string name = template.transform.Find("nameText").GetComponent<TMP_Text>().text;
        Event target = events.entryList.Find(e => e.name == name);
        currentEvent = target;
    }

    public void ExitEventPage()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void WriteNewEvent()
    {
        DateTime startTime = DateTime.Parse(eventTimeEdit.text).ToLocalTime();
        DateTimeOffset dto = new DateTimeOffset(startTime);
        Event e = new Event(eventNameEdit.text, dto.ToUnixTimeSeconds(), int.Parse(eventDurationEdit.text), eventOrganizerEdit.text, eventDescriptionEdit.text, eventLocationEdit.text, eventIsPublicEdit.isOn);
        string eventJson = JsonUtility.ToJson(e);
        string prefix = eventIsPublicEdit.isOn ? "events/public/" : "events/private/";
        DatabaseConnector.Instance.Root.Child(prefix + eventNameEdit.text).SetRawJsonValueAsync(eventJson);
        // Add new event to the rendered list, clear the filter and render the first page
        events.addNewEntry(e);
        clearing();
        DisplayEventList();
    }

    public void DeleteEvent()
    {
        string prefix = eventIsPublicView.isOn ? "events/public/" : "events/private/";
        DatabaseConnector.Instance.Root.Child(prefix + eventNameView.text).SetValueAsync(null);
        // Delete this lecture from the rendered list, clear the filter and render the first page
        var target = events.entryList.Find(e => e.name == eventNameView.text);
        events.removeEntry(target);
        clearing();
        DisplayEventList();
    }

    /// <summary>
    /// When the bookmark is clicked, it should redirect to bookmarkedEvents page
    /// </summary>
    public void GoToBookmark()
    {
        SettingsManager.currentUser = true;
        SettingsManager.state = 2;
        SceneManager.LoadScene("SettingsScene");
    }
}