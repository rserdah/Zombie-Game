using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    private static bool m_paused;

    public static bool paused
    {
        get => m_paused;
        
        set
        {
            m_paused = value;
            Time.timeScale = m_paused ? 0f : normalTimeScale;
            onPaused?.Invoke(m_paused);
        }
    }

    //The keyword event before Action<...> makes it so only the type that declares the event can invoke. Outside types can only subscribe/unsubscribe. They can't invoke it nor overwrite other
    //subscriptions
    public static event Action<bool> onPaused;

    private static float m_normalTimeScale = 1;

    public static float normalTimeScale { get => m_normalTimeScale; set { m_normalTimeScale = value; } }

    public GameMode gameMode;


    private void Start()
    {
        gameMode = new HordeMode(this, FindObjectOfType<SpawnEnemies>());
    }

    private void Update()
    {
        gameMode.GameLoop();
    }
}
