using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    // These are used for accessing database and current user
    private FirebaseAuth auth;
    private DatabaseReference db;
    // These are used to open the profile page for other users
    public static bool currentUser;
    public static FirebaseUser query;

    [Header("Canvas")]
    // Canvas containing user information
    public GameObject DisplayCanvas;
    // Canvas handling the edit of user information
    public GameObject EditCanvas;
    public GameObject PasswordCanvas;

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

    [Header("Input Values")]
    public TMP_InputField newUsername;
    public TMP_InputField newLevel;
    public TMP_InputField newProgram;
    // These values are used during change password
    public TMP_InputField CurrentPassword;
    public TMP_InputField NewPassword;
    public TMP_InputField ConfirmPassword;

    [Header("Pinned")]
    [SerializeField] Transform PinnedTemplate;

    private void Start()
    {
        // This value should be the query user value when navigated from friend!
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseDatabase.DefaultInstance.RootReference;
        // Display is the default view
        DisplayCanvas.SetActive(true);
        EditCanvas.SetActive(false);
        PasswordCanvas.SetActive(false);
        // If the id is not the user, then remove the edit button!
        if (currentUser)
        {
            EditButton.SetActive(false);
            ChangePasswordButton.SetActive(false);
        }

        StartCoroutine(getDBdata((List<string> data) =>
        {
            userIDDisplay.text = data[0];
            username.text = data[2];
            level.text = data[1];
            program.text = data[3];
        }));
    }

    /// <summary>
    /// 
    /// </summary>
    //private void favorites()
    //{
    //    StartCoroutine(getFavorites((List<string> data) =>
    //    {
    //        foreach (string entry in data)
    //        {
    //            Transform entryTransform = Instantiate(friendEntryTemplate, friendEntryContainer);
    //            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
    //            entryRectTransform.anchoredPosition = new Vector2(0, -friendEntryHeight * friendEntryTransformList.Count);
    //            entryTransform.gameObject.SetActive(true);
    //            entryTransform.Find("Email").GetComponent<TMP_Text>().text = friend.email;
    //            entryTransform.Find("Name").GetComponent<TMP_Text>().text = friend.nickName;
    //            friendEntryTransformList.Add(entryTransform);
    //        }
    //        if (friendEntryHeight * friends.Count > 1460)
    //        {
    //            // If the friend list is short, the default container height is viewport height (1460)
    //            friendEntryContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(800, friendEntryHeight * friends.Count);
    //        }
    //    }));

    //}

    private IEnumerable getFavorites(Action<List<string>> onCallBack)
    {
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

            var value = db.Child("users").Child(Utilities.removeDot(query.Email)).GetValueAsync();
            yield return new WaitUntil(predicate: () => value.IsCompleted);

            if(value != null)
            {
                DataSnapshot item = value.Result;
                userData.Add(item.Child("email").Value.ToString());
                userData.Add(item.Child("level").Value.ToString());
                userData.Add(item.Child("nickName").Value.ToString());
                userData.Add(item.Child("program").Value.ToString());
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
    }

    /// <summary>
    /// Function handling the save button click on edit canvas.
    /// </summary>
    public void Save()
    {
        updateDBdata();

        username.text = newUsername.text;
        level.text = newLevel.text;
        program.text = newProgram.text;

        DisplayCanvas.SetActive(true);
        EditCanvas.SetActive(false);
    }
}
