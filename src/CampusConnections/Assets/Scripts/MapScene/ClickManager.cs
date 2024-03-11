using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using TMPro;
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

    [Header("Notification")]
    public TMP_Text notificationText;
    [SerializeField] GameObject Notification;
    
    void Start()
    {
        BuildingClickHandler.PointerClickAction += DisplayTargetEvents;
        
        if (null == _locationProvider)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider as AbstractLocationProvider;
        }
        //Check user permissions
        if(AuthManager.perms == 0)
        {
            ARCameraButton.SetActive(false);
            LectureButton.SetActive(false);
        }

        isPanelActive = false;
    }
    
    void Update()
    { 
        userLocation = _locationProvider.CurrentLocation;
        // If the userLocation is initialized already, check if the user is still on campus
        if (userLocation.LatitudeLongitude.x != 0 & userLocation.LatitudeLongitude.y != 0)
        {
            if (userLocation.LatitudeLongitude.x < 43.25808 | userLocation.LatitudeLongitude.x > 43.26816 | userLocation.LatitudeLongitude.y < -79.92344 | userLocation.LatitudeLongitude.y > -79.91535)
            {
                notificationText.text = "<color=#F14141>Attention: You are not on campus, for sake of your personal data, you will be disconnected from the map.";
                Notification.SetActive(true);
            }
        }

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
