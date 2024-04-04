using System.Collections.Generic;
using Database;
using Auth;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// This class controls the lecture detail view (the pop up window), including read and edit views and bookmark/unbookmark methods.
/// Author: Zihao Du
/// Date: 2024-01-29
/// </summary>
public class LectureDetailViewManager : MonoBehaviour
{
    [FormerlySerializedAs("ViewPanel")] [Header("DetailView")] [SerializeField]
    private GameObject viewPanel;

    [FormerlySerializedAs("EditPanel")] [SerializeField] private GameObject editPanel;
    [FormerlySerializedAs("DeleteIcon")] [SerializeField] private GameObject deleteIcon;
    [FormerlySerializedAs("EditIcon")] [SerializeField] private GameObject editIcon;
    private Lecture _target;
    private bool _bookmarked;
    private List<string> _myLectureCodes;
    [SerializeField] private GameObject bookmarkIcon;
    [SerializeField] private GameObject unbookmarkIcon;
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
        _bookmarked = _myLectureCodes.Contains(_target.Code);
        bookmarkIcon.SetActive(!_bookmarked);
        unbookmarkIcon.SetActive(_bookmarked);
        UpdateView();
    }

    /// <summary>
    /// Update the view and edit panel information
    /// </summary>
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

    /// <summary>
    /// Once the user changes edit panel content and hits save button, update the state
    /// </summary>
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

    /// <summary>
    /// Bookmark the lecture, add that to the database under the user
    /// </summary>
    public void Bookmark()
    {
        _myLectureCodes.Add(_target.Code);
        bookmarkIcon.SetActive(false);
        unbookmarkIcon.SetActive(true);
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/lectures/" + _target.Code)
            .SetValueAsync("True");
    }

    /// <summary>
    /// Unbookmark the lecture, remove that to the database under the user
    /// </summary>
    public void Unbookmark()
    {
        _myLectureCodes.Remove(_target.Code);
        bookmarkIcon.SetActive(true);
        unbookmarkIcon.SetActive(false);
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