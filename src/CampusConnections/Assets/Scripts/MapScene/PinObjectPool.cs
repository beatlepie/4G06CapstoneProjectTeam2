using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls an object pool of remote user pin prefabs.
/// Author: Waseef Nayeem
/// Date: 2024-03-15
/// </summary>
public class PinObjectPool : MonoBehaviour
{
    public static PinObjectPool SharedInstance;
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public GameObject parentObj;
    public int amountToPool;

    private void Awake()
    {
        SharedInstance = this;
    }

    private void Start()
    {
        pooledObjects = new List<GameObject>();
        GameObject tmp;
        for (var i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(objectToPool, parentObj.transform);
            tmp.SetActive(false);
            pooledObjects.Add(tmp);
        }
    }

    public GameObject GetPooledObject()
    {
        for (var i = 0; i < amountToPool; i++)
            if (!pooledObjects[i].activeInHierarchy)
                return pooledObjects[i];
        return null;
    }
}