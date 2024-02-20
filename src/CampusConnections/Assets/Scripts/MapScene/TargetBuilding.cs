using UnityEngine;
using Mapbox.Utils;

public class TargetBuilding : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] float amplitude = 1.0f;
    [SerializeField] float frequency = 0.5f;
    
    public Vector2d buildingCoords;
    public string buildingName;
    
    // Update is called once per frame
    void Update()
    {
        FloatAndSpin();
    }

    void FloatAndSpin()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, (Mathf.Sin(Time.fixedTime*Mathf.PI*frequency)*amplitude + 10), transform.position.z);
    }
}
