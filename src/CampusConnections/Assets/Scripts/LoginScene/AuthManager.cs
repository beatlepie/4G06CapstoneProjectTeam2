using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using TMPro;
using System.Threading.Tasks;
using Auth;
using Database;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")] public static FirebaseUser User;

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

    [Header("Notificiation")] [SerializeField]
    private GameObject Notification;

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

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = AuthConnector.Instance.Auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(() => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning($"Failed to register task with {LoginTask.Exception}");
            var firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
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
            Notification.SetActive(true);
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result.User;
            AuthConnector.Instance.IsEmailVerified = User.IsEmailVerified;
            var getPerms = DatabaseConnector.Instance.Root.Child("users/" + Utilities.removeDot(User.Email) + "/perms")
                .GetValueAsync();
            yield return new WaitUntil(() => getPerms.IsCompleted);
            AuthConnector.Instance.Perms = (PermissonLevel)int.Parse(getPerms.Result.Value.ToString());

            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            notificationText.text = "";
            SceneManager.LoadScene("MenuScene");
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            notificationText.text = "<color=#F14141>Missing Username";
            Notification.SetActive(true);
        }
        else if (Utilities.containSpecialChar(_username))
        {
            notificationText.text = "<color=#F14141>Username Cannot Contain Special Characters!";
            Notification.SetActive(true);
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            notificationText.text = "<color=#F14141>Password Does Not Match!";
            Notification.SetActive(true);
        }
        else if (consentAgreement.isOn == false)
        {
            //If the user does not agree to the user consent
            notificationText.text = "<color=#F14141>Please read and agree to the consent to create an account.";
            Notification.SetActive(true);
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = AuthConnector.Instance.Auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(() => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning($"Failed to register task with {RegisterTask.Exception}");
                var firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
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
                Notification.SetActive(true);
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result.User;

                if (User != null)
                {
                    //Create a user profile and set the username
                    var profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(() => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning($"Failed to register task with {ProfileTask.Exception}");
                        var firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        notificationText.text = "<color=#F14141>Username Set Failed!";
                        Notification.SetActive(true);
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginScreen();
                        var user = new User(User.Email);
                        user.nickName = User.DisplayName;
                        var emailWithoutDot = Utilities.removeDot(User.Email);
                        DatabaseConnector.Instance.Root.Child("users").Child(emailWithoutDot)
                            .SetRawJsonValueAsync(JsonUtility.ToJson(user));
                        notificationText.text = "";

                        // Since the registration was success, the email verification can be sent
                        var verification = User.SendEmailVerificationAsync();
                        yield return new WaitUntil(() => verification.IsCompleted);

                        // If it fails, will attempt at login instead!
                        if (verification.IsCompletedSuccessfully)
                            notificationText.text = "Email verification sent!";
                        else
                            notificationText.text = "Email verification will be sent on login!";
                    }
                }
            }
        }
    }

    private IEnumerator ForgetPassword(string _email)
    {
        var ResetPwdTask = AuthConnector.Instance.Auth.SendPasswordResetEmailAsync(_email);
        yield return new WaitUntil(() => ResetPwdTask.IsCompleted);
        if (ResetPwdTask.IsFaulted)
        {
            Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + ResetPwdTask.Exception);
            notificationText.text = "<color=#F14141>SendPasswordResetEmailAsync encountered an error.";
            Notification.SetActive(true);
        }
        else if (ResetPwdTask.IsCanceled)
        {
            Debug.LogError("SendPasswordResetEmailAsync was canceled.");
            notificationText.text = "<color=#F14141>SendPasswordResetEmailAsync was canceled.";
            Notification.SetActive(true);
        }
        else
        {
            notificationText.text = "Password reset email sent successfully.";
            Notification.SetActive(true);
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