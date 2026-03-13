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
        // Trouve le premier bouton actif
        indexActuel = TrouverPremierActif();
        SurlígnerBouton(indexActuel);
    }

    void Update()
    {
        var kb = Keyboard.current;
        var gp = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool gauche = false, droite = false, valider = false;

        if (kb != null)
        {
            gauche = kb.leftArrowKey.wasPressedThisFrame
                   || kb.qKey.wasPressedThisFrame;
            droite = kb.rightArrowKey.wasPressedThisFrame
                   || kb.dKey.wasPressedThisFrame;
            valider = kb.enterKey.wasPressedThisFrame
                   || kb.spaceKey.wasPressedThisFrame;
        }

        if (gp != null)
        {
            gauche = gauche || gp.leftStick.left.wasPressedThisFrame
                              || gp.dpad.left.wasPressedThisFrame;
            droite = droite || gp.leftStick.right.wasPressedThisFrame
                              || gp.dpad.right.wasPressedThisFrame;
            valider = valider || gp.buttonSouth.wasPressedThisFrame;
        }

        if (gauche) Naviguer(-1);
        if (droite) Naviguer(1);
        if (valider) Valider();
    }

    void Naviguer(int direction)
    {
        int nouveau = indexActuel;
        int essais = boutons.Count;

        // Cherche le prochain bouton actif dans la direction
        while (essais-- > 0)
        {
            nouveau = (nouveau + direction + boutons.Count) % boutons.Count;

            if (EstActif(nouveau))
            {
                indexActuel = nouveau;
                SurlígnerBouton(indexActuel);
                hubManager?.JouerSFX(hubManager.sfxFocus);
                return;
            }
        }
        // Aucun autre actif — on reste sur place
    }

    bool EstActif(int index)
    {
        if (index < 0 || index >= boutons.Count) return false;
        var btn = boutons[index];
        return btn != null
            && btn.gameObject.activeInHierarchy
            && btn.interactable;
    }

    int TrouverPremierActif()
    {
        for (int i = 0; i < boutons.Count; i++)
            if (EstActif(i)) return i;
        return 0;
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