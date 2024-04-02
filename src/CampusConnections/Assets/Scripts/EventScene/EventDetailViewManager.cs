using System;
using System.Collections.Generic;
using Database;
using Auth;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EventDetailViewManager : MonoBehaviour
{
    [FormerlySerializedAs("ViewPanel")] [Header("DetailView")] [SerializeField]
    private GameObject viewPanel;

    [FormerlySerializedAs("EditPanel")] [SerializeField] private GameObject editPanel;
    private Event _target;
    private bool _pinned;
    private List<string> _myEventNames;
    [FormerlySerializedAs("PinIcon")] [SerializeField] private GameObject pinIcon;
    [FormerlySerializedAs("UnpinIcon")] [SerializeField] private GameObject unpinIcon;
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
        _pinned = _myEventNames.Contains(_target.Name);
        pinIcon.SetActive(!_pinned);
        unpinIcon.SetActive(_pinned);
        UpdateView();
    }

    private void UpdateView()
    {
        viewName.text = _target.Name;
        viewDescription.text = _target.Description;
        viewOrganizer.text = _target.Organizer;
        viewLocation.text = _target.Location;
        viewTime.text = DateTimeOffset.FromUnixTimeSeconds(_target.Time).ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        viewDuration.text = _target.Duration.ToString();
        viewIsPublic.isOn = _target.IsPublic;
        editName.text = _target.Name;
        editDescription.text = _target.Description;
        editOrganizer.text = _target.Organizer;
        editLocation.text = _target.Location;
        editTime.text = DateTimeOffset.FromUnixTimeSeconds(_target.Time).ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        editDuration.text = _target.Duration.ToString();
        editIsPublic.isOn = _target.IsPublic;
    }

    public void SaveChanges()
    {
        _target.Name = editName.text;
        _target.Description = editDescription.text;
        _target.Organizer = editOrganizer.text;
        _target.Location = editLocation.text;
        var dto = new DateTimeOffset(DateTime.Parse(editTime.text).ToUniversalTime());
        _target.Time = dto.ToUnixTimeSeconds();
        _target.Duration = int.Parse(editDuration.text);
        _target.IsPublic = editIsPublic.isOn;
        UpdateView();
        var targetJson = JsonUtility.ToJson(_target);
        var prefix = _target.IsPublic ? "events/public/" : "events/private/";
        DatabaseConnector.Instance.Root.Child(prefix + _target.Name).SetRawJsonValueAsync(targetJson);
    }

    public void Pin()
    {
        _myEventNames.Add(_target.Name);
        pinIcon.SetActive(false);
        unpinIcon.SetActive(true);
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/events/" + _target.Name)
            .SetValueAsync("True");
    }

    public void Unpin()
    {
        _myEventNames.Remove(_target.Name);
        pinIcon.SetActive(true);
        unpinIcon.SetActive(false);
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