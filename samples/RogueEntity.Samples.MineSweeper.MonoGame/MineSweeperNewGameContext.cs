using RogueEntity.SadCons;
using RogueEntity.SadCons.Controls;
using RogueEntity.Samples.MineSweeper.Core.Services;
using SadConsole;
using SadConsole.Controls;
using System;
using System.ComponentModel;
using System.Globalization;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public class MineSweeperNewGameContext : ConsoleContext<Window>
    {
        public event EventHandler<MineSweeperGameParameter> NewGameRequested;
        
        readonly RadioButtonSet<MineSweeperDifficulty> difficultySelector;
        readonly DefaultLabelBinding<MineSweeperGameParameter> profileData;
        Button submitButton;
        FlexibleTextBox minesBox;
        FlexibleTextBox widthBox;
        FlexibleTextBox heightBox;
        

        public MineSweeperNewGameContext()
        {
            this.difficultySelector = new RadioButtonSet<MineSweeperDifficulty>("DifficultyGroup");
            this.difficultySelector.SelectionChanged += OnDifficultyChanged;
            this.profileData = new DefaultLabelBinding<MineSweeperGameParameter>(FormatProfileData,
                                                                                 MineSweeperGameParameter.Easy);
            this.profileData.PropertyChanged += OnValueChanged;
        }

        void OnValueChanged(object sender, PropertyChangedEventArgs e)
        {
            System.Console.WriteLine("Value changed");
            if (submitButton != null)
            {
                submitButton.IsEnabled = this.profileData.Value.Validate();
            }
        }

        string FormatProfileData(MineSweeperGameParameter arg)
        {
            var pct = arg.MineCount / Math.Max(1f, arg.PlayFieldArea.Area);
            return $"of {arg.PlayFieldArea.Area} ({pct:P})";
        }

        void OnDifficultyChanged(object sender, MineSweeperDifficulty e)
        {
            switch (e)
            {
                case MineSweeperDifficulty.Easy:
                    profileData.Value = MineSweeperGameParameter.Easy;
                    break;
                case MineSweeperDifficulty.Normal:
                    profileData.Value = MineSweeperGameParameter.Normal;
                    break;
                case MineSweeperDifficulty.Hard:
                    profileData.Value = MineSweeperGameParameter.Hard;
                    break;
                case MineSweeperDifficulty.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }

            minesBox?.UpdateText(profileData.Value.MineCount.ToString());
            widthBox?.UpdateText(profileData.Value.PlayFieldArea.Width.ToString());
            heightBox?.UpdateText(profileData.Value.PlayFieldArea.Height.ToString());
        }

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);
            Console = new Window(60, 20);
            Console.Center();
            
            // Difficulty:
            // [ Easy ] [Medium] [ Hard ] [Custom]
            //
            // Width: [......]   Height: [.....]
            // Mines: [......]  (0.00 %)
            // 
            //                 [Back] [Start]
            int x = 2;
            int y = 4;
            

            Console.Add(SadConsoleControls.CreateLabel("Difficulty:").WithPlacementAt(2, 2));
            Console.Add(SadConsoleControls.CreateRadioButton("Easy", difficultySelector, MineSweeperDifficulty.Easy, 8)
                                          .WithHorizontalPlacementAt(ref x, y));
            Console.Add(SadConsoleControls.CreateRadioButton("Normal", difficultySelector, MineSweeperDifficulty.Normal, 8)
                                          .WithHorizontalPlacementAt(ref x, y));
            Console.Add(SadConsoleControls.CreateRadioButton("Hard", difficultySelector, MineSweeperDifficulty.Hard, 8)
                                          .WithHorizontalPlacementAt(ref x, y));
            Console.Add(SadConsoleControls.CreateRadioButton("Custom", difficultySelector, MineSweeperDifficulty.Custom, 8)
                                          .WithHorizontalPlacementAt(ref x, y));

            x = 2;
            y = 6;
            Console.Add(SadConsoleControls.CreateLabel("Width:", 8)
                                          .WithHorizontalPlacementAt(ref x, y));
            Console.Add(SadConsoleControls.CreateDecimalTextBox("10", 8)
                                          .With(b => widthBox = b)
                                          .WithHorizontalPlacementAt(ref x, y)
                                          .WithInputHandler(OnWidthTextChanged));
            Console.Add(SadConsoleControls.CreateLabel("Height:", 8)
                                          .WithHorizontalPlacementAt(ref x, y));
            Console.Add(SadConsoleControls.CreateDecimalTextBox("10", 8)
                                          .With(b => heightBox = b)
                                          .WithHorizontalPlacementAt(ref x, y)
                                          .WithInputHandler(OnHeightTextChanged));

            x = 2;
            y = 8;
            Console.Add(SadConsoleControls.CreateLabel("Mines:", 8)
                                          .WithHorizontalPlacementAt(ref x, y));
            Console.Add(SadConsoleControls.CreateDecimalTextBox("10", 8)
                                          .WithHorizontalPlacementAt(ref x, y)
                                          .With(b => minesBox = b)
                                          .WithInputHandler(OnMineCountTextChanged));
            Console.Add(SadConsoleControls.CreateLabel("of 9999 fields (100%)")
                                          .WithHorizontalPlacementAt(ref x, y)
                                          .WithBinding(profileData));
            
            
            x = 2;
            y = 12;
            Console.Add(SadConsoleControls.CreateButton("Back", 8, 1)
                                          .WithAction(OnBack)
                                          .WithHorizontalPlacementAt(ref x, y));
            Console.Add(SadConsoleControls.CreateButton("Start", 8, 1)
                                          .WithAction(OnStart)
                                          .WithHorizontalPlacementAt(ref x, y)
                                          .With(InstallValidator));

            difficultySelector.SelectedValue = MineSweeperDifficulty.Normal;
        }

        protected override void OnParentConsoleResized()
        {
            base.OnParentConsoleResized();
            Console.Center();
        }

        void InstallValidator(Button button)
        {
            this.submitButton = button;
            OnValueChanged(this, null);
        }

        void OnStart()
        {
            if (profileData.Value.Validate())
            {
                Hide();
                NewGameRequested?.Invoke(this, profileData.Value);
            }
        }

        void OnBack()
        {
            Hide();
        }

        void OnMineCountTextChanged(FlexibleTextBox b)
        {
            if (int.TryParse(b.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var number) &&
                number > 0 && number < profileData.Value.PlayFieldArea.Area)
            {
                profileData.Value = profileData.Value.WithMines(number);
                difficultySelector.SelectedValue = MineSweeperDifficulty.Custom;
            }
        }

        void OnWidthTextChanged(FlexibleTextBox b)
        {
            if (int.TryParse(b.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var number) &&
                number > 1 &&
                number != profileData.Value.PlayFieldArea.Width)
            {
                profileData.Value = profileData.Value.WithArea(number, profileData.Value.PlayFieldArea.Height);
                difficultySelector.SelectedValue = MineSweeperDifficulty.Custom;
            }
        }
        
        void OnHeightTextChanged(FlexibleTextBox b)
        {
            if (int.TryParse(b.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var number) &&
                number > 1 &&
                number != profileData.Value.PlayFieldArea.Height)
            {
                profileData.Value = profileData.Value.WithArea(profileData.Value.PlayFieldArea.Width, number);
                difficultySelector.SelectedValue = MineSweeperDifficulty.Custom;
            }
        }
    }
}
