using Godot;
using System;

public partial class Player : CharacterBody2D
{

	private AnimatedSprite2D _animations;
	private StateMachine _stateMachine;

	public override void _Ready()
	{
		_stateMachine = GetNode<StateMachine>("StateMachine");
		_stateMachine.Initialize(this);
	}

	public override void _Process(double delta)
	{
		_stateMachine.ProcessFrame(delta);
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
