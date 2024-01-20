using Godot;
using System;

public partial class Fall : State
{

	[ExportGroup("ConnectedStates")]
	[Export]
	private State _jumpState;

	[Export]
	private State _moveState;

	[ExportGroup("Components")]
	[Export]
	private CharacterMoveComponent _moveComponent;

	[ExportGroup("MovementProperties")]
	[Export]
	private float _gravityMultiplier = 3.0f;


	public override State ProcessInput(InputEvent inputEvent)
	{
		if (_moveComponent.WantsJump() && _moveComponent.CanDoubleJump)
		{
			_player.Velocity = _player.Velocity with { Y = -_moveComponent.DoubleJumpForce };
			_moveComponent.SpendDoubleJump();
		}

		return this;
	}

	public override State ProcessPhysics(double delta)
	{
		Vector2 currentVelocity = _player.Velocity;
		float horizontalMovement = Input.GetAxis("move_left", "move_right") * _moveComponent.MoveSpeed;

		float verticalMovementMultiplier = currentVelocity.Y > 0 ? _gravityMultiplier : 1;
		float verticalAcceleration = _moveComponent.Gravity * verticalMovementMultiplier * (float)delta;

		_player.Velocity = _player.Velocity with
		{
			X = horizontalMovement,
			Y = _player.Velocity.Y + verticalAcceleration
		};

		_player.MoveAndSlide();

		if (_player.IsOnFloor())
			return _moveState;

		return this;
	}
}
