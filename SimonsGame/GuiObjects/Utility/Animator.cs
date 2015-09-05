using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// To be implemented later.
namespace SimonsGame.GuiObjects.Utility
{
	public struct Animator
	{
		/// <summary>
		/// Gets the animation which is currently playing.
		/// </summary>
		public Animation Animation { get { return _animation; } }
		private Animation _animation;

		Color _color;
		public Color Color { get { return _color == Color.Transparent ? Color.White : _color; } set { _color = value; } }

		private float _radians;

		/// <summary>
		/// Gets the index of the current frame in the animation.
		/// </summary>
		private int _frameIndex;
		public int FrameIndex { get { return _frameIndex; } }

		/// <summary>
		/// The amount of time in seconds that the current frame has been shown for.
		/// </summary>
		private float _time;

		/// <summary>
		/// Gets a texture origin at the bottom center of each frame.
		/// </summary>
		public Vector2 Origin { get { return Animation.Scale * Animation.FrameWidth / 2.0f; } }//new Vector2(Animation.Scale.X * Animation.FrameWidth / 2.0f, Animation.Scale.Y * Animation.FrameHeight / 2.0f); } }

		public void HideAnimations()
		{
			PlayAnimation(null);
		}
		/// <summary>
		/// Begins or continues playback of an animation.
		/// </summary>
		public void PlayAnimation(Animation animation)
		{
			// If this animation is already running, do not restart it.
			if (Animation == animation)
				return;

			// Start the new animation.
			this._animation = animation;
			ResetAnimation();
		}
		public void ResetAnimation()
		{
			this._frameIndex = 0;
			this._time = 0.0f;
			this._radians = 0;
		}
		public bool IsAnimating(Animation animation)
		{
			return Animation == animation;
		}

		/// <summary>
		/// Advances the time position and draws the current frame of the animation.
		/// </summary>
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects, float scale = 1.0f)
		{
			if (Animation == null)
				return; //throw new NotSupportedException("No animation is currently playing.");

			// Calculate the source rectangle of the current frame.
			Rectangle source = new Rectangle((int)(FrameIndex * Animation.FrameWidth), 0, (int)Animation.FrameWidth, (int)Animation.FrameHeight);
			Rectangle destination = new Rectangle((int)(position.X + (Animation.ActualSize.X / 2.0f)), (int)(position.Y + (Animation.ActualSize.Y / 2.0)), (int)(Animation.ActualSize.X), (int)(Animation.ActualSize.Y));

			// Draw the current frame.
			//spriteBatch.Draw(Animation.Texture, position, source, Color, 0.0f, Origin, scale * Animation.Scale, spriteEffects, 0.0f);
			spriteBatch.Draw(Animation.Texture, position + Animation.ActualSize / 2.0f, source, Color, _radians, Animation.FrameSize / 2.0f, Animation.Scale, spriteEffects, 0.0f);
			_radians += Animation.RotateAmount;
		}
		public void Update(GameTime gameTime)
		{
			if (Animation == null)
				return;
			// Process passing time.
			_time += (float)gameTime.ElapsedGameTime.TotalSeconds;
			while (_time > Animation.FrameTime)
			{
				_time -= Animation.FrameTime;

				// Advance the frame index; looping or clamping as appropriate.
				if (Animation.IsLooping)
					_frameIndex = (_frameIndex + 1) % Animation.FrameCount;
				else
					_frameIndex = Math.Min(_frameIndex + 1, Animation.FrameCount - 1);
			}
		}

	}
}
