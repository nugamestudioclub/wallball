using Godot;
using System;

public enum GameStage
{
    Seeking,
    Serving,
    Recovering,
}

public class GameLogic : Node2D
{
    [Export]
    private GameConfig gameConfig;

    [Export]
    private PlayerConfig playerConfig = new PlayerConfig();

    [Export]
    private BallConfig ballConfig;

    private Player player = new Player();

    private Ball ball = new Ball();



    float serveDuration;

    float serveTimeFactor;

    float recoverDuration;

    private Vector2 wallBounds;

    private PlayArea playArea = new PlayArea();

    private Vector2 tileSpan; //how many tiles in the game area

    [Export]
    private string ballSpritePath;

    private readonly PlayerInput input = new PlayerInput();

    private GameStage stage;

    private float timeInStage = 0.0f;


    private ColorRect GetPlayArea()
    {
        return GetNode<ColorRect>("PlayArea");
    }

    private Node2D GetBall()
    {
        return GetNode<Node2D>("Ball");
    }

    private Node2D GetPlayer()
    {
        return GetNode<Node2D>("Player");
    }

    private Node2D GetEnvironment()
    {
        return GetNode<Node2D>("Environment");
    }
    public override void _Ready()
    {
        ColorRect area = GetPlayArea();
        area.Color = new Color(0, 0, 0, 0);
        playArea.Position = area.RectGlobalPosition;
        playArea.Size = area.RectSize;


        wallBounds = gameConfig.WallBounds;
        serveDuration = gameConfig.ServeDuration;
        serveTimeFactor = gameConfig.ServeTimeFactor;
        recoverDuration = gameConfig.RecoverDuration;

        Vector2 screenScale = PlayToScreenScale();


        Node2D ballScene = GetBall();
        ballScene.ApplyScale(screenScale);
        ball.Position = ScreenToPlay(ballScene.Position);
        ball.Radius = ballConfig.Radius;
        ball.Gravity = ballConfig.Gravity;
        ball.Velocity = ballConfig.InitialVelocity;

        Node2D playerScene = GetPlayer();
        playerScene.ApplyScale(screenScale);
        player.Position = ScreenToPlay(playerScene.Position);
        player.Radius = playerConfig.Radius;
        player.Gravity = playerConfig.Gravity;
        player.JumpDuration = playerConfig.JumpDuration;
        player.JumpSpeed = playerConfig.JumpSpeed;
        player.MoveSpeed = playerConfig.MoveSpeed;

        // TODO: Initialize score

        stage = GameStage.Seeking;
    }

    public override void _Draw()
    {
        //var wallColor = new Color(1, 1, 0, .5f);
        //DrawRect(new Rect2(0, 0, wallBounds), wallColor, filled: true);

        //var colliderColor = new Color(0, 1, 0);
        //DrawCircle(player.Position, player.Radius, colliderColor);
        //DrawCircle(ball.Position, ball.Radius, colliderColor);
    }
    private Vector2 PlayToScreenScale()
    {
        return new Vector2(playArea.Size.x / wallBounds.x,
            playArea.Size.y / wallBounds.y);
    }
    private Vector2 PlayToScreen(Vector2 position)
    {
        return new Vector2(playArea.Position.x + playArea.Size.x / wallBounds.x * position.x,
            playArea.Position.y + playArea.Size.y / wallBounds.y * position.y);
    }

    private Vector2 ScreenToPlay(Vector2 position)
    {
        return new Vector2(
            (position.x - playArea.Position.x) * (wallBounds.x / playArea.Size.x),
            (position.y - playArea.Position.y) * (wallBounds.y / playArea.Size.y));
    }

    public override void _Process(float delta)
    {
        GetBall().Position = PlayToScreen(ball.Position);
        GetPlayer().Position = PlayToScreen(player.Position);

        // Redraw
        Update();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (stage == GameStage.Serving)
            delta *= serveTimeFactor;

        ProcessInput();
        ProcessState(delta);
        ProcessMovement(delta);
        ProcessCollision(delta);
        ProcessStage(delta);
    }

