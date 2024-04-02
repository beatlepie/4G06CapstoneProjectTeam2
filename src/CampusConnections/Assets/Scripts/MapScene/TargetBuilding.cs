using UnityEngine;
using Mapbox.Utils;

public class TargetBuilding : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float amplitude = 1.0f;
    [SerializeField] private float frequency = 0.5f;

    public Vector2d buildingCoords;
    public string buildingName;

    // Update is called once per frame
    private void Update()
    {
        FloatAndSpin();
    }

    private void FloatAndSpin()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x,
            Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude + 10, transform.position.z);
    }
}