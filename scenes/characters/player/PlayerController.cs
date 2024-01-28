using Godot;
using System;

[GlobalClass]
public partial class PlayerController : Node
{
	public float WantedMovement()
	{
		return Input.GetAxis("move_left", "move_right");
	}

	public bool IsJumpWanted()
	{
		return Input.IsActionJustPressed("jump");
	}

	public bool IsJumpHeld()
	{
		return Input.IsActionPressed("jump");
	}
}
