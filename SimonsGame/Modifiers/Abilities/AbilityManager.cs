using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers
{
	public enum KnownAbility
	{
		Jump,
		Elemental,
		Miscellaneous
	}

	public class PlayerAbilityInfo
	{
		// Must Set Id.
		public PlayerAbilityInfo(Guid id, int numberAvailable = 1)
		{
			_id = id;
			NumberAvailable = numberAvailable;
		}
		// Unique identifier for this particular ability.
		private Guid _id;
		public Guid Id { get { return _id; } }

		public int NumberAvailable { get; set; }

		// Function that checks if the ability can be used or not.
		public Func<AbilityManager, bool> IsUsable { get; set; }

		// The amount of magic the ability will use to cast.
		public float CastAmount { get; set; }

		// If the magic can be held down, and this is NOT The first tick of casting, then this is the amount of magic the ability will use to initiate.
		public float ReChargeAmount { get; set; }

		// This is the amount of time until you can use this ability again.
		public TimeSpan Cooldown { get; set; }

		// This is the amount of time until you can use an ability with a recharge amount.
		public int LayoverTickCount { get; set; }

		public ModifierBase Modifier { get; set; }
	}

	public class AbilityManager
	{
		private PhysicsObject _player;

		// Abilities that will be added when a Player is created.
		// Later, these lists can be expanded/Modified when the game is played.
		public Dictionary<KnownAbility, List<PlayerAbilityInfo>> KnownAbilities { get { return _knownAbilities; } }
		private Dictionary<KnownAbility, List<PlayerAbilityInfo>> _knownAbilities;
		List<Guid> _knownAbilityIds;

		// Cooldowns for specific Abilities
		private Dictionary<Guid, TimeSpan> _coolDownCounter;
		private Dictionary<Guid, int> _layoverCounter;

		// These are the abilities that are currently active on the user.
		private Dictionary<Guid, ModifierBase> _currentAbilities = new Dictionary<Guid, ModifierBase>();
		public Dictionary<Guid, ModifierBase> CurrentAbilities { get { return _currentAbilities; } }
		public AbilityManager(PhysicsObject player, Dictionary<KnownAbility, List<PlayerAbilityInfo>> knownAbilities)
		{
			_coolDownCounter = new Dictionary<Guid, TimeSpan>();
			_layoverCounter = new Dictionary<Guid, int>();
			_knownAbilities = knownAbilities;
			_knownAbilityIds = _knownAbilities.Values.SelectMany(ps => ps).Select(pi => pi.Id).ToList();
			_player = player;
		}

		public void AddKnownAbility(KnownAbility type, PlayerAbilityInfo abilityInfo)
		{
			// If it's already added, then don't add again.
			if (_knownAbilityIds.Contains(abilityInfo.Id))
				return;
			List<PlayerAbilityInfo> abilityInfos;
			if (_knownAbilities.TryGetValue(type, out abilityInfos))
			{
				abilityInfos.Add(abilityInfo);
				_knownAbilityIds.Add(abilityInfo.Id);
			}
		}

		public void CheckKnownAbilities(GameTime gameTime)
		{

			// Check to see if any abilities have been used.
			foreach (KeyValuePair<KnownAbility, List<PlayerAbilityInfo>> pair in _knownAbilities)
			{
				foreach (PlayerAbilityInfo playerAbilityInfo in pair.Value)
				{
					if (ShouldRechargeAbility(playerAbilityInfo.Id, playerAbilityInfo))
					{
						_player.UseMana(playerAbilityInfo.ReChargeAmount);
					}
					if (CanUseAbility(playerAbilityInfo.Id, playerAbilityInfo))
					{
						ModifierBase currentModifier = playerAbilityInfo.Modifier.Clone();
						//currentModifier.Reset();
						_currentAbilities.Add(playerAbilityInfo.Id, currentModifier);
						_player.UseMana(playerAbilityInfo.CastAmount);
					}

					// Check abilities that are on cooldown.
					TimeSpan coolDownTime;
					if (_coolDownCounter.TryGetValue(playerAbilityInfo.Id, out coolDownTime) && coolDownTime > TimeSpan.Zero)
					{
						if (coolDownTime >= playerAbilityInfo.Cooldown)
							_coolDownCounter.Remove(playerAbilityInfo.Id);
						else
							_coolDownCounter[playerAbilityInfo.Id] += gameTime.ElapsedGameTime;
					}

					// Check if abilities are done with their layover.
					if (_layoverCounter.ContainsKey(playerAbilityInfo.Id))
					{
						_layoverCounter[playerAbilityInfo.Id]++;
						if (_layoverCounter[playerAbilityInfo.Id] > playerAbilityInfo.LayoverTickCount)
							_layoverCounter.Remove(playerAbilityInfo.Id);
					}
				}
			}
		}

		private bool ShouldRechargeAbility(Guid id, PlayerAbilityInfo playerAbilityInfo)
		{
			TimeSpan coolDownTime;
			return playerAbilityInfo.ReChargeAmount > 0
				&& playerAbilityInfo.IsUsable(this)
				&& playerAbilityInfo.ReChargeAmount <= _player.ManaCurrent
				&& (!_coolDownCounter.TryGetValue(playerAbilityInfo.Id, out coolDownTime) || coolDownTime == TimeSpan.Zero)
				&& !_layoverCounter.ContainsKey(id);
		}
		public bool CanUseAbility(Guid id, PlayerAbilityInfo playerAbilityInfo)
		{
			TimeSpan coolDownTime;
			return playerAbilityInfo.IsUsable(this)
				&& playerAbilityInfo.CastAmount <= _player.ManaCurrent
				&& ((!_coolDownCounter.TryGetValue(playerAbilityInfo.Id, out coolDownTime) || coolDownTime == TimeSpan.Zero) || (playerAbilityInfo.ReChargeAmount > 0 && _layoverCounter.ContainsKey(playerAbilityInfo.Id)));
		}

		public void HasExpired(Guid id)
		{
			_currentAbilities.Remove(id);
			if (_knownAbilityIds.Contains(id))
			{
				_coolDownCounter.Add(id, new TimeSpan(1));
				_layoverCounter.Add(id, 1);
			}
		}


		internal void AddAbility(ModifierBase mb)
		{
			if (!_currentAbilities.ContainsKey(mb.Id))
				_currentAbilities.Add(mb.Id, mb);
		}
		public float CoolDownTimer(PlayerAbilityInfo abilityInfo)
		{
			TimeSpan timeInCooldown;
			if (_coolDownCounter.TryGetValue(abilityInfo.Id, out timeInCooldown))
				return (float)(abilityInfo.Cooldown.TotalMilliseconds - timeInCooldown.TotalMilliseconds);
			return 0;
		}
	}
}
