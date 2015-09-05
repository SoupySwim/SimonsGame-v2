using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects.Zones;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects.BaseClasses
{
	public class GenericBoss : CreepBase
	{

		protected enum BossBehavior
		{
			WaitingForPlayer = 0,
			Attacking, // Will use subBehaviors
		}


		protected MainGuiObject _targetedObject;
		protected BossBehavior _bossBehavior;
		public GenericBoss(Vector2 position, Vector2 hitbox, Group group, Level level, string name)
			: base(position, hitbox, group, level, name)
		{
			Team = Team.Neutral;
			_bossBehavior = BossBehavior.WaitingForPlayer;
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X, AverageSpeed.Y);
			_healthTotal = 1800;
			_healthCurrent = _healthTotal;
		}

		public override float GetXMovement()
		{
			return 0;
		}

		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}

		public override void PreUpdate(GameTime gameTime)
		{
			base.PreUpdate(gameTime);
			// If we are waiting for a player, then we will search our zone for intruders!
			if (_bossBehavior == BossBehavior.WaitingForPlayer)
			{
				GenericZone zone;
				//= Level.GetAllZones().FirstOrDefault(z => ZoneIds.Contains(z.Id));
				if (ZoneIds.Any() && Level.GetAllZones().TryGetValue(ZoneIds.FirstOrDefault(), out zone))
				{
					foreach (Player player in Level.Players.Values)
					{
						if (MainGuiObject.GetIntersectionDepth(player.Bounds, zone.Bounds) != Vector2.Zero)
						{
							_bossBehavior = BossBehavior.Attacking;
							_targetedObject = player;
						}
					}
				}
			}
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			if (mgo != null)
				_targetedObject = mgo;

			_abilityManager.AddAbility(mb);
		}

		public override Vector2 GetAim()
		{
			if (_targetedObject != null)
			{
				Vector2 distance = _targetedObject.Center - Center;
				var normal = (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2));
				return distance / normal;
			}
			return Vector2.Zero;
		}

	}
}
