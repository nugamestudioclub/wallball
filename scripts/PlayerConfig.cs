using Godot;
using System;

public class PlayerConfig : Resource {
	[Export]
	public float Radius { get; set; } = 1.0f;

	[Export]
	public float JumpDuration = 1.0f;

	[Export]
	public float JumpSpeed = 100.0f;

	[Export]
	public float MoveSpeed = 100.0f;

	[Export]
	public float Gravity = 100.0f;
}
