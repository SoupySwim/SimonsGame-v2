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
		public HashSet<MainGuiObject> AggregateSelectedObjects { get; set; }
		private MainGuiObject _selectedObject;
		private GuiObjectClass _selectedObjectClass;

		public MapEditorRightPanel(MapEditorEditMap mapEditorEditMap, Vector4 rightPanelBounds, Level level, MenuStateManager manager)
		{
			_mapEditorEditMap = mapEditorEditMap;
			_rightPanelBounds = rightPanelBounds;
			float topButtonWidths = (_rightPanelBounds.W - 15) / 2;

			AggregateSelectedObjects = new HashSet<MainGuiObject>();

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
			_selectedObjectClass = (GuiObjectClass)(-1); // Start off with nothing selected!
			MakeButtons();
		}

		public void SelectNewItem(MainGuiObject mgo)
		{
			if (mgo != null)
			{
				GuiObjectClass classType = mgo.GetClass(); //(GuiObjectClass)Enum.Parse(typeof(GuiObjectClass), mgo.GetType().Name);
				SelectNewItem(mgo, classType);
			}
			else _selectedObject = null; // If you select nothingness, then you will be modifying the level.
		}

		private void SelectNewItem(MainGuiObject mgo, GuiObjectClass objClass)
		{
			_selectedObject = mgo;
			if (_selectedObjectClass != objClass)
			{
				_selectedObjectClass = objClass;
				ButtonConfiguration.GenericButtons.ForEach(bType => _buttons[bType].ChangeTitle(mgo.GetSpecialTitle(bType)));
			}
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
				{
					button.CheckMouse(mousePosition);
					if (Controls.CurrentMouse.ScrollWheelValue > Controls.PreviousMouse.ScrollWheelValue)
						button.Scroll(mousePosition, true);
					else if (Controls.CurrentMouse.ScrollWheelValue < Controls.PreviousMouse.ScrollWheelValue)
						button.Scroll(mousePosition, false);
				}
		}

		private List<ButtonConfiguration> GetVisibleButtons()
		{
			if (AggregateSelectedObjects.Any())
				return _buttons.Where(b => _availableButtons[GuiObjectClass.Aggregate].HasFlag(b.Key)).Select(b => b.Value).ToList();
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

		// This should be cleaned up a bit...
		private void MakeButtons()
		{
			Vector4 leftButtonBounds = new Vector4(Bounds.X + Size.X - 63, 0, 30, 30);
			Vector4 rightButtonBounds = new Vector4(Bounds.X + Size.X - 33, 0, 30, 30);
			_buttons = new Dictionary<ButtonType, ButtonConfiguration>();

			var groupButtons = new List<RightPanelButtonGroup>();
			Action upAction = () => { _selectedObject.Group = (Group)(((int)_selectedObject.Group + 1) % Enum.GetNames(typeof(Group)).Length); };
			Action downAction = () => { _selectedObject.Group = (Group)(((int)_selectedObject.Group + Enum.GetNames(typeof(Group)).Length - 1) % Enum.GetNames(typeof(Group)).Length); };
			groupButtons.Add(new RightPanelButtonGroup((mgo) => mgo.Group.ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(upAction, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(downAction, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, upAction, downAction));
			_buttons.Add(ButtonType.Group, new ButtonConfiguration(this, ButtonType.Group.ToString(), groupButtons));

			var teamButtons = new List<RightPanelButtonGroup>();
			upAction = () => { _selectedObject.Team = (Team)(((int)_selectedObject.Team + 1) % Enum.GetNames(typeof(Team)).Length); };
			downAction = () => { _selectedObject.Team = (Team)(((int)_selectedObject.Team + Enum.GetNames(typeof(Team)).Length - 1) % Enum.GetNames(typeof(Team)).Length); };
			teamButtons.Add(new RightPanelButtonGroup((mgo) => mgo.Team.ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.Team = (Team)(((int)_selectedObject.Team + 1) % Enum.GetNames(typeof(Team)).Length); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.Team = (Team)(((int)_selectedObject.Team + Enum.GetNames(typeof(Team)).Length -1) % Enum.GetNames(typeof(Team)).Length); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, upAction, downAction));
			_buttons.Add(ButtonType.Team, new ButtonConfiguration(this, ButtonType.Team.ToString(), teamButtons));

			var positionButtons = new List<RightPanelButtonGroup>();
			positionButtons.Add(new RightPanelButtonGroup((mgo) => "X: " + ((int)(mgo.Position.X / MapEditorEditMap.SnapTo)).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(MoveRight, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(MoveLeft, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () => { }, () => { }));
			positionButtons.Add(new RightPanelButtonGroup((mgo) => "Y: " + ((int)(mgo.Position.Y / MapEditorEditMap.SnapTo)).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(MoveDown, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(MoveUp, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () => { }, () => { }));
			_buttons.Add(ButtonType.Position, new ButtonConfiguration(this, ButtonType.Position.ToString(), positionButtons));

			var sizeButtons = new List<RightPanelButtonGroup>();
			sizeButtons.Add(new RightPanelButtonGroup((mgo) => "Width: " + ((int)(mgo.Size.X / MapEditorEditMap.SnapTo)).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X + MapEditorEditMap.SnapTo, _selectedObject.Size.Y); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X - MapEditorEditMap.SnapTo, _selectedObject.Size.Y); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () => { _selectedObject.Size = new Vector2(_selectedObject.Size.X + (MapEditorEditMap.SnapTo * 4), _selectedObject.Size.Y); },
				() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X - (MapEditorEditMap.SnapTo * 4), _selectedObject.Size.Y); }));
			sizeButtons.Add(new RightPanelButtonGroup((mgo) => "Height: " + ((int)(mgo.Size.Y / MapEditorEditMap.SnapTo)).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X, _selectedObject.Size.Y + MapEditorEditMap.SnapTo); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X, _selectedObject.Size.Y - MapEditorEditMap.SnapTo); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () => { _selectedObject.Size = new Vector2(_selectedObject.Size.X, _selectedObject.Size.Y + (MapEditorEditMap.SnapTo * 4)); },
				() => { _selectedObject.Size = new Vector2(_selectedObject.Size.X, _selectedObject.Size.Y - (MapEditorEditMap.SnapTo * 4)); }));
			_buttons.Add(ButtonType.Size, new ButtonConfiguration(this, ButtonType.Size.ToString(), sizeButtons));

			var levelSizeButtons = new List<RightPanelButtonGroup>();
			levelSizeButtons.Add(new RightPanelButtonGroup((mgo) => "Width: " + ((int)(_mapEditorEditMap.Level.Size.X / (MapEditorEditMap.SnapTo * 4))).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X + (MapEditorEditMap.SnapTo * 4), _mapEditorEditMap.Level.Size.Y); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X - (MapEditorEditMap.SnapTo * 4), _mapEditorEditMap.Level.Size.Y); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X + (MapEditorEditMap.SnapTo * 16), _mapEditorEditMap.Level.Size.Y); },
				() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X - (MapEditorEditMap.SnapTo * 16), _mapEditorEditMap.Level.Size.Y); }));

			levelSizeButtons.Add(new RightPanelButtonGroup((mgo) => "Height: " + ((int)(_mapEditorEditMap.Level.Size.Y / (MapEditorEditMap.SnapTo * 4))).ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X, _mapEditorEditMap.Level.Size.Y + (MapEditorEditMap.SnapTo*4)); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X, _mapEditorEditMap.Level.Size.Y - (MapEditorEditMap.SnapTo*4)); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X, _mapEditorEditMap.Level.Size.Y + (MapEditorEditMap.SnapTo * 16)); },
				() => { _mapEditorEditMap.Level.Size = new Vector2(_mapEditorEditMap.Level.Size.X, _mapEditorEditMap.Level.Size.Y - (MapEditorEditMap.SnapTo * 16)); }));
			_buttons.Add(ButtonType.LevelSize, new ButtonConfiguration(this, ButtonType.LevelSize.ToString(), levelSizeButtons));

			int numWinConditions = Enum.GetNames(typeof(WinCondition)).Length - 1;
			var winConditionButtons = new List<RightPanelButtonGroup>();
			upAction = () => { _mapEditorEditMap.LevelMetaData.WinCondition = (WinCondition)(((int)_mapEditorEditMap.LevelMetaData.WinCondition + 1) % numWinConditions); };
			downAction = () => { _mapEditorEditMap.LevelMetaData.WinCondition = (WinCondition)(((int)_mapEditorEditMap.LevelMetaData.WinCondition + numWinConditions - 1) % numWinConditions); };
			winConditionButtons.Add(new RightPanelButtonGroup((mgo) => _mapEditorEditMap.LevelMetaData.WinCondition.ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton( upAction, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton( downAction, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, upAction, downAction));
			_buttons.Add(ButtonType.LevelWinCondition, new ButtonConfiguration(this, ButtonType.LevelWinCondition.ToString(), winConditionButtons));

			int numScenarioType = Enum.GetNames(typeof(ScenarioType)).Length - 1;
			var scenarioButtons = new List<RightPanelButtonGroup>();
			upAction = () => { _mapEditorEditMap.LevelMetaData.ScenarioType = (ScenarioType)(((int)_mapEditorEditMap.LevelMetaData.ScenarioType + 1) % numScenarioType); };
			downAction = () => { _mapEditorEditMap.LevelMetaData.ScenarioType = (ScenarioType)(((int)_mapEditorEditMap.LevelMetaData.ScenarioType + numScenarioType - 1) % numScenarioType); };
			scenarioButtons.Add(new RightPanelButtonGroup((mgo) => _mapEditorEditMap.LevelMetaData.ScenarioType.ToString(),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(upAction, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(downAction, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, upAction, downAction));
			_buttons.Add(ButtonType.LevelScenarioType, new ButtonConfiguration(this, ButtonType.LevelScenarioType.ToString(), scenarioButtons));

			var speedButtons = new List<RightPanelButtonGroup>();
			speedButtons.Add(new RightPanelButtonGroup((mgo) => "X Speed: " + mgo.MaxSpeedBase.X.ToString("#.0"),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X + .2f, _selectedObject.MaxSpeedBase.Y); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X - .2f, _selectedObject.MaxSpeedBase.Y); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X + .6f, _selectedObject.MaxSpeedBase.Y); },
				() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X - .6f, _selectedObject.MaxSpeedBase.Y); }));
			speedButtons.Add(new RightPanelButtonGroup((mgo) => "Y Speed: " + mgo.MaxSpeedBase.Y.ToString("#.0"),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X, _selectedObject.MaxSpeedBase.Y + .2f); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X, _selectedObject.MaxSpeedBase.Y - .2f); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X, _selectedObject.MaxSpeedBase.Y + .6f); },
				() => { _selectedObject.MaxSpeedBase = new Vector2(_selectedObject.MaxSpeedBase.X, _selectedObject.MaxSpeedBase.Y - .6f); }));
			_buttons.Add(ButtonType.Speed, new ButtonConfiguration(this, ButtonType.Speed.ToString(), speedButtons));

			for (int i = 0; i < 3; i++)
			{
				ButtonType healthButton = ButtonType.HealthTotalSmall;
				int increaseBy = 20;
				int minHealth = 20;
				int maxHealth = 2000;
				if (i == 1)
				{
					increaseBy = 100;
					minHealth = 100;
					maxHealth = 4000;
					healthButton = ButtonType.HealthTotalMedium;
				}
				else if (i == 2)
				{
					increaseBy = 200;
					minHealth = 400;
					maxHealth = 10000;
					healthButton = ButtonType.HealthTotalLarge;
				}


				var healthSmallButtons = new List<RightPanelButtonGroup>();
				healthSmallButtons.Add(new RightPanelButtonGroup((mgo) => "Total Health: " + string.Format("{0}", mgo.HealthTotal),
					new List<MenuItemButton>(){
					new ImageMenuItemButton(() => {
						_selectedObject.SetHealthTotal(MathHelper.Clamp(_selectedObject.HealthTotal + increaseBy, minHealth, maxHealth));
						_selectedObject.HealthCurrent = _selectedObject.HealthTotal;
					}, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(() => {
						_selectedObject.SetHealthTotal(MathHelper.Clamp(_selectedObject.HealthTotal - increaseBy, minHealth, maxHealth));
						_selectedObject.HealthCurrent = _selectedObject.HealthTotal;
					}, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, () =>
					{
						_selectedObject.SetHealthTotal(MathHelper.Clamp(_selectedObject.HealthTotal + (increaseBy * 4), minHealth, maxHealth));
						_selectedObject.HealthCurrent = _selectedObject.HealthTotal;
					},
					() =>
					{
						_selectedObject.SetHealthTotal(MathHelper.Clamp(_selectedObject.HealthTotal - (increaseBy * 4), minHealth, maxHealth));
						_selectedObject.HealthCurrent = _selectedObject.HealthTotal;
					}));
				_buttons.Add(healthButton, new ButtonConfiguration(this, "Total Health", healthSmallButtons));
			}

			ButtonConfiguration.GenericButtons.ForEach(type =>
			{
				var genericButton = new List<RightPanelButtonGroup>();
				//
				upAction = () => { _selectedObject.ModifySpecialText(type, true); };
				downAction = () => { _selectedObject.ModifySpecialText(type, false); };
				genericButton.Add(new RightPanelButtonGroup((mgo) => mgo.GetSpecialText(type),
					new List<MenuItemButton>(){
					new ImageMenuItemButton(upAction, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
					new ImageMenuItemButton(downAction, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
				}, upAction, downAction));
				_buttons.Add(type, new ButtonConfiguration(this, type.ToString(), genericButton));
			});



			//var direction1Buttons = new List<RightPanelButtonGroup>();
			//direction1Buttons.Add(new RightPanelButtonGroup((mgo) => _selectedObject.GetDirectionalText(),
			//	new List<MenuItemButton>(){
			//		new ImageMenuItemButton(() => { _selectedObject.SwitchDirections(); }, _arrow, rightButtonBounds, Color.Orange, Color.Red, false),
			//		new ImageMenuItemButton(() => { _selectedObject.SwitchDirections(); }, _arrow, leftButtonBounds, Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally)
			//	}));
			//_buttons.Add(ButtonType.Direction1, new ButtonConfiguration(this, ButtonType.Direction1.ToString(), direction1Buttons));

			var trashButtons = new List<RightPanelButtonGroup>();
			trashButtons.Add(new RightPanelButtonGroup((mgo) => string.Format("Click to Trash {0}", (mgo == null ? "Items" : mgo.Name)),
				new List<MenuItemButton>(){
					new ImageMenuItemButton(TrashCurrentItem, _trashCan, rightButtonBounds + new Vector4(0,0,10,0), false),
				}, () => { }, () => { }));
			_buttons.Add(ButtonType.Remove, new ButtonConfiguration(this, ButtonType.Remove.ToString(), trashButtons));
		}
		private Dictionary<GuiObjectClass, ButtonType> _availableButtons = new Dictionary<GuiObjectClass, ButtonType>()
		{
			{GuiObjectClass.Level					, ButtonType.LevelSize | ButtonType.LevelWinCondition | ButtonType.LevelScenarioType },

			{GuiObjectClass.Player					, ButtonType.Remove | ButtonType.Position | ButtonType.Team | ButtonType.Group },
			{GuiObjectClass.AIPlayer				, ButtonType.Remove | ButtonType.Position | ButtonType.Team | ButtonType.Group | ButtonType.Speed },

			// Characters
			{GuiObjectClass.HealthCreep			, ButtonType.Remove | ButtonType.Position | ButtonType.Direction | ButtonType.Group | ButtonType.Speed | ButtonType.SpecialToggle1 },
			{GuiObjectClass.ElementalCharacter	, ButtonType.Remove | ButtonType.Position | ButtonType.SpecialToggle1 | ButtonType.HealthTotalMedium },
			{GuiObjectClass.MinionNormal			, ButtonType.Remove | ButtonType.Position | ButtonType.Team | ButtonType.Direction | ButtonType.Group | ButtonType.Speed | ButtonType.HealthTotalSmall },
			{GuiObjectClass.MinionFlying			, ButtonType.Remove | ButtonType.Position | ButtonType.Team | ButtonType.Direction | ButtonType.Group | ButtonType.Speed | ButtonType.HealthTotalSmall },
			{GuiObjectClass.MinionLarge			, ButtonType.Remove | ButtonType.Position | ButtonType.Team | ButtonType.Direction | ButtonType.Group | ButtonType.Speed | ButtonType.HealthTotalMedium },
			{GuiObjectClass.NeutralCreep			, ButtonType.Remove | ButtonType.Position | ButtonType.SpecialToggle1 | ButtonType.HealthTotalSmall },
			{GuiObjectClass.FlyingCreature		, ButtonType.Remove | ButtonType.Position | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 | ButtonType.HealthTotalSmall },
			{GuiObjectClass.LargeCreep				, ButtonType.Remove | ButtonType.Position | ButtonType.Direction | ButtonType.HealthTotalMedium },
			{GuiObjectClass.CreepBoss				, ButtonType.Remove | ButtonType.Position | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 | ButtonType.SpecialToggle3 | ButtonType.HealthTotalLarge },
			{GuiObjectClass.WallRunner				, ButtonType.Remove | ButtonType.Position  | ButtonType.Direction | ButtonType.Speed | ButtonType.HealthTotalSmall },

			// Environment
			{GuiObjectClass.Platform				, ButtonType.Remove | ButtonType.Position | ButtonType.Team | ButtonType.Group | ButtonType.Size | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 | ButtonType.SpecialToggle3 | ButtonType.SpecialToggle4 },
			{GuiObjectClass.MovingPlatform		, ButtonType.Remove | ButtonType.Position | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 | ButtonType.Group  | ButtonType.Size},
			{GuiObjectClass.StandardTurret		, ButtonType.Remove | ButtonType.Position | ButtonType.Direction | ButtonType.Team | ButtonType.Size },
			{GuiObjectClass.StandardBase			, ButtonType.Remove | ButtonType.Position | ButtonType.Team | ButtonType.Size },
			{GuiObjectClass.FinishLineFlagPole	, ButtonType.Remove | ButtonType.Position | ButtonType.Size },
			{GuiObjectClass.Block					, ButtonType.Remove | ButtonType.Position | ButtonType.Group | ButtonType.Size },
			{GuiObjectClass.ObjectSpawner			, ButtonType.Remove | ButtonType.Position | ButtonType.Team | ButtonType.Group | ButtonType.Direction | ButtonType.Size | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 | ButtonType.SpecialToggle3 | ButtonType.SpecialToggle4 }, // Definitely must do object spawner...!
			{GuiObjectClass.Ladder					, ButtonType.Remove | ButtonType.Position | ButtonType.Size },
			{GuiObjectClass.Button					, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 },
			{GuiObjectClass.JumpPad					, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 },
			{GuiObjectClass.HealthPack				, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 },
			{GuiObjectClass.Teleporter				, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1| ButtonType.SpecialToggle2},
			{GuiObjectClass.Spike					, ButtonType.Remove | ButtonType.Position | ButtonType.Group | ButtonType.Size | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 },
			{GuiObjectClass.LockedBarrier			, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 },
			{GuiObjectClass.SmallKeyObject		, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 },

			// Other Stuff
			{GuiObjectClass.SuperSpeed				, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 },
			{GuiObjectClass.SuperJump				, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 },
			{GuiObjectClass.AbilityObject			, ButtonType.Remove | ButtonType.Position | ButtonType.Size },
			{GuiObjectClass.JungleCreepZone		, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 },
			{GuiObjectClass.BehaviorZone			, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.Team | ButtonType.SpecialToggle1 | ButtonType.SpecialToggle2 },
			{GuiObjectClass.GuiFunction			, ButtonType.Remove | ButtonType.Position },
			{GuiObjectClass.GuiIfClause			, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 },
			{GuiObjectClass.GuiThenClause			, ButtonType.Remove | ButtonType.Position | ButtonType.Size | ButtonType.SpecialToggle1 },

			
			{GuiObjectClass.Aggregate				, ButtonType.Remove },
		};
		public ButtonType GetCurrentSettings(GuiObjectClass selectedObjectClass)
		{
			return _availableButtons[selectedObjectClass];
		}

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
			if (AggregateSelectedObjects.Any())
			{
				foreach (var mgo in AggregateSelectedObjects.ToList())
				{
					_mapEditorEditMap.SwitchState(MapEditorState.Select);
					if (mgo.GetType().IsAssignableFrom(typeof(Player)))
						MainGame.PlayerManager.RemovePlayer(mgo.Id);
					mgo.Level.RemoveGuiObject(mgo);
					AggregateSelectedObjects.Clear();
				}
			}
			if (_selectedObject != null)
			{
				if (_selectedObject.GetType().IsAssignableFrom(typeof(Player)))
					MainGame.PlayerManager.RemovePlayer(_selectedObject.Id);
				_mapEditorEditMap.SwitchState(MapEditorState.Select);
				_selectedObject.Level.RemoveGuiObject(_selectedObject);
				DeselectItem();
			}
		}
	}
	[Flags]
	public enum ButtonType
	{
		Group = 1,
		Position = 2,
		Size = 4,
		Direction = 8,
		//HealthTotal = 16,
		Speed = 32,
		Remove = 64,
		Team = 128,
		LevelSize = 256,
		LevelWinCondition = 512,
		LevelScenarioType = 1024,
		SpecialToggle1 = 2048,
		SpecialToggle2 = 4096,
		SpecialToggle3 = 8192,
		SpecialToggle4 = 16384,
		HealthTotalSmall = 32768,
		HealthTotalMedium = 65536,
		HealthTotalLarge = 131072,
		// Health, damage, stuff like that.
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
		private List<RightPanelButtonGroup> _buttonGroups;
		public ButtonConfiguration(MapEditorRightPanel manager, string title, List<RightPanelButtonGroup> buttons)
		{
			_manager = manager;
			ChangeTitle(title);
			_buttonGroups = buttons;
		}

		public void ChangeTitle(string title)
		{
			if (title == "")
				return;
			_title = title;
			_titleRelativeBounds = _title.GetTextBoundsByCenter(MainGame.PlainFont, _manager.Size / 2);
			_titleRelativeBounds.Y = 0;
		}
		public int Draw(GameTime gameTime, SpriteBatch spriteBatch, int yOffset)
		{
			_yOffsetbegin = yOffset;
			yOffset += 10;
			spriteBatch.DrawString(MainGame.PlainFont, _title, _titleRelativeBounds.GetPosition() + new Vector2(_manager.Bounds.X, yOffset), Color.Black);
			yOffset += (int)_titleRelativeBounds.Z + 10;
			foreach (RightPanelButtonGroup group in _buttonGroups)
			{
				string currentText = group.GetObjectValue(_manager.SelectedObject);
				spriteBatch.DrawString(MainGame.PlainFont, currentText, new Vector2(_manager.Bounds.X + 10, 5 + yOffset), Color.Black);
				foreach (MenuItemButton button in group)
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
			foreach (RightPanelButtonGroup buttons in _buttonGroups)
			{
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
			foreach (RightPanelButtonGroup group in _buttonGroups)
			{
				foreach (MenuItemButton button in group.Where(b => b.IsHighLighted))
					button.CallAction();
			}
		}
		public void Scroll(Vector2 mousePosition, bool up)
		{
			foreach (RightPanelButtonGroup buttons in _buttonGroups)
			{
				foreach (MenuItemButton button in buttons)
				{
					if (mousePosition.IsInBounds(button.TotalBounds))
					{
						if (up)
							buttons.ScrollUp();
						else // down
							buttons.ScrollDown();
						return;
					}
				}
			}
		}
		//public void DeselectAll()
		//{
		//	foreach (RightPanelButtonGroup buttonTuple in _buttons)
		//	{
		//		List<MenuItemButton> buttons = buttonTuple.Item2;
		//		foreach (MenuItemButton button in buttons)
		//			button.HasBeenDeHighlighted();
		//	}
		//}
		public static List<ButtonType> GenericButtons = new List<ButtonType>()
		{
			ButtonType.Direction,
			ButtonType.SpecialToggle1,
			ButtonType.SpecialToggle2,
			ButtonType.SpecialToggle3,
			ButtonType.SpecialToggle4,
		};
	}

	public class RightPanelButtonGroup : List<MenuItemButton>
	{
		public Action ScrollUp;
		public Action ScrollDown;
		public RightPanelButtonGroup(Func<MainGuiObject, string> func, List<MenuItemButton> ls, Action up, Action down)
			: base(ls)
		{
			GetObjectValue = func;
			ScrollUp = up;
			ScrollDown = down;
		}
		public Func<MainGuiObject, string> GetObjectValue { get; set; }
	}
}
