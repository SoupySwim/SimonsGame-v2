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
		public Vector2 Origin { get { return new Vector2(Animation.Scale * Animation.FrameWidth / 2.0f, Animation.Scale * Animation.FrameHeight / 2.0f); } }

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
			this._frameIndex = 0;
			this._time = 0.0f;
		}
		public void ResetAnimation()
		{
			this._frameIndex = 0;
			this._time = 0.0f;
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
			Rectangle source = new Rectangle(FrameIndex * Animation.FrameWidth, 0, Animation.FrameWidth, Animation.FrameHeight);
			Rectangle destination = new Rectangle((int)position.X, (int)position.Y, (int)(Animation.FrameWidth * Animation.Scale), (int)(Animation.FrameHeight * Animation.Scale));

			// Draw the current frame.
			//spriteBatch.Draw(Animation.Texture, position, source, Color, 0.0f, Origin, scale * Animation.Scale, spriteEffects, 0.0f);
			spriteBatch.Draw(Animation.Texture, destination, source, Color, 0.0f, new Vector2(0f, 0f), spriteEffects, 0.0f);
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
