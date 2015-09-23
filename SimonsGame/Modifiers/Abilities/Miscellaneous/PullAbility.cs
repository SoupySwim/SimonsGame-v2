using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Utility;
using Microsoft.Xna.Framework.Graphics;

namespace SimonsGame.Modifiers.Abilities
{
	public class PullAbility : AbilityModifier
	{
		protected PhysicsObject _character;
		private int _length;

		public PullAbility(PhysicsObject p)
			: base(ModifyType.Add, p, new Tuple<Element, float>(Utility.Element.Normal, 0))
		{
			_character = p;
			IsExpiredFunction = IsExpiredFunc;
			_length = (int)((p.Level.PlatformDifference * 1.6f) + (p.Size.GetDistance() / 2.0f));
		}
		public bool IsExpiredFunc(GameTime gameTime)
		{
			Vector2 characterCenter = _character.Center;
			Vector2 characterAim = _character.GetAim();
			float angle = (float)(Math.Atan2(-characterAim.Y, characterAim.X));

			Vector2 lineOfVisionBox = characterAim * _length;
			Vector4 pullBounds = new Vector4(characterCenter.X, characterCenter.Y, lineOfVisionBox.Y, lineOfVisionBox.X);

			if (pullBounds.W < 0)
			{
				pullBounds.X = pullBounds.X + pullBounds.W;
				pullBounds.W = -pullBounds.W;
			}
			if (pullBounds.Z < 0)
			{
				pullBounds.Y = pullBounds.Y + pullBounds.Z;
				pullBounds.Z = -pullBounds.Z;
			}

			//_character.Level.AddLevelAnimation(new LineAnimation(_character.Level, characterCenter, characterCenter + lineOfVisionBox));

			IEnumerable<MainGuiObject> targetableCharacters = _character.Level.GetAllMovableCharacters(pullBounds);

			foreach (MainGuiObject mgo in targetableCharacters)
			{
				if (mgo.Team != _character.Team) // If the character is not on your team, then pull it towards you!
				{
					bool isTooFar = true;
					Vector2 distanceVector = mgo.Center - characterCenter;
					float distanceBetweenObjects = (distanceVector).GetDistance() - (mgo.Size.GetDistance() / 2);
					if (distanceBetweenObjects < _length)
						isTooFar = false;
					float angleFromCenter = (float)(Math.Atan2(-distanceVector.Y, distanceVector.X));

					if (!isTooFar && Math.Abs(angleFromCenter - angle) < .32f)
					{
						mgo.TeleportTo(characterCenter + (_character.Size * characterAim), 10, false);
					}
				}
			}
			return true;
		}
		public override void Reset()
		{
			base.Reset();
		}
		public override ModifierBase Clone(Guid id)
		{
			PullAbility blink = new PullAbility(_character);
			blink._guid = id == Guid.Empty ? Guid.NewGuid() : id;
			return blink;
		}
	}
}
