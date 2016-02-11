using System;
using Microsoft.Xna.Framework;

namespace Monkey.Tap
{
	public class GridCell {
		public Rectangle DisplayRectangle;
		public Color Color;
		public TimeSpan CountDown;
		public float Transition;

		public GridCell ()
		{
			Reset ();
		}

		public bool Update(GameTime gameTime) {
			if (Color == Color.White) {
				Transition += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 100f;
				CountDown -= gameTime.ElapsedGameTime;
				if (CountDown.TotalMilliseconds <= 0) {
					return true;
				}
			}
			return false;
		}

		public void Reset ()
		{
			Color = Color.TransparentBlack;
			CountDown = TimeSpan.FromSeconds (5);
			Transition = 0f;
		}

		public void Show()
		{
			Color = Color.White;
			CountDown = TimeSpan.FromSeconds (5);
		}
	}
}

