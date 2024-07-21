using Godot;

public class BallConfig : Resource {
	[Export]
	public float Radius { get; set; } = 10.0f;

	[Export]
	public float Gravity { get; set; } = 1000.0f;

	[Export]
	public Vector2 InitialVelocity { get; set; }

	[Export]
	public float AngularVelocity { get; set; }
}