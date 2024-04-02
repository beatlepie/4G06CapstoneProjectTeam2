using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class LectureDetailViewManager : MonoBehaviour
{
    [Header("DetailView")] [SerializeField]
    private GameObject ViewPanel;

    [SerializeField] private GameObject EditPanel;
    [SerializeField] private GameObject DeleteIcon;
    [SerializeField] private GameObject EditIcon;
    private Lecture target;
    private bool pinned;
    private List<string> myLectureCodes;
    [SerializeField] private GameObject PinIcon;
    [SerializeField] private GameObject UnpinIcon;
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
        if (AuthConnector.Instance.Perms != PermissonLevel.Admin)
        {
            DeleteIcon.SetActive(false);
            EditIcon.SetActive(false);
        }

        myLectureCodes = LectureManager.myLectures;
    }

    private void OnEnable()
    {
        target = LectureManager.currentLecture;
        pinned = myLectureCodes.Contains(target.code);
        PinIcon.SetActive(!pinned);
        UnpinIcon.SetActive(pinned);
        UpdateView();
    }

    private void UpdateView()
    {
        viewCode.text = target.code;
        viewName.text = target.name;
        viewInstructor.text = target.instructor;
        viewLocation.text = target.location;
        viewTimes.text = target.time;
        editCode.text = target.code;
        editName.text = target.name;
        editInstructor.text = target.instructor;
        editLocation.text = target.location;
        editTimes.text = target.time;
    }

    public void SaveChanges()
    {
        target.code = editCode.text;
        target.name = editName.text;
        target.instructor = editInstructor.text;
        target.location = editLocation.text;
        target.time = editTimes.text;
        UpdateView();
        var targetJson = JsonUtility.ToJson(target);
        DatabaseConnector.Instance.Root.Child("lectures/" + target.code).SetRawJsonValueAsync(targetJson);
    }

    public void Pin()
    {
        myLectureCodes.Add(target.code);
        PinIcon.SetActive(false);
        UnpinIcon.SetActive(true);
        var emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/lectures/" + target.code)
            .SetValueAsync("True");
    }

    public void Unpin()
    {
        myLectureCodes.Remove(target.code);
        PinIcon.SetActive(true);
        UnpinIcon.SetActive(false);
        var emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/lectures/" + target.code)
            .SetValueAsync(null);
    }

    public void DetailViewClose()
    {
        ViewPanel.SetActive(true);
        EditPanel.SetActive(false);
    }
}