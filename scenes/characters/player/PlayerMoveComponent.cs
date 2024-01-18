using Godot;
using System;

[GlobalClass]
public partial class PlayerMoveComponent : CharacterMoveComponent
{
	public override float WantsMovement()
	{
		return Input.GetAxis("move_left", "move_right") * MoveSpeed;
	}

	public override bool WantsJump()
	{
		return Input.IsActionJustPressed("jump");
	}

	public override void RefreshDoubleJump()
	{
		CanDoubleJump = true;
	}

	public override void SpendDoubleJump()
	{
		CanDoubleJump = false;
	}
}
