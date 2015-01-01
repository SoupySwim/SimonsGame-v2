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
	// First draft of Long Range Magic.
	// First draft will not include type of magic as that comes at a later sprint.
	public class LongRangeMagic : PhysicsObject
	{
		// the temp-est of textures...
		private Texture2D _fireball;
		private float radians = 0;
		private Player _player;

		private ModifierBase _damageDoneOnDetonate;

		public LongRangeMagic(Vector2 position, Vector2 hitbox, Group group, Level level, Vector2 speed, Player player)
			: base(position, hitbox, group, level, "LongRangeMagic")
		{
			MaxSpeedBase = speed;
			_fireball = level.Content.Load<Texture2D>("Test/Fireball");
			_damageDoneOnDetonate = new TickModifier(1, ModifyType.Add);
			_damageDoneOnDetonate.SetHealthTotal(-4);
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

		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			//spriteBatch.Begin();

			float scale = Size.Y / _fireball.Height;
			spriteBatch.Draw(_fireball, Position + (Size / 2), null, Color.White, radians, new Vector2(_fireball.Width / 2, _fireball.Height / 2), scale, SpriteEffects.None, 0);

			radians += (float)(Math.PI / 22.5f);
			//spriteBatch.End();
		}
		public override void SetMovement(GameTime gameTime) { }
		protected override bool ShowHitBox()
		{
			return false;
		}
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);
			// If it hit something it can't pass through, detonate it!
			if (PrimaryOverlapObjects.Any())
				Detonate();
		}
		public void Detonate()
		{
			Dictionary<Group, List<MainGuiObject>> guiObjects = Level.GetAllGuiObjects().Where(kv => kv.Key != Group.Passable).ToDictionary(kv => kv.Key, kv => kv.Value);
			Vector4 bounds = new Vector4(this.Position.X - 5, this.Position.Y - 5, this.Size.X + 10, this.Size.Y + 10);
			IEnumerable<Tuple<DoubleVector2, MainGuiObject>> hitPlatforms = GetHitObjects(guiObjects, this.HitBoxBounds, (mgo) => mgo.Id == _player.Id);
			if (hitPlatforms.Any()) // Probably apply any effects it would have.
			{
				foreach (MainGuiObject mgo in hitPlatforms.Select(hp => hp.Item2))
				{
					//MainGuiObject mgo = hitPlatforms.First().Item2;
					mgo.HitByObject(this, _damageDoneOnDetonate);
				}
				Level.RemoveGuiObject(this);
			}
		}
		protected override Dictionary<Group, List<MainGuiObject>> GetAllVerticalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			return guiObjects.Where(g => g.Key == Group.ImpassableIncludingMagic).ToDictionary(o => o.Key, o => o.Value);
		}
		protected override Dictionary<Group, List<MainGuiObject>> GetAllHorizontalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			return guiObjects.Where(g => g.Key == Group.ImpassableIncludingMagic).ToDictionary(o => o.Key, o => o.Value);
		}
	}
}
