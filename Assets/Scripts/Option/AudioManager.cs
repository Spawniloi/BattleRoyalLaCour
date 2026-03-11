using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    // Clés PlayerPrefs
    const string KEY_MUSIQUE = "vol_musique";
    const string KEY_VFX = "vol_vfx";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Charge volumes sauvegardés
        float volMusique = PlayerPrefs.GetFloat(KEY_MUSIQUE, 0.8f);
        float volVFX = PlayerPrefs.GetFloat(KEY_VFX, 0.8f);

        AppliquerMusique(volMusique);
        AppliquerVFX(volVFX);
    }

    // ── Applique volumes ──────────────────────────────────────────────────────
    public void AppliquerMusique(float valeur)
    {
        // Convertit 0-1 en dB (-80 à 0)
        float db = valeur > 0.001f
            ? Mathf.Log10(valeur) * 20f
            : -80f;

        mixer?.SetFloat("VolMusique", db);
        PlayerPrefs.SetFloat(KEY_MUSIQUE, valeur);
    }

    public void AppliquerVFX(float valeur)
    {
        float db = valeur > 0.001f
            ? Mathf.Log10(valeur) * 20f
            : -80f;

        mixer?.SetFloat("VolVFX", db);
        PlayerPrefs.SetFloat(KEY_VFX, valeur);
    }

    // ── Getters pour les sliders ──────────────────────────────────────────────
    public float GetVolMusique()
        => PlayerPrefs.GetFloat(KEY_MUSIQUE, 0.8f);
    public float GetVolVFX()
        => PlayerPrefs.GetFloat(KEY_VFX, 0.8f);

    public void Sauvegarder()
        => PlayerPrefs.Save();
}