using Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using Auth;
using UnityEngine.Serialization;

public class MenuManager : MonoBehaviour
{
    [FormerlySerializedAs("LecturesButton")] [SerializeField] private GameObject lecturesButton;
    [FormerlySerializedAs("ARButton")] [SerializeField] private GameObject arButton;

    private void Awake()
    {
        if (AuthConnector.Instance.Perms == PermissionLevel.Guest)
        {
            lecturesButton.SetActive(false);
            arButton.SetActive(false);
        }
    }

    public void Lectures()
    {
        SceneManager.LoadScene("LectureScene");
        LectureManager.DefaultSearchOption = null;
        LectureManager.DefaultSearchString = null;
    }

    public void Events()
    {
        SceneManager.LoadScene("EventScene");
        EventManager.DefaultSearchOption = null;
        EventManager.DefaultSearchString = null;
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
        SettingsManager.CurrentUser = true;
        SettingsManager.State = 0;
    }

    public void Quit()
    {
        Application.Quit();
    }
}