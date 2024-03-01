using Database;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject LecturesButton;
    [SerializeField] GameObject ARButton;

    private void Awake()
    {
        if(DatabaseConnector.Instance.Perms == PermissonLevel.Guest)
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
    }

    public void Quit()
    {
        Application.Quit();
    }
}
