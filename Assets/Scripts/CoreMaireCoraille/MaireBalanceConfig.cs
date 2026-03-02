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

    [Header("Coraille — Rotation aleatoire")]
    public float corailleAngleMin = -45f;  // angle min rotation
    public float corailleAngleMax = 45f;  // angle max rotation
    public float corailleDureeRotMin = 1.5f; // durée rotation min
    public float corailleDureeRotMax = 3.5f; // durée rotation max
    public float coraillePauseRotMin = 0.3f; // pause entre rotations min
    public float coraillePauseRotMax = 1.0f; // pause entre rotations max

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

    [Header("UI Icones Dash")]
    public float iconesDashTaille = 30f;  // taille en pixels de chaque icone
    public float iconesDashEspacement = 35f; // espacement entre icones

    [Header("Dash")]
    public float dashForce = 18f;  // force de l'impulsion
    public float dashDuree = 0.15f; // durée du dash (secondes)
    public float dashCooldown = 0.5f;  // cooldown entre 2 dashs
    public KeyCode dashKeyJ1 = KeyCode.LeftShift;
    public KeyCode dashKeyJ2 = KeyCode.RightShift;
    public KeyCode dashKeyJ3 = KeyCode.E;
    public KeyCode dashKeyJ4 = KeyCode.Keypad0;

    [Header("Item Dash")]
    public float itemRespawnDelai = 4f;   // délai avant respawn
    public int itemNombreSimult = 3;    // nb items simultanés sur la map
    public float itemRayonCollecte = 0.6f; // rayon pour ramasser

    [Header("Dash Bulles")]
    public float dashBulleTaille = 0.15f; // taille des bulles
    public float dashBulleduree = 0.4f;  // durée de vie bulle
    public int dashBulleNombre = 5;     // nb bulles émises
    public float dashBulleIntervalle = 0.03f; // intervalle entre bulles

    // ── Calcul vitesse selon slider ───────────────────────────────────────────
    public float GetSpeedFromSlider(float sliderValue)
    {
        // Vitesse fixe — plus de malus/bonus selon le slider
        return baseSpeed;
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