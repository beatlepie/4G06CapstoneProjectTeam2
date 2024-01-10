using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    private DatabaseReference db;

    [Header("Canvas")]
    // Canvas containing user information
    public GameObject DisplayCanvas;
    // Canvas handling the edit of user information
    public GameObject EditCanvas;

    [Header("Buttons")]
    // Edit button in display canvas
    public GameObject EditButton;

    [Header("Values")]
    // UserID, immutable value, for display and edit canvas
    public TMP_Text userIDDisplay;
    public TMP_Text userIDEdit;
    // User nickname, can be changed by user.
    public TMP_Text username;
    public TMP_Text program;
    public TMP_Text level;
    public TMP_Text x;

    [Header("Input Values")]
    public TMP_InputField newUsername;

    private void Start()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;
        // Display is the default view
        EditCanvas.SetActive(false);
        getDBdata();
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
    private void getDBdata()
    {


        // If the id is not the user, then remove the edit button!
        if (false)
        {
            EditButton.SetActive(false);
        }

        // These operations can be done for all users
    }

    /// <summary>
    /// Function for updating the db with the new data set by the user.
    /// </summary>
    private void updateDBdata()
    {

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
