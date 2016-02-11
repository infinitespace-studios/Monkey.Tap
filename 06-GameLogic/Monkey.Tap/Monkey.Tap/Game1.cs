#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

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

			for (int y = padding; y < gridHeight*5; y+=gridHeight+padding) {
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
			spriteBatch.Begin ();
			foreach (var square in grid) {
				spriteBatch.Draw (monkey, destinationRectangle: square.DisplayRectangle,
					color: Color.White);
			}
			spriteBatch.End ();
			base.Draw (gameTime);
		}
	}
}

