using Godot;
using System;

public partial class Jump : State
{
	[ExportGroup("ConnectedStates")]
	[Export]
	private State _fallState;

	[Export]
	private State _idleState;

	[Export]
	private State _moveState;

	[ExportGroup("Components")]
	[Export]
	private CharacterMoveComponent _moveComponent;

	public override void Enter()
	{
		// TODO play animation
		_player.Velocity = _player.Velocity with { Y = -_moveComponent.JumpForce };
	}

	public override State ProcessInput(InputEvent inputEvent)
	{
		// TODO add double-jump
		return this;
	}

	public override State ProcessPhysics(double delta)
	{
		float horizontalMovement = Input.GetAxis("move_left", "move_right") * _moveComponent.MoveSpeed;
		float verticalMovement = _player.Velocity.Y + (float)(_moveComponent.Gravity * delta);
		_player.Velocity = _player.Velocity with
		{
			X = horizontalMovement,
			Y = verticalMovement,
		};

		// TODO flip animation on negative horizontal movement

		_player.MoveAndSlide();

		if (_player.IsOnFloor())
		{
			if (horizontalMovement == 0)
				return _idleState;
			return _moveState;
		}

		if (verticalMovement > 0)
			return _fallState;

		return this;
	}
}
