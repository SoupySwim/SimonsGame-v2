using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers.Abilities
{
	public class ProjectileElementalMagic : PlayerMagicObject
	{
		protected ProjectileElementalMagicAbility _abilityParent;
		public ProjectileElementalMagic(Vector2 position, Vector2 hitbox, Group group, Level level, Vector2 speed, PhysicsObject player, Element element, float damage, string name, Animation animation, ProjectileElementalMagicAbility abilityParent)
			: base(position, hitbox, group, level, player, name, animation)
		{
			_abilityParent = abilityParent;
			MaxSpeedBase = speed;
			_modifier = new TickModifier(1, ModifyType.Add, _character, _abilityParent.Element);
			_modifier.SetHealthTotal(_abilityParent.Damage);
			IsMovable = !HasAbility(AbilityAttributes.CanPush);
		}
		public bool HasAbility(AbilityAttributes abilityAttributes)
		{
			return _abilityParent.AbilityAttributes.HasFlag(abilityAttributes);
		}
		public override float GetXMovement()
		{
			return MaxSpeed.X;
		}
		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}
		public override void ExtraExpireFunction(MainGuiObject hitObject = null)
		{
			bool hitMultiple = HasAbility(AbilityAttributes.Explosion) || HasAbility(AbilityAttributes.Pierce);
			// If only one object was going to be hit, then hit it.
			if (hitObject != null && !hitMultiple)
				hitObject.HitByObject(this, _modifier);
			else
			{
				IEnumerable<MainGuiObject> guiObjects = Level.GetAllGuiObjects().Where(kv => kv.Group != Group.Passable);
				Vector4 bounds = new Vector4(this.Position.X - 5, this.Position.Y - 5, this.Size.X + 10, this.Size.Y + 10);
				IEnumerable<MainGuiObject> hitPlatforms = GetHitObjects(guiObjects, this.HitBoxBounds).Where(tup => tup.Item2.Id != _character.Id).Select(tup => tup.Item2).Concat(PrimaryOverlapObjects.SelectMany(mgos => mgos.Value));
				if (hitPlatforms.Any()) // Probably apply any effects it would have.
				{
					foreach (MainGuiObject mgo in hitMultiple ? hitPlatforms : hitPlatforms.Take(1))
					{
						//MainGuiObject mgo = hitPlatforms.First().Item2;
						if (mgo.Team != Team)
							mgo.HitByObject(this, _modifier);
					}
				}
			}
			PlayerAbilityInfo pai = _character.AbilityManager.GetAbilityInfo(_abilityParent.PlayerInfoId);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);
			// If it hit something it can't pass through, detonate it!
			if (HasAbility(AbilityAttributes.CanPush))
				return; // Keep pushing until the end!
			var overlappedObjects = PrimaryOverlapObjects.SelectMany(mgos => mgos.Value);
			if (overlappedObjects.Any())
				Expire(overlappedObjects.FirstOrDefault());
			else
			{
				List<MainGuiObject> newGuiObjects = new List<MainGuiObject>();
				if (!HasAbility(AbilityAttributes.PassCharacters)) // If the magic cannot pass through characters, then stop it on a hit character.
					newGuiObjects.AddRange(Level.GetAllUnPassableMovableObjects());
				if (!HasAbility(AbilityAttributes.PassWall)) // If the magic cannot pass through walls, then stop it on a hit wall.
					newGuiObjects.AddRange(Level.GetAllUnPassableEnvironmentObjects());

				// If the magic can go through characters and environment objects, then the following code will do nothing (presumably click to detonate will be enforced).
				IEnumerable<Tuple<Vector2, MainGuiObject>> hitObjects = GetHitObjects(newGuiObjects, this.HitBoxBounds).Where(tup => tup.Item2.Id != _character.Id && tup.Item2.Team != Team);
				hitObjects = hitObjects.Where(hp => hp.Item2.Team != Team);
				var hitMgo = hitObjects.Any() ? hitObjects.First().Item2 : null;
				if (hitMgo != null)
					Expire(hitMgo);
			}
		}
		public override bool CanPushObjects()
		{
			return HasAbility(AbilityAttributes.CanPush);
		}
	}
}
