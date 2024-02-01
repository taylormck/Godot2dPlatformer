using Godot;
using System;

[GlobalClass]
public partial class PlatformerMoveComponent : Node
{
	static int TileSize = 16;

	[Export]
	private CharacterBody2D _character;

	[ExportGroup("Movement Variables")]

	private float _moveSpeed = 15;
	[Export]
	public float MoveSpeed
	{
		get => _moveSpeed * TileSize;
		set
		{
			_moveSpeed = value;
			RecalculateDependentProperties();
		}
	}

	private float _moveAcceleration = 1;
	[Export]
	public float MoveAcceleration
	{
		get => _moveAcceleration * TileSize;
		set => _moveAcceleration = value;
	}

	private float _moveDeceleration = 2;
	[Export]
	public float MoveDeceleration
	{
		get => _moveDeceleration * TileSize;
		set => _moveDeceleration = value;
	}

	[ExportGroup("JumpProperties")]
	private float _jumpHeight = 4;
	[Export]
	public float JumpHeight
	{
		get => _jumpHeight * TileSize;
		set
		{
			_jumpHeight = value;
			RecalculateDependentProperties();
		}
	}

	private float _jumpDistanceToPeak = 3;
	[Export]
	public float JumpDistanceToPeak
	{
		get => _jumpDistanceToPeak * TileSize;
		set
		{
			_jumpDistanceToPeak = value;
			RecalculateDependentProperties();
		}
	}

	private float _jumpDistanceFromPeakToGround = 1;
	[Export]
	public float JumpDistanceFromPeakToGround
	{
		get => _jumpDistanceFromPeakToGround * TileSize;
		set
		{
			_jumpDistanceFromPeakToGround = value;
			RecalculateDependentProperties();
		}
	}

	private float _doubleJumpHeight = 2;
	[Export]
	public float DoubleJumpHeight
	{
		get => _doubleJumpHeight * TileSize;
		set
		{
			_doubleJumpHeight = value;
			RecalculateDependentProperties();
		}
	}

	private float _doubleJumpDistance = 2;
	[Export]
	public float DoubleJumpDistance
	{
		get => _doubleJumpDistance * TileSize;
		set
		{
			_doubleJumpDistance = value;
			RecalculateDependentProperties();
		}
	}

	private float _jumpPeakFloatHeight = 1;
	[Export]
	public float JumpPeakFloatHeight
	{
		get => _jumpPeakFloatHeight * TileSize;
		set
		{
			_jumpPeakFloatHeight = value;
			RecalculateDependentProperties();
		}
	}

	private float _jumpPeakFloatDistance = 2;
	[Export]
	public float JumpPeakFloatDistance
	{
		get => _jumpPeakFloatDistance * TileSize;
		set
		{
			_jumpPeakFloatDistance = value;
			RecalculateDependentProperties();
		}
	}

	private float _jumpPeakFloatSpeed = 4;
	[Export]
	public float JumpPeakFloatSpeed
	{
		get => _jumpPeakFloatSpeed * TileSize;
		set
		{
			_jumpPeakFloatSpeed = value;
			RecalculateDependentProperties();
		}
	}

	[Export]
	public float BeginFloatingVelocity { get; set; } = 75;

	[Export]
	public float BeginFallingVelocity { get; set; } = 0;

	private float _maximumFallSpeed = 50;
	[Export]
	public float MaximumFallSpeed
	{
		get => _maximumFallSpeed * TileSize;
		set
		{
			_maximumFallSpeed = value;
		}
	}

	[Export]
	public float CoyoteTimeDuration { get; set; } = 0.2f;

	[Export]
	public float JumpQueueTimeDuration { get; set; } = 0.2f;

	public float JumpVerticalVelocity { get; private set; }
	public float DoubleJumpVerticalVelocity { get; private set; }
	public float JumpGravity { get; private set; }
	public float FloatGravity { get; private set; }
	public float FallGravity { get; private set; }


	private Vector2 Velocity { get; set; } = Vector2.Zero;

