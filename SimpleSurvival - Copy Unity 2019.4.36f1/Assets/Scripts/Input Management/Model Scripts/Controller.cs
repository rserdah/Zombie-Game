using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    protected bool newInput;
    protected bool m_active = false;
    
    /// <summary>
    /// Returns true if the GameObject is active AND Controller.m_active is true. Setting this value however will only set Controller.m_active
    /// </summary>
    public bool active { get => gameObject.activeSelf && m_active; set => m_active = value; }
    
    public CinemachineVirtualCameraBase cinemachineCamera;



    public abstract void ReadInput(InputData data);
    public abstract void Enable(InputManager manager);
    public abstract void Disable();

    
}
