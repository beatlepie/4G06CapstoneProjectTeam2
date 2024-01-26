using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Lectures()
    {
        SceneManager.LoadScene("LectureScene");
    }

    public void Map()
    {
        SceneManager.LoadScene("MapScene");
    }

    public void Friends()
    {
        SceneManager.LoadScene("FriendScene");
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
