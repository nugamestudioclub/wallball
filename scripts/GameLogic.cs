using Godot;
using System;

public class GameLogic : Node2D {
	[Export]
	private Node ballData;
	private Ball ball;

	[Export]
	private PlayerConfig playerConfig = new PlayerConfig();

	private Player player = new Player();

	[Export]
	private Vector2 wallBounds;

	private Vector2 tileSpan; //how many tiles in the game area

	[Export]
	private string ballSpritePath;

	private readonly PlayerInput input = new PlayerInput();

	public override void _Ready() {
		//initialize ball
		ball = new Ball();
		ball.Velocity = new Vector2(400, 400);
		ball.Position = GetChild<Node2D>(0).Position;
		//initialize score

		player.Radius = playerConfig.Radius;
		player.Gravity = playerConfig.Gravity;
		player.JumpDuration = playerConfig.JumpDuration;
		player.JumpSpeed = playerConfig.JumpSpeed;
		player.MoveSpeed = playerConfig.MoveSpeed;
		player.Position = GetChild<Node2D>(1).Position;
	}

	public override void _Draw() {
		var wallColor = new Color(1, 1, 0, .5f);
		DrawRect(new Rect2(0, 0, wallBounds), wallColor, filled: true);

		//var colliderColor = new Color(0, 1, 0);
		//DrawCircle(player.Position, player.Radius, colliderColor);
		//DrawCircle(ball.Position, ball.Radius, colliderColor);
	}

	public override void _Process(float delta) {
		GetChild<Node2D>(0).Position = ball.Position;
		GetChild<Node2D>(1).Position = player.Position;

		// Redraw
		Update();
	}

	public override void _PhysicsProcess(float delta) {
		ProcessInput();
		ProcessState(delta);
		ProcessMovement(delta);
		ProcessCollision(delta);
	}

	private bool CalculateCircleCircleCollision(Vector2 position1, float radius1, Vector2 position2, float radius2) {
		return position2.DistanceSquaredTo(position1) < (radius1 + radius2) * (radius1 + radius2);
	}

	private Vector2 CalculateCircleWallCollision(Vector2 position, float radius) {
		float x = position.x - radius < 0 ? position.x - radius
			: position.x + radius > wallBounds.x ? position.x + radius - wallBounds.x
			: 0f;
		float y = position.y - radius < 0 ? position.y - radius
			: position.y + radius > wallBounds.y ? position.y + radius - wallBounds.y
			: 0f;
		return new Vector2(x, y);
	}

	private float G(Player player) {
		return player.State == PlayerState.Jumping
			? player.Gravity * (player.TimeInState / player.JumpDuration)
			: player.Gravity;
	}

	private bool IsFalling(Player player) {
		return !player.IsGrounded
			&& (!input.Jump || player.TimeInState >= player.JumpDuration || player.Velocity.y > 0);
	}

	private bool IsJumping(Player player) {
		return !player.IsGrounded
			&& (input.Jump && player.State != PlayerState.Freefall)
			&& player.TimeInState < player.JumpDuration && player.Velocity.y <= 0;
	}
	private bool IsMoving(Player player) {
		return player.IsGrounded && player.Velocity.x != 0;
	}

	private bool IsIdle(Player player) {
		return player.IsGrounded && player.Velocity.x == 0;
	}

	private bool IsStartingJump(Player player) {
		return player.IsGrounded && input.Jump;
	}

	private void MoveBall(float delta) {
		//apply velocity and gravity to position
		ball.PreviousPosition = ball.Position;
		ball.Velocity += new Vector2(0, ball.Gravity) * delta;
		ball.Position += ball.Velocity * delta;

	}

	private void MovePlayer(float delta) {
		float vx = (Convert.ToSingle(input.Right) - Convert.ToSingle(input.Left)) * player.MoveSpeed;
		float vy = IsStartingJump(player)
			? -player.JumpSpeed
			: player.Velocity.y + (G(player) * delta);
		player.Velocity = new Vector2(vx, vy);
		player.Position += player.Velocity * delta;
	}

	private void ProcessBallWallCollision() {
		var collision = CalculateCircleWallCollision(ball.Position, ball.Radius);
		float vx = collision.x == 0 ? ball.Velocity.x : -ball.Velocity.x;
		float vy = collision.y == 0 ? ball.Velocity.y : -ball.Velocity.y;

		//emit signal for which tile(s) collided with

		ball.Velocity = new Vector2(vx, vy);
	}

	private void ProcessCollision(float delta) {
		ProcessBallWallCollision();
		ProcessPlayerWallCollision();
	}

	private void ProcessInput() {
		if( Input.IsActionJustPressed("ui_left") )
			input.Left = true;
		else if( !Input.IsActionPressed("ui_left") )
			input.Left = false;
		if( Input.IsActionJustPressed("ui_right") )
			input.Right = true;
		else if( !Input.IsActionPressed("ui_right") )
			input.Right = false;
		if( Input.IsActionJustPressed("ui_up") )
			input.Up = true;
		else if( !Input.IsActionPressed("ui_up") )
			input.Up = false;
		if( Input.IsActionJustPressed("ui_down") )
			input.Down = true;
		else if( !Input.IsActionPressed("ui_down") )
			input.Down = false;
		if( Input.IsActionJustPressed("ui_accept") )
			input.Jump = true;
		else if( !Input.IsActionPressed("ui_accept") )
			input.Jump = false;
		if( Input.IsActionJustPressed("ui_accept") )
			input.Finish = true;
		else if( !Input.IsActionPressed("ui_accept") )
			input.Finish = false;
	}

	private void ProcessMovement(float delta) {
		MoveBall(delta);
		MovePlayer(delta);
	}

	private void ProcessPlayerWallCollision() {
		var collision = CalculateCircleWallCollision(player.Position, player.Radius);
		// Bottom
		if( collision.y > 0 ) {
			player.IsGrounded = true;
		}
		// Top
		else if( collision.y < 0 ) {
			player.IsGrounded = false;
			input.Jump = false;
		}
		// Horizontal
		else {
			player.IsGrounded = false;
		}
		player.Position -= collision;
	}

	private void ProcessState(float delta) {
		if( IsMoving(player) && player.State != PlayerState.Moving ) {
			player.ChangeState(PlayerState.Moving);
		}
		else if( IsIdle(player) && player.State != PlayerState.Idle ) {
			player.ChangeState(PlayerState.Idle);
		}
		else if( IsFalling(player) && player.State != PlayerState.Freefall ) {
			player.ChangeState(PlayerState.Freefall);
			input.Jump = false;
		}
		else if( IsJumping(player) && player.State != PlayerState.Jumping ) {
			player.ChangeState(PlayerState.Jumping);
		}
		player.UpdateState(delta);
	}
}
