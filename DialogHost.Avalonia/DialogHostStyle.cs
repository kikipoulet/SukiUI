using Avalonia;

namespace DialogHost {
    public class DialogHostStyle {
        /// <summary>
        /// Controls CornerRadius DialogHost's popup background.
        /// Works only for default DialogHost theme!
        /// </summary>
        public static readonly AttachedProperty<CornerRadius> CornerRadiusProperty = 
            AvaloniaProperty.RegisterAttached<DialogHostStyle, DialogHost, CornerRadius>("CornerRadius");

        /// <summary>
        /// Get CornerRadius in DialogHost's popup background.
        /// Works only for default DialogHost theme!
        /// </summary>
        public static CornerRadius GetCornerRadius(DialogHost element) {
            return element.GetValue(CornerRadiusProperty);
        }

        /// <summary>
        /// Set CornerRadius in DialogHost's popup background.
        /// Works only for default DialogHost theme!
        /// </summary>
        public static void SetCornerRadius(DialogHost element, CornerRadius value) {
            element.SetValue(CornerRadiusProperty, value);
        }
    }
}