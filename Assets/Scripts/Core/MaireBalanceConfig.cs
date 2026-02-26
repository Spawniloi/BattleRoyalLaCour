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
    public float transfertCooldownDuree = 1.5f; // était hardcodé 1.5f
    public bool speedResetOnTransfer = false;

    [Header("Knockback")]
    public float knockbackTransfert = 14.0f; // entre 2 joueurs au transfert
    public float knockbackPoissonPoisson = 5.0f; // entre 2 poissons
    public float knockbackCorailleBody = 10.0f; // rebond sur corps coraille
    public float knockbackMultiplier = 1.3f;  // multiplicateur réflexion

    [Header("Corailles")]
    public float corailleEntryAngle = 60f;
    public float corailleRepulsionForce = 6.0f;
    public float stunDuration = 0.3f;
    public float corailleCooldown = 3.5f;
    public float propulsionForce = 8.0f;
    public float corailleSwapDelay = 0.5f;
    public float corailleRebondForceMin = 5.0f; // force min rebond trigger

    [Header("Terrain")]
    public Vector2 terrainSize2J = new Vector2(12f, 7f);
    public Vector2 terrainSize3J = new Vector2(15f, 8f);
    public Vector2 terrainSize4J = new Vector2(17f, 9f);
    public float distanceMinCorailles = 3.0f; // distance min entre corailles

    [Header("Temps & Score")]
    public float roundDuration = 90f;
    public float mayorTimeTickRate = 0.1f;
    public float corailleCooldownDuration = 3.5f;

    [Header("UI Slider")]
    public float sliderValeurMax = 10f; // valeur max affichée sur le slider

    // ── Calcul vitesse selon slider ───────────────────────────────────────────
    public float GetSpeedFromSlider(float sliderValue)
    {
        if (sliderValue >= 0)
        {
            // Plus le slider monte, plus on est rapide
            float t = Mathf.Clamp01(sliderValue / sliderValeurMax);
            float bonus = Mathf.Pow(t, sliderExpCurve) * maxSpeedBonus;
            return baseSpeed + bonus;
        }
        else
        {
            // Plus le slider descend, plus on est lent
            float t = Mathf.Clamp01(-sliderValue / sliderValeurMax);
            float penalty = Mathf.Pow(t, sliderExpCurve) * maxSpeedPenalty;
            return Mathf.Max(0.5f, baseSpeed - penalty);
        }
    }

    // ── Taille terrain ────────────────────────────────────────────────────────
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