using Godot;
using System;

public abstract partial class CharacterMoveComponent : Node
{
	static int TileSize = 16;

	[Export]
	protected CharacterBody2D _character;

	[ExportGroup("Movement Variables")]

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

	private int _moveAcceleration = 1;
	[Export]
	public int MoveAcceleration
	{
		get => _moveAcceleration * TileSize;
		set => _moveAcceleration = value;
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

	private Vector2 Velocity { get; set; } = Vector2.Zero;

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

	public abstract float WantedMovement();

	public virtual void SetHorizontalVelocity()
	{
		// TODO Decelerate faster than we accelerate
		float horizontalVelocity = Mathf.MoveToward(Velocity.X, WantedMovement(), MoveAcceleration);
		Velocity = Velocity with { X = horizontalVelocity };
		_character.Velocity = Velocity;
	}

	public abstract bool WantsJump();

	public virtual void ApplyJump()
	{
		Velocity = Velocity with { Y = -JumpVelocity };
		_character.Velocity = Velocity;
	}

	public virtual void ApplyDoubleJump()
	{
		Velocity = Velocity with { Y = -DoubleJumpVelocity };
	}

	public virtual void ApplyGravity(double delta)
	{
		float verticalAcceleration = Gravity * (float)delta;
		float halfVerticalAcceleration = verticalAcceleration / 2.0f;

		// Note that we only apply half the velocity to the character velocity.
		// This is the Verlet approximation. We add half now, but keep track
		// of the full acceleration in the local Velocity. In the next frame,
		// we'll add the remaining half of this frame's acceleration, as well
		// as half of the next frame's acceleration
		_character.Velocity = Velocity with { Y = Velocity.Y + halfVerticalAcceleration };

		Velocity = Velocity with { Y = Velocity.Y + verticalAcceleration };
	}

	public virtual void ClearVerticalVelocity()
	{
		Velocity = Velocity with { Y = 0 };
		_character.Velocity = Velocity;
	}
}
