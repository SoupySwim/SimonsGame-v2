using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;
using SimonsGame.Utility;
using Microsoft.Xna.Framework.Input;

namespace SimonsGame.Menu.MenuScreens
{
	public enum MapEditorState
	{
		AddNew,
		Select
	}
	public class MapEditorEditMap : MainMenuScreen
	{
		private class MapEditStep
		{
			public Vector4 OldBounds { get; set; }
			public bool isCreating { get; set; }
			public Guid ItemId { get; set; }
			public GuiObjectType Type { get; set; }
			public Group Group { get; set; }
			public MapEditorState State { get; set; }
		}

		#region Level
		public Level Level { get { return _level; } }
		private Level _level;
		private Viewport _levelViewport;
		private Vector2 _cameraPosition;
		private Matrix _scaleMatrix;
		private float _scaleAmount = 1f;
		public static float SnapTo = 10.0f;

		// Only used when in select mode.
		private bool _isResizing = false;
		private Vector2 _mouseOffset = Vector2.Zero;
		private LevelFileMetaData _levelMetaData;
		public LevelFileMetaData LevelMetaData { get { return _levelMetaData; } }
		#endregion
		#region Panel Information
		private Vector4 _namePanelBounds;
		private Vector4 _leftPanelBounds;
		public MapEditorLeftPanel LeftPanel { get { return _leftPanel; } }
		private MapEditorLeftPanel _leftPanel;
		private Vector4 _rightPanelBounds;
		public MapEditorRightPanel RightPanel { get { return _rightPanel; } }
		private MapEditorRightPanel _rightPanel;

		private Vector4 _levelBounds;
		private string _mapName;
		private Vector2 _namePosition;
		private int _borderRadius = 2;
		#endregion

		private MapEditorShortcutHandler _shortcutHandler = new MapEditorShortcutHandler();

		private MainGuiObject selectedItemToAdd; // Only used in AddNew state.
		private Stack<MapEditStep> _undoStack = new Stack<MapEditStep>();

		private Vector2 _moveCameraAnchor;

		private MapEditorState _state;

		public MapEditorEditMap(MenuStateManager manager, Vector2 screenSize)
			: base(manager, screenSize)
		{
			// For now, we use a dummy menuitem so we can override everything else.
			_menuLayout = new MenuItemButton[1][];
			_menuLayout[0] = new MenuItemButton[1];
			_menuLayout[0][0] = new TextMenuItemButton(() => { /* Does nothing */ }, "", Vector4.Zero, Color.Orange, Color.Red, Vector2.Zero, true);

			// Now for the panels
			_leftPanelBounds = new Vector4(0, 0, screenSize.Y, screenSize.X / 5.0f);
			_rightPanelBounds = new Vector4(screenSize.X - (screenSize.X / 5.0f), 0, screenSize.Y, screenSize.X / 5.0f);
			int namePanelHeight = 30;
			_namePanelBounds = new Vector4(screenSize.X / 5.0f, 0, namePanelHeight, (3.0f / 5.0f) * screenSize.X);
			_levelBounds = new Vector4(screenSize.X / 5.0f, namePanelHeight, screenSize.Y - namePanelHeight, (3.0f / 5.0f) * screenSize.X);
			_mapName = "Sprint 5 Map";
			_namePosition = _namePanelBounds.GetPosition() + _namePanelBounds.GetSize() / 2 - _mapName.GetTextSize(MainGame.PlainFont) / 2;

			_levelViewport = new Viewport((int)(_levelBounds.X + _borderRadius), (int)(_levelBounds.Y + _borderRadius),
				(int)(_levelBounds.W - (_borderRadius * 2)), (int)(_levelBounds.Z - (_borderRadius * 2)));
			_cameraPosition = Vector2.Zero;
			_scaleMatrix = Matrix.CreateScale(_scaleAmount);

			_state = MapEditorState.AddNew; // MapEditorState.Select;
			_leftPanel = new MapEditorLeftPanel(this, _leftPanelBounds, this);
		}
		public void AddLevel(Level level, LevelFileMetaData levelMetaData)
		{
			_levelMetaData = levelMetaData;
			_level = level;
			selectedItemToAdd = new Platform(Vector2.Zero, Vector2.Zero, Group.ImpassableIncludingMagic, _level);
			_rightPanel = new MapEditorRightPanel(this, _rightPanelBounds, _level, _manager); // Can't have a level editor without a level!
			//SnapTo = _level.PlatformDifference / 16.0f; // Defaulting SnapTo to 10 pixels...
			_cameraPosition = Vector2.Zero;
			_scaleAmount = 1f;
			_scaleMatrix = Matrix.CreateScale(_scaleAmount);
		}

