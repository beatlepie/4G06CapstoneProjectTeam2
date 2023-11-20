using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Examples;
using Mapbox.Utils;

public class TargetBuilding : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] float amplitude = 1.0f;
    [SerializeField] float frequency = 0.5f;

    LocationStatus playerLocation;
    public Vector2d targetPos;
    public string targetName;
    ClickManager clickManager;
    // Start is called before the first frame update
    void Start()
    {
        clickManager = GameObject.Find("ClickEventCanvas").GetComponent<ClickManager>();
    }

    // Update is called once per frame
    void Update()
    {
        FloatandSpin();
    }

    void FloatandSpin()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, (Mathf.Sin(Time.fixedTime*Mathf.PI*frequency)*amplitude + 10), transform.position.z);
    }

    private void OnMouseDown()
    {
        playerLocation = GameObject.Find("Canvas").GetComponent<LocationStatus>();
        var currentPlayerLocation = new GeoCoordinatePortable.GeoCoordinate(playerLocation.GetLocationLat(), playerLocation.GetLocationLon());
        var targetLocation = new GeoCoordinatePortable.GeoCoordinate(targetPos[0], targetPos[1]);
        var distance = currentPlayerLocation.GetDistanceTo(targetLocation);
        clickManager.DisplayTargetEvents(targetName, (double)distance);
    }
}
