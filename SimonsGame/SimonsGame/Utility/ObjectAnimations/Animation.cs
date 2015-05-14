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
		/// <summary>
		/// All frames in the animation arranged horizontally.
		/// </summary>
		private Texture2D _texture;
		public Texture2D Texture { get { return _texture; } }

		public Vector2 Scale { get; set; }
		public float FrameWidth { get { return FrameSize.X; } }
		public float FrameHeight { get { return FrameSize.Y; } }
		public Vector2 FrameSize;
		public Vector2 ActualSize { get { return FrameSize * Scale; } }

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
		public int FrameCount { get { return (int)(Texture.Width / FrameWidth); } }

		public float RotateAmount { get; set; }

		public Animation(Texture2D texture, float frameTime, bool isLooping, int width, int height, float scale)
			: this(texture, frameTime, isLooping, width, height, new Vector2(scale)) { }
		public Animation(Texture2D texture, float frameTime, bool isLooping, int width, int height, Vector2 scale, float rotateAmount = 0)
		{
			this._texture = texture;
			this._frameTime = frameTime;
			this._isLooping = isLooping;
			FrameSize = new Vector2(width, height);
			Scale = scale;
			RotateAmount = rotateAmount;
		}
	}
}