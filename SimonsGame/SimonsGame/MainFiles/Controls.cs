using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;

namespace SimonsGame
{
	public class PlayerControls
	{
		public float XMovement { get; set; }
		public float YMovement { get; set; }
		public Func<Player, Vector2> GetAim { get; set; }
		public AvailableButtons PressedButtons { get; set; }
	}

	public class MouseProperties
	{
		public bool LeftClickDown { get; set; }
		public bool LeftClickPressed { get; set; } // If it was up before and now down.

		public bool RightClickDown { get; set; }
		public bool RightClickPressed { get; set; } // If it was up before and now down.

		public bool MiddleClickDown { get; set; }
		public bool MiddleClickPressed { get; set; } // If it was up before and now down.

		public Vector2 MousePosition { get; set; }
	}

	public enum AvailableButtons
	{
		Default = 0,
		Action = 1,
		Secondary = 2,
		Third = 4,
		Fourth = 8,
		LeftTrigger = 16,
		RightTrigger = 32,
		LeftBumper = 64,
		RightBumper = 128,
		Start = 256,
		Select = 512
	}
	public enum Direction
	{
		Up = 1,
		Left = 2,
		Down = 4,
		Right = 8
	}
	public class Controls
	{
		private static MouseProperties _previousMouseProperties = null;
		public static List<PlayerControls> PlayerControls { get; set; }
		public Controls(int playerCount)
		{
			if (playerCount < 1)
				playerCount = 1;
			PlayerControls = new List<PlayerControls>();
			for (int i = 0; i < playerCount; i++)
			{
				PlayerControls.Add(new PlayerControls());
			}
		}
		//All of this will change soon...
		public static Tuple<MouseProperties, Dictionary<Guid, PlayerControls>> GetControls(PlayerManager playerManager)
		{
			KeyboardState keyboardState = Keyboard.GetState();
			MouseProperties mouseProperties = GetMouseProperties();

			return new Tuple<MouseProperties, Dictionary<Guid, PlayerControls>>(mouseProperties, playerManager.PlayerInputMap.Keys.ToDictionary(id => id, id =>
			{
				UsableInputMap inputMap = null;
				if (playerManager.PlayerInputMap.TryGetValue(id, out inputMap))
				{
					PlayerControls playerControls = new PlayerControls();
					if (inputMap is KeyboardUsableInputMap)
					{
						KeyboardUsableInputMap keyInputMap = (KeyboardUsableInputMap)inputMap;
						playerControls.XMovement = 0;
						playerControls.YMovement = 0;
						if (keyboardState.IsKeyDown(keyInputMap.Right))
							playerControls.XMovement = 1;
						else if (keyboardState.IsKeyDown(keyInputMap.Left))
							playerControls.XMovement = -1;
						if (keyboardState.IsKeyDown(keyInputMap.Up))
							playerControls.YMovement = -1;
						else if (keyboardState.IsKeyDown(keyInputMap.Down))
							playerControls.YMovement = 1;

						playerControls.GetAim = (p) =>
						{
							Vector2 mouseOffset = Vector2.Zero;
							if (p.Level != null && p.Level.GameStateManager != null)
							{
								PlayerViewport viewport = p.Level.GameStateManager.GetPlayerViewport(p);
								mouseOffset = viewport.CameraPosition;
							}
							Vector2 mousePosition = mouseProperties.MousePosition + mouseOffset - p.Center;
							float normalizer = (float)Math.Sqrt(Math.Pow((double)mousePosition.X, 2) + Math.Pow((double)mousePosition.Y, 2));
							return mousePosition / normalizer;
						};

						playerControls.PressedButtons = inputMap.Aggregate(AvailableButtons.Default, (ab, kv) => keyboardState.IsKeyDown((Keys)kv.Value) ? ab |= kv.Key : ab);

						// Hack for now.
						if (mouseProperties.LeftClickDown)
						{
							playerControls.PressedButtons = playerControls.PressedButtons | AvailableButtons.RightTrigger;
						}
						if (mouseProperties.RightClickDown)
						{
							playerControls.PressedButtons = playerControls.PressedButtons | AvailableButtons.RightBumper;
						}
						if (mouseProperties.MiddleClickDown)
						{
							playerControls.PressedButtons = playerControls.PressedButtons | AvailableButtons.LeftTrigger;
						}
					}
					else if (inputMap is ControllerUsableInputMap)
					{
						ControllerUsableInputMap controllerInputMap = (ControllerUsableInputMap)inputMap;
						GamePadState gamePadState = GamePad.GetState(controllerInputMap.PlayerIndex);

						playerControls.GetAim = (p) => gamePadState.ThumbSticks.Right * new Vector2(1, -1);

						playerControls.XMovement = gamePadState.ThumbSticks.Left.X;
						playerControls.YMovement = gamePadState.ThumbSticks.Left.Y < 0
							 ? -gamePadState.ThumbSticks.Left.Y
							 : (gamePadState.IsButtonDown((Buttons)controllerInputMap[AvailableButtons.LeftBumper]) ? -1f : 0f);
						playerControls.PressedButtons = inputMap.Aggregate(AvailableButtons.Default, (ab, kv) => gamePadState.IsButtonDown((Buttons)kv.Value) ? ab |= kv.Key : ab);
					}
					return playerControls;
				}
				return new PlayerControls();
			}));
		}
		private static MouseProperties GetMouseProperties()
		{
			MouseState currentMouse = Mouse.GetState();
			_previousMouseProperties = new MouseProperties()
			{
				LeftClickDown = currentMouse.LeftButton.CompareTo(ButtonState.Pressed) == 0,
				LeftClickPressed = currentMouse.LeftButton.CompareTo(ButtonState.Pressed) == 0 // If it wasn't down last time, then it was just pressed if down
				 && (_previousMouseProperties == null || !_previousMouseProperties.LeftClickDown),

				RightClickDown = currentMouse.RightButton.CompareTo(ButtonState.Pressed) == 0,
				RightClickPressed = currentMouse.RightButton.CompareTo(ButtonState.Pressed) == 0 // If it wasn't down last time, then it was just pressed if down
				 && (_previousMouseProperties == null || !_previousMouseProperties.RightClickDown),

				MiddleClickDown = currentMouse.MiddleButton.CompareTo(ButtonState.Pressed) == 0,
				MiddleClickPressed = currentMouse.MiddleButton.CompareTo(ButtonState.Pressed) == 0 // If it wasn't down last time, then it was just pressed if down
				 && (_previousMouseProperties == null || !_previousMouseProperties.MiddleClickDown),

				MousePosition = new Vector2(currentMouse.X, currentMouse.Y)
			};

			return _previousMouseProperties;
		}
		public static PlayerControls GetEmptyControls()
		{
			return new PlayerControls()
			{
				PressedButtons = AvailableButtons.Default,
				GetAim = (prop) => new Vector2(0, 0),
				XMovement = 0f,
				YMovement = 0f
			};
		}

