using UnityEngine;
using Mapbox.Utils;

/// <summary>
/// Controller class for the building pin prefab. Keeps track of the location and name of the building.
/// Controls the animation of the building pin.
/// </summary>
public class TargetBuilding : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float amplitude = 1.0f;
    [SerializeField] private float frequency = 0.5f;

    public Vector2d buildingCoords;
    public string buildingName;
    
    private void Update()
    {
        FloatAndSpin();
    }

    /// <summary>
    /// Animate the building pin to float up and down and rotate about the Y axis.
    /// </summary>
    private void FloatAndSpin()
    {
        Transform rotatedTransform;
        (rotatedTransform = transform).Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        var position = rotatedTransform.position;
        position = new Vector3(position.x,
            Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude + 10, position.z);

        transform.position = position;
    }
}