using Godot;
using System;

public partial class Player : CharacterBody2D
{

	private AnimatedSprite2D _animations;
	private StateMachine _stateMachine;
	private PlayerMoveComponent _moveComponent;
	private Sprite2D _sprite;
	private AnimationTree _animationTree;

	public override void _Ready()
	{
		_stateMachine = GetNode<StateMachine>("StateMachine");
		_stateMachine.Initialize(this);

		_animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;

		_moveComponent = GetNode<PlayerMoveComponent>("PlayerMoveComponent");
		_sprite = GetNode<Sprite2D>("Sprite2D");
	}

	public override void _Process(double delta)
	{
		_stateMachine.ProcessFrame(delta);

		float direction = _moveComponent.WantsMovement();
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
		_stateMachine.ProcessPhysics(delta);
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		_stateMachine.ProcessInput(inputEvent);
	}
}
