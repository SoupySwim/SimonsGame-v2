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
	public class ShortRangeProjectileMagic : PlayerMagicObject
	{
		private Texture2D _leaf;
		private float radians = 0;
		public ModifierBase DamageDoneOnCollide { get { return _damageDoneOnCollide; } }
		private ModifierBase _damageDoneOnCollide;


		public ShortRangeProjectileMagic(Vector2 position, Vector2 hitbox, Group group, Level level, Vector2 speed, Player player, Element element, float damage)
			: base(position, hitbox, group, level, player, "ShortRangeProjectileMagic", null)
		{
			MaxSpeedBase = speed;
			_leaf = MainGame.ContentManager.Load<Texture2D>("Test/leaf");
			_damageDoneOnCollide = new TickModifier(1, ModifyType.Add, _character, element);
			_damageDoneOnCollide.SetHealthTotal(damage);
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
			MainGuiObject hitMgo = null;
			var hitObjects = PrimaryOverlapObjects.SelectMany(mgos => mgos.Value);
			if (hitObjects.Any())
				hitMgo = hitObjects.FirstOrDefault();
			else
			{
				IEnumerable<MainGuiObject> guiObjects = Level.GetPossiblyHitEnvironmentObjects(this);
				IEnumerable<Tuple<Vector2, MainGuiObject>> hitPlatforms = GetHitObjects(guiObjects, this.HitBoxBounds).Where(tup => tup.Item2.Id != _character.Id && tup.Item2.Team != Team);
				hitPlatforms = hitPlatforms.Where(hp => hp.Item2.Team != Team);
				hitMgo = hitPlatforms.Any() ? hitPlatforms.First().Item2 : null;
			}
			if (hitMgo != null) // Probably apply any effects it would have.
			{
				hitMgo.HitByObject(this, _damageDoneOnCollide);
				Level.RemoveGuiObject(this);
			}
		}

		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			//spriteBatch.Begin();

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
		protected override IEnumerable<MainGuiObject> GetAllVerticalPassableGroups(IEnumerable<MainGuiObject> guiObjects)
		{
			return guiObjects.ToList().Where(mgo => mgo.Team != Parent.Team);
		}
		protected override IEnumerable<MainGuiObject> GetAllHorizontalPassableGroups(IEnumerable<MainGuiObject> guiObjects)
		{
			return guiObjects.ToList().Where(mgo => mgo.Team != Parent.Team);
		}
	}
}
