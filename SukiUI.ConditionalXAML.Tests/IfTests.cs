using System.ComponentModel;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using SukiUI.Helpers.ConditionalXAML;
using Xunit;

namespace SukiUI.ConditionalXAML.Tests;

public class IfTests
{
    [AvaloniaFact]
    public void Relevant_notifications_update_visibility_synchronously()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model);
        Assert.True(view.Control.IsVisible);
        model.Flag = false;
        Assert.False(view.Control.IsVisible);
        model.Flag = true;
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Unrelated_notifications_still_evaluate_property_getters()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model, "CanShow");
        var reads = model.Reads;
        model.Notify("Unrelated");
        Assert.Equal(reads + 1, model.Reads);
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Unrelated_notifications_still_evaluate_method_calls()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model, "EvaluateFlag()");
        var reads = model.Reads;
        model.Notify("Unrelated");
        Assert.Equal(reads + 1, model.Reads);
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Empty_and_null_property_names_still_evaluate()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model, "CanShow");
        var reads = model.Reads;
        model.Notify(string.Empty);
        model.Notify(null);
        Assert.Equal(reads + 2, model.Reads);
    }

    [AvaloniaFact]
    public void Same_type_context_replacement_uses_the_current_instance()
    {
        var oldModel = new Model { Flag = true };
        var newModel = new Model { Flag = false };
        using var view = new AttachedIf(oldModel);
        view.Control.DataContext = newModel;
        Assert.False(view.Control.IsVisible);
        oldModel.Flag = false;
        newModel.Flag = true;
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Context_replacement_unsubscribes_the_previous_instance()
    {
        var oldModel = new Model { Flag = true };
        var newModel = new Model { Flag = true };
        using var view = new AttachedIf(oldModel, "CanShow");
        view.Control.DataContext = newModel;
        var reads = newModel.Reads;
        oldModel.Notify(null);
        Assert.Equal(reads, newModel.Reads);
        newModel.Notify(null);
        Assert.Equal(reads + 1, newModel.Reads);
    }

    [AvaloniaFact]
    public void Runtime_type_changes_recompile_the_expression()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model);
        view.Control.DataContext = new OtherModel { Flag = true };
        Assert.True(view.Control.IsVisible);
        view.Control.DataContext = model;
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Boxed_value_contexts_use_the_current_value()
    {
        using var view = new AttachedIf(new ValueModel(true));
        Assert.True(view.Control.IsVisible);
        view.Control.DataContext = new ValueModel(false);
        Assert.False(view.Control.IsVisible);
        view.Control.DataContext = new ValueModel(true);
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Condition_changes_replace_the_evaluator()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model);
        view.Control.Condition = "Flag == false";
        Assert.False(view.Control.IsVisible);
        model.Flag = false;
        Assert.True(view.Control.IsVisible);
        view.Control.Condition = "Flag == true";
        Assert.False(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Empty_conditions_hide_and_can_recover()
    {
        using var view = new AttachedIf(new Model { Flag = true });
        foreach (var condition in new[] { null, string.Empty, " \t " })
        {
            view.Control.Condition = condition!;
            Assert.False(view.Control.IsVisible);
            view.Control.Condition = "Flag == true";
            Assert.True(view.Control.IsVisible);
        }
    }

    [AvaloniaFact]
    public void Null_context_hides_and_can_recover()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model);
        view.Control.DataContext = null;
        Assert.False(view.Control.IsVisible);
        view.Control.DataContext = model;
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Parse_failures_hide_without_reusing_a_stale_evaluator()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model);
        view.Control.Condition = "Flag ==";
        Assert.False(view.Control.IsVisible);
        model.Notify(null);
        Assert.False(view.Control.IsVisible);
        view.Control.Condition = "Flag == true";
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Invocation_failures_can_recover_without_condition_changes()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model, "CanShow");
        model.ThrowOnRead = true;
        model.Notify(null);
        Assert.False(view.Control.IsVisible);
        model.ThrowOnRead = false;
        model.Notify(null);
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Detach_stops_notifications_and_reattach_reads_current_state()
    {
        var model = new Model { Flag = true };
        using var view = new AttachedIf(model, "CanShow");
        view.Window.Content = null;
        var reads = model.Reads;
        model.Flag = false;
        Assert.Equal(reads, model.Reads);
        view.Window.Content = view.Control;
        Assert.False(view.Control.IsVisible);
        Assert.Equal(reads + 1, model.Reads);
        model.Flag = true;
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Explicit_parameter_names_remain_supported()
    {
        using var view = new AttachedIf(new Model { Flag = true }, "x.Flag == true");
        Assert.True(view.Control.IsVisible);
    }

    [AvaloniaFact]
    public void Nested_member_access_reads_current_objects()
    {
        var model = new Model { Child = new OtherModel { Flag = true } };
        using var view = new AttachedIf(model, "Child.Flag == true");
        Assert.True(view.Control.IsVisible);
        model.Child = new OtherModel { Flag = false };
        model.Notify(null);
        Assert.False(view.Control.IsVisible);
    }

    private sealed class AttachedIf : IDisposable
    {
        public If Control { get; }
        public Window Window { get; }

        public AttachedIf(object model, string condition = "Flag == true")
        {
            Control = new If { Condition = condition, DataContext = model };
            Window = new Window { Content = Control };
            Window.Show();
        }

        public void Dispose() => Window.Close();
    }

    [DynamicLinqType]
    public sealed class Model : INotifyPropertyChanged
    {
        private bool _flag;
        public int Reads { get; private set; }
        public bool ThrowOnRead { get; set; }
        public OtherModel Child { get; set; } = new();

        public bool Flag
        {
            get => _flag;
            set { _flag = value; Notify(nameof(Flag)); }
        }

        public bool CanShow
        {
            get
            {
                Reads++;
                if (ThrowOnRead) throw new InvalidOperationException("Transient getter failure");
                return Flag;
            }
        }

        public bool EvaluateFlag() { Reads++; return Flag; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void Notify(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class OtherModel
    {
        public bool Flag { get; set; }
    }

    public readonly record struct ValueModel(bool Flag);
}
