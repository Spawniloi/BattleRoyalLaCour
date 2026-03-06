using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] LayerMask unitLayer;

    // Unit List
    public List<Unit> units = new List<Unit>();
    int currentUnitIndex = 0;
    public Unit CurrentUnit => units[currentUnitIndex];
    
    // Player Input
    PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
    }

    private void Start()

    {
        Initialize();
    }

    private void Update()
    {
        DebugDeath();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Player.Switch.performed += ctx => SwitchUnit();
        controls.Player.Throw.performed += ctx => ThrowBall();
    }

    void OnDisable()
    {
        controls.Disable();
    }
    
    public void Initialize()
    {
        for(int i = 0; i < units.Count; i++)
        {
            units[i].SetControlled(false);
        }
        
        units[currentUnitIndex].SetControlled(true);
    }
    
    public void SwitchUnit()
    {   
        Unit target = GetUnitInFront();

        if(target == null) return;
        
        CurrentUnit.SetControlled(false);
        currentUnitIndex = units.IndexOf(target);
        CurrentUnit.SetControlled(true);
    }


    
    Unit GetUnitInFront()
    {
        Unit current = CurrentUnit;
        Vector2 direction = current.GetComponent<PlayerController>().lastDirection;
        Vector2 origin = (Vector2)current.transform.position + direction * 1f; // offSetForce = 1f ( + direction * 1f to prevent a self cast)
        //Vector2 origin = current.transform.position; // Self Cast Issue 
        
        // --- RAY CAST ---
        //RaycastHit2D hit = Physics2D.Raycast(origin, direction, 3f); 
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
