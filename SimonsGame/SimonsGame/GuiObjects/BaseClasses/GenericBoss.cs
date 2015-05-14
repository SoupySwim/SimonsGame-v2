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
	public class GenericBoss : PhysicsObject
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
			//_defeatedModifier = new TickModifier(1, ModifyType.Add, this, Element.Normal);

			//_startingPosition = position;

			//_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/Mover"), 1, false, 80, 160, (Size.X / 80.0f));
			//_animator.Color = Color.LightGreen;
			//_animator.PlayAnimation(_idleAnimation);

			//Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			//// Jumps.
			//List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			//jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, 1.5f));
			//PlayerAbilityInfo jumpPai = jumpInfos.First(ei => ei.Name == "Jump");
			//SingleJump jump = jumpPai.Modifier as SingleJump;
			//jump.CheckStopped = () => jump.HasReachedEnd || ((MainGame.Randomizer.Next(30) <= 1) && (_lastTargetHitBy != null && _lastTargetHitBy.Center.Y > Center.Y));
			//jumpPai.IsUsable = (abilityManager) =>
			//{
			//	if (_lastTargetHitBy != null)
			//	{
			//		// If we already have a jump active, don't jump again.
			//		if (abilityManager.CurrentAbilities.ContainsKey(jumpPai.Id))
			//			return false;
			//		return ((MainGame.Randomizer.Next(40) <= 1) && (_lastTargetHitBy.Center.Y + 20) < Center.Y);
			//	}
			//	return false;
			//};

			//abilities.Add(KnownAbility.Jump, jumpInfos);
			//// Elemental Magic
			//List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			//PlayerAbilityInfo pai = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball");
			//pai.Name = "Ball";
			//pai.AbilityAttributes = AbilityAttributes.Explosion;
			//pai.Modifier.Damage = -25;
			//pai.Modifier.Speed = 10.5f;
			//pai.Cooldown = new TimeSpan(0, 0, 0, 0, 333);
			//pai.Modifier.SetSize(new Vector2(20, 20));
			//pai.IsUsable = (abilityManager) =>
			//{
			//	if (abilityManager.CurrentAbilities.ContainsKey(pai.Id))
			//		return false;

			//	return _lastTargetHitBy != null && _idleCounterCurrent >= 0;
			//	//return (AIState == MoveCharacterAIState.MoveRight ? -1 : 1) * (_previousPosition.X + GetXMovement()) < Position.X;
			//};
			//elementalInfos.Add(pai);


			//PlayerAbilityInfo paiMelee = AbilityBuilder.GetShortRangeMeleeElementalAbility1(this);
			//paiMelee.IsUsable = (abilityManager) =>
			//{
			//	if (abilityManager.CurrentAbilities.ContainsKey(paiMelee.Id))
			//		return false;

			//	return _lastTargetHitBy != null && Math.Abs(_lastTargetHitBy.Center.X - Center.X) < 60;
			//	//return (AIState == MoveCharacterAIState.MoveRight ? -1 : 1) * (_previousPosition.X + GetXMovement()) < Position.X;
			//};
			//paiMelee.Modifier.Damage = -30;
			//elementalInfos.Add(paiMelee);


			//abilities.Add(KnownAbility.Elemental, elementalInfos);

			//// Miscellaneous Magic
			//List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			//abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			//_abilityManager = new AbilityManager(this, abilities,
			//	AvailableButtons.RightTrigger | AvailableButtons.RightBumper | AvailableButtons.LeftTrigger | AvailableButtons.LeftBumper);

			//// Will Change when we level up stuff and things...
			//_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Melee"), AvailableButtons.RightTrigger);
			//_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Ball"), AvailableButtons.RightBumper);

		}
		public override float GetXMovement()
		{
			return 0;
		}
		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}

		protected override void Died()
		{
			base.Died();
		}

		public override void PreUpdate(GameTime gameTime)
		{
			base.PreUpdate(gameTime);
			// If we are waiting for a player, then we will search our zone for intruders!
			if (_bossBehavior == BossBehavior.WaitingForPlayer)
			{
				GenericZone zone = Level.GetAllZones().FirstOrDefault(z => ZoneIds.Contains(z.Id));
				if (zone != null)
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
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
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
