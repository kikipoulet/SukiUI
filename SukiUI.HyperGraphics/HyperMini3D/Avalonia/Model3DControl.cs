using System;
using System.Collections.Generic;
using Avalonia;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Classe de base unifiee pour les controles Mini3D :
///     - Contrat Avalonia (<c>Control.cs</c>) : proprietes Avalonia, <c>BindChannel</c> et <see cref="Model3DView.OnModelClicked"/>.
///     - Scene 3D pure (<c>Control.Scene.cs</c>) : classe <typeparamref name="TScene"/> (<see cref="Model3D"/>) sans dependance Avalonia.
/// </summary>
public abstract class Model3DControl<TScene> : Model3DView where TScene : Model3D, new()
{
    private static readonly TScene SharedScene = new();
    private static readonly List<Action<Model3DControl<TScene>>> InitialSyncs = new();

    protected Model3DControl()
    {
        Model = SharedScene;
    }

    protected static void BindChannel<TControl>(
        StyledProperty<bool> property,
        string channel,
        bool animate = true)
        where TControl : Model3DControl<TScene>
        => BindChannel<TControl, bool>(property, channel, static v => v ? 1.0 : 0.0, animate);

    protected static void BindChannel<TControl>(
        StyledProperty<double> property,
        string channel,
        bool animate = true)
        where TControl : Model3DControl<TScene>
        => BindChannel<TControl, double>(property, channel, static v => v, animate);

    protected static void BindChannel<TControl, TValue>(
        StyledProperty<TValue> property,
        string channel,
        Func<TValue, double> converter,
        bool animate = true)
        where TControl : Model3DControl<TScene>
    {
        property.Changed.AddClassHandler<TControl>((c, e) =>
            c.SetChannel(channel, converter(e.GetNewValue<TValue>()), animate));
        InitialSyncs.Add(c =>
        {
            if (c is TControl typed)
                typed.SetChannel(channel, converter(typed.GetValue(property)), animate: false);
        });
    }

    protected override void OnBeforeAttach()
    {
        base.OnBeforeAttach();
        SyncChannels();
    }

    /// <summary>Reapplique immediatement (sans animation) tous les canaux lies aux proprietes.</summary>
    protected virtual void SyncChannels()
    {
        foreach (var sync in InitialSyncs)
            sync(this);
    }
}
