using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HubManager : MonoBehaviour
{
    [Header("Panel Config")]
    public PanelConfigManager panelConfig;

    [Header("Panel Options")]
    public OptionsManager panelOptions;

    [Header("Boutons")]
    public Button btnJouer;
    public Button btnPerso;
    public Button btnTuto;
    public Button btnOptions;
    public Button btnQuitter;

    [Header("Titre")]
    public TextMeshProUGUI textTitre;
    public float vitesseEcriture = 0.05f;

    [Header("Audio")]
    public AudioSource sourceMusique;
    public AudioSource sourceSFX;
    public AudioClip musicHub;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;
    public AudioClip sfxRetour;

    string L(string cle, params string[] args)
        => LocalisationManager.Instance != null
            ? LocalisationManager.Instance.Get(cle, args)
            : cle;

    void Start()
    {
        // Musique
        if (sourceMusique != null && musicHub != null)
        {
            sourceMusique.clip = musicHub;
            sourceMusique.loop = true;
            sourceMusique.Play();
        }

        // Boutons
        btnJouer?.onClick.AddListener(AllerJouer);
        btnPerso?.onClick.AddListener(AllerPerso);
        btnTuto?.onClick.AddListener(AllerTuto);
        btnOptions?.onClick.AddListener(AllerOptions);
        btnQuitter?.onClick.AddListener(Quitter);

        // Titre — attend que les traductions soient prêtes
        if (LocalisationManager.Instance != null)
        {
            if (LocalisationManager.Instance.EstCharge())
                LancerEcriture();
            else
                LocalisationManager.Instance.onTraductionsChargees
                    += LancerEcriture;
        }
    }

    // ── Titre — écrit une seule fois quand prêt ───────────────────────────────
    void LancerEcriture()
    {
        // Se désabonne immédiatement — ne se relance pas
        if (LocalisationManager.Instance != null)
            LocalisationManager.Instance.onTraductionsChargees
                -= LancerEcriture;

        if (textTitre == null) return;

        StopAllCoroutines();
        StartCoroutine(EcrireTitre(L("hub_titre")));
    }

    IEnumerator EcrireTitre(string texte)
    {
        textTitre.text = "";
        foreach (char c in texte)
        {
            textTitre.text += c;
            yield return new WaitForSeconds(vitesseEcriture);
        }
    }

    void OnDestroy()
    {
        if (LocalisationManager.Instance != null)
            LocalisationManager.Instance.onTraductionsChargees
                -= LancerEcriture;
    }

    // ── Audio ─────────────────────────────────────────────────────────────────
    public void JouerSFX(AudioClip clip)
    {
        if (sourceSFX != null && clip != null)
            sourceSFX.PlayOneShot(clip);
    }

    // ── Boutons ───────────────────────────────────────────────────────────────
    void AllerJouer()
    {
        JouerSFX(sfxValider);
        panelConfig?.Ouvrir();
    }

    void AllerPerso()
    {
        JouerSFX(sfxValider);
        SceneManager.LoadScene("Scene_Personnalisation");
    }

    void AllerTuto()
    {
        JouerSFX(sfxValider);
        SceneManager.LoadScene("Scene_Tuto");
    }

    void AllerOptions()
    {
        JouerSFX(sfxValider);
        panelOptions?.Ouvrir();
    }

    void Quitter()
    {
        JouerSFX(sfxValider);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}