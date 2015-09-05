using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.OtherCharacters.Global;
using SimonsGame.GuiObjects.Zones;
using SimonsGame.MapEditor;
using SimonsGame.Test;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Extensions
{
	public static class MainGuiObjectExtensions
	{
		public static GuiObjectClass GetClass(this MainGuiObject mgo)
		{
			if (mgo != null)
			{
				try
				{
					string className = mgo.GetType().Name;
					if (className == "PowerUp")
						className = ((PowerUp)mgo).PowerUpType.ToString();
					GuiObjectClass objectClass = (GuiObjectClass)Enum.Parse(typeof(GuiObjectClass), className);
					if (objectClass == GuiObjectClass.Player && ((Player)mgo).IsAi)
						return GuiObjectClass.AIPlayer;
					return objectClass;

				}
				catch (Exception) { }
			}
			return GuiObjectClass.Platform;
		}

		public static GuiObjectStore GetGuiObjectStore(this MainGuiObject mgo)
		{
			return new GuiObjectStore()
			{
				X = mgo.Bounds.X,
				Y = mgo.Bounds.Y,
				Width = mgo.Bounds.W,
				Height = mgo.Bounds.Z,
				Class = mgo.GetClass(),
				Group = mgo.Group,
				Team = mgo.Team,
				ExtraSavedInformation = ButtonConfiguration.GenericButtons.ToDictionary(bType => bType, bType => mgo.GetSpecialValue(bType)),
				MaxSpeedX = mgo.MaxSpeedBase.X,
				MaxSpeedY = mgo.MaxSpeedBase.Y,
				HealthTotal = (int)mgo.HealthTotal,
			};
		}

		public static MainGuiObject GetNewItem(this Level level, GuiObjectClass selectedObjectClass)
		{
			switch (selectedObjectClass)
			{
				case GuiObjectClass.Platform:
					return new Platform(Vector2.Zero, Vector2.Zero, Group.ImpassableIncludingMagic, level);
				case GuiObjectClass.MovingPlatform:
					return new MovingPlatform(Vector2.Zero, Vector2.Zero, Group.BothPassable, level, true, (int)level.PlatformDifference, false);
				case GuiObjectClass.Player:
					int playerCount = MainGame.PlayerManager.PlayerInputMap.Count(kv => !kv.Value.IsAi);
					Guid playerId = MainGame.PlayerManager.AddPlayer(TempControls.GetPlayerInput(playerCount));
					return new Player(playerId, Vector2.Zero, new Vector2(50, 100), Group.BothPassable, level, "Player " + MainGame.PlayerManager.PlayerInfoMap.Count(), Team.Team1, false);
				case GuiObjectClass.AIPlayer:
					Guid id = MainGame.PlayerManager.AddPlayer(new UsableInputMap() { IsAi = true });
					return new Player(id, Vector2.Zero, new Vector2(50, 100), Group.BothPassable, level, "Player " + MainGame.PlayerManager.PlayerInfoMap.Count(), Team.Neutral, true);
				case GuiObjectClass.HealthCreep:
					return new HealthCreep(Vector2.Zero, new Vector2(36, 20), Group.BothPassable, level, true, 0, (int)level.Size.X);
				case GuiObjectClass.ElementalCharacter:
					return new ElementalCharacter(Vector2.Zero, new Vector2(50, 100), Group.BothPassable, level);
				case GuiObjectClass.MinionNormal:
					return new MinionNormal(Vector2.Zero, new Vector2(40, 80), Group.BothPassable, level, true);
				case GuiObjectClass.MinionFlying:
					return new MinionFlying(Vector2.Zero, new Vector2(40, 40), Group.BothPassable, level, true);
				case GuiObjectClass.MinionLarge:
					return new MinionLarge(Vector2.Zero, new Vector2(105, 140), Group.BothPassable, level, true);
				case GuiObjectClass.NeutralCreep:
					return new NeutralCreep(Vector2.Zero, new Vector2(40, 80), Group.BothPassable, level);
				case GuiObjectClass.FlyingCreature:
					return new FlyingCreature(Vector2.Zero, new Vector2(40, 40), Group.BothPassable, level);
				case GuiObjectClass.LargeCreep:
					return new LargeCreep(Vector2.Zero, new Vector2(180, 240), Group.BothPassable, level);
				case GuiObjectClass.CreepBoss:
					return new CreepBoss(Vector2.Zero, new Vector2(210, 280), Group.BothPassable, level);
				case GuiObjectClass.WallRunner:
					return new WallRunner(Vector2.Zero, new Vector2(50, 50), level, true);
				case GuiObjectClass.StandardTurret:
					return new StandardTurret(Vector2.Zero, Vector2.Zero, level, Team.Team1);
				case GuiObjectClass.StandardBase:
					return new StandardBase(Vector2.Zero, Vector2.Zero, level, Team.Team1);
				case GuiObjectClass.FinishLineFlagPole:
					return new FinishLineFlagPole(Vector2.Zero, Vector2.Zero, Group.Passable, level);
				case GuiObjectClass.Block:
					return new Block(Vector2.Zero, Vector2.Zero, Group.Impassable, level);
				case GuiObjectClass.ObjectSpawner:
					return new ObjectSpawner(Vector2.Zero, Vector2.Zero, Group.Impassable, level);
				case GuiObjectClass.Ladder:
					return new Ladder(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.GuiFunction:
					return new GuiFunction(Vector2.Zero, level);
				case GuiObjectClass.GuiIfClause:
					return new GuiIfClause(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.GuiThenClause:
					return new GuiThenClause(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.Button:
					return new Button(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.JumpPad:
					return new JumpPad(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.HealthPack:
					return PowerUpBuilder.GetHealthPackPU(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.SuperSpeed:
					return PowerUpBuilder.GetSpeedUpPU(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.SuperJump:
					return PowerUpBuilder.GetSuperJumpPU(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.Teleporter:
					return new Teleporter(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.Spike:
					return new Spike(Vector2.Zero, Vector2.Zero, Group.ImpassableIncludingMagic, level);
				case GuiObjectClass.LockedBarrier:
					return new LockedBarrier(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.SmallKeyObject:
					return new SmallKeyObject(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.AbilityObject:
					return PowerUpBuilder.GetBlinkAbilityObject(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.JungleCreepZone:
					return new JungleCreepZone(Vector2.Zero, Vector2.Zero, level);
				case GuiObjectClass.BehaviorZone:
					return new BehaviorZone(Vector2.Zero, Vector2.Zero, level, Team.Team1);
			}
			return null;
		}
	}
}
