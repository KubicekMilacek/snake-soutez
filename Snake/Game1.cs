using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Snake
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;

        // Game constants
        private const int GridSize = 20;
        private const int CellSize = 30;
        private const float MoveInterval = 0.1f;

        // Snake properties
        private List<Point> _snake;
        private Vector2 _direction;
        private Vector2 _nextDirection;
        private float _moveTimer;

        // Food and game state
        private Point _food;
        private Random _random;
        private int _score;
        private bool _gameOver;
        private KeyboardState _previousKeyState;

        // Visual effects
        private float _pulseTimer;
        private Color _foodColor;
        private int _offsetX;
        private int _offsetY;
        private float _screenShake;
        private float _gameOverAnimation;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        _graphics.PreferredBackBufferWidth = 1000;
        _graphics.PreferredBackBufferHeight = 700;

             // Calculate centering offset
            _offsetX = (_graphics.PreferredBackBufferWidth - GridSize* CellSize) / 2;
        _offsetY = (_graphics.PreferredBackBufferHeight - GridSize* CellSize) / 2;
        }


        protected override void Initialize()
        {
            // TODO: Add your initi alization logic here
            _random = new Random();
            _snake = new List<Point>();
            ResetGame();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            // Create a 1x1 white pixel texture
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        private void ResetGame()
        {
            _snake.Clear();
            _snake.Add(new Point(GridSize / 2, GridSize / 2));
            _snake.Add(new Point(GridSize / 2 - 1, GridSize / 2));
            _snake.Add(new Point(GridSize / 2 - 2, GridSize / 2));

            _direction = new Vector2(1, 0);
            _nextDirection = _direction;
            _moveTimer = 0;
            _score = 0;
            _gameOver = false;
            _gameOverAnimation = 0;

            SpawnFood();
        }

        private void SpawnFood()
        {
            do
            {
                _food = new Point(_random.Next(GridSize), _random.Next(GridSize));
            } while (_snake.Contains(_food));
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
