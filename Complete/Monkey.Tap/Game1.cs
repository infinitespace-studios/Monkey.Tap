#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections;

#endregion

namespace Monkey.Tap
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D monkey;
		Texture2D background;
		Texture2D logo;
		SpriteFont font;
		SoundEffect hit;
		Song title;

		List<GridCell> grid = new List<GridCell>();

		GameState currentState = GameState.Start;
		Random rnd = new Random ();
		string gameOverText = "Game Over";
		string tapToStartText = "Tap to Start";
		string scoreText = "Score : {0}";
		TimeSpan gameTimer = TimeSpan.FromMilliseconds (0);
		TimeSpan increaseLevelTimer = TimeSpan.FromMilliseconds(0);
		TimeSpan tapToRestartTimer = TimeSpan.FromSeconds(2);
		int cellsToChange = 0;
		int maxCells = 1;
		int maxCellsToChange = 14;
		int score = 0;

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
			graphics.IsFullScreen = true;
			graphics.SupportedOrientations = DisplayOrientation.Portrait;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			// TODO: Add your initialization logic here
			base.Initialize ();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);

			//TODO: use this.Content to load your game content here 
			monkey = Content.Load<Texture2D> ("monkey");
			background = Content.Load <Texture2D> ("background");
			logo = Content.Load<Texture2D> ("logo");
			font = Content.Load<SpriteFont> ("font");
			hit = Content.Load<SoundEffect> ("hit");
			title = Content.Load<Song> ("title");
			Microsoft.Xna.Framework.Media.MediaPlayer.IsRepeating = true;
			Microsoft.Xna.Framework.Media.MediaPlayer.Play (title);

			var viewport = graphics.GraphicsDevice.Viewport;
			var padding = (viewport.Width / 100);
			var gridWidth = (viewport.Width - (padding * 5)) / 4;
			var gridHeight = gridWidth;
			var verticaloffset = (viewport.Height / 10);

			for (int y = verticaloffset; y < gridHeight*5; y+=gridHeight+padding) {
				for (int x = padding; x < viewport.Width-gridWidth; x+=gridWidth+padding) {
					grid.Add (new GridCell () {
						DisplayRectangle = new Rectangle (x, y, gridWidth, gridHeight)
					});
				}
			}
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
			#if !__IOS__ &&  !__TVOS__
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
			    Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
				Exit ();
			}
			#endif
			// TODO: Add your update logic here
			var touchState = TouchPanel.GetState ();
			switch (currentState) {
			case GameState.Start:
				if (touchState.Count > 0) {
					currentState = GameState.Playing;
				}
				break;
			case GameState.Playing:
				PlayGame (gameTime, touchState);
				break;
			case GameState.GameOver:
				tapToRestartTimer -= gameTime.ElapsedGameTime;
				if (touchState.Count > 0 && tapToRestartTimer.TotalMilliseconds < 0) {
					currentState = GameState.Start;
					score = 0;
					gameTimer = TimeSpan.FromMilliseconds (0);
					cellsToChange = 1;
					for (int i = 0; i < grid.Count; i++) {
						grid [i].Reset ();
					}
				}
				break;
			}
			base.Update (gameTime);
		}

		void ProcessTouches(TouchCollection touchState)
		{
			foreach (var touch in touchState) {
				if (touch.State != TouchLocationState.Released)
					continue;
				for (int i=0; i < grid.Count; i++) {
					if (grid [i].DisplayRectangle.Contains (touch.Position) && grid[i].Color == Color.White) {
						hit.Play ();
						grid [i].Reset ();
						score += 1;
					}
				}
			}
		}

		void CheckForGameOver (GameTime gameTime)
		{
			for (int i = 0; i < grid.Count; i++) {
				if (grid [i].Update (gameTime)) {
					currentState = GameState.GameOver;
					tapToRestartTimer = TimeSpan.FromSeconds (2);
					break;
				}
			}
		}

		void CalculateCellsToChange ()
		{
			if (gameTimer.TotalSeconds > 2) {
				gameTimer = TimeSpan.FromMilliseconds (0);
				cellsToChange = Math.Min (maxCells, maxCellsToChange);
			}
		}
			
		void MakeMonkeysVisible()
		{
			if (cellsToChange > 0) {
				var idx = rnd.Next (grid.Count);
				// check the cell isn't already visible
				if (grid [idx].Color == Color.TransparentBlack) {
					grid [idx].Show ();
					cellsToChange--;
				}
			}
		}

		void IncreaseLevel ()
		{
			if (increaseLevelTimer.TotalSeconds > 10) {
				increaseLevelTimer = TimeSpan.FromMilliseconds (0);
				maxCells++;
			}
		}

		void PlayGame(GameTime gameTime, TouchCollection touchState)
		{
			// increment all the timers by the ElaspedGameTime
			gameTimer += gameTime.ElapsedGameTime;
			increaseLevelTimer += gameTime.ElapsedGameTime;

			ProcessTouches (touchState);
			CheckForGameOver (gameTime);
			CalculateCellsToChange ();
			MakeMonkeysVisible ();
			IncreaseLevel ();
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.SaddleBrown);
			// calcualte the center of the screen
			var center = graphics.GraphicsDevice.Viewport.Bounds.Center.ToVector2();
			// calcualte half the width of the screen
			var half = graphics.GraphicsDevice.Viewport.Width / 2;
			// figure out the aspect ratio of the logo.
			var aspect = (float)logo.Height / logo.Width;
			// now figure out where to draw the logo on the screen
			var rect = new Rectangle ((int)center.X - (half /2) , 0, half, (int)(half * aspect));

			spriteBatch.Begin ();
			// draw the background
			spriteBatch.Draw (background, destinationRectangle: graphics.GraphicsDevice.Viewport.Bounds, color: Color.White);
			// draw the logo
			spriteBatch.Draw (logo, destinationRectangle: rect, color: Color.White);
			// draw a Grid of Squares
			foreach (var square in grid) {
				spriteBatch.Draw (monkey, destinationRectangle: square.DisplayRectangle,
					color: Color.Lerp (Color.TransparentBlack, square.Color, square.Transition));
			}
			// if we are finished draw the score and Gameover in the center of the screen.
			if (currentState == GameState.GameOver) {
				// measure the text so we can center it correctly.
				var v = new Vector2(font.MeasureString (gameOverText).X /2 , 0);
				spriteBatch.DrawString (font, gameOverText, center - v, Color.OrangeRed);
				var t = string.Format (scoreText, score);
				// measure the text so we can center it correctly.
				v = new Vector2(font.MeasureString (t).X /2 , 0);
				// we use the font.LineSpacing to draw on the line underneath the gameover Text
				spriteBatch.DrawString (font, t, center  + new Vector2(-v.X, font.LineSpacing), Color.White);
			}
			if (currentState == GameState.Start) {
				// measure the text so we can center it correctly.
				var v = new Vector2(font.MeasureString (tapToStartText).X /2 , 0);
				spriteBatch.DrawString (font, tapToStartText, center - v, Color.White);
			}

			spriteBatch.End ();

			base.Draw (gameTime);
		}
	}
}

