using Godot;
using System;
using System.Runtime;
using static Godot.VisualServer;

public enum GameStage
{
    Seeking,
    Serving,
    Recovering,
    Lose
}

public class GameLogic : Node2D
{
    [Export]
    private GameConfig gameConfig;

    [Export]
    private PlayerConfig playerConfig = new PlayerConfig();

    [Export]
    private BallConfig ballConfig;

    [Export]
    private ComboConfig comboConfig;

    private Player player = new Player();

    private Ball ball = new Ball();

    string currentComboCode = "";

    float serveDuration;

    float serveTimeFactor;

    float recoverDuration;

    float catchInfluencePercent;

    private Vector2 wallBounds;

    private PlayArea gameArea = new PlayArea();
    private PlayArea dangerArea = new PlayArea();

    private readonly PlayerInput input = new PlayerInput();

    private GameStage stage;

    private float timeInStage = 0.0f;

    Vector2 catchVector;

    private int score = 0;

    bool hasStarted;

    [Export(PropertyHint.File)]
    private string settingsFile;

    private GameSettings settings;

    private static Random random = new Random();

    private ColorRect GetGameArea()
    {
        return GetNode<ColorRect>("PlayArea");
    }

    private ColorRect GetDangerArea()
    {
        return GetNode<ColorRect>("DangerArea");
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

    private Node GetComboHandler()
    {
        return GetNode<Node>("ComboHandler");
    }

    public override void _Ready()
    {
        wallBounds = gameConfig.WallBounds;
        serveDuration = gameConfig.ServeDuration;
        serveTimeFactor = gameConfig.ServeTimeFactor;
        recoverDuration = gameConfig.RecoverDuration;
        catchInfluencePercent = gameConfig.CatchInfluence;

        ColorRect gameAreaNode = GetGameArea();
        gameAreaNode.Color = new Color(0, 0, 0, 0);
        gameArea.Position = gameAreaNode.RectGlobalPosition;
        gameArea.Size = gameAreaNode.RectSize;

        ColorRect dangerAreaNode = GetDangerArea();
        dangerAreaNode.Color = new Color(0, 0, 0, 0);
        var (dangerAreaPosition, dangerAreaSize) = ScreenToPlayRect(dangerAreaNode.RectPosition, dangerAreaNode.RectSize);
        dangerArea.Position = dangerAreaPosition;
        dangerArea.Size = dangerAreaSize;

        if (ResourceLoader.Exists(settingsFile))
        {
            settings = ResourceLoader.Load<GameSettings>(settingsFile);
        }
        else
        {
            settings = new GameSettings();
        }
        ((Godot.Object)GetEnvironment().Get("vol_slider")).Set("value", settings.volume);
        ((HSlider)GetEnvironment().Get("vol_slider")).Connect("value_changed", this, nameof(_OnVolumeChanged));

        Vector2 playToScreenScale = PlayToScreenScale();

        var ballNode = GetBall();
        ball.InitialPosition = ScreenToPlayPosition(ballNode.Position);
        ballNode.ApplyScale(playToScreenScale);
        ball.Radius = ballConfig.Radius;
        ball.Gravity = ballConfig.Gravity;
        ball.AngularVelocity = ballConfig.AngularVelocity;


        var playerNode = GetPlayer();
        player.Position = ScreenToPlayPosition(playerNode.Position);
        player.Radius = playerConfig.Radius;
        player.Gravity = playerConfig.Gravity;
        player.JumpDuration = playerConfig.JumpDuration;
        player.JumpSpeed = playerConfig.JumpSpeed;
        player.MoveSpeed = playerConfig.MoveSpeed;

        GetEnvironment().Call("update_hi_score", settings.highScore.ToString());
        Initialize();
        Reset();
    }

    //public override void _Draw() {
    //	var wallColor = new Color(1, 1, 0, .5f);
    //	var (dangerAreaPosition, dangerAreaSize) = PlayToScreenRect(dangerArea.Position, dangerArea.Size);
    //	DrawRect(new Rect2(dangerAreaPosition.x, dangerAreaPosition.y, dangerAreaSize), wallColor, filled: true);
    //}

    public override void _Process(float delta)
    {
        var ballNode = GetBall();
        ballNode.Position = PlayToScreenPosition(ball.Position);
        ballNode.Rotate((ball.AngularVelocity / 180f) * Mathf.Pi);

        var playerNode = GetPlayer();
        playerNode.Position = PlayToScreenPosition(player.Position);

        ProcessInput();
        ProcessCombo();

        // Redraw
        Update();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (stage == GameStage.Serving)
            delta *= serveTimeFactor;


        ProcessState(delta);
        ProcessMovement(delta);
        ProcessCollision(delta);
        ProcessStage(delta);
        ProcessAnimation();
    }

    private bool CheckCircleCircleCollision(Vector2 position1, float radius1, Vector2 position2, float radius2)
    {
        return position2.DistanceSquaredTo(position1) < (radius1 + radius2) * (radius1 + radius2);
    }

    private bool CheckCircleRectCollision(Vector2 circlePosition, float circleRadius, Vector2 rectPosition, Vector2 rectSize)
    {
        float closestX = Mathf.Clamp(circlePosition.x, rectPosition.x, rectPosition.x + rectSize.x);
        float closestY = Mathf.Clamp(circlePosition.y, rectPosition.y, rectPosition.y + -rectSize.y);
        float distanceX = circlePosition.x - closestX;
        float distanceY = circlePosition.y - closestY;
        return (distanceX * distanceX) + (distanceY * distanceY) < (circleRadius * circleRadius);
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

        switch (this.stage)
        {
            case GameStage.Serving:
                environment.Call("backpedal_aberration");
                break;
            case GameStage.Lose:
                Reset();
                break;
        }

        switch (stage)
        {
            case GameStage.Serving:
                environment.Call("begin_aberration");
                if (!hasStarted)
                {
                    Start();
                }
                break;
            case GameStage.Lose:
                GameOver();
                break;
        }

        input.Finish = false;

        this.stage = stage;
        timeInStage = 0.0f;
    }

    private float G(Player player)
    {
        return player.State == PlayerState.Jumping
            ? player.Gravity * (player.TimeInState / player.JumpDuration)
            : player.Gravity;
    }

    private void GameOver()
    {
        var environment = GetEnvironment();
        settings.highScore = Math.Max(score, settings.highScore);
        environment.Call("game_over", settings.highScore.ToString());
        SaveSettings();
        Initialize();

        hasStarted = false;
    }

    private void HandleBallWallContact(Vector2 collision)
    {
        if (collision.LengthSquared() == 0)
            return;

        var point = CalculateCircleContactPoint(ball.Position, ball.Radius, collision);
        // GD.Print($"ball contact point: {point}");

        // TODO: Safe/unsafe zones

        //Signal hit effect
        Node2D environment = GetEnvironment();
        environment.Call("pulse_effect", PlayToScreenPosition(point));
        environment.Call("spawn_particles", PlayToScreenPosition(point), ball.Velocity);

        environment.Call("play_wall_hit", random.Next(3) + 1);

    }

    private void Initialize()
    {
        ball.Position = ball.InitialPosition;
        ball.Velocity = Vector2.Zero;
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
        if (stage == GameStage.Serving)
        {
            ball.Position = player.Position;
        }
        else if (hasStarted)
        {
            ball.Velocity += new Vector2(0, ball.Gravity) * delta;
            ball.Position += ball.Velocity * delta;
        }
    }

    private void MovePlayer(float delta)
    {
        if (stage != GameStage.Serving && stage != GameStage.Lose)
        {
            float vx = (Convert.ToSingle(input.Right) - Convert.ToSingle(input.Left)) * player.MoveSpeed;
            float vy = IsStartingJump(player)
                ? -player.JumpSpeed
                : player.Velocity.y + (G(player) * delta);
            player.Velocity = new Vector2(vx, vy);
        }
        else
        {
            player.Velocity = new Vector2(player.Velocity.x, player.Velocity.y + (G(player) * delta));
        }
        player.Position += player.Velocity * delta;
    }

    private Vector2 PlayToScreenPosition(Vector2 position)
    {
        var scale = PlayToScreenScale();
        return new Vector2(
            gameArea.Position.x + (scale.x * position.x),
            gameArea.Position.y + (scale.y * position.y)
        );
    }

    private (Vector2 position, Vector2 size) PlayToScreenRect(Vector2 position, Vector2 size)
    {
        var scale = PlayToScreenScale();
        var scaledPosition = new Vector2(
            gameArea.Position.x + (scale.x * position.x),
            gameArea.Position.y + (scale.y * position.y)
        );
        var scaledSize = new Vector2(size.x * scale.x, size.y * scale.y);
        return (scaledPosition, scaledSize);
    }

    private Vector2 PlayToScreenScale()
    {
        return new Vector2(
            gameArea.Size.x / wallBounds.x,
            gameArea.Size.y / wallBounds.y
        );
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

    private void ProcessBallDangerCollision()
    {
        if (CheckCircleRectCollision(ball.Position, ball.Radius, dangerArea.Position, dangerArea.Size))
        {
            //TODO: Call game over event
            ChangeStage(GameStage.Lose);
        }
    }


    private void ProcessCollision(float delta)
    {
        ProcessBallDangerCollision();
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

    private void ProcessCombo()
    {
        if (stage != GameStage.Serving)
        {
            return;
        }
        string comboInput = GetComboInput();
        var comboHandler = GetComboHandler();
        var environment = GetEnvironment();
        if (comboInput == "_")
        {
            Godot.Object comboValue = (Godot.Object)comboHandler.Call("confirm_combo");
            int comboScore = (int)comboValue.Get("points");
            Godot.Collections.Array comboChars = (Godot.Collections.Array)comboValue.Get("inputs");

            currentComboCode = comboChars.Count > 0 ? (string)comboChars[0] : "";
            // GD.Print($"Combo completed with score: {comboScore}");
            score += comboScore;
            environment.Call("update_score", score.ToString());

        }
        else if (comboInput != "")
        {
            comboHandler.Call("accept_input", comboInput);
            environment.Call("play_combo_hit", random.Next(3) + 1);
            GD.Print("playing combo hit");
            environment.Call("hit_effect", PlayToScreenPosition(ball.Position));

        }
    }


    private string GetComboInput()
    {
        if (input.Up)
        {
            input.Up = false;
            return "W";
        }
        if (input.Down)
        {
            input.Down = false;
            return "S";
        }
        if (input.Left)
        {
            input.Left = false;
            return "A";
        }
        if (input.Right)
        {
            input.Right = false;
            return "D";
        }
        if (input.Finish)
        {
            return "_";
        }
        return "";
    }

    private float CalculateComboAngle(string comboName)
    {
        if (string.IsNullOrEmpty(comboName))
        {
            return -random.Next(180_000) * 0.001f;
        }
        switch (comboName[0])
        {
            case 'A': return -150;
            case 'W': return -120;
            case 'S': return -45;
            case 'D': return -15;
            default: return -90;
        }
    }
    private void ProcessMovement(float delta)
    {
        MoveBall(delta);
        MovePlayer(delta);
    }

    private void ProcessAnimation()
    {
        if (stage == GameStage.Serving)
        {
            ProcessComboAnimation();
        }
        else
        {
            ProcessMovementAnimation();
        }
    }

    private void ProcessMovementAnimation()
    {
        var playerNode = GetPlayer();

        if (!player.IsGrounded)
        {
            if (player.CurrentAnimation != "JUMP")
            {
                playerNode.Call("jump");
                player.CurrentAnimation = "JUMP";
            }
        }
        else if (player.Velocity.x < 0)
        {
            if (player.CurrentAnimation != "WALK_LEFT")
            {
                playerNode.Call("anim_walk_left");
                player.CurrentAnimation = "WALK_LEFT";
            }
        }
        else if (player.Velocity.x > 0)
        {
            if (player.CurrentAnimation != "WALK_RIGHT")
            {
                playerNode.Call("anim_walk_right");
                player.CurrentAnimation = "WALK_RIGHT";
            }
        }
        else
        {
            if (player.CurrentAnimation != "IDLE")
            {
                playerNode.Call("anim_idle");
                player.CurrentAnimation = "IDLE";
            }
        }
    }

    private void ProcessComboAnimation()
    {
        var playerNode = GetPlayer();
        if (currentComboCode == "W")
        {
            playerNode.Call("anim_combo_w");
            player.CurrentAnimation = "COMBO_W";
        }
        else if (currentComboCode == "A")
        {
            playerNode.Call("anim_combo_a");
            player.CurrentAnimation = "COMBO_A";
        }
        else if (currentComboCode == "S")
        {
            playerNode.Call("anim_combo_s");
            player.CurrentAnimation = "COMBO_S";
        }
        else if (currentComboCode == "D")
        {
            playerNode.Call("anim_combo_d");
            player.CurrentAnimation = "COMBO_D";
        }
    }

    private void ProcessPlayerBallCollision()
    {
        var areColliding = CheckCircleCircleCollision(player.Position, player.Radius, ball.Position, ball.Radius);
        switch (stage)
        {
            case GameStage.Seeking:
                if (areColliding && !player.IsGrounded)
                {
                    catchVector = ball.Position - player.Position;
                    ChangeStage(GameStage.Serving);
                }
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
            if (stage == GameStage.Serving)
            {
                //TODO: Call game over event
                ChangeStage(GameStage.Lose);
            }

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
                {
                    float speedup = input.Finish && currentComboCode != ""
                        ? gameConfig.BallNormalSpeedup
                        : gameConfig.BallFasterSpeedup;
                    float angle = (CalculateComboAngle(currentComboCode) / 180f) * Mathf.Pi;
                    ball.Velocity = CalculateThrowVelocity(angle, speedup);
                    ChangeStage(GameStage.Recovering);
                }
                break;
            case GameStage.Recovering:
                if (timeInStage >= recoverDuration)
                    ChangeStage(GameStage.Seeking);
                break;
            case GameStage.Lose:
                if (input.Finish)
                    ChangeStage(GameStage.Seeking);
                break;
        }
    }

    private Vector2 CalculateThrowVelocity(float angle, float speedup)
    {
        var magnitude = Mathf.Max(ball.Velocity.Length() * (1f + speedup), ballConfig.InitialSpeed);
        var throwDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        var catchDirection = new Vector2(catchVector.x, Mathf.Min(catchVector.y, 0f)).Normalized();
        return (catchDirection * catchInfluencePercent + throwDirection).Normalized() * magnitude;
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

    private void Reset()
    {
        var environment = GetEnvironment();
        environment.Call("game_reset");

        stage = GameStage.Seeking;

        score = 0;
        environment.Call("update_score", score.ToString());
    }

    private Vector2 ScreenToPlayPosition(Vector2 position)
    {
        var scale = ScreenToPlayScale();
        return new Vector2(
            (position.x - gameArea.Position.x) * scale.x,
            (position.y - gameArea.Position.y) * scale.y
        );
    }

    private (Vector2 position, Vector2 size) ScreenToPlayRect(Vector2 position, Vector2 size)
    {
        var scale = ScreenToPlayScale();
        var scaledPosition = new Vector2(
            (position.x - gameArea.Position.x) * scale.x,
            (position.y - gameArea.Position.y) * scale.y
        );
        var scaledSize = new Vector2(size.x * scale.x, size.y * scale.y);
        return (scaledPosition, scaledSize);
    }

    private Vector2 ScreenToPlayScale()
    {
        return new Vector2(
            wallBounds.x / gameArea.Size.x,
            wallBounds.y / gameArea.Size.y
        );
    }

    private void Start()
    {
        var environment = GetEnvironment();
        environment.Call("game_start");
        hasStarted = true;
    }

    private void _OnVolumeChanged(float newVol)
    {
        // GD.Print("Changed");
        settings.volume = newVol;
        SaveSettings();
    }

    private void SaveSettings()
    {
        ResourceSaver.Save(settingsFile, settings);
    }
}
