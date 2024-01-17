using Godot;
using System;
using System.Runtime.Serialization;

[GlobalClass]
public partial class StateMachine : Node
{

	[Export]
	private State _startingState;

	private State _currentState;


	public void Initialize(CharacterBody2D player)
	{
		foreach (State child in GetChildren())
			child.Initialize(player);

		_ChangeState(_startingState);
	}

	private void _ChangeState(State newState)
	{
		if (newState != _currentState)
		{
			_currentState?.Exit();
			_currentState = newState;
			_currentState.Enter();
		}
	}

	public void ProcessInput(InputEvent inputEvent)
	{
		State newState = _currentState.ProcessInput(inputEvent);
		_ChangeState(newState);
	}

	public void ProcessFrame(double delta)
	{
		State newState = _currentState.ProcessFrame(delta);
		_ChangeState(newState);
	}

	public void ProcessPhysics(double delta)
	{
		State newState = _currentState.ProcessPhysics(delta);
		_ChangeState(newState);
	}
}
