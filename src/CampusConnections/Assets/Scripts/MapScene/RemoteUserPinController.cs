using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;

/// <summary>
/// Controller class for managing remote user pins on the map. Receives locations from the server and displays
/// them as pins on the map.
/// Author: Waseef Nayeem
/// Date: 2024-03-15
/// </summary>
public class RemoteUserPinController : MonoBehaviour
{
    [SerializeField] private AbstractMap map;
    [SerializeField] private LocationHubService locationService;

    private string _email;
    private DatabaseReference _dbRoot;

    private Rect _geoFence;
    private Dictionary<string, Vector2d> _remoteLocations;
    private Dictionary<string, GameObject> _remotePins;
    private Dictionary<string, DateTime> _lastUpdated;
    private Queue<Tuple<string, Vector2d>> _pinUpdates;
    private List<string> _friendEmails;

    private bool _isInitialized;
    private bool _sendLocationEnabled;

    private float _elapsed;
    private const float Period = 1f;

    private ILocationProvider _locationProvider;

    private ILocationProvider LocationProvider =>
        _locationProvider ??= LocationProviderFactory.Instance.DefaultLocationProvider;

    private void Start()
    {
        // Geofence for restricting pins to the confines of McMaster Campus
        _geoFence = new Rect
        {
            x = 43.25810845308126f,
            y = -79.92312479137712f,
            xMax = 43.2637823403875f,
            yMax = -79.9161072211226f
        };

        _remoteLocations = new Dictionary<string, Vector2d>();
        _lastUpdated = new Dictionary<string, DateTime>();
        _remotePins = new Dictionary<string, GameObject>();
        _pinUpdates = new Queue<Tuple<string, Vector2d>>();
        _friendEmails = new List<string>();

        _email = FirebaseAuth.DefaultInstance.CurrentUser.Email;
        _dbRoot = FirebaseDatabase.DefaultInstance.RootReference;

        LocationProviderFactory.Instance.mapManager.OnInitialized += () => _isInitialized = true;

        locationService.NewLocationReceived.AddListener(OnReceivedListener);

        _sendLocationEnabled = true;

        StartCoroutine(GetFriendsList());
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        // Receive updated locations from queue
        while (_pinUpdates.Count > 0)
        {
            var p = _pinUpdates.Dequeue();
            _remoteLocations[p.Item1] = p.Item2;
        }

        // Update pin object locations
        foreach (var loc in _remoteLocations)
            if (_remotePins.TryGetValue(loc.Key, out var pin))
            {
                Debug.Log(loc.Value);
                pin.transform.position = map.GeoToWorldPosition(loc.Value);
            }
            else
            {
                var p = PinObjectPool.SharedInstance.GetPooledObject();
                if (p != null)
                {
                    p.transform.position = map.GeoToWorldPosition(loc.Value);
                    p.SetActive(true);

                    _remotePins[loc.Key] = p;
                }
            }

        // Clear inactive pins
        foreach (var email in _lastUpdated.Keys)
            if (_remoteLocations.ContainsKey(email) && _remotePins.ContainsKey(email))
            {
                var time = _lastUpdated[email];
                if (DateTime.Now - time >= TimeSpan.FromSeconds(60))
                {
                    _remoteLocations.Remove(email);
                    _remotePins[email].SetActive(false);
                    _remotePins.Remove(email);
                }
            }

        // Send the local user's current location once per time period
        if (_elapsed >= Period)
        {
            _elapsed %= Period;
            SendLocation();
        }
    }

    /// <summary>
    /// Handler function that is called when a location is received.
    /// </summary>
    /// <remarks>
    /// Incoming locations must be added to queue.
    /// This is because the UI cannot be updated from inside an event handler.
    /// </remarks>
    /// <param name="loc">Location object received from remote user.</param>
    private void OnReceivedListener(RemoteUserLocation loc)
    {
        // Only keep friend locations
        if (_friendEmails.Contains(loc.Email))
        {
            _pinUpdates.Enqueue(new Tuple<string, Vector2d>(loc.Email, new Vector2d(loc.Latitude, loc.Longitude)));
            _lastUpdated[loc.Email] = DateTime.Now;
        }
    }
    
    private async void SendLocation()
    {
        if (!_isInitialized || !_sendLocationEnabled) return;
        var location = LocationProvider.CurrentLocation.LatitudeLongitude;
        if (!_geoFence.Contains(new Vector2((float)location.x, (float)location.y))) return;

        var packet = new RemoteUserLocation
            { Latitude = (float)location.x, Longitude = (float)location.y, Email = _email };
        await locationService.SendAsync(packet);
    }

    /// <summary>
    /// Helper coroutine function for getting the user's list of friends.
    /// This is to enforce the privacy requirement that only allows friend locations to be displayed.
    /// </summary>
    private IEnumerator GetFriendsList()
    {
        var emailWithoutDot = Utilities.RemoveDot(_email);
        var userData = _dbRoot.Child("users/" + emailWithoutDot + "/friends").GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);

        if (userData != null)
        {
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                var email = Utilities.AddDot(x.Key);
                _friendEmails.Add(email);
            }
        }
    }
}