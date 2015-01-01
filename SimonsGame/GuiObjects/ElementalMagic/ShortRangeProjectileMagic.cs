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
	public class ShortRangeProjectileMagic : PhysicsObject
	{
		private Texture2D _leaf;
		private float radians = 0;
		private Player _player;
		private ModifierBase _damageDoneOnCollide;


		public ShortRangeProjectileMagic(Vector2 position, Vector2 hitbox, Group group, Level level, Vector2 speed, Player player)
			: base(position, hitbox, group, level, "ShortRangeProjectileMagic")
		{
			MaxSpeedBase = speed;
			_leaf = level.Content.Load<Texture2D>("Test/leaf");
			_damageDoneOnCollide = new TickModifier(1, ModifyType.Add);
			_damageDoneOnCollide.SetHealthTotal(-2);
			_player = player;
			Parent = player;
		}

		public override float GetXMovement()
		{
			return MaxSpeed.X;
		}

		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);
			Dictionary<Group, List<MainGuiObject>> guiObjects = Level.GetAllGuiObjects().Where(kv => kv.Key != Group.Passable).ToDictionary(kv => kv.Key, kv => kv.Value);
			IEnumerable<Tuple<DoubleVector2, MainGuiObject>> hitPlatforms = GetHitObjects(guiObjects, this.HitBoxBounds, (mgo) => mgo.Id == _player.Id);
			if (hitPlatforms.Any()) // Probably apply any effects it would have.
			{
				MainGuiObject mgo = hitPlatforms.First().Item2;
				mgo.HitByObject(this, _damageDoneOnCollide);
				Level.RemoveGuiObject(this);
			}
		}

		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			//spriteBatch.Begin();
			Rectangle destinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

			float scale = Size.Y / _leaf.Height;
			spriteBatch.Draw(_leaf, Position + (Size / 2), null, Color.White, radians, new Vector2(_leaf.Width / 2, _leaf.Height / 2), scale, SpriteEffects.None, 0);

			radians += (float)(Math.PI / 30f);
			//spriteBatch.End();
		}
		public override void SetMovement(GameTime gameTime) { }
		protected override bool ShowHitBox()
		{
			return false;
		}
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
	}
}
