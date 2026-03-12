using UnityEngine;
using System.Collections.Generic;

public class SnakeBodyFollow : MonoBehaviour
{
    public Transform _head;
    public List<Transform> _segments = new List<Transform>();
    public float _followDistance;

    void Update()
    {
        Vector3 previousPosition = _head.position;

        foreach (Transform segment in _segments)
        {
            float distance = Vector3.Distance(segment.position, previousPosition);
            if (distance > _followDistance)
            {
                Vector3 direction = (previousPosition - segment.position).normalized;
                segment.position += direction * (distance - _followDistance);
            }
            previousPosition = segment.position;
        }

    }
}
