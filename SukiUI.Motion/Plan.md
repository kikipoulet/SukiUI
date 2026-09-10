L'API visée — canaux fortement typés
// ── PRESS — le comportement button actuel, exactement, raconté en 20 lignes
public static class SukiPress
{
    private const double HoverScale = 1.02, PressDepth = 0.96, DeepFloor = 0.87;
    private static readonly Easing HoverEase = new CubicEaseOut();
    private static readonly Easing PressEase = new SukiEaseElasticIn { Damping: 2.5, Frequency: 3 };
    private static readonly Easing DeepEase = new LinearEasing();
    // omega/decay = vérité de bas niveau (pixels du moteur actuel) ; le sucre
    // Spring(duration:, bounce:) à la SwiftUI sert à écrire du neuf à l'oreille.
    private static readonly Spring Release = new(omega: 16.0, decay: 9.333);

    public static Mover Attach(Control button)
    {
        var c = Motion.For(button);
        var scale = c.Scale;

        // Équilibre courant, re-résolu aux entrées/sorties du pointeur :
        // en plein rebond, la cible se déplace sans snap, depuis pose + vélocité.
        var relax = scale.To(() => button.IsPointerOver ? HoverScale : 1.0).Spring(Release);

        return c
            .OnPointerEntered(scale.To(HoverScale).Over(150).Ease(HoverEase))
            .OnPointerExited (scale.To(1.0).Over(150).Ease(HoverEase))
            // Descente garantie jusqu'au bout : une release d'entre-temps est mémorisée
            // (re-press = purge l'attente et réarme depuis la pose courante), puis
            // enfoncement profond tant que l'appui tient, hold au plancher.
            .OnPointerPressed(scale.To(PressDepth).Over(150).Ease(PressEase).MustFinish()
                .Then(scale.To(DeepFloor).Over(2000).Ease(DeepEase)))
            .OnPointerReleased(relax)
            .OnPointerCaptureLost(relax)
            .OnDetachedFromVisualTree(scale.To(1.0));
    }
}

// ── POPUP — le comportement combobox actuel, exactement
public static class SukiPopup
{
    private static readonly Spring Open = new(omega: 16.0, decay: 20.8);     // ≈ duration 0.42, bounce 0.35
    private static readonly Spring Close = new(omega: 26.7, decay: 34.7);    // ≈ duration 0.25, bounce 0.35

    public static Mover Attach(ComboBox combo)
    {
        var host = Motion.For(combo);
        var popup = Motion.Popup(combo, "PART_SukiPopup");          // la lib résout les parts, re-wire à
        var surface = Motion.Part(combo, "PART_LayoutTransform");   // chaque TemplateApplied
        var items = Motion.Items(combo, "PART_ItemsPresenter");
        var (x, y, o, blur) = (surface.ScaleX, surface.ScaleY, surface.Opacity, surface.Blur);

        // Canaux dérivés : rééchantillonnés chaque frame, meurent avec le programme.
        var motionBlur = blur.Follow(() =>
            Ramp.Map(Math.Abs(x.Velocity) + Math.Abs(y.Velocity), from: 0, to: 3).Lerp(0, 12));
        var dissolveBlur = blur.Follow(() => Ramp.Map(1.0 - o.Value).Lerp(0, 20));

        var cascade = Motion.Stagger(items)
            .To(1.0).Over(250).After(150)
            .StaggerBy(count => Ramp.Map(count, from: 4, to: 10).Lerp(40, 20))  // la formule actuelle, nommée
            .SkipAbove(20);

        // Show = vrai Popup.IsOpen=true ; les From du bloc sont écrits AVANT (pas de flash)
        // et ignorés si le canal est en vol (reopen mid-collapse = reprise pose + vélocité).
        var open = popup.Show()
            .And(x.From(0.92).To(1.0).Spring(Open))
            .And(y.From(0.72).To(1.0).Spring(Open))
            .And(o.From(0).To(1.0).Over(350))
            .And(motionBlur)
            .And(cascade);

        var close = x.To(0.968).Spring(Close)
            .And(y.To(0.888).Spring(Close))
            .And(o.To(0).Over(150))
            .And(dissolveBlur)
            .Then(popup.Hide());                                    // IsOpen=false seulement au settle

        return host                                                  // déclencheurs = noms Avalonia, câblage auto
            .OnPropertyChanged(ComboBox.IsDropDownOpenProperty, open, when: true)
            .OnPropertyChanged(ComboBox.IsDropDownOpenProperty, close, when: false)
            .OnTopLevelPointerPressed(Motion.Do(() => combo.IsDropDownOpen = false), handledEventsToo: true)
            .OnWindowDeactivated(close.Instant().And(Motion.Do(() => combo.IsDropDownOpen = false)));
    }
}
Cahier des charges — mini
Ce qu'on fait. Le moteur déjà prouvé dans ControlsAnimation (SukiTicker partagé, SukiSpring, interruption depuis pose + vélocité, retarget à chaud) est déplacé tel quel dans une couche interne SukiUI/Motion. Les comportements press (bouton/combobox) et popup (dropdown), aujourd'hui écrits en code physique impératif (~275 l + ~550 l), deviennent les deux fichiers de description ci-dessus : canaux → trajectoires nommées → chorégraphies → branchement. Plus une ligne de ticker, d'intégrateur, de Transitions/Animation Avalonia, d'AddHandler ou de RenderTransform dans le code métier.
Canaux fortement typés : chaque cible expose des propriétés compilées — c.Scale, surface.ScaleX/ScaleY/Opacity/Blur, tout le reste via Property(AvaloniaProperty<T>). Une faute de canal casse à la compilation ; le seul string restant est le nom de PART_ (inhérent à Avalonia — exception explicite si la part manque). Les canaux doubles portent la physique (.Velocity, .Spring) ; les canaux T (Color, Vector…) viennent avec des interpolateurs, phase 2.
Ce que la lib garantit (règles apprises du code actuel, chacune = une branche existante généralisée) : interrompre = reprendre depuis pose + vélocité ; MustFinish() = intention, pas file (release mémorisée, re-press purge et réarme) ; .Then gardé par l'intention active ; le hover ne préempte jamais la chaîne press ni un ressort (il ne fait que re-timer/repos) ; settle 0.0005/0.02 + snap exact multi-canaux ; programme sur popup fermé = pose simple ; pré-pose des From avant Show() ; Popup.IsOpen anormal, TemplateApplied, detach = cycle de vie owned par la lib ; Dispose laisse les contrôles fonctionnels. Vocabulaire emprunté à SwiftUI uniquement comme sucre : Spring(duration:, bounce:), WithVelocity (le kick du shake), arguments () => résolus au démarrage, Ramp.Map().Lerp() (pur helper), Spring.Between.
Inchangés : tout le XAML (SukiPress.Enable/Preset, SukiPopupAnimation.Enable/Preset, SukiAnimationTheme, les PART_…), les templates, FluentAnimator/legacy. Concessions : les valeurs restent lues des profils existants pendant les ports (switch live intact ; formes « en dur » = cible quand les profils sauteront), et le pilotage programmatique de la page de stress devient Mover.Simulate(Gesture.PointerPressed…).
