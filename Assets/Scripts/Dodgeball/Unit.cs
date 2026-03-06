using UnityEngine;

public class Unit : MonoBehaviour
{
    PlayerController playerController;
    public int teamID;
    public TeamZones zone;

    public bool isControlled = false;
    public bool isAlive = true;

    // BALL
    public Ball heldBall;
    public bool HasBall => heldBall != null;
    private void Start()
    {
        TeamZones[] zones = FindObjectsOfType<TeamZones>();

        foreach (TeamZones z in zones)
        {
            if (z.TeamID == teamID)
            {
                zone = z;
                break;
            }
        }
    }


    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if(!isAlive) return;
        playerController.enabled = isControlled;
    }
    
    public void SetControlled(bool value)
    {
        isControlled = value;
    }


}
