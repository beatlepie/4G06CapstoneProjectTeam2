using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    private FirebaseAuth auth;
    private DatabaseReference db;

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
    // These values are used during change password
    public TMP_InputField CurrentPassword;
    public TMP_InputField NewPassword;
    public TMP_InputField ConfirmPassword;

    private void Start()
    {
        // This value should be the query user value when navigated from friend!
        auth = FirebaseAuth.DefaultInstance;
        FirebaseUser query = auth.CurrentUser;
        db = FirebaseDatabase.DefaultInstance.RootReference;
        // Display is the default view
        DisplayCanvas.SetActive(true);
        EditCanvas.SetActive(false);
        PasswordCanvas.SetActive(false);
        // If the id is not the user, then remove the edit button!
        if (auth.CurrentUser != query)
        {
            EditButton.SetActive(false);
            ChangePasswordButton.SetActive(false);
        }
        getDBdata(query);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    /// <summary>
    /// Function for calling values from the database.
    /// </summary>
    private IEnumerator getDBdata(FirebaseUser query)
    {
        var value = db.Child("Users").Child(query.Email).GetValueAsync();
        userIDDisplay.text = value.Result.Value.ToString();

        DataSnapshot item = value.Result;
        level.text = item.Child("Level").Value.ToString();

        yield return new WaitUntil(predicate: () => value.IsCompleted);
    }

    /// <summary>
    /// Function for updating the db with the new data set by the user.
    /// </summary>
    private void updateDBdata()
    {

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

        // This checks that the current password
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
                        return;
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
            return;
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
        newUsername.text = username.text;


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

        DisplayCanvas.SetActive(true);
        EditCanvas.SetActive(false);
    }
}
