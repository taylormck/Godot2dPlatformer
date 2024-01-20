using Godot;
using System;

public abstract partial class CharacterMoveComponent : Node
{
	static int TileSize = 16;

	[Export]
	private CharacterBody2D _character;

	[ExportGroup("MovementVariables")]

	private int _moveSpeed = 3;
	[Export]
	public int MoveSpeed
	{
		get => _moveSpeed * TileSize;
		set
		{
			_moveSpeed = value;
			RecalculateDependentProperties();
		}
	}

	private int _jumpHeight = 4;
	[Export]
	public int JumpHeight
	{
		get => _jumpHeight * TileSize;
		set
		{
			_jumpHeight = value;
			RecalculateDependentProperties();
		}
	}



	private int _jumpDistance = 3;
	[Export]
	public int JumpDistance
	{
		get => _jumpDistance * TileSize;
		set
		{
			_jumpDistance = value;
			RecalculateDependentProperties();
		}
	}

	private int _doubleJumpHeight = 2;
	[Export]
	public int DoubleJumpHeight
	{
		get => _doubleJumpHeight * TileSize;
		set
		{
			_doubleJumpHeight = value;
			RecalculateDependentProperties();
		}
	}

	private int _doubleJumpDistance = 2;
	[Export]
	public int DoubleJumpDistance
	{
		get => _doubleJumpDistance * TileSize;
		set
		{
			_doubleJumpDistance = value;
			RecalculateDependentProperties();
		}
	}

	public float JumpVelocity { get; protected set; }
	public float DoubleJumpVelocity { get; protected set; }
	public float Gravity { get; protected set; }

	private void RecalculateDependentProperties()
	{
		JumpVelocity = CalculateJumpForce(JumpHeight, JumpDistance);
		DoubleJumpVelocity = CalculateJumpForce(DoubleJumpHeight, DoubleJumpDistance);
		Gravity = CalculateGravity();
	}

	// These formulas are based on the famous GDC talk about building
	// a better jump. The short version is, we want our jump height and
	// distance to remain constant to make level design easier.
	protected virtual float CalculateJumpForce(float jumpHeight, float jumpSpeed)
	{
		// v0 = (2 * h * vx) / xh
		return 2 * jumpHeight * MoveSpeed / JumpDistance;
	}

	protected virtual float CalculateGravity()
	{
		// g = (-2 * h * vx ^ 2) / (xh ^ 2)
		// We return positive instead of negative because Y is positive going down
		// in 2D.
		return 2 * JumpHeight * (float)Math.Pow(MoveSpeed, 2) / (float)Math.Pow(JumpDistance, 2);
	}

	public override void _Ready()
	{
		RecalculateDependentProperties();
	}

	public abstract float WantsMovement();
	public abstract bool WantsJump();
}
