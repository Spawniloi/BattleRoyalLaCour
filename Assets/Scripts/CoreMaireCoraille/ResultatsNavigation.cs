using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ResultatsNavigation : MonoBehaviour
{
    [Header("Boutons navigables")]
    public List<Button> boutons = new List<Button>();

    [Header("Styles boutons")]
    public List<BoutonStylee> styles = new List<BoutonStylee>();

    private int indexActuel = 0;
    private float cooldownInput = 0f;

    void Start()
    {
        if (boutons.Count > 0)
            SurlигнerBouton(0);
    }

    void Update()
    {
        if (cooldownInput > 0f)
        {
            cooldownInput -= Time.deltaTime;
            return;
        }

        var kb = Keyboard.current;
        var manette = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool gauche = (kb != null && kb.leftArrowKey.wasPressedThisFrame)
                   || (manette != null &&
                      (manette.leftStick.left.wasPressedThisFrame ||
                       manette.dpad.left.wasPressedThisFrame));

        bool droite = (kb != null && kb.rightArrowKey.wasPressedThisFrame)
                   || (manette != null &&
                      (manette.leftStick.right.wasPressedThisFrame ||
                       manette.dpad.right.wasPressedThisFrame));

        bool valider = (kb != null && kb.enterKey.wasPressedThisFrame)
                    || (kb != null && kb.spaceKey.wasPressedThisFrame)
                    || (manette != null &&
                        manette.buttonSouth.wasPressedThisFrame);

        if (gauche)
        {
            indexActuel = (indexActuel - 1 + boutons.Count) % boutons.Count;
            SurlигнerBouton(indexActuel);
            cooldownInput = 0.2f;
        }
        else if (droite)
        {
            indexActuel = (indexActuel + 1) % boutons.Count;
            SurlигнerBouton(indexActuel);
            cooldownInput = 0.2f;
        }
        else if (valider)
        {
            boutons[indexActuel]?.onClick.Invoke();
            cooldownInput = 0.5f;
        }
    }

    void SurlигнerBouton(int index)
    {
        for (int i = 0; i < styles.Count; i++)
            styles[i]?.SetSelectionne(i == index);
    }
}