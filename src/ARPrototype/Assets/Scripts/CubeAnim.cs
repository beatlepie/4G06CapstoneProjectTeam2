using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeAnim : MonoBehaviour
{

    public Vector3 rotationAmount;
    
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationAmount * Time.deltaTime);
    }
}
