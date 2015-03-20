using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Test
{
	public class TempControls
	{
		//This would be read from a file?
		public static UsableInputMap GetPlayerInput(int playerNumber)
		{
			if (playerNumber > 0)
			{
				ControllerUsableInputMap inputMap = new ControllerUsableInputMap();
				inputMap.IsAi = false;
				inputMap.Add(AvailableButtons.Action, (int)Buttons.A);
				inputMap.Add(AvailableButtons.Secondary, (int)Buttons.B);
				inputMap.Add(AvailableButtons.Third, (int)Buttons.X);
				inputMap.Add(AvailableButtons.Fourth, (int)Buttons.Y);
				inputMap.Add(AvailableButtons.LeftTrigger, (int)Buttons.LeftTrigger);
				inputMap.Add(AvailableButtons.RightTrigger, (int)Buttons.RightTrigger);
				inputMap.Add(AvailableButtons.LeftBumper, (int)Buttons.LeftShoulder);
				inputMap.Add(AvailableButtons.RightBumper, (int)Buttons.RightShoulder);
				inputMap.Add(AvailableButtons.Start, (int)Buttons.Start);
				inputMap.Add(AvailableButtons.Select, (int)Buttons.Back);
				inputMap.PlayerIndex = playerNumber == 1 ? PlayerIndex.One : PlayerIndex.Two; // Three players for now

				return inputMap;
			}
			else if (playerNumber == 0)
			{
				KeyboardUsableInputMap inputMap = new KeyboardUsableInputMap();
				inputMap.IsAi = false;
				inputMap.Add(AvailableButtons.Action, (int)Keys.Q);
				inputMap.Add(AvailableButtons.Secondary, (int)Keys.E);
				inputMap.Add(AvailableButtons.Third, (int)Keys.Z);
				inputMap.Add(AvailableButtons.Fourth, (int)Keys.X);
				inputMap.Add(AvailableButtons.LeftTrigger, (int)Keys.D1);
				inputMap.Add(AvailableButtons.RightTrigger, (int)Keys.D2);
				inputMap.Add(AvailableButtons.LeftBumper, (int)Keys.D3);
				inputMap.Add(AvailableButtons.RightBumper, (int)Keys.D4);
				inputMap.Add(AvailableButtons.Start, (int)Keys.Enter);
				inputMap.Add(AvailableButtons.Select, (int)Keys.RightShift);
				inputMap.Up = Keys.W;
				inputMap.Down = Keys.S;
				inputMap.Left = Keys.A;
				inputMap.Right = Keys.D;

				return inputMap;
			}
			return null;
		}
	}
}
