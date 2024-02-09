using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SettingsManager : MonoBehaviour
{
    // These are used for accessing database and current user
    private FirebaseAuth auth;
    private DatabaseReference db;
    // These are used to open the profile page for other users
    public static bool currentUser;
    public static string queryEmail;

    [Header("Canvas")]
    // Canvas containing user information
    public GameObject DisplayCanvas;
    // Canvas handling the edit of user information
    public GameObject EditCanvas;
    public GameObject PasswordCanvas;
    public GameObject PinnedCanvas;

    [Header("Buttons")]
    // Edit button in display canvas
    public GameObject EditButton;
    public GameObject ChangePasswordButton;

    [Header("Values")]
    // UserID, immutable value, for display and edit canvas
    public TMP_Text userIDDisplay;
    public TMP_Text userIDEdit;
    // User nickname, can be changed by user.
    public TMP_Text username;
    public TMP_Text program;
    public TMP_Text level;
    public TMP_Text status;
    public Image profileDisplay;
    public Image profileEdit;

    [Header("Input Values")]
    public TMP_InputField newUsername;
    public TMP_InputField newLevel;
    public TMP_InputField newProgram;
    public TMP_InputField profileImageLink;
    // These values are used during change password
    public TMP_InputField CurrentPassword;
    public TMP_InputField NewPassword;
    public TMP_InputField ConfirmPassword;

    [Header("Pinned")]
    [SerializeField] Transform PinnedTemplate;
    [SerializeField] Transform PinnedView;

    private void Start()
    {
        // Default values for profile page.
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseDatabase.DefaultInstance.RootReference;
        // Display is the default view
        DisplayCanvas.SetActive(true);
        EditCanvas.SetActive(false);
        PasswordCanvas.SetActive(false);
        PinnedCanvas.SetActive(false);
        // If the id is not the user, then remove the edit buttons!
        if (!currentUser)
        {
            EditButton.SetActive(false);
            ChangePasswordButton.SetActive(false);
        }
        else
        {
            // This is technically only reachable from debug, but if there is an error, it will display the user profile page.
            queryEmail = auth.CurrentUser.Email;
        }

        StartCoroutine(getDBdata((List<string> data) =>
        {
            userIDDisplay.text = data[0];
            username.text = data[2];
            level.text = data[1];
            program.text = data[3];
            profileImageLink.text = data[4];
        }));

        favorites();
    }

    private void favorites()
    {
        StartCoroutine(getPinned((List<string> data) =>
        {
            int entryHeight = -200;

            for (int i = 0; i < data.Count; i = i + 3)
            {
                Transform entryTransform = Instantiate(PinnedTemplate, PinnedView);
                RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition = new Vector2(0, -660 + entryHeight * i/3);
                entryTransform.gameObject.SetActive(true);

                entryTransform.Find("Code").GetComponent<TMP_Text>().text = data[i];
                entryTransform.Find("Name").GetComponent<TMP_Text>().text = data[i+1];
                entryTransform.Find("Location").GetComponent<TMP_Text>().text = data[i+2];
            }
        }));
    }

    /// <summary>
    /// Returns the list of pinned events and lectures.
    /// </summary>
    /// <param name="onCallBack">List to be returned.</param>
    /// <returns>Returns list of events/lectures that are pinned by the user. </returns>
    private IEnumerator getPinned(Action<List<string>> onCallBack)
    {
        //Currently a duplicate of a function in the lecture view side!
        string emailWithoutDot = Utilities.removeDot(auth.CurrentUser.Email);
        var userData = db.Child("users/" + emailWithoutDot + "/lectures").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if (userData != null)
        {
            List<string> pinnedLectures = new List<string>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                pinnedLectures.Add(x.Key.ToString());

                var lecture = db.Child("lectures").Child(x.Key.ToString()).GetValueAsync();
                yield return new WaitUntil(predicate: () => lecture.IsCompleted);

                pinnedLectures.Add(lecture.Result.Child("instructor").Value.ToString());
                pinnedLectures.Add(lecture.Result.Child("location").Value.ToString());
                
            }
            onCallBack.Invoke(pinnedLectures);
        }
    }

    /// <summary>
    /// Function for calling values from the database.
    /// </summary>
    private IEnumerator getDBdata(Action<List<string>> onCallBack)
    {
        List<string> userData = new List<string>();

        var value = db.Child("users").Child(Utilities.removeDot(queryEmail)).GetValueAsync();
        yield return new WaitUntil(predicate: () => value.IsCompleted);

        if (value != null)
        {
            DataSnapshot item = value.Result;
            userData.Add(item.Child("email").Value.ToString());
            userData.Add(item.Child("level").Value.ToString());
            userData.Add(item.Child("nickName").Value.ToString());
            userData.Add(item.Child("program").Value.ToString());
            userData.Add(item.Child("photo").Value.ToString());

            // GET image from web
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(item.Child("photo").Value.ToString());
            yield return www.SendWebRequest();

            Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            // Below method must be used as resize and reinitialize only changes the container not the image!
            Texture2D scaled = new Texture2D(300, 300);
            Graphics.ConvertTexture(tex, scaled);
            // Convert to sprite and give to profileDisplay!
            Sprite displayable = Sprite.Create(scaled, new Rect(new Vector2(0, 0), new Vector2(300, 300)), new Vector2(0, 0));
            profileDisplay.sprite = displayable;
            profileEdit.sprite = displayable;
            //// Clean up
            www.Dispose();
            www = null;
        }
        else
        {
            // TODO: make this better?
            // Handles case where no data was retrieved
            userData.Add("Email Placeholder");
            userData.Add("Level Placeholder");
            userData.Add("Username Placeholder");
            userData.Add("Program Placeholder");
        }
        onCallBack.Invoke(userData);
    }

    /// <summary>
    /// Function for updating the db with the new data set by the user.
    /// </summary>
    private void updateDBdata()
    {
        // TODO: implement safety feature!
        string emailWithoutDot = Utilities.removeDot(auth.CurrentUser.Email);
        db.Child("users").Child(emailWithoutDot).Child("level").SetValueAsync(newLevel.text);
        db.Child("users").Child(emailWithoutDot).Child("nickName").SetValueAsync(newUsername.text);
        db.Child("users").Child(emailWithoutDot).Child("program").SetValueAsync(newProgram.text);
        db.Child("users").Child(emailWithoutDot).Child("photo").SetValueAsync(profileImageLink.text);
    }

    /// <summary>
    /// Function handling the change password button click on display canvas.
    /// </summary>
    public void ChangePassword()
    {
        PasswordCanvas.SetActive(true);
        DisplayCanvas.SetActive(false);
        // Technically not necessary to do this, but for safety
        EditCanvas.SetActive(false);
    }

    /// <summary>
    /// Function handling the Save button click on password canvas.
    /// </summary>
    public void SaveNewPassword()
    {
        FirebaseUser user = auth.CurrentUser;

        // This checks the current password
        user.ReauthenticateAsync(EmailAuthProvider.GetCredential(user.Email, CurrentPassword.text)).ContinueWith(task =>
        {
            Debug.LogFormat("new password attempt!");
            // If the current password is correct:
            if (task.IsCompletedSuccessfully)
            {
            Debug.LogFormat("Password correct!");
                // If the new passwords match:
                if (NewPassword.text == ConfirmPassword.text)
                {
                Debug.LogFormat("Password matches!");
                    // This attempts to update the password
                    user.UpdatePasswordAsync(NewPassword.text).ContinueWith(update =>
                    {
                        if (update.IsCompletedSuccessfully)
                        {
                            Debug.LogFormat("new password updated!");
                            status.text = "New password update successful!";
                        }
                        else
                        {
                            Debug.LogFormat(update.Exception.ToString());
                            Debug.LogFormat("new password update failed!");
                                // There is a chance insecure password is used, firebase will reject that password
                                status.text = "New password update failed! \n" +
                                          "This password may not be viable!";
                        }
                    });
                }
                else
                {
                    status.text = "New password does not match!";
                }
            }
            else
            {
                status.text = "Current password is incorrect!";
            }

            new WaitUntil(predicate: () => task.IsCompleted);
        });

        // Need to fix this, this method must be coroutined to work!

        // Leaving the IEnumerator code here just in case!
        //Debug.LogFormat("new password attempt!");
        //// This checks the current password
        //Task userValidation = user.ReauthenticateAsync(EmailAuthProvider.GetCredential(user.Email, CurrentPassword.text));
        //yield return new WaitUntil(predicate: () => userValidation.IsCompleted);

        //if (userValidation.IsCompletedSuccessfully)
        //{
        //    Debug.LogFormat("Password correct!");
        //    // If the new passwords match:
        //    if (NewPassword.text == ConfirmPassword.text)
        //    {
        //        Debug.LogFormat("Password matches!");

        //        Task updatePassword = user.UpdatePasswordAsync(NewPassword.text);
        //        yield return new WaitUntil(predicate: () => updatePassword.IsCompleted);

        //        if (updatePassword.IsCompletedSuccessfully)
        //        {
        //            Debug.LogFormat("new password updated!");
        //            status.text = "New password update successful!";
        //        }
        //        else
        //        {
        //            Debug.LogFormat(updatePassword.Exception.ToString());
        //            Debug.LogFormat("new password update failed!");
        //            // There is a chance insecure password is used, firebase will reject that password
        //            status.text = "New password update failed! \n" +
        //                          "This password may not be viable!";
        //        }
        //    }
        //    else
        //    {
        //        status.text = "New password does not match!";
        //    }
        //}
        //else
        //{
        //    status.text = "Current password is incorrect!";
        //}
    }

    /// <summary>
    /// Function handling the return button click on display canvas.
    /// </summary>
    public void Return()
    {
        SceneManager.LoadScene("MenuScene");
    }

    /// <summary>
    /// Function handling the edit button click on display canvas.
    /// </summary>
    public void Edit()
    {
        // All settings values should have the original values
        userIDEdit.text = userIDDisplay.text;
        newUsername.text = username.text;
        newProgram.text = program.text;
        newLevel.text = level.text;

        DisplayCanvas.SetActive(false);
        EditCanvas.SetActive(true);
    }

    /// <summary>
    /// Function handling the cancel button click on edit canvas.
    /// </summary>
    public void Cancel()
    {
        DisplayCanvas.SetActive(true);
        EditCanvas.SetActive(false);
        PinnedCanvas.SetActive(false);
    }

    /// <summary>
    /// Function handling the save button click on edit canvas.
    /// </summary>
    public void Save()
    {
        updateDBdata();

        // reusing this function as it will also update profile image
        StartCoroutine(getDBdata((List<string> data) =>
        {
            userIDDisplay.text = data[0];
            username.text = data[2];
            level.text = data[1];
            program.text = data[3];
            profileImageLink.text = data[4];
        }));

        DisplayCanvas.SetActive(true);
        EditCanvas.SetActive(false);
    }

    public void Pinned()
    {
        DisplayCanvas.SetActive(false);
        EditCanvas.SetActive(false);
        PinnedCanvas.SetActive(true);
        PasswordCanvas.SetActive(false);
    }

    // Copied over from lecturemanager, need to change for pinned view!
    //public void OnEntryClick()
    //{
    //    GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
    //    string code = template.transform.Find("codeText").GetComponent<TMP_Text>().text;
    //    Lecture target = lectureEntryList.Find(lecture => lecture.code == code);
    //    currentLecture = target;
    //}
}
