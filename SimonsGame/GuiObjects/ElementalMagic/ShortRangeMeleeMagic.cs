using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects.ElementalMagic
{
	// First draft of Short Range Magic.
	// First draft will not include type of magic as that comes at a later sprint.
	public class ShortRangeMeleeMagic : PlayerMagicObject
	{
		private bool _isFlippeed;
		private Texture2D _splash;
		public ModifierBase DamageDoneOnCollide { get { return _damageDoneOnCollide; } }
		private ModifierBase _damageDoneOnCollide;
		public ShortRangeMeleeMagic(Vector2 position, PhysicsObject character, Vector2 hitbox, Group group, Level level, bool isFlipped, Tuple<Element, float> element, float damage)
			: base(position, hitbox, group, level, character, "ShortRangeMeleeMagic", null)
		{
			_splash = MainGame.ContentManager.Load<Texture2D>("Test/splash");
			_isFlippeed = isFlipped;
			Parent = character;
			_damageDoneOnCollide = new TickModifier(1, ModifyType.Add, _character, element);
			_damageDoneOnCollide.SetHealthTotal(damage);

			float heightDif = character.Size.Y - Size.Y - 10; // 10 is an arbitrarily good enough number.
			BufferVector.Y = -heightDif / 2.0f;
			BufferVector.Z = heightDif;
		}
		public override float GetXMovement()
		{
			return 0;
		}
		public override float GetYMovement()
		{
			return 0;
		}
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);
			IEnumerable<Tuple<Vector2, MainGuiObject>> hitPlatforms = GetHitObjects(Level.GetAllMovableCharacters(Bounds).Concat(Level.GetAllUnPassableEnvironmentObjects(Bounds)), this.HitBoxBounds).Where(tup => tup.Item2.Id != _character.Id);

			foreach (MainGuiObject mgo in hitPlatforms.Select(tup => tup.Item2).Where(mgo => mgo.Team != Team).ToList())
			{
				mgo.HitByObject(this, _damageDoneOnCollide);
				Level.RemoveGuiObject(this); // later... this will not happen...
			}
		}

		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer)
		{
			Rectangle destinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
			spriteBatch.Draw(_splash, destinationRect, null, Color.White, 0, Vector2.Zero, _isFlippeed ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
		}
		public override void SetMovement(GameTime gameTime) { }
		protected override bool ShowHitBox()
		{
			return false;
		}
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
	}
}