    private bool CalculateCircleCircleCollision(Vector2 position1, float radius1, Vector2 position2, float radius2)
    {
        return position2.DistanceSquaredTo(position1) < (radius1 + radius2) * (radius1 + radius2);
    }

    Vector2 CalculateCircleContactPoint(Vector2 position, float radius, Vector2 collision)
    {
        if (collision.x == 0)
        {
            return new Vector2(
                position.x,
                collision.y < 0 ? position.y - radius : position.y + radius
            );
        }
        else if (collision.y == 0)
        {
            return new Vector2(
                collision.x < 0 ? position.x - radius : position.x + radius,
                position.y
            );
        }
        else
        {
            return new Vector2(
                collision.x < 0 ? position.x - radius : position.x + radius,
                collision.y < 0 ? position.y - radius : position.y + radius
            );
        }
    }

    private Vector2 CalculateCircleWallCollision(Vector2 position, float radius)
    {
        float x = position.x - radius < 0 ? position.x - radius
            : position.x + radius > wallBounds.x ? position.x + radius - wallBounds.x
            : 0f;
        float y = position.y - radius < 0 ? position.y - radius
            : position.y + radius > wallBounds.y ? position.y + radius - wallBounds.y
            : 0f;
        return new Vector2(x, y);
    }

    private void ChangeStage(GameStage stage)
    {
        Node2D environment = GetEnvironment();
        if (stage == GameStage.Serving)
        {
            environment.Call("begin_aberration");
        }
        else if (this.stage == GameStage.Serving)
        {
            environment.Call("backpedal_aberration");
        }
        this.stage = stage;
        timeInStage = 0.0f;
        input.Finish = false;
        GD.Print(stage);
    }

    private float G(Player player)
    {
        return player.State == PlayerState.Jumping
            ? player.Gravity * (player.TimeInState / player.JumpDuration)
            : player.Gravity;
    }

    private void HandleBallWallContact(Vector2 collision)
    {
        if (collision.LengthSquared() == 0)
            return;

        var point = CalculateCircleContactPoint(ball.Position, ball.Radius, collision);
        GD.Print($"ball contact point: {point}");

        // TODO: Safe/unsafe zones

        //Signal hit effect
        Node2D environment = GetEnvironment();
        environment.Call("pulse_effect", PlayToScreen(point));
        environment.Call("spawn_particles", PlayToScreen(point), ball.Velocity);

        if (stage == GameStage.Serving)
            ChangeStage(GameStage.Recovering);
    }

    private bool IsFalling(Player player)
    {
        return !player.IsGrounded
            && (!input.Jump || player.TimeInState >= player.JumpDuration || player.Velocity.y > 0);
    }

    private bool IsJumping(Player player)
    {
        return !player.IsGrounded
            && (input.Jump && player.State != PlayerState.Freefall)
            && player.TimeInState < player.JumpDuration && player.Velocity.y <= 0;
    }
    private bool IsMoving(Player player)
    {
        return player.IsGrounded && player.Velocity.x != 0;
    }

    private bool IsIdle(Player player)
    {
        return player.IsGrounded && player.Velocity.x == 0;
    }

    private bool IsStartingJump(Player player)
    {
        return player.IsGrounded && input.Jump;
    }

    private void MoveBall(float delta)
    {
        ball.PreviousPosition = ball.Position;
        ball.Velocity += new Vector2(0, ball.Gravity) * delta;
        ball.Position += ball.Velocity * delta;
    }

    private void MovePlayer(float delta)
    {
        player.PreviousPosition = player.Position;
        if (stage != GameStage.Serving)
        {
            float vx = (Convert.ToSingle(input.Right) - Convert.ToSingle(input.Left)) * player.MoveSpeed;
            float vy = IsStartingJump(player)
                ? -player.JumpSpeed
                : player.Velocity.y + (G(player) * delta);
            player.Velocity = new Vector2(vx, vy);
        }
        player.Position += player.Velocity * delta;
    }

