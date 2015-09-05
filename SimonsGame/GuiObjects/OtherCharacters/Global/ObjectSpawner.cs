using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.GuiObjects
{
	public class ObjectSpawner : PhysicsObject
	{
		private Animation _emptyAnimation;
		private bool _isVisible = true;

		private int _spawnAmountPerCycle;

		private int _frequencyInTicks;
		private int _currentTicks;
		private GuiObjectStore _characterToCreate;
		private bool _facingRight = true;
		private byte _behaviorChannel;
		public byte BehaviorChannel { get { return _behaviorChannel; } }

		public ObjectSpawner(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Object Spawner")
		{
			_team = Team.None;
			_objectType = GuiObjectType.Structure;
			_currentTicks = 0;
			MainGuiObject mgo = Level.GetNewItem(GuiObjectClass.MinionNormal);
			_characterToCreate = mgo.GetGuiObjectStore();
			_spawnAmountPerCycle = 2;
			_frequencyInTicks = 120;
			IsMovable = false;
			_behaviorChannel = 0;
			if (Group == Group.Passable && MainGame.GameState == MainGame.MainGameState.Game)
				HideSpawner();

			if (_isVisible)
			{
				Texture2D animationTexture = MainGame.ContentManager.Load<Texture2D>("Test/ObjectSpawnerEmpty");
				_emptyAnimation = new Animation(animationTexture, .1f, true, animationTexture.Width / 8, animationTexture.Height, new Vector2(hitbox.X / (animationTexture.Width / 8), hitbox.Y / animationTexture.Height));
				_animator.PlayAnimation(_emptyAnimation);
			}
		}
		public void HideSpawner()
		{
			if (_isVisible)
			{
				_isVisible = false;
				_animator.HideAnimations();
			}
		}
		public void ShowSpawner()
		{
			if (!_isVisible)
			{
				_isVisible = true;
				Texture2D animationTexture = MainGame.ContentManager.Load<Texture2D>("Test/ObjectSpawnerEmpty");
				_emptyAnimation = new Animation(animationTexture, .1f, true, animationTexture.Width / 8, animationTexture.Height, new Vector2(Size.X / (animationTexture.Width / 8), Size.Y / animationTexture.Height));
				_animator.PlayAnimation(_emptyAnimation);
			}
		}
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void ExtraSizeManipulation(ref Vector2 newSize)
		{
			if (_isVisible)
				_emptyAnimation.Scale = new Vector2(Size.X / _emptyAnimation.FrameWidth, Size.Y / _emptyAnimation.FrameHeight);
			//= new Animation(MainGame.Content.Load<Texture2D>("Test/Turret"), 1, false, 300, 500, (Size.X / 300.0f));
			base.ExtraSizeManipulation(ref newSize);
		}
		public override void PreUpdate(GameTime gameTime)
		{
			_currentTicks++;

			// if we have reached our frequency for creating objects, create some!
			if (_currentTicks >= _frequencyInTicks)
			{
				int madeSoFar = 1 + ((_currentTicks - _frequencyInTicks) / 10);
				if (_currentTicks % 10 == 0)
				{
					bool didSwitchDirections = DidSwitchDirection();
					MainGuiObject mgo = _characterToCreate.GetObject(Level);
					mgo.Team = Team;
					mgo.ZoneIds = ZoneIds;

					mgo.Position = new Vector2(didSwitchDirections ? Position.X - mgo.Size.X : Position.X + Size.X, BasePosition.Y - mgo.Size.Y);
					if (didSwitchDirections) mgo.SwitchDirections();

					Level.AddGuiObject(mgo);
				}
				if (madeSoFar >= _spawnAmountPerCycle)
					_currentTicks = 0;
			}
		}
		public override void PostUpdate(GameTime gameTime) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			if (_healthTotal == 1)
				return;
			else
				_abilityManager.AddAbility(mb);
		}


		public override void SwitchDirections()
		{
			_facingRight = !_facingRight;
		}
		public override string GetDirectionalText()
		{
			return _facingRight ? "Right" : "Left";
		}
		public override bool DidSwitchDirection()
		{
			return !_facingRight;
		}

		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			return _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		}

		#region Map Editor
		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Character Type";
			else if (bType == ButtonType.SpecialToggle2)
				return "Frequency";
			else if (bType == ButtonType.SpecialToggle3)
				return "Spawn Amount";
			else if (bType == ButtonType.SpecialToggle4)
				return "Behavior Channel";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _characterToCreate.Class.ToString();
			else if (bType == ButtonType.SpecialToggle2)
				return string.Format("{0:0.0} Seconds", _frequencyInTicks / 60.0);
			else if (bType == ButtonType.SpecialToggle3)
				return _spawnAmountPerCycle.ToString();
			else if (bType == ButtonType.SpecialToggle4)
				return _behaviorChannel.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				MainGuiObject mgo = null;
				GuiObjectClass nextObject = _characterToCreate.Class;
				do
				{
					nextObject = moveRight
						? (GuiObjectClass)MiscExtensions.GetNextEnum<GuiObjectClass>(nextObject)
						: (GuiObjectClass)MiscExtensions.GetPreviousEnum<GuiObjectClass>(nextObject);
					mgo = Level.GetNewItem(nextObject);
				}
				while (mgo == null || mgo.ObjectType != GuiObjectType.Character);
				_characterToCreate = mgo.GetGuiObjectStore();
			}
			else if (bType == ButtonType.SpecialToggle2)
				_frequencyInTicks = MathHelper.Clamp(_frequencyInTicks + (moveRight ? 1 : -1) * 30, 30, 3600);
			else if (bType == ButtonType.SpecialToggle3)
				_spawnAmountPerCycle = MathHelper.Clamp(_spawnAmountPerCycle + (moveRight ? 1 : -1), 1, 6);
			else if (bType == ButtonType.SpecialToggle4)
				_behaviorChannel = (byte)((_behaviorChannel + (moveRight ? 1 : 9)) % 10);
			base.ModifySpecialText(bType, moveRight);
		}

		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return (int)_characterToCreate.Class;
			else if (bType == ButtonType.SpecialToggle2)
				return _frequencyInTicks;
			else if (bType == ButtonType.SpecialToggle3)
				return _spawnAmountPerCycle;
			else if (bType == ButtonType.SpecialToggle4)
				return _behaviorChannel;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				MainGuiObject mgo = Level.GetNewItem((GuiObjectClass)value);
				_characterToCreate = mgo.GetGuiObjectStore();
			}
			else if (bType == ButtonType.SpecialToggle2)
				_frequencyInTicks = value;
			else if (bType == ButtonType.SpecialToggle3)
				_spawnAmountPerCycle = value;
			else if (bType == ButtonType.SpecialToggle4)
				_behaviorChannel = (byte)value;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}

}
