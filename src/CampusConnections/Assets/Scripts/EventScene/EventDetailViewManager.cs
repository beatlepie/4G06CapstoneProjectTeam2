using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventDetailViewManager : MonoBehaviour
{
    [Header("DetailView")] [SerializeField]
    private GameObject ViewPanel;

    [SerializeField] private GameObject EditPanel;
    private Event target;
    private bool pinned;
    private List<string> myEventNames;
    [SerializeField] private GameObject PinIcon;
    [SerializeField] private GameObject UnpinIcon;
    [SerializeField] private GameObject DeleteIcon;
    [SerializeField] private GameObject EditIcon;

    [Header("ViewPanel")] [SerializeField] private TMP_Text viewName;
    [SerializeField] private TMP_Text viewDescription;
    [SerializeField] private TMP_Text viewOrganizer;
    [SerializeField] private TMP_Text viewLocation;
    [SerializeField] private TMP_Text viewTime;
    [SerializeField] private TMP_Text viewDuration;
    [SerializeField] private Toggle viewIsPublic;

    [Header("EditPanel")] [SerializeField] private TMP_InputField editName;
    [SerializeField] private TMP_InputField editDesciprtion;
    [SerializeField] private TMP_InputField editOrganizer;
    [SerializeField] private TMP_InputField editLocation;
    [SerializeField] private TMP_InputField editTime;
    [SerializeField] private TMP_InputField editDuration;
    [SerializeField] private Toggle editIsPublic;

    private void Awake()
    {
        if (AuthConnector.Instance.Perms != PermissonLevel.Admin)
        {
            DeleteIcon.SetActive(false);
            EditIcon.SetActive(false);
        }

        myEventNames = EventManager.myEvents;
    }

    private void OnEnable()
    {
        target = EventManager.currentEvent;
        pinned = myEventNames.Contains(target.name);
        PinIcon.SetActive(!pinned);
        UnpinIcon.SetActive(pinned);
        UpdateView();
    }

    private void UpdateView()
    {
        viewName.text = target.name;
        viewDescription.text = target.description;
        viewOrganizer.text = target.organizer;
        viewLocation.text = target.location;
        viewTime.text = DateTimeOffset.FromUnixTimeSeconds(target.time).ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        viewDuration.text = target.duration.ToString();
        viewIsPublic.isOn = target.isPublic;
        editName.text = target.name;
        editDesciprtion.text = target.description;
        editOrganizer.text = target.organizer;
        editLocation.text = target.location;
        editTime.text = DateTimeOffset.FromUnixTimeSeconds(target.time).ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        editDuration.text = target.duration.ToString();
        editIsPublic.isOn = target.isPublic;
    }

    public void SaveChanges()
    {
        target.name = editName.text;
        target.description = editDesciprtion.text;
        target.organizer = editOrganizer.text;
        target.location = editLocation.text;
        var dto = new DateTimeOffset(DateTime.Parse(editTime.text).ToUniversalTime());
        target.time = dto.ToUnixTimeSeconds();
        target.duration = int.Parse(editDuration.text);
        target.isPublic = editIsPublic.isOn;
        UpdateView();
        var targetJson = JsonUtility.ToJson(target);
        var prefix = target.isPublic ? "events/public/" : "events/private/";
        DatabaseConnector.Instance.Root.Child(prefix + target.name).SetRawJsonValueAsync(targetJson);
    }

    public void Pin()
    {
        myEventNames.Add(target.name);
        PinIcon.SetActive(false);
        UnpinIcon.SetActive(true);
        var emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/events/" + target.name)
            .SetValueAsync("True");
    }

    public void Unpin()
    {
        myEventNames.Remove(target.name);
        PinIcon.SetActive(true);
        UnpinIcon.SetActive(false);
        var emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/events/" + target.name)
            .SetValueAsync(null);
    }

    public void DetailViewClose()
    {
        ViewPanel.SetActive(true);
        EditPanel.SetActive(false);
    }
}