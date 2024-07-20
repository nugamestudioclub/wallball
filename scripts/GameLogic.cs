using Godot;
using System;

public class GameLogic : Node2D
{
    [Export]
    private Node ballData;
    private Ball ball;
    [Export]
    private Node player;
    [Export]
    private Vector2 wallBounds;

    private Vector2 tileSpan; //how many tiles in the game area

    [Export]
    private string ballSpritePath;

    private readonly PlayerInput input = new PlayerInput();

    public override void _Ready()
    {
        //initialize ball
        ball = new Ball();
        ball.Velocity = new Vector2(400, 400);
        ball.Position = GetChild<Node2D>(0).Position;
        //initialize score
    }

    public override void _Draw()
    {
        var color = new Color(1, 1, 0, .5f);
        DrawRect(new Rect2(0, 0, wallBounds), color, filled: true);
    }

    public override void _Process(float delta)
    {
         GetChild<Node2D>(0).Position = ball.Position;
    }

    public override void _PhysicsProcess(float delta)
    {
        ProcessInput();
        ProcessMovement(delta);
        ProcessCollision(delta);
    }

    private void ProcessInput()
    {
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

    private void ProcessMovement(float delta)
    {
        MoveBall(delta);
        MovePlayer(delta);
    }
    private void MoveBall(float delta)
    {
        //apply velocity and gravity to position
        ball.PreviousPosition = ball.Position;
        ball.Velocity += new Vector2(0, ball.Gravity) * delta;
        ball.Position += ball.Velocity * delta;
        
    }

    private void MovePlayer(float delta)
    {

    }
    private void ProcessCollision(float delta)
    {
        float xVelocity = ball.Velocity.x;
        float yVelocity = ball.Velocity.y;

        //ball vs ceiling 
        if (CalculateCircleWallCollision(ball.Position, ball.Radius).y > 0)
        {
            //calculate which y tile hit
            yVelocity *= -1;
        }
        //ball vs floor
        else if (CalculateCircleWallCollision(ball.Position, ball.Radius).y < 0)
        {
            yVelocity *= -1;
        }
        //ball vs left wall
        if (CalculateCircleWallCollision(ball.Position, ball.Radius).x > 0)
        {
            //calculate which x tile hit
            xVelocity *= -1;
        }
        //ball vs right wall
        else if (CalculateCircleWallCollision(ball.Position, ball.Radius).x < 0)
        {
            //calculate which x tile hit
            xVelocity *= -1;
        }
        //ball vs players
        //player vs ceiling
        //player vs floor
        //player vs left wall
        //player vs right wall

        //emit signal for which tile(s) collided with
        ball.Velocity = new Vector2(xVelocity, yVelocity);
    }

    private Vector2 CalculateCircleWallCollision(Vector2 position, float radius)
    {
        float x = 0;
        float y = 0;

        if (position.x + radius >= wallBounds.x)
        {
            x = 1;
        }
        else if (position.x - radius <= 0)
        {
            x = -1;
        }
        if (position.y + radius >= wallBounds.y)
        {
            y = 1;
        }
        else if (position.y - radius <= 0)
        {
            y = -1;
        }
        return new Vector2(x, y);
    }

    private bool CalculateCircleCircleCollision(Vector2 position1, float radius1,
        Vector2 position2, float radius2)
    {
        return position2.DistanceSquaredTo(position1) < (radius1 + radius2) * (radius1 + radius2);
    }
}
