using Godot;
using System;

public abstract partial class CharacterMoveComponent : Node
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

	private int _doubleJumpHeight = 50;
	[Export]
	public int DoubleJumpHeight
	{
		get => _doubleJumpHeight;
		set
		{
			_doubleJumpHeight = value;
			RecalculateDependentProperties();
		}
	}

	private int _doubleJumpDistance = 200;
	[Export]
	public int DoubleJumpDistance
	{
		get => _doubleJumpDistance;
		set
		{
			_doubleJumpDistance = value;
			RecalculateDependentProperties();
		}
	}

	public float JumpForce { get; protected set; }
	public float DoubleJumpForce { get; protected set; }
	public float Gravity { get; protected set; }

	private void RecalculateDependentProperties()
	{
		JumpForce = CalculateJumpForce(JumpHeight, JumpDistance);
		DoubleJumpForce = CalculateJumpForce(DoubleJumpHeight, DoubleJumpDistance);
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
		RefreshDoubleJump();
	}

	public abstract float WantsMovement();
	public abstract bool WantsJump();

	private bool _canDoubleJump = false;
	public virtual bool CanDoubleJump { get; protected set; } = false;
	public virtual void RefreshDoubleJump() { }
	public virtual void SpendDoubleJump() { }
}
