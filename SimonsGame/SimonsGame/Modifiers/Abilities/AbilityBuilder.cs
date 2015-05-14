using SimonsGame.Modifiers.Abilities;
using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Utility;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects.BaseClasses;

namespace SimonsGame.Modifiers
{
	public class AbilityBuilder
	{
		public static PlayerAbilityInfo GetJumpAbility(PhysicsObject character, float power, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Jump")
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (character.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.AllControls[character.Id].YMovement < -.5 && Controls.PreviousControls[character.Id].YMovement >= -.5;
				},
				CastAmount = castAmount,
				Cooldown = TimeSpan.Zero,
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new SingleJump(character, power, () => character.Movement.Y >= 0, () => { return character.IsStunned; }),
				KnownAbility = KnownAbility.Jump
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetLongRangeElementalAbility1(Player player, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			AbilityAttributes abilityAttributes = AbilityAttributes.ClickToDetonate | AbilityAttributes.Explosion | AbilityAttributes.PassWall | AbilityAttributes.PassCharacters;
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Ball", abilityAttributes)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.PressedDown(player.Id, manager.AbilityButtonMap[id]); //AvailableButtons.RightBumper);
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 3, 0),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new LongRangeElementalMagicAbility(player, () => Controls.PressedDown(player.Id, player.AbilityManager.AbilityButtonMap[id]), Element.Fire),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}
		public static PlayerAbilityInfo GetBaseLongRangeElementAbility(PhysicsObject character, string baseSprite, float castAmount = 0, int duration = 81)
		{
			Texture2D texture = MainGame.ContentManager.Load<Texture2D>(baseSprite);
			Animation animation = new Animation(texture, 1, false, texture.Bounds.Width, texture.Bounds.Height, new Vector2(.25f), (float)(Math.PI / 30f));
			Guid id = Guid.NewGuid();
			ProjectileElementalMagicAbility modifier = new ProjectileElementalMagicAbility(character, AbilityAttributes.None, animation, Element.Normal, id, 9.5f, -200, duration);
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Ball2", AbilityAttributes.None)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (character.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					PlayerAbilityInfo self = manager.GetAbilityInfo(id);

					return self.AbilityAttributes.HasFlag(AbilityAttributes.ClickToDetonate)
						? Controls.PressedDown(character.Id, manager.AbilityButtonMap[id])
						: Controls.IsDown(character.Id, manager.AbilityButtonMap[id]); //AvailableButtons.RightBumper);
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 1, 0),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = modifier,
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetBasePushElementAbility(Player player, string baseSprite, float castAmount = 0)
		{
			Texture2D texture = MainGame.ContentManager.Load<Texture2D>(baseSprite);
			Animation animation = new Animation(texture, 1, false, texture.Bounds.Width, texture.Bounds.Height, new Vector2(.1f), 0);
			Guid id = Guid.NewGuid();
			AbilityAttributes abilityAttributes = AbilityAttributes.CanPush | AbilityAttributes.OnlyHorizontal | AbilityAttributes.PassCharacters | AbilityAttributes.PassWall;
			ProjectileElementalMagicAbility modifier = new ProjectileElementalMagicAbility(player, abilityAttributes, animation, Element.Normal, id, 4, 0);
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Push", abilityAttributes)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (player.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.PressedDown(player.Id, manager.AbilityButtonMap[id]); //AvailableButtons.RightBumper);
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 4, 0),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = modifier,
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetShortRangeMeleeElementalAbility1(PhysicsObject character, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Melee", AbilityAttributes.Pierce)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (character.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.IsDown(character.Id, manager.AbilityButtonMap[id]);//AvailableButtons.RightTrigger
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 0, 200),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new ShortRangeMeleeElementalMagicAbility(character, Element.Water),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}



		public static PlayerAbilityInfo GetSelfHealAbility(PhysicsObject character, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Heal", AbilityAttributes.Pierce)
			{
				IsUsable = (manager) =>
				{
					if (manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.IsDown(character.Id, manager.AbilityButtonMap[id]);//AvailableButtons.RightTrigger
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 4, 200),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new HealSelf(character, Element.Water),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetShortRangeProjectileElementalAbility1(Player player, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Daft")
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (player.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.IsDown(player.Id, manager.AbilityButtonMap[id]);//AvailableButtons.RightTrigger
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 0, 100),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new ShortRangeProjectileElementalMagicAbility(player, Element.Grass),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetSurroundRangeElementalAbility1(Player player, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Surround", AbilityAttributes.SpeedUp)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (player.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.PressedDown(player.Id, manager.AbilityButtonMap[id]);//AvailableButtons.LeftTrigger
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 0, 1000),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new SurroundRangeElementalMagicAbility(player, Element.Normal),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetBlinkMiscAbility(PhysicsObject player)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Blink")
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (player.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.PressedDown(player.Id, manager.AbilityButtonMap[id]);//AvailableButtons.LeftTrigger
				},
				CastAmount = 0,
				Cooldown = new TimeSpan(0, 0, 0, 0, 1000),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new BlinkAbility(player, 3, () => Controls.ReleasedDown(player.Id, player.AbilityManager.AbilityButtonMap[id])),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetTurretAttackAbility(StandardTurret turret)
		{
			Guid id = Guid.NewGuid();
			MultiPlayerAbilityInfo playerAbility = new MultiPlayerAbilityInfo(id, 5, "Turret Shot")
			{
				CastAmount = 0,
				Cooldown = new TimeSpan(0, 0, 0, 0, 120),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new TurretAttackAbility(turret, Element.Normal),
				KnownAbility = KnownAbility.Elemental
			};
			playerAbility.IsUsable = (manager) =>
			{
				var set = manager.CurrentAbilities.Keys.Intersect(playerAbility.GetAbilityIds());
				bool hasNotUsedAll = set.Count() != playerAbility.GetAbilityIds().Count();
				if (!hasNotUsedAll)
					return false;
				// Could be cleaned up some.
				List<MainGuiObject> characters = turret.Level.GetAllCharacterObjects().Where(p => p.Team != turret.Team && p.Team > Team.Neutral).ToList();
				bool characterCloseEnough = false;
				characters.ForEach(p =>
				{
					characterCloseEnough = characterCloseEnough || MainGuiObject.GetIntersectionDepth(p.Bounds, turret.SensorBounds) != Vector2.Zero;
				});
				return characterCloseEnough && hasNotUsedAll;
			};

			return playerAbility;
		}




		public static MultiPlayerAbilityInfo GetBossProjectile(GenericBoss boss, string baseSprite)
		{
			Texture2D texture = MainGame.ContentManager.Load<Texture2D>(baseSprite);
			Animation animation = new Animation(texture, 1, false, texture.Bounds.Width, texture.Bounds.Height, new Vector2(.25f), (float)(Math.PI / 30f));
			Guid id = Guid.NewGuid();
			ProjectileElementalMagicAbility modifier = new ProjectileElementalMagicAbility(boss, AbilityAttributes.None, animation, Element.Normal, id, 9.5f, -200, 140);
			MultiPlayerAbilityInfo playerAbility = new MultiPlayerAbilityInfo(id, 250, "CreepBoss", AbilityAttributes.None) // Oh Boy!  There's a lot of 'em!
			{
				CastAmount = 0,
				Cooldown = new TimeSpan(0, 0, 0, 0, 20),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = modifier,
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}
	}
}
