using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginBehavior : MonoBehaviour
{
    [SerializeField] GameObject LoginCanvas, AppCanvas;
    [SerializeField] TMP_InputField IDInputField, PasswordInputField;
    private string ID, password;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CredentialCheck()
    {
        getInput();
        //ENCRYPTION
        //DATABASE STUFF
        //A

    }

    private void getInput()
    {
        ID = IDInputField.text;
        password = PasswordInputField.text;
    }
}
