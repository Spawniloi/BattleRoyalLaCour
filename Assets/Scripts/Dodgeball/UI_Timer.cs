using UnityEngine;
using TMPro;

public class UI_Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (timerText == null ) return; 
        if (GameManager.Instance.currentState != GameManager.GameState.Playing) return; //HIDE
        float time = GameManager.Instance.GetRemainingTime();
        if (time <= 60f)
        {
            timerText.color = Color.red;
        }

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

    }


}