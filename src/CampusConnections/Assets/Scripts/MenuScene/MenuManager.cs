using Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using Auth;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject LecturesButton;
    [SerializeField] private GameObject ARButton;

    private void Awake()
    {
        if (AuthConnector.Instance.Perms == PermissonLevel.Guest)
        {
            LecturesButton.SetActive(false);
            ARButton.SetActive(false);
        }
    }

    public void Lectures()
    {
        SceneManager.LoadScene("LectureScene");
        LectureManager.defaultSearchOption = null;
        LectureManager.defaultSearchString = null;
    }

    public void Events()
    {
        SceneManager.LoadScene("EventScene");
        EventManager.defaultSearchOption = null;
        EventManager.defaultSearchString = null;
    }

    public void Map()
    {
        SceneManager.LoadScene("MapScene");
    }

    public void Friends()
    {
        SceneManager.LoadScene("FriendScene");
    }

    public void AR()
    {
        SceneManager.LoadScene("ARCameraScene");
    }

    public void Settings()
    {
        SceneManager.LoadScene("SettingsScene");
        SettingsManager.currentUser = true;
        SettingsManager.state = 0;
    }

    public void Quit()
    {
        Application.Quit();
    }
}