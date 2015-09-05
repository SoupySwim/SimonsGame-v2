using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects.BaseClasses;
using SimonsGame.GuiObjects.Zones;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Modifiers.Abilities;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class ElementalCharacter : GenericBoss
	{
		protected enum ElementalCharacterWalkBehavior
		{
			CloseIntoPlayer = 0,
			StayDisatanceAway,
			StayPut,
			TeleportAway,
			TeleportTo
		}

		private Dictionary<Guid, int> _universalTickCurrent;
		private int _walkTickCurrent = 0;
		private int _teleportTickCurrent = 0;

		protected HashSet<Vector2> _teleportLocations;

		private int _intensity = 5; // How "intense" the character is. 0 through 20 scale.
		private float _ticksIntensityPercentage;

		private int _maxIntensity = 20;
		private int _maxIntensityChange = 0;

		#region Attack Specfic Attributes
		private float _radians = 0;
		private int _circleAttackLimit = 24;
		private int _flyHeight = 0; // For Flying
		private Guid _lightningAttackId;
		private Guid _fireAttackId;
		#endregion

		protected ElementalCharacterWalkBehavior _walkBehavior;

		protected Animation _idleAnimation;
		protected Animation _runAnimation;
		protected Animation _teleportAnimation;

		public ElementalCharacter(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Elemental Character")
		{
			Team = Team.Neutral;
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 2, AverageSpeed.Y);
			_healthTotal = 1200;
			_healthCurrent = _healthTotal;

			_walkBehavior = ElementalCharacterWalkBehavior.StayPut;
			// Temp Animations
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Sprites/PlayerSprites/TempPlayer/IdleFull"), 0.15f, true, 100, 200, (Size.X / 100));
			_runAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Sprites//PlayerSprites/TempPlayer/RunFull"), 0.1f, true, 100, 200, (Size.X / 100));
			_teleportAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Sprites//PlayerSprites/TempPlayer/Blob"), 0.1f, false, 50, 50, (Size.X / 50));

			_animator.Color = Color.Purple;
			_animator.PlayAnimation(_idleAnimation);

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			// Jumps.
			List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, 1.75f));
			PlayerAbilityInfo jumpPai = jumpInfos.First(ei => ei.Name == "Jump");
			SingleJump jump = jumpPai.Modifier as SingleJump;
			jump.CheckStopped = () => jump.HasReachedEnd || ((MainGame.Randomizer.Next(40) <= 1) && (_targetedObject != null && _targetedObject.Center.Y > Center.Y));
			jumpPai.IsUsable = (abilityManager) =>
			{
				if (_targetedObject != null)
				{
					// If we already have a jump active, don't jump again.
					if (abilityManager.CurrentAbilities.ContainsKey(jumpPai.Id))
						return false;
					return MainGame.Randomizer.Next(1200 - (_intensity * 50)) <= 1;
				}
				return false;
			};

			abilities.Add(KnownAbility.Jump, jumpInfos);

			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();

			// Will be getting "random" elements later.
			MultiPlayerAbilityInfo pai = AbilityBuilder.GetBossProjectile(this, "Test/Fireball");

			pai.IsUsable = (manager) =>
			{
				var set = manager.CurrentAbilities.Keys.Intersect(pai.GetAbilityIds());
				if (set.Count() == pai.GetAbilityIds().Count())
					return false;
				return _targetedObject != null && CanAttack(manager, pai);
			};

			pai.Modifier.Damage = -160;
			pai.Modifier.SetSize(new Vector2(60, 60));

			_fireAttackId = pai.Id;

			elementalInfos.Add(pai);

			// Lightning
			MultiPlayerAbilityInfo pai2 = AbilityBuilder.GetBossProjectile(this, "Test/LightningBolt");

			pai2.Modifier.StopRotation();

			pai2.IsUsable = (manager) =>
			{
				var set = manager.CurrentAbilities.Keys.Intersect(pai2.GetAbilityIds());
				if (set.Count() == pai2.GetAbilityIds().Count())
					return false;
				return _targetedObject != null && CanAttack(manager, pai2);
			};

			pai2.Modifier.Damage = -100;
			pai2.Modifier.Element = new Tuple<Element, float>(Element.Lightning, .5f);
			pai2.Modifier.SetSize(new Vector2(80, 40));
			pai2.Modifier.Speed = 19f; //10f

			_lightningAttackId = pai2.Id;

			elementalInfos.Add(pai2);

			abilities.Add(KnownAbility.Elemental, elementalInfos);

			// Miscellaneous Magic
			List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			_abilityManager = new AbilityManager(this, abilities,
				AvailableButtons.RightTrigger | AvailableButtons.RightBumper | AvailableButtons.LeftTrigger | AvailableButtons.LeftBumper);

			_universalTickCurrent = elementalInfos.ToDictionary(ei => ei.Id, ei => MainGame.Randomizer.Next(50) + 105);

			_abilityManager.Experience = (_intensity + 1) * 10;
		}

		public override void Initialize()
		{
			// ... more to come!

			_teleportLocations = new HashSet<Vector2>();
			GenericZone zone;
			//= Level.GetAllZones().FirstOrDefault(z => ZoneIds.Contains(z.Id));
			//GenericZone zone = Level.GetAllZones().FirstOrDefault(z => ZoneIds.Contains(z.Id));
			//if (zone != null)
			if (ZoneIds.Any() && Level.GetAllZones().TryGetValue(ZoneIds.FirstOrDefault(), out zone))
			{
				foreach (var env in Level.GetPossiblyHitEnvironmentObjects(zone.Bounds).Where(mgo => MainGuiObject.GetIntersectionDepth(mgo.Bounds, zone.Bounds) != Vector2.Zero))
					_teleportLocations.Add(new Vector2(env.Center.X, env.Position.Y - this.Size.Y));
			}
			if (!_teleportLocations.Any())
				_teleportLocations.Add(Position);
		}

		public override float GetXMovement()
		{
			if (_targetedObject != null)
			{
				float speed = MaxSpeed.X * (.5f + (_intensity * .1f));
				if (_walkBehavior == ElementalCharacterWalkBehavior.StayDisatanceAway)
				{
					float distance = Center.X - _targetedObject.Center.X;
					if (distance < -500 || distance > 0) // to the left of player
						return speed;
					if (distance < 0 || distance > 500)
						return -speed;
					return 0;
				}
				if (_walkBehavior == ElementalCharacterWalkBehavior.CloseIntoPlayer)
				{
					float distance = Center.X - _targetedObject.Center.X;
					if (distance < -100) // to the left of player
						return speed;
					if (distance > 100)
						return -speed;
					return 0;
				}
			}
			return 0;
		}
		public override float GetYMovement()
		{
			return base.GetYMovement();
		}
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);

			foreach (var kv in _universalTickCurrent.ToList())
			{
				if (kv.Value > 0)
					_universalTickCurrent[kv.Key]--;
			}

			if (_teleportTickCurrent > 0)
				_teleportTickCurrent--;
			if (_walkTickCurrent > 0)
				_walkTickCurrent--;
			if (_walkTickCurrent == 0)
				ChangeWalkBehavior(_teleportTickCurrent == 0);

			_ticksIntensityPercentage = Math.Max((float)_intensity / (float)(_maxIntensity), 0);


			if (_lastTargetHitBy != null)
				_targetedObject = _lastTargetHitBy;

		}
		private void ChangeWalkBehavior(bool includeTeleport = false)
		{
			_walkBehavior = (ElementalCharacterWalkBehavior)(MainGame.Randomizer.Next(!includeTeleport || _targetedObject == null ? 3 : 5));
			if (_walkBehavior == ElementalCharacterWalkBehavior.TeleportTo)
				TeleportTo(_targetedObject.Center);
			if (_walkBehavior == ElementalCharacterWalkBehavior.TeleportAway)
			{

				var teleportLocations = _teleportLocations.OrderBy(v =>
				{
					Vector2 distance = _targetedObject.Center - v;
					return (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2));
				}).Take(2);

				TeleportTo(teleportLocations.ElementAt(MainGame.Randomizer.Next(teleportLocations.Count())));
			}
			_walkTickCurrent = MainGame.Randomizer.Next(150) + 30;
		}
		// At this point, we know we have a target.
		private bool CanAttack(AbilityManager abilityManager, PlayerAbilityInfo pai)
		{
			if (_universalTickCurrent[pai.Id] > 0)
				return false;

			//float minValue = float.MaxValue;
			//Element weakSpot = _targetedObject.ElementLevel.Aggregate(Element.Lightning, (agg, kv) => kv.Value < minValue ? kv.Key : agg);

			bool canAttack = MainGame.Randomizer.Next(5) == 0; // For now.
			if (canAttack)
				_universalTickCurrent[pai.Id] = MainGame.Randomizer.Next(50) + 95;
			return canAttack;
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

		public override void PlayTeleportAnimation()
		{
			_animator.PlayAnimation(_teleportAnimation);
		}

		public override void FinishTeleport()
		{
			ChangeWalkBehavior();
			_teleportTickCurrent = 500;
			_animator.PlayAnimation(_idleAnimation);
		}

		private void ChangeIntesity(int newIntensity)
		{
			_intensity = newIntensity;
			_ticksIntensityPercentage = (float)_intensity / (float)_maxIntensity;

			PlayerAbilityInfo lightningPai = _abilityManager.GetAbilityInfo(_lightningAttackId);
			lightningPai.Modifier.Damage = -20 - (180 * _ticksIntensityPercentage); // _inEmergency ? -320 : -280;
			float lightWidth = 50 + (30 * _ticksIntensityPercentage);
			lightningPai.Modifier.SetSize(new Vector2(lightWidth, lightWidth / 2.0f)); //_inEmergency ? 80 : 60));
			lightningPai.Modifier.Speed = 13f + (12 * _ticksIntensityPercentage); //10f;

			PlayerAbilityInfo firePai = _abilityManager.GetAbilityInfo(_fireAttackId);
			firePai.Modifier.Damage = -50 - (250 * _ticksIntensityPercentage); // _inEmergency ? -320 : -280;
			firePai.Modifier.SetSize(new Vector2(30 + (25 * _ticksIntensityPercentage))); //_inEmergency ? 80 : 60));
			firePai.Modifier.Speed = 8f + (5 * _ticksIntensityPercentage); //10f;
		}

		public override void PreDraw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (_objState == GuiObjectState.Normal)
			{
				if (IsStunned || CurrentMovement.X == 0)
					_animator.PlayAnimation(_idleAnimation);
				else
					_animator.PlayAnimation(_runAnimation);
			}
			base.PreDraw(gameTime, spriteBatch);
		}

		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			return CurrentMovement.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		}

		#region Map Editor
		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Intensity";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _intensity.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				ChangeIntesity(MathHelper.Clamp(_intensity + (moveRight ? 1 : -1), 0, _maxIntensity - _maxIntensityChange));
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _intensity;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				ChangeIntesity(value);
			base.SetSpecialValue(bType, value);
		}
		#endregion
	}
}
