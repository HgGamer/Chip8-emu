using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chip8_emu
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Emulator emulator = new Emulator();
        private bool[] framebuffer = new bool[64 * 32];
        private bool spacepressed = false;
        private void DrawFramebuffer()
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            Texture2D rect = new Texture2D(_graphics.GraphicsDevice, 64, 32);

            Color[] data = new Color[64 * 32];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = framebuffer[i] ? Color.White : Color.Black;
            }
            rect.SetData(data);

            Vector2 coor = new Vector2(5, 5);
       
            _spriteBatch.Draw(rect, coor,null, Color.White,0,new Vector2(0,0), new Vector2(10, 10), SpriteEffects.None,0);
            _spriteBatch.End();
        }


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

            framebuffer[12] = true;

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            emulator.LoadRom("");
           
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            emulator.SetKey(0x01, Keyboard.GetState().IsKeyDown(Keys.D1));
            emulator.SetKey(0x02, Keyboard.GetState().IsKeyDown(Keys.D2));
            emulator.SetKey(0x03, Keyboard.GetState().IsKeyDown(Keys.D3));
            emulator.SetKey(0x0C, Keyboard.GetState().IsKeyDown(Keys.D4));

            emulator.SetKey(0x04, Keyboard.GetState().IsKeyDown(Keys.Q));
            emulator.SetKey(0x05, Keyboard.GetState().IsKeyDown(Keys.W));
            emulator.SetKey(0x06, Keyboard.GetState().IsKeyDown(Keys.E));
            emulator.SetKey(0x0D, Keyboard.GetState().IsKeyDown(Keys.R));

            emulator.SetKey(0x07, Keyboard.GetState().IsKeyDown(Keys.A));
            emulator.SetKey(0x08, Keyboard.GetState().IsKeyDown(Keys.S));
            emulator.SetKey(0x09, Keyboard.GetState().IsKeyDown(Keys.D));
            emulator.SetKey(0x0E, Keyboard.GetState().IsKeyDown(Keys.F));

            emulator.SetKey(0x0A, Keyboard.GetState().IsKeyDown(Keys.Y));
            emulator.SetKey(0x00, Keyboard.GetState().IsKeyDown(Keys.X));
            emulator.SetKey(0x0B, Keyboard.GetState().IsKeyDown(Keys.C));
            emulator.SetKey(0x0F, Keyboard.GetState().IsKeyDown(Keys.V));

            if (Keyboard.GetState().IsKeyUp(Keys.Space) && spacepressed)
            {
                spacepressed = false;
                for(int i = 0; i < 5; i++)
                {
                    emulator.Tick();
                    framebuffer = emulator.framebuffer;
                }
                
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                spacepressed = true;
                
            }
            // TODO: Add your update logic here
            for (int i = 0; i < 16; i++)
            {
                emulator.Tick();
                framebuffer = emulator.framebuffer;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
           
            DrawFramebuffer();

           
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
