using JetBrains.Annotations;
using SadConsole.Controls;
using System;
using System.Collections.Generic;

namespace RogueEntity.SadCons.Controls
{
    public class RadioButtonSet<TValue>: IDisposable
    {
        public event EventHandler<TValue> SelectionChanged;
        
        readonly Dictionary<TValue, RadioButton> valueToButtonMapping;
        readonly Dictionary<RadioButton, TValue> buttonToValueMapping;

        public RadioButtonSet(string name = null)
        {
            this.Name = name ?? Guid.NewGuid().ToString();
            
            valueToButtonMapping = new Dictionary<TValue, RadioButton>();
            buttonToValueMapping = new Dictionary<RadioButton, TValue>();
        }

        public void Dispose()
        {
            foreach (var b in buttonToValueMapping)
            {
                b.Key.IsSelectedChanged -= OnSelectionChanged;
            }
            
            buttonToValueMapping.Clear();
            valueToButtonMapping.Clear();
        }

        public string Name { get; }
        
        public void Register(TValue value, [NotNull] RadioButton button)
        {
            if (valueToButtonMapping.TryGetValue(value, out var oldButton))
            {
                oldButton.IsSelectedChanged -= OnSelectionChanged;
                buttonToValueMapping.Remove(oldButton);
            }
            valueToButtonMapping[value] = button ?? throw new ArgumentNullException(nameof(button));
            buttonToValueMapping[button] = value ?? throw new ArgumentNullException(nameof(value));
            
            button.IsSelectedChanged += OnSelectionChanged;
            
        }

        void OnSelectionChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton {IsSelected: true} b && 
                buttonToValueMapping.TryGetValue(b, out var selection))
            {
                SelectionChanged?.Invoke(this, selection);
            }
        }

        public TValue SelectedValue
        {
            get
            {
                foreach (var e in buttonToValueMapping)
                {
                    if (e.Key.IsSelected)
                    {
                        return e.Value;
                    }
                }

                return default;
            }
            set
            {
                foreach (var e in valueToButtonMapping)
                {
                    if (EqualityComparer<TValue>.Default.Equals(e.Key, value))
                    { 
                        e.Value.IsSelected = true;
                        return;
                    }
                }
            }
        }
    }
}
