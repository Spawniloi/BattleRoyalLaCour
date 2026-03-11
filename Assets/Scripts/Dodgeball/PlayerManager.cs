using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] LayerMask unitLayer;
    // Unit List
    public List<Unit> units = new List<Unit>();
    int currentUnitIndex = 0;
    public Unit CurrentUnit => units[currentUnitIndex];
    
    // Teams
    public int teamID;

    //Input
    Vector2 currentMoveInput;

    private void Start()
    {
        var playerInput = GetComponentInChildren<PlayerInput>();

        Initialize();
        TeamInitialize();
    }

    #region PLAYER_INPUTS_REGION
    public void OnThrow(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.currentState != GameState.Playing) return;

        if (!context.performed) return;
        ThrowBall();
    }

    public void OnSwitch(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.currentState != GameState.Playing) return;

        if (!context.performed) return;
        SwitchUnit();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.currentState != GameState.Playing) return;

        currentMoveInput = context.ReadValue<Vector2>();
        if (CurrentUnit == null) return;

        PlayerController controller = CurrentUnit.GetComponent<PlayerController>();
        controller.SetMoveInput(currentMoveInput);
    }
    #endregion

    public void Initialize()
    {
        PlayerInput input = GetComponentInChildren<PlayerInput>(); 
        if (input.playerIndex == 0) teamID = 0;
        if (input.playerIndex == 1) teamID = 1;
    }

    private void TeamInitialize()
    {
        Unit[] allUnits = GetComponentsInChildren<Unit>();

        foreach (Unit u in allUnits)
        {
            u.teamID = teamID;
            u.ApplyTeamColor(); // Apply Unit Color
            units.Add(u);
        }
        if (units.Count > 0) SetControlledUnit(units[0]);
    }

    public void SwitchUnit()
    {   
        Unit target = GetUnitInFront();

        if(target == null) return;
        
        CurrentUnit.SetControlled(false);
        currentUnitIndex = units.IndexOf(target);
        CurrentUnit.SetControlled(true);

        // Transfer Current MoveInput to the new Unit
        CurrentUnit.GetComponent<PlayerController>().SetMoveInput(currentMoveInput);
    }

    
    Unit GetUnitInFront()
    {
        Unit current = CurrentUnit;
        Vector2 direction = current.GetComponent<PlayerController>().lastDirection;
        Vector2 origin = (Vector2)current.transform.position + direction * 1f; 

        // --- RAY CAST ---
        RaycastHit2D hit = Physics2D.CircleCast(origin, 0.5f, direction, 3f, unitLayer);
        Debug.DrawRay(origin, direction * 3f, Color.green);
        
        
        if(hit.collider == null) return null;

        Unit unit = hit.collider.GetComponent<Unit>();
        if (unit != null) print($"j'ai grab qq : {unit.name}");

        if (unit != null && unit.teamID == current.teamID && unit != CurrentUnit)
        { return unit; }
        
        return null;
    }

    void ThrowBall()
    {

        Unit unit = CurrentUnit;
        if (!unit.HasBall) return;

        Ball ball = unit.heldBall;
        ball.OwnerTeam = unit.teamID; 

        Vector2 direction = unit.GetComponent<PlayerController>().lastDirection;

        ball.Throw(direction);
        unit.heldBall = null;

        if (GameManager.Instance.teamScores.ContainsKey(unit.teamID))
        {
            GameManager.Instance.teamScores[unit.teamID].ballsThrown++;
        }
    }

    public void SwitchToClosestUnit(Unit deadUnit)
    {
        Unit[] units = FindObjectsOfType<Unit>();

        Unit closest = null;
        float dist = Mathf.Infinity;

        foreach (Unit u in units)
        {
            if (u == deadUnit) continue;
            if (u.teamID != deadUnit.teamID) continue;

            float d = Vector2.Distance(deadUnit.transform.position, u.transform.position);

            if (d < dist)
            {
                dist = d;
                closest = u;
            }
        }

        if (closest != null) SetControlledUnit(closest);
    }


    public void SetControlledUnit(Unit closest)
    {
        if (CurrentUnit != null) CurrentUnit.SetControlled(false); else return;
        currentUnitIndex = units.IndexOf(closest);
        CurrentUnit.SetControlled(true);
    }



    // EDITOR CHEATS
    void Kill(Unit unit)
    {
        unit.isAlive = false;

        print($"{unit.name} is Killed !");
        SwitchToClosestUnit(unit);

        Destroy(unit.gameObject);
    }

    private void DebugDeath()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Unit deadUnit = CurrentUnit;
            Kill(deadUnit);
        }
    }
}
