using System.Collections.Generic;
using Database;
using Auth;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class LectureDetailViewManager : MonoBehaviour
{
    [FormerlySerializedAs("ViewPanel")] [Header("DetailView")] [SerializeField]
    private GameObject viewPanel;

    [FormerlySerializedAs("EditPanel")] [SerializeField] private GameObject editPanel;
    [FormerlySerializedAs("DeleteIcon")] [SerializeField] private GameObject deleteIcon;
    [FormerlySerializedAs("EditIcon")] [SerializeField] private GameObject editIcon;
    private Lecture _target;
    private bool _pinned;
    private List<string> _myLectureCodes;
    [FormerlySerializedAs("PinIcon")] [SerializeField] private GameObject pinIcon;
    [FormerlySerializedAs("UnpinIcon")] [SerializeField] private GameObject unpinIcon;
    [SerializeField] private TMP_Text viewCode;
    [SerializeField] private TMP_Text viewName;
    [SerializeField] private TMP_Text viewInstructor;
    [SerializeField] private TMP_Text viewLocation;
    [SerializeField] private TMP_Text viewTimes;
    [SerializeField] private TMP_InputField editCode;
    [SerializeField] private TMP_InputField editName;
    [SerializeField] private TMP_InputField editInstructor;
    [SerializeField] private TMP_InputField editLocation;
    [SerializeField] private TMP_InputField editTimes;

    private void Awake()
    {
        if (AuthConnector.Instance.Perms != PermissionLevel.Admin)
        {
            deleteIcon.SetActive(false);
            editIcon.SetActive(false);
        }

        _myLectureCodes = LectureManager.MyLectures;
    }

    private void OnEnable()
    {
        _target = LectureManager.CurrentLecture;
        _pinned = _myLectureCodes.Contains(_target.Code);
        pinIcon.SetActive(!_pinned);
        unpinIcon.SetActive(_pinned);
        UpdateView();
    }

    private void UpdateView()
    {
        viewCode.text = _target.Code;
        viewName.text = _target.Name;
        viewInstructor.text = _target.Instructor;
        viewLocation.text = _target.Location;
        viewTimes.text = _target.Time;
        editCode.text = _target.Code;
        editName.text = _target.Name;
        editInstructor.text = _target.Instructor;
        editLocation.text = _target.Location;
        editTimes.text = _target.Time;
    }

    public void SaveChanges()
    {
        _target.Code = editCode.text;
        _target.Name = editName.text;
        _target.Instructor = editInstructor.text;
        _target.Location = editLocation.text;
        _target.Time = editTimes.text;
        UpdateView();
        var targetJson = JsonUtility.ToJson(_target);
        DatabaseConnector.Instance.Root.Child("lectures/" + _target.Code).SetRawJsonValueAsync(targetJson);
    }

    public void Pin()
    {
        _myLectureCodes.Add(_target.Code);
        pinIcon.SetActive(false);
        unpinIcon.SetActive(true);
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/lectures/" + _target.Code)
            .SetValueAsync("True");
    }

    public void Unpin()
    {
        _myLectureCodes.Remove(_target.Code);
        pinIcon.SetActive(true);
        unpinIcon.SetActive(false);
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/lectures/" + _target.Code)
            .SetValueAsync(null);
    }

    public void DetailViewClose()
    {
        viewPanel.SetActive(true);
        editPanel.SetActive(false);
    }
}