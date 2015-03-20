using Microsoft.Xna.Framework;
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
		private List<LeftPanelChoice> _menuItems;
		private TextMenuItemButton _selectModeButton;

		private int _skipAmount = 0;
		private int _selectedIndex = 0;
		private Texture2D _arrow;

		public MapEditorLeftPanel(MapEditorEditMap mapEditorEditMap, Vector4 leftPanelBounds, MapEditorEditMap manager)
		{
			_mapEditorEditMap = mapEditorEditMap;
			_leftPanelBounds = leftPanelBounds;
			_selectedObjectClass = GuiObjectClass.Platform;
			_menuItems = new List<LeftPanelChoice>();
			_menuItems.Add(new LeftPanelChoice("Test/Platform", GuiObjectClass.Platform, Color.SandyBrown));
			_menuItems.Add(new LeftPanelChoice("Test/Platform", GuiObjectClass.MovingPlatform, Color.White));
			_menuItems.Add(new LeftPanelChoice("Test/PlayerStill", GuiObjectClass.Player, Color.Black));
			_menuItems.Add(new LeftPanelChoice("Test/PlayerStillAI", GuiObjectClass.AIPlayer, new Color(50, 50, 50)));
			_menuItems.Add(new LeftPanelChoice("Test/HealthCreep", GuiObjectClass.HealthCreep, Color.Green));
			_menuItems.Add(new LeftPanelChoice("Test/Mover", GuiObjectClass.MovingCharacter, Color.LightPink));
			_menuItems.Add(new LeftPanelChoice("Test/WallRunner", GuiObjectClass.WallRunner, Color.Red));
			_menuItems.Add(new LeftPanelChoice("Test/Turret", GuiObjectClass.StandardTurret, GuiVariables.TeamColorMap[Team.Team1])); // team 1 for now...
			_menuItems.Add(new LeftPanelChoice("Test/Base", GuiObjectClass.StandardBase, GuiVariables.TeamColorMap[Team.Team1])); // team 1 for now...
			_menuItems.Add(new LeftPanelChoice("Test/FlagPole", GuiObjectClass.FinishLineFlagPole, GuiVariables.TeamColorMap[Team.None])); // team 1 for now...
			_arrow = MainGame.ContentManager.Load<Texture2D>("Test/Menu/RightArrow");
			Vector2 textSize = "Select Mode".GetTextSize(MainGame.PlainFont);
			float desiredWidth = _leftPanelBounds.W - 10;
			_selectModeButton = new TextMenuItemButton(() =>
			{
				manager.SwitchState(MapEditorState.Select);
			}, "Select Mode", new Vector4(_leftPanelBounds.X + 5 + ((desiredWidth - textSize.X) / 2), _leftPanelBounds.Y + 5 + ((80 - textSize.Y) / 2), textSize.Y, textSize.X), new Vector2(desiredWidth - textSize.X, 80 - textSize.Y), false);
		}

		public void ScrollDown()
		{
			_skipAmount = Math.Min(_menuItems.Count() - 5, _skipAmount + 1);
		}
		public void ScrollUp()
		{
			_skipAmount = Math.Max(0, _skipAmount - 1);
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			_selectModeButton.Draw(gameTime, spriteBatch);

			int currentIndex = _skipAmount;
			int currentY = 100;
			foreach (LeftPanelChoice item in _menuItems.Skip(_skipAmount))
			{
				if (currentIndex == _selectedIndex)
					spriteBatch.Draw(_arrow, new Rectangle((int)(_leftPanelBounds.X + 10), currentY + 25, 40, 40), Color.Orange);
				// Center it and make sure it's 80 pixels tall
				float scale = 80.0f / item.Texture.Bounds.Height;
				int width = (int)(item.Texture.Bounds.Width * scale);
				int height = (int)(item.Texture.Bounds.Height * scale);
				int x = (int)(_leftPanelBounds.W / 2.0 - width / 2.0);
				spriteBatch.Draw(item.Texture, new Rectangle(x, currentY, width, height), item.Color);
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
		}
		public MainGuiObject GetNewItem(Level level)
		{
			switch (_selectedObjectClass)
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
				case GuiObjectClass.MovingCharacter:
					return new MovingCharacter(Vector2.Zero, new Vector2(40, 80), Group.BothPassable, level, true);
				case GuiObjectClass.WallRunner:
					return new WallRunner(Vector2.Zero, new Vector2(50, 50), Group.BothPassable, level, true);
				case GuiObjectClass.StandardTurret:
					return new StandardTurret(Vector2.Zero, new Vector2(0, 0), Group.Impassable, level, Team.Team1);
				case GuiObjectClass.StandardBase:
					return new StandardBase(Vector2.Zero, new Vector2(0, 0), Group.Impassable, level, Team.Team1);
				case GuiObjectClass.FinishLineFlagPole:
					return new FinishLineFlagPole(Vector2.Zero, new Vector2(0, 0), Group.Passable, level);
			}
			return null;
		}
		public class LeftPanelChoice
		{
			public Texture2D Texture { get { return _texture; } }
			private Texture2D _texture;
			public Color Color { get; set; }
			public GuiObjectClass Class { get; set; }
			public LeftPanelChoice(string textureName, GuiObjectClass guiObjectClass, Color color)
			{
				_texture = MainGame.ContentManager.Load<Texture2D>(textureName);
				Class = guiObjectClass;
				Color = color;
			}
		}

		public MainGuiObject Click(Vector2 mousePosition, Level level)
		{
			if (mousePosition.IsInBounds(_selectModeButton.TotalBounds))
			{
				_selectedIndex = -1;
				return null;
			}
			_selectedIndex = MathHelper.Clamp((int)(_skipAmount + ((mousePosition.Y - _leftPanelBounds.Y - 100) / 90)), 0, _menuItems.Count() - 1);
			_selectedObjectClass = _menuItems.ElementAt(_selectedIndex).Class;
			return GetNewItem(level);
		}
		public MainGuiObject CycleSelectedItem(Level level)
		{
			_selectedIndex = (_selectedIndex + 1) % _menuItems.Count();
			_selectedObjectClass = _menuItems.ElementAt(_selectedIndex).Class;
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
		MovingCharacter,
		WallRunner,
		StandardTurret,
		StandardBase,
		Level,
		FinishLineFlagPole
	}
}