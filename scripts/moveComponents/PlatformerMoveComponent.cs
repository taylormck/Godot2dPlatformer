using Godot;
using System;

[GlobalClass]
public partial class PlatformerMoveComponent : Node
{
	static int TileSize = 16;

	[Export]
	private CharacterBody2D _character;

	[ExportGroup("Movement Variables")]

	private float _moveSpeed = 4.0f;
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

	private float _moveAcceleration = 10.0f;
	[Export]
	public float MoveAcceleration
	{
		get => _moveAcceleration * TileSize;
		set => _moveAcceleration = value;
	}

	private float _moveDeceleration = 15.0f;
	[Export]
	public float MoveDeceleration
	{
		get => _moveDeceleration * TileSize;
		set => _moveDeceleration = value;
	}

	[ExportGroup("JumpProperties")]
	private float _jumpHeight = 4.0f;
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

	private float _jumpDistanceToPeak = 3.0f;
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

	private float _jumpDistanceFromPeakToGround = 1.0f;
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

	private float _doubleJumpHeight = 2.0f;
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

	private float _doubleJumpDistance = 2.0f;
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

	private float _jumpPeakFloatHeight = 1.0f;
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

	private float _jumpPeakFloatDistance = 2.0f;
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

	private float _jumpPeakFloatSpeed = 4.0f;
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
	public float BeginFloatingVelocity { get; set; } = 75.0f;

	[Export]
	public float BeginFallingVelocity { get; set; } = 0.0f;

	private float _maximumFallSpeed = 50.0f;
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

	[Export]
	public float WallJumpControlTimeout { get; set; } = 0.5f;


	private float _pendingGravity = 0;

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

	public void ImmediatelyUpdateHorizontalVelocity(float input)
	{
		float targetVelocity = input * MoveSpeed;
		_character.Velocity = _character.Velocity with { X = targetVelocity };
	}

	public void UpdateHorizontalVelocity(float input, float delta)
	{
		float targetVelocity = input * MoveSpeed;

		// We want to use the deceleration rate if either we are slowing down
		// or changing directions.
		// NOTE: very important to use `Math.Sign` and not `Mathf.Sign`.
		// `Mathf.Sign` doesn't return a distinct value for `0`, but `Math.Sign`
		// does.
		bool decelerating = (
			Math.Abs(targetVelocity) < Math.Abs(_character.Velocity.X) ||
			Math.Sign(targetVelocity) != Math.Sign(_character.Velocity.X)
		);

		// Get our base acceleration
		float acceleration = decelerating ? MoveDeceleration : MoveAcceleration;

		// Apply our acceleration to the character's movement.
		float newHorizontalVelocity = Mathf.MoveToward(_character.Velocity.X, targetVelocity, acceleration * delta);

		// TODO preserve momentum if we are launched by moving platforms and the like

		_character.Velocity = _character.Velocity with { X = newHorizontalVelocity };
	}

	private void ApplyJump(float jumpVerticalVelocity)
	{
		_pendingGravity = 0.0f;
		_character.Velocity = _character.Velocity with { Y = -jumpVerticalVelocity };
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
		_character.Velocity = _character.Velocity with { Y = 0.0f };
		_pendingGravity = 0.0f;
	}

	private void ApplyGravity(double delta, float gravity)
	{
		float verticalAcceleration = gravity * (float)delta;
		float halfVerticalAcceleration = verticalAcceleration / 2.0f;

		// Note that we only apply half the acceleration to the character
		// velocity. This is the Verlet approximation. We add half now, but keep
		// track of the full acceleration in the local Velocity. In the next
		// frame, we'll add the remaining half of this frame's acceleration, as
		// well as half of the next frame's acceleration
		float wantedVerticalVelocity = _character.Velocity.Y + halfVerticalAcceleration + _pendingGravity;

		// Limit ourselves to a terminal velocity
		float clampedVerticalVelocity = Math.Min(wantedVerticalVelocity, MaximumFallSpeed);

		// Apply the new velocity to the character
		_character.Velocity = _character.Velocity with { Y = clampedVerticalVelocity };

		// Save the remaining gravity from this frame to be applied the next
		// time this method is called.
		_pendingGravity = halfVerticalAcceleration;
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
