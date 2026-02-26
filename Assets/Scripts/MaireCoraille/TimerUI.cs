using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI labelTimer;
    private MaireGameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<MaireGameManager>();
    }

    void Update()
    {
        if (gameManager == null || labelTimer == null) return;

        float t = Mathf.Max(0, gameManager.tempsRestant);
        int minutes = (int)(t / 60);
        int seconds = (int)(t % 60);

        labelTimer.text = $"{minutes:00}:{seconds:00}";

        // Rouge dans les 10 dernières secondes
        labelTimer.color = t <= 10f
            ? new Color(0.9f, 0.2f, 0.2f)
            : Color.white;
    }
}