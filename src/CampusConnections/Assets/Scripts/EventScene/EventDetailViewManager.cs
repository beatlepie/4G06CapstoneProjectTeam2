using System;
using System.Collections.Generic;
using Database;
using Auth;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// This class controls the event detail view (the pop up window), including read and edit views and bookmark/unbookmark methods.
/// Author: Zihao Du
/// Date: 2024-02-20
/// </summary>
public class EventDetailViewManager : MonoBehaviour
{
    [FormerlySerializedAs("ViewPanel")] [Header("DetailView")] [SerializeField]
    private GameObject viewPanel;

    [FormerlySerializedAs("EditPanel")] [SerializeField] private GameObject editPanel;
    private Event _target;
    private bool _bookmarked;
    private List<string> _myEventNames;
    [SerializeField] private GameObject bookmarkIcon;
    [SerializeField] private GameObject unbookmarkIcon;
    [FormerlySerializedAs("DeleteIcon")] [SerializeField] private GameObject deleteIcon;
    [FormerlySerializedAs("EditIcon")] [SerializeField] private GameObject editIcon;

    [Header("ViewPanel")] [SerializeField] private TMP_Text viewName;
    [SerializeField] private TMP_Text viewDescription;
    [SerializeField] private TMP_Text viewOrganizer;
    [SerializeField] private TMP_Text viewLocation;
    [SerializeField] private TMP_Text viewTime;
    [SerializeField] private TMP_Text viewDuration;
    [SerializeField] private Toggle viewIsPublic;

    [Header("EditPanel")] [SerializeField] private TMP_InputField editName;
    [FormerlySerializedAs("editDesciprtion")] [SerializeField] private TMP_InputField editDescription;
    [SerializeField] private TMP_InputField editOrganizer;
    [SerializeField] private TMP_InputField editLocation;
    [SerializeField] private TMP_InputField editTime;
    [SerializeField] private TMP_InputField editDuration;
    [SerializeField] private Toggle editIsPublic;

    private void Awake()
    {
        if (AuthConnector.Instance.Perms != PermissionLevel.Admin)
        {
            deleteIcon.SetActive(false);
            editIcon.SetActive(false);
        }

        _myEventNames = EventManager.MyEvents;
    }

    private void OnEnable()
    {
        _target = EventManager.CurrentEvent;
        _bookmarked = _myEventNames.Contains(_target.Name);
        bookmarkIcon.SetActive(!_bookmarked);
        unbookmarkIcon.SetActive(_bookmarked);
        UpdateView();
    }

    /// <summary>
    /// Update the view and edit panel information
    /// </summary>
    private void UpdateView()
    {
        viewName.text = _target.Name;
        viewDescription.text = _target.Description;
        viewOrganizer.text = _target.Organizer;
        viewLocation.text = _target.Location;
        // To unix time
        viewTime.text = DateTimeOffset.FromUnixTimeSeconds(_target.Time).ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        viewDuration.text = _target.Duration.ToString();
        viewIsPublic.isOn = _target.IsPublic;
        editName.text = _target.Name;
        editDescription.text = _target.Description;
        editOrganizer.text = _target.Organizer;
        editLocation.text = _target.Location;
        // To unix time
        editTime.text = DateTimeOffset.FromUnixTimeSeconds(_target.Time).ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        editDuration.text = _target.Duration.ToString();
        editIsPublic.isOn = _target.IsPublic;
    }

    /// <summary>
    /// Once the user changes edit panel content and hits save button, update the state
    /// </summary>
    public void SaveChanges()
    {
        _target.Name = editName.text;
        _target.Description = editDescription.text;
        _target.Organizer = editOrganizer.text;
        _target.Location = editLocation.text;
        // Convert time to unix time
        var dto = new DateTimeOffset(DateTime.Parse(editTime.text).ToUniversalTime());
        _target.Time = dto.ToUnixTimeSeconds();
        _target.Duration = int.Parse(editDuration.text);
        _target.IsPublic = editIsPublic.isOn;
        UpdateView();
        var targetJson = JsonUtility.ToJson(_target);
        var prefix = _target.IsPublic ? "events/public/" : "events/private/";
        DatabaseConnector.Instance.Root.Child(prefix + _target.Name).SetRawJsonValueAsync(targetJson);
    }

    /// <summary>
    /// Bookmark the event, add that to the database under the user
    /// </summary>
    public void Bookmark()
    {
        _myEventNames.Add(_target.Name);
        bookmarkIcon.SetActive(false);
        unbookmarkIcon.SetActive(true);
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/events/" + _target.Name)
            .SetValueAsync("True");
    }

    /// <summary>
    /// Unbookmark the event, remove that to the database under the user
    /// </summary>
    public void Unbookmark()
    {
        _myEventNames.Remove(_target.Name);
        bookmarkIcon.SetActive(true);
        unbookmarkIcon.SetActive(false);
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/events/" + _target.Name)
            .SetValueAsync(null);
    }

    public void DetailViewClose()
    {
        viewPanel.SetActive(true);
        editPanel.SetActive(false);
    }
}