using Godot;
using System;

public partial class CharacterMoveComponent : Node
{
	[ExportGroup("MovementVariables")]

	private int _moveSpeed = 300;
	[Export]
	public int MoveSpeed
	{
		get => _moveSpeed;
		set
		{
			_moveSpeed = value;
			RecalculateDependentProperties();
		}
	}

	private int _jumpHeight = 100;
	[Export]
	public int JumpHeight
	{
		get => _jumpHeight;
		set
		{
			_jumpHeight = value;
			RecalculateDependentProperties();
		}
	}

	private int _jumpDistance = 200;
	[Export]
	public int JumpDistance
	{
		get => _jumpDistance;
		set
		{
			_jumpDistance = value;
			RecalculateDependentProperties();
		}
	}

	public float JumpForce { get; set; }

	public float Gravity { get; private set; }

	private void RecalculateDependentProperties()
	{
		JumpForce = CalculateJumpForce();
		Gravity = CalculateGravity();

		GD.Print("MoveSpeed: ", MoveSpeed);
		GD.Print("JumpHeight: ", JumpHeight);
		GD.Print("JumpDistance: ", JumpDistance);
		GD.Print("JumpForce: ", JumpForce);
		GD.Print("Gravity: ", Gravity);
	}

	// These formulas are based on the famous GDC talk about building
	// a better jump. The short version is, we want our jump height and
	// distance to remain constant to make level design easier.
	private float CalculateJumpForce()
	{
		// v0 = (2 * h * vx) / xh
		return 2 * _jumpHeight * MoveSpeed / (float)_jumpDistance;
	}

	private float CalculateGravity()
	{
		// g = (-2 * h * vx ^ 2) / (xh ^ 2)
		// We return positive instead of negative because Y is positive going down
		// in 2D.
		return 2 * _jumpHeight * (float)Math.Pow(MoveSpeed, 2) / (float)Math.Pow(_jumpDistance, 2);
	}

	public override void _Ready()
	{
		RecalculateDependentProperties();
	}
}
