using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Idle : State
{

	[ExportGroup("ConnectedStates")]
	[Export]
	private State _fallState;

	[Export]
	private State _jumpState;

	[Export]
	private State _moveState;

	[ExportGroup("Components")]
	[Export]
	private CharacterMoveComponent _moveComponent;


	public override void Enter()
	{
		_player.Velocity = _player.Velocity with { Y = 0.0f };
		// Play animation
	}

	public override State ProcessInput(InputEvent inputEvent)
	{
		if (Input.IsActionJustPressed("jump"))
			return _jumpState;

		if (Input.IsActionJustPressed("move_left") || Input.IsActionJustPressed("move_right"))
			return _moveState;

		return this;
	}

	public override State ProcessPhysics(double delta)
	{
		_player.Velocity = _player.Velocity with { Y = _player.Velocity.Y + (float)(_moveComponent.Gravity * delta) };
		_player.MoveAndSlide();

		// TODO add coyote time
		if (!_player.IsOnFloor())
		{
			return _fallState;
		}

		return this;
	}

}
