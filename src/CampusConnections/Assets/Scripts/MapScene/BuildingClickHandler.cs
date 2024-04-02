using System.Collections;
using System.Collections.Generic;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingClickHandler : MonoBehaviour, IPointerClickHandler
{
    public static System.Action<GameObject> PointerClickAction;

    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClickAction?.Invoke(gameObject);
    }
}