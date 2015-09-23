using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Story;
using SimonsGame.GuiObjects;
using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects.Zones;

namespace SimonsGame.Test
{
	public class TempStory
	{
		public static StoryBoard GetTempStoryBoard(Level level)
		{
			var allChars = level.GetAllGuiObjects();
			Player player = level.Players.Values.First();
			Player player2 = level.Players.Values.Skip(1).First();
			Player player3 = level.Players.Values.Skip(2).First();
			var movingEnemies = allChars.Where(mgo => mgo is MinionNormal);
			var allStoryZones = level.GetAllZones().Where(z => z.Value is StoryZone);
			StoryZone openDoorZone = allStoryZones.First().Value as StoryZone;
			StoryZone trapDoorZone = allStoryZones.Skip(1).First().Value as StoryZone;
			StoryZone holeZone = allStoryZones.Skip(2).First().Value as StoryZone;
			StoryZone bossZone = allStoryZones.Skip(3).First().Value as StoryZone;

			MainGuiObject newAbility = allChars.First(mgo => mgo is AbilityObject);

			MainGuiObject flyingMinion = allChars.First(mgo => mgo is MinionFlying);//level.Players.Values.Skip(2).First();

			PhysicsObject boss = allChars.First(mgo => mgo is CreepBoss) as PhysicsObject;//level.Players.Values.Skip(2).First();

			StoryBoard storyBoard = new StoryBoard(level);


			// Phase 1 - talk to dude
			StoryBoardPhaseAction action1_1 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			action1_1.AddDialogue(new Dialogue.TextOverhead("It's dangerous to go alone. Take this", player2, Dialogue.TextOverheadBehavior.Proximity, false));

			StoryBoardPhase phase1 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			phase1.SetIntersectionBounds(player2.HitBoxBounds + new Vector4(-player2.Bounds.W, 0, 0, 0));

			phase1.AddAction(action1_1);


			// Phase 2 - show ability and pick it up.
			StoryBoardPhaseAction action2_1 = new StoryBoardPhaseAction(newAbility, StoryBoardAction.AddToLevel);
			StoryBoardPhaseAction action2_2 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			action2_2.AddDialogue(new Dialogue.TextOverhead("It's dangerous to go alone. Take this", player2, Dialogue.TextOverheadBehavior.Proximity, false));

			StoryBoardPhase phase2 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			phase2.SetIntersectionBounds(newAbility.HitBoxBounds);

			phase2.AddAction(action2_1);
			phase2.AddAction(action2_2);


			// Phase 3 - get to new spot.
			StoryBoardPhaseAction action3_1 = new StoryBoardPhaseAction(player, StoryBoardAction.PlaceHolder);
			StoryBoardPhase phase3 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			phase3.SetIntersectionBounds(openDoorZone.Bounds);
			phase3.AddAction(action3_1);


			// Phase 4 - open door and show enemy.
			StoryBoardPhaseAction action4_1 = new StoryBoardPhaseAction(flyingMinion, StoryBoardAction.AddToLevel);

			StoryBoardPhase phase4 = new StoryBoardPhase(level, StoryBoardEnd.Duration, StoryBoardPhaseBlock.None);
			phase4.SetDuration(1);

			phase4.AddAction(action4_1);


			// Phase 5 - Get to new player.
			StoryBoardPhaseAction action5_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			player3.Group = Utility.Group.Passable;
			action5_1.AddDialogue(new Dialogue.TextOverhead("You found me!  Come here.", player3, Dialogue.TextOverheadBehavior.Proximity, false));
			StoryBoardPhase phase5 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			phase5.SetIntersectionBounds(player3.HitBoxBounds + new Vector4(-player3.Bounds.W, 0, 0, 0));
			phase5.AddAction(action5_1);


			//trapDoorZone


			// Phase 6 - Dialogue with the hidden dude.
			StoryBoardPhaseAction action6_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action6_1.AddDialogue(new Dialogue.TextOverhead("How'd you find me?", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action6_2 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action6_2.AddDialogue(new Dialogue.TextOverhead("Are you stuck?", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action6_3 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action6_3.AddDialogue(new Dialogue.TextOverhead("...", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action6_4 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action6_4.AddDialogue(new Dialogue.TextOverhead("Here, follow me.", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase6 = new StoryBoardPhase(level, StoryBoardEnd.PlayerCyclesThroughActions, StoryBoardPhaseBlock.AllCharacters);
			phase6.AddAction(action6_1);
			phase6.AddAction(action6_2);
			phase6.AddAction(action6_3);
			phase6.AddAction(action6_4);


			// Phase 7 - Player3 walks to trap door.
			StoryBoardPhaseAction action7_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.MoveLeft);
			action7_1.AddDialogue(new Dialogue.TextOverhead("Check this out", player3, Dialogue.TextOverheadBehavior.Proximity, false));
			StoryBoardPhase phase7 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.AllCharacters);
			phase7.SetIntersectionBounds(trapDoorZone.Bounds);
			phase7.AddAction(action7_1);


			// Phase 6 - Dialogue with the hidden dude opening door.
			StoryBoardPhaseAction action8_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action8_1.AddDialogue(new Dialogue.TextOverhead("A little of this.", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action8_2 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action8_2.AddDialogue(new Dialogue.TextOverhead("A little of that...", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action8_3 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action8_3.AddDialogue(new Dialogue.TextOverhead("...", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action8_4 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action8_4.AddDialogue(new Dialogue.TextOverhead("And There we go!", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase8 = new StoryBoardPhase(level, StoryBoardEnd.PlayerCyclesThroughActions, StoryBoardPhaseBlock.AllCharacters);
			phase8.AddAction(action8_1);
			phase8.AddAction(action8_2);
			phase8.AddAction(action8_3);
			phase8.AddAction(action8_4);


			// Phase 9 - Player3 walks through trap door.
			StoryBoardPhaseAction action9_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.MoveLeft);
			StoryBoardPhaseAction action9_2 = new StoryBoardPhaseAction(player, StoryBoardAction.ActivateFunction);
			action9_2.AddFunctionIndex(trapDoorZone.StoryPoint);
			StoryBoardPhaseAction action9_3 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action9_3.AddDialogue(new Dialogue.TextOverhead("WOO HOO!!!", player3, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase9 = new StoryBoardPhase(level, StoryBoardEnd.Duration, StoryBoardPhaseBlock.None);
			phase9.SetDuration(100);

			phase9.AddAction(action9_1);
			phase9.AddAction(action9_2);
			phase9.AddAction(action9_3);


			// Phase 10 - Remove Player3.
			StoryBoardPhaseAction action10_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.Remove);

			StoryBoardPhase phase10 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			phase10.SetIntersectionBounds(holeZone.Bounds);

			phase10.AddAction(action10_1);



			// Phase 11 - Add enemies.

			StoryBoardPhase phase11 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			phase11.SetIntersectionBounds(bossZone.Bounds);

			foreach (var movingEnemy in movingEnemies)
			{
				StoryBoardPhaseAction action11_temp = new StoryBoardPhaseAction(movingEnemy, StoryBoardAction.AddToLevel);
				phase11.AddAction(action11_temp);
			}


			// Phase 12 - Boss says his bit.
			StoryBoardPhaseAction action12_1 = new StoryBoardPhaseAction(boss, StoryBoardAction.Talk);
			action12_1.AddDialogue(new Dialogue.TextOverhead("You dare come in my...", boss, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action12_2 = new StoryBoardPhaseAction(boss, StoryBoardAction.Talk);
			action12_2.AddDialogue(new Dialogue.TextOverhead("...", boss, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action12_3 = new StoryBoardPhaseAction(boss, StoryBoardAction.Talk);
			action12_3.AddDialogue(new Dialogue.TextOverhead("Room?!", boss, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhaseAction action12_4 = new StoryBoardPhaseAction(boss, StoryBoardAction.Talk);
			action12_4.AddDialogue(new Dialogue.TextOverhead("I WILL DESTROY YOU!", boss, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase12 = new StoryBoardPhase(level, StoryBoardEnd.PlayerCyclesThroughActions, StoryBoardPhaseBlock.AllCharacters);
			phase12.AddAction(action12_1);
			phase12.AddAction(action12_2);
			phase12.AddAction(action12_3);
			phase12.AddAction(action12_4);

			storyBoard.AddPhase(phase1);
			storyBoard.AddPhase(phase2);
			storyBoard.AddPhase(phase3);
			storyBoard.AddPhase(phase4);
			storyBoard.AddPhase(phase5);
			storyBoard.AddPhase(phase6);
			storyBoard.AddPhase(phase7);
			storyBoard.AddPhase(phase8);
			storyBoard.AddPhase(phase9);
			storyBoard.AddPhase(phase10);
			storyBoard.AddPhase(phase11);
			storyBoard.AddPhase(phase12);

			return storyBoard;
		}
		public static StoryBoard GetTempStoryBoardAmbush(Level level)
		{
			var allChars = level.GetAllGuiObjects();
			StoryBoard storyBoard = new StoryBoard(level);
			Player player = level.Players.Values.First();
			Player player2 = level.Players.Values.Skip(1).First();
			MainGuiObject badGuy = allChars.First(mgo => mgo is ElementalCharacter);//level.Players.Values.Skip(2).First();
			MainGuiObject newAbility = allChars.First(mgo => mgo is AbilityObject);

			// Phase 1
			StoryBoardPhaseAction action1_1 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);

			action1_1.AddDialogue(new Dialogue.TextOverhead("Stand in the far corner.", player2, Dialogue.TextOverheadBehavior.Proximity, false));

			StoryBoardPhase phase1 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			Vector4 phase1Bounds = player.Bounds;
			phase1Bounds.X -= (player.Bounds.W + 10);
			phase1.SetIntersectionBounds(phase1Bounds);

			phase1.AddAction(action1_1);


			// Phase 2
			StoryBoardPhaseAction action2_1 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);

			action2_1.AddDialogue(new Dialogue.TextOverhead("Good, now come here.", player2, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase2 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			Vector4 phase2Bounds = player2.Bounds;
			phase2Bounds.X -= (player2.Bounds.W + 10);
			phase2.SetIntersectionBounds(phase2Bounds);

			phase2.AddAction(action2_1);


			// Phase 3
			StoryBoardPhaseAction action3_1 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);
			StoryBoardPhaseAction action3_2 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			StoryBoardPhaseAction action3_3 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);
			StoryBoardPhaseAction action3_4 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			StoryBoardPhaseAction action3_5 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);
			StoryBoardPhaseAction action3_6 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			StoryBoardPhaseAction action3_7 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);

			action3_1.AddDialogue(new Dialogue.TextOverhead("Can you fight?.", player2, Dialogue.TextOverheadBehavior.Always, false));
			action3_2.AddDialogue(new Dialogue.TextOverhead("Yessir.", player, Dialogue.TextOverheadBehavior.Always, false));
			action3_3.AddDialogue(new Dialogue.TextOverhead("We will be ambushed any minute.", player2, Dialogue.TextOverheadBehavior.Always, false));
			action3_4.AddDialogue(new Dialogue.TextOverhead("What can I do?", player, Dialogue.TextOverheadBehavior.Always, false));
			action3_5.AddDialogue(new Dialogue.TextOverhead("Prepare your magic.", player2, Dialogue.TextOverheadBehavior.Always, false));
			action3_6.AddDialogue(new Dialogue.TextOverhead("I need help with that.", player, Dialogue.TextOverheadBehavior.Always, false));
			action3_7.AddDialogue(new Dialogue.TextOverhead("Here, I have just the thing", player2, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase3 = new StoryBoardPhase(level, StoryBoardEnd.PlayerCyclesThroughActions, StoryBoardPhaseBlock.AllCharacters);
			phase3.AddAction(action3_1);
			phase3.AddAction(action3_2);
			phase3.AddAction(action3_3);
			phase3.AddAction(action3_4);
			phase3.AddAction(action3_5);
			phase3.AddAction(action3_6);
			phase3.AddAction(action3_7);


			// Phase 4
			StoryBoardPhaseAction action4_1 = new StoryBoardPhaseAction(player2, StoryBoardAction.MoveLeft);

			StoryBoardPhase phase4 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.AllCharacters);
			phase4.SetIntersectionBounds(phase1Bounds);
			phase4.AddAction(action4_1);

			// Phase 5
			StoryBoardPhaseAction action5_1 = new StoryBoardPhaseAction(newAbility, StoryBoardAction.AddToLevel);
			StoryBoardPhaseAction action5_2 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);
			action5_2.AddDialogue(new Dialogue.TextOverhead("Here, take this.", player2, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase5 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			phase5.SetIntersectionBounds(newAbility.HitBoxBounds);
			phase5.AddAction(action5_1);
			phase5.AddAction(action5_2);

			// Phase 6
			StoryBoardPhaseAction action6_1 = new StoryBoardPhaseAction(badGuy, StoryBoardAction.MoveLeft);
			StoryBoardPhaseAction action6_2 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);
			action6_2.AddDialogue(new Dialogue.TextOverhead("WATCH OUT!!", player2, Dialogue.TextOverheadBehavior.Always, false));
			StoryBoardPhaseAction action6_3 = new StoryBoardPhaseAction(player, StoryBoardAction.ActivateFunction);
			action6_3.AddFunctionIndex(1);

			StoryBoardPhase phase6 = new StoryBoardPhase(level, StoryBoardEnd.Duration, StoryBoardPhaseBlock.None);
			phase6.SetDuration(160);
			phase6.AddAction(action6_1);
			phase6.AddAction(action6_2);
			phase6.AddAction(action6_3);


			// Phase 7
			StoryBoardPhaseAction action7_1 = new StoryBoardPhaseAction(badGuy, StoryBoardAction.PlaceHolder);
			StoryBoardPhase phase7 = new StoryBoardPhase(level, StoryBoardEnd.HasBeenDestroyed, StoryBoardPhaseBlock.None);
			phase7.AddAction(action7_1);


			// Phase 8
			StoryBoardPhaseAction action8_1 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);
			action8_1.AddDialogue(new Dialogue.TextOverhead("Good Job, I think we are safe for now.", player2, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase8 = new StoryBoardPhase(level, StoryBoardEnd.Duration, StoryBoardPhaseBlock.None);
			phase8.SetDuration(10000);
			phase8.AddAction(action8_1);

			storyBoard.AddPhase(phase1);
			storyBoard.AddPhase(phase2);
			storyBoard.AddPhase(phase3);
			storyBoard.AddPhase(phase4);
			storyBoard.AddPhase(phase5);
			storyBoard.AddPhase(phase6);
			storyBoard.AddPhase(phase7);
			storyBoard.AddPhase(phase8);

			return storyBoard;
		}
		public static StoryBoard GetTempStoryBoard2(Level level)
		{
			StoryBoard storyBoard = new StoryBoard(level);
			Player player = level.Players.Values.First();
			Player player2 = level.Players.Values.Skip(1).First();
			Player player3 = level.Players.Values.Skip(2).First();

			// Phase 1
			StoryBoardPhaseAction action1_1 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			StoryBoardPhaseAction action1_2 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			StoryBoardPhaseAction action1_3 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			StoryBoardPhaseAction action1_4 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);

			action1_1.AddDialogue(new Dialogue.TextOverhead("Will you marry me?", player, Dialogue.TextOverheadBehavior.Always, false));
			action1_2.AddDialogue(new Dialogue.TextOverhead("I will!", player2, Dialogue.TextOverheadBehavior.Always, false));
			action1_3.AddDialogue(new Dialogue.TextOverhead("I love you!", player2, Dialogue.TextOverheadBehavior.Always, false));
			action1_4.AddDialogue(new Dialogue.TextOverhead("I love you too!", player, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase1 = new StoryBoardPhase(level, StoryBoardEnd.PlayerCyclesThroughActions, StoryBoardPhaseBlock.AllCharacters);
			phase1.AddAction(action1_1);
			phase1.AddAction(action1_2);
			phase1.AddAction(action1_3);
			phase1.AddAction(action1_4);

			// Phase 2
			StoryBoardPhaseAction action2_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.MoveLeft);

			StoryBoardPhase phase2 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.AllCharacters);
			Vector4 phase2Bounds = player2.Bounds;
			phase2Bounds.X += 200;
			phase2.SetIntersectionBounds(phase2Bounds);
			phase2.AddAction(action2_1);

			// Phase 3
			StoryBoardPhaseAction action3_1 = new StoryBoardPhaseAction(player, StoryBoardAction.ActivateFunction);
			action3_1.AddFunctionIndex(1);

			StoryBoardPhase phase3 = new StoryBoardPhase(level, StoryBoardEnd.Duration, StoryBoardPhaseBlock.AllCharacters);
			phase3.SetDuration(20);
			phase3.AddAction(action3_1);

			// Phase 4
			StoryBoardPhaseAction action4_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.UseButton);
			action4_1.AddButton(AvailableButtons.Action, player2);
			StoryBoardPhaseAction action4_2 = new StoryBoardPhaseAction(player, StoryBoardAction.Talk);
			action4_2.AddDialogue(new Dialogue.TextOverhead("*Gasp*", player, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase4 = new StoryBoardPhase(level, StoryBoardEnd.Duration, StoryBoardPhaseBlock.AllCharacters);
			phase4.SetDuration(30);
			phase4.AddAction(action4_1);
			phase4.AddAction(action4_2);

			// Phase 5
			StoryBoardPhaseAction action5_1 = new StoryBoardPhaseAction(player2, StoryBoardAction.Talk);
			action5_1.AddDialogue(new Dialogue.TextOverhead("HELP!!", player2, Dialogue.TextOverheadBehavior.Always, false));

			StoryBoardPhase phase5 = new StoryBoardPhase(level, StoryBoardEnd.EnterZone, StoryBoardPhaseBlock.None);
			Vector4 phase5Bounds = player2.Bounds;
			phase5Bounds.W += 100;
			phase5Bounds.Z += 200;
			phase5Bounds.Y -= 200;
			phase5.SetIntersectionBounds(phase5Bounds);
			phase5.AddAction(action5_1);

			// Phase 6
			StoryBoardPhaseAction action6_1 = new StoryBoardPhaseAction(player2, StoryBoardAction.Remove);
			StoryBoardPhaseAction action6_2 = new StoryBoardPhaseAction(player3, StoryBoardAction.Talk);
			action6_2.AddDialogue(new Dialogue.TextOverhead("HAHAHAHAHA!", player3, Dialogue.TextOverheadBehavior.Always, false));
			StoryBoardPhaseAction action6_3 = new StoryBoardPhaseAction(player3, StoryBoardAction.DeactivateFunction);
			action6_3.AddFunctionIndex(1);
			StoryBoardPhaseAction action6_4 = new StoryBoardPhaseAction(player3, StoryBoardAction.MoveRight);

			StoryBoardPhase phase6 = new StoryBoardPhase(level, StoryBoardEnd.Duration, StoryBoardPhaseBlock.None);
			phase6.SetDuration(120);
			phase6.AddAction(action6_1);
			phase6.AddAction(action6_2);
			phase6.AddAction(action6_3);
			phase6.AddAction(action6_4);

			// Phase 7
			StoryBoardPhaseAction action7_1 = new StoryBoardPhaseAction(player3, StoryBoardAction.Remove);
			StoryBoardPhaseAction action7_2 = new StoryBoardPhaseAction(player, StoryBoardAction.ActivateFunction);
			action7_2.AddFunctionIndex(1);

			StoryBoardPhase phase7 = new StoryBoardPhase(level, StoryBoardEnd.Duration, StoryBoardPhaseBlock.AllCharacters);
			phase7.SetDuration(1);
			phase7.AddAction(action7_1);
			phase7.AddAction(action7_2);


			storyBoard.AddPhase(phase1);
			storyBoard.AddPhase(phase2);
			storyBoard.AddPhase(phase3);
			storyBoard.AddPhase(phase4);
			storyBoard.AddPhase(phase5);
			storyBoard.AddPhase(phase6);
			storyBoard.AddPhase(phase7);
			return storyBoard;
		}
	}
}
