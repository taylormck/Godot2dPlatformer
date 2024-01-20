using Godot;


public partial class Jump : State
{
	[ExportGroup("ConnectedStates")]
	[Export]
	private State _fallState;

	[Export]
	private State _moveState;

	[ExportGroup("Components")]
	[Export]
	private CharacterMoveComponent _moveComponent;

	[ExportGroup("JumpProperties")]
	[Export]
	private float _minJumpHeight;

	public override void Enter()
	{
		// TODO play animation
		_player.Velocity = _player.Velocity with { Y = -_moveComponent.JumpForce };
	}

	public override State ProcessInput(InputEvent inputEvent)
	{
		// TODO doesn't work with controllers because they don't continuously
		// send the pressed signal
		// if (!_moveComponent.WantsJump())
		// 	return _fallState;

		if (_moveComponent.WantsJump() && _moveComponent.CanDoubleJump)
		{
			_player.Velocity = _player.Velocity with { Y = -_moveComponent.DoubleJumpForce };
			_moveComponent.SpendDoubleJump();
		}

		return this;
	}

	public override State ProcessPhysics(double delta)
	{
		float horizontalMovement = _moveComponent.WantsMovement();
		float verticalMovement = _player.Velocity.Y + (float)(_moveComponent.Gravity * delta);
		_player.Velocity = _player.Velocity with
		{
			X = horizontalMovement,
			Y = verticalMovement,
		};

		// TODO flip animation on negative horizontal movement

		_player.MoveAndSlide();

		if (_player.IsOnFloor())
			return _moveState;

		if (verticalMovement > 0)
			return _fallState;

		return this;
	}
}
