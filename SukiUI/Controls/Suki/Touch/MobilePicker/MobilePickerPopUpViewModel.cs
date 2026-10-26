using SukiUI.Helpers;
using System.Collections.ObjectModel;

namespace SukiUI.Controls.Touch.MobilePicker
{
    public class MobilePickerPopUpViewModel : SukiObservableObject
    {
        private ObservableCollection<string> _items = new();

        public ObservableCollection<string> Items {
            get => _items;
            set => SetAndRaise(ref _items, value);
        }

        private string? _selecteditem;

        public string? SelectedItem {
            get => _selecteditem;
            set => SetAndRaise(ref _selecteditem, value);
        }

        private object? _title;

        public object? Title {
            get => _title;
            set => SetAndRaise(ref _title, value);
        }

        private string? _subtitle;

        public string? SubTitle {
            get => _subtitle;
            set => SetAndRaise(ref _subtitle, value);
        }

        public MobilePicker? MobilePicker { get; set; }
    }
}
