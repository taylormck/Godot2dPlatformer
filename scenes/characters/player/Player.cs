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

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;

		_moveComponent = GetNode<PlatformerMoveComponent>("PlatformerMoveComponent");
		_controller = GetNode<PlayerController>("PlayerController");
		_stateChart = StateChart.Of(GetNode("StateChart"));

		#region Initialize timers
		_coyoteTimer = new Timer();
		_coyoteTimer.OneShot = true;
		AddChild(_coyoteTimer);

		_jumpQueueTimer = new Timer();
		_jumpQueueTimer.OneShot = true;
		AddChild(_jumpQueueTimer);
		#endregion
	}

	public override void _Process(double delta)
	{
		float direction = _controller.WantedMovement();
		_animationTree.Set("parameters/Move/blend_position", direction);

		if (direction < 0)
		{
			_sprite.FlipH = true;
		}
		else if (direction > 0)
		{
			_sprite.FlipH = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float wantedHorizontalVelocity = _controller.WantedMovement();

		_moveComponent.UpdateHorizontalVelocity(wantedHorizontalVelocity);
		_moveComponent.Move();
	}

	public void OnGroundedStatePhysicsProcess(double delta)
	{
		if (!IsOnFloor())
			_stateChart.SendEvent("airborne");
	}

	public void OnAirborneStateEntered()
	{
	}

	public void OnAirborneStatePhysicsProcess(double delta)
	{
		if (!_controller.IsJumpHeld())
			_stateChart.SendEvent("fall");

		if (IsOnFloor())
			_stateChart.SendEvent("grounded");

		if (IsOnFloor() || IsOnCeiling())
			_moveComponent.ClearVerticalVelocity();
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
}
