using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private GameObject targetActions;
    [SerializeField] private GameObject notInRangePanel;
    
    private AbstractLocationProvider _locationProvider = null;
    private Location userLocation;
    
    private bool isPanelActive;
    private TargetBuilding currentBuilding;

    public int maximumDistance = 30;
    
    void Start()
    {
        BuildingClickHandler.PointerClickAction += DisplayTargetEvents;
        
        if (null == _locationProvider)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider as AbstractLocationProvider;
        }
    }
    
    void Update()
    { 
        userLocation = _locationProvider.CurrentLocation;
    }

    public void DisplayTargetEvents(GameObject target)
    {
        if (!isPanelActive)
        {
            currentBuilding = target.GetComponent<TargetBuilding>();
            targetActions.SetActive(true);
            isPanelActive = true;
        }
    }

    public void OnViewButtonClick()
    {
        Debug.Log("Welcome to " + currentBuilding.buildingName);
        SceneManager.LoadScene("ActivityPage");
    }

    public void onARButtonClick()
    {
        var currentPlayerLocation = new GeoCoordinatePortable.GeoCoordinate(userLocation.LatitudeLongitude.x, userLocation.LatitudeLongitude.y);
        var targetLocation = new GeoCoordinatePortable.GeoCoordinate(currentBuilding.buildingCoords.x, currentBuilding.buildingCoords.y);
        var distance = currentPlayerLocation.GetDistanceTo(targetLocation);
        
        Debug.Log(currentPlayerLocation);
        Debug.Log(targetLocation);
        Debug.Log("Distance is " + distance);
        
        if (distance > maximumDistance) {
            targetActions.SetActive(false);
            notInRangePanel.SetActive(true);
        } else {
            Debug.Log("AR Camera Mode");
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
