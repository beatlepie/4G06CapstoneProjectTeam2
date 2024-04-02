using System;
using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class SpawnOnMap : MonoBehaviour
{
    [SerializeField] private AbstractMap _map;

    [SerializeField] [Geocode] private string[] _locationStrings;

    private Vector2d[] _locations;

    [SerializeField] private float _spawnScale = 100f;

    [SerializeField] private GameObject _markerPrefab;

    private List<GameObject> _spawnedObjects;
    private List<TargetBuilding> _buildingData;

    private void Start()
    {
        _locations = new Vector2d[_locationStrings.Length];
        _spawnedObjects = new List<GameObject>();

        _buildingData = new List<TargetBuilding>();
        for (var i = 0; i < _locationStrings.Length; i++)
        {
            var locationString = _locationStrings[i];
            _locations[i] = Conversions.StringToLatLon(locationString);
            var instance = Instantiate(_markerPrefab);
            instance.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);
            instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            var data = instance.GetComponent<TargetBuilding>();
            data.buildingCoords = _locations[i];
            data.buildingName = BuildingLocation.Name[locationString];

            _spawnedObjects.Add(instance);
            _buildingData.Add(data);
        }
    }

    private void Update()
    {
        var count = _spawnedObjects.Count;
        for (var i = 0; i < count; i++)
        {
            var spawnedObject = _spawnedObjects[i];
            var data = _buildingData[i];
            var location = _locations[i];
            spawnedObject.transform.localPosition = _map.GeoToWorldPosition(location, true);
            spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            data.buildingCoords = location;
        }
    }
}