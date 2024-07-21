using Godot;

public enum PlayerState {
	Idle,
	Moving,
	Jumping,
	Freefall,
}

public class Player {
	private PlayerState state = PlayerState.Idle;
	private float timeInState;

	public float Radius { get; set; }
	public Vector2 Position { get; set; }
	public Vector2 Velocity { get; set; }
	public float Gravity { get; set; }
	public bool IsGrounded { get; set; }
	public float JumpDuration { get; set; }
	public float JumpSpeed { get; set; }
	public float MoveSpeed { get; set; }
	public PlayerState State => state;
	public float TimeInState => timeInState;

	public void ChangeState(PlayerState state) {
		this.state = state;
		timeInState = 0f;
	}

	public void UpdateState(float delta) {
		timeInState += delta;
	}
}
