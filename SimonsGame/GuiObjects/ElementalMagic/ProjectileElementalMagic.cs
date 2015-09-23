using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Modifiers.Abilities
{
	public class ProjectileElementalMagic : PlayerMagicObject
	{
		protected ProjectileElementalMagicAbility _abilityParent;
		public ProjectileElementalMagic(Vector2 position, Vector2 hitbox, Group group, Level level, Vector2 speed, PhysicsObject player, Tuple<Element, float> element, float damage, string name, Animation animation, ProjectileElementalMagicAbility abilityParent)
			: base(position, hitbox, group, level, player, name, animation)
		{
			_abilityParent = abilityParent;
			MaxSpeedBase = speed;
			ModifierBase modifierBase = new TickModifier(1, ModifyType.Add, _character, _abilityParent.Element);
			modifierBase.SetHealthTotal(_abilityParent.Damage);
			_modifierList.Add(modifierBase);

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
			{
				foreach (ModifierBase mod in _modifierList)
					hitObject.HitByObject(_character, mod);
				hitObject.HitByObject(_character, this.GetKnockbackAbility(hitObject, _knockbackBase));
			}
			else
			{
				IEnumerable<MainGuiObject> guiObjects = Level.GetPossiblyHitEnvironmentObjects(HitBoxBounds).Concat(Level.GetAllMovableCharacters(HitBoxBounds)).Where(tup => tup.Id != _character.Id && tup.Group != Group.Passable); //Level.GetAllGuiObjects().Where(kv => kv.Group != Group.Passable);
				IEnumerable<MainGuiObject> hitPlatforms = GetHitObjects(guiObjects, this.HitBoxBounds).Select(tup => tup.Item2).Concat(PrimaryOverlapObjects.SelectMany(mgos => mgos.Value));
				if (hitPlatforms.Any()) // Probably apply any effects it would have.
				{
					foreach (MainGuiObject mgo in hitMultiple ? hitPlatforms : hitPlatforms.Take(1))
					{
						//MainGuiObject mgo = hitPlatforms.First().Item2;
						if (mgo.Team != Team)
						{
							foreach (ModifierBase mod in _modifierList)
								mgo.HitByObject(_character, mod);
							mgo.HitByObject(_character, this.GetKnockbackAbility(mgo, _knockbackBase));
						}
					}
				}
			}
			PlayerAbilityInfo pai = _character.AbilityManager.GetAbilityInfo(_abilityParent.PlayerInfoId);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PostUpdate(GameTime gameTime)
		{
			//base.PostUpdate(gameTime);
			//// If it hit something it can't pass through, detonate it!
			if (HasAbility(AbilityAttributes.CanPush))
			{
				base.PostUpdate(gameTime);
				return; // Keep pushing until the end!
			}
			//var overlappedObjects = PrimaryOverlapObjects.SelectMany(mgos => mgos.Value);
			//if (overlappedObjects.Any())
			//	Expire(overlappedObjects.FirstOrDefault());
			//else
			//{
			List<MainGuiObject> newGuiObjects = new List<MainGuiObject>();
			if (!HasAbility(AbilityAttributes.PassCharacters)) // If the magic cannot pass through characters, then stop it on a hit character.
				newGuiObjects.AddRange(Level.GetAllMovableCharacters(Bounds).Where(mgo => mgo.Team != Team && mgo.Id != _character.Id));
			if (!HasAbility(AbilityAttributes.PassWall)) // If the magic cannot pass through walls, then stop it on a hit wall.
				newGuiObjects.AddRange(Level.GetPossiblyHitEnvironmentObjects(Bounds).Where(mgo => (mgo.Team != Team || Team <= Team.Neutral) && mgo.Group == Group.ImpassableIncludingMagic));

			// If the magic can go through characters and environment objects, then the following code will do nothing (presumably click to detonate will be enforced).
			IEnumerable<Tuple<Vector2, MainGuiObject>> hitObjects = GetHitObjects(newGuiObjects, Bounds);
			//hitObjects = hitObjects.Where(hp => hp.Item2.Team != Team);
			var hitMgo = hitObjects.Any() ? hitObjects.First().Item2 : null;
			if (hitMgo != null)
				Expire(hitMgo);
			//}
		}

		public override bool CanPushObjects()
		{
			return HasAbility(AbilityAttributes.CanPush);
		}

		protected override IEnumerable<MainGuiObject> GetAllVerticalPassableGroups(IEnumerable<MainGuiObject> guiObjects)
		{
			return guiObjects.Where(g => g.Group == Group.ImpassableIncludingMagic);
		}
	}
}
