using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame3DTest
{
  public class Game1 : Game
  {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _mapTexture;
    private Texture2D _carTexture;
    private Texture2D _bgTexture;

    private Vector2 _carPosition = new Vector2(128,1800.0f);
    private float _carRotation = MathF.PI * 0.5f;
    private Vector2 _carVelocity = Vector2.Zero;
    private float _carTurningSpeed = 2.0f;
    private float _carAcceleration = 50.0f;
    private float _carMaxSpeed = 100.0f;
    private float _cameraDistance = 100.0f;
    private float _cameraHeight = 50.0f;
    private float _cameraTargetHeight = 10.0f;
    private float _cameraFar = 300.0f;
    private float _bgScale = 1.75f;
    private float _bgScrollFactor = 120.0f;

    public Game1()
    {
      _graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      IsMouseVisible = true;

    }

    protected override void Initialize()
    {
      // TODO: Add your initialization logic here

      base.Initialize();
    }

    protected override void LoadContent()
    {
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      // TODO: use this.Content to load your game content here

      _mapTexture = Texture2D.FromFile(_graphics.GraphicsDevice, "map.png");
      _carTexture = Texture2D.FromFile(_graphics.GraphicsDevice, "car.png");
      _bgTexture = Texture2D.FromFile(_graphics.GraphicsDevice, "bg.png");
    }

    protected override void Update(GameTime gameTime)
    {
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        Exit();

      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      var kbState = Keyboard.GetState();

      if (kbState.IsKeyDown(Keys.Left))
      {
        _carRotation += deltaTime * _carTurningSpeed;
      }
      if (kbState.IsKeyDown(Keys.Right))
      {
        _carRotation -= deltaTime * _carTurningSpeed;
      }
      Vector2 direction = new Vector2(MathF.Cos(_carRotation + MathF.PI), MathF.Sin(_carRotation + MathF.PI));
      if (kbState.IsKeyDown(Keys.Up))
      {
        _carVelocity += direction * deltaTime * _carAcceleration;
      }

      if (_carVelocity.LengthSquared() > _carMaxSpeed * _carMaxSpeed)
      {
        _carVelocity.Normalize();
        _carVelocity *= _carMaxSpeed;
      }

      _carPosition += _carVelocity * deltaTime;

      // TODO: Add your update logic here

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      RasterizerState rasterizerState = new RasterizerState();
      rasterizerState.CullMode = CullMode.None;

      BasicEffect bgEffect = new BasicEffect(GraphicsDevice);

      Vector2 bgSize = new Vector2(512, 64) * _bgScale;

      float bgOffset = _carRotation * _bgScrollFactor;
      while (bgOffset > bgSize.X) bgOffset -= bgSize.X;
      while (bgOffset < -bgSize.X) bgOffset += bgSize.X;

      _spriteBatch.Begin();
      for (int i = -1; i <= 1; i++)
      {
        _spriteBatch.Draw(
          _bgTexture,
          new Vector2(bgOffset, 0) + new Vector2(bgSize.X, 0) * i,
          null,
          Color.White,
          0,
          Vector2.Zero,
          _bgScale,
          SpriteEffects.None,
          0);
      }
      _spriteBatch.End();

      Vector3 cameraOffset = new Vector3((float)Math.Cos(_carRotation) * _cameraDistance, (float)Math.Sin(_carRotation) * _cameraDistance, _cameraHeight);


      BasicEffect effect = new BasicEffect(GraphicsDevice);
      effect.World = Matrix.Identity;
      effect.Projection = Matrix.CreatePerspectiveFieldOfView(
          MathHelper.PiOver4,                         // The field-of-view 
          GraphicsDevice.Viewport.AspectRatio,   // The aspect ratio
          0.1f, // The near plane distance 
          _cameraFar // The far plane distance
      );
      effect.VertexColorEnabled = true;

      Vector3 cameraPosition = new Vector3(_carPosition, 0) + cameraOffset - new Vector3(_carVelocity, 0) * 0.1f;
      Vector3 cameraTarget = new Vector3(_carPosition.X, _carPosition.Y, _cameraTargetHeight) - new Vector3(_carVelocity, 0) * 0.1f;
      Vector3 cameraUp = new Vector3(0, 0, 1); // Z axis up
      Matrix cameraView = Matrix.CreateLookAt(cameraPosition, cameraTarget, new Vector3(0, 0, 1));
      effect.View = cameraView;

      effect.TextureEnabled = true;

      _spriteBatch.Begin(effect: effect, rasterizerState: rasterizerState);
        _spriteBatch.Draw(_mapTexture, Vector2.Zero, Color.White);
      _spriteBatch.End();

      DrawCar(effect, new Vector3(_carPosition, 0), cameraView);
      DrawCar(effect, new Vector3(_carPosition, 0) + new Vector3(30,30,0), cameraView);
      DrawCar(effect, new Vector3(_carPosition, 0) + new Vector3(-30, -30, 0), cameraView);
    }

    private void DrawCar(BasicEffect effect, Vector3 position, Matrix cameraMatrix)
    {

      RasterizerState rasterizerState = new RasterizerState();
      rasterizerState.CullMode = CullMode.None;

      cameraMatrix.Decompose(out _, out Quaternion rotation, out _);
      var rotationMatrix = Matrix.Invert(Matrix.CreateFromQuaternion(rotation));

      effect.World = Matrix.CreateScale(0.2f, 0.2f, 0.2f) * rotationMatrix * Matrix.CreateTranslation(position);

      _spriteBatch.Begin(effect: effect, rasterizerState: rasterizerState);
      _spriteBatch.Draw(_carTexture, Vector2.Zero, null, Color.White, 0, new Vector2(_carTexture.Width, _carTexture.Height) * 0.5f, 1.0f, SpriteEffects.None, 0);
      _spriteBatch.End();
    }
  }
}
