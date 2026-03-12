using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Visuel Dodgeball")]
    public DodgeballVisuel dodgeballVisuel;

    PlayerController playerController;
    public int teamID;
    public TeamZones zone;
    public bool isControlled = false;
    public bool isAlive = true;

    // BALL
    public Ball heldBall;
    public bool HasBall => heldBall != null;

    // VISUAL legacy
    public GameObject selectCircle;
    private SpriteRenderer sr;
    public SpriteRenderer srCursor;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Trouve la zone selon teamID
        TeamZones[] zones = FindObjectsOfType<TeamZones>();
        foreach (TeamZones z in zones)
        {
            if (z.TeamID == teamID)
            {
                zone = z;
                break;
            }
        }

        // Applique visuel APRES tous les Awake
        PlayerData data = GameData.GetJoueur(teamID + 1);
        if (data != null && dodgeballVisuel != null)
            dodgeballVisuel.AppliquerData(data);
    }

    void Update()
    {
        if (!isAlive) return;
        playerController.enabled = isControlled;
    }

    public void AppliquerCouleurGameData(PlayerData data)
    {
        if (dodgeballVisuel != null)
            dodgeballVisuel.AppliquerData(data);
    }

    public void SetControlled(bool value)
    {
        isControlled = value;
        playerController.enabled = value;
        if (dodgeballVisuel != null)
            dodgeballVisuel.SetControle(value);
        if (selectCircle != null)
            selectCircle.SetActive(value);
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
                case 0: sr.color = Color.red; break;
                case 1: sr.color = Color.yellow; break;
                case 2: sr.color = Color.blue; break;
                case 3: sr.color = Color.green; break;
                default: sr.color = Color.white; break;
            }
        }
        if (srCursor != null)
        {
            switch (teamID)
            {
                case 0: srCursor.color = Color.red; break;
                case 1: srCursor.color = Color.yellow; break;
                case 2: srCursor.color = Color.blue; break;
                case 3: srCursor.color = Color.green; break;
                default: srCursor.color = Color.white; break;
            }
        }
    }
}