		protected override void DrawExtra(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{

			// Draw level first as we want to block the level out at all costs.
			spriteBatch.Draw(MainGame.SingleColor, _levelBounds.ToRectangle(), Color.Black);
			//Rectangle well = new Rectangle((int)(_levelBounds.X + _borderRadius), (int)(_levelBounds.Y + bord_borderRadiuserRadius),
			//	(int)(_levelBounds.W - (_borderRadius * 2)), (int)(_levelBounds.Z - (_borderRadius * 2)));
			//spriteBatch.Draw(MainGame.SingleColor, well, Color.CornflowerBlue);
			spriteBatch.End();

			// Must do new begin for viewport...
			Viewport originalViewport = spriteBatch.GraphicsDevice.Viewport;
			spriteBatch.GraphicsDevice.Viewport = _levelViewport;
			Matrix cameraTransform = Matrix.CreateTranslation(-_cameraPosition.X, -_cameraPosition.Y, 0.0f) * _scaleMatrix;
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, cameraTransform);
			_level.DrawInViewport(gameTime, spriteBatch, new Vector4((_levelBounds.X + _borderRadius), (_levelBounds.Y + _borderRadius),
				(_levelBounds.Z - (_borderRadius * 2)) / _scaleAmount, (_levelBounds.W - (_borderRadius * 2)) / _scaleAmount), _cameraPosition, null);
			_level.DrawGridInViewport(spriteBatch, new Vector4((_levelBounds.X + _borderRadius), (_levelBounds.Y + _borderRadius),
				(_levelBounds.Z - (_borderRadius * 2)) / _scaleAmount, (_levelBounds.W - (_borderRadius * 2)) / _scaleAmount), _cameraPosition, _scaleAmount);
			spriteBatch.End();

			spriteBatch.GraphicsDevice.Viewport = originalViewport;
			spriteBatch.Begin();
			base.DrawExtra(gameTime, spriteBatch);

			// Left Panel
			spriteBatch.Draw(MainGame.SingleColor, _leftPanelBounds.ToRectangle(), Color.Black);
			Rectangle well = new Rectangle((int)(_leftPanelBounds.X + _borderRadius), (int)(_leftPanelBounds.Y + _borderRadius),
			  (int)(_leftPanelBounds.W - (_borderRadius * 2)), (int)(_leftPanelBounds.Z - (_borderRadius * 2)));
			spriteBatch.Draw(MainGame.SingleColor, well, Color.CornflowerBlue);
			_leftPanel.Draw(gameTime, spriteBatch);

			// Name Panel
			spriteBatch.Draw(MainGame.SingleColor, _namePanelBounds.ToRectangle(), Color.Black);
			well = new Rectangle((int)(_namePanelBounds.X + _borderRadius), (int)(_namePanelBounds.Y + _borderRadius),
			  (int)(_namePanelBounds.W - (_borderRadius * 2)), (int)(_namePanelBounds.Z - (_borderRadius * 2)));
			spriteBatch.Draw(MainGame.SingleColor, well, Color.CornflowerBlue);
			spriteBatch.DrawString(MainGame.PlainFont, _mapName, _namePosition, Color.Black);

			// Right Panel
			spriteBatch.Draw(MainGame.SingleColor, _rightPanelBounds.ToRectangle(), Color.Black);
			well = new Rectangle((int)(_rightPanelBounds.X + _borderRadius), (int)(_rightPanelBounds.Y + _borderRadius),
				(int)(_rightPanelBounds.W - (_borderRadius * 2)), (int)(_rightPanelBounds.Z - (_borderRadius * 2)));
			spriteBatch.Draw(MainGame.SingleColor, well, Color.CornflowerBlue);
			_rightPanel.Draw(gameTime, spriteBatch);

			//private Vector4 _namePanelBounds;
			//private Vector4 _leftPanelBounds;
			//private Vector4 _rightPanelBounds;
			//private Vector4 _levelBounds;
		}

