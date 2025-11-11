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

            _graphics.PreferredBackBufferWidth = 1000;
            _graphics.PreferredBackBufferHeight = 700;

            // Calculate centering offset
            _offsetX = (_graphics.PreferredBackBufferWidth - GridSize * CellSize) / 2;
            _offsetY = (_graphics.PreferredBackBufferHeight - GridSize * CellSize) / 2;
        }

        protected override void Initialize()
        {
            _random = new Random();
            _snake = new List<Point>();
            ResetGame();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white pixel texture
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
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

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyState = Keyboard.GetState();

            // Restart game
            if (_gameOver && keyState.IsKeyDown(Keys.Space) &&
                _previousKeyState.IsKeyUp(Keys.Space))
            {
                ResetGame();
            }

            if (!_gameOver)
            {
                // Handle input
                if (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.Up))
                {
                    if (_direction.Y == 0) _nextDirection = new Vector2(0, -1);
                }
                else if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down))
                {
                    if (_direction.Y == 0) _nextDirection = new Vector2(0, 1);
                }
                else if (keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Left))
                {
                    if (_direction.X == 0) _nextDirection = new Vector2(-1, 0);
                }
                else if (keyState.IsKeyDown(Keys.D) || keyState.IsKeyDown(Keys.Right))
                {
                    if (_direction.X == 0) _nextDirection = new Vector2(1, 0);
                }

                // Update movement timer
                _moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_moveTimer >= MoveInterval)
                {
                    _moveTimer = 0;
                    _direction = _nextDirection;
                    MoveSnake();
                }

                // Update visual effects
                _pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
                float pulse = (float)Math.Sin(_pulseTimer) * 0.3f + 0.7f;
                _foodColor = new Color(1f, pulse, pulse);
            }
            else
            {
                // Animate game over
                _gameOverAnimation += (float)gameTime.ElapsedGameTime.TotalSeconds * 3;
            }

            // Update screen shake
            if (_screenShake > 0)
            {
                _screenShake -= (float)gameTime.ElapsedGameTime.TotalSeconds * 2;
                if (_screenShake < 0) _screenShake = 0;
            }

            _previousKeyState = keyState;
            base.Update(gameTime);
        }

        private void MoveSnake()
        {
            Point newHead = new Point(
                _snake[0].X + (int)_direction.X,
                _snake[0].Y + (int)_direction.Y
            );

            // Wrap around walls
            if (newHead.X < 0) newHead.X = GridSize - 1;
            if (newHead.X >= GridSize) newHead.X = 0;
            if (newHead.Y < 0) newHead.Y = GridSize - 1;
            if (newHead.Y >= GridSize) newHead.Y = 0;

            // Check self collision
            if (_snake.Contains(newHead))
            {
                _gameOver = true;
                _screenShake = 0.5f;
                return;
            }

            _snake.Insert(0, newHead);

            // Check food collision
            if (newHead == _food)
            {
                _score += 10;
                SpawnFood();
            }
            else
            {
                _snake.RemoveAt(_snake.Count - 1);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(15, 15, 20));

            // Calculate shake offset
            int shakeX = 0;
            int shakeY = 0;
            if (_screenShake > 0)
            {
                shakeX = _random.Next(-10, 11);
                shakeY = _random.Next(-10, 11);
            }

            _spriteBatch.Begin();

            // Draw border
            int borderThickness = 5;
            Rectangle borderRect = new Rectangle(
                _offsetX - borderThickness + shakeX,
                _offsetY - borderThickness + shakeY,
                GridSize * CellSize + borderThickness * 2,
                GridSize * CellSize + borderThickness * 2
            );
            DrawBorder(borderRect, borderThickness, new Color(80, 80, 100));

            // Draw grid background
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    Color cellColor = (x + y) % 2 == 0 ?
                        new Color(30, 30, 40) : new Color(25, 25, 35);

                    DrawCell(x, y, cellColor, 1f, shakeX, shakeY);
                }
            }

            // Draw snake
            for (int i = 0; i < _snake.Count; i++)
            {
                Color snakeColor = i == 0 ?
                    new Color(100, 255, 100) :
                    Color.Lerp(new Color(50, 200, 50), new Color(30, 150, 30),
                        i / (float)_snake.Count);

                DrawCell(_snake[i].X, _snake[i].Y, snakeColor, i == 0 ? 0.95f : 0.9f, shakeX, shakeY);
            }

            // Draw food with pulse effect
            DrawCell(_food.X, _food.Y, _foodColor, 0.85f, shakeX, shakeY);

            // Draw score at top
            string scoreText = $"SCORE: {_score}";
            DrawPixelText(scoreText, new Vector2(_graphics.PreferredBackBufferWidth / 2, 20),
                Color.White, true);

            if (_gameOver)
            {
                DrawGameOver();
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawCell(int x, int y, Color color, float scale = 0.9f, int shakeX = 0, int shakeY = 0)
        {
            int padding = (int)((1 - scale) * CellSize / 2);
            Rectangle rect = new Rectangle(
                _offsetX + x * CellSize + padding + shakeX,
                _offsetY + y * CellSize + padding + shakeY,
                (int)(CellSize * scale),
                (int)(CellSize * scale)
            );
            _spriteBatch.Draw(_pixel, rect, color);
        }

        private void DrawBorder(Rectangle rect, int thickness, Color color)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        private void DrawGameOver()
        {
            int centerX = _graphics.PreferredBackBufferWidth / 2;
            int centerY = _graphics.PreferredBackBufferHeight / 2;

            // Animate background box
            float animScale = Math.Min(_gameOverAnimation, 1f);
            int boxWidth = (int)(500 * animScale);
            int boxHeight = (int)(200 * animScale);

            Rectangle bgRect = new Rectangle(
                centerX - boxWidth / 2,
                centerY - boxHeight / 2,
                boxWidth,
                boxHeight
            );

            if (animScale >= 1f)
            {
                _spriteBatch.Draw(_pixel, bgRect, new Color(0, 0, 0, 200));
                DrawBorder(bgRect, 4, Color.Red);

                // Blink effect
                float blink = (float)Math.Sin(_gameOverAnimation * 6) * 0.5f + 0.5f;
                Color textColor = Color.Lerp(Color.Red, Color.Yellow, blink);

                // Draw GAME OVER in pixel style
                DrawPixelText("GAME OVER", new Vector2(centerX, centerY - 30), textColor, true, 4);

                // Draw restart text
                if (_gameOverAnimation % 1 < 0.5f)
                {
                    DrawPixelText("PRESS SPACE", new Vector2(centerX, centerY + 40),
                        Color.Yellow, true, 2);
                }
            }
        }

        private void DrawPixelText(string text, Vector2 position, Color color, bool centered = false, int size = 2)
        {
            // Simple pixel font - each character is 5x7 pixels
            Dictionary<char, bool[,]> font = GetPixelFont();

            int charWidth = 6 * size;
            int charHeight = 8 * size;
            int totalWidth = text.Length * charWidth;

            float startX = centered ? position.X - totalWidth / 2 : position.X;
            float startY = position.Y;

            for (int i = 0; i < text.Length; i++)
            {
                char c = char.ToUpper(text[i]);
                if (font.ContainsKey(c))
                {
                    DrawPixelChar(font[c], new Vector2(startX + i * charWidth, startY), color, size);
                }
            }
        }

        private void DrawPixelChar(bool[,] pattern, Vector2 position, Color color, int size)
        {
            for (int y = 0; y < pattern.GetLength(0); y++)
            {
                for (int x = 0; x < pattern.GetLength(1); x++)
                {
                    if (pattern[y, x])
                    {
                        Rectangle rect = new Rectangle(
                            (int)position.X + x * size,
                            (int)position.Y + y * size,
                            size,
                            size
                        );
                        _spriteBatch.Draw(_pixel, rect, color);
                    }
                }
            }
        }

        private Dictionary<char, bool[,]> GetPixelFont()
        {
            var font = new Dictionary<char, bool[,]>();

            // G
            font['G'] = new bool[,] {
                {false, true, true, true, false},
                {true, false, false, false, true},
                {true, false, false, false, false},
                {true, false, true, true, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {false, true, true, true, false}
            };

            // A
            font['A'] = new bool[,] {
                {false, true, true, true, false},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, true, true, true, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true}
            };

            // M
            font['M'] = new bool[,] {
                {true, false, false, false, true},
                {true, true, false, true, true},
                {true, false, true, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true}
            };

            // E
            font['E'] = new bool[,] {
                {true, true, true, true, true},
                {true, false, false, false, false},
                {true, false, false, false, false},
                {true, true, true, true, false},
                {true, false, false, false, false},
                {true, false, false, false, false},
                {true, true, true, true, true}
            };

            // O
            font['O'] = new bool[,] {
                {false, true, true, true, false},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {false, true, true, true, false}
            };

            // V
            font['V'] = new bool[,] {
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {false, true, false, true, false},
                {false, false, true, false, false}
            };

            // R
            font['R'] = new bool[,] {
                {true, true, true, true, false},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, true, true, true, false},
                {true, false, false, true, false},
                {true, false, false, false, true},
                {true, false, false, false, true}
            };

            // S
            font['S'] = new bool[,] {
                {false, true, true, true, true},
                {true, false, false, false, false},
                {true, false, false, false, false},
                {false, true, true, true, false},
                {false, false, false, false, true},
                {false, false, false, false, true},
                {true, true, true, true, false}
            };

            // P
            font['P'] = new bool[,] {
                {true, true, true, true, false},
                {true, false, false, false, true},
                {true, false, false, false, true},
                {true, true, true, true, false},
                {true, false, false, false, false},
                {true, false, false, false, false},
                {true, false, false, false, false}
            };

            // C
            font['C'] = new bool[,] {
                {false, true, true, true, true},
                {true, false, false, false, false},
                {true, false, false, false, false},
                {true, false, false, false, false},
                {true, false, false, false, false},
                {true, false, false, false, false},
                {false, true, true, true, true}
            };

            // Numbers
            font['0'] = font['O'];
            font['1'] = new bool[,] {
                {false, false, true, false, false},
                {false, true, true, false, false},
                {false, false, true, false, false},
                {false, false, true, false, false},
                {false, false, true, false, false},
                {false, false, true, false, false},
                {false, true, true, true, false}
            };

            font[':'] = new bool[,] {
                {false, false, false, false, false},
                {false, false, true, false, false},
                {false, false, false, false, false},
                {false, false, false, false, false},
                {false, false, false, false, false},
                {false, false, true, false, false},
                {false, false, false, false, false}
            };

            font[' '] = new bool[,] {
                {false, false, false, false, false},
                {false, false, false, false, false},
                {false, false, false, false, false},
                {false, false, false, false, false},
                {false, false, false, false, false},
                {false, false, false, false, false},
                {false, false, false, false, false}
            };

            return font;
        }
    }
}