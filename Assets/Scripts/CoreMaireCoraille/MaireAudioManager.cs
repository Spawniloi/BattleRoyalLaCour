using UnityEngine;
using System.Collections;

public class MaireAudioManager : MonoBehaviour
{
    public static MaireAudioManager Instance;

    [Header("Musique")]
    public AudioSource sourcMusique;
    public AudioClip musiqueFont;

    [Header("Ambiance piscine")]
    public AudioSource sourceAmbiance;
    public AudioClip ambiancePiscine;
    public AudioClip criEnfant;
    public float intervalleAmbMin = 5f;
    public float intervalleAmbMax = 15f;
    public float pitchAmbMin = 0.8f;
    public float pitchAmbMax = 1.2f;

    [Header("Clapotis déplacement")]
    public AudioSource sourceClapotis;
    public AudioClip clapotis;
    public float volumeClapotisMax = 0.4f;

    [Header("SFX")]
    public AudioSource sourceSFX;
    public AudioClip sfxRebond;
    public AudioClip sfxTransfert;
    public AudioClip sfxCollecteDash;
    public AudioClip sfxDash;
    public AudioClip sfxFusion;

    private int nbJoueursEnMouvement = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Musique fond
        if (sourcMusique != null && musiqueFont != null)
        {
            sourcMusique.clip = musiqueFont;
            sourcMusique.loop = true;
            sourcMusique.Play();
        }

        // Ambiance piscine
        if (sourceAmbiance != null && ambiancePiscine != null)
        {
            sourceAmbiance.clip = ambiancePiscine;
            sourceAmbiance.loop = true;
            sourceAmbiance.Play();
        }

        // Clapotis — volume 0 au départ
        if (sourceClapotis != null && clapotis != null)
        {
            sourceClapotis.clip = clapotis;
            sourceClapotis.loop = true;
            sourceClapotis.volume = 0f;
            sourceClapotis.Play();
        }

        StartCoroutine(CrisAleatoires());
    }

    void Update()
    {
        // Clapotis — volume selon nb joueurs qui bougent
        if (sourceClapotis != null)
        {
            float volumeCible = nbJoueursEnMouvement > 0
                ? volumeClapotisMax * (nbJoueursEnMouvement / 4f)
                : 0f;

            sourceClapotis.volume = Mathf.Lerp(
                sourceClapotis.volume, volumeCible,
                Time.deltaTime * 3f);
        }
    }

    // ── Appelé par RacailleController ─────────────────────────────────────────
    public void SetJoueurEnMouvement(bool bouge)
    {
        nbJoueursEnMouvement = Mathf.Clamp(
            nbJoueursEnMouvement + (bouge ? 1 : -1), 0, 4);
    }

    // ── Cris aléatoires ───────────────────────────────────────────────────────
    IEnumerator CrisAleatoires()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(intervalleAmbMin, intervalleAmbMax));

            if (criEnfant != null && sourceSFX != null)
            {
                sourceSFX.pitch = Random.Range(pitchAmbMin, pitchAmbMax);
                sourceSFX.PlayOneShot(criEnfant);
                sourceSFX.pitch = 1f;
            }
        }
    }

    // ── SFX ───────────────────────────────────────────────────────────────────
    public void JouerRebond()
        => sourceSFX?.PlayOneShot(sfxRebond);

    public void JouerTransfert()
        => sourceSFX?.PlayOneShot(sfxTransfert);

    public void JouerCollecteDash()
        => sourceSFX?.PlayOneShot(sfxCollecteDash);

    public void JouerDash()
        => sourceSFX?.PlayOneShot(sfxDash);

    public void JouerFusion()
        => sourceSFX?.PlayOneShot(sfxFusion);
}

