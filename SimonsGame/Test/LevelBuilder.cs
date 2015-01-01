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
		public static Level BuildLevel3(IServiceProvider serviceProvider, Vector2 levelSize, GameStateManager gameStateManager)
		{
			int baseHeight = 160;

			Level level = new Level(serviceProvider, levelSize + new Vector2(0, 200), gameStateManager);
			level.PlatformDifference = baseHeight; // for now.

			int notFirstOne = 0;
			int playerWidth = 50;
			MainGame.PlayerManager.PlayerInputMap.ToList().ForEach(kv =>
			{
				Guid guid = kv.Key;
				level.AddPlayer(new Player(guid, new Vector2(50, levelSize.Y - 50 - (playerWidth * 2)), new Vector2(playerWidth, playerWidth * 2), Group.BothPassable, level, "Player " + notFirstOne, notFirstOne++ > 0));
			});

			//level.AddGuiObject(new WallRunner(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 100), new Vector2(50, 50), Group.BothPassable, level, true));
			//level.AddGuiObject(new WallRunner(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 100), new Vector2(50, 50), Group.BothPassable, level, false));
			//level.AddGuiObject(new HealthCreep(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 100), new Vector2(36, 20), Group.BothPassable, level, false, 200, (int)(level.Size.X - 200)));
			//level.AddGuiObject(new HealthCreep(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 100), new Vector2(36, 20), Group.BothPassable, level, true, 200, (int)(level.Size.X - 200)));

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
			int widthOfPlat = (int)((levelSize.X - 100) / 5f);

			for (var currentY = levelSize.Y - (50 + baseHeight); currentY > baseHeight; currentY = currentY - baseHeight)
			{
				if (isEven)
				{
					Platform leftSidePlatform = new Platform(new Vector2(50, currentY), new Vector2(widthOfPlat, 20), Group.BothPassable, level);
					level.AddGuiObject(leftSidePlatform);
					MovingCharacter p = new MovingCharacter(new Vector2(50, currentY - 90), new Vector2(40, 80), Group.BothPassable, level, true);
					level.AddGuiObject(p);

					Platform middleSidePlatform = new Platform(new Vector2(50 + 2 * widthOfPlat, currentY), new Vector2(widthOfPlat, 20), Group.BothPassable, level);
					level.AddGuiObject(middleSidePlatform);
					MovingCharacter p2 = new MovingCharacter(new Vector2(50 + 2 * widthOfPlat, currentY - 90), new Vector2(40, 80), Group.BothPassable, level, true);
					level.AddGuiObject(p2);

					Platform rightSidePlatform = new Platform(new Vector2(50 + 4 * widthOfPlat, currentY), new Vector2(widthOfPlat + 20, 20), Group.BothPassable, level);
					level.AddGuiObject(rightSidePlatform);
					MovingCharacter p3 = new MovingCharacter(new Vector2(50 + 4 * widthOfPlat, currentY - 90), new Vector2(40, 80), Group.BothPassable, level, true);
					level.AddGuiObject(p3);
				}
				else
				{
					level.Players.Last().Value.Position = new Vector2(levelSize.X / 2 - 20, currentY - 80);

					Platform middlePlatform1 = new Platform(new Vector2(50 + 1 * widthOfPlat, currentY), new Vector2(widthOfPlat, 20), Group.Impassable, level);
					level.AddGuiObject(middlePlatform1);
					WallRunner p = new WallRunner(new Vector2(50 + 1 * widthOfPlat, currentY - 50), new Vector2(50, 50), Group.BothPassable, level, false);
					level.AddGuiObject(p);

					Platform middlePlatform2 = new Platform(new Vector2(50 + 3 * widthOfPlat, currentY), new Vector2(widthOfPlat, 20), Group.Impassable, level);
					level.AddGuiObject(middlePlatform2);
					WallRunner p2 = new WallRunner(new Vector2(50 + 3 * widthOfPlat, currentY - 50), new Vector2(50, 50), Group.BothPassable, level, false);
					level.AddGuiObject(p2);
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
			//level.Players = level.Players.Where(p => !p.Value.IsAi).ToDictionary(kv => kv.Key, kv => kv.Value);
			return level;
		}
		public static Level BuildLevel2(IServiceProvider serviceProvider, Vector2 levelSize, GameStateManager gameStateManager)
		{
			int baseHeight = 110;

			Level level = new Level(serviceProvider, levelSize, gameStateManager);
			level.PlatformDifference = baseHeight; // for now.

			int notFirstOne = 0;

			MainGame.PlayerManager.PlayerInputMap.ToList().ForEach(kv =>
			{
				Guid guid = kv.Key;
				level.AddPlayer(new Player(guid, new Vector2(50, levelSize.Y - 150), new Vector2(40, 80), Group.BothPassable, level, "Player " + notFirstOne, notFirstOne++ > 0));
			});

			level.AddGuiObject(new WallRunner(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 100), new Vector2(50, 50), Group.BothPassable, level, true));
			level.AddGuiObject(new WallRunner(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 100), new Vector2(50, 50), Group.BothPassable, level, false));
			level.AddGuiObject(new HealthCreep(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 100), new Vector2(36, 20), Group.BothPassable, level, false, 200, (int)(level.Size.X - 200)));
			level.AddGuiObject(new HealthCreep(new Vector2(levelSize.X / 2 - 25, levelSize.Y - 100), new Vector2(36, 20), Group.BothPassable, level, true, 200, (int)(level.Size.X - 200)));

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

			for (var currentY = levelSize.Y - (50 + baseHeight); currentY > baseHeight; currentY = currentY - baseHeight)
			{
				if (isEven)
				{
					Platform leftSidePlatform = new Platform(new Vector2(50, currentY), new Vector2(500, 20), Group.BothPassable, level);
					level.AddGuiObject(leftSidePlatform);
					MovingCharacter p = new MovingCharacter(new Vector2(50, currentY - 90), new Vector2(40, 80), Group.BothPassable, level, true);
					level.AddGuiObject(p);

					Platform rightSidePlatform = new Platform(new Vector2(levelSize.X - 550, currentY), new Vector2(500, 20), Group.BothPassable, level);
					level.AddGuiObject(rightSidePlatform);
					MovingCharacter p2 = new MovingCharacter(new Vector2(levelSize.X - 550, currentY - 90), new Vector2(40, 80), Group.BothPassable, level, true);
					level.AddGuiObject(p2);
				}
				else
				{
					WallRunner p = new WallRunner(new Vector2(levelSize.X / 2 - 25, currentY - 50), new Vector2(50, 50), Group.BothPassable, level, false);
					level.AddGuiObject(p);
					level.Players.Last().Value.Position = new Vector2(levelSize.X / 2 - 20, currentY - 80);

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
			//level.Players = level.Players.Where(p => !p.Value.IsAi).ToDictionary(kv => kv.Key, kv => kv.Value);
			return level;
		}
		public static Level BuildLevel1(IServiceProvider serviceProvider, Vector2 levelSize, GameStateManager gameStateManager)
		{
			Level level = new Level(serviceProvider, levelSize, gameStateManager);

			int baseHeight = 60;

			int notFirstOne = 0;
			MainGame.PlayerManager.PlayerInputMap.ToList().ForEach(kv =>
			{
				Guid guid = kv.Key;
				level.AddPlayer(new Player(guid, new Vector2(levelSize.X / 2 - 25, 100), new Vector2(50, 100), Group.BothPassable, level, "Player " + notFirstOne, notFirstOne++ > 0));
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
