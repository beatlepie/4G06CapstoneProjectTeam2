using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference databaseReference;
    public TMP_Text lecList;
    [SerializeField] TMP_InputField lecCode;
    [SerializeField] TMP_InputField lecName;
    [SerializeField] TMP_InputField lecInstructor;
    // Start is called before the first frame update
    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        GetLectureData();
    }

    public void WriteNewLec()
    {
        Lecture lec = new Lecture(lecCode.text, lecName.text, lecInstructor.text, "Tue, Wed, Fri 12:30-13:20", "ITB AB102");
        string lecJson = JsonUtility.ToJson(lec);
        databaseReference.Child("lectures").Child(lec.code).SetRawJsonValueAsync(lecJson);
    }

    IEnumerator GetLectures(Action<string> onCallBack)
    {
        var lectureData = databaseReference.Child("lectures").OrderByChild("instructor").LimitToFirst(2).GetValueAsync();
        yield return new WaitUntil(predicate: () => lectureData.IsCompleted);
        if(lectureData != null)
        {
            string result = "";
            DataSnapshot snapshot = lectureData.Result;
            foreach (var x in snapshot.Children)
            {
                foreach (var i in x.Children)
                {
                    result += i.Value + " ";
                }
                result += "\n";
            }
            onCallBack.Invoke(result);
        }
    }

    public void GetLectureData()
    {
        StartCoroutine(GetLectures((string data) =>
        {
            lecList.text = data;
        }));
    }

    public void ExitDataPage()
    {
        SceneManager.LoadScene("MapComponent");
    }
}
