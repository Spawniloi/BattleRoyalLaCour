using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public int playerID = 1;

    public Vector2 MoveInput { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool DashPressed { get; private set; }

    // Config — assigné par le GameManager
    public MaireBalanceConfig config;

    void Update()
    {
        MoveInput = Vector2.zero;
        InteractPressed = false;
        DashPressed = false;

        // ── Manette prioritaire ───────────────────────────────────────────────
        var manettes = Gamepad.all;
        if (playerID - 1 < manettes.Count)
        {
            var manette = manettes[playerID - 1];
            MoveInput = manette.leftStick.ReadValue();

            if (MoveInput.magnitude < 0.2f) MoveInput = Vector2.zero;

            if (manette.buttonSouth.wasPressedThisFrame)
                InteractPressed = true;

            // Dash manette = bouton ouest (carré/X)
            if (manette.buttonWest.wasPressedThisFrame)
                DashPressed = true;

            if (MoveInput != Vector2.zero || InteractPressed || DashPressed)
                return;
        }

        // ── Clavier fallback ──────────────────────────────────────────────────
        var kb = Keyboard.current;
        if (kb == null) return;

        if (playerID == 1)
        {
            float x = 0f, y = 0f;
            if (kb.qKey.isPressed) x = -1f;
            if (kb.dKey.isPressed) x = 1f;
            if (kb.zKey.isPressed) y = 1f;
            if (kb.sKey.isPressed) y = -1f;
            MoveInput = new Vector2(x, y).normalized;

            if (kb.spaceKey.wasPressedThisFrame) InteractPressed = true;
            if (config != null && Input.GetKeyDown(config.dashKeyJ1))
                DashPressed = true;
        }
        else if (playerID == 2)
        {
            float x = 0f, y = 0f;
            if (kb.leftArrowKey.isPressed) x = -1f;
            if (kb.rightArrowKey.isPressed) x = 1f;
            if (kb.upArrowKey.isPressed) y = 1f;
            if (kb.downArrowKey.isPressed) y = -1f;
            MoveInput = new Vector2(x, y).normalized;

            if (kb.enterKey.wasPressedThisFrame) InteractPressed = true;
            if (config != null && Input.GetKeyDown(config.dashKeyJ2))
                DashPressed = true;
        }
        else if (playerID == 3)
        {
            float x = 0f, y = 0f;
            if (kb.jKey.isPressed) x = -1f;
            if (kb.lKey.isPressed) x = 1f;
            if (kb.iKey.isPressed) y = 1f;
            if (kb.kKey.isPressed) y = -1f;
            MoveInput = new Vector2(x, y).normalized;

            if (config != null && Input.GetKeyDown(config.dashKeyJ3))
                DashPressed = true;
        }
        else if (playerID == 4)
        {
            float x = 0f, y = 0f;
            if (kb.numpad4Key.isPressed) x = -1f;
            if (kb.numpad6Key.isPressed) x = 1f;
            if (kb.numpad8Key.isPressed) y = 1f;
            if (kb.numpad2Key.isPressed) y = -1f;
            MoveInput = new Vector2(x, y).normalized;

            if (config != null && Input.GetKeyDown(config.dashKeyJ4))
                DashPressed = true;
        }
    }
}