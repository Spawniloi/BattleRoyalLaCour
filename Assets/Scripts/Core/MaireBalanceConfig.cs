using UnityEngine;

[CreateAssetMenu(fileName = "MaireBalanceConfig",
                 menuName = "BattleRoyal/Maire Balance Config")]
public class MaireBalanceConfig : ScriptableObject
{
    [Header("Vitesse & Slider")]
    public float baseSpeed = 4.0f;
    public float sliderExpCurve = 1.5f;
    public float maxSpeedBonus = 3.0f;
    public float maxSpeedPenalty = 2.0f;
    public float sliderRiseRate = 0.15f;
    public float sliderFallRate = 0.10f;

    [Header("Effet Glace")]
    public float iceAcceleration = 8.0f;
    public float iceDeceleration = 4.0f;
    public float iceTurnSpeed = 6.0f;

    [Header("Transfert & Freeze")]
    public float freezeDuration = 1.0f;
    public float mayorTransferRadius = 0.5f;
    public bool speedResetOnTransfer = false;

    [Header("Corailles")]
    public float corailleEntryAngle = 60f;
    public float corailleRepulsionForce = 6.0f;
    public float stunDuration = 0.3f;
    public float corailleCooldown = 3.5f;
    public float propulsionForce = 8.0f;
    public float corailleSwapDelay = 0.5f;

    [Header("Terrain")]
    public Vector2 terrainSize2J = new Vector2(12f, 7f);
    public Vector2 terrainSize3J = new Vector2(15f, 8f);
    public Vector2 terrainSize4J = new Vector2(17f, 9f);

    [Header("Temps & Score")]
    public float roundDuration = 90f;
    public float mayorTimeTickRate = 0.1f;
    public float corailleCooldownDuration = 3.5f;

    // Calcule la vitesse selon la valeur du slider
    public float GetSpeedFromSlider(float sliderValue)
    {
        if (sliderValue >= 0)
        {
            float bonus = Mathf.Pow(sliderValue, sliderExpCurve) * (maxSpeedBonus / Mathf.Pow(10f, sliderExpCurve));
            return baseSpeed + bonus;
        }
        else
        {
            float penalty = Mathf.Pow(-sliderValue, sliderExpCurve) * (maxSpeedPenalty / Mathf.Pow(10f, sliderExpCurve));
            return Mathf.Max(0.5f, baseSpeed - penalty);
        }
    }

    // Retourne la taille du terrain selon le nombre de joueurs
    public Vector2 GetTerrainSize(int playerCount)
    {
        return playerCount switch
        {
            2 => terrainSize2J,
            3 => terrainSize3J,
            _ => terrainSize4J,
        };
    }
}