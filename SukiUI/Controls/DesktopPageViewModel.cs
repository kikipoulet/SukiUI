using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SukiUI.Controls
{
    public class DesktopPageViewModel : ReactiveObject
    {
        public bool dialogOpen  = false;

        public bool DialogOpen
        {
            get => dialogOpen;
            set => this.RaiseAndSetIfChanged(ref dialogOpen, value);
        }

        public Control currentDialog = new Grid();

        public Control CurrentDialog
        {
            get => currentDialog;
            set => this.RaiseAndSetIfChanged(ref currentDialog, value);
        }

        public void CloseDialog()
        {
            DialogOpen = false;
        }
    }
}
