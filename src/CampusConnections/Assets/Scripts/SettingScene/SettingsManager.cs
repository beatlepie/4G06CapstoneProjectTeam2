using UnityEngine;
using UnityEngine.SceneManagement;
using Auth;
using Firebase.Auth;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;
using Database;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class SettingsManager : MonoBehaviour
{
    // These are used to open the profile page for other users
    public static bool CurrentUser;
    public static string QueryEmail;
    public static int State;

    [FormerlySerializedAs("DisplayCanvas")] [Header("Canvas")]
    // Canvas containing user information
    public GameObject displayCanvas;

    // Canvas handling the edit of user information
    [FormerlySerializedAs("EditCanvas")] public GameObject editCanvas;
    [FormerlySerializedAs("PasswordCanvas")] public GameObject passwordCanvas;
    [FormerlySerializedAs("PinnedLectureCanvas")] public GameObject pinnedLectureCanvas;
    [FormerlySerializedAs("PinnedEventCanvas")] public GameObject pinnedEventCanvas;
    [FormerlySerializedAs("ConfirmationCanvas")] public GameObject confirmationCanvas;

    [FormerlySerializedAs("EditButton")] [Header("Buttons")]
    // Edit button in display canvas
    public GameObject editButton;

    [FormerlySerializedAs("ChangePasswordButton")] public GameObject changePasswordButton;
    [FormerlySerializedAs("VerifyEmailButton")] public GameObject verifyEmailButton;

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

    [Header("Input Values")] public TMP_InputField newUsername;
    public TMP_InputField newLevel;
    public TMP_InputField newProgram;

    public TMP_InputField profileImageLink;

    // These values are used during change password
    [FormerlySerializedAs("CurrentPassword")] public TMP_InputField currentPassword;
    [FormerlySerializedAs("NewPassword")] public TMP_InputField newPassword;
    [FormerlySerializedAs("ConfirmPassword")] public TMP_InputField confirmPassword;

    [FormerlySerializedAs("PinnedLectureTemplate")] [Header("BookmarkedLecture")] [SerializeField]
    private Transform pinnedLectureTemplate;

    [FormerlySerializedAs("PinnedLectureView")] [SerializeField] private Transform pinnedLectureView;

    [FormerlySerializedAs("PinnedEventTemplate")] [Header("BookmarkedEvent")] [SerializeField]
    private Transform pinnedEventTemplate;

    [FormerlySerializedAs("PinnedEventView")] [SerializeField] private Transform pinnedEventView;

    [Header("EmailVerification")] public TMP_Text notificationText;

    private void Start()
    {
        // Default values for profile page.
        // Display is the default view
        displayCanvas.SetActive(true);
        editCanvas.SetActive(false);
        passwordCanvas.SetActive(false);
        pinnedLectureCanvas.SetActive(false);
        pinnedEventCanvas.SetActive(false);
        confirmationCanvas.SetActive(false);
        // If the id is not the user, then remove the edit buttons!
        if (!CurrentUser)
        {
            editButton.SetActive(false);
            changePasswordButton.SetActive(false);
        }
        else
        {
            QueryEmail = AuthConnector.Instance.CurrentUser.Email;
            editButton.SetActive(true);
            changePasswordButton.SetActive(true);
        }

        // If email is verified, do not display email verification button!
        if (AuthConnector.Instance.IsEmailVerified | (AuthConnector.Instance.CurrentUser.Email != QueryEmail))
            verifyEmailButton.SetActive(false);
        else
            verifyEmailButton.SetActive(true);

        // Based on where the scene was called from, load respective screens first!
        switch (State)
        {
            // bookmarked lectures
            case 1:
                displayCanvas.SetActive(false);
                pinnedLectureCanvas.SetActive(true);
                break;
            // bookmarked events
            case 2:
                displayCanvas.SetActive(false);
                pinnedEventCanvas.SetActive(true);
                break;
        }

        StartCoroutine(GetDBData(data =>
        {
            userIDDisplay.text = data[0];
            username.text = data[2];
            level.text = data[1];
            program.text = data[3];
            profileImageLink.text = data[4];

            StartCoroutine(GetImage(data[4]));
        }));

        Favorites();
    }

    private void Favorites()
    {
        //NO lectures are visible for guest accounts
        if (AuthConnector.Instance.Perms != PermissionLevel.Guest)
            StartCoroutine(GetPinnedLectures(data =>
            {
                var entryHeight = -200;

                for (var i = 0; i < data.Count; i = i + 3)
                {
                    var entryTransform = Instantiate(pinnedLectureTemplate, pinnedLectureView);
                    var entryRectTransform = entryTransform.GetComponent<RectTransform>();
                    entryRectTransform.anchoredPosition = new Vector2(0, -660 + entryHeight * i / 3f);
                    entryTransform.gameObject.SetActive(true);

                    entryTransform.Find("Code").GetComponent<TMP_Text>().text = data[i];
                    entryTransform.Find("Name").GetComponent<TMP_Text>().text = data[i + 1];
                    entryTransform.Find("Location").GetComponent<TMP_Text>().text = data[i + 2];
                }
            }));
        StartCoroutine(GetPinnedEvents(data =>
        {
            var entryHeight = -200;
            for (var i = 0; i < data.Count; i = i + 3)
            {
                var entryTransform = Instantiate(pinnedEventTemplate, pinnedEventView);
                var entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition = new Vector2(0, -660 + entryHeight * i / 3f);
                entryTransform.gameObject.SetActive(true);

                entryTransform.Find("Name").GetComponent<TMP_Text>().text = data[i];
                entryTransform.Find("Organizer").GetComponent<TMP_Text>().text = data[i + 1];
                entryTransform.Find("Location").GetComponent<TMP_Text>().text = data[i + 2];
            }
        }));
    }

    /// <summary>
    /// Returns the list of pinned lectures.
    /// </summary>
    /// <param name="onCallBack">List to be returned.</param>
    /// <returns>Returns list of lectures that are pinned by the user. </returns>
    private static IEnumerator GetPinnedLectures(Action<List<string>> onCallBack)
    {
        //Currently a duplicate of a function in the lecture view side!
        var emailWithoutDot = Utilities.RemoveDot(QueryEmail);
        var dbRoot = DatabaseConnector.Instance.Root;
        var userData = dbRoot.Child("users/" + emailWithoutDot + "/lectures").GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);
        if (userData != null)
        {
            var pinnedLectures = new List<string>();
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                pinnedLectures.Add(x.Key);

                var lecture = dbRoot.Child("lectures").Child(x.Key).GetValueAsync();
                yield return new WaitUntil(() => lecture.IsCompleted);

                pinnedLectures.Add(lecture.Result.Child("instructor").Value.ToString());
                pinnedLectures.Add(lecture.Result.Child("location").Value.ToString());
            }

            onCallBack.Invoke(pinnedLectures);
        }
    }

    /// <summary>
    /// Returns the list of pinned events.
    /// </summary>
    /// <param name="onCallBack">List to be returned.</param>
    /// <returns>Returns list of events that are pinned by the user. </returns>
    private static IEnumerator GetPinnedEvents(Action<List<string>> onCallBack)
    {
        //Currently a duplicate of a function in the lecture view side!
        var emailWithoutDot = Utilities.RemoveDot(QueryEmail);
        var dbRoot = DatabaseConnector.Instance.Root;
        var userData = dbRoot.Child("users/" + emailWithoutDot + "/events").GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);
        if (userData != null)
        {
            var pinnedEvents = new List<string>();
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                pinnedEvents.Add(x.Key);

                var e1 = dbRoot.Child("events/public").Child(x.Key).GetValueAsync();
                var e2 = dbRoot.Child("events/private").Child(x.Key).GetValueAsync();
                // If the user is a guest, then DO NOT query the private events!
                if (AuthConnector.Instance.Perms == PermissionLevel.Guest) e2 = null;
                yield return new WaitUntil(() => e2 != null && e1.IsCompleted && e2.IsCompleted);
                if (e1 != null && e1.Result.HasChild("name"))
                {
                    pinnedEvents.Add(e1.Result.Child("organizer").Value.ToString());
                    pinnedEvents.Add(e1.Result.Child("location").Value.ToString());
                }
                else if (e2 != null && e2.Result.HasChild("name"))
                {
                    pinnedEvents.Add(e2.Result.Child("organizer").Value.ToString());
                    pinnedEvents.Add(e2.Result.Child("location").Value.ToString());
                }
            }

            onCallBack.Invoke(pinnedEvents);
        }
    }

    /// <summary>
    /// Function for calling values from the database.
    /// </summary>
    private static IEnumerator GetDBData(Action<List<string>> onCallBack)
    {
        var userData = new List<string>();

        var dbRoot = DatabaseConnector.Instance.Root;
        var value = dbRoot.Child("users").Child(Utilities.RemoveDot(QueryEmail)).GetValueAsync();
        yield return new WaitUntil(() => value.IsCompleted);

        if (value != null)
        {
            var item = value.Result;
            userData.Add(item.Child("email").Value.ToString());
            userData.Add(item.Child("level").Value.ToString());
            userData.Add(item.Child("nickName").Value.ToString());
            userData.Add(item.Child("program").Value.ToString());
            userData.Add(item.Child("photo").Value.ToString());
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
    /// Retrieves and sets the image
    /// </summary>
    /// <param name="url">url of the photo we want to retrieve.</param>
    /// <param name="success"></param>
    /// <returns>Action on whether the image was retrieved or not</returns>
    private IEnumerator GetImage(string url, Action<bool> success = null)
    {
        // GET image from web
        var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            var tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Debug.Log("texture received!");
            // Below method must be used as resize and reinitialize only changes the container not the image!
            var scaled = new Texture2D(500, 500);
            Graphics.ConvertTexture(tex, scaled);
            // Convert to sprite and give to profileDisplay!
            var displayable = Sprite.Create(scaled, new Rect(new Vector2(0, 0), new Vector2(500, 500)),
                new Vector2(0, 0));
            profileDisplay.sprite = displayable;
            profileEdit.sprite = displayable;
        }
        else
        {
            Debug.Log("Error retrieving profile image!");
            success?.Invoke(false);
        }

        //// Clean up
        www.Dispose();
    }

    /// <summary>
    /// Function for updating the db with the new data set by the user.
    /// </summary>
    private void UpdateDBData()
    {
        // TODO: implement safety feature!
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);

        var dbRoot = DatabaseConnector.Instance.Root;
        dbRoot.Child("users").Child(emailWithoutDot).Child("level").SetValueAsync(newLevel.text);
        dbRoot.Child("users").Child(emailWithoutDot).Child("nickName").SetValueAsync(newUsername.text);
        dbRoot.Child("users").Child(emailWithoutDot).Child("program").SetValueAsync(newProgram.text);
        dbRoot.Child("users").Child(emailWithoutDot).Child("photo").SetValueAsync(profileImageLink.text);
    }

    /// <summary>
    /// Function handling the change password button click on display canvas.
    /// </summary>
    public void ChangePassword()
    {
        passwordCanvas.SetActive(true);
        displayCanvas.SetActive(false);
        // Technically not necessary to do this, but for safety
        editCanvas.SetActive(false);
    }

    /// <summary>
    /// Function handling the Save button click on password canvas.
    /// </summary>
    public void SaveNewPassword()
    {
        var user = AuthConnector.Instance.CurrentUser;

        // This checks the current password
        user.ReauthenticateAsync(EmailAuthProvider.GetCredential(user.Email, currentPassword.text)).ContinueWith(task =>
        {
            Debug.LogFormat("new password attempt!");
            // If the current password is correct:
            if (task.IsCompletedSuccessfully)
            {
                Debug.LogFormat("Password correct!");
                // If the new passwords match:
                if (newPassword.text == confirmPassword.text)
                {
                    Debug.LogFormat("Password matches!");
                    // This attempts to update the password
                    user.UpdatePasswordAsync(newPassword.text).ContinueWith(update =>
                    {
                        if (update.IsCompletedSuccessfully)
                        {
                            Debug.LogFormat("new password updated!");
                            status.text = "New password update successful!";
                        }
                        else
                        {
                            if (update.Exception != null) Debug.LogFormat(update.Exception.ToString());
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

            return new WaitUntil(() => task.IsCompleted);
        });
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

        displayCanvas.SetActive(false);
        editCanvas.SetActive(true);
    }

    /// <summary>
    /// Function handling the cancel button click on edit canvas.
    /// </summary>
    public void Cancel()
    {
        if (State == 0)
        {
            displayCanvas.SetActive(true);
            editCanvas.SetActive(false);
            pinnedLectureCanvas.SetActive(false);
            pinnedEventCanvas.SetActive(false);
        }
        // return to lecture scene
        else if (State == 1)
        {
            SceneManager.LoadScene("LectureScene");
        }
        // return to event scene
        else if (State == 2)
        {
            SceneManager.LoadScene("EventScene");
        }
    }

    /// <summary>
    /// Function handling the save button click on edit canvas.
    /// </summary>
    public void Save()
    {
        UpdateDBData();

        StartCoroutine(GetImage(profileImageLink.text, _ =>
        {
            profileImageLink.text = "This image is invalid!";
        }));

        // reusing this function as it will also update profile image
        StartCoroutine(GetDBData(data =>
        {
            userIDDisplay.text = data[0];
            username.text = data[2];
            level.text = data[1];
            program.text = data[3];
            profileImageLink.text = data[4];
        }));

        displayCanvas.SetActive(true);
        editCanvas.SetActive(false);
    }

    public void PinnedLecture()
    {
        displayCanvas.SetActive(false);
        editCanvas.SetActive(false);
        pinnedLectureCanvas.SetActive(true);
        pinnedEventCanvas.SetActive(false);
        passwordCanvas.SetActive(false);
    }

    public void PinnedEvent()
    {
        displayCanvas.SetActive(false);
        editCanvas.SetActive(false);
        pinnedLectureCanvas.SetActive(false);
        pinnedEventCanvas.SetActive(true);
        passwordCanvas.SetActive(false);
    }

    public void OnEmailVerificationClick()
    {
        StartCoroutine(SendEmailVerification());
    }

    /// <summary>
    /// Function called when Email Verification button is pressed.
    /// Sends email verification email.
    /// </summary>
    private IEnumerator SendEmailVerification()
    {
        // Since the registration was success, the email verification can be sent
        var verification = AuthConnector.Instance.CurrentUser.SendEmailVerificationAsync();
        yield return new WaitUntil(() => verification.IsCompleted);

        // If it fails, will attempt at login instead!
        notificationText.text = verification.IsCompletedSuccessfully ? "Email verification sent!" : "<color=#F74848>Email verification failed! Try again later.";
    }

    /// <summary>
    /// This function exists to link DeleteAccountConfirmed to the button click on unity.
    /// </summary>
    public void DeleteAccount()
    {
        StartCoroutine(DeleteAccountConfirmed());
    }

    /// <summary>
    /// This will delete all user data related to the account and kill the application.
    /// </summary>
    private IEnumerator DeleteAccountConfirmed()
    {
        var dbRoot = DatabaseConnector.Instance.Root;
        var value = dbRoot.Child("users").Child(Utilities.RemoveDot(QueryEmail)).GetValueAsync();
        yield return new WaitUntil(() => value.IsCompleted);

        foreach (var i in value.Result.Child("friends").Children)
            dbRoot.Child("users").Child(Utilities.AddDot(i.Key)).Child("friends").Child(Utilities.RemoveDot(QueryEmail))
                .SetValueAsync(null);
        dbRoot.Child("users").Child(Utilities.RemoveDot(QueryEmail)).SetValueAsync(null);
        // This is supposed to remove the user from firebase auth
        AuthConnector.Instance.Auth.CurrentUser.DeleteAsync();

        Application.Quit();
    }

    /// <summary>
    /// This function will handle jumping to the respective list with the item searched when the bookmarked item is clicked!
    /// </summary>
    public void OnEntryClick()
    {
        // if pinned lecture screen is true, then redirect entry clicked to lecture scene
        if (pinnedLectureCanvas.activeSelf)
        {
            SceneManager.LoadScene("LectureScene");
            LectureManager.DefaultSearchOption = "code";
            LectureManager.DefaultSearchString = EventSystem.current.currentSelectedGameObject.transform.parent
                .gameObject.transform.Find("Code").GetComponent<TMP_Text>().text;
        }
        // if pinned event screen is true, then redirect entry clicked to event scene
        else
        {
            SceneManager.LoadScene("EventScene");
            EventManager.DefaultSearchOption = "name";
            EventManager.DefaultSearchString = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject
                .transform.Find("Name").GetComponent<TMP_Text>().text;
        }
    }
}