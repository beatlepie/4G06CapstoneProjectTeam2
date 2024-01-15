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
    [SerializeField] TMP_InputField lecLocation;
    [SerializeField] TMP_InputField lecTimes;

    // Start is called before the first frame update



    void Start()
    {

        addMSG.CrossFadeAlpha(0f,0f,false);

        UnityEngine.Debug.Log("db manager script running");
        Firebase.AppOptions options = new Firebase.AppOptions();
        options.ApiKey = "AIzaSyADnG2YOg9G7q9pddYlTthUj9G16G8dlOE";
        options.AppId = "1:89992135088:android:0f8c9a92bf587c521e5675";
        options.ProjectId = "campusconnections";
        options.DatabaseUrl = new System.Uri("https://campusconnections-default-rtdb.firebaseio.com");
        options.StorageBucket = "campusconnections.appspot.com";
        FirebaseApp.Create(options);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    
    }


    public void WriteNewLec()
    {    
        //UnityEngine.Debug.Log(ServerValue.Timestamp);
        Lecture lec = new Lecture(lecCode.text, lecName.text, lecInstructor.text, lecTimes.text, lecLocation.text);
        string lecJson = JsonUtility.ToJson(lec);

        databaseReference.Child("lectures").Push().SetRawJsonValueAsync(lecJson);

   
        addMSG.CrossFadeAlpha(1f, 0f, false);
        addMSG.CrossFadeAlpha(0f, 2f, false);

    }

    public void resetAlpha()
    {
        addMSG.CrossFadeAlpha(1, 2f, false);
    }


    public void ExitDataPage()
    {
        SceneManager.LoadScene("MapComponent");
    }



}
