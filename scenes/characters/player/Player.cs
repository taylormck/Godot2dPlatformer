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

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;

		_moveComponent = GetNode<PlatformerMoveComponent>("PlatformerMoveComponent");
		_controller = GetNode<PlayerController>("PlayerController");
		_stateChart = StateChart.Of(GetNode("StateChart"));
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

		_stateChart.SendEvent(Math.Abs(wantedHorizontalVelocity) > 0.005 ? "moving" : "idle");
		_stateChart.SendEvent(IsOnFloor() ? "grounded" : "airborne");

		_stateChart.SetExpressionProperty("vertical_velocity", Velocity.Y);
	}

	public void OnGroundedStatePhysicsProcess(double delta)
	{
		if (IsOnFloor())
			_moveComponent.ClearVerticalVelocity();
	}

	public void OnAirborneStateEntered()
	{
		_stateChart.SetExpressionProperty("vertical_velocity", Velocity.Y);
	}

	public void OnAirborneStatePhysicsProcess(double delta)
	{
		if (!_controller.IsJumpHeld())
			_stateChart.SendEvent("fall");

		if (IsOnFloor() || IsOnCeiling())
			_moveComponent.ClearVerticalVelocity();
	}

	public void OnJumpEnabledStatePhysicsProcess(double delta)
	{
		if (_controller.IsJumpWanted())
		{
			_stateChart.SendEvent("jump");
			_moveComponent.ApplyFirstJump();
			_stateChart.SetExpressionProperty("vertical_velocity", Velocity.Y);
		}
	}

	public void OnDoubleDumpEnabledStatePhysicsProcess(double delta)
	{
		if (_controller.IsJumpWanted())
		{
			_stateChart.SendEvent("jump");
			_moveComponent.ApplyDoubleJump();
			_stateChart.SetExpressionProperty("vertical_velocity", Velocity.Y);
		}
	}

	public void OnRisingStatePhysicsProcessing(double delta)
	{
		_moveComponent.ApplyJumpGravity(delta);
	}

	public void OnFloatingStatePhysicsProcessing(double delta)
	{
		_moveComponent.ApplyFloatGravity(delta);
	}

	public void OnFallingStatePhysicsProcessing(double delta)
	{
		_moveComponent.ApplyFallGravity(delta);
	}
}
