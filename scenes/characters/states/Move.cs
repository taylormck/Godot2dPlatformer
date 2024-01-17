using Godot;
using System;


public partial class Move : State
{
	[ExportGroup("ConnectedStates")]
	[Export]
	private State _fallState;

	[Export]
	private State _idleState;

	[Export]
	private State _jumpState;

	[ExportGroup("Components")]
	[Export]
	private CharacterMoveComponent _moveComponent;


	public override State ProcessInput(InputEvent inputEvent)
	{
		// TODO Add coyote time
		if (Input.IsActionJustPressed("jump"))
			return _jumpState;

		return this;
	}

	public override State ProcessPhysics(double delta)
	{
		double horizontalMovement = Input.GetAxis("move_left", "move_right") * _moveComponent.MoveSpeed;

		Vector2 newVelocity = _player.Velocity;
		newVelocity.X = (float)horizontalMovement;
		newVelocity.Y += (float)(_moveComponent.Gravity * delta);

		_player.Velocity = newVelocity;

		_player.MoveAndSlide();

		if (horizontalMovement == 0)
			return _idleState;

		// TODO adjust animation based on direction

		// TODO Add coyote time
		if (!_player.IsOnFloor())
			return _fallState;

		return this;
	}
}
