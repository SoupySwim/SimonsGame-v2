using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.Menu;
using SimonsGame.Menu.MenuScreens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.MainFiles;
using SimonsGame.Utility;

namespace SimonsGame.MapEditor
{
	public class MapEditorRightPanel
	{
		private MapEditorEditMap _mapEditorEditMap;
		private Vector4 _rightPanelBounds;
		public Vector2 Size { get { return _rightPanelBounds.GetSize(); } }
		public Vector4 Bounds { get { return _rightPanelBounds; } }
		private TextMenuItemButton _saveButton;
		private TextMenuItemButton _goBackButton;
		private TextMenuItemButton _testLevelButton;
		private Texture2D _arrow;
		private Texture2D _trashCan;

		private Dictionary<ButtonType, ButtonConfiguration> _buttons;

		public MainGuiObject SelectedObject { get { return _selectedObject; } }
		private MainGuiObject _selectedObject;
		private GuiObjectClass _selectedObjectClass;

		public MapEditorRightPanel(MapEditorEditMap mapEditorEditMap, Vector4 rightPanelBounds, Level level, MenuStateManager manager)
		{
			_mapEditorEditMap = mapEditorEditMap;
			_rightPanelBounds = rightPanelBounds;
			float topButtonWidths = (_rightPanelBounds.W - 15) / 2;

			//_leftPanelBounds.X + 5 + ((desiredWidth - textSize.X) / 2), _leftPanelBounds.Y + 5 + ((80 - textSize.Y) / 2)
			Vector2 textSize = "Save".GetTextSize(MainGame.PlainFont);
			_saveButton = new TextMenuItemButton(() =>
			{
				MapEditorIOManager.SerializeLevelToFile(level, mapEditorEditMap.LevelMetaData);
			}, "Save", new Vector4(_rightPanelBounds.X + 5 + ((topButtonWidths - textSize.X) / 2), _rightPanelBounds.Y + 5 + ((40 - textSize.Y) / 2), textSize.Y, textSize.X), new Vector2(topButtonWidths - textSize.X, 40 - textSize.Y), false);

			textSize = "Exit".GetTextSize(MainGame.PlainFont);
			_goBackButton = new TextMenuItemButton(() => { manager.NavigateToPreviousScreen();/*Probably want to check for save first.*/ }, "Exit",
				new Vector4(_rightPanelBounds.X + 10 + topButtonWidths + ((topButtonWidths - textSize.X) / 2), _rightPanelBounds.Y + 5 + ((40 - textSize.Y) / 2), textSize.Y, textSize.X),
				new Vector2(topButtonWidths - textSize.X, 40 - textSize.Y), false);

			textSize = "Test".GetTextSize(MainGame.PlainFont);
			float desiredWidth = _rightPanelBounds.W - 10;
			_testLevelButton = new TextMenuItemButton(() =>
			{
				LevelFileMetaData tempData = new LevelFileMetaData(_mapEditorEditMap.LevelMetaData);
				tempData.LevelRelativePath = "";
				tempData.LevelName = "TestMapEditor";
				MapEditorIOManager.SerializeLevelToFile(level, tempData);
				if (!manager.StartGame(new GameSettings()
				{
					AllowAIScreens = false,
					PauseStopsGame = true,
					MapName = "TestMapEditor",
					LevelFileMetaData = tempData
				}))
				{ }
			}, "Test", new Vector4(_rightPanelBounds.X + 5 + ((desiredWidth - textSize.X) / 2), _rightPanelBounds.Y + _rightPanelBounds.Z - 45 + (-textSize.Y / 2), textSize.Y, textSize.X), new Vector2(desiredWidth - textSize.X, 80 - textSize.Y), false);

			_arrow = MainGame.ContentManager.Load<Texture2D>("Test/Menu/RightArrow");
			_trashCan = MainGame.ContentManager.Load<Texture2D>("Test/TrashCan");

			MakeButtons();
		}
		public void SelectNewItem(MainGuiObject mgo)
		{
			if (mgo != null)
			{
				GuiObjectClass classType = (GuiObjectClass)Enum.Parse(typeof(GuiObjectClass), mgo.GetType().Name);
				SelectNewItem(mgo, classType);
			}
			else _selectedObject = null; // If you select nothingness, then you will be modifying the level.
		}
		private void SelectNewItem(MainGuiObject mgo, GuiObjectClass objClass)
		{
			_selectedObject = mgo;
			_selectedObjectClass = objClass;
		}
		public void DeselectItem()
		{
			_selectedObject = null;
		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			_saveButton.Draw(gameTime, spriteBatch);
			_goBackButton.Draw(gameTime, spriteBatch);
			_testLevelButton.Draw(gameTime, spriteBatch);
			int yOffset = (int)_rightPanelBounds.Y + 50;
			List<ButtonConfiguration> visibleButtons = GetVisibleButtons();
			foreach (var button in visibleButtons)
				yOffset = button.Draw(gameTime, spriteBatch, yOffset);
		}
		public void Update(Vector2 mousePosition)
		{
			// Hard Coded Buttons
			if (mousePosition.IsInBounds(_saveButton.TotalBounds))
				_saveButton.HasBeenHighlighted();
			else
				_saveButton.HasBeenDeHighlighted();
			if (mousePosition.IsInBounds(_goBackButton.TotalBounds))
				_goBackButton.HasBeenHighlighted();
			else
				_goBackButton.HasBeenDeHighlighted();
			if (mousePosition.IsInBounds(_testLevelButton.TotalBounds))
				_testLevelButton.HasBeenHighlighted();
			else
				_testLevelButton.HasBeenDeHighlighted();

			// Other buttons
			List<ButtonConfiguration> visibleButtons = GetVisibleButtons();
			foreach (var button in visibleButtons)
				if (button.YOffset < mousePosition.Y && mousePosition.Y < button.YOffset + button.Height)
					button.CheckMouse(mousePosition);
		}
		private List<ButtonConfiguration> GetVisibleButtons()
		{
			GuiObjectClass curClass = _selectedObject == null ? GuiObjectClass.Level : _selectedObjectClass;
			return _buttons.Where(b => _availableButtons[curClass].HasFlag(b.Key)).Select(b => b.Value).ToList();
		}

