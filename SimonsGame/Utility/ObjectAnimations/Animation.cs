using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Utility
{
	public class Animation
	{
		public Animation(Texture2D texture, float frameTime, bool isLooping, int width, int height, float scale)
		{
			this._texture = texture;
			this._frameTime = frameTime;
			this._isLooping = isLooping;
			_frameBounds = new Point(width, height);
			_scale = scale;
		}
		/// <summary>
		/// All frames in the animation arranged horizontally.
		/// </summary>
		private Texture2D _texture;
		public Texture2D Texture { get { return _texture; } }

		private float _scale;
		public float Scale { get { return _scale; } }
		private Point _frameBounds;
		public int FrameWidth { get { return _frameBounds.X; } }
		public int FrameHeight { get { return _frameBounds.Y; } }

		/// <summary>
		/// Duration of time to show each frame.
		/// </summary>
		private float _frameTime;
		public float FrameTime { get { return _frameTime; } }

		/// <summary>
		/// When the end of the animation is reached, should it
		/// continue playing from the beginning?
		/// </summary>
		private bool _isLooping;
		public bool IsLooping { get { return _isLooping; } }

		/// <summary>
		/// Gets the number of frames in the animation.
		/// </summary>
		public int FrameCount { get { return Texture.Width / FrameWidth; } }


	}
}