﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.Menu;
using SimonsGame.Menu.MenuScreens;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Test;

namespace SimonsGame.MapEditor
{
	public class MapEditorLeftPanel
	{
		private MapEditorEditMap _mapEditorEditMap;
		private Vector4 _leftPanelBounds;
		private GuiObjectClass _selectedObjectClass;
		public GuiObjectClass SelectedObjectClass { get { return _selectedObjectClass; } }
		private List<LeftPanelChoice> _menuItems;
		private List<LeftPanelChoice> _currentMenuItems;
		private SortType _currentSort = SortType.All;
		private Color _selectedColor = Color.Lerp(Color.Black, Color.TransparentBlack, .8f);

		private TextMenuItemButton _selectModeButton;
		private Dictionary<SortType, TextMenuItemButton> _filterButtons;

		private enum SortType
		{
			All,
			Environment,
			Character,
			Structure,
			PowerUp,
			Zones
		}
		private Dictionary<SortType, HashSet<GuiObjectClass>> _filterMap = new Dictionary<SortType, HashSet<GuiObjectClass>>()
		{
			{SortType.All, new HashSet<GuiObjectClass>()},
			{
				SortType.Environment, new HashSet<GuiObjectClass>()
				{
					GuiObjectClass.Platform,
					GuiObjectClass.MovingPlatform,
					GuiObjectClass.FinishLineFlagPole,
					GuiObjectClass.Block,
					GuiObjectClass.ObjectSpawner,
					GuiObjectClass.Spike,
				}
			},
			{
				SortType.Character, new HashSet<GuiObjectClass>()
				{
					GuiObjectClass.Player,
					GuiObjectClass.AIPlayer,
					GuiObjectClass.HealthCreep,
					GuiObjectClass.ElementalCharacter,
					GuiObjectClass.MinionNormal,
					GuiObjectClass.MinionFlying,
					GuiObjectClass.MinionLarge,
					GuiObjectClass.NeutralCreep,
					GuiObjectClass.FlyingCreature,
					GuiObjectClass.LargeCreep,
					GuiObjectClass.CreepBoss,
					GuiObjectClass.WallRunner
				}
			},
			{
				SortType.Structure, new HashSet<GuiObjectClass>()
				{
					GuiObjectClass.StandardTurret,
					GuiObjectClass.StandardBase,
					GuiObjectClass.Ladder,
					GuiObjectClass.Button,
					GuiObjectClass.JumpPad,
					GuiObjectClass.Teleporter,
					GuiObjectClass.LockedBarrier,
					GuiObjectClass.SmallKeyObject,
				}
			},
			{
				SortType.PowerUp, new HashSet<GuiObjectClass>()
				{
					GuiObjectClass.HealthPack,
					GuiObjectClass.SuperJump,
					GuiObjectClass.SuperSpeed,
					GuiObjectClass.AbilityObject,
				}
			},
			{
				SortType.Zones, new HashSet<GuiObjectClass>()
				{
					GuiObjectClass.GuiFunction,
					GuiObjectClass.GuiIfClause,
					GuiObjectClass.GuiThenClause,
					GuiObjectClass.JungleCreepZone,
					GuiObjectClass.BehaviorZone,
				}
			},
		};

		private int _skipAmount = 0;
		private int _selectedIndex = 0;
		private Texture2D _arrow;

