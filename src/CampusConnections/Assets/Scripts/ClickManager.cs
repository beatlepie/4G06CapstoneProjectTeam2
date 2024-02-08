using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private GameObject targetActions;
    [SerializeField] private GameObject notInRangePanel;
    [SerializeField] private AbstractMap _map;
    
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

        isPanelActive = false;
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
        SceneManager.LoadScene("LectureScene");
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
            SceneManager.LoadScene("LectureScene");
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

    public void onCenterButtonClick()
    {
        _map.UpdateMap(new Vector2d(userLocation.LatitudeLongitude.x, userLocation.LatitudeLongitude.y), _map.AbsoluteZoom);
    }

    public void onReturnButtonClick()
    {
        if (!isPanelActive)
        {
            isPanelActive = true;
        }
        SceneManager.LoadScene("MenuScene");
    }
}