		public static bool IsDown(Guid guiId /* See what I did there, ha! */, AvailableButtons button)
		{
			try
			{
				return (GameStateManager.AllControls[guiId].PressedButtons & button) == button;
			}
			catch (Exception) { return false; } // if guid doesn't exist, then say no!
		}
		public static bool PressedDown(Guid guiId /* See what I did there, ha! */, AvailableButtons button)
		{
			try
			{
				return PressedDown(GameStateManager.AllControls[guiId], GameStateManager.PreviousControls[guiId], button);
			}
			catch (Exception) { return false; } // if guid doesn't exist, then say no!
		}
		public static bool PressedDown(PlayerControls controls, PlayerControls previousControls, AvailableButtons button)
		{
			return (((previousControls.PressedButtons & button) != button)
				&& ((controls.PressedButtons & button) == button));
		}
		public static bool ReleasedDown(PlayerControls controls, PlayerControls previousControls, AvailableButtons button)
		{
			return (((controls.PressedButtons & button) != button)
				&& ((previousControls.PressedButtons & button) == button));
		}
		public static bool PressingDown(PlayerControls controls, PlayerControls previousControls, AvailableButtons button)
		{
			return (((controls.PressedButtons & button) == button)
				&& ((previousControls.PressedButtons & button) == button));
		}
		public static bool PressedDirectionDown(PlayerControls controls, PlayerControls previousControls, Direction controlDirection, double threshold = .5)
		{
			bool goingThatDirection = true;
			if (controlDirection.HasFlag(Direction.Up))
			{
				goingThatDirection &= controls.YMovement < -threshold && previousControls.YMovement >= -threshold;
			}
			if (controlDirection.HasFlag(Direction.Down))
			{
				goingThatDirection &= controls.YMovement > threshold && previousControls.YMovement <= threshold;
			}
			if (controlDirection.HasFlag(Direction.Left))
			{
				goingThatDirection &= controls.XMovement < -threshold && previousControls.XMovement >= -threshold;
			}
			if (controlDirection.HasFlag(Direction.Right))
			{
				goingThatDirection &= controls.XMovement > threshold && previousControls.XMovement <= threshold;
			}


			return goingThatDirection;
		}


		public void SetPlayerCount(int playerCount)
		{
			PlayerControls = new List<PlayerControls>();
		}

	}
}