		public MapEditorLeftPanel(MapEditorEditMap mapEditorEditMap, Vector4 leftPanelBounds, MapEditorEditMap manager)
		{
			_mapEditorEditMap = mapEditorEditMap;
			_leftPanelBounds = leftPanelBounds;
			_selectedObjectClass = GuiObjectClass.Platform;
			_menuItems = new List<LeftPanelChoice>();
			_menuItems.Add(new LeftPanelChoice("Test/Platform", GuiObjectClass.Platform, Color.SandyBrown, "Platform"));
			_menuItems.Add(new LeftPanelChoice("Test/Platform", GuiObjectClass.MovingPlatform, Color.White, "Moving Platform"));
			_menuItems.Add(new LeftPanelChoice("Test/PlayerStill", GuiObjectClass.Player, Color.Black, "Player"));
			_menuItems.Add(new LeftPanelChoice("Test/PlayerStillAI", GuiObjectClass.AIPlayer, new Color(50, 50, 50), "AI Player"));
			_menuItems.Add(new LeftPanelChoice("Test/HealthCreep", GuiObjectClass.HealthCreep, Color.Green, "Health Creep"));
			_menuItems.Add(new LeftPanelChoice("Test/PlayerStill", GuiObjectClass.ElementalCharacter, Color.Purple, "Elemental Character"));
			_menuItems.Add(new LeftPanelChoice("Test/Mover", GuiObjectClass.MinionNormal, Color.LightPink, "Minion Normal"));
			_menuItems.Add(new LeftPanelChoice("Test/MinionFlyingFrame", GuiObjectClass.MinionFlying, Color.LightPink, "Minion Flying"));
			_menuItems.Add(new LeftPanelChoice("Test/LargeCreep", GuiObjectClass.MinionLarge, Color.LightPink, "Minion Large"));
			_menuItems.Add(new LeftPanelChoice("Test/Mover", GuiObjectClass.NeutralCreep, Color.LightGreen, "Neutral Creep"));
			_menuItems.Add(new LeftPanelChoice("Test/BatIdle", GuiObjectClass.FlyingCreature, Color.Gray, "Flying Creature"));
			_menuItems.Add(new LeftPanelChoice("Test/LargeCreep", GuiObjectClass.LargeCreep, Color.White, "Large Creep"));
			_menuItems.Add(new LeftPanelChoice("Test/LargeCreep", GuiObjectClass.CreepBoss, Color.SlateGray, "Creep Boss"));
			_menuItems.Add(new LeftPanelChoice("Test/WallRunner", GuiObjectClass.WallRunner, Color.Red, "Wall Runner"));
			_menuItems.Add(new LeftPanelChoice("Test/Turret", GuiObjectClass.StandardTurret, GuiVariables.TeamColorMap[Team.Team1], "Turret")); // team 1 for now...
			_menuItems.Add(new LeftPanelChoice("Test/Base", GuiObjectClass.StandardBase, GuiVariables.TeamColorMap[Team.Team1], "Base")); // team 1 for now...
			_menuItems.Add(new LeftPanelChoice("Test/FlagPole", GuiObjectClass.FinishLineFlagPole, GuiVariables.TeamColorMap[Team.None], "Flag Pole"));
			_menuItems.Add(new LeftPanelChoice("Test/Block", GuiObjectClass.Block, Color.White, "Block"));
			_menuItems.Add(new LeftPanelChoice("Test/ObjectSpawnerFrame", GuiObjectClass.ObjectSpawner, GuiVariables.TeamColorMap[Team.None], "Object Spawner"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.Ladder, Color.Yellow, "Ladder"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.Button, Color.DarkRed, "Button"));
			_menuItems.Add(new LeftPanelChoice("Test/Hammer", GuiObjectClass.GuiFunction, Color.Black, "Function"));
			_menuItems.Add(new LeftPanelChoice("Test/IfClause", GuiObjectClass.GuiIfClause, Color.Black, "IfClause"));
			_menuItems.Add(new LeftPanelChoice("Test/ThenClause", GuiObjectClass.GuiThenClause, Color.Black, "ThenClause"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.JumpPad, Color.Green, "Jump pad"));
			_menuItems.Add(new LeftPanelChoice("Test/HealthPack", GuiObjectClass.HealthPack, Color.White, "Health Pack"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.SuperJump, Color.White, "Jump Boost"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.SuperSpeed, Color.White, "Speed Boost"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.Teleporter, Color.Orange, "Teleporter"));
			_menuItems.Add(new LeftPanelChoice("Test/Spikes", GuiObjectClass.Spike, Color.White, "Spike"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.LockedBarrier, Color.Gray, "Barrier"));
			_menuItems.Add(new LeftPanelChoice("Test/SmallKey", GuiObjectClass.SmallKeyObject, Color.Gray, "Key"));
			_menuItems.Add(new LeftPanelChoice("Test/NewAbility", GuiObjectClass.AbilityObject, Color.White, "New Ability"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.JungleCreepZone, Color.Lerp(Color.Purple, new Color(1, 1, 1, 0), .5f), "Jungle Zone"));
			_menuItems.Add(new LeftPanelChoice("Test/SingleColor", GuiObjectClass.BehaviorZone, Color.LightGray, "Behavior Zone"));


			_currentMenuItems = _menuItems.ToList();

			_arrow = MainGame.ContentManager.Load<Texture2D>("Test/Menu/RightArrow");
			Vector2 textSize = "Select Mode".GetTextSize(MainGame.PlainFont);
			float desiredWidth = _leftPanelBounds.W - 10;
			_selectModeButton = new TextMenuItemButton(() =>
			{
				manager.SwitchState(MapEditorState.Select);
			}, "Select Mode", new Vector4(_leftPanelBounds.X + 5 + ((desiredWidth - textSize.X) / 2), _leftPanelBounds.Y + 5 + ((80 - textSize.Y) / 2), textSize.Y, textSize.X), new Vector2(desiredWidth - textSize.X, 80 - textSize.Y), false);

			float desiredFilterWidth = desiredWidth / _filterMap.Count();

			int ndx = 0;
			_filterButtons = _filterMap.ToDictionary(kv => kv.Key, kv =>
				{
					Vector4 sortBounds = new Vector4(_leftPanelBounds.X + 5 + (ndx++ * desiredFilterWidth), _selectModeButton.TotalBounds.Y + _selectModeButton.TotalBounds.Z, 50, desiredFilterWidth);
					string sortText = kv.Key.ToString().Substring(0, 3);
					var tuple = sortText.GetSizeAndPadding(MainGame.PlainFont, sortBounds);
					return new TextMenuItemButton(() => ChangeFilter(kv.Key), sortText, tuple.Item1, tuple.Item2, false);
				});
		}

