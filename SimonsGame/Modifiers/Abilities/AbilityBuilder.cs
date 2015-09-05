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
		private static Dictionary<string, Func<PhysicsObject, PlayerAbilityInfo>> _magicNameMap = new Dictionary<string, Func<PhysicsObject, PlayerAbilityInfo>>()
		{
			{ "Pull", GetPullMiscAbility },
			{ "Push", GetPushMiscAbility },
			{ "SpeedUp", GetSurroundMiscAbility },
			{ "Heal", GetSelfHealAbility },
		};
		public static PlayerAbilityInfo GetJumpAbility(PhysicsObject character, float power, float castAmount = 0)
		{
			Player player = character as Player;
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Jump")
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (character.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.AllControls[character.Id].IsJumping && !Controls.PreviousControls[character.Id].IsJumping;
				},
				CastAmount = castAmount,
				Cooldown = TimeSpan.Zero,
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new SingleJump(character, power, () => !Controls.AllControls[character.Id].IsJumping, () => { return character.IsStunned; }),
				KnownAbility = KnownAbility.Jump
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetBaseLongRangeElementAbility(PhysicsObject character, string baseSprite, float castAmount = 0, int duration = 81)
		{
			Texture2D texture = MainGame.ContentManager.Load<Texture2D>(baseSprite);
			Animation animation = new Animation(texture, 1, false, texture.Bounds.Width, texture.Bounds.Height, new Vector2(.25f), (float)(Math.PI / 30f));
			Guid id = Guid.NewGuid();
			ProjectileElementalMagicAbility modifier = new ProjectileElementalMagicAbility(character, AbilityAttributes.None, animation, new Tuple<Element, float>(Element.Fire, .3f), id, 9.5f, -200, duration);
			MultiPlayerAbilityInfo playerAbility = new MultiPlayerAbilityInfo(id, 5, "Ball2", AbilityAttributes.None)
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
				Cooldown = new TimeSpan(0, 0, 0, 3, 0),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = modifier,
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetPushMiscAbility(PhysicsObject mgo)
		{
			Texture2D texture = MainGame.ContentManager.Load<Texture2D>("Test/PushingWave");
			Animation animation = new Animation(texture, 1, false, texture.Bounds.Width, texture.Bounds.Height, new Vector2(.1f), 0);
			Guid id = Guid.NewGuid();
			AbilityAttributes abilityAttributes = AbilityAttributes.CanPush | AbilityAttributes.OnlyHorizontal | AbilityAttributes.PassCharacters;
			ProjectileElementalMagicAbility modifier = new ProjectileElementalMagicAbility(mgo, abilityAttributes, animation, new Tuple<Element, float>(Element.Normal, 0), id, 4, 0);
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Push", abilityAttributes)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (mgo.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.PressedDown(mgo.Id, manager.AbilityButtonMap[id]); //AvailableButtons.RightBumper);
				},
				CastAmount = 0,
				Cooldown = new TimeSpan(0, 0, 0, 5, 0),
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
				Cooldown = new TimeSpan(0, 0, 0, 0, 500),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new ShortRangeMeleeElementalMagicAbility(character, new Tuple<Element, float>(Element.Water, .3f)),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetSelfHealAbility(PhysicsObject character)
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
				CastAmount = 0,
				Cooldown = new TimeSpan(0, 0, 0, 12),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new HealSelf(character, new Tuple<Element, float>(Element.Water, .3f)),
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
				Modifier = new ShortRangeProjectileElementalMagicAbility(player, new Tuple<Element, float>(Element.Plant, .3f)),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetSurroundMiscAbility(PhysicsObject mgo)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "SpeedUp", AbilityAttributes.SpeedUp)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (mgo.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.PressedDown(mgo.Id, manager.AbilityButtonMap[id]);//AvailableButtons.LeftTrigger
				},
				CastAmount = 0,
				Cooldown = new TimeSpan(0, 0, 0, 0, 8000),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new SurroundRangeElementalMagicAbility(mgo, new Tuple<Element, float>(Element.Normal, 0)),
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
				Modifier = new BlinkAbility(player, 4, () => Controls.ReleasedDown(player.Id, player.AbilityManager.AbilityButtonMap[id])),
				KnownAbility = KnownAbility.Elemental
			};

			return playerAbility;
		}

		public static PlayerAbilityInfo GetPullMiscAbility(PhysicsObject player)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id, "Pull")
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (player.IsStunned || manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.PressedDown(player.Id, manager.AbilityButtonMap[id]);//AvailableButtons.LeftTrigger
				},
				CastAmount = 0,
				Cooldown = new TimeSpan(0, 0, 0, 6),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new PullAbility(player),
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
				Cooldown = new TimeSpan(0, 0, 0, 0, 150),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new TurretAttackAbility(turret, new Tuple<Element, float>(Element.Normal, 0)),
				KnownAbility = KnownAbility.Elemental
			};
			playerAbility.IsUsable = (manager) =>
			{
				var set = manager.CurrentAbilities.Keys.Intersect(playerAbility.GetAbilityIds());
				bool hasNotUsedAll = set.Count() != playerAbility.GetAbilityIds().Count();
				if (!hasNotUsedAll)
					return false;
				// Could be cleaned up some.
				IEnumerable<MainGuiObject> characters = turret.Level.GetAllCharacterObjects(turret.SensorBounds);
				//if (characters.Count() > 500)
				//{
				//	foreach (var character in characters)
				//		character.HitBoxColor = Color.Black;
				//	turret.HitBoxColor = Color.White;
				//}
				characters = characters.Where(p => p.Team != turret.Team && p.Team > Team.Neutral);
				bool characterCloseEnough = false;
				foreach (MainGuiObject p in characters)
					characterCloseEnough = characterCloseEnough || MainGuiObject.GetIntersectionDepth(p.Bounds, turret.SensorBounds) != Vector2.Zero;

				return characterCloseEnough && hasNotUsedAll;
			};

			return playerAbility;
		}

		public static MultiPlayerAbilityInfo GetBossProjectile(PhysicsObject mgo, string baseSprite)
		{
			Texture2D texture = MainGame.ContentManager.Load<Texture2D>(baseSprite);
			Animation animation = new Animation(texture, 1, false, texture.Bounds.Width, texture.Bounds.Height, new Vector2(.25f), (float)(Math.PI / 30f));
			Guid id = Guid.NewGuid();
			ProjectileElementalMagicAbility modifier = new ProjectileElementalMagicAbility(mgo, AbilityAttributes.None, animation, new Tuple<Element, float>(Element.Fire, .3f), id, 9.5f, -200, 140);
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

		public static PlayerAbilityInfo GetBaseAbilityFromElement(PhysicsObject mgo, Element element, string magicName)
		{
			Func<PhysicsObject, PlayerAbilityInfo> outFunc;
			if (_magicNameMap.TryGetValue(magicName, out outFunc))
			{
				return outFunc(mgo);
			}

			if (element == Element.Lightning)
				return GetBaseLightningAbility(mgo, magicName);
			if (element == Element.Rock)
				return GetBaseRockAbility(mgo, magicName);
			if (element == Element.Fire)
			{
				if (magicName == "FireBall")
					return GetBaseFireAbility(mgo, magicName);
				else if (magicName == "Ball")
				{
					PlayerAbilityInfo pai = GetBaseLongRangeElementAbility(mgo, "Test/Fireball");
					pai.Name = "Ball";
					pai.AbilityAttributes = AbilityAttributes.PassWall;
					pai.Modifier.SetSize(new Vector2(62, 62));
					pai.Modifier.Speed = 7;
					pai.Modifier.LevelUpMagicHitBoxBuffer(new Vector4(-10, -10, 20, 20));
					return pai;
				}
			}
			return null;
		}

		public static PlayerAbilityInfo GetBaseLightningAbility(PhysicsObject mgo, string magicName)
		{
			PlayerAbilityInfo pai = AbilityBuilder.GetBaseLongRangeElementAbility(mgo, "Test/LightningBolt");
			pai.Name = magicName;

			pai.Modifier.StopRotation();
			pai.Modifier.LevelUpMagicAddAbilities(AbilityAttributes.Explosion);
			pai.IsUsable = (manager) =>
			{
				if (mgo.IsStunned || manager.CurrentAbilities.ContainsKey(pai.Id)) // May want to do something with this.  Double Jump stuff.  Combos?
					return false;

				PlayerAbilityInfo self = manager.GetAbilityInfo(pai.Id);

				return self.AbilityAttributes.HasFlag(AbilityAttributes.ClickToDetonate)
					? Controls.PressedDown(mgo.Id, manager.AbilityButtonMap[pai.Id])
					: Controls.IsDown(mgo.Id, manager.AbilityButtonMap[pai.Id]); //AvailableButtons.RightBumper);
			};
			pai.Modifier.Damage = -100;
			pai.Cooldown = new TimeSpan(0, 0, 0, 1, 750);
			pai.Modifier.Element = new Tuple<Element, float>(Element.Lightning, .25f);
			pai.Modifier.SetSize(new Vector2(50, 25));
			pai.Modifier.Speed = 12.5f;

			pai.Modifier.LevelUpMagicDuration(50);

			return pai;
		}

		public static PlayerAbilityInfo GetBaseRockAbility(PhysicsObject mgo, string magicName)
		{
			PlayerAbilityInfo pai = AbilityBuilder.GetBaseLongRangeElementAbility(mgo, "Test/Fireball");
			pai.Name = magicName;

			pai.Modifier.LevelUpMagicAddAbilities(AbilityAttributes.None);

			pai.IsUsable = (manager) =>
			{
				if (mgo.IsStunned || manager.CurrentAbilities.ContainsKey(pai.Id)) // May want to do something with this.  Double Jump stuff.  Combos?
					return false;

				PlayerAbilityInfo self = manager.GetAbilityInfo(pai.Id);

				return self.AbilityAttributes.HasFlag(AbilityAttributes.ClickToDetonate)
					? Controls.PressedDown(mgo.Id, manager.AbilityButtonMap[pai.Id])
					: Controls.IsDown(mgo.Id, manager.AbilityButtonMap[pai.Id]); //AvailableButtons.RightBumper);
			};
			pai.Modifier.Damage = -220;
			pai.Modifier.Element = new Tuple<Element, float>(Element.Rock, .25f);
			pai.Modifier.SetSize(new Vector2(80, 80));
			pai.Modifier.Speed = 4f;
			pai.Cooldown = new TimeSpan(0, 0, 0, 2, 250);
			return pai;
		}

		public static PlayerAbilityInfo GetBaseFireAbility(PhysicsObject mgo, string magicName)
		{
			PlayerAbilityInfo pai = AbilityBuilder.GetBossProjectile(mgo, "Test/Fireball");
			pai.Name = magicName;
			pai.Modifier.LevelUpMagicAddAbilities(AbilityAttributes.None);

			pai.IsUsable = (manager) =>
			{
				var set = manager.CurrentAbilities.Keys.Intersect(pai.GetAbilityIds());
				if (mgo.IsStunned || set.Count() == pai.GetAbilityIds().Count())
					return false;

				return pai.AbilityAttributes.HasFlag(AbilityAttributes.ClickToDetonate)
					? Controls.PressedDown(mgo.Id, manager.AbilityButtonMap[pai.Id])
					: Controls.IsDown(mgo.Id, manager.AbilityButtonMap[pai.Id]); //AvailableButtons.RightBumper);
			};

			pai.Cooldown = new TimeSpan(0, 0, 0, 0, 100);
			pai.Modifier.Damage = -16;
			pai.Modifier.Element = new Tuple<Element, float>(Element.Fire, .25f);
			pai.Modifier.SetSize(new Vector2(25, 25));
			pai.Modifier.Speed = 6f;
			pai.Modifier.LevelUpMagicDuration(46);
			return pai;
		}

	}
}
