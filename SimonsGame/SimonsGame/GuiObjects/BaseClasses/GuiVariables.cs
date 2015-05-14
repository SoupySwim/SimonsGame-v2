using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SimonsGame.GuiObjects
{
	public enum Team
	{
		None, // Everyone is your enemy
		Neutral, // A "team" for creeps.
		Team1,
		Team2,
		Team3,
		Team4
	}
	/// <summary>
	/// This class is a container for all GUI objects.
	/// Also used to manipulate GUI Objects with Modifiers.
	/// </summary>
	public abstract class GuiVariables
	{
		public static Dictionary<Team, Color> TeamColorMap = new Dictionary<Team, Color>()
		{
			{ Team.None, Color.White },
			{ Team.Neutral, Color.White },
			{ Team.Team1, Color.Red },
			{ Team.Team2, Color.Blue },
			{ Team.Team3, Color.Green },
			{ Team.Team4, Color.Orange }
		};
		protected Guid _guid;
		public Guid Id { get { return _guid; } }

		// Will base all other speeds to this.  Y direction is gravity.
		public static float Gravity { get { return AverageSpeed.Y; } }
		private static Vector2 _averageSpeed = new Vector2(4.2f, 6.5f);
		public static Vector2 AverageSpeed { get { return _averageSpeed; } set { _averageSpeed = value; } }
		//public float Scale { get; set; }

		#region Movement
		// Percentage of MaxSpeeds an object will move in one tick.
		public Vector2 Movement { get; set; }

		// Percentage of movement an object can gain in one tick.  Base is 1
		public Vector2 Acceleration { get; set; }

		// Max speed one can achieve (right now, only utilizing X direction
		public Vector2 MaxSpeed { get; set; }

		// Speed at which the object is currently moving.
		public Vector2 CurrentMovement { get; set; }

		// How much mana you have total (used for certain magic)
		protected float _manaTotal;
		public float ManaTotal { get { return _manaTotal; } }

		// How much health you have total
		protected float _healthTotal;
		public float HealthTotal { get { return _healthTotal; } }
		#endregion
	}
}
