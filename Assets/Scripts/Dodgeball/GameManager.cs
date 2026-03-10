using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    PlayerInputManager inputManager;
    List<InputDevice> joinedDevices = new List<InputDevice>();
    public bool allowJoin = true;

    void Awake()
    {
        Instance = this;
        inputManager = FindObjectOfType<PlayerInputManager>();
    }

    private void Update()
    {
        ManualJoin();    
    }

    private void ManualJoin()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            GameManager.Instance.JoinPlayer(Keyboard.current);
        }

        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            GameManager.Instance.JoinPlayer(Gamepad.current);
        }
    }


    public void JoinPlayer(InputDevice device)
    {
        if (!allowJoin) return;
        if (joinedDevices.Contains(device)) return;

        PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
        joinedDevices.Add(device);
    }
}

