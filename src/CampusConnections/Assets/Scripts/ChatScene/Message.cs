using UnityEngine;

[System.Serializable]
public class Message
{
    [SerializeField]
    private string content = "";
    public string Content
    {
        get => content;
        set => content = value;
    }
}