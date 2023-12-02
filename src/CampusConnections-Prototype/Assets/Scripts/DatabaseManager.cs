using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using System.Threading;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference databaseReference;
    public TMP_Text lecList;
    public TMP_Text addMSG;
    [SerializeField] TMP_InputField lecCode;
    [SerializeField] TMP_InputField lecName;
    [SerializeField] TMP_InputField lecInstructor;
    // Start is called before the first frame update

    

    void Start()
    {

        addMSG.alpha = 0f;

        UnityEngine.Debug.Log("db manager script running");
        Firebase.AppOptions options = new Firebase.AppOptions();
        options.ApiKey = "AIzaSyADnG2YOg9G7q9pddYlTthUj9G16G8dlOE";
        options.AppId = "1:89992135088:android:0f8c9a92bf587c521e5675";
        options.ProjectId = "campusconnections";
        options.DatabaseUrl = new System.Uri("https://campusconnections-default-rtdb.firebaseio.com");
        options.StorageBucket = "campusconnections.appspot.com";
        FirebaseApp.Create(options);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        GetLectureData();
    }


    public void WriteNewLec()
    {
        //UnityEngine.Debug.Log(ServerValue.Timestamp);
        Lecture lec = new Lecture(lecCode.text, lecName.text, lecInstructor.text, "Tue, Wed, Fri 12:30-13:20", "ITB AB102");
        string lecJson = JsonUtility.ToJson(lec);

        //var newLectureRef = databaseReference.Child("lectures").Push();
        //lecJson = lecJson+
        databaseReference.Child("lectures").Push().SetRawJsonValueAsync(lecJson);

        addMSG.alpha = 1f;
        //addMSG.text = "Lecture succesfully added!";
        addMSG.CrossFadeAlpha(0,2, false);
        //addMSG.enabled = false;
        //addMSG.text = "Lecture succesfully added!";
        //Thread.Sleep(2000);
        //addMSG.text = "";

    }

    IEnumerator GetLectures(Action<string> onCallBack)
    {
        var lecInfo = new List<string>();

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
                    lecInfo.Add(i.Value.ToString());
                    //UnityEngine.Debug.Log(lecInfo);
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
