using Microsoft.Xna.Framework;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using SimonsGame.Modifiers;

namespace SimonsGame.GuiObjects
{
	public class Player : PhysicsObject
	{

		public Player(Guid guid, Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level)
		{
			_guid = guid;
			MaxSpeedBase = new Vector2(AverageSpeed.X, AverageSpeed.Y);

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			// Jumps.
			List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, 1f));

			abilities.Add(KnownAbility.Jump, jumpInfos);

			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			elementalInfos.Add(AbilityBuilder.GetLongRangeElementalAbility1(this));
			//elementalInfos.Add(AbilityBuilder.GetShortRangeMeleeElementalAbility1(this));
			elementalInfos.Add(AbilityBuilder.GetShortRangeProjectileElementalAbility1(this));
			elementalInfos.Add(AbilityBuilder.GetSurroundRangeElementalAbility1(this));

			abilities.Add(KnownAbility.Elemental, elementalInfos);

			// Miscellaneous Magic
			List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			_abilityManager = new AbilityManager(this, abilities);
		}
		public override float GetXMovement()
		{
			return Movement.X * MaxSpeed.X; // For now, no "physics"
		}
		public override float GetYMovement()
		{
			return StopGravity ? 0f : CurrentMovement.Y;
		}
		public override void PostPhysicsPreUpdate(GameTime gameTime)
		{
			VerticalPass = GameStateManager.AllControls[_guid].YMovement > .5;
		}
		// If there are player specific modifiers, I will add these.
		//public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		//public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PostUpdate(GameTime gameTime) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime)
		{
			Movement = new Vector2(GameStateManager.AllControls[_guid].XMovement, GameStateManager.AllControls[_guid].YMovement);
		}
	}
}
