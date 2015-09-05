using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Utility
{
	public class AnimatedLevelAnimation : LevelAnimation
	{
		private Animator _animator;
		protected Animation _animation;
		protected Color _activeColor;
		protected int _tickTotal;
		protected int _tickCount = 0;
		public AnimatedLevelAnimation(Animation animation, Level level, Vector2 position, Color color, int tickDuration = 1) // Only last 1 frame :O
			: base(level, position)
		{
			_animation = animation;
			_activeColor = color;
			_tickTotal = tickDuration;
			_animator = new Animator();
			_animator.Color = _activeColor;
			_animator.PlayAnimation(_animation);
		}
		public override void Update(GameTime gameTime)
		{
			_tickCount++;
			if (_tickCount > _tickTotal)
				Level.RemoveLevelAnimation(this);
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			_animator.Draw(gameTime, spriteBatch, Position, SpriteEffects.None);
		}
	}
}
