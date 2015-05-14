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
	public enum PowerUpType
	{
		HealthPack,
		SuperJump,
		SuperSpeed
	}
	public class PowerUp : AffectedSpace
	{
		private int _respawnTimeTotal = 180; // 14400; // 4 minutes
		private int _currentSpawnTime = 0;
		public PowerUpType PowerUpType { get; set; }
		protected Animation _availableAnimation; // Animation shown when a player can grab it.

		public PowerUp(Vector2 position, Vector2 size, Level level, Animation animation, ModifierBase modifier, PowerUpType puType)
			: base(position, size, level, "PowerUp")
		{
			_collisionModifier = modifier;
			Group = Group.Passable;
			_availableAnimation = animation;
			_animator.PlayAnimation(animation);
			PowerUpType = puType;
		}
		protected override void HitObject(MainGuiObject mgo, Vector2 bounds)
		{
			_animator.HideAnimations();
			_currentSpawnTime = 1;
			mgo.HitByObject(this, _collisionModifier.Clone());
		}
		public override void PostUpdate(GameTime gameTime)
		{
			if (_currentSpawnTime == 0)
			{
				base.PostUpdate(gameTime);
				return;
			}
			_currentSpawnTime++;
			if (_currentSpawnTime > _respawnTimeTotal)
			{
				_animator.PlayAnimation(_availableAnimation);
				_currentSpawnTime = 0;
			}
		}
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { base.PreUpdate(gameTime); }
		protected override bool ShowHitBox() { return false; }
		public override IEnumerable<Tuple<Vector2, MainGuiObject>> GetAffectedObjects()
		{
			return GetHitObjects(Level.Players.Values, Bounds); // ID won't be the same as this is not a player.
		}
		public override void ExtraSizeManipulation(Vector2 newSize)
		{
			_availableAnimation.Scale = new Vector2(Size.X / _availableAnimation.FrameWidth, Size.Y / _availableAnimation.FrameHeight);
			//= new Animation(MainGame.Content.Load<Texture2D>("Test/Turret"), 1, false, 300, 500, (Size.X / 300.0f));
			base.ExtraSizeManipulation(newSize);
		}


		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Active Time";
			if (bType == ButtonType.SpecialToggle2)
				return "Respawn Time";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return string.Format("{0:0.0}", (_collisionModifier.GetTickCount() / 60.0f));
			if (bType == ButtonType.SpecialToggle2)
				return _respawnTimeTotal == -1 ? "Never" : (_respawnTimeTotal / 60).ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				_collisionModifier.SetTickCount((long)MathHelper.Clamp(_collisionModifier.GetTickCount() + (moveRight ? 30 : -30), 30, 900));
			if (bType == ButtonType.SpecialToggle2)
			{
				if (_respawnTimeTotal < 0)
					_respawnTimeTotal = moveRight ? 300 : 3600;
				else if (!moveRight && _respawnTimeTotal == 300)
					_respawnTimeTotal = -1;
				else if (moveRight && _respawnTimeTotal == 3600)
					_respawnTimeTotal = -1;
				else
					_respawnTimeTotal = MathHelper.Clamp(_respawnTimeTotal + (moveRight ? 300 : -300), 300, 3600);
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return (int)_collisionModifier.GetTickCount();
			if (bType == ButtonType.SpecialToggle2)
				return _respawnTimeTotal;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_collisionModifier.SetTickCount(value);
			if (bType == ButtonType.SpecialToggle2)
				_respawnTimeTotal = value;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}