		private void ChangeFilter(SortType sortType)
		{
			if (sortType == SortType.All)
				_currentMenuItems = _menuItems.ToList();
			else
				_currentMenuItems = _menuItems.Where(mi => _filterMap[sortType].Contains(mi.Class)).ToList();
			_skipAmount = 0;
			_currentSort = sortType;
		}

		public void ScrollDown()
		{
			_skipAmount = Math.Min(_currentMenuItems.Count() - 5, _skipAmount + 1);
		}
		public void ScrollUp()
		{
			_skipAmount = Math.Max(0, _skipAmount - 1);
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			_selectModeButton.Draw(gameTime, spriteBatch);
			foreach (var kv in _filterButtons)
			{
				if (_currentSort == kv.Key)
					kv.Value.OverrideColor(Color.Orange);
				kv.Value.Draw(gameTime, spriteBatch);
			};
			int currentIndex = _skipAmount;
			int currentY = 150;
			foreach (LeftPanelChoice item in _currentMenuItems.Skip(_skipAmount))
			{
				if (currentIndex == _selectedIndex)
				{
					spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)_leftPanelBounds.X, currentY - 4, (int)(_leftPanelBounds.W - 2), 90), _selectedColor);
					spriteBatch.Draw(_arrow, new Rectangle((int)(_leftPanelBounds.X + 10), currentY + 25, 40, 40), Color.Orange);
				}
				// Center it and make sure it's 80 pixels tall
				float scale = 80.0f / item.Texture.Bounds.Height;
				int width = (int)(item.Texture.Bounds.Width * scale);
				int height = (int)(item.Texture.Bounds.Height * scale);
				//int x = (int)(_leftPanelBounds.W / 2.0 - width / 2.0);
				int x = (int)(_leftPanelBounds.X + 55);


				spriteBatch.Draw(item.Texture, new Rectangle(x, currentY, width, height), item.Color);

