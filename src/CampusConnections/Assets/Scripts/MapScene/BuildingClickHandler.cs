using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for handling click actions on building pins.
/// Author: Waseef Nayeem
/// Date: 2023-11-22
/// </summary>
public class BuildingClickHandler : MonoBehaviour, IPointerClickHandler
{
    public static System.Action<GameObject> PointerClickAction;

    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClickAction?.Invoke(gameObject);
    }
}