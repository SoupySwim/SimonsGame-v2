using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers
{
	public enum ModifyType
	{
		Add,
		Multiply
	}
	public abstract class ModifierBase : GuiVariables
	{
		public ModifyType Type { get; set; }
		public bool StopGravity { get; set; }
		protected bool _hasReachedEnd = false;
		public bool HasReachedEnd { get { return _hasReachedEnd; } } // Used for combos
		protected MainGuiObject _owner;
		public MainGuiObject Owner { get { return _owner; } }

		#region abstract functions

		public abstract bool IsExpired(GameTime gameTime);
		public abstract void Reset();
		public abstract ModifierBase Clone();

		#endregion

		public ModifierBase(ModifyType type, MainGuiObject owner)
			: base()
		{
			_guid = Guid.NewGuid();
			Type = type;
			StopGravity = false;
			if (type == ModifyType.Multiply)
			{
				Movement = new Vector2(1, 1);
				Acceleration = new Vector2(1, 1);
				MaxSpeed = new Vector2(1, 1);
				CurrentMovement = new Vector2(1, 1);
				_healthTotal = 1;
			}
			_owner = owner;
		}
		public void SetHealthTotal(float newHealth)
		{
			_healthTotal = newHealth;
		}
		public static ModifierBase operator +(ModifierBase a, ModifierBase b)
		{
			if (a.Type != b.Type)
				return a;
			a.Movement = new Vector2(a.Movement.X + b.Movement.X, a.Movement.Y + b.Movement.Y);
			a.Acceleration = new Vector2(a.Acceleration.X + b.Acceleration.X, a.Acceleration.Y + b.Acceleration.Y);
			a.MaxSpeed = new Vector2(a.MaxSpeed.X + b.MaxSpeed.X, a.MaxSpeed.Y + b.MaxSpeed.Y);
			a.CurrentMovement = new Vector2(a.CurrentMovement.X + b.CurrentMovement.X, a.CurrentMovement.Y + b.CurrentMovement.Y);
			a.StopGravity = a.StopGravity || b.StopGravity;
			if (Math.Abs(a.HealthTotal) < Math.Abs(b.HealthTotal))
				a._owner = b._owner;
			a.SetHealthTotal(a.HealthTotal + b.HealthTotal);
			return a;
		}
		public static ModifierBase operator *(ModifierBase a, ModifierBase b)
		{
			if (a.Type != b.Type)
				return a;
			a.Movement = new Vector2(a.Movement.X * b.Movement.X, a.Movement.Y * b.Movement.Y);
			a.Acceleration = new Vector2(a.Acceleration.X * b.Acceleration.X, a.Acceleration.Y * b.Acceleration.Y);
			a.MaxSpeed = new Vector2(a.MaxSpeed.X * b.MaxSpeed.X, a.MaxSpeed.Y * b.MaxSpeed.Y);
			a.CurrentMovement = new Vector2(a.CurrentMovement.X * b.CurrentMovement.X, a.CurrentMovement.Y * b.CurrentMovement.Y);
			a.StopGravity = a.StopGravity || b.StopGravity;
			if (a.HealthTotal < b.HealthTotal)
				a._owner = b._owner;
			a.SetHealthTotal(a.HealthTotal * b.HealthTotal);
			return a;
		}
	}
}
