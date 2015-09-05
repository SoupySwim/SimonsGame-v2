using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class Platform : MainGuiObject
	{
		private Texture2D _background;

		private float _projectedWidth;
		private float _projectedHeight;
		private float _heightCap;
		private int _repeatXCount;
		private bool _isTeamPlatform = false;

		#region Hidden Area
		private bool _isHiddenArea = false;
		private List<Team> _visibleToTeams = new List<Team>();
		#endregion

		#region Drop Logic
		private bool _doesDrop;
		private float _yOffsetDueToDrop = 0;
		private float _xOffsetDueToDrop = 0;
		private int _dropTimeTotal;
		private int _dropTimeCurrent;
		private int _respawnTimeTotal = -1;
		private int _respawnTimeCurrent = 0;
		private Group _storeGroup;
		#endregion

		public Platform(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Platform")
		{
			AdditionalGroupChange(group, group);
			_background = MainGame.ContentManager.Load<Texture2D>("Test/Platform");
			ExtraSizeManipulation(ref hitbox);
			_doesDrop = false;
			_dropTimeTotal = 30;
			_dropTimeCurrent = 0;
			_respawnTimeTotal = -1;
		}
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }

		public override void PreUpdate(GameTime gameTime) { }
		public override void PostUpdate(GameTime gameTime)
		{
			if (_doesDrop)
			{
				if (_dropTimeCurrent > 0)
				{
					_dropTimeCurrent++;
					if (_dropTimeCurrent >= _dropTimeTotal)
					{
						_respawnTimeCurrent = 1;
						_dropTimeCurrent = 0;
						if (_respawnTimeTotal == -1)
							Level.RemoveGuiObject(this);
						else // We will respawn soon.
						{
							_storeGroup = Group;
							Group = Group.Passable;
						}
					}
					else
					{
						if (_dropTimeCurrent % 5 == 0)
							_xOffsetDueToDrop *= -1;
						if (_dropTimeCurrent % 6 == 0)
							_yOffsetDueToDrop += 1;
					}
				}
				else if (_respawnTimeCurrent > 0)
				{
					_respawnTimeCurrent++;
					if (_respawnTimeCurrent >= _respawnTimeTotal)
					{
						Group = _storeGroup;
						_respawnTimeCurrent = 0;
						_yOffsetDueToDrop = 0;
						_xOffsetDueToDrop = 0;
					}
				}
			}


			if (_isHiddenArea)
			{
				_visibleToTeams = Level.GetAllCharacterObjects(Bounds).Where(mgo => mgo.Group != Group.Passable && mgo.GetIntersectionDepth(this) != Vector2.Zero).Select(mgo => mgo.Team).Distinct().ToList();
			}
		}
		public override void PreDraw(GameTime gameTime, SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, SpriteBatch spriteBatch, Player curPlayer)
		{
			if (_respawnTimeCurrent > 0) // If we aren't currently drawn on screen, return!
				return;
			Rectangle rect = new Rectangle(0, 0, 0, 0);
			Color drawColor = _hitBoxColor;
			if ((curPlayer != null && _visibleToTeams.Contains(curPlayer.Team)) || (_isHiddenArea && MainGame.GameState != MainGame.MainGameState.Game))
				drawColor = Color.Lerp(_hitBoxColor, Color.Transparent, .75f);
			int multX = Size.X < 0 ? -1 : 1;
			int multY = Size.Y < 0 ? -1 : 1;
			int addX = Size.X < 0 ? (int)-_projectedWidth : 0;
			int addY = Size.Y < 0 ? (int)-_projectedWidth : 0;
			for (float h = 0; h < _heightCap; h += _projectedHeight)
				for (int w = 0; w < _repeatXCount; w++)
				{
					rect.X = (int)(Position.X + +_xOffsetDueToDrop + addX + multX * w * _projectedWidth);
					rect.Y = (int)(Position.Y + _yOffsetDueToDrop + addY + multY * h);
					rect.Width = (int)(_projectedWidth);
					rect.Height = (int)(_projectedHeight);
					spriteBatch.Draw(_background, rect, drawColor);
				}
			//spriteBatch.Draw(_background, Position + new Vector2(w * projectedHeight, h * projectedHeight), _hitBoxColor);
		}

		public override void ExtraSizeManipulation(ref Vector2 newSize)
		{
			float sizeX = Math.Abs(newSize.X);
			float sizeY = Math.Abs(newSize.Y);
			_projectedHeight = (int)(sizeY / _background.Height);
			int remainder = (int)(sizeY % _background.Height);
			if (_projectedHeight == 0 || remainder >= _background.Height / 2)
				_projectedHeight++;
			_projectedHeight = sizeY / _projectedHeight;

			_repeatXCount = (int)(sizeX / _projectedHeight);
			remainder = (int)(sizeX % _projectedHeight);
			if (remainder >= _projectedHeight / 2)
				_repeatXCount++;
			_projectedWidth = sizeX / _repeatXCount;


			_heightCap = sizeY - _projectedHeight / 2;
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			if (_doesDrop && _dropTimeCurrent == 0 && Group != Group.Passable)
			{
				_dropTimeCurrent = 1;
				_xOffsetDueToDrop = 2;
			}
		}
		protected override void AdditionalGroupChange(Group _group, Group newGroup)
		{
			ChangePlatformColorBasedOnGroup(newGroup);
			base.AdditionalGroupChange(_group, newGroup);
		}
		private void ChangePlatformColorBasedOnGroup(Group newGroup)
		{
			if (!_isTeamPlatform)
			{
				DrawImportant = 0;
				switch (newGroup)
				{
					case Group.ImpassableIncludingMagic:
						_hitBoxColor = Color.SandyBrown;
						break;
					case Group.Impassable:
						_hitBoxColor = Color.Khaki;
						break;
					default:
						_hitBoxColor = Color.Wheat;
						break;
				}
			}
			if (_isHiddenArea)
			{
				DrawImportant = 10;
				_hitBoxColor = Color.Lerp(Color.CornflowerBlue, Color.Black, .15f);
			}
		}
		protected override bool ShowHitBox() { return false; }

		public override bool IsHitBy(MainGuiObject mgo)
		{
			return !_isHiddenArea && (!_isTeamPlatform || mgo.Team != Team);
		}

		public override void SwitchTeam(Team newTeam)
		{
			_team = newTeam;
			if (_isTeamPlatform)
			{
				DrawImportant = 1;
				_hitBoxColor = Color.Lerp(TeamColorMap[newTeam], Color.Transparent, .4f);
			}

			if (_isHiddenArea)
			{
				DrawImportant = 10;
				_hitBoxColor = Color.Lerp(Color.CornflowerBlue, Color.Black, .15f);
			}
		}

		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Dropping";
			if (bType == ButtonType.SpecialToggle2)
				return "Respawn Time";
			if (bType == ButtonType.SpecialToggle3)
				return "Team Platform";
			if (bType == ButtonType.SpecialToggle4)
				return "IsBushes";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _doesDrop ? "Yes" : "No";
			if (bType == ButtonType.SpecialToggle2)
				return _respawnTimeTotal == -1 ? "Never" : (_respawnTimeTotal / 60).ToString();
			if (bType == ButtonType.SpecialToggle3)
				return _isTeamPlatform ? "Yes" : "No";
			if (bType == ButtonType.SpecialToggle4)
				return _isHiddenArea ? "Yes" : "No";
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				_doesDrop = !_doesDrop;
			if (bType == ButtonType.SpecialToggle2)
			{
				if (_respawnTimeTotal < 0)
					_respawnTimeTotal = moveRight ? 300 : 3600;
				else if (!moveRight && _respawnTimeTotal == 300)
					_respawnTimeTotal = -1;
				else if (moveRight && _respawnTimeTotal == 3600)
					_respawnTimeTotal = -1;
				else
					_respawnTimeTotal = MathHelper.Clamp(_respawnTimeTotal + (moveRight ? 300 : -300), 300, 3600);
			}
			if (bType == ButtonType.SpecialToggle3)
			{
				_isTeamPlatform = !_isTeamPlatform;
				if (!_isTeamPlatform)
					ChangePlatformColorBasedOnGroup(Group);
				else
					SwitchTeam(Team);
			}
			if (bType == ButtonType.SpecialToggle4)
			{
				_isHiddenArea = !_isHiddenArea;
				if (_isHiddenArea)
				{
					DrawImportant = 10;
					_hitBoxColor = Color.Lerp(Color.CornflowerBlue, Color.Black, .15f);
				}
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _doesDrop ? 1 : 0;
			if (bType == ButtonType.SpecialToggle2)
				return _respawnTimeTotal;
			if (bType == ButtonType.SpecialToggle3)
				return _isTeamPlatform ? 1 : 0;
			if (bType == ButtonType.SpecialToggle4)
				return _isHiddenArea ? 1 : 0;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_doesDrop = value == 1;
			if (bType == ButtonType.SpecialToggle2)
				_respawnTimeTotal = value;
			if (bType == ButtonType.SpecialToggle3)
			{
				_isTeamPlatform = value == 1;
				if (!_isTeamPlatform)
					ChangePlatformColorBasedOnGroup(Group);
				else
					SwitchTeam(Team);
			}
			if (bType == ButtonType.SpecialToggle4)
			{
				_isHiddenArea = value == 1;
				if (_isHiddenArea)
				{
					DrawImportant = 10;
					_hitBoxColor = Color.Lerp(Color.CornflowerBlue, Color.Black, .15f);
					Group = Group.Passable;
				}
			}
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}
