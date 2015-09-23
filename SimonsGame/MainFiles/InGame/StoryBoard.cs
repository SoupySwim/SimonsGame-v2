using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility.ObjectAnimations;
using System;
using System.Collections.Generic;
using System.Linq;
using SimonsGame.Extensions;
using System.Text;
using SimonsGame.Dialogue;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers.Abilities;
using SimonsGame.GuiObjects.Zones;
using SimonsGame.Modifiers;

namespace SimonsGame.Story
{
	public enum StoryBoardEnd
	{
		PlayerCyclesThroughActions,
		Duration,
		EnterZone,
		HasBeenDestroyed,
	}

	public enum StoryBoardPhaseBlock
	{
		None,
		JustPlayers,
		AllCharacters
	}

	public enum StoryBoardAction
	{
		MoveRight,
		MoveLeft,
		UseButton,
		Talk,
		ActivateFunction,
		DeactivateFunction,
		Remove,
		AddToLevel,
		PlaceHolder,
	}

	public class StoryBoard
	{
		private Level _level;
		private List<StoryBoardPhase> Phases;
		private StoryBoardPhase _currentPhase;

		public StoryBoard(Level level)
		{
			_level = level;
			Phases = new List<StoryBoardPhase>();
		}

		public void AddPhase(StoryBoardPhase phase)
		{
			Phases.Add(phase);
			if (_currentPhase == null)
				_currentPhase = phase;
		}

		public void Update(GameTime gameTime)
		{
			if (_currentPhase != null)
			{
				_currentPhase.Update(gameTime);
				if (_currentPhase.HasEnded())
				{
					_currentPhase.EndPhase();
					Phases.Remove(_currentPhase);
					_currentPhase = Phases.FirstOrDefault();
					if (_currentPhase != null) _currentPhase.StartPhase();
				}
			}
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (_currentPhase != null)
				_currentPhase.Draw(gameTime, spriteBatch);
		}

		public bool IsStoryComplete()
		{
			return _currentPhase == null;
		}
	}

	// A phase carries out actions until something happens to end the phase.
	public class StoryBoardPhase
	{
		private StoryBoardEnd _storyBoardEnd;
		private Level _level;
		private Vector4 _levelSize;
		private List<StoryBoardPhaseAction> _actions;
		private StoryBoardPhaseBlock _blockLevel;

		#region Specific End Conditions
		private TickTimer _durationTimer;
		private Vector4 _intersectionBounds;
		#endregion

		public StoryBoardPhase(Level level, StoryBoardEnd storyBoardEnd, StoryBoardPhaseBlock blockLevel)
		{
			_level = level;
			_levelSize = new Vector4(0, 0, _level.Size.Y, _level.Size.X);
			_storyBoardEnd = storyBoardEnd;
			_blockLevel = blockLevel;
			_actions = new List<StoryBoardPhaseAction>();
		}

		public void SetDuration(int durationTicks)
		{
			_durationTimer = new TickTimer(durationTicks, () => { }, false);
			_durationTimer.Restart();
		}

		public void SetIntersectionBounds(Vector4 intersectionBounds)
		{
			_intersectionBounds = intersectionBounds;
		}

		public void Update(GameTime gameTime)
		{
			// Make sure blocked properly.
			if (_blockLevel == StoryBoardPhaseBlock.AllCharacters)
			{
				foreach (MainGuiObject mgo in _level.GetAllMovableCharacters(_levelSize))
				{
					mgo.CompletelyStopAllActivity = true;
				}
			}
			else if (_blockLevel == StoryBoardPhaseBlock.JustPlayers)
			{
				foreach (var playerkv in _level.Players)
				{
					playerkv.Value.CompletelyStopAllActivity = true;
				}
			}

			if (_storyBoardEnd == StoryBoardEnd.Duration)
			{
				_durationTimer.Update(gameTime);

				foreach (var action in _actions.ToList())
				{
					action.Update(gameTime);
				}
			}
			else if (_storyBoardEnd == StoryBoardEnd.EnterZone)
			{
				foreach (var action in _actions.ToList())
				{
					action.Update(gameTime);
				}
			}
			else if (_storyBoardEnd == StoryBoardEnd.PlayerCyclesThroughActions)
			{
				StoryBoardPhaseAction action = _actions.FirstOrDefault();
				if (action != null)
				{
					action.Update(gameTime);
					foreach (var playerkv in _level.Players)
					{
						if (Controls.PressedDown(playerkv.Value.Id, AvailableButtons.Action))
							_actions.Remove(action);
					}
				}
			}
		}

		// Check End Condition
		public bool HasEnded()
		{
			if (_storyBoardEnd == StoryBoardEnd.Duration)
			{
				return !_durationTimer.IsRunning(); // If it's not running, then it's done with the phase.
			}
			else if (_storyBoardEnd == StoryBoardEnd.EnterZone)
			{
				foreach (var playerkv in _level.Players)
				{
					if (MainGuiObject.GetIntersectionDepth(playerkv.Value.Bounds, _intersectionBounds) != Vector2.Zero)
						return true;
				}
			}
			else if (_storyBoardEnd == StoryBoardEnd.PlayerCyclesThroughActions)
			{
				return !_actions.Any(); // No more actions that require actions to pass through.
			}
			else if (_storyBoardEnd == StoryBoardEnd.HasBeenDestroyed)
			{
				return _actions.First().Character.HealthCurrent <= 0; // No more actions that require actions to pass through.
			}

			return false;
		}

