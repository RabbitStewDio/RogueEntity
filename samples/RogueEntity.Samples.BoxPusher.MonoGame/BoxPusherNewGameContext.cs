using JetBrains.Annotations;
using RogueEntity.Core.Players;
using RogueEntity.SadCons;
using RogueEntity.SadCons.Controls;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using SadConsole;
using SadConsole.Controls;
using System;
using System.Collections.Generic;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherNewGameContext: ConsoleContext<Window>
    {
        public event EventHandler<(Guid, BoxPusherPlayerProfile)> Play;
        readonly HashSet<string> existingProfileNames;
        FlexibleTextBox nameField;
        Button submitButton;
        IPlayerProfileManager<BoxPusherPlayerProfile> profileManager;

        public BoxPusherNewGameContext([NotNull] IPlayerProfileManager<BoxPusherPlayerProfile> profileManager)
        {
            this.profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
            
            this.existingProfileNames = new HashSet<string>();
            foreach (var p in profileManager.ReadProfiles())
            {
                existingProfileNames.Add(p.PlayerName);
            }
        }

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);

            Console = new Window(40, 20);


            nameField = SadConsoleControls.CreateTextBox("<Name>", 30).WithInputHandler(ValidateName).With(t => t.EditedTextChanged += OnValidatePartialTextInput).WithPlacementAt(5, 5);
            submitButton = SadConsoleControls.CreateButton("Play", 10, 1).WithPlacementAt(5, 10).WithAction(OnPlay);
            Console.Add(nameField);
            Console.Add(submitButton);
        }

        void OnPlay()
        {
            if (!existingProfileNames.Contains(nameField.Text.Trim()) &&
                profileManager.TryCreatePlayer(new BoxPusherPlayerProfile(nameField.Text.Trim()), out var id, out var profileData))
            {
                Hide();
                Play?.Invoke(this, (id, profileData));
            }
        }

        void OnValidatePartialTextInput(object sender, EventArgs e)
        {
            ValidateName(nameField);
        }

        void ValidateName(FlexibleTextBox obj)
        {
            var profileName = nameField.Text.Trim();
            var validProfileName = !string.IsNullOrWhiteSpace(profileName) && !existingProfileNames.Contains(profileName);
            submitButton.IsEnabled = validProfileName;
        }
        
        
    }
}
