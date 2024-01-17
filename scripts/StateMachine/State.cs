using Godot;
using System;

[GlobalClass]
public abstract partial class State : Node
{
	protected CharacterBody2D _player;
	protected double _gravity = 9.8f;

	public void Initialize(CharacterBody2D player)
	{
		_player = player;
	}

	public virtual void Enter() { }

	public virtual void Exit() { }

	public virtual State ProcessInput(InputEvent inputEvent) { return this; }

	public virtual State ProcessFrame(double delta) { return this; }

	public virtual State ProcessPhysics(double delta) { return this; }
}
