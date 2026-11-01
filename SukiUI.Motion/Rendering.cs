using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;

namespace SukiUI.Motion
{
    /// <summary>
    /// The shared render-transform block of one target: ONE <c>TransformGroup</c> lazily
    /// attached on the first transform-channel write (the proven attach-once rule — that
    /// write is what schedules the frame the animation rides on), its children created
    /// lazily per channel kind, all transform channels (scale, translate, rotate, skew)
    /// writing through the same group so they COMPOSE instead of fighting over
    /// RenderTransform. Keyed on the rendered visual (two surfaces over the same target
    /// share the block; a re-resolved template part gets a fresh one for free). Fixed
    /// composition order — translate · skew · rotate · scale around the target's
    /// RenderTransformOrigin (the proven dialog order when rotate/skew are absent; the
    /// skew shears in the parent frame, after rotation in point order); children are
    /// inserted at their canonical rank whatever their creation order. A pre-existing
    /// bare ScaleTransform is adopted on first attach (pose continuity); any other
    /// RenderTransform value is replaced by the first write.
    /// </summary>
    internal static class Transforms
    {
        private static readonly ConditionalWeakTable<Visual, Block> Blocks = new();

        private const int TranslateRank = 0, SkewRank = 1, RotateRank = 2, ScaleRank = 3;

        private sealed class Block
        {
            public TransformGroup? Group;
            public TranslateTransform? Translate;
            public SkewTransform? Skew;
            public RotateTransform? Rotate;
            public ScaleTransform? Scale;
        }

        // ---- reads (never attach; identity until the first write) -------------------------

        internal static double ReadScaleX(Visual? t)
        {
            if (t is null)
                return 1.0;
            if (Blocks.TryGetValue(t, out var b) && b.Scale is { } s)
                return s.ScaleX;
            return t.RenderTransform is ScaleTransform bare ? bare.ScaleX : 1.0;
        }

        internal static double ReadScaleY(Visual? t)
        {
            if (t is null)
                return 1.0;
            if (Blocks.TryGetValue(t, out var b) && b.Scale is { } s)
                return s.ScaleY;
            return t.RenderTransform is ScaleTransform bare ? bare.ScaleY : 1.0;
        }

        internal static double ReadTranslateX(Visual? t) =>
            t is not null && Blocks.TryGetValue(t, out var b) && b.Translate is { } tr ? tr.X : 0.0;

        internal static double ReadTranslateY(Visual? t) =>
            t is not null && Blocks.TryGetValue(t, out var b) && b.Translate is { } tr ? tr.Y : 0.0;

        internal static double ReadRotate(Visual? t) =>
            t is not null && Blocks.TryGetValue(t, out var b) && b.Rotate is { } r ? r.Angle : 0.0;

        internal static double ReadSkewX(Visual? t) =>
            t is not null && Blocks.TryGetValue(t, out var b) && b.Skew is { } sk ? sk.AngleX : 0.0;

        internal static double ReadSkewY(Visual? t) =>
            t is not null && Blocks.TryGetValue(t, out var b) && b.Skew is { } sk ? sk.AngleY : 0.0;

        // ---- writes (attach the block once, then mutate children in place) ----------------

        internal static void WriteScale(Visual t, double v)
        {
            var b = EnsureScale(t);
            b.Scale!.ScaleX = v;
            b.Scale.ScaleY = v;
        }

        internal static void WriteScaleX(Visual t, double v) => EnsureScale(t).Scale!.ScaleX = v;

        internal static void WriteScaleY(Visual t, double v) => EnsureScale(t).Scale!.ScaleY = v;

        internal static void WriteTranslateX(Visual t, double v)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            b.Translate ??= AddChild(b.Group!, new TranslateTransform(), TranslateRank);
            b.Translate.X = v;
        }

