using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HubNavigation : MonoBehaviour
{
    [Header("Boutons dans l'ordre")]
    public List<Button> boutons = new List<Button>();

    [Header("Couleurs")]
    public Color couleurNormal = Color.white;
    public Color couleurSelectionne = new Color(1f, 0.84f, 0f, 1f);

    [Header("Audio")]
    public HubManager hubManager;

    private int indexActuel = 0;

    void Start()
    {
        SurlígnerBouton(indexActuel);
    }

    void Update()
    {
        var kb = Keyboard.current;
        var gp = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool haut = false, bas = false, valider = false;

        // Clavier
        if (kb != null)
        {
            haut = kb.upArrowKey.wasPressedThisFrame
                   || kb.zKey.wasPressedThisFrame;
            bas = kb.downArrowKey.wasPressedThisFrame
                   || kb.sKey.wasPressedThisFrame;
            valider = kb.enterKey.wasPressedThisFrame
                   || kb.spaceKey.wasPressedThisFrame;
        }

        // Manette J1
        if (gp != null)
        {
            haut = haut || gp.leftStick.up.wasPressedThisFrame
                              || gp.dpad.up.wasPressedThisFrame;
            bas = bas || gp.leftStick.down.wasPressedThisFrame
                              || gp.dpad.down.wasPressedThisFrame;
            valider = valider || gp.buttonSouth.wasPressedThisFrame;
        }

        if (haut) Naviguer(-1);
        if (bas) Naviguer(1);
        if (valider) Valider();
    }

    void Naviguer(int direction)
    {
        indexActuel = (indexActuel + direction + boutons.Count)
                    % boutons.Count;

        SurlígnerBouton(indexActuel);
        hubManager?.JouerSFX(hubManager.sfxFocus);
    }

    void Valider()
    {
        if (indexActuel >= 0 && indexActuel < boutons.Count)
            boutons[indexActuel].onClick.Invoke();
    }

    void SurlígnerBouton(int index)
    {
        for (int i = 0; i < boutons.Count; i++)
        {
            if (boutons[i] == null) continue;

            Image img = boutons[i].GetComponent<Image>();
            TextMeshProUGUI txt = boutons[i]
                .GetComponentInChildren<TextMeshProUGUI>();

            bool actif = (i == index);

            if (txt != null)
                txt.color = actif ? couleurSelectionne : couleurNormal;

            boutons[i].transform.localScale = actif
                ? Vector3.one * 1.08f
                : Vector3.one;
        }
    }
}
