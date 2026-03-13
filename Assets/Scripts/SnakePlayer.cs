using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnakePlayer : MonoBehaviour
{

    public SnakeSegment _tail;
    public SnakeSegment _head;


    public int playerID;
    public int score = 0;
    ///public TextMeshProUGUI scoreText;
    public bool showScore = true;

    [Header("Body Prefabs")]
    public GameObject bodyPrefab1;

    public Transform tail;

    public int maxBodySegments = 3;

    public List<SnakeSegment> _bodySegments =new List<SnakeSegment>();

    void Start()
    {
        //Debug.Log("I am player" + playerID);
        //score = 0;
        //scoreText.text = name + " : " + score;

        //if(scoreText == null)
        //{
        //    scoreText = GetComponentInChildren<TextMeshProUGUI>();
        //}

        //if(scoreText != null)
        //{
        //    scoreText.gameObject.SetActive(showScore);
        //    //UpdateScoreText();
        //}

        //SetupBody();
        //Score();
        
    }
   
    //void SetupBody()
    //{
    //    if (body1 == null) body1._owner = this;
    //    if (body2 == null) body2._owner = this;
    //    if (body3 == null) body3._owner = this;
    //}
    public void RemoveBodySegment(SnakeSegment segment, SnakePlayer player)
    {
        GetComponent<SnakeIntegritiyManager>().OnSegmentLost();
        if (!_bodySegments.Contains(segment))
            return;

        //if(segment == body1) body1 = null;
        //else if(segment == body2) body2 = null;
        //else if(segment == body3) body3 = null;


        // _bodySegments.Remove(segment);
        //->
        int index = _bodySegments.IndexOf(segment);
        _bodySegments.Remove(segment);

        GetComponent<SnakeBodyFollow>()._segments.RemoveAt(index);


        if(player != null)
        {
            player.AddScore(segment.scoreValue);
        }

        //AddScore(-segment);
        //Destroy(segment.gameObject);
        segment.ShowSegment(false);
        CheckIfDead();
    }

    void CheckIfDead()
    {
        //if(body1 == null && body2 == null && body3 == null)
        //{
        //    Die();
        //}

        if (_bodySegments.Count == 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(name + " est éliminé !");
        
        GameManager.Instance.PlayerDied(this);

        //foreach(var player in GameManager.Instance.alivePlayers)
        //{
        //    player.enabled = false;
        //}

        Destroy(_head.gameObject);
        Destroy(_tail.gameObject);

        Destroy(gameObject);

    }

    public bool TryAddBodySegments()
    {
        if (_bodySegments.Count >= maxBodySegments)
            return false;
        //if (body1 == null)
        //{
        //    body1 = SpawnSegment(bodyPrefab1, slot1);
        //    return;
        //}
        //if (body2 == null)
        //{
        //    body2 = SpawnSegment(bodyPrefab2 , slot2);
        //    return;
        //}
        //if (body3 == null)
        //{
        //    body3 = SpawnSegment(bodyPrefab3, slot3);
        //    return;
        //}

        //Debug.Log("Corps complet.");

        //GameObject prefabToSpawn;

        GameObject newSegmentObj = Instantiate(bodyPrefab1, tail.position, Quaternion.identity, transform);

        SnakeSegment newSegment = newSegmentObj.GetComponent<SnakeSegment>();
        newSegment._owner = this;
        newSegment.segmentType = SnakeSegment.SegmentType.Body;

        _bodySegments.Add(newSegment);

        GetComponent<SnakeBodyFollow>()._segments.Add(newSegment.transform);//_segments.Insert(_bodySegments.Count - 1, newSegment.transform);

        AddScore(newSegment.scoreValue);
        Debug.Log("Segment ajouté!");
        return true;
    }

    public void AddScore(int amount)
    {
        score += amount;
        //if (scoreManager != null)
        //    scoreManager.AddScoreUI(playerID - 1, amount);

        ScoreManager.Instance.AddScoreUI(playerID - 1, amount);
        //scoreText.text = name + " : " + score;
        //UpdateScoreText();

        Debug.Log("Player " + playerID + " score : " + score);
        //Debug.Log(name + " score :" + score);
    }

    //void UpdateScoreText()
    //{
    //    if(scoreText != null)
    //        scoreText.text = "P" + playerID + " : " + score;
    //}


    

    //void Score()
    //{
    //    score = 0;
    //    foreach (SnakeSegment seg in _bodySegments)
    //    {
    //        score += seg.scoreValue;
    //    }
    //}

}
