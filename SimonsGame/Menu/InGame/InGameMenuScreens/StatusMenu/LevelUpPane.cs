using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Menu.InGame
{
	public class LevelUpPane : InGameMenuPartialView
	{
		private SelectedPane _parent;
		private LevelUpNode SelectedNode;
		private string SelectedMagicName;
		private float _upgradeHeight;
		private float _upgradeYOffset;
		private float _magicHeight;
		public Dictionary<LevelUpNode, Rectangle> LevelUpNodes;
		private Dictionary<string, LevelUpNodeContainer> _magicNames;
		private Vector2 _lineStart;
		private Vector2 _lineEnd;

		public LevelUpPane(Vector4 bounds, SelectedPane parent, List<LevelUpNode> upgradeNodes)
			: base(bounds)
		{
			_magicHeight = bounds.Z * .1f;
			_upgradeHeight = bounds.Z * .9f;
			_upgradeYOffset = bounds.Y + _magicHeight;
			_lineStart = new Vector2(bounds.X, _upgradeYOffset);
			_lineEnd = new Vector2(bounds.X + bounds.W, _upgradeYOffset);

			int magicIndex = 0;
			int magicWidth = (int)(bounds.W / upgradeNodes.Count());
			_magicNames = upgradeNodes.ToDictionary(un => un.MagicName, un => new LevelUpNodeContainer(un, new Rectangle((int)(Bounds.X + 10 + (magicIndex++ * magicWidth)), (int)(Bounds.Y + 10), magicWidth - 20, (int)(_magicHeight - 20))));

			//LevelUpNode baseNode = new LevelUpNode(LevelUpType.Health, 0); // So I can back out all the way when moving up/down
			//baseNode.AlreadyPurchased = true;
			//baseNode.ChildNodes.AddRange(upgradeNodes);
			//foreach (LevelUpNode node in upgradeNodes)
			//	node.ParentNode = baseNode;

			SelectedNode = upgradeNodes.First();
			SelectedMagicName = SelectedNode.MagicName;

			_parent = parent;
			LevelUpNodes = new Dictionary<LevelUpNode, Rectangle>();
			foreach (LevelUpNode node in upgradeNodes)
			{
				CreateTier(new List<LevelUpNode>() { node }, 0);
			}
			//CreateTier(upgradeNodes, 0);
		}
		private void AddMagicAbility(LevelUpNode node)
		{
			int magicIndex = 0;
			int magicWidth = (int)(Bounds.W / (_magicNames.Count() + 1));
			foreach (var kv in _magicNames)
			{
				LevelUpNodeContainer container = kv.Value;
				container.Rectangle.X = (int)(Bounds.X + 10 + (magicIndex++ * magicWidth));
				container.Rectangle.Width = magicWidth - 20;
			}
			CreateTier(new List<LevelUpNode>() { node }, 0);
			_magicNames.Add(node.MagicName, new LevelUpNodeContainer(node, new Rectangle((int)(Bounds.X + 10 + (magicIndex++ * magicWidth)), (int)(Bounds.Y + 10), magicWidth - 20, (int)(_magicHeight - 20))));

		}
		private void CreateTier(List<LevelUpNode> nodes, int tierLevel)
		{
			//List<LevelUpNode> nextTier = new List<LevelUpNode>();
			//float yNdx = 0;
			//float yTotal = nodes.Count();
			//foreach (LevelUpNode node in nodes)
			//{
			//	nextTier.AddRange(node.ChildNodes);
			//	LevelUpNodes.Add(node, new Rectangle((int)(Bounds.X + 10 + (tierLevel * 130)), (int)(Bounds.Y + 10 + ((yNdx / yTotal) * Bounds.Z)), 80, 36));
			//	yNdx++;
			//}
			//if (nextTier.Any())
			//	CreateTier(nextTier, tierLevel + 1);

			List<LevelUpNode> nextTier = new List<LevelUpNode>();
			float yNdx = 0;
			float yTotal = nodes.Count();
			if (tierLevel == 0)
			{
				yNdx++;
				yTotal++;
			}
			foreach (LevelUpNode node in nodes)
			{
				nextTier.AddRange(node.ChildNodes);
				LevelUpNodes.Add(node, new Rectangle((int)(Bounds.X + 10 + (tierLevel * 200)), (int)(_upgradeYOffset + 10 + ((yNdx / yTotal) * _upgradeHeight)), 150, 90));
				yNdx++;
			}
			if (nextTier.Any())
				CreateTier(nextTier, tierLevel + 1);
		}

		public override void Update(GameTime gameTime, Vector2 newMousePosition)
		{

			if (IsGoBack())
				_parent.IsLevelUpMode = false;

			// If you are buying, and you've purchased its parent, but not it, then you can buy it!
			if (IsAction() && !SelectedNode.AlreadyPurchased && (SelectedNode.ParentNode == null || SelectedNode.ParentNode.AlreadyPurchased) && SelectedNode.ExperienceCost <= _parent.Player.AbilityManager.Experience)
			{
				UnlockMagicNode unlockMagic = SelectedNode.ApplyUpgrade(_parent.Player);
				if (unlockMagic != null)
					AddMagicAbility(unlockMagic.LevelUpNode);
			}

			if (_parent.Player.UsesMouseAndKeyboard && Controls.PreviousMouse.Position != Controls.CurrentMouse.Position)
			{
				if (newMousePosition.Y < _upgradeYOffset)
				{
					foreach (var kv in _magicNames)
					{
						if (kv.Value.Rectangle.X < newMousePosition.X && kv.Value.Rectangle.X + kv.Value.Rectangle.Width > newMousePosition.X
							&& kv.Value.Rectangle.Y < newMousePosition.Y && kv.Value.Rectangle.Y + kv.Value.Rectangle.Height > newMousePosition.Y)
						{
							SelectedMagicName = kv.Key;
							break;
						}
					}
				}
				else if (SelectedMagicName != "")
				{
					foreach (KeyValuePair<LevelUpNode, Rectangle> kv in LevelUpNodes)
					{
						if (kv.Key.MagicName == SelectedMagicName
							&& kv.Value.X < newMousePosition.X && kv.Value.X + kv.Value.Width > newMousePosition.X
							&& kv.Value.Y < newMousePosition.Y && kv.Value.Y + kv.Value.Height > newMousePosition.Y)
						{
							SelectedNode = kv.Key;
							break;
						}
					}
				}
			}
			else if (!_parent.Player.UsesMouseAndKeyboard)
			{
				// If using controller, then check for bumpers
				if (Controls.PressedDown(_parent.Player.Id, AvailableButtons.LeftBumper))
					SelectNextMagic(true);
				if (Controls.PressedDown(_parent.Player.Id, AvailableButtons.RightBumper))
					SelectNextMagic(false);
			}

		}

		private void SelectNextMagic(bool reverse)
		{
			bool isFound = false;
			bool didMatch = false;
			foreach (string magicName in (reverse ? _magicNames.Keys.Reverse() : _magicNames.Keys))
			{
				if (isFound)
				{
					SelectedMagicName = magicName;
					SelectedNode = _magicNames[magicName].LevelUpNode;
					didMatch = true;
					break;
				}
				else if (SelectedMagicName == magicName)
				{
					isFound = true;
				}
			}
			if (!didMatch)
			{
				SelectedMagicName = reverse ? _magicNames.Keys.Last() : _magicNames.Keys.First();
				SelectedNode = _magicNames[SelectedMagicName].LevelUpNode;
			}
		}

		private bool IsGoBack()
		{
			return Controls.PressedDown(Controls.AllControls[_parent.Player.Id], Controls.PreviousControls[_parent.Player.Id], AvailableButtons.Secondary)
				|| (_parent.Player.UsesMouseAndKeyboard && Controls.IsClickingRightMouse());
		}

		private bool IsAction()
		{
			return Controls.PressedDown(Controls.AllControls[_parent.Player.Id], Controls.PreviousControls[_parent.Player.Id], AvailableButtons.Action)
				|| (_parent.Player.UsesMouseAndKeyboard && Controls.IsClickingLeftMouse());
		}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(MainGame.SingleColor, Bounds.ToRectangle(), Color.White);


			foreach (var kv in _magicNames)
			{
				spriteBatch.Draw(MainGame.SingleColor, kv.Value.Rectangle, Color.Lerp(SelectedMagicName == kv.Key ? Color.Gold : Color.Gray, Color.White, .2f));
				Vector4 nameBounds = kv.Key.ToString().GetTextBoundsByCenter(MainGame.PlainFontSmall, new Vector2(kv.Value.Rectangle.X + kv.Value.Rectangle.Width / 2.0f, kv.Value.Rectangle.Y + kv.Value.Rectangle.Height / 2.0f));
				spriteBatch.DrawString(MainGame.PlainFontSmall, kv.Key.ToString(), nameBounds.GetPosition(), Color.Black);
			}

			spriteBatch.DrawLine(_lineStart, _lineEnd, Color.Black);

			// draw own experience.
			spriteBatch.DrawString(MainGame.PlainFont, _parent.Player.AbilityManager.Experience.ToString(), new Vector2(Bounds.X + Bounds.W - 22, Bounds.Y + 2), Color.Black);

			if (SelectedMagicName != "")
			{
				// Draw the current magic.
				Vector2 placeholderStart = Vector2.Zero;
				Vector2 placeholderEnd = Vector2.Zero;

				foreach (var kv in LevelUpNodes)
				{
					if (kv.Key.MagicName == SelectedMagicName)
					{
						spriteBatch.Draw(MainGame.SingleColor, kv.Value, Color.Lerp(SelectedNode == kv.Key ? Color.Gold : Color.Gray, kv.Key.AlreadyPurchased ? Color.Black : Color.White, .2f));
						spriteBatch.DrawString(MainGame.PlainFontSmall, kv.Key.ExperienceCost.ToString(), new Vector2(kv.Value.X + kv.Value.Width - 22, kv.Value.Y + 2), Color.Black);
						Vector4 nameBounds = kv.Key.Name.ToString().GetTextBoundsByCenter(MainGame.PlainFontSmall, new Vector2(kv.Value.X + kv.Value.Width / 2.0f, kv.Value.Y + kv.Value.Height / 2.0f));
						spriteBatch.DrawString(MainGame.PlainFont, kv.Key.Name.ToString(), nameBounds.GetPosition(), Color.Black);

						placeholderStart.X = kv.Value.X + kv.Value.Width;
						placeholderStart.Y = kv.Value.Y + kv.Value.Height / 2.0f;

						foreach (LevelUpNode child in kv.Key.ChildNodes)
						{
							Rectangle endRectangle = LevelUpNodes[child];
							placeholderEnd.X = endRectangle.X;
							placeholderEnd.Y = endRectangle.Y + endRectangle.Height / 2.0f;
							spriteBatch.DrawLine(placeholderStart, placeholderEnd, Color.Black);
						}
					}
				}
			}
		}

		public override bool MoveDown()
		{
			SelectedNode = MoveDownRecursive(SelectedNode);
			return true;
		}

		public override bool MoveUp()
		{

			SelectedNode = MoveUpRecursive(SelectedNode);
			return true;
		}

		private LevelUpNode MoveDownRecursive(LevelUpNode currentNode)
		{
			if (currentNode.ParentNode == null)
				return currentNode;
			LevelUpNode parentNode = currentNode.ParentNode;
			int index = parentNode.ChildNodes.FindIndex(n => n == currentNode);
			if (parentNode.ChildNodes.Count() > index + 1)
				return parentNode.ChildNodes.ElementAt(index + 1);
			LevelUpNode nextNode = MoveDownRecursive(parentNode);
			if (!nextNode.ChildNodes.Any())
				return nextNode;
			return nextNode.ChildNodes.First();
		}

		private LevelUpNode MoveUpRecursive(LevelUpNode currentNode)
		{
			if (currentNode.ParentNode == null)
				return currentNode;
			LevelUpNode parentNode = currentNode.ParentNode;
			int index = parentNode.ChildNodes.FindIndex(n => n == currentNode);
			if (0 < index)
				return parentNode.ChildNodes.ElementAt(index - 1);
			LevelUpNode nextNode = MoveUpRecursive(parentNode);
			if (!nextNode.ChildNodes.Any())
				return nextNode;
			return nextNode.ChildNodes.Last();
		}


		public override bool MoveLeft()
		{
			if (SelectedNode.ParentNode != null)
			{
				SelectedNode = SelectedNode.ParentNode;
				return true;
			}
			_parent.IsLevelUpMode = false;
			return false;
		}

		public override bool MoveRight()
		{
			SelectedNode = SelectedNode.ChildNodes.Any() ? SelectedNode.ChildNodes.First() : SelectedNode;
			return true;
		}

		public override void HasBeenHighlighted()
		{
		}

	}

	public class LevelUpNodeContainer
	{
		public Rectangle Rectangle;
		public LevelUpNode LevelUpNode;
		public LevelUpNodeContainer(LevelUpNode node, Rectangle rect)
		{
			Rectangle = rect;
			LevelUpNode = node;
		}
	}

	public class LevelUpNode
	{
		public LevelUpType TypeOfUpgrade;
		public LevelUpMagicType MagicType;
		public LevelUpMagicAbility MagicAbility;
		public bool RemoveSiblings;
		public int ExperienceCost;
		public string MagicName;
		public string Name;
		public float Amount;
		public int SpecialType;
		public Element Element;
		public List<LevelUpNode> ChildNodes;
		public List<LevelUpNode> AdditionalNodes;
		public LevelUpNode ParentNode;
		public bool AlreadyPurchased;
		//public LevelUpNode ParentNode;

		public LevelUpNode(LevelUpType typeOfUpgrade, int experienceCost) : this(typeOfUpgrade, experienceCost, new List<LevelUpNode>()) { }

		public LevelUpNode(LevelUpType typeOfUpgrade, int experienceCost, List<LevelUpNode> additionalNodes)
		{
			TypeOfUpgrade = typeOfUpgrade;
			ExperienceCost = experienceCost;
			AdditionalNodes = additionalNodes;
			ChildNodes = new List<LevelUpNode>();
			AlreadyPurchased = false;
			SpecialType = -1;
			RemoveSiblings = false;
		}

		public UnlockMagicNode ApplyUpgrade(PhysicsObject mgo)
		{
			UnlockMagicNode newAbilityUnlock = null;

			AlreadyPurchased = true;
			if (TypeOfUpgrade == LevelUpType.Magic)
				LevelUpMagic(mgo);
			else if (TypeOfUpgrade == LevelUpType.NewAbility)
				LevelUpNewAbility(mgo);
			else if (TypeOfUpgrade == LevelUpType.Health)
				mgo.SetHealthTotal(mgo.HealthTotal + Amount); // For now... I'm thinking it has to be a percentage...
			else if (TypeOfUpgrade == LevelUpType.Mana)
				mgo.ManaTotal += Amount; // For now... I'm thinking it has to be a percentage...
			else if (TypeOfUpgrade == LevelUpType.Regen)
				mgo.RegenAmount += Amount;
			else if (TypeOfUpgrade == LevelUpType.Speed)
				mgo.MaxSpeedBase = new Vector2(mgo.MaxSpeedBase.X + Amount, mgo.MaxSpeedBase.Y);
			else if (TypeOfUpgrade == LevelUpType.UpgradeMagicNode)
				newAbilityUnlock = this as UnlockMagicNode;


			if (newAbilityUnlock != null)
				newAbilityUnlock.LevelUpNode.ApplyUpgrade(mgo);
			else // Buy the upgrade as normal.
				mgo.AbilityManager.Experience -= ExperienceCost;

			// Apply the associated upgrades as well.
			foreach (LevelUpNode lun in AdditionalNodes)
				lun.ApplyUpgrade(mgo);
			return newAbilityUnlock;
		}

		private void LevelUpMagic(PhysicsObject mgo)
		{
			foreach (var id in mgo.AbilityManager.KnownAbilityIds)
			{
				PlayerAbilityInfo pai = mgo.AbilityManager.GetAbilityInfo(id);
				// Only go until a match is found.
				if (pai.Name == MagicName)
				{
					LevelUpMagicByType(mgo, pai);
					break; // This will be a time saver...
				}
			}
		}

		private void LevelUpMagicByType(PhysicsObject mgo, PlayerAbilityInfo pai)
		{
			if (MagicType == LevelUpMagicType.Damage)
			{
				pai.Modifier.LevelUpMagicDamage(Amount);
			}
			else if (MagicType == LevelUpMagicType.Speed)
			{
				pai.Modifier.LevelUpMagicSpeed(Amount);
			}
			else if (MagicType == LevelUpMagicType.NewAbilityAttribute)
			{
				AbilityAttributes newAbilities = (AbilityAttributes)Amount; // unsure if this will work with multiple abilities...
				pai.Modifier.LevelUpMagicAddAbilities(newAbilities);
			}
			else if (MagicType == LevelUpMagicType.NewCustomAbility)
			{
				if (MagicAbility == LevelUpMagicAbility.Slow)
				{
					pai.Modifier.LevelUpMagicSpeedManipulation(Amount);
				}
				else if (MagicAbility == LevelUpMagicAbility.SlowTime)
				{
					pai.Modifier.LevelUpMagicSpeedManipulationTime((int)Amount);
				}
				else if (MagicAbility == LevelUpMagicAbility.KnockBack)
				{
					pai.Modifier.LevelUpMagicKnockback(Amount);
				}
				else if (MagicAbility == LevelUpMagicAbility.Stun)
				{
					pai.Modifier.LevelUpMagicStunTimer((int)Amount);
				}
			}
			else if (MagicType == LevelUpMagicType.Special)
			{
				pai.Modifier.LevelUpSpecial(SpecialType, Amount);
			}

		}

		private void LevelUpNewAbility(PhysicsObject mgo)
		{
			var thing = AbilityBuilder.GetBaseAbilityFromElement(mgo, Element, MagicName);
			mgo.AbilityManager.AddKnownAbility(KnownAbility.Elemental, thing);
		}

	}

	public class UnlockMagicNode : LevelUpNode
	{
		public LevelUpNode LevelUpNode;

		public UnlockMagicNode(string name, LevelUpNode levelUpNode)
			: base(LevelUpType.UpgradeMagicNode, levelUpNode == null ? 0 : levelUpNode.ExperienceCost)
		{
			MagicName = "Upgrades";
			Name = name;
			LevelUpNode = levelUpNode;
		}
	}

	public enum LevelUpType
	{
		Magic,
		NewAbility,
		Health,
		Regen,
		Mana,
		Speed,
		UpgradeMagicNode
	}

	public enum LevelUpMagicType
	{
		Damage,
		Speed,
		NewAbilityAttribute,
		NewCustomAbility,
		Special
	}

	public enum LevelUpMagicAbility
	{
		Slow,
		SlowTime,
		KnockBack,
		Stun,
	}

}
