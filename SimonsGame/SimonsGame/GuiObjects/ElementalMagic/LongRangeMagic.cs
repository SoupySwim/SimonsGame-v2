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
	public class LongRangeMagic : PlayerMagicObject
	{
		// the temp-est of textures...
		private Texture2D _fireball;
		private float radians = 0;
		private bool _hasBeenDetonated = false;

		public ModifierBase DamageDoneOnDetonate { get { return _damageDoneOnDetonate; } }
		private ModifierBase _damageDoneOnDetonate;

		public LongRangeMagic(Vector2 position, Vector2 hitbox, Group group, Level level, Vector2 speed, float damage, Element element, Player player)
			: base(position, hitbox, group, level, player, "LongRangeMagic", null)
		{
			MaxSpeedBase = speed;
			_fireball = MainGame.ContentManager.Load<Texture2D>("Test/Fireball");
			_damageDoneOnDetonate = new TickModifier(1, ModifyType.Add, _character, element);
			_damageDoneOnDetonate.SetHealthTotal(damage);
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
			if (PrimaryOverlapObjects.SelectMany(mgos => mgos.Value).Any())
				Detonate();
		}
		public void Detonate()
		{
			if (!_hasBeenDetonated)
			{
				IEnumerable<MainGuiObject> guiObjects = Level.GetAllGuiObjects().Where(kv => kv.Group != Group.Passable);
				Vector4 bounds = new Vector4(this.Position.X - 5, this.Position.Y - 5, this.Size.X + 10, this.Size.Y + 10);
				IEnumerable<MainGuiObject> hitPlatforms = GetHitObjects(guiObjects, this.HitBoxBounds).Select(tup => tup.Item2).Where(mgo => mgo.Id != _character.Id).Concat(PrimaryOverlapObjects.SelectMany(mgos => mgos.Value));
				if (hitPlatforms.Any()) // Probably apply any effects it would have.
				{
					foreach (MainGuiObject mgo in hitPlatforms)
					{
						//MainGuiObject mgo = hitPlatforms.First().Item2;
						if (mgo.Team != Team)
							mgo.HitByObject(this, _damageDoneOnDetonate);
					}
				}
				Level.RemoveGuiObject(this);
				_hasBeenDetonated = true;
			}
		}
		protected override IEnumerable<MainGuiObject> GetAllVerticalPassableGroups(IEnumerable<MainGuiObject> guiObjects)
		{
			return guiObjects.Where(g => g.Group == Group.ImpassableIncludingMagic);
		}
		protected override IEnumerable<MainGuiObject> GetAllHorizontalPassableGroups(IEnumerable<MainGuiObject> guiObjects)
		{
			return guiObjects.Where(g => g.Group == Group.ImpassableIncludingMagic);
		}
	}
}
