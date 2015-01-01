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
	public class ShortRangeMeleeMagic : PhysicsObject
	{
		private bool _isFlippeed;
		private Texture2D _splash;
		private ModifierBase _damageDoneOnCollide;
		private Player _player;
		public ShortRangeMeleeMagic(Vector2 position, Player player, Vector2 hitbox, Group group, Level level, bool isFlipped)
			: base(position, hitbox, group, level, "ShortRangeMeleeMagic")
		{
			_splash = level.Content.Load<Texture2D>("Test/splash");
			_isFlippeed = isFlipped;
			_player = player;
			Parent = player;
			_damageDoneOnCollide = new TickModifier(1, ModifyType.Add);
			_damageDoneOnCollide.SetHealthTotal(-2);
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
			Dictionary<Group, List<MainGuiObject>> guiObjects = Level.GetAllGuiObjects().Where(kv => kv.Key != Group.Passable).ToDictionary(kv => kv.Key, kv => kv.Value);
			IEnumerable<Tuple<DoubleVector2, MainGuiObject>> hitPlatforms = GetHitObjects(guiObjects, this.HitBoxBounds, (mgo) => mgo.Id == _player.Id);

			foreach (MainGuiObject mgo in hitPlatforms.Select(tup => tup.Item2))
			{
				mgo.HitByObject(this, _damageDoneOnCollide);
				Level.RemoveGuiObject(this); // later... this will not happen...
			}
		}

		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
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
