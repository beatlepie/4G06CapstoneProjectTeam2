using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using TMPro;
using Auth;
using Database;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")] private static FirebaseUser _user;

    //Login variables
    [Header("Login")] public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_InputField forgetEmailField;

    //Register variables
    [Header("Register")] public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public Toggle consentAgreement;

    [FormerlySerializedAs("Notification")] [Header("Notification")] [SerializeField]
    private GameObject notification;

    public TMP_Text notificationText;

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void ForgetPasswordButton()
    {
        forgetEmailField.text = "";
    }

    public void ForgetPasswordSubmitButton()
    {
        StartCoroutine(ForgetPassword(forgetEmailField.text));
    }

    private IEnumerator Login(string email, string password)
    {
        //Call the Firebase auth signin function passing the email and password
        var loginTask = AuthConnector.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);
        //Wait until the task completes
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning($"Failed to register task with {loginTask.Exception}");
            if (loginTask.Exception.GetBaseException() is FirebaseException firebaseEx)
            {
                var errorCode = (AuthError)firebaseEx.ErrorCode;

                var message = "Wrong Password or Account does not exist!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WrongPassword:
                        message = "Wrong Password";
                        break;
                    case AuthError.InvalidEmail:
                        message = "Invalid Email";
                        break;
                    case AuthError.UserNotFound:
                        message = "Account does not exist";
                        break;
                }

                notificationText.text = message;
            }

            notification.SetActive(true);
        }
        else
        {
            //User is now logged in
            //Now get the result
            _user = loginTask.Result.User;
            AuthConnector.Instance.IsEmailVerified = _user.IsEmailVerified;
            var getPerms = DatabaseConnector.Instance.Root.Child("users/" + Utilities.RemoveDot(_user.Email) + "/perms")
                .GetValueAsync();
            yield return new WaitUntil(() => getPerms.IsCompleted);
            AuthConnector.Instance.Perms = (PermissionLevel)int.Parse(getPerms.Result.Value.ToString());

            Debug.LogFormat("User signed in successfully: {0} ({1})", _user.DisplayName, _user.Email);
            notificationText.text = "";
            SceneManager.LoadScene("MenuScene");
        }
    }

    private IEnumerator Register(string email, string password, string username)
    {
        if (username == "")
        {
            //If the username field is blank show a warning
            notificationText.text = "<color=#F14141>Missing Username";
            notification.SetActive(true);
        }
        else if (Utilities.ContainSpecialChar(username))
        {
            notificationText.text = "<color=#F14141>Username Cannot Contain Special Characters!";
            notification.SetActive(true);
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            notificationText.text = "<color=#F14141>Password Does Not Match!";
            notification.SetActive(true);
        }
        else if (consentAgreement.isOn == false)
        {
            //If the user does not agree to the user consent
            notificationText.text = "<color=#F14141>Please read and agree to the consent to create an account.";
            notification.SetActive(true);
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var registerTask = AuthConnector.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            //Wait until the task completes
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning($"Failed to register task with {registerTask.Exception}");
                if (registerTask.Exception.GetBaseException() is FirebaseException firebaseEx)
                {
                    var errorCode = (AuthError)firebaseEx.ErrorCode;

                    var message = "Register Failed!";
                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            message = "Missing Email";
                            break;
                        case AuthError.MissingPassword:
                            message = "Missing Password";
                            break;
                        case AuthError.WeakPassword:
                            message = "Weak Password";
                            break;
                        case AuthError.InvalidEmail:
                            message = "Invalid Email";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            message = "Email Already In Use";
                            break;
                    }

                    notificationText.text = "<color=#F14141>" + message;
                }

                notification.SetActive(true);
            }
            else
            {
                //User has now been created
                //Now get the result
                _user = registerTask.Result.User;

                if (_user != null)
                {
                    //Create a user profile and set the username
                    var profile = new UserProfile { DisplayName = username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var profileTask = _user.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(() => profileTask.IsCompleted);

                    if (profileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning($"Failed to register task with {profileTask.Exception}");
                        if (profileTask.Exception.GetBaseException() is FirebaseException firebaseEx)
                        {
                            var errorCode = (AuthError)firebaseEx.ErrorCode;
                        }

                        notificationText.text = "<color=#F14141>Username Set Failed!";
                        notification.SetActive(true);
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.Instance.LoginScreen();
                        var user = new User(_user.Email)
                        {
                            NickName = _user.DisplayName
                        };
                        var emailWithoutDot = Utilities.RemoveDot(_user.Email);
                        DatabaseConnector.Instance.Root.Child("users").Child(emailWithoutDot)
                            .SetRawJsonValueAsync(JsonUtility.ToJson(user));
                        notificationText.text = "";

                        // Since the registration was success, the email verification can be sent
                        var verification = _user.SendEmailVerificationAsync();
                        yield return new WaitUntil(() => verification.IsCompleted);

                        // If it fails, will attempt at login instead!
                        notificationText.text = verification.IsCompletedSuccessfully
                            ? "Email verification sent!"
                            : "Email verification will be sent on login!";
                    }
                }
            }
        }
    }

    private IEnumerator ForgetPassword(string email)
    {
        var resetPwdTask = AuthConnector.Instance.Auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => resetPwdTask.IsCompleted);
        if (resetPwdTask.IsFaulted)
        {
            Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + resetPwdTask.Exception);
            notificationText.text = "<color=#F14141>SendPasswordResetEmailAsync encountered an error.";
            notification.SetActive(true);
        }
        else if (resetPwdTask.IsCanceled)
        {
            Debug.LogError("SendPasswordResetEmailAsync was canceled.");
            notificationText.text = "<color=#F14141>SendPasswordResetEmailAsync was canceled.";
            notification.SetActive(true);
        }
        else
        {
            notificationText.text = "Password reset email sent successfully.";
            notification.SetActive(true);
        }
    }

    public void AgreeToConsent()
    {
        consentAgreement.isOn = true;
    }

    public void DisagreeToConsent()
    {
        consentAgreement.isOn = false;
    }
}