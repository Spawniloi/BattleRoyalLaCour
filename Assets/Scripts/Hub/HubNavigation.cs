using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HubNavigation : MonoBehaviour
{
    [Header("Boutons dans l'ordre")]
    public List<Button> boutons = new List<Button>();

    [Header("Styles boutons")]
    public List<BoutonStylee> styles = new List<BoutonStylee>();

    [Header("Audio")]
    public HubManager hubManager;

    private int indexActuel = 0;

    void Start()
    {
        // Sélectionne le premier par défaut
        SurlígnerBouton(indexActuel);
    }

    void Update()
    {
        var kb = Keyboard.current;
        var gp = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool haut = false, bas = false, valider = false;

        if (kb != null)
        {
            haut = kb.upArrowKey.wasPressedThisFrame
                   || kb.zKey.wasPressedThisFrame;
            bas = kb.downArrowKey.wasPressedThisFrame
                   || kb.sKey.wasPressedThisFrame;
            valider = kb.enterKey.wasPressedThisFrame
                   || kb.spaceKey.wasPressedThisFrame;
        }

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
        for (int i = 0; i < styles.Count; i++)
            styles[i]?.SetSelectionne(i == index);
    }
}
