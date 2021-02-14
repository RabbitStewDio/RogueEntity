using System.ComponentModel;

namespace RogueEntity.SadCons
{
    public interface ILabelBinding : INotifyPropertyChanged
    {
        public string FormattedValue { get; }
    }
}
