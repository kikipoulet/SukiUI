using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls.Experimental.DesktopEnvironment
{
    public partial class InternalWindow : UserControl
    {
       private Point _startPoint;
       private Thickness _startMargin;
        private bool _isDragging;
        private Panel _windowBorder;
        private Size _initialSize;
        private WindowManager _ParentWM;
        private bool _isResizing;

        private bool isVisible = true;
        public InternalWindow(WindowManager WM, Control content, string title)
        {
            InitializeComponent();

            _ParentWM = WM;
            this.Get<TextBlock>("TBTitle").Text = title;
            _windowBorder = this.FindControl<Panel>("WindowBorder");
             this.FindControl<ContentControl>("WindowContent").Content = content;

            // Gérer les événements pour permettre le déplacement
            var titleBar = this.FindControl<GlassCard>("PART_TitleBarBackground");
            titleBar.PointerPressed += OnTitleBarPointerPressed;
            titleBar.PointerMoved += OnTitleBarPointerMoved;
            titleBar.PointerReleased += OnTitleBarPointerReleased;
        }

        private void OnTitleBarPointerPressed(object sender, PointerPressedEventArgs e)
        {
           
                _startPoint = e.GetPosition( _ParentWM);
                _startMargin = _windowBorder.Margin;
                _isDragging = true;
            
        }

        private void OnTitleBarPointerMoved(object sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                    var currentPosition = e.GetPosition( _ParentWM);

                    // Calculer le décalage par rapport à la position initiale
                    var offsetX = currentPosition.X - _startPoint.X;
                    var offsetY = currentPosition.Y - _startPoint.Y;

                    // Appliquer la transformation de déplacement à la fenêtre
                    _windowBorder.Margin = new Thickness(_startMargin.Left + offsetX, _startMargin.Top + offsetY, 0, 0);
                
            }
        }

        private void OnTitleBarPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            // Arrête le suivi de la souris
            _isDragging = false;

            var parent = _ParentWM;
             var currentPosition = e.GetPosition( _ParentWM);
            if (parent != null)
            {

                // Vérifier la largeur et la hauteur du parent
                var parentWidth = parent.Bounds.Width;
                var parentHeight = parent.Bounds.Height;

                // Si la position X est à moins de 50 pixels du bord gauche
                if (currentPosition.X < 60)
                {
                    // Redimensionner et repositionner la fenêtre pour occuper toute la moitié gauche du parent
                    _windowBorder.Margin = new Thickness(0, 0, parentWidth / 2, 0);
                    _windowBorder.Animate<double>(WidthProperty, _windowBorder.Width,parentWidth / 2);
                    _windowBorder.Animate<double>(HeightProperty, _windowBorder.Height,parentHeight);
                    
                }
                // Si la position X est à moins de 50 pixels du bord droit
               else if (currentPosition.X + _windowBorder.Width > parentWidth - 60)
                {
                    // Redimensionner et repositionner la fenêtre pour occuper toute la moitié droite du parent
                    _windowBorder.Margin = new Thickness(parentWidth / 2, 0, 0, 0);
              
                    _windowBorder.Animate<double>(WidthProperty, _windowBorder.Width,parentWidth / 2);
                    _windowBorder.Animate<double>(HeightProperty, _windowBorder.Height,parentHeight);
                }
                // Si la position Y est à moins de 50 pixels du bord supérieur
                else if (currentPosition.Y < 60)
                {
                    // Redimensionner et repositionner la fenêtre pour occuper tout l'écran
                    _windowBorder.Margin = new Thickness(0);
      
                    _windowBorder.Animate<double>(WidthProperty, _windowBorder.Width,parentWidth );
                    _windowBorder.Animate<double>(HeightProperty, _windowBorder.Height,parentHeight);
                }
            }
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public event EventHandler Closed;

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void resizeborder(object? sender, PointerPressedEventArgs e)
        {
            _startPoint = e.GetPosition(_windowBorder);
            _isResizing= true;
            _initialSize = new Size(_windowBorder.Width, _windowBorder.Height);
        }

        private void rezisebordermove(object? sender, PointerEventArgs e)
        {
            if (_isResizing)
            {
                // Obtenir la position actuelle de la souris
                var currentPosition = e.GetPosition(_windowBorder);
                var deltaX = currentPosition.X - _startPoint.X;
                var deltaY = currentPosition.Y - _startPoint.Y;

                // Calculer la nouvelle taille
                var newWidth = Math.Max(_initialSize.Width + deltaX, 100); // Limite minimale de taille
                var newHeight = Math.Max(_initialSize.Height , 100);

                // Appliquer la nouvelle taille
                _windowBorder.Width = newWidth;
                _windowBorder.Height = newHeight;
            }
        }
        
        private void rezisebordermoveNS(object? sender, PointerEventArgs e)
        {
           
            if (_isResizing)
            {
                // Obtenir la position actuelle de la souris
                var currentPosition = e.GetPosition(_windowBorder);
                var deltaX = currentPosition.X - _startPoint.X;
                var deltaY = currentPosition.Y - _startPoint.Y;

                // Calculer la nouvelle taille
                var newWidth = Math.Max(_initialSize.Width , 100); // Limite minimale de taille
                var newHeight = Math.Max(_initialSize.Height + deltaY, 100);

                // Appliquer la nouvelle taille
                _windowBorder.Width = newWidth;
                _windowBorder.Height = newHeight;
            }
        }

        private void resizereleased(object? sender, PointerReleasedEventArgs e)
        {
            _isResizing = false;
        }

        public void ChangeVisibility()
        {
            if (isVisible)
                _windowBorder.Animate<double>(OpacityProperty,1,0);
            else
                _windowBorder.Animate<double>(OpacityProperty,0,1);

            _windowBorder.IsHitTestVisible = !isVisible; 

            isVisible = !isVisible;
        }

        private void PART_MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Redimensionner et repositionner la fenêtre pour occuper tout l'écran
            _windowBorder.Margin = new Thickness(0);
      
            _windowBorder.Animate<double>(WidthProperty, _windowBorder.Width,_ParentWM.Bounds.Width );
            _windowBorder.Animate<double>(HeightProperty, _windowBorder.Height,_ParentWM.Bounds.Height);
        }

        private void PART_MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeVisibility();
        }
    }
}