				Vector2 nameSize = item.Name.GetTextSize(MainGame.PlainFont);
				Vector2 namePosition = new Vector2(_leftPanelBounds.X + _leftPanelBounds.W - 10 - nameSize.X, currentY + ((90 - nameSize.Y) / 2.0f));
				spriteBatch.DrawString(MainGame.PlainFont, item.Name, namePosition, Color.Black);

				currentY += 90;
				currentIndex++;
			}
		}
		public void Update(Vector2 mousePosition)
		{
			if (mousePosition.IsInBounds(_selectModeButton.TotalBounds))
				_selectModeButton.HasBeenHighlighted();
			else
				_selectModeButton.HasBeenDeHighlighted();
			_filterButtons.Values.ToList().ForEach(button =>
			{
				if (mousePosition.IsInBounds(button.TotalBounds))
					button.HasBeenHighlighted();
				else
					button.HasBeenDeHighlighted();
			});

		}
		public MainGuiObject GetNewItem(Level level)
		{
			if (_mapEditorEditMap.SelectedItemToAdd != null && _mapEditorEditMap.SelectedItemToAdd.GetClass() == _selectedObjectClass)
			{
				MainGuiObject mgo = (MainGuiObject)_mapEditorEditMap.SelectedItemToAdd.Clone();
				if (_mapEditorEditMap.GetCurrentObjectOptions().HasFlag(ButtonType.Size))
					mgo.Size = Vector2.Zero;
				mgo.Position = Vector2.Zero;
				return mgo;
			}
			return level.GetNewItem(_selectedObjectClass);
		}

		public class LeftPanelChoice
		{
			public string Name { get; set; }
			public Texture2D Texture { get { return _texture; } }
			private Texture2D _texture;
			public Color Color { get; set; }
			public GuiObjectClass Class { get; set; }
			public LeftPanelChoice(string textureName, GuiObjectClass guiObjectClass, Color color, string name)
			{
				_texture = MainGame.ContentManager.Load<Texture2D>(textureName);
				Class = guiObjectClass;
				Color = color;
				Name = name;
			}
		}

		public MainGuiObject Click(Vector2 mousePosition, Level level)
		{
			if (mousePosition.IsInBounds(_selectModeButton.TotalBounds))
			{
				_selectModeButton.CallAction();
				return null;
			}
			_filterButtons.Values.ToList().ForEach(button =>
			{
				if (mousePosition.IsInBounds(button.TotalBounds))
					button.CallAction();
			});
			_selectedIndex = MathHelper.Clamp((int)(_skipAmount + ((mousePosition.Y - _leftPanelBounds.Y - 150) / 90)), 0, _currentMenuItems.Count() - 1);
			_selectedObjectClass = _currentMenuItems.ElementAt(_selectedIndex).Class;
			return GetNewItem(level);
		}
		public MainGuiObject CycleSelectedItem(Level level)
		{
			_selectedIndex = (_selectedIndex + 1) % _currentMenuItems.Count();
			_selectedObjectClass = _currentMenuItems.ElementAt(_selectedIndex).Class;
			return GetNewItem(level);
		}

		internal void SwitchState(MapEditorState mapEditorState)
		{
			if (mapEditorState != MapEditorState.AddNew)
				_selectedIndex = -1;
			else if (_selectedIndex < 0)
				_selectedIndex = 0;
		}
	}

	[Serializable]
	public enum GuiObjectClass
	{
		Platform = 0,
		MovingPlatform,
		Player,
		AIPlayer,
		HealthCreep,
		MinionNormal,
		WallRunner,
		StandardTurret,
		StandardBase,
		Level,
		FinishLineFlagPole,
		Block,
		ObjectSpawner,
		Ladder,
		JumpPad,
		HealthPack,
		SuperJump,
		SuperSpeed,
		Teleporter,
		Spike,
		LockedBarrier,
		SmallKeyObject,
		AbilityObject,
		JungleCreepZone,
		NeutralCreep,
		LargeCreep,
		FlyingCreature,
		BehaviorZone,
		CreepBoss,
		MinionLarge,
		MinionFlying,
		ElementalCharacter,
		Button,
		GuiFunction,
		GuiIfClause,
		GuiThenClause,
		Aggregate,
	}
}