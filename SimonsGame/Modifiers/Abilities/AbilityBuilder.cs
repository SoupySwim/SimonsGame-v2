using SimonsGame.Modifiers.Abilities;
using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
					bool isDown = Controls.IsDown(player.Id, AvailableButtons.LeftBumper);
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
					return Controls.IsDown(player.Id, AvailableButtons.LeftBumper);
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
					return Controls.IsDown(player.Id, AvailableButtons.RightTrigger);
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
	}
}
