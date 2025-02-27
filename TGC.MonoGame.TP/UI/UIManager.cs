using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.UI
{
    internal class UIManager
    {
        private TGCGame _game;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        public GameStatus UIStatus { get; set; }
        public MenuOption MenuStatus { get; set; }
        public int StageOption { get; set; }

        public TimeSpan Timer { get; set; }
        public int Score { get; set; }

        public SoundEffect MenuSoundEffect;
        public SoundEffectInstance MenuSoundEffectInstance;

        private KeyboardState _previouskeyboardState;

        public UIManager(TGCGame game, ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            _game = game;
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _spriteFont = font;
            _previouskeyboardState = Keyboard.GetState();

            UIStatus = GameStatus.Title;
            MenuStatus = MenuOption.Resume;
            StageOption = game.StageNumber;
            Timer = TimeSpan.Zero;
            Score = 0;


            MenuSoundEffect = content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "menu");
            MenuSoundEffectInstance = MenuSoundEffect.CreateInstance();
        }

        public void ResetStage(int stage)
        {
            MenuStatus = MenuOption.Resume;
            StageOption = stage;
            Timer = TimeSpan.Zero;
            Score = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (UIStatus == GameStatus.Playing)
            {
                Timer += gameTime.ElapsedGameTime;
            }
            else if (UIStatus == GameStatus.Title)
            {
                if (TitleScreen.PressAnyKey())
                {
                    UIStatus = GameStatus.Playing;
                }
            }
            else if (UIStatus == GameStatus.Menu)
            {
                KeyboardState keyboardState = Keyboard.GetState();
                if ((IsKeyPressed(keyboardState, _previouskeyboardState, Keys.Down) || IsKeyPressed(keyboardState, _previouskeyboardState, Keys.S)) && MenuStatus < MenuOption.Exit)
                {
                    MenuStatus++;
                    MenuSoundEffectInstance.Play();
                }
                else if ((IsKeyPressed(keyboardState, _previouskeyboardState, Keys.Up) || IsKeyPressed(keyboardState, _previouskeyboardState, Keys.W)) && MenuStatus > MenuOption.Resume)
                {
                    MenuStatus--;
                    MenuSoundEffectInstance.Play();
                }
                else if (IsKeyPressed(keyboardState, _previouskeyboardState, Keys.Enter))
                {
                    HandleMenuSelection();
                }

                _previouskeyboardState = keyboardState;
            }
            else if (UIStatus == GameStatus.StageSelector)
            {
                KeyboardState keyboardState = Keyboard.GetState();
                if ((IsKeyPressed(keyboardState, _previouskeyboardState, Keys.Down) || IsKeyPressed(keyboardState, _previouskeyboardState, Keys.S)) && StageOption < 2)
                {
                    StageOption++;
                    MenuSoundEffectInstance.Play();
                }
                else if ((IsKeyPressed(keyboardState, _previouskeyboardState, Keys.Up) || IsKeyPressed(keyboardState, _previouskeyboardState, Keys.W)) && StageOption > 1)
                {
                    StageOption--;
                    MenuSoundEffectInstance.Play();
                }
                else if (IsKeyPressed(keyboardState, _previouskeyboardState, Keys.Enter))
                {
                    HandleStageSelection();
                }

                _previouskeyboardState = keyboardState;
            }
        }

        private bool IsKeyPressed(KeyboardState keyboardState, KeyboardState previouskeyboardState, Keys key)
        {
            return keyboardState.IsKeyDown(key) && previouskeyboardState.IsKeyUp(key);
        }

        private void HandleMenuSelection()
        {
            switch (MenuStatus)
            {
                case MenuOption.Resume:
                    // TODO: AudioManager.ResumeBackgroundMusic();
                    UIStatus = GameStatus.Playing;
                    MediaPlayer.Volume = AudioManager.StandardVolume;
                    break;

                case MenuOption.Restart:
                    _game.LoadStage(StageOption);
                    UIStatus = GameStatus.Playing;
                    MediaPlayer.Volume = AudioManager.StandardVolume;
                    break;

                case MenuOption.GodMode:
                    // TODO: God mode
                    break;

                case MenuOption.SelectStage:
                    UIStatus = GameStatus.StageSelector;
                    break;

                case MenuOption.Exit:
                    // TODO: select stage
                    UIStatus = GameStatus.Exit;
                    break;
            }
        }

        private void HandleStageSelection()
        {
            UIStatus = GameStatus.Playing;
            _game.LoadStage(StageOption);
        }

        public void Draw()
        {
            if (UIStatus == GameStatus.Playing)
            {
                HUD.Draw(_graphicsDevice, _spriteBatch, _spriteFont, Timer, Score);
            }
            else if (UIStatus == GameStatus.Title)
            {
                TitleScreen.Draw(_graphicsDevice, _spriteBatch, _spriteFont);
            }
            else if (UIStatus == GameStatus.Menu)
            {
                Menu.Draw(_graphicsDevice, _spriteBatch, _spriteFont, MenuStatus);
            }
            else if (UIStatus == GameStatus.StageSelector)
            {
                StageSelector.Draw(_graphicsDevice, _spriteBatch, _spriteFont, StageOption);
            }
        }
    }
}
