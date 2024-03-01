using System.Collections;
using System.Collections.Generic;
using Database;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private GameObject targetActions;
    [SerializeField] private AbstractMap _map;

    [SerializeField] private GameObject ARCameraButton;
    [SerializeField] private GameObject LectureButton;

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
        //Check user permissions
        if(DatabaseConnector.Instance.Perms == PermissonLevel.Guest)
        {
            ARCameraButton.SetActive(false);
            LectureButton.SetActive(false);
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

    public void OnViewLecButtonClick()
    {
        Debug.Log("Welcome to " + currentBuilding.buildingName);
        SceneManager.LoadScene("LectureScene");
    }

    public void OnViewEventButtonClick()
    {
        Debug.Log("Welcome to " + currentBuilding.buildingName);
        SceneManager.LoadScene("EventScene");
        EventManager.defaultSearchString = currentBuilding.buildingName;
        EventManager.defaultSearchOption = "location";
    }

    public void onARButtonClick()
    {
        var currentPlayerLocation = new GeoCoordinatePortable.GeoCoordinate(userLocation.LatitudeLongitude.x, userLocation.LatitudeLongitude.y);
        var targetLocation = new GeoCoordinatePortable.GeoCoordinate(currentBuilding.buildingCoords.x, currentBuilding.buildingCoords.y);        
        Debug.Log(currentPlayerLocation);
        Debug.Log(targetLocation);
        Debug.Log("AR Camera Mode");
        SceneManager.LoadScene("ARCameraScene");
    }

    public void onBackButtonClick()
    {
        targetActions.SetActive(true);
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
            isPanelActive = true; // Prevents bug that causes app to freeze when displaying target events after returning to Map Scene
        }
        SceneManager.LoadScene("MenuScene");
    }
}