		// We no longer move back when right clicking.
		public override void MoveBack() { }
		public override void HandleMouseEvent(GameTime gameTime, Vector2 newMousePosition)
		{
			_shortcutHandler.Update(this, gameTime);
			// If we are in the left panel
			if (newMousePosition.IsInBounds(_leftPanelBounds))
				CheckLeftPanelActions(newMousePosition);
			// else if we are in the right panel
			else if (newMousePosition.IsInBounds(_rightPanelBounds))
				CheckRightPanelActions(newMousePosition);
			// else if we are in the level editor
			else if (newMousePosition.IsInBounds(_levelBounds))
				CheckLevelPanelActions(newMousePosition);
			// else we are in the text panel
			else if (newMousePosition.IsInBounds(_namePanelBounds))
				CheckTextPanelActions(newMousePosition);
		}

		private void CheckLeftPanelActions(Vector2 mousePosition)
		{
			_leftPanel.Update(mousePosition);
			if (MenuStateManager.CurrentMouse.ScrollWheelValue > MenuStateManager.PreviousMouse.ScrollWheelValue)
				_leftPanel.ScrollUp();
			else if (MenuStateManager.CurrentMouse.ScrollWheelValue < MenuStateManager.PreviousMouse.ScrollWheelValue)
				_leftPanel.ScrollDown();


			if (MenuStateManager.IsClickingLeftMouse())
			{
				selectedItemToAdd = _leftPanel.Click(mousePosition, _level);
				if (selectedItemToAdd == null)
					_state = MapEditorState.Select;
				else
					_state = MapEditorState.AddNew;
			}
		}
		private void CheckRightPanelActions(Vector2 mousePosition)
		{
			_rightPanel.Update(mousePosition);
			if (MenuStateManager.IsClickingLeftMouse())
				_rightPanel.Click(mousePosition, _level);
		}
		private void CheckLevelPanelActions(Vector2 mousePosition)
		{
			//Fix Scaling issue later... least important now...
			if (MenuStateManager.CurrentMouse.ScrollWheelValue > MenuStateManager.PreviousMouse.ScrollWheelValue)
				ScaleCamera(() => Math.Min(3.0f, _scaleAmount + (_scaleAmount <= 1 ? .1f : .2f)));
			else if (MenuStateManager.CurrentMouse.ScrollWheelValue < MenuStateManager.PreviousMouse.ScrollWheelValue)
				ScaleCamera(() => Math.Max(.1f, _scaleAmount - (_scaleAmount <= 1 ? .1f : .2f)));

			if (MenuStateManager.IsClickingMiddleMouse())
			{
				_moveCameraAnchor = mousePosition / _scaleAmount;// -_levelBounds.GetPosition() + _cameraPosition;
				_moveCameraAnchor = new Vector2(_moveCameraAnchor.X - (_moveCameraAnchor.X % (_level.PlatformDifference / 4)),
													_moveCameraAnchor.Y - (_moveCameraAnchor.Y % (_level.PlatformDifference / 4)));
			}
			else if (MenuStateManager.IsHoldingMiddleMouse())
			{
				//Vector2 tempPosition = mousePosition;// -_levelBounds.GetPosition() + _cameraPosition;
				Vector2 tempPosition = mousePosition / _scaleAmount;
				tempPosition = new Vector2(tempPosition.X - (tempPosition.X % (_level.PlatformDifference / 4)),
													tempPosition.Y - (tempPosition.Y % (_level.PlatformDifference / 4)));
				_cameraPosition = (_cameraPosition + _moveCameraAnchor - tempPosition);
				_moveCameraAnchor = tempPosition;
			}
			if (MenuStateManager.IsClickingRightMouse())
				UndoAction();
			else
			{
				switch (_state)
				{
					case MapEditorState.AddNew:
						if (MenuStateManager.IsClickingLeftMouse())
						{
							Vector2 tempPosition = ((mousePosition - _levelBounds.GetPosition()) / _scaleAmount + _cameraPosition);
							tempPosition = NormalizePosition(tempPosition);
							if (selectedItemToAdd.ObjectType == GuiObjectType.Environment || selectedItemToAdd.ObjectType == GuiObjectType.Structure)
								selectedItemToAdd.Position = tempPosition;
							else
								selectedItemToAdd.Center = tempPosition + (NormalizePosition(selectedItemToAdd.Center) - selectedItemToAdd.Center);

							_level.AddGuiObject(selectedItemToAdd);
							//selectedItemToAdd = new Platform(Vector2.Zero, new Vector2(200, 20), Group.ImpassableIncludingMagic, _level);
						}
						else if (MenuStateManager.IsHoldingLeftMouse())
						{
							IsAlteringObject(selectedItemToAdd, mousePosition, selectedItemToAdd.ObjectType == GuiObjectType.Environment || selectedItemToAdd.ObjectType == GuiObjectType.Structure);
						}
						else if (MenuStateManager.IsReleasingLeftMouse() && FinishAlteringObject(selectedItemToAdd))
						{
							_undoStack.Push(new MapEditStep()
							{
								ItemId = selectedItemToAdd.Id,
								Group = selectedItemToAdd.Group,
								Type = selectedItemToAdd.ObjectType,
								State = _state,
								isCreating = true
							});
							selectedItemToAdd = _leftPanel.GetNewItem(_level);
						}
						break;
					case MapEditorState.Select:
						if (MenuStateManager.IsClickingLeftMouse())
						{
							Vector2 tempPosition = ((mousePosition - _levelBounds.GetPosition()) / _scaleAmount + _cameraPosition);
							_rightPanel.SelectNewItem(_level.GetGuiObjectAtPosition(tempPosition));
							if (_rightPanel.SelectedObject != null)
							{
								MainGuiObject mgo = _rightPanel.SelectedObject;
								float snapExaggerater = SnapTo * 1.6f;
								Vector4 isSizingBounds = new Vector4(mgo.Size + mgo.Position - new Vector2(snapExaggerater), snapExaggerater, snapExaggerater);
								_undoStack.Push(new MapEditStep()
								{
									ItemId = mgo.Id,
									Group = mgo.Group,
									Type = mgo.ObjectType,
									State = _state,
									isCreating = false,
									OldBounds = mgo.Bounds
								});
								_isResizing = tempPosition.IsInBounds(isSizingBounds);
								_mouseOffset = _isResizing ? (mgo.Size + mgo.Position) - tempPosition : mgo.Center - tempPosition;
							}
						}
						else if (MenuStateManager.IsHoldingLeftMouse())
						{
							IsAlteringObject(_rightPanel.SelectedObject, mousePosition, _isResizing, _mouseOffset);
						}
						else if (MenuStateManager.IsReleasingLeftMouse() && FinishAlteringObject(_rightPanel.SelectedObject)) { } // if it's releasing, then we finish altering the object.
						break;
				}
			}
		}
		private void ScaleCamera(Func<float> scaleFunction)
		{
			Vector2 middleOfLevelBounds = NormalizePosition((_levelBounds.GetSize() / 2.0f) / _scaleAmount);
			_cameraPosition = _cameraPosition + middleOfLevelBounds;
			_scaleAmount = scaleFunction();// Math.Max(.2f, _scaleAmount - .1f);
			_scaleMatrix = Matrix.CreateScale(_scaleAmount);
			middleOfLevelBounds = NormalizePosition((_levelBounds.GetSize() / 2.0f) / _scaleAmount);
			_cameraPosition = _cameraPosition - middleOfLevelBounds;
			_cameraPosition = new Vector2(_cameraPosition.X - (_cameraPosition.X % (_level.PlatformDifference / 4)),
												_cameraPosition.Y - (_cameraPosition.Y % (_level.PlatformDifference / 4)));
		}
		private void CheckTextPanelActions(Vector2 mousePosition)
		{
		}

