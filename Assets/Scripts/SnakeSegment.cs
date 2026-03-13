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

    public void ShowSegment(bool enabled)
    {
        GetComponent<SpriteRenderer>().enabled = enabled;
        GetComponent<Collider2D>().enabled = enabled;
    }

}
