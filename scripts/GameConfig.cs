using Godot;

public class GameConfig : Resource {
	[Export]
	public Vector2 WallBounds { get; set; } = new Vector2(500, 500);

	[Export]
	public float ServeDuration { get; set; } = 5.0f;

	[Export]
	public float ServeTimeFactor { get; set; } = 0.1f;

	[Export]
	public float RecoverDuration { get; set; } = 2.0f;

    [Export]
    public float CatchInfluence { get; set; } = 1.0f;

	[Export]
	public float BallNormalSpeedup { get; set; } = 0.0f;

	[Export]
	public float BallFasterSpeedup { get; set; } = 0.0f;
}