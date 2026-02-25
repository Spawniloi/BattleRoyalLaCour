using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public int playerID = 1; // assigné par le GameManager

    public Vector2 MoveInput { get; private set; }
    public bool InteractPressed { get; private set; }

    private Gamepad manette;

    void Update()
    {
        MoveInput = Vector2.zero;
        InteractPressed = false;

        // ── Manette prioritaire ───────────────────────────────
        var manettes = Gamepad.all;

        if (playerID - 1 < manettes.Count)
        {
            manette = manettes[playerID - 1];
            MoveInput = manette.leftStick.ReadValue();

            // Normalise si très petit (dead zone)
            if (MoveInput.magnitude < 0.2f)
                MoveInput = Vector2.zero;

            if (manette.buttonSouth.wasPressedThisFrame)
                InteractPressed = true;

            // Si la manette donne un input on s'arrête là
            if (MoveInput != Vector2.zero || InteractPressed)
                return;
        }

        // ── Clavier fallback ──────────────────────────────────
        var kb = Keyboard.current;
        if (kb == null) return;

        if (playerID == 1)
        {
            // ZQSD
            float x = 0f, y = 0f;
            if (kb.aKey.isPressed) x = -1f;
            if (kb.dKey.isPressed) x = 1f;
            if (kb.wKey.isPressed) y = 1f;
            if (kb.sKey.isPressed) y = -1f;
            MoveInput = new Vector2(x, y).normalized;

            if (kb.spaceKey.wasPressedThisFrame)
                InteractPressed = true;
        }
        else if (playerID == 2)
        {
            // Flèches
            float x = 0f, y = 0f;
            if (kb.leftArrowKey.isPressed) x = -1f;
            if (kb.rightArrowKey.isPressed) x = 1f;
            if (kb.upArrowKey.isPressed) y = 1f;
            if (kb.downArrowKey.isPressed) y = -1f;
            MoveInput = new Vector2(x, y).normalized;

            if (kb.enterKey.wasPressedThisFrame)
                InteractPressed = true;
        }
        else if (playerID == 3)
        {
            // IJKL
            float x = 0f, y = 0f;
            if (kb.jKey.isPressed) x = -1f;
            if (kb.lKey.isPressed) x = 1f;
            if (kb.iKey.isPressed) y = 1f;
            if (kb.kKey.isPressed) y = -1f;
            MoveInput = new Vector2(x, y).normalized;
        }
        else if (playerID == 4)
        {
            // Pavé numérique
            float x = 0f, y = 0f;
            if (kb.numpad4Key.isPressed) x = -1f;
            if (kb.numpad6Key.isPressed) x = 1f;
            if (kb.numpad8Key.isPressed) y = 1f;
            if (kb.numpad5Key.isPressed) y = -1f;
            MoveInput = new Vector2(x, y).normalized;
        }
    }
}