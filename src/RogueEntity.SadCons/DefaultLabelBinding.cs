using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RogueEntity.SadCons
{
    public class DefaultLabelBinding<T> : ILabelBinding
    {
        readonly Func<T, string> formatter;
        T value;

        public DefaultLabelBinding(Func<T, string> formatter, T initialValue = default)
        {
            this.formatter = formatter;
            value = initialValue;
        }

        public void NotifyChange()
        {
            OnPropertyChanged();
        }
        
        public T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(value, this.value))
                {
                    return;
                }
                
                this.value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedValue));
            }
        }

        public string FormattedValue => formatter?.Invoke(Value) ?? $"{Value}";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
