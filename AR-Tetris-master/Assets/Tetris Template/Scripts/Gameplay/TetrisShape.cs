using UnityEngine;
using System.Collections;
using System;

public enum ShapeType
{
    I,
    T,
    O,
    L,
    J,
    S,
    Z
}

public class TetrisShape : MonoBehaviour
{
    [HideInInspector]
    public ShapeType type;

    [HideInInspector]
    public ShapeMovementController movementController;

    // The "next shape" preview is display-only and never takes part in the game.
    [HideInInspector]
    public bool isPreview;

    void Awake()
    {
        movementController = GetComponent<ShapeMovementController>();
        AssignRandomColor();
    }

    void Start()
    {
        if (isPreview)
            return;

        // Default position not valid? Then it's game over
        if (!Managers.Grid.IsValidGridPosition(this.transform))
            Managers.Game.SetState(typeof(GameOverState));
    }

    void AssignRandomColor()
    {
        Color color = Managers.Palette.TurnRandomColorFromTheme();
        Block3DView.Apply(transform, color);
    }
}
