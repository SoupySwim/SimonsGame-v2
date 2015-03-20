using SimonsGame.Modifiers.Abilities;
using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.GuiObjects.Utility;

namespace SimonsGame.Modifiers
{
	public class AbilityBuilder
	{
		public static PlayerAbilityInfo GetJumpAbility(Player player, float power, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return GameStateManager.AllControls[player.Id].YMovement < -.5;
				},
				CastAmount = castAmount,
				Cooldown = TimeSpan.Zero,
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new SingleJump(player, power, () => player.Movement.Y >= 0, () =>
				{

					return false;
				})
			};

			//SingleJump ability = new SingleJump(player, power, () => player.Movement.Y >= 0, () => false); // new Jump(player, pow, checkStopped, forceStop, isSuperJump);
			return playerAbility;
		}

		public static PlayerAbilityInfo GetLongRangeElementalAbility1(Player player, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.PressedDown(player.Id, AvailableButtons.RightBumper);
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 3, 0),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new LongRangeElementalMagicAbility(player, () => Controls.PressedDown(player.Id, AvailableButtons.RightBumper))
			};

			//SingleJump ability = new SingleJump(player, power, () => player.Movement.Y >= 0, () => false); // new Jump(player, pow, checkStopped, forceStop, isSuperJump);
			return playerAbility;
		}

		public static PlayerAbilityInfo GetShortRangeMeleeElementalAbility1(Player player, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					bool isDown = Controls.IsDown(player.Id, AvailableButtons.RightTrigger);
					return isDown;
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 0, 200),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new ShortRangeMeleeElementalMagicAbility(player)
			};

			//SingleJump ability = new SingleJump(player, power, () => player.Movement.Y >= 0, () => false); // new Jump(player, pow, checkStopped, forceStop, isSuperJump);
			return playerAbility;
		}

		public static PlayerAbilityInfo GetShortRangeProjectileElementalAbility1(Player player, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.IsDown(player.Id, AvailableButtons.RightTrigger);
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 0, 100),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new ShortRangeProjectileElementalMagicAbility(player)
			};

			//SingleJump ability = new SingleJump(player, power, () => player.Movement.Y >= 0, () => false); // new Jump(player, pow, checkStopped, forceStop, isSuperJump);
			return playerAbility;
		}

		public static PlayerAbilityInfo GetSurroundRangeElementalAbility1(Player player, float castAmount = 0)
		{
			Guid id = Guid.NewGuid();
			PlayerAbilityInfo playerAbility = new PlayerAbilityInfo(id)
			{
				IsUsable = (manager) =>
				{
					// If we already have a jump active, don't jump again.
					if (manager.CurrentAbilities.ContainsKey(id)) // May want to do something with this.  Double Jump stuff.  Combos?
						return false;
					return Controls.IsDown(player.Id, AvailableButtons.LeftTrigger);
				},
				CastAmount = castAmount,
				Cooldown = new TimeSpan(0, 0, 0, 0, 1000),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new SurroundRangeElementalMagicAbility(player)
			};

			//SingleJump ability = new SingleJump(player, power, () => player.Movement.Y >= 0, () => false); // new Jump(player, pow, checkStopped, forceStop, isSuperJump);
			return playerAbility;
		}

		public static PlayerAbilityInfo GetTurretAttackAbility(StandardTurret turret)
		{
			Guid id = Guid.NewGuid();
			MultiPlayerAbilityInfo playerAbility = new MultiPlayerAbilityInfo(id, 5)
			{
				CastAmount = 0,
				Cooldown = new TimeSpan(0, 0, 0, 0, 120),
				LayoverTickCount = 0,
				ReChargeAmount = 0,
				Modifier = new TurretAttackAbility(turret)
			};
			playerAbility.IsUsable = (manager) =>
			{
				var set = manager.CurrentAbilities.Keys.Intersect(playerAbility.GetAbilityIds());
				bool hasNotUsedAll = set.Count() != playerAbility.GetAbilityIds().Count();
				if (!hasNotUsedAll)
					return false;
				List<Player> players = turret.Level.Players.Values.Where(p => p.Team != turret.Team).ToList(); // Perhaps more team logic?
				bool playerCloseEnough = false;
				players.ForEach(p =>
				{
					playerCloseEnough = playerCloseEnough || MainGuiObject.GetIntersectionDepth(p.Bounds, turret.SensorBounds) != DoubleVector2.Zero;
				});
				return playerCloseEnough && hasNotUsedAll;
			};

			return playerAbility;
		}
	}
}
