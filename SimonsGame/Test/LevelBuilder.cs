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
		public static Level BuildLevel2(IServiceProvider serviceProvider, Vector2 levelSize, GameStateManager gameStateManager)
		{
			int baseHeight = 110;

			Level level = new Level(serviceProvider, levelSize, gameStateManager);
			level.PlatformDifference = baseHeight; // for now.


			MainGame.PlayerManager.PlayerInputMap.ToList().ForEach(kv =>
			{
				Guid guid = kv.Key;
				//level.AddPlayer(new Player(guid, new Vector2(50, levelSize.Y - 150), new Vector2(50, 70), Group.BothPassable, level));
			});

			Platform bottomPlatform = new Platform(new Vector2(0, levelSize.Y - 50), new Vector2(levelSize.X, 50), Group.ImpassableIncludingMagic, level);
			level.AddGuiObject(bottomPlatform);
			Platform leftBarrior = new Platform(new Vector2(0, 0), new Vector2(50, levelSize.Y), Group.ImpassableIncludingMagic, level);
			level.AddGuiObject(leftBarrior);
			Platform rightBarrior = new Platform(new Vector2(levelSize.X - 50, 0), new Vector2(50, levelSize.Y), Group.ImpassableIncludingMagic, level);
			level.AddGuiObject(rightBarrior);
			Platform topPlatform = new Platform(new Vector2(0, 0), new Vector2(levelSize.X, 50), Group.ImpassableIncludingMagic, level);
			level.AddGuiObject(topPlatform);
			//Platform middleBarrior = new Platform(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 300), new Vector2(50, 300), Group.ImpassableIncludingMagic, level);
			//level.AddGuiObject(middleBarrior);
			bool isEven = false;
			bool testOne = true;
			for (var currentY = levelSize.Y - (50 + baseHeight); currentY > baseHeight; currentY = currentY - baseHeight)
			{
				if (isEven)
				{
					Platform leftSidePlatform = new Platform(new Vector2(50, currentY), new Vector2(500, 20), Group.BothPassable, level);
					level.AddGuiObject(leftSidePlatform);
					MovingCharacter p = new MovingCharacter(new Vector2(50, currentY - 90), new Vector2(50, 90), Group.BothPassable, level, true);
					//level.AddGuiObject(p);

					Platform rightSidePlatform = new Platform(new Vector2(levelSize.X - 550, currentY), new Vector2(500, 20), Group.BothPassable, level);
					level.AddGuiObject(rightSidePlatform);
					MovingCharacter p2 = new MovingCharacter(new Vector2(levelSize.X - 550, currentY - 90), new Vector2(50, 90), Group.BothPassable, level, true);
					//level.AddGuiObject(p2);
				}
				else
				{
					if (testOne)
					{
						WallRunner p = new WallRunner(new Vector2(levelSize.X / 2 - 25, currentY + 50), new Vector2(50, 50), Group.BothPassable, level, false);
						level.AddGuiObject(p);
						testOne = false;
					}
					Platform middlePlatform = new Platform(new Vector2(levelSize.X / 2 - 250, currentY), new Vector2(500, 20), Group.Impassable, level);
					level.AddGuiObject(middlePlatform);
				}
				isEven = !isEven;
			}
			//MovingPlatform leftMovingPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 - 75, 300), new Vector2(150, 50), Group.VerticalPassable, level, true, 650, true);
			//level.AddGuiObject(leftMovingPlatform);
			//MovingPlatform rightMovingPlatform = new MovingPlatform(new Vector2(5 * levelSize.X / 6 - 75, 300), new Vector2(150, 50), Group.VerticalPassable, level, true, 650, true);
			//level.AddGuiObject(rightMovingPlatform);

			//MovingPlatform horizontalBottomPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 + 75, 900), new Vector2(150, 50), Group.VerticalPassable, level, false, (int)(2 * levelSize.X / 3 - 300), true);
			//level.AddGuiObject(horizontalBottomPlatform);

			//int topPlatformMoving = (int)((levelSize.X / 2) - (levelSize.X / 6 + 275));// (int)(levelSize.X / 6 + 150);
			//MovingPlatform horizontalLeftPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 + 75, 400), new Vector2(150, 50), Group.VerticalPassable, level, false, topPlatformMoving, true);
			//level.AddGuiObject(horizontalLeftPlatform);
			//MovingPlatform horizontalRightPlatform = new MovingPlatform(new Vector2(levelSize.X / 2 + 50 + topPlatformMoving, 400), new Vector2(150, 50), Group.VerticalPassable, level, false, topPlatformMoving, false);
			//level.AddGuiObject(horizontalRightPlatform);


			return level;
		}
		public static Level BuildLevel1(IServiceProvider serviceProvider, Vector2 levelSize, GameStateManager gameStateManager)
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

			MovingPlatform leftMovingPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 - 75, 300), new Vector2(150, 50), Group.BothPassable, level, true, 650, true);
			level.AddGuiObject(leftMovingPlatform);
			MovingPlatform rightMovingPlatform = new MovingPlatform(new Vector2(5 * levelSize.X / 6 - 75, 300), new Vector2(150, 50), Group.BothPassable, level, true, 650, true);
			level.AddGuiObject(rightMovingPlatform);

			MovingPlatform horizontalBottomPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 + 75, 900), new Vector2(150, 50), Group.BothPassable, level, false, (int)(2 * levelSize.X / 3 - 300), true);
			level.AddGuiObject(horizontalBottomPlatform);

			int topPlatformMoving = (int)((levelSize.X / 2) - (levelSize.X / 6 + 275));// (int)(levelSize.X / 6 + 150);
			MovingPlatform horizontalLeftPlatform = new MovingPlatform(new Vector2(levelSize.X / 6 + 75, 400), new Vector2(150, 50), Group.BothPassable, level, false, topPlatformMoving, true);
			level.AddGuiObject(horizontalLeftPlatform);
			MovingPlatform horizontalRightPlatform = new MovingPlatform(new Vector2(levelSize.X / 2 + 50 + topPlatformMoving, 400), new Vector2(150, 50), Group.BothPassable, level, false, topPlatformMoving, false);
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
