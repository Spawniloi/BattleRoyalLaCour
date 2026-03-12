using UnityEngine;
using static GameManager;

public class UnitAI : MonoBehaviour
{
    Unit _unit;

    public float patrolSpeed = 1.5f;
    Vector2 targetPosition;

    void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    private void Start()
    {
        ChooseNewTarget();
    }

    void Update()
    {
        if (GameManager.Instance.currentState != GameState.Playing) return;

        if (_unit.isControlled || !_unit.isAlive) return;
        if (_unit.HasBall) return;

        Patrol();
    }

    private void Patrol()
    {
        if (Vector2.Distance(transform.position, targetPosition) < 0.3f)
        {
            ChooseNewTarget();
        }

        Vector2 newPos = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);

        // Clamp
        if (_unit.zone != null)
        {
            newPos = _unit.zone.ClampPosition(newPos);
        }

        transform.position = newPos;

        // Rotation vers la cible
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void ChooseNewTarget()
    {
        if (_unit.zone != null)
        {
            targetPosition = _unit.zone.GetRandomPoint();
        }
        else
        {
            // fallback si pas de zone
            targetPosition = (Vector2)transform.position;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPosition, 0.2f);
    }
}