using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// Controller class that manages displaying and updating the location of building pins on the map.
/// Author: Waseef Nayeem
/// Date: 20203-11-22
/// </summary>
public class SpawnOnMap : MonoBehaviour
{
    [FormerlySerializedAs("_map")] [SerializeField] private AbstractMap map;

    [FormerlySerializedAs("_locationStrings")] [SerializeField] [Geocode] private string[] locationStrings;

    private Vector2d[] _locations;

    [FormerlySerializedAs("_spawnScale")] [SerializeField] private float spawnScale = 100f;

    [FormerlySerializedAs("_markerPrefab")] [SerializeField] private GameObject markerPrefab;

    private List<GameObject> _spawnedObjects;
    private List<TargetBuilding> _buildingData;

    private void Start()
    {
        _locations = new Vector2d[locationStrings.Length];
        _spawnedObjects = new List<GameObject>();

        _buildingData = new List<TargetBuilding>();
        for (var i = 0; i < locationStrings.Length; i++)
        {
            var locationString = locationStrings[i];
            _locations[i] = Conversions.StringToLatLon(locationString);
            var instance = Instantiate(markerPrefab);
            instance.transform.localPosition = map.GeoToWorldPosition(_locations[i]);
            instance.transform.localScale = new Vector3(spawnScale, spawnScale, spawnScale);
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
            spawnedObject.transform.localPosition = map.GeoToWorldPosition(location);
            spawnedObject.transform.localScale = new Vector3(spawnScale, spawnScale, spawnScale);
            data.buildingCoords = location;
        }
    }
}