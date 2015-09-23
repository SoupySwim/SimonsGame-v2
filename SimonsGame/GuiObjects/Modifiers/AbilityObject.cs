using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class AbilityObject : AffectedSpace
	{
		private static Dictionary<string, Func<PhysicsObject, PlayerAbilityInfo>> _magicNameMap = new Dictionary<string, Func<PhysicsObject, PlayerAbilityInfo>>()
		{
			{ "Daft", AbilityBuilder.GetDaftAbility },
			{ "Melee", AbilityBuilder.GetShortRangeMeleeElementalAbility1 },
			{ "Blink", AbilityBuilder.GetBlinkMiscAbility },
		};

		protected Animation _availableAnimation; // Animation shown when a player can grab it.
		protected string _abilityName;

		public AbilityObject(Vector2 position, Vector2 size, Level level, Animation animation, Func<PhysicsObject, PlayerAbilityInfo> getAbility)
			: base(position, size, level, "AbilityObject")
		{
			Group = Group.Passable;
			_availableAnimation = animation;
			_animator.PlayAnimation(animation);
			//_getAbility = getAbility;
			_abilityName = "Blink";
		}
		protected override void HitObject(MainGuiObject mgo, Vector2 bounds)
		{
			PhysicsObject pmgo = mgo as PhysicsObject;
			if (pmgo != null)
			{
				PlayerAbilityInfo pai = _magicNameMap[_abilityName](pmgo);
				pmgo.AbilityManager.AddKnownAbility(pai.KnownAbility, pai);
				Level.RemoveGuiObject(this);
			}
		}

		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { base.PreUpdate(gameTime); }
		protected override bool ShowHitBox() { return false; }
		public override IEnumerable<Tuple<Vector2, MainGuiObject>> GetAffectedObjects()
		{
			return GetHitObjects(Level.Players.Values, Bounds); // ID won't be the same as this is not a player.
		}
		public override void ExtraSizeManipulation(ref Vector2 newSize)
		{
			_availableAnimation.Scale = new Vector2(Size.X / _availableAnimation.FrameWidth, Size.Y / _availableAnimation.FrameHeight);
			//= new Animation(MainGame.Content.Load<Texture2D>("Test/Turret"), 1, false, 300, 500, (Size.X / 300.0f));
			base.ExtraSizeManipulation(ref newSize);
		}



		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "MagicType";
			//if (bType == ButtonType.SpecialToggle2)
			//	return "";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _abilityName;
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				bool assignedValue = false;
				bool foundMatch = false;
				string newValue = "";
				foreach (string abilityname in moveRight ? _magicNameMap.Keys : _magicNameMap.Keys.Reverse())
				{
					if (foundMatch)
					{
						newValue = abilityname;
						assignedValue = true;
						break;
					}
					else if (abilityname == _abilityName)
					{
						foundMatch = true;
					}
				}
				if (!assignedValue)
					newValue = moveRight ? _magicNameMap.Keys.First() : _magicNameMap.Keys.Last();

				_abilityName = newValue;
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _magicNameMap.Keys.TakeWhile(s => s != _abilityName).Count();
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_abilityName = _magicNameMap.Skip(value).First().Key;
			base.SetSpecialValue(bType, value);
		}

		#endregion

	}
}
