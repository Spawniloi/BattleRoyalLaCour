using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
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

    void OnEnable()
    {
        controls.Enable();
        controls.Player.Switch.performed += ctx => SwitchUnit();
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
        /*
        units[currentUnitIndex].SetControlled(false);
        currentUnitIndex++;
        if(currentUnitIndex >= units.Count)
            currentUnitIndex = 0;
        units[currentUnitIndex].SetControlled(true);
        */
        
        Unit target = GetUnitInFront();

        if(target == null)
            return;
        
        CurrentUnit.SetControlled(false);
        print("OMG IM HERE");
        currentUnitIndex = units.IndexOf(target);
        CurrentUnit.SetControlled(true);
    }
    
    Unit GetUnitInFront()
    {
        Unit current = CurrentUnit;
        Vector2 direction = current.GetComponent<PlayerController>().lastDirection;
        Vector2 origin = (Vector2)current.transform.position + direction * 1f;
        //Vector2 origin = current.transform.position;
        

        //RaycastHit2D hit = Physics2D.Raycast(origin, direction, 3f);
        RaycastHit2D hit = Physics2D.CircleCast(origin, 0.5f, direction, 3f);
        
        
        if(hit.collider == null)
            return null;

        Unit unit = hit.collider.GetComponent<Unit>();
        print($"Omg j'ai grab qq : {unit.name}" );

        if (unit != null && unit.teamID == current.teamID && unit != CurrentUnit)
        {
            return unit;
        }    
        return null;
    }
}
