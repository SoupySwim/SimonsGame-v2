using SimonsGame.GuiObjects.ElementalMagic;
using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Test
{
	public class LevelBuilder
	{
		public static Level BuildLevel(IServiceProvider serviceProvider, Vector2 levelSize, GameStateManager gameStateManager)
		{
			Level level = new Level(serviceProvider, levelSize, gameStateManager);

			int baseHeight = 60;

			MainGame.PlayerManager.PlayerInputMap.ToList().ForEach(kv =>
			{
				Guid guid = kv.Key;
				level.AddPlayer(new Player(guid, new Vector2(levelSize.X / 2 - 25, 100), new Vector2(50, 100), Group.BothPassable, level));
			});

			//Player player = new Player(new Vector2(levelSize.X / 2 - 25, 100), new Vector2(50, 100), Group.Impassable, level);
			//level.AddPlayer(player);

			Platform bottomPlatform = new Platform(new Vector2(0, levelSize.Y - 50), new Vector2(levelSize.X, 50), Group.ImpassableIncludingMagic, level);
			level.AddGuiObject(bottomPlatform);
			Platform midPlatform = new Platform(new Vector2(levelSize.X / 2 - 50, levelSize.Y / 2 - 25), new Vector2(100, 50), Group.Impassable, level);
			level.AddGuiObject(midPlatform);

			MovingPlatform leftMovingPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 - 75, 300), new Vector2(150, 50), Group.VerticalPassable, level, true, 650, true);
			level.AddGuiObject(leftMovingPlatform);
			MovingPlatform rightMovingPlatform = new MovingPlatform(new Vector2(5 * levelSize.X / 6 - 75, 300), new Vector2(150, 50), Group.VerticalPassable, level, true, 650, true);
			level.AddGuiObject(rightMovingPlatform);

			MovingPlatform horizontalBottomPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 + 75, 900), new Vector2(150, 50), Group.VerticalPassable, level, false, (int)(2 * levelSize.X / 3 - 300), true);
			level.AddGuiObject(horizontalBottomPlatform);

			int topPlatformMoving = (int)((levelSize.X / 2) - (levelSize.X / 6 + 275));// (int)(levelSize.X / 6 + 150);
			MovingPlatform horizontalLeftPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 + 75, 400), new Vector2(150, 50), Group.VerticalPassable, level, false, topPlatformMoving, true);
			level.AddGuiObject(horizontalLeftPlatform);
			MovingPlatform horizontalRightPlatform = new MovingPlatform(new Vector2(levelSize.X / 2 + 50 + topPlatformMoving, 400), new Vector2(150, 50), Group.VerticalPassable, level, false, topPlatformMoving, false);
			level.AddGuiObject(horizontalRightPlatform);


			Platform leftBarrior = new Platform(new Vector2(0, 0), new Vector2(50, levelSize.Y), Group.ImpassableIncludingMagic, level);
			level.AddGuiObject(leftBarrior);
			Platform rightBarrior = new Platform(new Vector2(levelSize.X - 50, 0), new Vector2(50, levelSize.Y), Group.ImpassableIncludingMagic, level);
			level.AddGuiObject(rightBarrior);
			Platform middleBarrior = new Platform(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 300), new Vector2(50, 300), Group.ImpassableIncludingMagic, level);
			level.AddGuiObject(middleBarrior);

			level.PlatformDifference = baseHeight; // for now.
			return level;
		}
	}
}