		private void UndoAction()
		{
			if (_undoStack.Any())
			{
				MapEditStep lastStep = _undoStack.Pop();
				MainGuiObject lastItemUsed = _level.GetObject(lastStep.Type, lastStep.ItemId);
				if (lastStep.isCreating && lastItemUsed != null)
				{
					if (lastItemUsed.GetType().IsAssignableFrom(typeof(Player)))
						MainGame.PlayerManager.RemovePlayer(lastItemUsed.Id);
					_level.RemoveGuiObject(lastItemUsed);
				}
				else if (lastItemUsed != null)
				{
					lastItemUsed.Size = lastStep.OldBounds.GetSize();
					lastItemUsed.Position = lastStep.OldBounds.GetPosition();
				}
			}
			_rightPanel.DeselectItem();
		}

		private bool FinishAlteringObject(MainGuiObject mgo)
		{
			if (mgo == null)
				return false;
			bool didCreate = true;
			if (mgo.Size.X == 0 || mgo.Size.Y == 0)
			{
				_level.RemoveGuiObject(mgo);
				_rightPanel.DeselectItem();
				didCreate = false;
			}
			else
			{
				Vector2 newPosition = mgo.Position;
				Vector2 newSize = mgo.Size;
				if (mgo.Size.X < 0)
				{
					newPosition.X = mgo.Position.X + mgo.Size.X;
					newSize.X = Math.Abs(mgo.Size.X);
				}
				if (mgo.Size.Y < 0)
				{
					newPosition.Y = mgo.Position.Y + mgo.Size.Y;
					newSize.Y = Math.Abs(mgo.Size.Y);
				}
				mgo.Position = newPosition;
				mgo.Size = newSize;
				_rightPanel.SelectNewItem(mgo);
			}
			return didCreate;
		}
		private void IsAlteringObject(MainGuiObject mgo, Vector2 mousePosition, bool isAlteringSize)
		{
			IsAlteringObject(mgo, mousePosition, isAlteringSize, Vector2.Zero);
		}
		private void IsAlteringObject(MainGuiObject mgo, Vector2 mousePosition, bool isAlteringSize, Vector2 mouseOffset)
		{
			if (mgo == null)
				return;
			Vector2 tempPosition = NormalizePosition(((mousePosition - _levelBounds.GetPosition()) / _scaleAmount + _cameraPosition) + mouseOffset);

			if (isAlteringSize)
				mgo.Size = tempPosition - mgo.Position;
			else
				mgo.Center = tempPosition + (mgo.Center - NormalizePosition(mgo.Center));
		}

		public void SwitchState(MapEditorState mapEditorState)
		{
			_leftPanel.SwitchState(mapEditorState);
			_state = mapEditorState;
		}
		public void SelectItem(MainGuiObject mgo)
		{
			selectedItemToAdd = mgo;
		}
		private Vector2 NormalizePosition(Vector2 levelPosition)
		{
			return new Vector2(levelPosition.X - (levelPosition.X % SnapTo),
												levelPosition.Y - (levelPosition.Y % SnapTo));
		}
	}
}
