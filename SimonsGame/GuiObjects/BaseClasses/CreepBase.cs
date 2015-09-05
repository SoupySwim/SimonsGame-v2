using Microsoft.Xna.Framework;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public abstract class CreepBase : PhysicsObject
	{
		public CreepBase(Vector2 position, Vector2 hitbox, Group group, Level level, string name)
			: base(position, hitbox, group, level, name) { }

		protected override void Died()
		{
			try
			{
				var otherPlayers = Level.Players.Where(p => p.Value.Team == _lastTargetHitBy.Team);
				if (otherPlayers.Any())
				{
					float experience = _abilityManager.Experience / otherPlayers.Count();
					foreach (var player in otherPlayers.Select(t => t.Value))
						player.GainExperience(experience);
				}

			}
			catch { }
			//Player player = _lastTargetHitBy as Player;
			//if (player != null)
			//	player.GainExperience(_abilityManager.Experience);
			base.Died();
		}
	}
}
