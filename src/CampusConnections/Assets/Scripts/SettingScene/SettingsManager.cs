using UnityEngine;
using UnityEngine.SceneManagement;
using Auth;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;
using Database;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class SettingsManager : MonoBehaviour
{
    // These are used to open the profile page for other users
    public static bool currentUser;
    public static string queryEmail;
    public static int state;

    [Header("Canvas")]
    // Canvas containing user information
    public GameObject DisplayCanvas;
    // Canvas handling the edit of user information
    public GameObject EditCanvas;
    public GameObject PasswordCanvas;
    public GameObject PinnedLectureCanvas;
    public GameObject PinnedEventCanvas;
    public GameObject ConfirmationCanvas;

    [Header("Buttons")]
    // Edit button in display canvas
    public GameObject EditButton;
    public GameObject ChangePasswordButton;
    public GameObject VerifyEmailButton;

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

    [Header("PinnedLecture")]
    [SerializeField] Transform PinnedLectureTemplate;
    [SerializeField] Transform PinnedLectureView;

    [Header("PinnedEvent")]
    [SerializeField] Transform PinnedEventTemplate;
    [SerializeField] Transform PinnedEventView;

    public TMP_Text notificationText;

    private void Start()
    {
        // Default values for profile page.
        // Display is the default view
        DisplayCanvas.SetActive(true);
        EditCanvas.SetActive(false);
        PasswordCanvas.SetActive(false);
        PinnedLectureCanvas.SetActive(false);
        PinnedEventCanvas.SetActive(false);
        ConfirmationCanvas.SetActive(false);
        // If the id is not the user, then remove the edit buttons!
        if (!currentUser)
        {
            EditButton.SetActive(false);
            ChangePasswordButton.SetActive(false);
        }
        else
        {
            queryEmail = AuthConnector.Instance.CurrentUser.Email;
            EditButton.SetActive(true);
            ChangePasswordButton.SetActive(true);
        }

        // If email is verified, do not display email verification button!
        if (AuthConnector.Instance.CurrentUser.IsEmailVerified)
        {
            VerifyEmailButton.SetActive(false);
        }
        else
        {
            VerifyEmailButton.SetActive(true);
        }

        // Based on where the scene was called from, load respective screens first!
        switch (state)
        {
            // bookmarked lectures
            case 1:
                DisplayCanvas.SetActive(false);
                PinnedLectureCanvas.SetActive(true);
                break;
            // bookmarked events
            case 2:
                DisplayCanvas.SetActive(false);
                PinnedEventCanvas.SetActive(true);
                break;

            default:
                break;
        }

        StartCoroutine(getDBdata((List<string> data) =>
        {
            userIDDisplay.text = data[0];
            username.text = data[2];
            level.text = data[1];
            program.text = data[3];
            profileImageLink.text = data[4];

            StartCoroutine(getImage(data[4]));
        }));

        favorites();
    }

    private void favorites()
    {
        //NO lectures are visible for guest accounts
        if (DatabaseConnector.Instance.Perms != PermissonLevel.Guest)
        {
            StartCoroutine(getPinnedLectures((List<string> data) =>
            {
                int entryHeight = -200;

                for (int i = 0; i < data.Count; i = i + 3)
                {
                    Transform entryTransform = Instantiate(PinnedLectureTemplate, PinnedLectureView);
                    RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
                    entryRectTransform.anchoredPosition = new Vector2(0, -660 + entryHeight * i / 3);
                    entryTransform.gameObject.SetActive(true);

                    entryTransform.Find("Code").GetComponent<TMP_Text>().text = data[i];
                    entryTransform.Find("Name").GetComponent<TMP_Text>().text = data[i + 1];
                    entryTransform.Find("Location").GetComponent<TMP_Text>().text = data[i + 2];
                }
            }));
        }
        StartCoroutine(getPinnedEvents((List<string> data) =>
        {
            int entryHeight = -200;
            for (int i = 0; i < data.Count; i = i + 3)
            {
                Transform entryTransform = Instantiate(PinnedEventTemplate, PinnedEventView);
                RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition = new Vector2(0, -660 + entryHeight * i / 3);
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
    private IEnumerator getPinnedLectures(Action<List<string>> onCallBack)
    {
        //Currently a duplicate of a function in the lecture view side!
        string emailWithoutDot = Utilities.removeDot(queryEmail);
        var dbRoot = DatabaseConnector.Instance.Root;
        var userData = dbRoot.Child("users/" + emailWithoutDot + "/lectures").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if (userData != null)
        {
            List<string> pinnedLectures = new List<string>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                pinnedLectures.Add(x.Key.ToString());

                var lecture = dbRoot.Child("lectures").Child(x.Key.ToString()).GetValueAsync();
                yield return new WaitUntil(predicate: () => lecture.IsCompleted);

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
    private IEnumerator getPinnedEvents(Action<List<string>> onCallBack)
    {
        //Currently a duplicate of a function in the lecture view side!
        string emailWithoutDot = Utilities.removeDot(queryEmail);
        var dbRoot = DatabaseConnector.Instance.Root;
        var userData = dbRoot.Child("users/" + emailWithoutDot + "/events").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if (userData != null)
        {
            List<string> pinnedEvents = new List<string>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                pinnedEvents.Add(x.Key.ToString());

                var e1 = dbRoot.Child("events/public").Child(x.Key.ToString()).GetValueAsync();
                var e2 = dbRoot.Child("events/private").Child(x.Key.ToString()).GetValueAsync();
                // If the user is a guest, then DO NOT query the private events!
                if (DatabaseConnector.Instance.Perms == PermissonLevel.Guest)
                {
                    e2 = null;
                }
                yield return new WaitUntil(predicate: () => e1.IsCompleted & e2.IsCompleted);
                if (e1 != null & e1.Result.HasChild("name"))
                {
                    pinnedEvents.Add(e1.Result.Child("organizer").Value.ToString());
                    pinnedEvents.Add(e1.Result.Child("location").Value.ToString());
                }
                else if (e2 != null & e2.Result.HasChild("name"))
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
    private IEnumerator getDBdata(Action<List<string>> onCallBack)
    {
        List<string> userData = new List<string>();

        var dbRoot = DatabaseConnector.Instance.Root;
        var value = dbRoot.Child("users").Child(Utilities.removeDot(queryEmail)).GetValueAsync();
        yield return new WaitUntil(predicate: () => value.IsCompleted);

        if (value != null)
        {
            DataSnapshot item = value.Result;
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
    /// <returns>Action on whether the image was retrieved or not</returns>
    private IEnumerator getImage(string url, Action<bool> success = null)
    {
        // GET image from web
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Debug.Log("texture recieved!");
            // Below method must be used as resize and reinitialize only changes the container not the image!
            Texture2D scaled = new Texture2D(500, 500);
            Graphics.ConvertTexture(tex, scaled);
            // Convert to sprite and give to profileDisplay!
            Sprite displayable = Sprite.Create(scaled, new Rect(new Vector2(0, 0), new Vector2(500, 500)), new Vector2(0, 0));
            profileDisplay.sprite = displayable;
            profileEdit.sprite = displayable;
        }
        else
        {
            Debug.Log("Error retrieving profile image!");
            success.Invoke(false);
        }
        //// Clean up
        www.Dispose();
    }

    /// <summary>
    /// Function for updating the db with the new data set by the user.
    /// </summary>
    private void updateDBdata()
    {
        // TODO: implement safety feature!
        string emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);

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
        FirebaseUser user = AuthConnector.Instance.CurrentUser;

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
        if(state == 0)
        {
            DisplayCanvas.SetActive(true);
            EditCanvas.SetActive(false);
            PinnedLectureCanvas.SetActive(false);
            PinnedEventCanvas.SetActive(false);
        }
        // return to lecture scene
        else if(state == 1)
        {
            SceneManager.LoadScene("LectureScene");
        }
        // return to event scene
        else if(state == 2)
        {
            SceneManager.LoadScene("EventScene");
        }
    }

    /// <summary>
    /// Function handling the save button click on edit canvas.
    /// </summary>
    public void Save()
    {
        updateDBdata();

        StartCoroutine(getImage(profileImageLink.text, success =>
        {
            profileImageLink.text = "This image is invalid!";
            return;
        }));

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

    public void PinnedLecture()
    {
        DisplayCanvas.SetActive(false);
        EditCanvas.SetActive(false);
        PinnedLectureCanvas.SetActive(true);
        PinnedEventCanvas.SetActive(false);
        PasswordCanvas.SetActive(false);
    }

    public void PinnedEvent()
    {
        DisplayCanvas.SetActive(false);
        EditCanvas.SetActive(false);
        PinnedLectureCanvas.SetActive(false);
        PinnedEventCanvas.SetActive(true);
        PasswordCanvas.SetActive(false);
    }

    /// <summary>
    /// Function called when Email Verification button is pressed.
    /// Sends email verification email.
    /// </summary>
    public IEnumerator SendEmailVerification()
    {
        // Since the registration was success, the email verification can be sent
        Task verification = AuthConnector.Instance.CurrentUser.SendEmailVerificationAsync();
        yield return new WaitUntil(predicate: () => verification.IsCompleted);

        // If it fails, will attempt at login instead!
        if (verification.IsCompletedSuccessfully)
        {
            notificationText.text = "Email verification sent!";
        }
        else
        {
            notificationText.text = "Email verification failed! Try again later.";
        }
    }

    /// <summary>
    /// This function exists to link DeleteAccountConfirmed to the button click on unity.
    /// </summary>
    public void DeleteAccount()
    {
        DeleteAccountConfirmed();
    }

    /// <summary>
    /// This will delete all user data related to the account and kill the application.
    /// </summary>
    private IEnumerator DeleteAccountConfirmed()
    {
        var dbRoot = DatabaseConnector.Instance.Root;
        var value = dbRoot.Child("users").Child(Utilities.removeDot(queryEmail)).GetValueAsync();
        yield return new WaitUntil(predicate: () => value.IsCompleted);

        foreach (var i in value.Result.Child("friends").Children)
        {
            dbRoot.Child("users").Child(Utilities.addDot(i.Key)).Child("friends").Child(Utilities.removeDot(queryEmail)).SetValueAsync(null);
        }
        dbRoot.Child("users").Child(Utilities.removeDot(queryEmail)).SetValueAsync(null);
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
        if (PinnedLectureCanvas.activeSelf)
        {
            SceneManager.LoadScene("LectureScene");
            LectureManager.defaultSearchOption = "code";
            LectureManager.defaultSearchString = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.transform.Find("Code").GetComponent<TMP_Text>().text;
        }
        // if pinned event screen is true, then redirect entry clicked to event scene
        else
        {
            SceneManager.LoadScene("EventScene");
            EventManager.defaultSearchOption = "name";
            EventManager.defaultSearchString = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.transform.Find("Name").GetComponent<TMP_Text>().text;
        }
    }
}