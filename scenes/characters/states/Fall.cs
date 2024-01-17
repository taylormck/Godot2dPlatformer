using Godot;
using System;

public partial class Fall : State
{

	[ExportGroup("ConnectedStates")]
	[Export]
	private State _idleState;

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


	public override State ProcessPhysics(double delta)
	{
		float horizontalMovement = Input.GetAxis("move_left", "move_right") * _moveComponent.MoveSpeed;

		_player.Velocity = _player.Velocity with
		{
			X = horizontalMovement,
			Y = _player.Velocity.Y + (float)(_moveComponent.Gravity * _gravityMultiplier * delta)
		};

		_player.MoveAndSlide();

		if (_player.IsOnFloor())
		{
			if (horizontalMovement == 0)
				return _idleState;
			return _moveState;
		}

		return this;
	}
}
