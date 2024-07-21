using Godot;
using System;

public class Ball
{

    public float Radius { get; set; } = 1;
    public Vector2 InitialPosition { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float AngularVelocity { get; set; }

    public float Gravity { get; set; }

}
