using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class PowerUpBuilder
	{
		public static PowerUp GetHealthPackPU(Vector2 position, Vector2 size, Level level)
		{
			TickModifier modifier = new TickModifier(1, ModifyType.Add, null, new Tuple<Element, float>(Element.Fire, .3f));
			modifier.SetHealthTotal(500);
			Texture2D healthPack = MainGame.ContentManager.Load<Texture2D>("Test/HealthPack");
			Animation animation = new Animation(healthPack, 1, false, healthPack.Bounds.Width, healthPack.Bounds.Height, new Vector2(size.X / healthPack.Bounds.Width, size.Y / healthPack.Bounds.Height));
			PowerUp pUp = new PowerUp(position, size, level, animation, modifier, PowerUpType.HealthPack);
			return pUp;
		}
		public static PowerUp GetSpeedUpPU(Vector2 position, Vector2 size, Level level)
		{
			TickModifier modifier = new TickModifier(120, ModifyType.Multiply, null, new Tuple<Element, float>(Element.Fire, .3f));
			modifier.Movement = new Vector2(1.75f, 1f);
			modifier.MaxSpeed = new Vector2(1.75f, 1f);
			Texture2D healthPack = MainGame.ContentManager.Load<Texture2D>("Test/SingleColor");
			Animation animation = new Animation(healthPack, 1, false, healthPack.Bounds.Width, healthPack.Bounds.Height, new Vector2(size.X / healthPack.Bounds.Width, size.Y / healthPack.Bounds.Height));
			PowerUp pUp = new PowerUp(position, size, level, animation, modifier, PowerUpType.SuperSpeed);
			return pUp;
		}
		public static PowerUp GetSuperJumpPU(Vector2 position, Vector2 size, Level level)
		{
			TickModifier modifier = new TickModifier(300, ModifyType.Multiply, null, new Tuple<Element, float>(Element.Fire, .3f));
			modifier.Movement = new Vector2(1f, 2f);
			Texture2D healthPack = MainGame.ContentManager.Load<Texture2D>("Test/SingleColor");
			Animation animation = new Animation(healthPack, 1, false, healthPack.Bounds.Width, healthPack.Bounds.Height, new Vector2(size.X / healthPack.Bounds.Width, size.Y / healthPack.Bounds.Height));
			PowerUp pUp = new PowerUp(position, size, level, animation, modifier, PowerUpType.SuperJump);
			return pUp;
		}

		public static AbilityObject GetBlinkAbilityObject(Vector2 position, Vector2 size, Level level)
		{
			Texture2D healthPack = MainGame.ContentManager.Load<Texture2D>("Test/NewAbility");
			Animation animation = new Animation(healthPack, 1, false, healthPack.Bounds.Width, healthPack.Bounds.Height, new Vector2(size.X / healthPack.Bounds.Width, size.Y / healthPack.Bounds.Height));
			return new AbilityObject(position, size, level, animation, AbilityBuilder.GetBlinkMiscAbility);
		}
	}
}
