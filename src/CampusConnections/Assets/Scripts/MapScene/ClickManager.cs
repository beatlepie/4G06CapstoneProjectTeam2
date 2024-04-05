using Auth;
using Database;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Class for handling user interactions on the map scene.
/// THis includes handling popup interface clicks and navigating back to the menu.
/// Authors: Zihao Du, Waseef Nayeem
/// Date: 2023-11-19
/// </summary>
public class ClickManager : MonoBehaviour
{
    [SerializeField] private GameObject targetActions;
    [FormerlySerializedAs("_map")] [SerializeField] private AbstractMap map;

    [FormerlySerializedAs("ARCameraButton")] [SerializeField] private GameObject arCameraButton;
    [FormerlySerializedAs("LectureButton")] [SerializeField] private GameObject lectureButton;

    private AbstractLocationProvider _locationProvider;
    private Location _userLocation;

    private bool _isPanelActive;
    private TargetBuilding _currentBuilding;

    [Header("Notification")] public TMP_Text notificationText;
    [FormerlySerializedAs("Notification")] [SerializeField] private GameObject notification;

    private void Start()
    {
        BuildingClickHandler.PointerClickAction += DisplayTargetEvents;

        if (null == _locationProvider)
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider as AbstractLocationProvider;
        //Check user permissions
        if (AuthConnector.Instance.Perms == PermissionLevel.Guest)
        {
            arCameraButton.SetActive(false);
            lectureButton.SetActive(false);
        }

        _isPanelActive = false;
    }

    private void Update()
    {
        _userLocation = _locationProvider.CurrentLocation;
        // If the userLocation is initialized already, check if the user is still on campus
        if ((_userLocation.LatitudeLongitude.x != 0) & (_userLocation.LatitudeLongitude.y != 0))
            if ((_userLocation.LatitudeLongitude.x < 43.25808) | (_userLocation.LatitudeLongitude.x > 43.26816) |
                (_userLocation.LatitudeLongitude.y < -79.92344) | (_userLocation.LatitudeLongitude.y > -79.91535))
            {
                notificationText.text =
                    "<color=#F14141>Attention: You are not on campus, for sake of your personal data, you will be disconnected from the map.";
                notification.SetActive(true);
            }
    }

    private void DisplayTargetEvents(GameObject target)
    {
        if (!_isPanelActive)
        {
            _currentBuilding = target.GetComponent<TargetBuilding>();
            targetActions.SetActive(true);
            _isPanelActive = true;
        }
    }

    public void OnViewLecButtonClick()
    {
        Debug.Log("Welcome to " + _currentBuilding.buildingName);
        SceneManager.LoadScene("LectureScene");
    }

    public void OnViewEventButtonClick()
    {
        Debug.Log("Welcome to " + _currentBuilding.buildingName);
        SceneManager.LoadScene("EventScene");
        EventManager.DefaultSearchString = _currentBuilding.buildingName;
        EventManager.DefaultSearchOption = "location";
    }

    public void OnARButtonClick()
    {
        var currentPlayerLocation =
            new GeoCoordinatePortable.GeoCoordinate(_userLocation.LatitudeLongitude.x, _userLocation.LatitudeLongitude.y);
        var targetLocation =
            new GeoCoordinatePortable.GeoCoordinate(_currentBuilding.buildingCoords.x, _currentBuilding.buildingCoords.y);
        Debug.Log(currentPlayerLocation);
        Debug.Log(targetLocation);
        Debug.Log("AR Camera Mode");
        SceneManager.LoadScene("ARCameraScene");
    }

    public void OnBackButtonClick()
    {
        targetActions.SetActive(true);
    }

    public void OnCloseButtonClick()
    {
        if (_isPanelActive)
        {
            targetActions.SetActive(false);
            _isPanelActive = false;
        }
    }

    public void OnCenterButtonClick()
    {
        map.UpdateMap(new Vector2d(_userLocation.LatitudeLongitude.x, _userLocation.LatitudeLongitude.y),
            map.AbsoluteZoom);
    }

    public void OnReturnButtonClick()
    {
        if (!_isPanelActive)
            _isPanelActive =
                true; // Prevents bug that causes app to freeze when displaying target events after returning to Map Scene
        SceneManager.LoadScene("MenuScene");
    }
}