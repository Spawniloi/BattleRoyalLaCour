using UnityEngine;

public class Unit : MonoBehaviour
{
    public int teamID;

    public bool isControlled = false;
    public bool isAlive = true;

    PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if(!isAlive)
            return;
        playerController.enabled = isControlled;
    }
    
    public void SetControlled(bool value)
    {
        isControlled = value;
    }
}
