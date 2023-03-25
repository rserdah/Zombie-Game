using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI instance { get; protected set; }

    public Dictionary<string, UIScreen> screens { get; protected set; }


    private void Awake()
    {
        if(!instance)
            instance = this;
        else
            Destroy(this);

        UIScreen u;
        screens = new Dictionary<string, UIScreen>();
        for(int i = 0; i < transform.childCount; i++)
        {
            u = transform.GetChild(i).GetComponent<UIScreen>();
            if(u)
            {
                u.init(this);
                screens.Add(u.title, u);
            }
        }

        Game.onPaused += Pause;

        //Invoke("Pause", 2f);
    }

    private void Pause(bool pause)
    {
        if(pause)
            Pause();
        //else
        //    Resume();
    }

    public void Pause()
    {
        screens["c_pause"].gameObject.SetActive(true);
        screens["c_pause"].SendMessage("Pause");
    }

    public void Resume()
    {
        screens["c_pause"].gameObject.SendMessage("Resume");
    }
}
