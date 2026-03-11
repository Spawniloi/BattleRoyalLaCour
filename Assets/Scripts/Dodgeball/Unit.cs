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

    // VISUAL
    public GameObject selectCircle; // assign in inspector
    private SpriteRenderer sr;
    public SpriteRenderer srCursor;

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
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if(!isAlive) return;
        playerController.enabled = isControlled;
    }

    public void SetControlled(bool value)
    {
        isControlled = value;
        playerController.enabled = value;

        if (selectCircle != null) selectCircle.SetActive(value); 
    }

    public void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        PlayerManager manager = GetComponentInParent<PlayerManager>();
        if (manager != null) manager.SwitchToClosestUnit(this);

        GameManager.Instance.CheckVictory();

        Destroy(gameObject);
    }

    public void ApplyTeamColor()
    {
        if (sr != null)
        {
            switch (teamID)
            {
                case 0: sr.color = Color.red; break;      // P1
                case 1: sr.color = Color.yellow; break;   // P2
                case 2: sr.color = Color.blue; break;     // P3
                case 3: sr.color = Color.green; break;    // P4
                default: sr.color = Color.white; break;
            }
        }

        if (srCursor != null)
        {
            switch (teamID)
            {
                case 0: srCursor.color = Color.red; break;      // P1
                case 1: srCursor.color = Color.yellow; break;   // P2
                case 2: srCursor.color = Color.blue; break;     // P3
                case 3: srCursor.color = Color.green; break;    // P4
                default: srCursor.color = Color.white; break;
            }
        }

    }

}
