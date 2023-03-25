using UnityEngine;

public class UIScreen : MonoBehaviour
{
    [SerializeField]
    protected string m_title;
    public string title { get => m_title; protected set { m_title = value; } }

    public void init(UI ui)
    {
        if(title.Equals(""))
            title = gameObject.name;
    }
}
