using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ToggleManager : MonoBehaviour
{
    public int selected;
    public GameObject option1;
    public GameObject option2;
    [FormerlySerializedAs("ChatUI")] public GameObject chatUI;
    [FormerlySerializedAs("RequestUI")] public GameObject requestUI;
    [SerializeField] private Color selectedBackground;
    [SerializeField] private Color unselectedBackground;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private Color unselectedTextColor;

    private void Start()
    {
        selected = 1;
        option1.GetComponentInChildren<TMP_Text>().color = selectedTextColor;
        option1.GetComponent<Image>().color = selectedBackground;
        option2.GetComponentInChildren<TMP_Text>().color = unselectedTextColor;
        option2.GetComponent<Image>().color = unselectedBackground;
    }

    public void SelectOption1()
    {
        if (selected == 1) return;
        selected = 1;
        option1.GetComponentInChildren<TMP_Text>().color = selectedTextColor;
        option1.GetComponent<Image>().color = selectedBackground;
        option2.GetComponentInChildren<TMP_Text>().color = unselectedTextColor;
        option2.GetComponent<Image>().color = unselectedBackground;
        chatUI.SetActive(true);
        requestUI.SetActive(false);
    }

    public void SelectOption2()
    {
        if (selected == 2) return;
        selected = 2;
        option2.GetComponentInChildren<TMP_Text>().color = selectedTextColor;
        option2.GetComponent<Image>().color = selectedBackground;
        option1.GetComponentInChildren<TMP_Text>().color = unselectedTextColor;
        option1.GetComponent<Image>().color = unselectedBackground;
        chatUI.SetActive(false);
        requestUI.SetActive(true);
    }
}