		public void StartPhase()
		{
			foreach (MainGuiObject mgo in _level.GetAllMovableCharacters(_levelSize))
			{
				mgo.CurrentMovement.X = 0;
			}
			foreach (var action in _actions)
			{
				if (action.DoesBlockControls())
				{
					action.Character.CompletelyStopAllActivity = true;
				}
			}
		}

		public void EndPhase()
		{
			// Make sure unblocked properly.
			if (_blockLevel == StoryBoardPhaseBlock.AllCharacters)
			{
				foreach (MainGuiObject mgo in _level.GetAllMovableCharacters(_levelSize))
				{
					mgo.CompletelyStopAllActivity = false;
				}
			}

			else if (_blockLevel == StoryBoardPhaseBlock.JustPlayers)
			{
				foreach (var playerkv in _level.Players)
				{
					playerkv.Value.CompletelyStopAllActivity = false;
				}
			}

			foreach (StoryBoardPhaseAction action in _actions.ToList())
				action.EndAction();
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (_storyBoardEnd == StoryBoardEnd.PlayerCyclesThroughActions)
			{
				StoryBoardPhaseAction action = _actions.FirstOrDefault();
				if (action != null)
					action.Draw(gameTime, spriteBatch);
			}
			else
			{
				foreach (var action in _actions.ToList())
				{
					action.Draw(gameTime, spriteBatch);
				}
			}
		}

		public void AddAction(StoryBoardPhaseAction action)
		{
			if (action.Action == StoryBoardAction.Talk && _storyBoardEnd == StoryBoardEnd.PlayerCyclesThroughActions)
			{
				action.AddNextTextMarker();
			}
			_actions.Add(action);
		}
	}

	// An action is something that happens to a character.
	public class StoryBoardPhaseAction
	{
		private MainGuiObject _character;
		public MainGuiObject Character { get { return _character; } }
		private StoryBoardAction _action;
		public StoryBoardAction Action { get { return _action; } }
		private TextOverhead Text;
		private AvailableButtons _buttonToPress;
		private PhysicsObject _aimAtCharacter;
		private int _functionIndex;
		private ModifierBase _modifier;
		private bool _alreadyHappened = false;

		public StoryBoardPhaseAction(MainGuiObject character, StoryBoardAction action)
		{
			_character = character;
			_action = action;

			// If we are to add it, it must first be removed.
			if (_action == StoryBoardAction.AddToLevel)
			{
				_character.Level.RemoveGuiObject(_character);
			}
		}

		public void AddDialogue(TextOverhead text)
		{
			Text = text;
		}

		public void AddNextTextMarker()
		{
			Text.SetPressToContine();
		}

		public void AddButton(AvailableButtons button, PhysicsObject aimAtObject)
		{
			_buttonToPress = button;
			_aimAtCharacter = aimAtObject;
		}
		public void AddFunctionIndex(int index)
		{
			_functionIndex = index;
		}
		public bool DoesBlockControls()
		{
			return _action == StoryBoardAction.MoveLeft || _action == StoryBoardAction.MoveRight;
		}
		public void Update(GameTime gameTime)
		{
			if (_action == StoryBoardAction.MoveLeft)
			{
				_character.CurrentMovement.X = -1 * _character.MaxSpeedBase.X;
			}
			else if (_action == StoryBoardAction.MoveRight)
			{
				_character.CurrentMovement.X = _character.MaxSpeedBase.X;
			}
			else if (_action == StoryBoardAction.UseButton)
			{
				if (_modifier == null)
				{
					PhysicsObject pmgo = _character as PhysicsObject;
					if (pmgo != null)
					{
						Vector2 aim = _aimAtCharacter.Center - _character.Center;
						float normalizer = (float)Math.Sqrt(Math.Pow((double)aim.X, 2) + Math.Pow((double)aim.Y, 2));
						pmgo.OverrideAim = aim / normalizer;
						_modifier = new PullAbility(pmgo);
					}
				}
				if (_modifier.IsExpired(gameTime))
				{
				}
			}
			else if (_action == StoryBoardAction.Remove)
			{
				if (!_alreadyHappened)
				{
					_alreadyHappened = true;
					_character.Level.RemoveGuiObject(_character);
				}
			}
			else if (_action == StoryBoardAction.AddToLevel)
			{
				if (!_alreadyHappened)
				{
					_alreadyHappened = true;
					_character.Level.AddGuiObject(_character);
				}
			}
			else if (_action == StoryBoardAction.ActivateFunction)
			{
				foreach (var zoneTuple in _character.Level.GetAllZones())
				{
					StoryZone storyZone = zoneTuple.Value as StoryZone;
					if (storyZone != null && storyZone.StoryPoint == _functionIndex)
					{
						storyZone.IsActiveForFunction = true;
					}
				}
			}
			else if (_action == StoryBoardAction.DeactivateFunction)
			{
				foreach (var zoneTuple in _character.Level.GetAllZones())
				{
					StoryZone storyZone = zoneTuple.Value as StoryZone;
					if (storyZone != null && storyZone.StoryPoint == _functionIndex)
					{
						storyZone.IsActiveForFunction = false;
					}
				}
			}
			else if (_action == StoryBoardAction.Talk)
			{
				Text.Update(gameTime);
			}
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (_action == StoryBoardAction.Talk)
				Text.Draw(gameTime, spriteBatch);
		}

		public void EndAction()
		{
			if (DoesBlockControls())
				_character.CompletelyStopAllActivity = false;

			_character.CurrentMovement.X = 0;
			if (_action == StoryBoardAction.UseButton)
			{
				PhysicsObject pmgo = _character as PhysicsObject;
				if (pmgo != null)
					pmgo.OverrideAim = Vector2.Zero;
			}
		}
	}
}
