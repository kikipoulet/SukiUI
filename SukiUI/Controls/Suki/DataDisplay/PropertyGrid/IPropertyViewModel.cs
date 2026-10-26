using System.ComponentModel;

namespace SukiUI.Controls
{
    public interface IPropertyViewModel : INotifyPropertyChanged, IDisposable
    {
        string DisplayName { get; }
        bool IsReadOnly { get; }
        object? Value { get; set; }
    }
}