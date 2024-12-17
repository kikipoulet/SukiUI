using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Helpers;

namespace SukiUI.Controls.Experimental.DesktopEnvironment
{
    public class SDESoftware : SukiObservableObject
    {
        public IImage? Icon { get; set; }
        
        public string Name { get; set; }

        public Type Content { get; set; }

 

        private InternalWindow instance = null;

        public  InternalWindow Instance
        {
            get => instance;
            set
            {
                SetAndRaise(ref instance, value);
            }
        }

        public void Click(WindowManager wm)
        {
            if (Instance == null)
            {
                Instance = new InternalWindow(wm, (Control)Activator.CreateInstance(Content), Name);
                Instance.Closed += (sender, args) => Instance = null;
                wm.OpenWindow(Instance);
                return;
            }

            Instance.ChangeVisibility();
        }
    }
}