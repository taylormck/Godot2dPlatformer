using Godot;


public partial class Move : State
{
	[ExportGroup("ConnectedStates")]
	[Export]
	private State _fallState;

	[Export]
	private State _jumpState;

	[ExportGroup("Components")]
	[Export]
	private CharacterMoveComponent _moveComponent;

	public override void Enter()
	{
		_moveComponent.RefreshDoubleJump();
	}

	public override State ProcessInput(InputEvent inputEvent)
	{
		// TODO Add coyote time
		if (_moveComponent.WantsJump())
			return _jumpState;

		return this;
	}

	public override State ProcessPhysics(double delta)
	{
		float horizontalMovement = _moveComponent.WantsMovement();

		Vector2 newVelocity = _player.Velocity;
		newVelocity.X = horizontalMovement;
		newVelocity.Y += (float)(_moveComponent.Gravity * delta);

		_player.Velocity = newVelocity;

		_player.MoveAndSlide();

		// TODO Add coyote time
		if (!_player.IsOnFloor())
			return _fallState;

		return this;
	}
}