		public void Click(Vector2 mousePosition, Level level)
		{
			// Hard Coded Buttons
			if (_saveButton.IsHighLighted)
				_saveButton.CallAction();
			if (_goBackButton.IsHighLighted)
				_goBackButton.CallAction();
			if (_testLevelButton.IsHighLighted)
				_testLevelButton.CallAction();

			// Other buttons
			List<ButtonConfiguration> visibleButtons = GetVisibleButtons();
			foreach (var button in visibleButtons)
				if (button.YOffset < mousePosition.Y && mousePosition.Y < button.YOffset + button.Height)
					button.Click();
		}
		[Flags]
		private enum ButtonType
		{
			Group = 1,
			Position = 2,
			Size = 4,
			Direction1 = 8,
			//Direction2 = 16, // Need to switch this out!
			Speed = 32,
			Remove = 64,
			Team = 128,
			LevelSize = 256,
			LevelWinCondition = 512,
			LevelScenarioType = 1024
			// Health, damage, stuff like that.
		}

		private void MakeButtons()
		{
			Vector4 leftButtonBounds = new Vector4(Bounds.X + Size.X - 63, 0, 30, 30);
			Vector4 rightButtonBounds = new Vector4(Bounds.X + Size.X - 33, 0, 30, 30);
			_buttons = new Dictionary<ButtonType, ButtonConfiguration>();
			var groupButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			groupButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => mgo.Group.ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.Group = (Group)(((int)_selectedObject.Group + 1) % Enum.GetNames(typeof(Group)).Length); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.Group = (Group)(((int)_selectedObject.Group + Enum.GetNames(typeof(Group)).Length -1) % Enum.GetNames(typeof(Group)).Length); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.Group, new ButtonConfiguration(this, ButtonType.Group.ToString(), groupButtons));