        internal static void WriteTranslateY(Visual t, double v)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            b.Translate ??= AddChild(b.Group!, new TranslateTransform(), TranslateRank);
            b.Translate.Y = v;
        }

        internal static void WriteRotate(Visual t, double v)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            b.Rotate ??= AddChild(b.Group!, new RotateTransform(), RotateRank);
            b.Rotate.Angle = v;
        }

        internal static void WriteSkewX(Visual t, double v)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            b.Skew ??= AddChild(b.Group!, new SkewTransform(), SkewRank);
            b.Skew.AngleX = v;
        }

        internal static void WriteSkewY(Visual t, double v)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            b.Skew ??= AddChild(b.Group!, new SkewTransform(), SkewRank);
            b.Skew.AngleY = v;
        }

        // ---- the block --------------------------------------------------------------------

        private static Block BlockOf(Visual t) =>
            Blocks.TryGetValue(t, out var b) ? b : Blocks.GetValue(t, _ => new Block());

        private static Block EnsureScale(Visual t)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            if (b.Scale is null)
                b.Scale = AddChild(b.Group!, new ScaleTransform(1, 1), ScaleRank);
            return b;
        }

        private static void EnsureGroup(Visual t, Block b)
        {
            if (b.Group is { } g && ReferenceEquals(t.RenderTransform, g))
                return;
            // (Re-)attach. The previous group, if any, was displaced by an external
            // RenderTransform write: release its children so they can move over.
            b.Group?.Children.Clear();
            var group = new TransformGroup();
            b.Group = group;
            // Adopt a pre-existing bare scale on first attach (pose continuity with the old
            // engines' read rule); anything else is replaced by the first write.
            if (b.Scale is null && t.RenderTransform is ScaleTransform existing)
                b.Scale = new ScaleTransform(existing.ScaleX, existing.ScaleY);
            // Every child the block already owns moves to the new group with its pose — a
            // re-attach must never orphan translate/rotate/skew (their writes would vanish).
            if (b.Translate is { } translate)
                AddChild(group, translate, TranslateRank);
            if (b.Skew is { } skew)
                AddChild(group, skew, SkewRank);
            if (b.Rotate is { } rotate)
                AddChild(group, rotate, RotateRank);
            if (b.Scale is { } scale)
                AddChild(group, scale, ScaleRank);
            t.RenderTransform = group; // the attach-once write that schedules the frame
        }

        private static T AddChild<T>(TransformGroup group, T child, int rank) where T : Transform
        {
            int index = 0;
            foreach (var existing in group.Children)
            {
                if (Rank(existing) < rank)
                    index++;
                else
                    break;
            }
            group.Children.Insert(index, child);
            return child;
        }

        private static int Rank(Transform t) => t switch
        {
            TranslateTransform => TranslateRank,
            SkewTransform => SkewRank,
            RotateTransform => RotateRank,
            _ => ScaleRank,
        };
    }

    /// <summary>
    /// The drop-shadow half of the single <see cref="Visual.Effect"/> slot: shadow
    /// channels mutate in place the DropShadowEffect holding the slot (adopting a
    /// template-placed one, replacing any other kind — the Blur precedent). Opacity is
    /// the presence channel: below the 0.02 threshold the effect leaves the slot
    /// entirely — no shader pass once the shadow has faded. The slot is single: blur and
    /// shadow are mutually exclusive on one target, the last writer owning it.
    /// </summary>
    internal static class ShadowEffects
    {
        private const double DetachBelow = 0.02;

        internal static void WriteBlur(Visual t, double v) => Effect(t).BlurRadius = v;

        internal static void WriteOffsetX(Visual t, double v) => Effect(t).OffsetX = v;

        internal static void WriteOffsetY(Visual t, double v) => Effect(t).OffsetY = v;

        internal static void WriteOpacity(Visual t, double v)
        {
            if (v < DetachBelow)
            {
                // Only a DropShadow leaves the slot — a foreign effect kind is never
                // detached by the shadow channels.
                if (t.Effect is DropShadowEffect)
                    t.Effect = null;
                return;
            }
            Effect(t).Opacity = v;
        }

        private static DropShadowEffect Effect(Visual t) => OwnedEffects.DropShadow(t);
    }

    /// <summary>
    /// The effects the engine may mutate in place: only instances it created itself. A
    /// foreign effect in the slot — typically a style setter value, ONE instance shared by
    /// every control the style matches — is never mutated: the first write copies it into
    /// an engine-owned instance (pose continuity) and takes the slot with that copy.
    /// </summary>
    internal static class OwnedEffects
    {
        private static readonly ConditionalWeakTable<Visual, IEffect> Owned = new();

        internal static BlurEffect Blur(Visual t)
        {
            if (t.Effect is BlurEffect existing && IsOwned(t, existing))
                return existing;
            var own = new BlurEffect { Radius = (t.Effect as BlurEffect)?.Radius ?? 0.0 };
            return Take(t, own);
        }

        internal static DropShadowEffect DropShadow(Visual t)
        {
            if (t.Effect is DropShadowEffect existing && IsOwned(t, existing))
                return existing;
            var own = t.Effect is DropShadowEffect foreign
                ? new DropShadowEffect
                {
                    BlurRadius = foreign.BlurRadius,
                    Color = foreign.Color,
                    Opacity = foreign.Opacity,
                    OffsetX = foreign.OffsetX,
                    OffsetY = foreign.OffsetY,
                }
                : new DropShadowEffect();
            return Take(t, own);
        }

        private static bool IsOwned(Visual t, IEffect effect) =>
            Owned.TryGetValue(t, out var owned) && ReferenceEquals(owned, effect);

        private static T Take<T>(Visual t, T effect) where T : class, IEffect
        {
            Owned.AddOrUpdate(t, effect);
            t.Effect = effect; // the attach-once write that schedules the frame
            return effect;
        }
    }
}
