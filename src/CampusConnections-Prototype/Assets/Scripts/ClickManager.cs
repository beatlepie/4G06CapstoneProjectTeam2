using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private GameObject targetActions;
    [SerializeField] private GameObject notInRangePanel;
    bool isPanelActive;
    string buildingName;
    double buildingDistance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayTargetEvents(string targetName, double distance)
    {
        if (!isPanelActive) {
            buildingName = targetName;
            buildingDistance = distance;
            targetActions.SetActive(true);
            isPanelActive = true;
        }
    }

    public void OnViewButtonClick()
    {
        Debug.Log("Welcome to " + buildingName);
        SceneManager.LoadScene("ActivityPage");
    }

    public void onARButtonClick()
    {
        Debug.Log("Distance is " + buildingDistance);
        if (buildingDistance > 200) {
            targetActions.SetActive(false);
            notInRangePanel.SetActive(true);
        } else {
            SceneManager.LoadScene("ActivityPage");
        }
    }

    public void onBackButtonClick()
    {
        targetActions.SetActive(true);
        notInRangePanel.SetActive(false);
    }

    public void onCloseButtonClick()
    {
        if (isPanelActive) {
            targetActions.SetActive(false);
            isPanelActive = false;
        }
    }
}
