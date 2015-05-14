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
	public enum AbilityAttributes
	{
		None = 1,
		ClickToDetonate = 2,
		Explosion = 4,
		PassWall = 8,
		PassCharacters = 16,
		Pierce = 32,
		SpeedUp = 64, // Only for decoration,
		OnlyHorizontal = 128,
		CanPush = 256,
	}

	public class PlayerAbilityInfo
	{
		// Must Set Id and name.
		public PlayerAbilityInfo(Guid id, string name, AbilityAttributes abilityAttributes = AbilityAttributes.None)
		{
			_id = id;
			Name = name;
			_abilityAttributes = abilityAttributes;
		}
		// Unique identifier for this particular ability.
		private Guid _id;
		public Guid Id { get { return _id; } }
		public string Name { get; set; }

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

		public AbilityModifier Modifier { get; set; }

		public KnownAbility KnownAbility { get; set; }

		public virtual List<Guid> GetAbilityIds()
		{
			return new List<Guid>() { Id };
		}
		public virtual Guid GetNextAbilityId()
		{
			return Id;
		}
		private AbilityAttributes _abilityAttributes;
		public AbilityAttributes AbilityAttributes
		{
			get { return _abilityAttributes; }
			set
			{
				if (Modifier != null) Modifier.AbilityAttributes = value;
				_abilityAttributes = value;
			}
		}
	}
	public class MultiPlayerAbilityInfo : PlayerAbilityInfo
	{
		private List<Guid> _abilityIds;
		private int _currentIdIndex = 0;
		public MultiPlayerAbilityInfo(Guid id, int count, string name, AbilityAttributes abilityAttributes = AbilityAttributes.None)
			: base(id, name, abilityAttributes)
		{
			_abilityIds = new List<Guid>();
			for (int i = 0; i < count; i++)
				_abilityIds.Add(Guid.NewGuid());
		}
		public override List<Guid> GetAbilityIds()
		{
			return _abilityIds;
		}
		public override Guid GetNextAbilityId()
		{
			Guid returnId = _abilityIds[_currentIdIndex];
			_currentIdIndex = (_currentIdIndex + 1) % _abilityIds.Count(); // Cycle... If this doesn't work in the future, I will cry, and change it.
			return returnId;
		}
	}

	public class AbilityManager
	{
		private PhysicsObject _character;

		// Performance Idea: DONE!!
		// Seperate list with PlayerAbilityInfo.
		// All other instances only store the Guid.

		// Abilities that will be added when a Player is created.
		// Later, these lists can be expanded/Modified when the game is played.
		private Dictionary<Guid, PlayerAbilityInfo> _abilityMap;
		public PlayerAbilityInfo GetAbilityInfo(Guid guid)
		{
			PlayerAbilityInfo pai;
			_abilityMap.TryGetValue(guid, out pai);
			return pai;
		}

		// A mapping between what kind of magic an ability is to the list of IDs that make up
		// the player abilities.
		public Dictionary<KnownAbility, List<Guid>> KnownAbilities { get; private set; }

		// These are the abilites that currently are selected to use for the player.
		// In a perfect world, they will be selected from the Usable Buttons variable.
		public HashSet<Guid> ActiveAbilities { get; private set; }

		// These are the buttons that the user can assign magic to.
		public AvailableButtons UsableButtons { get; set; }

		// Mapping of player abilities to the buttons that activate them.
		public Dictionary<Guid, AvailableButtons> AbilityButtonMap { get; private set; }

		public List<Guid> _knownAbilityIds;
		public List<Guid> KnownAbilityIds { get { return _knownAbilityIds.ToList(); } } // Copy it so no one tampers with it!

		// Cooldowns for specific Abilities
		private Dictionary<Guid, TimeSpan> _coolDownCounter;
		// For abilites that are active when held down, this is the "cooldown" for
		// when they stop.
		private Dictionary<Guid, int> _layoverCounter;

		// These are the abilities that are currently active on the user.
		private Dictionary<Guid, ModifierBase> _currentAbilities = new Dictionary<Guid, ModifierBase>();
		public Dictionary<Guid, ModifierBase> CurrentAbilities { get { return _currentAbilities; } }
		public AbilityManager(PhysicsObject player, Dictionary<KnownAbility, List<PlayerAbilityInfo>> knownAbilities, AvailableButtons usableButtons)
		{
			_abilityMap = knownAbilities.Values.SelectMany(ls => ls).ToDictionary(pai => pai.Id, pai => pai);
			_coolDownCounter = new Dictionary<Guid, TimeSpan>();
			_layoverCounter = new Dictionary<Guid, int>();
			KnownAbilities = knownAbilities.ToDictionary(kv => kv.Key, kv => kv.Value.Select(pai => pai.Id).ToList());
			_knownAbilityIds = KnownAbilities.Values.SelectMany(ps => ps).Select(pi => pi).ToList();
			_character = player;
			ActiveAbilities = new HashSet<Guid>();
			AbilityButtonMap = KnownAbilities.Values.SelectMany(ps => ps).ToDictionary(pi => pi, pi => AvailableButtons.None);
			UsableButtons = usableButtons;
		}

		public void AddKnownAbility(KnownAbility type, PlayerAbilityInfo abilityInfo)
		{
			// If it's already added, then don't add again.
			if (_knownAbilityIds.Contains(abilityInfo.Id))
				return;
			List<Guid> abilityIDs;
			if (KnownAbilities.TryGetValue(type, out abilityIDs))
			{
				abilityIDs.Add(abilityInfo.Id);
				_knownAbilityIds.Add(abilityInfo.Id);
				_abilityMap.Add(abilityInfo.Id, abilityInfo);
				AbilityButtonMap[abilityInfo.Id] = AvailableButtons.None;
			}
		}

		public void CheckKnownAbilities(GameTime gameTime)
		{

			// Check to see if any abilities have been used.
			foreach (KeyValuePair<KnownAbility, List<Guid>> pair in KnownAbilities)
			{
				foreach (Guid abilityId in pair.Value)
				{
					PlayerAbilityInfo pai = _abilityMap[abilityId];
					if (ShouldRechargeAbility(abilityId, pai))
					{
						_character.UseMana(pai.ReChargeAmount);
					}
					if (CanUseAbility(abilityId, pai))
					{
						ModifierBase currentModifier = pai.Modifier.Clone();
						//currentModifier.Reset();
						_currentAbilities.Add(pai.GetNextAbilityId(), currentModifier);

						if (ShouldStartCooldownImmediately(pai))
							_coolDownCounter.Add(abilityId, new TimeSpan(1));
						_character.UseMana(pai.CastAmount);
					}

					// Check abilities that are on cooldown.
					TimeSpan coolDownTime;
					if (_coolDownCounter.TryGetValue(abilityId, out coolDownTime) && coolDownTime > TimeSpan.Zero)
					{
						if (coolDownTime >= pai.Cooldown)
							_coolDownCounter.Remove(abilityId);
						else
							_coolDownCounter[abilityId] += gameTime.ElapsedGameTime;
					}

					// Check if abilities are done with their layover.
					if (_layoverCounter.ContainsKey(abilityId))
					{
						_layoverCounter[abilityId]++;
						if (_layoverCounter[abilityId] > pai.LayoverTickCount)
							_layoverCounter.Remove(abilityId);
					}
				}
			}
		}

		private bool ShouldStartCooldownImmediately(PlayerAbilityInfo playerAbilityInfo)
		{
			return playerAbilityInfo is MultiPlayerAbilityInfo;
		}

		private bool ShouldRechargeAbility(Guid id, PlayerAbilityInfo playerAbilityInfo)
		{
			TimeSpan coolDownTime;
			return playerAbilityInfo.ReChargeAmount > 0
				&& playerAbilityInfo.IsUsable(this)
				&& playerAbilityInfo.ReChargeAmount <= _character.ManaCurrent
				&& (!_coolDownCounter.TryGetValue(playerAbilityInfo.Id, out coolDownTime) || coolDownTime == TimeSpan.Zero)
				&& !_layoverCounter.ContainsKey(id);
		}
		public bool CanUseAbility(Guid id, PlayerAbilityInfo playerAbilityInfo)
		{
			TimeSpan coolDownTime;
			return playerAbilityInfo.IsUsable(this)
				&& playerAbilityInfo.CastAmount <= _character.ManaCurrent
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


		public void AddAbility(ModifierBase mb)
		{
			if (mb == null)
				return;
			if (!_currentAbilities.ContainsKey(mb.Id))
				_currentAbilities.Add(mb.Id, mb);
		}
		public float CoolDownTimer(Guid abilityId)
		{
			PlayerAbilityInfo pai = _abilityMap[abilityId];
			TimeSpan timeInCooldown;
			if (_coolDownCounter.TryGetValue(abilityId, out timeInCooldown))
				return (float)(pai.Cooldown.TotalMilliseconds - timeInCooldown.TotalMilliseconds);
			return 0;
		}

		public void SetAbility(PlayerAbilityInfo pai, AvailableButtons availableButtons)
		{
			if (!UsableButtons.HasFlag(availableButtons))
				return;
			AvailableButtons selectedButton = AbilityButtonMap[pai.Id];
			// Get the magic that is currently assigned to the button.
			var assignedMagic = AbilityButtonMap.FirstOrDefault(kv => kv.Value == availableButtons);
			if (assignedMagic.Key != Guid.Empty)
			{
				AbilityButtonMap[assignedMagic.Key] = selectedButton;
				if (selectedButton == AvailableButtons.None)
					ActiveAbilities.Remove(ActiveAbilities.FirstOrDefault(aa => aa == assignedMagic.Key));
			}
			AbilityButtonMap[pai.Id] = availableButtons;
			ActiveAbilities.Add(pai.Id);
		}
	}
}
