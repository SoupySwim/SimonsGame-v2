using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;
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
		public MainGuiObject Owner { get { return _owner; } set { _owner = value; } }
		public bool PreventControls { get; set; }
		public Tuple<Element, float> Element { get; set; }

		#region Abstract Functions

		public abstract bool IsExpired(GameTime gameTime);
		public abstract void Reset();
		public ModifierBase Clone() { return Clone(Guid.Empty); }
		public abstract ModifierBase Clone(Guid id);

		#endregion

		public ModifierBase(ModifyType type, MainGuiObject owner, Tuple<Element, float> element)
			: base()
		{
			_guid = Guid.NewGuid();
			Type = type;
			StopGravity = false;
			if (type == ModifyType.Multiply)
			{
				Movement = new Vector2(1, 1);
				KnockBack = new Vector2(1, 1);
				Acceleration = new Vector2(1, 1);
				MaxSpeed = new Vector2(1, 1);
				CurrentMovement = new Vector2(1, 1);
				_healthTotal = 1;
			}
			_owner = owner;
			Element = element;
		}

		public abstract long GetTickCount();
		public abstract void SetTickCount(long value);

		public static ModifierBase operator +(ModifierBase a, ModifierBase b)
		{
			if (a.Type != b.Type)
				return a;
			a.Movement = new Vector2(a.Movement.X + b.Movement.X, a.Movement.Y + b.Movement.Y);
			a.KnockBack = new Vector2(a.KnockBack.X + b.KnockBack.X, a.KnockBack.Y + b.KnockBack.Y);
			a.Acceleration = new Vector2(a.Acceleration.X + b.Acceleration.X, a.Acceleration.Y + b.Acceleration.Y);
			a.MaxSpeed = new Vector2(a.MaxSpeed.X + b.MaxSpeed.X, a.MaxSpeed.Y + b.MaxSpeed.Y);
			a.CurrentMovement = new Vector2(a.CurrentMovement.X + b.CurrentMovement.X, a.CurrentMovement.Y + b.CurrentMovement.Y);
			if (Math.Abs(a.HealthTotal) < Math.Abs(b.HealthTotal))
				a._owner = b._owner;
			a.SetHealthTotal(a.HealthTotal + b.HealthTotal);

			a.StopGravity = a.StopGravity || b.StopGravity;
			a.PreventControls = a.PreventControls || b.PreventControls;
			return a;
		}
		public static ModifierBase operator *(ModifierBase a, ModifierBase b)
		{
			if (a.Type != b.Type)
				return a;
			a.Movement = new Vector2(a.Movement.X * b.Movement.X, a.Movement.Y * b.Movement.Y);
			a.KnockBack = new Vector2(a.KnockBack.X * b.KnockBack.X, a.KnockBack.Y * b.KnockBack.Y);
			a.Acceleration = new Vector2(a.Acceleration.X * b.Acceleration.X, a.Acceleration.Y * b.Acceleration.Y);
			a.MaxSpeed = new Vector2(a.MaxSpeed.X * b.MaxSpeed.X, a.MaxSpeed.Y * b.MaxSpeed.Y);
			a.CurrentMovement = new Vector2(a.CurrentMovement.X * b.CurrentMovement.X, a.CurrentMovement.Y * b.CurrentMovement.Y);
			if (a.HealthTotal < b.HealthTotal)
				a._owner = b._owner;
			a.SetHealthTotal(a.HealthTotal * b.HealthTotal);

			a.StopGravity = a.StopGravity || b.StopGravity;
			a.PreventControls = a.PreventControls || b.PreventControls;
			return a;
		}
	}
}
