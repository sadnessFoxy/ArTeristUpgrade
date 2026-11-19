using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Player controls via the Input System. Pointer gestures react while the finger is still down:
// drag sideways to move one column per step, drag down to drop, tap to rotate.
// Keyboard shortcuts are kept for desktop testing.
public class PlayerInputManager : MonoBehaviour
{
    // Gesture thresholds as fractions of the screen width, so they feel the same on any device.
    private const float MoveStepFraction = 0.07f;
    private const float DropFraction = 0.12f;
    private const float TapSlopFraction = 0.03f;

    public bool isActive;

    private bool tracking;          // a gesture started on the board (not on a UI button)
    private bool gestureUsed;       // the gesture already moved or dropped the shape
    private Vector2 pressPosition;
    private float moveAnchorX;      // x where the next column step is measured from

    void Update()
    {
        if (!isActive || Managers.Game.currentShape == null)
        {
            tracking = false;
            return;
        }

        ReadKeyboard();
        if (isActive)
            ReadPointer();
    }

    private void ReadKeyboard()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.aKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            Rotate(false);
        else if (keyboard.dKey.wasPressedThisFrame)
            Rotate(true);

        if (keyboard.leftArrowKey.wasPressedThisFrame)
            Move(Vector2.left);
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
            Move(Vector2.right);
        else if (keyboard.downArrowKey.wasPressedThisFrame)
            Drop();
    }

    private void ReadPointer()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
            return;

        Vector2 position = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
            BeginGesture(position);

        if (!tracking)
            return;

        if (pointer.press.isPressed)
            UpdateGesture(position);

        if (pointer.press.wasReleasedThisFrame)
            EndGesture(position);
    }

    private void BeginGesture(Vector2 position)
    {
        // Presses on the pause button and other UI must not rotate the shape.
        tracking = EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
        gestureUsed = false;
        pressPosition = position;
        moveAnchorX = position.x;
    }

    private void UpdateGesture(Vector2 position)
    {
        float step = Screen.width * MoveStepFraction;

        // Move one column for every step dragged, while the finger is still down.
        while (Mathf.Abs(position.x - moveAnchorX) >= step)
        {
            float direction = Mathf.Sign(position.x - moveAnchorX);
            Move(direction < 0 ? Vector2.left : Vector2.right);
            moveAnchorX += direction * step;
            gestureUsed = true;
        }

        Vector2 delta = position - pressPosition;
        bool mostlyDown = -delta.y > Mathf.Abs(delta.x) * 2f;
        if (mostlyDown && -delta.y >= Screen.width * DropFraction)
        {
            Drop();
            tracking = false;
        }
    }

    private void EndGesture(Vector2 position)
    {
        tracking = false;

        bool isTap = !gestureUsed &&
                     (position - pressPosition).magnitude <= Screen.width * TapSlopFraction;
        if (isTap)
            Rotate(pressPosition.x >= Screen.width / 2f);
    }

    private static void Rotate(bool clockwise)
    {
        Managers.Game.currentShape.movementController.RotateClockWise(clockwise);
    }

    private static void Move(Vector2 direction)
    {
        Managers.Game.currentShape.movementController.MoveHorizontal(direction);
    }

    private void Drop()
    {
        isActive = false;
        Managers.Game.currentShape.movementController.InstantFall();
    }
}
