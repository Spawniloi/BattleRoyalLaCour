using UnityEngine;
using static UnityEngine.GraphicsBuffer;
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
        //PickNewTarget();
        ChooseNewTarget();


    }


    void Update()
    {
        if ( _unit.isControlled || !_unit.isAlive ) return;
        if (_unit.HasBall) return;
        Patrol();
    }

    private void Patrol()
    {
        if (Vector2.Distance(transform.position, targetPosition) < 0.3f)
        {
            //PickNewTarget();
            ChooseNewTarget();
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);

        // Rotate to Movement
        
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void PickNewTarget()
    {
        Vector2 random = Random.insideUnitCircle * 4f; // Random.insideUnitCircle will be replace by the Team Area in the future
        targetPosition = (Vector2)transform.position + random;
    }
    private void ChooseNewTarget()
    {
        targetPosition = _unit.zone.GetRandomPoint();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPosition, 0.2f);
    }
}
