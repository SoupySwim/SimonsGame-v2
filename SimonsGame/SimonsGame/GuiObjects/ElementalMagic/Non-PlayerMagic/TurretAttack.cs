﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects.ElementalMagic
{
	public class TurretAttack : PhysicsObject
	{
		private float radians = 0;
		private Texture2D _fireball;
		private ModifierBase _damageDealt;
		private StandardTurret _turret;

		public TurretAttack(Vector2 position, Vector2 hitbox, Group group, Level level, Vector2 speed, StandardTurret turret)
			: base(position, hitbox, group, level, "TurretAttack")
		{
			_fireball = MainGame.ContentManager.Load<Texture2D>("Test/Fireball");
			Parent = turret;
			_turret = turret;
			MaxSpeedBase = speed;
			_fireball = MainGame.ContentManager.Load<Texture2D>("Test/Fireball");
			_damageDealt = new TickModifier(1, ModifyType.Add, turret);
			_damageDealt.SetHealthTotal(-1);
			Team = _turret.Team;
			//_objectType = GuiObjectType.Attack;
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
		public override float GetXMovement()
		{
			return MaxSpeed.X;
		}
		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		protected override bool ShowHitBox()
		{
			return false;
		}
		public void Detonate()
		{
			Level.RemoveGuiObject(this);
		}
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);
			// If it hit something it can't pass through, detonate it!
			//if (PrimaryOverlapObjects.Any())
			//{
			//	MainGuiObject hitTarget = PrimaryOverlapObjects.Values.Where(mgo => mgo.Team != _turret.Team && mgo.Team != GuiObjects.Team.Neutral && mgo.Team != GuiObjects.Team.None).FirstOrDefault();
			//	if (hitTarget != null)
			//	{
			//		hitTarget.HitByObject(this, _damageDealt);
			//		Detonate();
			//	}
			//}
			MainGuiObject hitMgo = null;
			if (PrimaryOverlapObjects.Any())
				hitMgo = PrimaryOverlapObjects.Values.FirstOrDefault();
			else
			{
				Dictionary<Group, List<MainGuiObject>> guiObjects = Level.GetAllGuiObjects().Where(kv => kv.Key != Group.Passable).ToDictionary(kv => kv.Key, kv => kv.Value);
				IEnumerable<Tuple<DoubleVector2, MainGuiObject>> hitPlatforms = GetHitObjects(guiObjects, this.HitBoxBounds, (mgo) => mgo.Id == _turret.Id);
				hitPlatforms = hitPlatforms.Where(hp => hp.Item2.Team != Team);
				hitMgo = hitPlatforms.Any() ? hitPlatforms.First().Item2 : null;
			}
			if (hitMgo != null) // Probably apply any effects it would have.
			{
				hitMgo.HitByObject(this, _damageDealt);
				Level.RemoveGuiObject(this);
			}
		}
		protected override Dictionary<Group, List<MainGuiObject>> GetAllVerticalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			var allObjects = guiObjects;//.Where(g => g.Key == Group.ImpassableIncludingMagic || g.Key == Group.Impassable).ToDictionary(o => o.Key, o => o.Value);
			foreach (KeyValuePair<Group, List<MainGuiObject>> kv in allObjects.ToList())
				allObjects[kv.Key] = kv.Value.Where(mgo => mgo.ObjectType != GuiObjectType.Structure || mgo.Team != Parent.Team).ToList();

			foreach (var kv in _turret.Level.Players)
			{
				Group playerGroup = kv.Value.Group;
				if (!allObjects.ContainsKey(playerGroup))
					allObjects.Add(playerGroup, new List<MainGuiObject>());
				allObjects[playerGroup].Add(kv.Value);
			}
			return allObjects;
		}
		protected override Dictionary<Group, List<MainGuiObject>> GetAllHorizontalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			var allObjects = guiObjects;//.Where(g => g.Key == Group.ImpassableIncludingMagic || g.Key == Group.Impassable).ToDictionary(o => o.Key, o => o.Value);
			foreach (KeyValuePair<Group, List<MainGuiObject>> kv in allObjects.ToList())
				allObjects[kv.Key] = kv.Value.Where(mgo => mgo.ObjectType != GuiObjectType.Structure || mgo.Team != Parent.Team).ToList();

			foreach (var kv in _turret.Level.Players)
			{
				Group playerGroup = kv.Value.Group;
				if (!allObjects.ContainsKey(playerGroup))
					allObjects.Add(playerGroup, new List<MainGuiObject>());
				allObjects[playerGroup].Add(kv.Value);
			}
			return allObjects;
		}
	}
}
