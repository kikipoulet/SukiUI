using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;

namespace SukiUI.Motion.Tests;

/// <summary>
/// The SukiUI ComboBox shape: an empty PART_Popup for the ComboBox's own logic, and the real
/// drop-down owned by the popup section, its root collapsing toward its top edge on close.
/// </summary>
public class ComboBoxPopupTests
{
    private static ComboBox Build()
    {
        var combo = new ComboBox { Width = 200, ItemsSource = Enumerable.Range(0, 10).Select(i => $"Item {i}").ToList() };
        combo.Template = new FuncControlTemplate<ComboBox>((host, scope) =>
        {
            var standIn = new Popup { Name = "PART_Popup" };
            var presenter = new ItemsPresenter { Name = "PART_ItemsPresenter" };
            var root = new Border
            {
                Name = "PART_LayoutTransform",
                RenderTransformOrigin = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                Child = presenter,
            };
            var real = new Popup { Name = "PART_SukiPopup", Child = root, PlacementTarget = host };
            scope.Register(standIn.Name, standIn);
            scope.Register(real.Name, real);
            scope.Register(presenter.Name, presenter);
            scope.Register(root.Name, root);
            return new Panel { Height = 40, Children = { standIn, real } };
        });

        Motion.Configure(combo, motion =>
        {
            var popup = motion.Popup()
                .PopupPart("PART_SukiPopup")
                .RootPart("PART_LayoutTransform")
                .ItemsPresenterPart("PART_ItemsPresenter")
                .OpenWhen(ComboBox.IsDropDownOpenProperty)
                .CloseOnPointerPressedOutside()
                .KeepPressesInside()
                .CloseWhenItemClicked();
            popup.OnOpening()
                 .Animate(popup.Root, SukiMotion.ScaleYProperty).From(0.72).To(1.0).WithSpring(new Spring(20.0, 40.8));
            popup.OnClosing()
                 .Animate(popup.Root, SukiMotion.ScaleYProperty).To(0.888).WithSpring(new Spring(26.7, 34.7))
                 .And()
                 .Animate(popup.Root, Visual.OpacityProperty).To(0.0).Over(TimeSpan.FromMilliseconds(150));
        });
        return combo;
    }

    private static Popup RealPopup(ComboBox combo) =>
        combo.GetTemplateChildren().OfType<Popup>().Single(p => p.Name == "PART_SukiPopup");

    private static Point CenterInRoot(Control control, Visual root) =>
        control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), root)!.Value;

    private static void Press(Control target, IPointer pointer, Visual root, Point at) =>
        target.RaiseEvent(new PointerPressedEventArgs(target, pointer, root, at, 0,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));

    private static void Release(Control target, IPointer pointer, Visual root, Point at) =>
        target.RaiseEvent(new PointerReleasedEventArgs(target, pointer, root, at, 1,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None, MouseButton.Left));

    // Selection is decided on release, against the item's transformed bounds: a press that
    // already closed the drop-down let the collapse pull the lower items out from under the
    // pointer before the release.
    [AvaloniaTheory]
    [InlineData(0, 0)]
    [InlineData(9, 0)]
    [InlineData(9, 3)]
    [InlineData(9, 6)]
    public void Clicking_an_item_selects_it_then_closes_the_drop_down(int index, int framesHeld)
    {
        var combo = Build();
        using var h = new MotionHarness(combo);
        combo.IsDropDownOpen = true;
        h.Frames(40);

        var item = (Control)combo.ContainerFromIndex(index)!;
        var root = (Visual)TopLevel.GetTopLevel(item)!;
        var at = CenterInRoot(item, root);
        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);

        Press(item, pointer, root, at);
        bool openAfterPress = combo.IsDropDownOpen;
        h.Frames(framesHeld);
        Release(item, pointer, root, at);
        bool openAfterRelease = combo.IsDropDownOpen;
        h.Frames(30);

        Assert.Multiple(
            () => Assert.True(openAfterPress, "the press closed the drop-down"),
            () => Assert.Equal(index, combo.SelectedIndex),
            () => Assert.False(openAfterRelease, "the selecting click did not close the drop-down"),
            () => Assert.False(RealPopup(combo).IsOpen, "the popup is still open after the collapse"));
    }

    [AvaloniaFact]
    public void A_click_inside_the_drop_down_but_off_the_items_keeps_it_open()
    {
        var combo = Build();
        using var h = new MotionHarness(combo);
        combo.IsDropDownOpen = true;
        h.Frames(40);

        var popupRoot = (Control)RealPopup(combo).Child!;
        var root = (Visual)TopLevel.GetTopLevel(popupRoot)!;
        var at = CenterInRoot(popupRoot, root);
        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);

        Press(popupRoot, pointer, root, at);
        Release(popupRoot, pointer, root, at);
        h.Frames(30);

        Assert.True(combo.IsDropDownOpen);
        Assert.True(RealPopup(combo).IsOpen);
        Assert.Equal(-1, combo.SelectedIndex);
    }
}
