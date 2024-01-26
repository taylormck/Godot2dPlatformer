using System;
using Godot;
using GodotStateCharts;

public partial class Player : CharacterBody2D
{

	private AnimatedSprite2D _animations;
	private PlayerMoveComponent _moveComponent;
	private Sprite2D _sprite;
	private AnimationTree _animationTree;
	private StateChart _stateChart;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;

		_moveComponent = GetNode<PlayerMoveComponent>("PlayerMoveComponent");
		_stateChart = StateChart.Of(GetNode("StateChart"));
	}

	public override void _Process(double delta)
	{
		float direction = _moveComponent.WantedMovement();
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
		float wantedHorizontalVelocity = _moveComponent.WantedMovement();

		_moveComponent.SetHorizontalVelocity();

		MoveAndSlide();

		if (Math.Abs(wantedHorizontalVelocity) > 0.005)
			_stateChart.SendEvent("moving");
		else
			_stateChart.SendEvent("idle");

		if (IsOnFloor())
		{
			_stateChart.SendEvent("grounded");
			_moveComponent.ClearVerticalVelocity();
		}
		else
		{
			_stateChart.SendEvent("airborne");

			if (IsOnCeiling())
			{
				_moveComponent.ClearVerticalVelocity();
			}

			_moveComponent.ApplyGravity(delta);
		}
	}

	public void OnJumpEnabledStatePhysicsProcess(double delta)
	{
		if (_moveComponent.WantsJump())
		{
			_stateChart.SendEvent("jump");
			_moveComponent.ApplyJump();
		}
	}

	public void OnDoubleDumpEnabledStatePhysicsProcess(double delta)
	{
		if (_moveComponent.WantsJump())
		{
			_stateChart.SendEvent("double_jump");
			_moveComponent.ApplyDoubleJump();
		}
	}
}
