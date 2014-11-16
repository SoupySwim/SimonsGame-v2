using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SimonsGame.GuiObjects
{
	/// <summary>
	/// This class is a container for all GUI objects.
	/// Also used to manipulate GUI Objects with Modifiers.
	/// </summary>
	public abstract class GuiVariables
	{
		protected Guid _guid;
		public Guid Id { get { return _guid; } }

		// Will base all other speeds to this.  Y direction is gravity.
		private static Vector2 _averageSpeed = new Vector2(4.2f, 5.5f);
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
