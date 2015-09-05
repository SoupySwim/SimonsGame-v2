using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Menu.InGame;
using SimonsGame.GuiObjects;

namespace SimonsGame.Menu.MenuScreens
{
	public enum ExperienceGainChoice
	{
		Early,
		Late,
	}

	public enum SelfUpgradeChoice
	{
		None,
		Speed,
		Health,
	}

	public enum BaseAttackChoice
	{
		Melee,
		ShortRange,
	}

	public class StartupChoiceMenu : InGameScreen
	{
		private Vector4 _overlayBounds;

		public bool IsReady = false;

		public ExperienceGainChoice SelectedExperienceGain = ExperienceGainChoice.Early;
		public SelfUpgradeChoice SelectedSelfUpgrade = SelfUpgradeChoice.None;
		public BaseAttackChoice SelectedBaseAttack = BaseAttackChoice.Melee;
		private bool _isMouseAndKeyboard = false;

		public StartupChoiceMenu(Player player, Vector4 overlayBounds)
			: base(null)
		{
			_overlayBounds = overlayBounds;


			string[] expNames = Enum.GetNames(typeof(ExperienceGainChoice));
			string[] selfNames = Enum.GetNames(typeof(SelfUpgradeChoice));
			string[] baseNames = Enum.GetNames(typeof(BaseAttackChoice));

			_menuLayout = new MenuItemButton[4][];
			_menuLayout[0] = new MenuItemButton[expNames.Count()];
			_menuLayout[1] = new MenuItemButton[selfNames.Count()];
			_menuLayout[2] = new MenuItemButton[baseNames.Count()];
			_menuLayout[3] = new MenuItemButton[1]; // Ready Button

			float offsetInterval = overlayBounds.Z / (_menuLayout.Count() + 2);
			float yOffset = overlayBounds.Y + offsetInterval;

			float buttonWidth = 250;

			for (int choiceNdx = 0; choiceNdx < 3; choiceNdx++)
			{
				string[] names = null;
				if (choiceNdx == 0)
					names = expNames;
				else if (choiceNdx == 1)
					names = selfNames;
				else if (choiceNdx == 2)
					names = baseNames;

				float buttonsWidth = buttonWidth * names.Count();
				Vector4 totalBounds = new Vector4(overlayBounds.X + (overlayBounds.W / 2) - (buttonsWidth / 2) - buttonWidth, yOffset, 50, buttonWidth);
				for (int i = 0; i < names.Count(); i++)
				{
					string text = names[i];
					totalBounds.X = totalBounds.X + buttonWidth;
					var sizeAndPad = text.GetSizeAndPadding(MainGame.PlainFont, totalBounds);
					Vector4 textBounds = sizeAndPad.Item1;
					Vector2 padding = sizeAndPad.Item2;
					int currentChoice = choiceNdx;
					_menuLayout[choiceNdx][i] = new TextMenuItemButton(() =>
					{
						foreach (MenuItemButton button in _menuLayout[currentChoice])
						{
							TextMenuItemButton textButton = button as TextMenuItemButton;
							if (textButton != null)
							{
								if (textButton.Text == text)
								{
									textButton.DefaultColor = Color.Lerp(Color.LightBlue, Color.White, .3f);
									textButton.SelectedColor = Color.LightBlue;
								}
								else
								{
									textButton.DefaultColor = Color.Black;
									textButton.SelectedColor = Color.White;
									textButton.OverrideColor(Color.Black);
								}
							}
						}
						if (currentChoice == 0)
							SelectedExperienceGain = text.ToEnum<ExperienceGainChoice>();
						else if (currentChoice == 1)
							SelectedSelfUpgrade = text.ToEnum<SelfUpgradeChoice>();
						else if (currentChoice == 2)
							SelectedBaseAttack = text.ToEnum<BaseAttackChoice>();

					}, text, textBounds, i == 0 ? Color.Lerp(Color.LightBlue, Color.White, .3f) : Color.Black, i == 0 ? Color.LightBlue : Color.White, padding, false);
				}

				yOffset += offsetInterval;
			}

			yOffset += offsetInterval;

			float readyButtonsWidth = buttonWidth * baseNames.Count();
			Vector4 readyTotalBounds = new Vector4(overlayBounds.X + (overlayBounds.W / 2) - (readyButtonsWidth / 2), yOffset, 50, readyButtonsWidth);
			string readyText = "Ready";
			var readySizeAndPad = readyText.GetSizeAndPadding(MainGame.PlainFont, readyTotalBounds);
			Vector4 readyTextBounds = readySizeAndPad.Item1;
			Vector2 readyPadding = readySizeAndPad.Item2;
			_menuLayout[3][0] = new TextMenuItemButton(() => { IsReady = true; }, readyText, readyTextBounds, Color.Black, Color.White, readyPadding, false);

			_isMouseAndKeyboard = player.UsesMouseAndKeyboard;
		}
		public override void HandleMouseEvent(GameTime gameTime, Vector2 newMousePosition)
		{
			if (_isMouseAndKeyboard)
				base.HandleMouseEvent(gameTime, newMousePosition);
		}

		protected override void DrawExtra(GameTime gameTime, SpriteBatch spriteBatch)
		{
			base.DrawExtra(gameTime, spriteBatch);
		}
	}
}