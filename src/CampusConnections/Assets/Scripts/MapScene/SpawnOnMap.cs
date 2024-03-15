using System;
using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class SpawnOnMap : MonoBehaviour
{
	[SerializeField]
	AbstractMap _map;

	[SerializeField]
	[Geocode]
	string[] _locationStrings;
	Vector2d[] _locations;
	
	[SerializeField]
	float _spawnScale = 100f;

	[SerializeField]
	GameObject _markerPrefab;

	List<GameObject> _spawnedObjects;
	List<TargetBuilding> _buildingData;

	void Start()
	{
		_locations = new Vector2d[_locationStrings.Length];
		_spawnedObjects = new List<GameObject>();
		
		_buildingData = new List<TargetBuilding>();
		for (int i = 0; i < _locationStrings.Length; i++)
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
		int count = _spawnedObjects.Count;
		for (int i = 0; i < count; i++)
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