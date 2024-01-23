using Godot;
using System;

[GlobalClass]
public partial class PlayerMoveComponent : CharacterMoveComponent
{
	[ExportGroup("Player Specific Properties")]
	[Export]
	private float _minJumpHeight = 3;

	public override float WantedMovement()
	{
		return Input.GetAxis("move_left", "move_right") * MoveSpeed;
	}

	public override bool WantsJump()
	{
		return Input.IsActionJustPressed("jump");
	}

	public bool JumpHeld()
	{
		return Input.IsActionPressed("jump");
	}
}
