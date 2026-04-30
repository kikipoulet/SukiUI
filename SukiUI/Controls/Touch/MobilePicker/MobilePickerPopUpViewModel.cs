using SukiUI.Helpers;
using System.Collections.ObjectModel;

namespace SukiUI.Controls.Touch.MobilePicker
{
    public class MobilePickerPopUpViewModel : SukiObservableObject
    {
        private ObservableCollection<string> _items = new ObservableCollection<string>() { };

        public ObservableCollection<string> Items {
            get => _items;
            set => SetAndRaise(ref _items, value);
        }

        private string _selecteditem = null;

        public string SelectedItem {
            get => _selecteditem;
            set => SetAndRaise(ref _selecteditem, value);
        }

        private string _title = null;

        public string Title {
            get => _title;
            set => SetAndRaise(ref _title, value);
        }

        private string _subtitle = null;

        public string SubTitle {
            get => _subtitle;
            set => SetAndRaise(ref _subtitle, value);
        }

        public MobilePicker mobilePicker { get; set; }
    }
}