	private void RecalculateDependentProperties()
	{
		// TODO we could probably factor this out to separate methods
		JumpVerticalVelocity = CalculateJumpVerticalVelocity(MoveSpeed, JumpHeight, JumpDistanceToPeak);
		DoubleJumpVerticalVelocity = CalculateJumpVerticalVelocity(MoveSpeed, DoubleJumpHeight, DoubleJumpDistance);
		JumpGravity = CalculateGravity(MoveSpeed, JumpHeight, JumpDistanceToPeak);
		FloatGravity = CalculateGravity(JumpPeakFloatSpeed, JumpPeakFloatHeight, JumpPeakFloatDistance);
		FallGravity = CalculateGravity(MoveSpeed, JumpHeight, JumpDistanceFromPeakToGround);
	}

	// These formulas are based on the famous GDC talk about building
	// a better jump. The short version is, we want our jump height and
	// distance to remain constant to make level design easier.
	private float CalculateJumpVerticalVelocity(float moveSpeed, float jumpHeight, float jumpDistance)
	{
		// v0 = (2 * h * vx) / xh
		return 2 * jumpHeight * moveSpeed / jumpDistance;
	}

	protected virtual float CalculateGravity(float moveSpeed, float jumpHeight, float jumpDistance)
	{
		// g = (-2 * h * vx ^ 2) / (xh ^ 2)
		// We return positive instead of negative because Y is positive going down
		// in 2D.
		return 2 * jumpHeight * (float)Math.Pow(moveSpeed, 2) / (float)Math.Pow(jumpDistance, 2);
	}

	public override void _Ready()
	{
		RecalculateDependentProperties();
	}

	public void UpdateHorizontalVelocity(float input)
	{
		float targetVelocity = input * MoveSpeed;

		// We want to use the deceleration rate if either we are slowing down
		// or changing directions.
		bool decelerating = (
			Math.Abs(targetVelocity) < Math.Abs(Velocity.X) ||
			Math.Sign(targetVelocity) != Math.Sign(Velocity.X)
		);

		float acceleration = decelerating ? MoveDeceleration : MoveAcceleration;

		float newHorizontalVelocity = Mathf.MoveToward(Velocity.X, targetVelocity, acceleration);

		Velocity = Velocity with { X = newHorizontalVelocity };
		_character.Velocity = Velocity;
	}

	private void ApplyJump(float jumpVerticalVelocity)
	{
		Velocity = Velocity with { Y = -jumpVerticalVelocity };
		_character.Velocity = Velocity;
	}

	public void ApplyFirstJump()
	{
		ApplyJump(JumpVerticalVelocity);
	}

	public void ApplyDoubleJump()
	{
		ApplyJump(DoubleJumpVerticalVelocity);
	}

	public void ClearVerticalVelocity()
	{
		Velocity = Velocity with { Y = 0 };
		_character.Velocity = Velocity;
	}

	private void ApplyGravity(double delta, float gravity)
	{
		if (_character.IsOnCeiling() || _character.IsOnFloor())
		{
			ClearVerticalVelocity();
		}

		float verticalAcceleration = gravity * (float)delta;
		float halfVerticalAcceleration = verticalAcceleration / 2.0f;

		// Note that we only apply half the acceleration to the character
		// velocity. This is the Verlet approximation. We add half now, but keep
		// track of the full acceleration in the local Velocity. In the next
		// frame, we'll add the remaining half of this frame's acceleration, as
		// well as half of the next frame's acceleration
		float characterVerticalVelocity = Math.Min(Velocity.Y + halfVerticalAcceleration, MaximumFallSpeed);
		_character.Velocity = Velocity with { Y = characterVerticalVelocity };

		float ownVerticalVelocity = Math.Min(Velocity.Y + verticalAcceleration, MaximumFallSpeed);
		Velocity = Velocity with { Y = ownVerticalVelocity };
	}

	public void ApplyJumpGravity(double delta)
	{
		ApplyGravity(delta, JumpGravity);
	}
	public void ApplyFloatGravity(double delta)
	{
		ApplyGravity(delta, FloatGravity);
	}
	public void ApplyFallGravity(double delta)
	{
		ApplyGravity(delta, FallGravity);
	}

	public void Move()
	{
		_character.MoveAndSlide();
	}
}
