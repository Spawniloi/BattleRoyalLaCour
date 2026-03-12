using UnityEngine;

public class SnakeSegment : MonoBehaviour
{
    public SnakePlayer _owner;
    public int scoreValue = 1;
    public enum SegmentType
    {
        Head,
        Body,
        Tail
    }
    public SegmentType segmentType;

}
