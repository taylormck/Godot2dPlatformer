using System;
using Godot;
using GodotStateCharts;

public partial class Player : CharacterBody2D
{

	private AnimatedSprite2D _animations;
	private PlatformerMoveComponent _moveComponent;
	private PlayerController _controller;
	private Sprite2D _sprite;
	private AnimationTree _animationTree;
	private StateChart _stateChart;
	private Timer _coyoteTimer;
	private Timer _jumpQueueTimer;
	private Timer _wallJumpControlTimer;
	private RayCast2D _wallDetector;

	// Essentially, the X component of Vector2.Right or Vector2.Left. Since
	// we won't need the Y component, there's no need to keep the full vector
	// around.
	private float _facing = 1;

	private void UpdateWallDetector()
	{
		// NOTE: Does this math get handled at compile time? I hope so, but I'm
		// not 100% sure.
		_wallDetector.Rotation = (float)Math.PI / 2.0f * _facing;
	}

	private bool IsGrabbingWall()
	{
		return _wallDetector.IsColliding() && Math.Sign(_controller.WantedMovement()) == Math.Sign(_facing);
	}

	private float LerpedMovement()
	{
		float wantedMovement = _controller.WantedMovement();
		float lerpCoefficient = 1.0f;

		// TODO refactor this out into state logic
		if (!_wallJumpControlTimer.IsStopped())
		{
			// If the timer is active, we've recently wall jumped, and need to
			// reduce movement input.
			float ratioOfTimeSpent = Mathf.InverseLerp(
				_moveComponent.WallJumpControlTimeout,
				0.0f,
				(float)_wallJumpControlTimer.TimeLeft
			);

			float responseFloor = 0.2f;

			if (ratioOfTimeSpent < responseFloor)
			{
				lerpCoefficient = 0.0f;
			}
			else
			{
				float ratioFromFloorToOne = Mathf.InverseLerp(responseFloor, 1.0f, ratioOfTimeSpent);
				lerpCoefficient = (float)Math.Sin(Math.PI / 2 * ratioFromFloorToOne);
			}
		}


		return Mathf.Lerp(_facing, wantedMovement, lerpCoefficient);
	}

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;

		_moveComponent = GetNode<PlatformerMoveComponent>("PlatformerMoveComponent");
		_controller = GetNode<PlayerController>("PlayerController");
		_stateChart = StateChart.Of(GetNode("StateChart"));

		_wallDetector = GetNode<RayCast2D>("WallDetector");
		UpdateWallDetector();

		#region Initialize timers
		_coyoteTimer = new Timer();
		_coyoteTimer.OneShot = true;
		AddChild(_coyoteTimer);

		_jumpQueueTimer = new Timer();
		_jumpQueueTimer.OneShot = true;
		AddChild(_jumpQueueTimer);

		_wallJumpControlTimer = new Timer();
		_wallJumpControlTimer.OneShot = true;
		AddChild(_wallJumpControlTimer);
		#endregion

	}

	public override void _Process(double delta)
	{
		_animationTree.Set("parameters/Move/blend_position", Velocity.X);

		// Update the sprite direction based on the facing
		if (_facing < 0)
		{
			_sprite.FlipH = true;
		}
		else if (_facing > 0)
		{
			_sprite.FlipH = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Apply the horizontal movement using the move component
		_moveComponent.UpdateHorizontalVelocity(LerpedMovement(), (float)delta);

		// Update facing
		if (Velocity.X > 0)
			_facing = 1;
		else if (Velocity.X < 0)
			_facing = -1;

		UpdateWallDetector();

		_moveComponent.Move();
	}

	public void OnGroundStateEntered()
	{
		if (!_jumpQueueTimer.IsStopped() && _jumpQueueTimer.TimeLeft > 0)
		{
			_stateChart.SendEvent("jump");
			_moveComponent.ApplyFirstJump();
		}

		_jumpQueueTimer.Stop();
	}

	public void OnGroundedStatePhysicsProcess(double delta)
	{
		if (!IsOnFloor())
			_stateChart.SendEvent("airborne");
	}

	public void OnAirborneStatePhysicsProcess(double delta)
	{
		if (!_controller.IsJumpHeld())
			_stateChart.SendEvent("fall");

		if (IsOnFloor())
			_stateChart.SendEvent("grounded");

		if (IsGrabbingWall())
			_stateChart.SendEvent("climb");
	}

	public void OnCoyoteTimeStateEntered()
	{
		_coyoteTimer.Start(_moveComponent.CoyoteTimeDuration);
	}

	public void OnCoyoteTimeStatePhysicsProcess(double delta)
	{
		if (_coyoteTimer.TimeLeft <= 0)
			_stateChart.SendEvent("coyote_time_expired");
	}

	public void OnJumpEnabledStatePhysicsProcess(double delta)
	{
		if (_controller.IsJumpWanted())
		{
			_stateChart.SendEvent("jump");
			_moveComponent.ApplyFirstJump();
		}
	}

	public void OnDoubleDumpEnabledStatePhysicsProcess(double delta)
	{
		if (_controller.IsJumpWanted())
		{
			_stateChart.SendEvent("jump");
			_moveComponent.ApplyDoubleJump();
		}
	}

	public void OnCantJumpStatePhysicsProcess(double delta)
	{
		if (_controller.IsJumpWanted())
			_jumpQueueTimer.Start(_moveComponent.JumpQueueTimeDuration);
	}

	public void OnRisingStatePhysicsProcessing(double delta)
	{
		_moveComponent.ApplyJumpGravity(delta);

		if (Velocity.Y > _moveComponent.BeginFloatingVelocity)
			_stateChart.SendEvent("float");
	}

	public void OnFloatingStatePhysicsProcessing(double delta)
	{
		_moveComponent.ApplyFloatGravity(delta);

		if (Velocity.Y > _moveComponent.BeginFallingVelocity)
			_stateChart.SendEvent("fall");
	}

	public void OnFallingStatePhysicsProcessing(double delta)
	{
		_moveComponent.ApplyFallGravity(delta);
	}

	public void OnClimbingStateEntered()
	{
		_moveComponent.ClearVerticalVelocity();
	}

	public void OnClimbingStatePhysicsProcess(double delta)
	{
		if (IsGrabbingWall())
		{
			if (_controller.IsJumpWanted())
			{
				_moveComponent.ImmediatelyUpdateHorizontalVelocity(-_facing);
				_moveComponent.ApplyFirstJump();
				_stateChart.SendEvent("jump");
				_wallJumpControlTimer.Start(_moveComponent.WallJumpControlTimeout);
			}
		}
		else
		{
			_stateChart.SendEvent("release_climb");
		}
	}
}
