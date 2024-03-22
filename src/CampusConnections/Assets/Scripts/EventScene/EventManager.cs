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
    private List<Event> eventList;
    private List<Event> filteredList;
    private List<Transform> eventEntryTransformList;
    public TMP_Text pgNum;
    public int maxPages;
    public const int PAGECOUNT = 10;
    [SerializeField] Image EditButton;
    public GameObject BookmarkButton;

    [Header("Detail View")]
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
        // If they are not admin, do not show edit button!
        if (AuthConnector.Instance.Perms != PermissonLevel.Admin)
        {
            EditButton.gameObject.SetActive(false);
            BookmarkButton.SetActive(true);
        }        
        GetEventData();
        if (defaultSearchOption != null & defaultSearchString != null)
        {
            SearchString.text = defaultSearchString;
            FilterDropdown.value = defaultSearchOption == "location" ? 2 : 0;
        }
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
    public void GetEventData()
    {
        StartCoroutine(GetEvents());
    }

    IEnumerator GetEvents()
    {
        eventEntryTransformList = new List<Transform>();
        eventList = new List<Event>();
        var publicEventData = DatabaseConnector.Instance.Root.Child("events/public").OrderByKey().StartAt("-").GetValueAsync();
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
        if (defaultSearchOption != null & defaultSearchString != null)
        {
            filteredList = new List<Event>();
            if (defaultSearchOption == "location")
            {
                foreach (Event e in eventList) {
                    if (e.location.Contains(defaultSearchString)) {
                        filteredList.Add(e);
                    }
                }
            }
            else
            {
                foreach (Event e in eventList) {
                    if (e.name.Contains(defaultSearchString)) {
                        filteredList.Add(e);
                    }
                }    
            }
        }
        else
        {
            filteredList = new List<Event>(eventList);
        }
        UpdateMaxPage();
        DisplayEventList();
    }

    public void DisplayEventList()
    {
        RectTransform titleRectTransform = tabeltitleTemplate.GetComponent<RectTransform>();
        titleRectTransform.sizeDelta = new Vector2((float)(Screen.width/1.2), titleRectTransform.sizeDelta.y);
        for (int i = ((Int32.Parse(pgNum.text) - 1) * PAGECOUNT); i < Math.Min((Int32.Parse(pgNum.text)) * PAGECOUNT, filteredList.Count); i++)
        {
            if (filteredList[i] != null)
            {
                Event eventEntry = filteredList[i];
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
            // Name
                foreach (Event e in eventList)
                {
                    if(e.name.Contains(SearchString.text))
                    {
                        filteredList.Add(e);
                    }
                }
                break;
            case(1):
            // Organizer
                foreach (Event e in eventList)
                {
                    if(e.organizer.Contains(SearchString.text))
                    {
                        filteredList.Add(e);
                    }
                }
                break;
            case(2):
            // Location
                foreach (Event e in eventList)
                {
                    if(e.location.Contains(SearchString.text))
                    {
                        filteredList.Add(e);
                    }
                }
                break;
            default:
                filteredList = eventList;
                break;
        }
        firstPage();
        UpdateMaxPage();
        DisplayEventList();
    }

    public void OnEntryClick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string name = template.transform.Find("nameText").GetComponent<TMP_Text>().text;
        Event target = eventList.Find(e => e.name == name);
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
        eventList.Add(e);
        filteredList = new List<Event>(eventList);
        clearing();
        DisplayEventList();
        UpdateMaxPage();
        firstPage();
    }

    public void DeleteEvent()
    {
        string prefix = eventIsPublicView.isOn ? "events/public/" : "events/private/";
        DatabaseConnector.Instance.Root.Child(prefix + eventNameView.text).SetValueAsync(null);
        // Delete this lecture from the rendered list, clear the filter and render the first page
        var target = eventList.Find(e => e.name == eventNameView.text);
        eventList.Remove(target);
        filteredList = new List<Event>(eventList);
        clearing();
        DisplayEventList();
        UpdateMaxPage();
        firstPage();
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