			var teamButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			teamButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => mgo.Team.ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.Team = (Team)(((int)_selectedObject.Team + 1) % Enum.GetNames(typeof(Team)).Length); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.Team = (Team)(((int)_selectedObject.Team + Enum.GetNames(typeof(Team)).Length -1) % Enum.GetNames(typeof(Team)).Length); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.Team, new ButtonConfiguration(this, ButtonType.Team.ToString(), teamButtons));

			var positionButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			positionButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "X: " + ((int)(mgo.Position.X / MapEditorEditMap.SnapTo)).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(MoveRight, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(MoveLeft, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			positionButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "Y: " + ((int)(mgo.Position.Y / MapEditorEditMap.SnapTo)).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(MoveDown, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(MoveUp, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.Position, new ButtonConfiguration(this, ButtonType.Position.ToString(), positionButtons));

			var sizeButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			sizeButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "Width: " + ((int)(mgo.Size.X / MapEditorEditMap.SnapTo)).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X + MapEditorEditMap.SnapTo, _selectedObject.Size.Y); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X - MapEditorEditMap.SnapTo, _selectedObject.Size.Y); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			sizeButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "Height: " + ((int)(mgo.Size.Y / MapEditorEditMap.SnapTo)).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X, _selectedObject.Size.Y + MapEditorEditMap.SnapTo); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X, _selectedObject.Size.Y - MapEditorEditMap.SnapTo); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.Size, new ButtonConfiguration(this, ButtonType.Size.ToString(), sizeButtons));

			var levelSizeButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			levelSizeButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "Width: " + ((int)(_mapEditorEditMap.Level.Size.X / (MapEditorEditMap.SnapTo * 4))).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X + (MapEditorEditMap.SnapTo*4), _mapEditorEditMap.Level.Size.Y); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X - (MapEditorEditMap.SnapTo*4), _mapEditorEditMap.Level.Size.Y); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			levelSizeButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "Height: " + ((int)(_mapEditorEditMap.Level.Size.Y / (MapEditorEditMap.SnapTo * 4))).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X, _mapEditorEditMap.Level.Size.Y + (MapEditorEditMap.SnapTo*4)); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X, _mapEditorEditMap.Level.Size.Y - (MapEditorEditMap.SnapTo*4)); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.LevelSize, new ButtonConfiguration(this, ButtonType.LevelSize.ToString(), levelSizeButtons));

			int numWinConditions = Enum.GetNames(typeof(WinCondition)).Length - 1;
			var winConditionButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			winConditionButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => _mapEditorEditMap.LevelMetaData.WinCondition.ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _mapEditorEditMap.LevelMetaData.WinCondition = (WinCondition)(((int)_mapEditorEditMap.LevelMetaData.WinCondition + 1) % numWinConditions); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _mapEditorEditMap.LevelMetaData.WinCondition = (WinCondition)(((int)_mapEditorEditMap.LevelMetaData.WinCondition + numWinConditions-1) % numWinConditions); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.LevelWinCondition, new ButtonConfiguration(this, ButtonType.LevelWinCondition.ToString(), winConditionButtons));

			int numScenarioType = Enum.GetNames(typeof(ScenarioType)).Length - 1;
			var scenarioButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			scenarioButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => _mapEditorEditMap.LevelMetaData.ScenarioType.ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _mapEditorEditMap.LevelMetaData.ScenarioType = (ScenarioType)(((int)_mapEditorEditMap.LevelMetaData.ScenarioType + 1) % numScenarioType); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _mapEditorEditMap.LevelMetaData.ScenarioType = (ScenarioType)(((int)_mapEditorEditMap.LevelMetaData.ScenarioType + numScenarioType-1) % numScenarioType); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.LevelScenarioType, new ButtonConfiguration(this, ButtonType.LevelScenarioType.ToString(), scenarioButtons));



			var direction1Buttons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			direction1Buttons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => _selectedObject.GetDirectionalText(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.SwitchDirections(); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.SwitchDirections(); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.Direction1, new ButtonConfiguration(this, ButtonType.Direction1.ToString(), direction1Buttons));

			var speedButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			speedButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "X Speed: " + mgo.MaxSpeedBase.X.ToString("#.0"),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X + .2f, _selectedObject.MaxSpeedBase.Y); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X - .2f, _selectedObject.MaxSpeedBase.Y); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			speedButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "Y Speed: " + mgo.MaxSpeedBase.Y.ToString("#.0"),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X, _selectedObject.MaxSpeedBase.Y + .2f); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X, _selectedObject.MaxSpeedBase.Y - .2f); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}));
			_buttons.Add(ButtonType.Speed, new ButtonConfiguration(this, ButtonType.Speed.ToString(), speedButtons));


			var trashButtons = new List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>>();
			trashButtons.Add(new Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>((mgo) => "Click to Trash " + mgo.Name,
				new List<MenuItemButton>(){
					new ImageMenuItemButton(TrashCurrentItem, _trashCan, rightButtonBounds + new Vector4(0,0,10,0), false),
				}));
			_buttons.Add(ButtonType.Remove, new ButtonConfiguration(this, ButtonType.Remove.ToString(), trashButtons));
		}

		private Dictionary<GuiObjectClass, ButtonType> _availableButtons = new Dictionary<GuiObjectClass, ButtonType>()
		{
			{GuiObjectClass.Platform       , ButtonType.Remove | ButtonType.Group | ButtonType.Position | ButtonType.Size},    
			{GuiObjectClass.MovingPlatform , ButtonType.Remove | ButtonType.Direction1 | ButtonType.Group | ButtonType.Position | ButtonType.Size},
			{GuiObjectClass.Player         , ButtonType.Remove | ButtonType.Team | ButtonType.Group | ButtonType.Position | ButtonType.Size},
			{GuiObjectClass.AIPlayer       , ButtonType.Remove | ButtonType.Team | ButtonType.Group | ButtonType.Position | ButtonType.Speed},
			{GuiObjectClass.HealthCreep    , ButtonType.Remove | ButtonType.Direction1 | ButtonType.Group | ButtonType.Position | ButtonType.Speed},
			{GuiObjectClass.MovingCharacter, ButtonType.Remove | ButtonType.Team | ButtonType.Direction1 | ButtonType.Group | ButtonType.Position | ButtonType.Speed},
			{GuiObjectClass.WallRunner		 , ButtonType.Remove | ButtonType.Team | ButtonType.Direction1 | ButtonType.Group | ButtonType.Position | ButtonType.Speed},
			{GuiObjectClass.StandardTurret , ButtonType.Remove | ButtonType.Direction1 | ButtonType.Team | ButtonType.Position | ButtonType.Size },
			{GuiObjectClass.StandardBase   , ButtonType.Remove | ButtonType.Team | ButtonType.Position | ButtonType.Size },
			{GuiObjectClass.Level			 , ButtonType.LevelSize | ButtonType.LevelWinCondition | ButtonType.LevelScenarioType },
			{GuiObjectClass.FinishLineFlagPole, ButtonType.Remove | ButtonType.Position | ButtonType.Size },
		};

		public void MoveLeft()
		{
			if (_selectedObject != null)
				_selectedObject.Position = new Vector2(_selectedObject.Position.X - MapEditorEditMap.SnapTo, _selectedObject.Position.Y);
		}
		public void MoveRight()
		{
			if (_selectedObject != null)
				_selectedObject.Position = new Vector2(_selectedObject.Position.X + MapEditorEditMap.SnapTo, _selectedObject.Position.Y);
		}
		public void MoveUp()
		{
			if (_selectedObject != null)
				_selectedObject.Position = new Vector2(_selectedObject.Position.X, _selectedObject.Position.Y - MapEditorEditMap.SnapTo);
		}
		public void MoveDown()
		{
			if (_selectedObject != null)
				_selectedObject.Position = new Vector2(_selectedObject.Position.X, _selectedObject.Position.Y + MapEditorEditMap.SnapTo);
		}
		public void TrashCurrentItem()
		{
			if (_selectedObject != null)
			{
				if (_selectedObject.GetType().IsAssignableFrom(typeof(Player)))
					MainGame.PlayerManager.RemovePlayer(_selectedObject.Id);
				_mapEditorEditMap.SwitchState(MapEditorState.Select);
				_selectedObject.Level.RemoveGuiObject(_selectedObject); DeselectItem();
			}
		}
	}
	public class ButtonConfiguration
	{
		public int YOffset { get { return _yOffsetbegin; } }
		private int _yOffsetbegin;
		public int Height { get { return _height; } }
		private int _height;

		private MapEditorRightPanel _manager;
		private string _title;
		private Vector4 _titleRelativeBounds;
		private List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>> _buttons;
		public ButtonConfiguration(MapEditorRightPanel manager, string title, List<Tuple<Func<MainGuiObject, string>, List<MenuItemButton>>> buttons)
		{
			_title = title;
			_manager = manager;
			_buttons = buttons;
			_titleRelativeBounds = _title.GetTextBoundsByCenter(MainGame.PlainFont, _manager.Size / 2);
			_titleRelativeBounds.Y = 0;
		}
		public int Draw(GameTime gameTime, SpriteBatch spriteBatch, int yOffset)
		{
			_yOffsetbegin = yOffset;
			yOffset += 10;
			spriteBatch.DrawString(MainGame.PlainFont, _title, _titleRelativeBounds.GetPosition() + new Vector2(_manager.Bounds.X, yOffset), Color.Black);
			yOffset += (int)_titleRelativeBounds.Z + 10;
			foreach (Tuple<Func<MainGuiObject, string>, List<MenuItemButton>> buttonTuple in _buttons)
			{
				string currentText = buttonTuple.Item1(_manager.SelectedObject);
				List<MenuItemButton> buttons = buttonTuple.Item2;
				spriteBatch.DrawString(MainGame.PlainFont, currentText, new Vector2(_manager.Bounds.X + 10, 5 + yOffset), Color.Black);
				foreach (MenuItemButton button in buttons)
				{
					button.Bounds = new Vector4(button.TotalBounds.X, yOffset, button.TotalBounds.Z, button.TotalBounds.W);
					button.Draw(gameTime, spriteBatch);
				}
				yOffset += 40;
			}
			_height = yOffset - _yOffsetbegin;
			return yOffset;
		}
		public void CheckMouse(Vector2 mousePosition)
		{
			foreach (Tuple<Func<MainGuiObject, string>, List<MenuItemButton>> buttonTuple in _buttons)
			{
				List<MenuItemButton> buttons = buttonTuple.Item2;
				foreach (MenuItemButton button in buttons)
				{
					if (mousePosition.IsInBounds(button.TotalBounds))
						button.HasBeenHighlighted();
					else
						button.HasBeenDeHighlighted();
				}
			}
		}
		public void Click()
		{
			foreach (Tuple<Func<MainGuiObject, string>, List<MenuItemButton>> buttonTuple in _buttons)
			{
				List<MenuItemButton> buttons = buttonTuple.Item2;
				foreach (MenuItemButton button in buttons.Where(b => b.IsHighLighted))
					button.CallAction();
			}
		}
		//public void DeselectAll()
		//{
		//	foreach (Tuple<Func<MainGuiObject, string>, List<MenuItemButton>> buttonTuple in _buttons)
		//	{
		//		List<MenuItemButton> buttons = buttonTuple.Item2;
		//		foreach (MenuItemButton button in buttons)
		//			button.HasBeenDeHighlighted();
		//	}
		//}
	}
}