    private void ProcessBallWallCollision()
    {
        var collision = CalculateCircleWallCollision(ball.Position, ball.Radius);
        float vx = collision.x == 0 ? ball.Velocity.x : -ball.Velocity.x;
        float vy = collision.y == 0 ? ball.Velocity.y : -ball.Velocity.y;

        ball.Position -= collision;
        ball.Velocity = new Vector2(vx, vy);

        HandleBallWallContact(collision);
    }


    private void ProcessCollision(float delta)
    {
        ProcessBallWallCollision();
        ProcessPlayerWallCollision();
        ProcessPlayerBallCollision();
    }

    private void ProcessInput()
    {
        if (Input.IsActionJustPressed("ui_left"))
            input.Left = true;
        else if (!Input.IsActionPressed("ui_left"))
            input.Left = false;
        if (Input.IsActionJustPressed("ui_right"))
            input.Right = true;
        else if (!Input.IsActionPressed("ui_right"))
            input.Right = false;
        if (Input.IsActionJustPressed("ui_up"))
            input.Up = true;
        else if (!Input.IsActionPressed("ui_up"))
            input.Up = false;
        if (Input.IsActionJustPressed("ui_down"))
            input.Down = true;
        else if (!Input.IsActionPressed("ui_down"))
            input.Down = false;
        if (Input.IsActionJustPressed("ui_accept"))
            input.Jump = true;
        else if (!Input.IsActionPressed("ui_accept"))
            input.Jump = false;
        if (Input.IsActionJustPressed("ui_accept"))
            input.Finish = true;
        else if (!Input.IsActionPressed("ui_accept"))
            input.Finish = false;
    }

    private void ProcessMovement(float delta)
    {
        MoveBall(delta);
        MovePlayer(delta);
    }

    private void ProcessPlayerBallCollision()
    {
        var areColliding = CalculateCircleCircleCollision(player.Position, player.Radius, ball.Position, ball.Radius);
        switch (stage)
        {
            case GameStage.Seeking:
                if (areColliding)
                    ChangeStage(GameStage.Serving);
                break;
            case GameStage.Serving:
                break;
        }
    }

    private void ProcessPlayerWallCollision()
    {
        var collision = CalculateCircleWallCollision(player.Position, player.Radius);
        // Bottom
        if (collision.y > 0)
        {
            player.IsGrounded = true;
        }
        // Top
        else if (collision.y < 0)
        {
            player.IsGrounded = false;
            input.Jump = false;
        }
        // Horizontal
        else
        {
            player.IsGrounded = false;
        }
        player.Position -= collision;
    }

    private void ProcessStage(float delta)
    {
        timeInStage += delta;

        switch (stage)
        {
            case GameStage.Serving:
                if (input.Finish || timeInStage >= serveDuration)
                    ChangeStage(GameStage.Recovering);
                break;
            case GameStage.Recovering:
                if (timeInStage >= recoverDuration)
                    ChangeStage(GameStage.Seeking);
                break;
        }
    }

    private void ProcessState(float delta)
    {
        if (IsMoving(player) && player.State != PlayerState.Moving)
        {
            player.ChangeState(PlayerState.Moving);
        }
        else if (IsIdle(player) && player.State != PlayerState.Idle)
        {
            player.ChangeState(PlayerState.Idle);
        }
        else if (IsFalling(player) && player.State != PlayerState.Freefall)
        {
            player.ChangeState(PlayerState.Freefall);
            input.Jump = false;
        }
        else if (IsJumping(player) && player.State != PlayerState.Jumping)
        {
            player.ChangeState(PlayerState.Jumping);
        }
        player.UpdateState(delta);
    }
}
