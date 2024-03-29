using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using RogueEntity.Core.Players;
using RogueEntity.SadCons.Controls;
using SadConsole;
using SadConsole.Controls;
using System;

namespace RogueEntity.SadCons
{
    public class LoadGameContext<TProfile> : ConsoleContext<Window>
    {
        readonly IPlayerProfileManager<TProfile> profileManager;
        Button backButton;
        Button selectButton;
        FlexibleListBox<PlayerProfileContainer<TProfile>> listContent;
        FlexibleListBoxItemTheme<PlayerProfileContainer<TProfile>> listItemRenderer;

        public event EventHandler<PlayerProfileContainer<TProfile>> LoadRequested;

        public FlexibleListBoxItemTheme<PlayerProfileContainer<TProfile>> ListItemRenderer
        {
            get => listItemRenderer;
            set
            {
                listItemRenderer = value ?? throw new ArgumentNullException();
                if (listContent != null)
                {
                    listContent.ItemTheme = listItemRenderer;
                }
            }
        }

        public LoadGameContext([NotNull] IPlayerProfileManager<TProfile> profileManager)
        {
            this.profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
            this.listContent = new FlexibleListBox<PlayerProfileContainer<TProfile>>(10, 10);
        }

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);
            var size = ParentContext.Bounds.BoundsFor(Global.FontDefault);
            Console = new Window(size.Width - 10, size.Height - 10)
            {
                Position = new Point(5, 5)
            };

            backButton = new Button(10)
            {
                Text = "Back", 
                Position = new Point(Console.Width - 13, Console.Height - 3)
            };
            backButton.Click += (e, args) => OnBack();

            selectButton = new Button(10)
            {
                Text = "Load", 
                Position = new Point(Console.Width - 26, Console.Height - 3)
            };
            selectButton.Click += OnPerformAction;
            
            Console.Add(backButton);
            Console.Add(selectButton);
            RecreateList();
        }
        
        void RecreateList()
        {
            if (listContent != null)
            {
                listContent.SelectedItemExecuted -= OnPerformAction;
                Console.Remove(listContent);
            }
            
            listContent = new FlexibleListBox<PlayerProfileContainer<TProfile>>(Console.Width - 4, Console.Height - 5)
            {
                Position = new Point(2,2)
            };
            listContent.SelectedItemExecuted += OnPerformAction;
            if (ListItemRenderer != null)
            {
                listContent.ItemTheme = ListItemRenderer;
            }
            Console.Add(listContent);
        }
        
        void RefreshData()
        {
            listContent.Items.Clear();
            foreach (var key in profileManager.KnownPlayerIds)
            {
                if (profileManager.TryLoadPlayerData(key, out var data))
                {
                    listContent.Items.Add(new PlayerProfileContainer<TProfile>(key, data));
                }
            }
        }

        void OnPerformAction(object sender, EventArgs e)
        {
            if (listContent.SelectedItem.TryGetValue(out var profile))
            {
                OnPerformAction(profile);
            }
        }

        protected virtual void OnPerformAction(PlayerProfileContainer<TProfile> profile)
        {
            LoadRequested?.Invoke(this, profile);
        }

        void OnBack()
        {
            Hide();
        }

        protected override void OnParentConsoleResized()
        {

            var size = ParentContext.Bounds.BoundsFor(Global.FontDefault);
            var width = size.Width - 10;
            var height = size.Height - 10;
            Console.Resize(width, height, true, new Rectangle(0, 0, width, height));

            backButton.Position = new Point(Console.Width - 13, Console.Height - 3);
            RecreateList();
            
            Console.IsDirty = true;
            FireConsoleResized();
        }

        public override void Show()
        {
            base.Show();
            RefreshData();
        }
    }
}
