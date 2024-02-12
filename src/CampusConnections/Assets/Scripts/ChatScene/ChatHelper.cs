using UnityEngine;
using UnityEngine.SceneManagement;

public class ChatHelper : MonoBehaviour
{
    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("FriendScene");
    }
}