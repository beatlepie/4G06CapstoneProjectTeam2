using UnityEngine;

/// <summary>
/// A manager class for login page which handles all scene changes.
/// Author: Zihao Du
/// Date: 2023-12-06
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;

    /// <summary>
    /// Default unity function left to guarentee that there is only one instance running.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    /// <summary>
    /// Functions to change the login screen UI, handles Back button behavior.
    /// </summary>
    public void OnBackButtonClick() 
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
    }

    /// <summary>
    /// Function to change to register page UI, handles register button behavior.
    /// </summary>
    public void OnRegisterButtonClick() 
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }
}