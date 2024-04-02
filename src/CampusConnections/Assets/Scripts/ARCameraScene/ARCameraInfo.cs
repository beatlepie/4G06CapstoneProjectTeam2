using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARCameraInfo : MonoBehaviour
{
    public TMP_Text notificationText;
    [SerializeField] private GameObject Notification;

    private void Start()
    {
        notificationText.text =
            "<color=#F14141>Attention: When using the AR camera, please be mindful of your surroundings. Ensure you have a clear, safe area to move around in and avoid obstacles or hazards. Stay aware of your surroundings and exercise caution to prevent accidents or injury.";
        Notification.SetActive(true);
    }

    public void helpbutton()
    {
        notificationText.text =
            "AR camera only works for some buildings on campus, please check the map for more details.\n\n It is very common that you lose track of an AR object, please try from a different angle.\n\n Weather and lighting can affect the functionality of AR Camera, if you think there is a bug, please contact the maintainers at <u>campusconnectionsT2@gmail.com</u>.";
        Notification.SetActive(true);
    }

    public void OnBack()
    {
        SceneManager.LoadScene("MenuScene");
    }
}