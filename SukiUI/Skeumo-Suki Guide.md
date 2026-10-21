---
name: skeumo-suki
description: Concevoir des contrôles "Skeumo-Suki" pour Avalonia + SukiUI, c'est-à-dire des objets physiques (vannes, leviers, réservoirs, appareils, boutons d'arrêt) dessinés en skeuomorphisme mais parfaitement intégrés au glassmorphism de SukiUI. À utiliser quand une interface SukiUI doit rendre un appareil ou un geste réel immersif et intuitif sans jurer avec le reste de l'application.
---

# Skeumo-Suki

> Des objets physiques dessinés avec la grammaire du verre Suki : **matière neutre teintée par le thème, une seule lumière douce, l'accent comme seule énergie, et un réalisme porté par le comportement plutôt que par la texture.**

SukiUI repose sur le glassmorphism : des cartes translucides, des bords lumineux fins, une couleur d'accent et beaucoup d'air. C'est élégant et clair, mais abstrait. Parfois, on veut qu'un écran donne l'impression de **manipuler la chose elle-même** : pousser un piston, tourner une vanne, basculer un levier, voir la lumière d'un appareil changer quand on règle un paramètre.

Le skeuomorphisme classique (cuir, bois, métal en bitmap, ombres portées lourdes) jure immédiatement avec SukiUI. Le Skeumo-Suki résout ce conflit. Il emprunte au skeuomorphisme sa **forme** et sa **physique**, mais garde la **surface**, la **lumière** et la **palette** de SukiUI.

**Pourquoi on fait du Skeumo-Suki.** Dans la pratique, c'est presque toujours pour la même raison : faire construire par une IA un contrôle très complexe (dessin, lumière, physique, thèmes) qu'un programmeur n'aurait jamais dessiné lui-même, puis l'utiliser comme n'importe quel contrôle ordinaire. Le programmeur ne voit qu'une API simple. Il lie des propriétés, écoute un événement, et son code logique reste clair. Toute la complexité reste enfermée dans le composant. C'est pour cela que l'organisation du code (§6.5) compte autant que le rendu : un contrôle superbe dont l'API n'est pas séparée de la mécanique visuelle a manqué son objectif.

**Comment lire ce guide.** Il transmet des **raisons**, pas des designs. Les chiffres, les couleurs et les exemples qu'il contient décrivent des cas rencontrés : ils montrent comment un principe s'est traduit à un moment donné, pas ce que doit devenir le prochain contrôle. Face à un nouvel appareil, on applique la raison (reconnaissance, lisibilité, discrétion, cohérence avec Suki), puis on règle les valeurs à l'écran. Quand une valeur du guide et la raison qui la porte divergent, la raison gagne.

---

## Sommaire

1. [Quand l'utiliser (et quand ne pas l'utiliser)](#1-quand-lutiliser-et-quand-ne-pas-lutiliser)
2. [Les dix lois](#2-les-dix-lois)
3. [Système de tokens](#3-système-de-tokens)
4. [Modèle de lumière](#4-modèle-de-lumière)
5. [Recettes de matériaux](#5-recettes-de-matériaux)
6. [Anatomie et composition d'une carte](#6-anatomie-et-composition-dune-carte)
7. [Choisir le bon contrôle](#7-choisir-le-bon-contrôle)
8. [Comportement et physique](#8-comportement-et-physique)
9. [Thèmes clair et sombre, couleur d'accent](#9-thèmes-clair-et-sombre-couleur-daccent)
10. [Pièges techniques Avalonia](#10-pièges-techniques-avalonia)
11. [Méthode de travail](#11-méthode-de-travail)
12. [Anti-patterns](#12-anti-patterns)
13. [Checklist de validation](#13-checklist-de-validation)
14. [Exemple complet : cuve et levier de remplissage / vidange](#14-exemple-complet--cuve-et-levier-de-remplissage--vidange)

---

## 1. Quand l'utiliser (et quand ne pas l'utiliser)

**Utilisez le Skeumo-Suki quand :**

- l'utilisateur pilote un **appareil physique réel** (pompe, moteur, caméra, vanne, actionneur) et doit le reconnaître ;
- le réglage est **continu, analogique ou gestuel** : une course, un angle, une vitesse, un niveau ;
- l'effet d'un paramètre est **difficile à comprendre en chiffres** et facile à comprendre en le voyant (plus de lumière, plus de bruit, plus de débit) ;
- l'état de l'appareil doit se lire **d'un coup d'œil** : vanne ouverte ou fermée, liquide en mouvement, moteur en marche.

**Ne l'utilisez pas quand :**

- l'écran sert à saisir des données, à configurer ou à naviguer : restez en SukiUI pur ;
- la valeur doit être **exacte** : un champ numérique Suki sera toujours meilleur qu'un bouton rotatif ;
- l'objet n'a pas d'équivalent physique crédible. N'inventez pas de métaphore décorative.


---

## 2. Les dix lois

### Loi 1 : Réutiliser GlassCard si on a besoin d'un conteneur
Le conteneur est toujours une `GlassCard` (titre, padding, coins, badges en verre). L'objet skeuo flotte à l'intérieur, **sans sol, sans scène décorative, sans cadre**. Les pièces qui appartiennent vraiment à l'appareil restent bienvenues : les pieds d'une cuve, la platine vissée d'un levier. Ce qui est interdit, c'est le décor : un sol, une ombre portée au sol, un plateau sur lequel on poserait l'objet. Si l'objet a des entrées ou des sorties (tuyau, câble), elles ne s'arrêtent pas net au bord : elles **se fondent dans le verre** par un fondu long (§5.8).

### Loi 2 : Le métal est teinté par le thème
Aucun gris neutre, aucune couleur chaude. Toutes les matières dures (aluminium, acier, plastique technique) utilisent la **rampe bleu-ardoise froide** qui correspond aux fonds de SukiUI (voir §3). L'objet a l'air « anodisé dans le thème ».

### Loi 3 : La matière est neutre, l'énergie est l'accent
C'est la loi centrale. Tout ce qui est **vivant, actif, porteur d'état** est dessiné en `SukiPrimaryColor` et `SukiPrimaryColor75` : un liquide, une lumière, un perçage ouvert, un indicateur, le remplissage d'un rail, une lueur. Tout le reste est inerte. Conséquence : changer la couleur du thème Suki recolore le liquide. **L'accent devient une matière physique.**

**Quand le réel impose sa couleur.** La loi sert la reconnaissance, elle ne l'emporte pas sur elle. Certaines choses ont une couleur que tout le monde connaît : une lumière a une teinte, l'électricité a la sienne, un niveau de charge se lit avec des couleurs d'alerte, un gaz n'est pas coloré comme un liquide. Là, imposer l'accent trahit l'objet et se lit comme une paresse : on l'a constaté en voyant des indicateurs, une lampe et un souffle d'air rejetés tant qu'ils étaient en accent. Le test est une question : *de quelle couleur est la vraie chose ?* Si elle en a une, on la suit (§3.5). L'accent reste pour ce qui n'a pas de couleur propre : un remplissage générique, la course d'un rail, l'arc d'un bouton, un liquide quelconque. 

### Loi 4 : Une seule lumière, la même que celle du verre
La source vient d'en haut à gauche, partout et toujours. Chaque pièce porte un **filet spéculaire blanc fin (1,5 à 2,5 px) en haut**, comme le bord lumineux d'une GlassCard. Les ombres sont courtes et douces, comme les élévations Suki.

### Loi 5 : Le verre de l'interface devient le verre de l'objet
Là où la réalité contient du verre (réservoir, tube, hublot, lentille, chambre), on utilise la transparence, un double liseré et un reflet. Le glassmorphism ne décore plus : il justifie l'objet.

### Loi 6 : Vectoriel, géométrique, peu détaillé
Pas de bitmap, pas de bruit, pas de texture. Le « métal brossé » n'est qu'un dégradé à 4 ou 5 arrêts. Les formes sont des primitives avec de **grands rayons d'angle**, comme les cartes Suki. Les détails sont **rares et justes** (vis, moletage, ouïes, gravure, voyant), chacun tracé en 1 px avec de l'alpha. On suggère, on ne rend pas.

### Loi 7 : Ressembler au vrai matériel
Dessinez **l'appareil que l'utilisateur a réellement devant lui**, avec ses pièces caractéristiques (type de monture, type de connecteur, forme du boîtier, inscriptions techniques). La reconnaissance fait l'immersion.

La **silhouette** passe avant tout. Un tube étroit et haut se lit comme une seringue ou une éprouvette, pas comme une cuve. Une cuve, c'est un corps large aux épaules très arrondies, une bride en haut par où entre l'alimentation, une sortie en bas, et des pieds. Si l'objet ne se reconnaît pas en aplat, aucun dégradé ne le sauvera (§11).

**Chaque objet parle son propre langage.** On ne prête pas à un appareil la métaphore d'un autre : un accumulateur qui se remplit comme un réservoir de liquide se lit comme un réservoir, pas comme un accumulateur. De même, un gaz ne se lit pas avec les codes d'un liquide (surface, gouttes, particules discrètes) : sa matière est diffuse, sans bord net, et ce sont ces qualités qui le distinguent. Avant de dessiner, on se demande quel est l'idiome visuel *propre* de ce que l'on représente.

**Le corps est skeuomorphe, l'affichage est du design.** Un indicateur intégré à un appareil (niveau, charge, état) est de l'information avant d'être un objet : si on lui rend la même matérialité que le boîtier (capuchons, ombrages de cylindre, bords métalliques par segment), il se lit comme du décor et perd en lisibilité. On garde le réalisme pour ce qu'on manipule et ce qui contient, et on traite ce qui *se lit* avec la sobriété d'un graphisme d'interface.

### Loi 8 : Deux thèmes, toujours
Chaque matière existe en version **sombre** et **claire**. En clair, le métal devient argent pâle et les ombres sont teintées bleu nuit au lieu du noir. Le skeuo classique ne vit que dans un seul éclairage. Ici, l'objet se ré-anodise avec le thème.

### Loi 9 : Le réalisme est dans le comportement
Ressorts, crans, inertie, enfoncement, écoulement, bulles, reflets qui avancent. L'objet n'est pas une illustration, c'est un **afficheur de conséquences** : quand on règle un paramètre, la matière montre ce qu'il fait.

### Loi 10 : La précision reste du Suki standard
Le skeuo sert aux gestes. Dès qu'il faut une **valeur exacte**, on revient aux composants Suki : champ de saisie avec unité fixe, `NumericUpDown`, bouton `Flat`, `ToggleSwitch`. Les deux mondes cohabitent dans la même carte.

Mais n'ajoutez un champ exact **que si l'utilisateur en a réellement besoin**. Un levier de jog n'a pas de valeur à saisir : lui accoler un champ « niveau cible » par principe alourdit la carte sans rien apporter. Le geste se suffit quand l'intention est « plus » ou « moins » ; le champ exact s'impose quand l'intention est « exactement tant ».

---

## 3. Système de tokens

Déclarez les matières comme **ressources nommées par rôle**, dans des `ThemeDictionaries`. Préfixez-les par un code court propre au composant (`Vx`, `Pm`…) pour éviter les collisions entre cartes.

### 3.1 Rampe métal (rôle par rôle)

| Rôle | Sombre | Clair | Usage |
|---|---|---|---|
| Spéculaire | `#E3E9F4` | `#FFFFFF` | Sommet des dégradés métal, reflets |
| Clair | `#8D99B1` | `#E3E9F4` | Face éclairée |
| Moyen | `#586278` | `#D9E0EC` | Haut d'un boîtier |
| Corps | `#3B4558` | `#CBD3E0` | Ton principal d'une pièce |
| Profond | `#2A3242` | `#B0BBCE` | Bas d'une pièce |
| Ombre | `#171C25` | `#98A4B9` | Fond du dégradé, ombres internes |
| Arête (`Edge`) | `#667289` | `#A3AEC1` | Contour 0,6 à 1,2 px de toute pièce |
| Creux (`Recess`) | `#C705080D` | `#FF5E6A84` | Perçages, alésages, fonds de cavité |
| Fente / vis | `#0B0F15` | `#3A465C` | Fente de vis, ouïes |

### 3.2 Verre

| Rôle | Sombre | Clair |
|---|---|---|
| Fond du verre (`GlassBack`) | `#B305080D → #591A2233 → #4D1A2233 → #BF05080D` (horizontal) | `#4D6F7F9E → #14FFFFFF → #10FFFFFF → #4D6F7F9E` |
| Liseré extérieur (`GlassRim`) | `#47FFFFFF` | `#73607090` |
| Liseré intérieur (`GlassInner`) | `#59000000` | `#40405070` |
| Bord de tube (`TubeRim`) | `#57FFFFFF` | `#FF98A4B9` |
| Corps de tube (`TubeBg`) | `#B805080D` | `#FFD3DBE8` |
| Cœur de tube (`TubeCore`) | `#00FFFFFF` (inutile en sombre) | `#FFF4F7FB` |

Le verre est plus sombre sur les bords qu'au centre (effet d'épaisseur vue de face) : c'est ce que fait le dégradé horizontal de `GlassBack`, et c'est ce que font les trois couches d'un tube (bord → corps → cœur, du plus large au plus étroit).

**Pourquoi le corps de tube est opaque en clair.** Un tube coudé se dessine en traits superposés (§5.8) : le corps est posé **par-dessus** le bord. Si le corps est presque transparent, comme le fond d'un réservoir, on voit le bord à travers sur toute la largeur et le tube devient un boudin gris-bleu foncé et uni. En sombre, le corps est déjà presque opaque, donc le problème n'apparaît pas. En clair, le corps doit être opaque et clair, et le cœur encore plus clair.

### 3.3 Ombres

| Rôle | Sombre | Clair |
|---|---|---|
| Élévation pièce (`Shadow`) | `0 3 6 0 #80000000` | `0 3 6 0 #3D283250` |
| Bouton relief (`RaisedShadow`) | `0 4 10 0 #66000000, inset 0 1 0 0 #66FFFFFF` | `0 3 8 0 #33283250, inset 0 1 0 0 #99FFFFFF` |
| Petit dôme (`DomeShadow`) | `0 2 5 0 #99000000` | `0 2 4 0 #4D283250` |
| Fenêtre creusée (`Inset`) | `inset 0 2 8 0 #CC000000` | `inset 0 1 4 0 #33283250` |
| Rail creusé | `inset 0 1 3 0 #AA000000` | `inset 0 1 2 0 #33000000` |

En clair, **jamais de noir pur dans les ombres** : utilisez `#283250` avec de l'alpha.

### 3.4 Énergie (accent)

| Rôle | Ressource |
|---|---|
| Liquide, remplissage, lumière active | `SukiPrimaryColor` |
| Bords d'un liquide, fin de dégradé | `SukiPrimaryColor75` |
| Arrêt, danger, alarme | `SukiDangerColor` |
| Texte actif sur l'objet | `SukiText` |
| Texte inactif, graduations, légendes | `SukiLowText` |
| Séparateurs, bords neutres | `SukiBorderBrush` |

Note : `SukiPrimaryColor` et `SukiText` sont des **`Color`**, pas des pinceaux. Dans un `GradientStop`, utilisez `Color="{DynamicResource SukiPrimaryColor}"`. Dans un `Background` ou un `Foreground`, `{DynamicResource SukiPrimaryColor}` fonctionne aussi. En code, voir §9.3.

### 3.5 Couleurs « réelles »

**La règle.** Quand l'objet représenté a une couleur conventionnelle forte, on la prend, au lieu de l'accent (Loi 3). **Pourquoi :** la reconnaissance fait l'immersion (Loi 7), et une couleur contraire à la convention oblige l'utilisateur à traduire.

**Ce qui la borne :**
- la couleur réelle est réservée à ce qui porte la convention (la lumière, l'indicateur de niveau, la matière dont on parle). Le corps de l'objet reste en rampe métal ;
- elle existe en version sombre et claire comme toute matière (Loi 8) et reste dans la saturation du reste de l'objet : un indicateur ne doit pas être le point le plus criard de la carte ;
- une même couleur n'a qu'un sens dans toute la carte (le rouge d'alerte n'est pas aussi la couleur d'un fil) ;
- le rouge d'arrêt et de danger vient toujours de `SukiDangerColor`.

**Cas rencontrés** (illustrations, pas modèles) : un voyant d'état, un indicateur de niveau gradué par seuils, la lumière d'une ampoule, le dôme d'un arrêt d'urgence.

---

## 4. Modèle de lumière

```
        ☀  source : haut-gauche (≈ 35 %, 30 %)
         ╲
          ╲   ┌──────────────────────┐ ← filet spéculaire blanc 1,5–2,5 px
           ╲  │░░░░  reflet net ░░   │ ← reflet diagonal à bord franc
              │                      │
              │       corps          │ ← dégradé vertical clair → sombre
              │                      │
              └──────────────────────┘
                 ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓      ← ombre courte et douce (0 3 6)
```

1. **Dégradés directionnels.**
   - Boîtiers : vertical, du clair (haut) au sombre (bas).
   - Pièces cylindriques horizontales (tige, poignée, tube métal) : vertical, avec le point le plus clair vers 28-35 %.
   - Pièces cylindriques verticales : même chose à l'horizontale.
2. **Pièces rondes** (pommeau, bouton, axe, lentille) : `RadialGradientBrush GradientOrigin="35%,30%"`.
3. **Filet spéculaire** : un `Border` de 1,5 à 2,5 px de haut, coins arrondis, `#2E–#99FFFFFF`, placé 1 à 2 px sous le bord supérieur et légèrement plus étroit que la pièce.
4. **Reflet de verre** : un dégradé diagonal avec une **coupure nette**, signature du glassmorphism :
   ```xml
   <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,100%">
       <GradientStop Offset="0"    Color="#38FFFFFF" />
       <GradientStop Offset="0.35" Color="#0AFFFFFF" />
       <GradientStop Offset="0.36" Color="#00FFFFFF" />  <!-- bord franc -->
       <GradientStop Offset="1"    Color="#00FFFFFF" />
   </LinearGradientBrush>
   ```
5. **Biseau** (bord d'une plaque) : un calque par-dessus avec un dégradé vertical `#B3FFFFFF` (0) → transparent (0.1) → transparent (0.9) → `#4D000000` (1).
6. **Lumière émise** : toujours en accent, sous forme de `BoxShadow` coloré flou (voir §5.11). La lumière émise est la seule chose « néon » de l'objet.

---

## 5. Recettes de matériaux

Toutes les recettes sont en Avalonia XAML. Les clés `Xx…` sont à renommer avec le préfixe de votre composant.

### 5.1 Boîtier métal (corps d'un appareil)

```xml
<!-- Dark -->
<LinearGradientBrush x:Key="XxBody" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"    Color="#FF586278" />
    <GradientStop Offset="0.12" Color="#FF3B4558" />
    <GradientStop Offset="0.55" Color="#FF2A3242" />
    <GradientStop Offset="1"    Color="#FF171C25" />
</LinearGradientBrush>
<!-- Light -->
<LinearGradientBrush x:Key="XxBody" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"    Color="#FFFFFFFF" />
    <GradientStop Offset="0.12" Color="#FFF1F4F9" />
    <GradientStop Offset="0.55" Color="#FFD9E0EC" />
    <GradientStop Offset="1"    Color="#FFB0BBCE" />
</LinearGradientBrush>
```

```xml
<Border Width="80" Height="86" CornerRadius="12"
        Background="{DynamicResource XxBody}"
        BorderBrush="{DynamicResource XxEdge}" BorderThickness="1.2"
        BoxShadow="{DynamicResource XxShadow}" />
<!-- reflet diagonal par-dessus -->
<Border Width="80" Height="86" CornerRadius="12" Background="{StaticResource XxSheen}" IsHitTestVisible="False" />
<!-- filet spéculaire -->
<Border Width="72" Height="2.5" CornerRadius="1.2" Background="#2EFFFFFF" Margin="0,2,0,0" VerticalAlignment="Top" />
```

### 5.2 Métal usiné / chromé (tige, poignée, collerette)

Pour une pièce **horizontale**, dégradé vertical (et l'inverse pour une pièce verticale) :

```xml
<!-- Dark -->
<LinearGradientBrush x:Key="XxRod" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"    Color="#FF3B4558" />
    <GradientStop Offset="0.28" Color="#FFE3E9F4" />  <!-- ligne de lumière -->
    <GradientStop Offset="0.62" Color="#FF8590A6" />
    <GradientStop Offset="1"    Color="#FF262D3A" />
</LinearGradientBrush>
<!-- Light -->
<LinearGradientBrush x:Key="XxRod" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"    Color="#FF98A4B9" />
    <GradientStop Offset="0.28" Color="#FFFFFFFF" />
    <GradientStop Offset="0.62" Color="#FFCBD3E0" />
    <GradientStop Offset="1"    Color="#FF8D99B1" />
</LinearGradientBrush>
```

C'est la même matière que la rampe du §3.1 : le chrome est simplement la rampe complète resserrée sur une petite hauteur.

### 5.3 Aluminium brossé (plaque, support)

Dégradé diagonal à 5 arrêts **non monotone**. Les alternances clair/sombre suffisent à suggérer le brossage :

```xml
<LinearGradientBrush x:Key="XxAlu" StartPoint="0%,0%" EndPoint="100%,100%">
    <GradientStop Offset="0"    Color="#FFE8ECF2" />
    <GradientStop Offset="0.25" Color="#FFB9C1CE" />
    <GradientStop Offset="0.5"  Color="#FFD9DEE7" />
    <GradientStop Offset="0.78" Color="#FF9AA4B4" />
    <GradientStop Offset="1"    Color="#FF7C8799" />
</LinearGradientBrush>
```

Ajoutez par-dessus le calque de biseau du §4.5.

### 5.4 Pommeau, bouton, axe

```xml
<!-- Dark : pommeau sombre -->
<RadialGradientBrush x:Key="XxKnob" GradientOrigin="35%,30%">
    <GradientStop Offset="0"   Color="#FF7C88A3" />
    <GradientStop Offset="0.4" Color="#FF3B4558" />
    <GradientStop Offset="0.8" Color="#FF222A38" />
    <GradientStop Offset="1"   Color="#FF141922" />
</RadialGradientBrush>
<!-- Dark : axe métal clair -->
<RadialGradientBrush x:Key="XxAxle" GradientOrigin="35%,30%">
    <GradientStop Offset="0"    Color="#FFE3E9F4" />
    <GradientStop Offset="0.4"  Color="#FF8D99B1" />
    <GradientStop Offset="0.75" Color="#FF586278" />
    <GradientStop Offset="1"    Color="#FF2A3242" />
</RadialGradientBrush>
```

```xml
<Grid Width="30" Height="30">
    <Border CornerRadius="15" Background="{DynamicResource XxKnob}"
            BorderBrush="{DynamicResource XxEdge}" BorderThickness="1"
            BoxShadow="0 3 6 0 #66000000" />
    <!-- point de brillance -->
    <Ellipse Width="9" Height="4" Margin="5,3,0,0" HorizontalAlignment="Left" VerticalAlignment="Top" Fill="#80FFFFFF" />
</Grid>
```

**Pouce de curseur « perle »** (charnière entre Suki et skeuo) : une sphère claire avec un point d'accent lumineux au centre.

```xml
<RadialGradientBrush x:Key="XxThumb" GradientOrigin="35%,28%">
    <GradientStop Offset="0"   Color="#FFF4F7FC" />
    <GradientStop Offset="0.6" Color="#FFB9C4D8" />
    <GradientStop Offset="1"   Color="#FF6F7C93" />
</RadialGradientBrush>

<Border Width="22" Height="22" CornerRadius="11" Background="{DynamicResource XxThumb}" BoxShadow="0 3 7 0 #AA000000">
    <Border Name="ThumbDot" Width="7" Height="7" CornerRadius="4" Background="{DynamicResource SukiPrimaryColor}" />
</Border>
```

### 5.5 Caoutchouc (joint, butée)

```xml
<LinearGradientBrush x:Key="XxRubber" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"   Color="#FF4A5468" />
    <GradientStop Offset="0.3" Color="#FF262D3A" />
    <GradientStop Offset="1"   Color="#FF0B0F15" />
</LinearGradientBrush>
```

Ajoutez une ou deux nervures horizontales de 1 px (`#8C667289`) et, si la pièce est en contact avec un liquide, une bande d'accent à 25 % d'opacité du côté du liquide.

### 5.6 Verre (réservoir, barillet, hublot)

Le verre se construit **en couches**, du fond vers l'avant :

```xml
<!-- 1. fond du verre -->
<Border Width="60" Height="200" CornerRadius="14,14,4,4" Background="{DynamicResource XxGlassBack}" IsHitTestVisible="False" />
<!-- 2. contenu (liquide, voir 5.7) -->
<!-- 3. liseré extérieur -->
<Border Width="60" Height="200" CornerRadius="14,14,4,4"
        BorderBrush="{DynamicResource XxGlassRim}" BorderThickness="1.6" IsHitTestVisible="False" />
<!-- 4. liseré intérieur, 2 px en retrait -->
<Border Margin="2" Width="56" Height="196" CornerRadius="12,12,3,3"
        BorderBrush="{DynamicResource XxGlassInner}" BorderThickness="1" IsHitTestVisible="False" />
<!-- 5. reflet vertical principal (côté lumière) -->
<Border Width="6" Height="168" CornerRadius="3" Opacity="0.7" IsHitTestVisible="False">
    <Border.Background>
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
            <GradientStop Offset="0"   Color="#8CFFFFFF" />
            <GradientStop Offset="0.6" Color="#26FFFFFF" />
            <GradientStop Offset="1"   Color="#00FFFFFF" />
        </LinearGradientBrush>
    </Border.Background>
</Border>
<!-- 6. reflet secondaire fin (côté opposé) -->
<Border Width="2" Height="150" CornerRadius="1" Background="#29FFFFFF" IsHitTestVisible="False" />
<!-- 7. ellipse de bord supérieur (ouverture vue en perspective) -->
<Ellipse Width="40" Height="7" Fill="#24FFFFFF" IsHitTestVisible="False" />
```

Pour un **hublot rond**, même logique : disque `RadialGradientBrush` sombre (`#1B2538 → #0C121C → #05080D`, origine 38 %,30 %), deux ellipses de liseré, et un arc blanc épais (`StrokeThickness="3"`, `#80FFFFFF`) en haut à gauche.

### 5.7 Liquide (énergie)

Le liquide **est** l'accent. Dégradé horizontal `75 → 100 → 75` pour suggérer le volume cylindrique, puis trois calques :

```xml
<Border Name="Liquid" CornerRadius="12,12,0,0" Opacity="0.95" BoxShadow="inset 0 0 12 0 #40FFFFFF">
    <Border.Background>
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
            <GradientStop Offset="0"   Color="{DynamicResource SukiPrimaryColor75}" />
            <GradientStop Offset="0.5" Color="{DynamicResource SukiPrimaryColor}" />
            <GradientStop Offset="1"   Color="{DynamicResource SukiPrimaryColor75}" />
        </LinearGradientBrush>
    </Border.Background>
    <Grid>
        <!-- coeur lumineux -->
        <Border Width="26" HorizontalAlignment="Center" IsHitTestVisible="False">
            <Border.Background>
                <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
                    <GradientStop Offset="0"   Color="#00FFFFFF" />
                    <GradientStop Offset="0.5" Color="#47FFFFFF" />
                    <GradientStop Offset="1"   Color="#00FFFFFF" />
                </LinearGradientBrush>
            </Border.Background>
        </Border>
        <!-- éclairage vertical : surface claire, fond sombre (ressource par thème) -->
        <Border Background="{DynamicResource XxLiquidShade}" IsHitTestVisible="False" />
        <!-- caustique au contact d'une paroi ou d'un joint -->
        <Border Height="10" VerticalAlignment="Bottom" IsHitTestVisible="False">
            <Border.Background>
                <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
                    <GradientStop Offset="0" Color="#00FFFFFF" />
                    <GradientStop Offset="1" Color="#47FFFFFF" />
                </LinearGradientBrush>
            </Border.Background>
        </Border>
        <!-- particules ou bulles en suspension, pilotées par le code -->
        <Canvas Name="LiquidParticles" IsHitTestVisible="False" />
    </Grid>
</Border>
```

`XxLiquidShade` (sombre) : `#38FFFFFF` (0) → `#00FFFFFF` (0.12) → `#00000000` (0.8) → `#47000000` (1). En clair, remplacez le noir du bas par `#26405070`.

**Découper le contenu à la forme du récipient.** Dès que le récipient a des épaules ou un fond arrondis, ne donnez pas de `CornerRadius` au liquide : placez le liquide, le filet qui tombe et les particules dans un `Border` intérieur avec `ClipToBounds="True"` et le rayon du verre intérieur. Le liquide est alors un simple rectangle ancré en bas, et c'est la découpe qui épouse la forme de la cuve, quel que soit le niveau :

```xml
<Border Canvas.Left="36" Canvas.Top="36" Width="96" Height="126" CornerRadius="32,32,14,14" ClipToBounds="True">
    <Canvas Width="96" Height="126">
        <Line Name="FallStream" ... />            <!-- filet qui tombe de l'entrée au remplissage -->
        <Border Name="Liquid" Width="96" Height="0" Canvas.Top="126" ... />  <!-- hauteur pilotée en code -->
        <Canvas Name="TankParticles" />           <!-- matière en suspension -->
    </Canvas>
</Border>
```

Gardez une **réserve d'air** sous le dôme : le niveau 100 % ne remplit pas tout l'intérieur (par exemple 112 px utiles sur 126).

### 5.8 Tube ou canalisation qui se fond dans la carte

Une vraie canalisation est **coudée** : elle sort d'une bride, descend, tourne et part vers le bord de la carte. On la dessine donc avec un même `Path` (lignes + courbe `Q` pour le coude) tracé plusieurs fois, du plus épais au plus fin :

```xml
<Canvas Width="288" Height="200" IsHitTestVisible="False">
    <Canvas.OpacityMask>
        <!-- fondu long et doux vers la droite (voir la règle plus bas) -->
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
            <GradientStop Offset="0.55" Color="#FFFFFFFF" />
            <GradientStop Offset="0.98" Color="#00FFFFFF" />
        </LinearGradientBrush>
    </Canvas.OpacityMask>
    <!-- 1. bord  2. corps  3. cœur : même géométrie, épaisseur décroissante -->
    <Path Data="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" Stroke="{DynamicResource XxTubeRim}"
          StrokeThickness="12" StrokeLineCap="Round" StrokeJoin="Round" />
    <Path Data="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" Stroke="{DynamicResource XxTubeBg}"
          StrokeThickness="9.6" StrokeLineCap="Round" StrokeJoin="Round" />
    <Path Data="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" Stroke="{DynamicResource XxTubeCore}"
          StrokeThickness="4.5" StrokeLineCap="Round" StrokeJoin="Round" />
    <!-- 4. énergie : opacité pilotée par le débit -->
    <Path Name="OutletFlow" Data="M 96,160 L 96,180 Q 96,192 108,192 L 288,192"
          Stroke="{DynamicResource SukiPrimaryColor}" StrokeThickness="6"
          StrokeLineCap="Round" StrokeJoin="Round" Opacity="0" />
    <!-- 5. filet spéculaire sur la partie droite, 2,8 px au-dessus de l'axe -->
    <Path Data="M 110,189.2 L 284,189.2" Stroke="#40FFFFFF" StrokeThickness="1.4" StrokeLineCap="Round" />
    <!-- 6. particules en suspension (§8.3), créées en code -->
    <Canvas Name="OutletParticles" />
</Canvas>
```

Trois règles :

- **Le fondu est long.** Il s'étale sur toute la longueur droite du tuyau, avec un arrêt intermédiaire pour une courbe douce, et non sur quelques pixels. En dessous d'environ 50 à 60 px, l'œil lit une coupure nette et non un fondu. Pour une entrée à gauche : `0.02 → #00`, `0.12 → #59`, `0.24 → #FF`.
- **Le fondu atteint zéro là où l'objet rencontre le bord de la carte.** Un fondu qui se termine avant le bord laisse un tuyau qui semble s'arrêter dans le vide ; un fondu confiné à l'intérieur du padding se lit comme une coupure. La scène doit donc s'étendre jusqu'au bord du verre (marges négatives du `Panel` si besoin), et la longueur du fondu se juge en pixels à l'écran, pas dans le code.
- **Tout ce que le tuyau contient suit la même courbe** (particules, reflets), sinon le contenu dépasse du fondu et révèle la coupure.
- **Vérifiez le fondu à l'écran.** Une technique qui devrait fonctionner (un masque d'opacité sur la couche) n'a pas produit l'effet attendu dans un cas : un dégradé porté directement par les pinceaux du tuyau en est une alternative. C'est le résultat visible qui décide, pas la technique.
- **Les extrémités se cachent.** Un `StrokeLineCap="Round"` dépasse du point final de la **moitié de l'épaisseur**. Un tube qui entre dans une bride doit s'arrêter assez tôt pour que son embout rond reste caché sous la bride (pour une épaisseur de 12, arrêtez-le au moins 6 px avant le bas de la bride).
- **Le tuyau passe derrière ou sous les pièces qu'il croise.** Une sortie qui passe près d'un pied descend plus bas que le pied et passe dessous. Un tuyau qui passe sous un autre contrôle est simplement dessiné avant lui.

Pour un tuyau droit très simple, une variante en `Border` suffit :

```xml
<Canvas Width="300" Height="60" IsHitTestVisible="False">
    <Canvas.OpacityMask>
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
            <GradientStop Offset="0.05" Color="#00FFFFFF" />
            <GradientStop Offset="0.23" Color="#FFFFFFFF" />
            <GradientStop Offset="0.77" Color="#FFFFFFFF" />
            <GradientStop Offset="0.95" Color="#00FFFFFF" />
        </LinearGradientBrush>
    </Canvas.OpacityMask>
    <!-- gaine de verre -->
    <Border Canvas.Top="15" Width="300" Height="14" CornerRadius="7"
            Background="{DynamicResource XxTubeBg}" BorderBrush="{DynamicResource XxTubeRim}" BorderThickness="1.2" />
    <!-- canal d'accent : opacité 0.7 si la voie est active, 0.1 sinon -->
    <Border Name="ChannelA" Canvas.Left="4" Canvas.Top="18.5" Width="146" Height="7" CornerRadius="3.5" Opacity="0.7">
        <Border.Background>
            <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
                <GradientStop Offset="0"   Color="{DynamicResource SukiPrimaryColor75}" />
                <GradientStop Offset="0.5" Color="{DynamicResource SukiPrimaryColor}" />
                <GradientStop Offset="1"   Color="{DynamicResource SukiPrimaryColor75}" />
            </LinearGradientBrush>
        </Border.Background>
    </Border>
    <!-- filet spéculaire du tube -->
    <Border Canvas.Left="10" Canvas.Top="16.6" Width="280" Height="2" CornerRadius="1" Background="#59FFFFFF" />
</Canvas>
```

Le masque est ce qui donne l'impression que l'objet **sort de l'interface**. Sans lui, le tube paraît coupé au cutter. Le masque est relatif aux limites du `Canvas` qui le porte : donnez à ce `Canvas` la taille de la scène pour que le fondu se termine exactement au bord du contrôle.

N'utilisez pas de `StrokeDashArray` pour faire « avancer » le liquide : des pointillés réguliers forment une file prévisible et mécanique. Le mouvement passe par des particules (§8.3).

### 5.9 Petits détails (à doser)

| Détail | Recette |
|---|---|
| Vis | `Ellipse` 3,6 à 4 px, remplissage `XxScrew` ou dégradé chrome, contour `XxEdge` 0,5 px. Avec fente : `Border` 6×1,5 px en `XxSlot`, tourné de 20 à 30° (jamais aligné à 0°). |
| Moletage | 3 à 5 lignes parallèles de 1 px en `XxGroove` (`#B30B0F15` sombre, `#554A5670` clair). |
| Ouïes, fentes | `Path` de petits rectangles en dégradé `#05070A → #232B38` (clair : `#8F9BB0 → #C3CCDB`), contour `XxEdge` 0,6 px. |
| Voyant LED | Deux ellipses superposées : halo 10 px à 20-25 % d'alpha, puis point 4,4 px plein. Éteint : point `#3A4558`. |
| Gravure | `TextBlock` 4,5 à 6 px, `LetterSpacing` 0,6 à 1,2, couleur `XxEngrave` (`#8FA4CC` / `#5A6580`), `Tag="NoTranslate"` si l'app traduit automatiquement. |
| Plaque signalétique | `Border` sombre `CornerRadius="7"`, liseré `SukiPrimaryColor` 0,8 px, nom en 8,5 px SemiBold `LetterSpacing="1.2"`, sous-titre en accent 5,5 px. |
| Connecteur | Petit bloc métal `XxMetal` + creux noir `#06080C` à l'intérieur. |
| Câble | `Path` épais (8 px, `StrokeLineCap="Round"`) en `XxCable`, doublé d'un filet clair de 1,5 px décalé de -2 px en Y. |

Règle : **3 à 6 détails par objet**. Chaque détail doit dire quelque chose de vrai sur l'appareil (il est vissé, il ventile, il est connecté, il est sous tension). Ne vissez pas tout : des vis sur la bride et sur la platine d'un levier se justifient, des vis sur chaque patte de pied ajoutent du bruit. De même, des cerclages épais en travers d'un réservoir en verre cachent ce qui compte (le liquide) : préférez de fines graduations.

### 5.10 Fenêtre creusée (variante légère)

Quand l'objet est un **écran** (courbe, image, flux), on ne dessine pas d'appareil : on creuse le verre.

```xml
<Border Width="268" Height="112" CornerRadius="14" ClipToBounds="True"
        Background="{DynamicResource XxWindowBg}"
        BorderBrush="{DynamicResource SukiBorderBrush}" BorderThickness="1">
    <Grid>
        <!-- contenu -->
        <!-- reflet de vitre très léger -->
        <Border IsHitTestVisible="False">
            <Border.Background>
                <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,100%">
                    <GradientStop Offset="0"   Color="#0FFFFFFF" />
                    <GradientStop Offset="0.4" Color="#00FFFFFF" />
                </LinearGradientBrush>
            </Border.Background>
        </Border>
        <!-- profondeur : ombre intérieure, en DERNIER pour passer au-dessus du contenu -->
        <Border CornerRadius="14" BoxShadow="{DynamicResource XxInset}" IsHitTestVisible="False" />
    </Grid>
</Border>
```

`XxWindowBg` : `#0A0E14` en sombre, `#EEF2F8` en clair. Des repères de visée en accent à 35 % d'opacité dans les coins complètent bien l'effet.

### 5.11 Lumière d'accent (glow)

Une lumière émise est un `BoxShadow` coloré à l'accent, **posé en code**, car un `BoxShadow` ne peut pas référencer une `DynamicResource` (voir §10) :

```csharp
private void ApplyAccentGlows()
{
    if (!this.TryFindResource("SukiPrimaryColor", ActualThemeVariant, out var res) || res is not Color accent) return;
    BoxShadows Glow(double blur, byte alpha, double spread = 0) =>
        new(new BoxShadow { Blur = blur, Spread = spread, Color = Color.FromArgb(alpha, accent.R, accent.G, accent.B) });

    this.Get<Border>("ThumbDot").BoxShadow = Glow(6, 0xFF);   // point lumineux
    this.Get<Border>("RailFill").BoxShadow = Glow(8, 0x66);   // remplissage de rail
    // halo à deux couches autour d'un liquide
    this.Get<Border>("LiquidAura").BoxShadow = new BoxShadows(
        new BoxShadow { Blur = 18, Spread = 4,  Color = Color.FromArgb(0xB0, accent.R, accent.G, accent.B) },
        new[] { new BoxShadow { Blur = 42, Spread = 10, Color = Color.FromArgb(0x55, accent.R, accent.G, accent.B) } });
}
```

Placez le halo **sous** l'objet (z-order) et dans un parent `ClipToBounds="False"`. Sinon la lueur est coupée net.

### 5.12 Bouton d'arrêt d'urgence

Collerette métal fixe et dôme de verre rouge rétro-éclairé. La lueur est **hors** de la collerette pour ne pas être clippée.

```xml
<Button Classes="Void" Padding="0" ClipToBounds="False" ToolTip.Tip="Stop">
    <Grid ClipToBounds="False">
        <Border Width="46" Height="46" CornerRadius="23" BoxShadow="0 0 16 3 #59E5484D" IsHitTestVisible="False" />
        <Border Width="56" Height="56" CornerRadius="28"
                Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="1"
                BoxShadow="{DynamicResource XxRaisedShadow}">
            <Grid Name="StopDome"> <!-- seule cette partie bouge à l'appui -->
                <Border Width="44" Height="44" CornerRadius="22" BoxShadow="{DynamicResource XxDomeShadow}">
                    <Border.Background>
                        <RadialGradientBrush GradientOrigin="38%,30%">
                            <GradientStop Offset="0"   Color="#FFFF8A8A" />
                            <GradientStop Offset="0.5" Color="{DynamicResource SukiDangerColor}" />
                            <GradientStop Offset="1"   Color="#FF7A1218" />
                        </RadialGradientBrush>
                    </Border.Background>
                </Border>
                <Ellipse Width="26" Height="12" Margin="0,9,0,0" VerticalAlignment="Top" IsHitTestVisible="False">
                    <Ellipse.Fill>
                        <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
                            <GradientStop Offset="0" Color="#8CFFFFFF" />
                            <GradientStop Offset="1" Color="#00FFFFFF" />
                        </LinearGradientBrush>
                    </Ellipse.Fill>
                </Ellipse>
                <TextBlock Text="STOP" FontSize="10" FontWeight="Bold" Foreground="White"
                           HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0,4,0,0" />
            </Grid>
        </Border>
    </Grid>
</Button>
```

### 5.13 Socle de levier

Un levier ne sort jamais d'un simple rond posé dans le vide. Il repose sur une **base solide et horizontale** : une platine métal vissée, sur laquelle est posé un **bossage** demi-cylindrique d'où sort la tige. L'axe chromé reste visible au centre du bossage.

```xml
<!-- la tige du levier est dessinée AVANT : elle sort du bossage -->
<!-- bossage : demi-cylindre posé sur la platine -->
<Border Canvas.Left="40" Canvas.Top="66" Width="30" Height="22" CornerRadius="15,15,3,3"
        Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="0.8"
        BoxShadow="{DynamicResource XxShadow}" />
<Border Canvas.Left="45" Canvas.Top="67.5" Width="20" Height="1.6" CornerRadius="0.8" Background="#59FFFFFF" />
<Ellipse Canvas.Left="50.5" Canvas.Top="73.5" Width="9" Height="9" Fill="{DynamicResource XxAxle}"
         Stroke="{DynamicResource XxEdge}" StrokeThickness="0.6" />
<!-- platine : bloc horizontal, biseau, filet spéculaire, deux vis -->
<Border Canvas.Left="17" Canvas.Top="86" Width="76" Height="16" CornerRadius="6"
        Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="1"
        BoxShadow="{DynamicResource XxShadow}" />
<Border Canvas.Left="17" Canvas.Top="86" Width="76" Height="16" CornerRadius="6">   <!-- biseau §4.5 -->
    <Border.Background>
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
            <GradientStop Offset="0" Color="#66FFFFFF" />
            <GradientStop Offset="0.12" Color="#00FFFFFF" />
            <GradientStop Offset="0.85" Color="#00000000" />
            <GradientStop Offset="1" Color="#33000000" />
        </LinearGradientBrush>
    </Border.Background>
</Border>
<Border Canvas.Left="22" Canvas.Top="87.5" Width="66" Height="1.6" CornerRadius="0.8" Background="#59FFFFFF" />
<Ellipse Canvas.Left="23" Canvas.Top="92" Width="4.4" Height="4.4" Fill="{DynamicResource XxAxle}"
         Stroke="{DynamicResource XxEdge}" StrokeThickness="0.5" />
<Border Canvas.Left="22.4" Canvas.Top="93.5" Width="5.6" Height="1.2" Background="{DynamicResource XxSlot}"
        RenderTransform="rotate(25deg)" />   <!-- fentes jamais alignées, angles différents d'une vis à l'autre -->
```

### 5.14 Pieds de cuve

Des **jambes inclinées** (trapèzes en `Path`), plus larges en haut qu'en bas. Elles sont fixées sur la paroi par une **patte** métal et posées sur un **patin** plat. Les jambes sont dessinées derrière le verre, et les pattes devant.

```xml
<!-- jambe : trapèze avec dégradé métal horizontal + filet de lumière sur l'arête gauche -->
<Path Data="M 47,146 L 57,146 L 50,177 L 40,177 Z" Fill="{DynamicResource XxLeg}"
      Stroke="{DynamicResource XxEdge}" StrokeThickness="0.6" StrokeJoin="Round" />
<Path Data="M 49,150 L 46.5,161" Stroke="#4DFFFFFF" StrokeThickness="1.2" StrokeLineCap="Round" />
<!-- patin -->
<Border Canvas.Left="33" Canvas.Top="175" Width="24" Height="6" CornerRadius="3"
        Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="0.8"
        BoxShadow="{DynamicResource XxShadow}" />
<Border Canvas.Left="36" Canvas.Top="175.8" Width="18" Height="1.2" CornerRadius="0.6" Background="#59FFFFFF" />
<!-- patte, devant le verre, sans vis -->
<Border Canvas.Left="42" Canvas.Top="141" Width="20" Height="9" CornerRadius="3"
        Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="0.8"
        BoxShadow="{DynamicResource XxShadow}" />
```

`XxLeg` : dégradé horizontal `#2A3242 → #8D99B1 (0.3) → #3B4558 (0.6) → #171C25` en sombre, `#B0BBCE → #FFFFFF (0.3) → #CBD3E0 (0.6) → #98A4B9` en clair.

---

## 6. Anatomie et composition d'une carte

### 6.1 Structure type

```
GlassCard (Classes="Card", Padding=16)
├── En-tête         : titre 17 DemiBold  +  badge d'état en GlassCard (point d'accent + texte 11)
├── Scène skeuo     : un ou plusieurs contrôles skeuo composés dans un Panel (la « maquette »)
├── Commandes       : contrôles Suki (valeur exacte, action), seulement s'il y en a besoin (Loi 10)
└── Aide contextuelle (optionnelle) : conseil qui n'apparaît que dans une situation à risque
```

- Hiérarchie : **titre → objet → commandes**. L'objet occupe le centre visuel.
- Largeur de carte typique : 300 à 320 px. Scène : environ 220 à 290 px de large.
- Espacements généreux entre la scène et les commandes (20 à 35 px) : l'objet a besoin d'air pour flotter.
- **Une carte = une scène = un appareil.** Une scène peut assembler plusieurs contrôles quand ils forment ensemble un seul appareil réel : une cuve et le levier qui commande ses vannes, une caméra et son bouton d'arrêt. Ce qui est interdit, c'est de juxtaposer dans une carte deux objets sans rapport.
- Dans une scène composée, **laissez de l'air entre les pièces** : un levier collé contre la cuve paraît posé dessus. Poussez l'actionneur vers le bord opposé et élargissez la carte si besoin (par exemple de 300 à 320 px).
- Les contrôles d'une scène composée sont **superposés dans un `Panel`** plutôt que juxtaposés dans une grille. Ainsi, un tuyau de la cuve peut passer sous le socle du levier et se fondre au bord de la carte :

```xml
<Panel Width="288" Height="200">
    <ss:Tank Level="{Binding Level}" Inflow="{Binding Inflow}" Outflow="{Binding Outflow}" />
    <ss:SpringLever Margin="0,46,0,0" HorizontalAlignment="Right" VerticalAlignment="Top"
                    LeftLabel="Fill" RightLabel="Drain"
                    Value="{Binding LeverValue}" Released="Lever_OnReleased" />
</Panel>
```

### 6.2 La scène : un repère de maquette fixe

Dessinez dans un `Canvas` de taille fixe (par exemple 332×112) qui reprend **exactement les coordonnées de votre maquette SVG**, et mettez-le à l'échelle avec un `Viewbox` :

```xml
<Viewbox Width="268" Height="93" Stretch="Uniform" HorizontalAlignment="Center">
    <Canvas Width="332" Height="112">
        <!-- coordonnées identiques à la maquette -->
    </Canvas>
</Viewbox>
```

Chaque contrôle a **son propre repère**, aussi petit que possible autour de sa pièce : la cuve dans 288×200, le levier dans 110×104 avec son pivot en (55,82). On ne dessine pas un levier dans le repère de la cuve. C'est la page qui place les contrôles l'un par rapport à l'autre (§6.1).

Attention : sous un `Viewbox`, les coordonnées de pointeur doivent être lues **dans le repère du Canvas** (`e.GetPosition(canvas)`), sinon l'interaction se décale.

### 6.3 Ordre des couches (z-order)

Du fond vers l'avant :

1. **Lueurs et halos** (sous tout, non cliquables)
2. **Pièces cachées** (tige derrière le verre, mécanisme interne)
3. **Fond du verre**
4. **Contenu** : liquide, particules, image
5. **Parois du verre** : liserés, reflets
6. **Pièces métal fixes** : têtes, collerettes, socle, vis
7. **Pièce manipulable** (joint, pommeau, curseur) : **toujours au premier plan**. C'est elle que la main attrape.
8. **Textes et labels** sur l'objet

Tout ce qui est purement décoratif reçoit `IsHitTestVisible="False"`. L'interaction est gérée par un seul `Canvas` parent avec `Background="Transparent"` (indispensable pour recevoir les clics) et des **zones de saisie larges** testées en code : plus large que la pièce visible, c'est plus confortable.

### 6.4 Texte sur l'objet

- Labels d'état (« In » / « Out », « Fill » / « Drain ») : 11 à 14 px Medium/DemiBold. **Actif = `SukiText`, inactif = `SukiLowText`.** C'est le texte qui indique l'état, pas une couleur ajoutée. Basculez une classe (`Classes.Set("Active", …)`) et laissez un style faire le reste. N'assignez pas de pinceau en code : il ne suivrait plus le thème.

  ```xml
  <Style Selector="TextBlock.EndLabel"><Setter Property="Foreground" Value="{DynamicResource SukiLowText}" /></Style>
  <Style Selector="TextBlock.EndLabel.Active"><Setter Property="Foreground" Value="{DynamicResource SukiText}" /></Style>
  ```
- Les labels des extrémités d'une course se placent **au-dessus des extrémités** (centrés sur le bout du rail), pas sur les côtés où ils élargissent inutilement la scène.
- Valeurs numériques : police à chasse fixe (`Consolas`), SemiBold.
- Gravures et plaques : voir §5.9.

### 6.5 Organisation du code : la boîte noire

C'est la raison d'être du style (voir l'introduction) : le livrable n'est pas un dessin, c'est **un composant complexe derrière une API simple**. Un contrôle skeuo produit beaucoup de code de dessin et d'animation (dégradés, reflets, particules, timers, lueurs recalculées au changement de thème). Ce code n'aide pas à comprendre la fonctionnalité. On l'isole donc, et seule l'interface avec la logique doit rester lisible.

**Un contrôle = une seule responsabilité.** On ne fabrique pas un « TankCard » qui contient à la fois la cuve et le levier. Un appareil qui affiche un état (cuve, moteur, caméra) et un actionneur qui capte un geste (levier, vanne, bouton d'arrêt) sont **deux contrôles distincts**. Chacun a une API universelle qui ne décrit que sa propre responsabilité. C'est la page, avec son ViewModel, qui les relie :

| Contrôle | Rôle | API |
|---|---|---|
| `Tank` | **Afficheur** : montre ce que l'appareil rapporte | `Level` (0..1), `Inflow` (0..1), `Outflow` (0..1) |
| `SpringLever` | **Actionneur** : rapporte l'intention de l'utilisateur | `Value` (-1..1, TwoWay), `LeftLabel`, `RightLabel`, événement `Released` |
| Page + ViewModel | **Logique** : traduit l'intention en commandes, simule ou pilote l'appareil | `LeverValue` → courbe → `Inflow` / `Outflow`, `CloseValves()` au relâchement |

Le test d'universalité : le levier ne sait pas qu'il pilote une cuve. Il ne parle que de position et de relâchement, et pourrait aussi bien piloter un moteur en jog. La cuve ne sait pas qu'un levier existe. Elle ne parle que de niveau et de débits, et pourrait être remplie par une pompe automatique. Si un nom de l'API trahit l'autre contrôle (`FlowRequested` sur une cuve, `Target` sur un levier), le découpage est faux.

Les débits affichés par la cuve sont ceux **rapportés par l'appareil**, et non ceux demandés par le levier. Quand l'appareil coupe la vanne parce que la cuve est vide, l'écoulement s'arrête à l'écran, même si l'utilisateur pousse encore le levier.

Chaque contrôle a son propre sous-dossier et trois fichiers :

- `X.axaml` : le dessin fixe et les thèmes Dark/Light.
- `X.axaml.cs` : uniquement l'API publique, courte et documentée (propriétés bindables, événements d'intention si besoin). C'est le contrat.
- `X.Rendering.cs` : classe partielle du même contrôle, avec toute la mécanique visuelle (tweens, timers, particules, glows).

Arborescence type :

```
Skeumorphism/
├── Controls/
│   ├── Tank/          Tank.axaml · Tank.axaml.cs · Tank.Rendering.cs
│   ├── SpringLever/   SpringLever.axaml · SpringLever.axaml.cs · SpringLever.Rendering.cs
│   ├── Wire/          la liaison entre un actionneur et son appareil (§6.6)
│   └── Shared/        ce que plusieurs contrôles réutilisent : matières (tokens), mouvement (tweens, courbes), saisies communes
├── SkeumorphismPage.axaml(.cs)   compose la scène, relaie Released
└── SkeumorphismViewModel.cs      toute la logique
```

Ce qui est commun à plusieurs contrôles (jeu de matières, primitives de mouvement, logique de saisie d'un geste) vit dans `Shared/` et non dans un contrôle : chaque contrôle reste ainsi lisible en trois fichiers, et un défaut corrigé une fois l'est partout.

Le contrôle ne connaît aucun objet métier : il reçoit des valeurs et les met en scène, et c'est lui qui lisse son animation à partir des variations de ces valeurs (par exemple, le flux apparaît en fondu quand `Outflow` passe de 0 à 0,6). Le ViewModel porte toute la logique et ignore qu'une animation existe.

Seule limite à la boîte noire : **elle décide comment ça se voit, jamais quoi envoyer au matériel ni quand.** Arrêt au relâchement, courbe de réponse, anti-spam ou refus d'une commande impossible restent dans le ViewModel. Le contrôle se contente d'émettre l'intention de l'utilisateur, par exemple une position normalisée ou `Released`. La zone morte fait exception : elle appartient au geste, donc au levier, qui émet déjà 0 au centre.

La revue de code porte sur le ViewModel, le branchement de la page et les contrats. Le rendu se valide à l'œil.

Test simple : **un programmeur doit pouvoir utiliser chaque contrôle en lisant uniquement son `X.axaml.cs`**, par exemple :

```xml
<ss:SpringLever Value="{Binding Speed}" Released="OnLeverReleased" />
<ss:Tank Level="{Binding Level}" Inflow="{Binding Inflow}" Outflow="{Binding Outflow}" />
```

### 6.6 Relier les pièces

**Pourquoi.** Séparer l'actionneur de l'appareil (§6.5) est une décision de code ; à l'écran, ils doivent se lire comme **un seul système**. Sans lien visible, l'utilisateur doit deviner qui commande quoi. Pour chaque actionneur de la page, on se pose donc deux questions : *quel est son appareil, et comment lui parle-t-il ?* Un actionneur sans lien est incomplet.

**La nature du lien dit la nature de l'information.** De la puissance ou une commande passent par un câble ; de la matière, par un tuyau ; une simple mesure par un lien plus discret que les deux. On choisit la représentation que ferait l'objet réel, parce que c'est elle qui rend le rôle de la pièce lisible : un tuyau pour dire « ceci mesure cela » fait croire à un passage de matière.

**Le lien est un contrôle à part.** Il est purement afficheur (même découpage que le reste) : une géométrie exprimée dans le repère du panneau hôte, une matière, et deux entrées d'information **distinctes** (§8.3) : l'état continu de ce qui circule et l'événement qui vient de se produire. Il ne sait rien des pièces qu'il relie.

**Les points d'ancrage sont documentés.** La page trace le lien, donc chaque contrôle indique, dans le commentaire de son repère, où s'attache la liaison (comme il indique ses dimensions). Un lien démarre et finit **sous une pièce** (cosse, presse-étoupe, collerette), pas dans le vide.

**Le lien passe derrière les pièces qu'il rejoint.** Il sort de dessous la plaque d'un actionneur au lieu de la traverser ; sinon il paraît collé dessus (même raison que pour un tuyau sous un socle, §6.1).

**Un lien qui vient de l'extérieur de la scène** obéit à la Loi 1 : il se fond dans le verre au lieu de s'arrêter au bord.

**L'information voyage de la source vers la destination.** Un lien de pure mesure n'a pas de sens de circulation et ne devrait pas en afficher.

---

## 7. Choisir le bon contrôle

| Intention de l'utilisateur | Contrôle Skeumo-Suki | Contrôle Suki |
|---|---|---|
| Déplacer quelque chose sur une course | Glisser directement la pièce (piston, chariot) | Champ position + unité |
| Piloter une vitesse ou un débit en continu (jog), dans un ou deux sens | **Levier à ressort** avec zone morte (gauche/droite = deux actions opposées : remplir/vider, reculer/avancer) | - |
| Choisir parmi 2 ou 3 positions physiques | **Interrupteur à crans** ou vanne rotative (clic = bascule) | `ToggleSwitch` / segmenté |
| Régler un paramètre avec effet visible | Curseur charnière (rail Suki + pouce perle) **et** objet qui réagit | Champ exact + unité fixe |
| Lancer une action | - | Bouton `Flat` pleine largeur |
| Arrêter immédiatement | **Bouton d'arrêt** (dôme rouge) | - |
| Nombre exact (répétitions…) | - | `NumericUpDown` |
| Activer une option | - | `ToggleSwitch` + titre + description |

### 7.1 Le champ exact avec unité fixe

C'est le partenaire naturel d'un curseur de **réglage** (un paramètre qui garde sa valeur, comme une durée ou un gain). Il n'accompagne pas un actionneur de jog, qui n'a pas de valeur à retenir (Loi 10). La valeur est à droite, l'unité dans un segment séparé, **sans boutons +/-** :

```xml
<controls:GlassCard CornerRadius="8" Padding="0">
    <Grid ColumnDefinitions="*,32">
        <TextBox Margin="16,7" Name="ValueBox" Classes="xxInput" />
        <Border Grid.Column="1" Background="#1A808080"
                BorderBrush="{DynamicResource SukiBorderBrush}" BorderThickness="1,0,0,0">
            <TextBlock FontSize="11" FontWeight="Medium" Foreground="{DynamicResource SukiLowText}"
                       HorizontalAlignment="Center" VerticalAlignment="Center" Text="ms" />
        </Border>
    </Grid>
</controls:GlassCard>
```

Le `TextBox` est restylé : fond transparent, sans bordure, `Consolas` 14 SemiBold, aligné à droite. Son template se réduit à un `TextPresenter` dans un `Border` transparent.

### 7.2 Le curseur charnière

```xml
<Grid Name="Track" Height="28" Background="Transparent" Focusable="True" Cursor="Hand">
    <Canvas Name="Ticks" IsHitTestVisible="False" />                       <!-- crans générés en code -->
    <Border Name="Rail" Height="7" VerticalAlignment="Center" CornerRadius="4"
            Background="{DynamicResource XxRail}" IsHitTestVisible="False" />  <!-- rail creusé : ombre inset en code -->
    <Border Name="Fill" Height="7" HorizontalAlignment="Left" VerticalAlignment="Center" CornerRadius="4"
            IsHitTestVisible="False">                                        <!-- remplissage accent + glow en code -->
        <Border.Background>
            <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
                <GradientStop Offset="0" Color="{DynamicResource SukiPrimaryColor75}" />
                <GradientStop Offset="1" Color="{DynamicResource SukiPrimaryColor}" />
            </LinearGradientBrush>
        </Border.Background>
    </Border>
    <!-- pouce perle (§5.4), positionné par Margin.Left = x - 11 -->
</Grid>
<Grid ColumnDefinitions="*,Auto">
    <TextBlock FontSize="11" Foreground="{DynamicResource SukiLowText}" Text="Plus sombre · mouvement net" />
    <TextBlock Grid.Column="1" FontSize="11" Foreground="{DynamicResource SukiLowText}" Text="Plus lumineux" />
</Grid>
```

Les **légendes aux deux extrémités** décrivent la conséquence (« plus lumineux », « plus de bruit »), pas l'unité. Pour un paramètre logarithmique, positionnez le pouce sur une échelle de crans (tiers ou demi-pas), pas sur une échelle linéaire.

### 7.3 Clavier et accessibilité

- Les rails et les leviers sont `Focusable="True"` : flèches pour un cran, Page Up/Down pour un grand pas.
- Chaque `TextBox` reçoit `AutomationProperties.Name` avec l'unité (« Durée (ms) »).
- La molette de la souris sur un interrupteur à crans change de cran.
- Toute action skeuo a un équivalent exact dans les commandes Suki ou au clavier.

### 7.4 Choisir la forme d'un actionneur

- **Discret ou continu.** Un nombre fini d'états nommés demande un organe à **crans** : l'état se lit sans interpréter. Un organe à course ou rotatif continu est pour une grandeur qui varie ; l'imposer à quelques états forcerait l'utilisateur à deviner où sont les positions.
- **Proportion.** L'actionneur est subordonné à l'appareil : s'il l'écrase, la carte se lit comme « un gros bouton avec un petit appareil ». Mais il doit rester attrapable : le compromis se tranche par la **zone de saisie**, qui peut être plus large que la pièce visible, et par une taille de poignée réellement confortable, pas en rétrécissant jusqu'à l'inconfort.
- **Emplacement.** On le place près de l'endroit où l'effet se produit, au niveau du raccordement, car la position raconte le lien (§6.6).
- **Repères.** Un organe à course montre où il commence et où il finit (butées, graduation, étiquette) : on doit savoir ce que signifie la position de repos sans l'essayer.

---

## 8. Comportement et physique

Le Skeumo-Suki se juge **en mouvement**. Une capture d'écran ne montre que la moitié du style.

### 8.1 Primitive : tween à minuteur

Simple, fiable, et ça fonctionne sur n'importe quelle propriété, y compris dans un `RenderTransform` :

```csharp
private static void Tween(double from, double to, int ms, Action<double> set, Func<double, double>? ease = null)
{
    ease ??= t => 1 - Math.Pow(1 - t, 3);                       // ease-out cubique par défaut
    var start = DateTime.UtcNow;
    DispatcherTimer? timer = null;
    timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) =>
    {
        var t = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / ms);
        set(from + (to - from) * ease(t));
        if (t >= 1) timer!.Stop();
    });
    timer.Start();
}
```

### 8.2 Bibliothèque de courbes

| Courbe | Formule | Usage | Durée |
|---|---|---|---|
| Ease-out cubique | `1 - (1-t)³` | Crans, bascules, rotations de vanne | 200-300 ms |
| Ressort amorti | `2^(-10t) · sin((10t - 0.75) · 2π/3) + 1` | Retour au centre d'un levier lâché | 600-800 ms |
| Pression | keyframes 0 → 25 % enfoncé → 100 % relâché, `CubicEaseOut` | Bouton physique | 400-500 ms |
| Flash | opacité 0 → 1 (15 %) → 0 | Confirmation d'arrêt, alerte | 600-800 ms |

### 8.3 Catalogue de comportements

**Levier à ressort (jog)** : le geste le plus riche du style.
- *Deux sens* : si l'appareil a deux actions opposées (remplir/vider, avancer/reculer), le levier va des deux côtés (±35°), avec un label au-dessus de chaque extrémité. Un levier qui ne va que d'un côté alors que l'objet suggère les deux paraît cassé.
- *Ancrage relatif et cumul* : le levier tourne **du même delta** que la main autour de l'axe, et ne saute jamais vers le pointeur. L'angle du pointeur est **périodique** : mesuré par rapport au clic, il saute dès que la main passe de l'autre côté de l'axe (au-delà de 180° du départ). On cumule donc les variations **par petits pas** (chaque variation ramenée entre -180° et 180°), on borne à chaque pas, et on ignore le pointeur quand il est trop près de l'axe, où l'angle est instable. Cette règle vaut pour tout organe rotatif (levier, bouton, vanne) : on la factorise dans `Shared/` pour qu'elle soit identique partout.
- *Zone morte* au centre (quelques degrés) : le levier émet 0.
- *Réponse quadratique* (dans le ViewModel) : `v = vMax · n · |n|`, avec `n` l'angle normalisé hors zone morte. On a du réglage fin au centre et la pleine vitesse aux bornes.
- *Anti-spam* (dans le ViewModel) : n'envoyez la commande que si la vitesse change d'au moins quelques %.
- *Relâchement* : `Value` repasse à 0 **avant** l'animation, puis `Released` est levé, puis le levier revient au centre par ressort amorti. Le tween de retour est purement visuel.
- *Reprise pendant le retour* : chaque tween porte un identifiant. Reprendre le levier incrémente l'identifiant, ce qui neutralise le tween en cours. Un simple test `if (!_engaged)` ne suffit pas : après une reprise suivie d'un second relâchement, deux tweens se disputeraient l'angle.
- *Sécurité* : traitez `PointerCaptureLost` (alt-tab, popup) **exactement** comme un relâchement, ainsi que le détachement du contrôle de l'arbre visuel.
- *Feedback* : lueur d'accent sous le pommeau, d'intensité proportionnelle à la poussée, arc de rail rempli du centre jusqu'à l'angle courant (dans le bon sens de balayage), et label du côté poussé en `SukiText`.
- *Clavier* : flèches gauche/bas et droite/haut pour pousser d'un cran, relâchement de la touche = relâchement du levier.

```csharp
private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
{
    var c = (Canvas)s!;
    c.Focus();
    Engage();
    _lastPointerDeg = PointerAngle(e.GetPosition(c));        // ancrage relatif : on cumule des pas
    e.Pointer.Capture(c);
    e.Handled = true;
}

private void Scene_OnMoved(object? s, PointerEventArgs e)
{
    if (!_engaged) return;
    var pointer = PointerAngle(e.GetPosition((Canvas)s!));
    var d = pointer - _lastPointerDeg;
    if (d > 180) d -= 360; else if (d < -180) d += 360;      // un pas, jamais le total depuis le clic
    _lastPointerDeg = pointer;
    SetAngle(Math.Clamp(_angle + d, -MaxDeg, MaxDeg));
}

private void Engage()
{
    _engaged = true;
    _tweenId++;                                              // neutralise un retour en cours
}

private void Release()
{
    if (!_engaged) return;
    _engaged = false;
    SetCurrentValue(ValueProperty, 0d);                      // arrêt d'abord, sans casser le binding
    Released?.Invoke(this, EventArgs.Empty);
    var id = ++_tweenId;
    Tween(_angle, 0, 700, a => { if (id == _tweenId) SetAngle(a, drive: false); }, ElasticOut);
}

private void SetAngle(double deg, bool drive = true)
{
    _angle = deg;
    _rot.Angle = deg;
    var abs = Math.Abs(deg);
    var n = abs <= DeadDeg ? 0 : Math.Sign(deg) * (abs - DeadDeg) / (MaxDeg - DeadDeg);
    // ... lueur, arc, labels ...
    if (drive) SetCurrentValue(ValueProperty, n);
}

private static double PointerAngle(Point p) => Math.Atan2(p.X - PivotX, PivotY - p.Y) * 180 / Math.PI;
private static double ElasticOut(double t) => Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * (2 * Math.PI / 3)) + 1;
```

**Interrupteur à crans** : 2 ou 3 positions, glissement ease-out cubique de 250 ms, le remplissage du rail suit le bouton pendant le mouvement, le label du cran actif passe en `SukiText`, la molette change de cran.

**Pièce glissée directement** (piston, chariot) : la pièce suit le pointeur **pendant** le glissé (aperçu), la commande matérielle part **au relâchement**. Si l'appareil est occupé, la pièce revient à la dernière position confirmée.

**Vanne rotative** : un clic sur la zone de la vanne bascule. Le rotor tourne (0° ↔ -90°) et son perçage d'accent s'oriente vers la voie active. Le canal actif passe à 0,7 d'opacité, l'autre à 0,1, et les labels changent de couleur.

**Bouton physique** : seul le dôme s'enfonce (marge haute +6 px), la collerette reste fixe. Il peut déclencher un flash sur la pièce concernée.

**Écoulement** : quand le liquide circule, le tuyau se remplit d'accent (opacité proportionnelle au débit), et des **particules en suspension** sont emportées par le flux. Pas de pointillés qui défilent : une file régulière se lit comme une animation, pas comme un liquide. Pour que le flux reste imprévisible, chaque particule a sa propre vie :

- *naissance aléatoire* : à chaque image, on tire au sort le nombre de particules à créer (en moyenne `0,9 × débit`), et non une particule à intervalle fixe ;
- *vitesse propre* : facteur tiré entre 0,55 et 1,45, multiplié par le débit courant ;
- *position propre dans la largeur du tuyau* : décalage latéral initial aléatoire (±2,4 px), plus une **dérive brownienne** à chaque image, bornée à la largeur du tube ;
- *ondulation propre* : `sin(s / λ + phase)`, avec une longueur d'onde `λ` tirée au hasard ;
- *mélange* : environ 80 % de points clairs et 20 % de points sombres, tailles de 1,2 à 2,8 px, alphas variés.

Les particules suivent la **ligne médiane du tuyau, coude compris** : on aplatit le `Path` (lignes + courbe `Q` échantillonnée) en une polyligne parcourue par distance, qui donne pour chaque `s` une position et une normale. Le décalage latéral s'applique le long de cette normale.

```csharp
var (pos, normal) = route.At(p.S);
var lateral = p.Offset + 0.9 * Math.Sin(p.S / p.WobbleLength + p.Phase);
Canvas.SetLeft(p.E, pos.X + normal.X * lateral - p.E.Width / 2);
Canvas.SetTop(p.E, pos.Y + normal.Y * lateral - p.E.Height / 2);
```

Le flux apparaît et disparaît en **fondu**, jamais d'un coup : le débit affiché rejoint le débit reçu par lissage exponentiel (`shown += (target - shown) · 0.12` à chaque image). Quand le débit tombe à zéro, les particules encore dans le tuyau s'effacent sur place au lieu de disparaître ou de continuer à filer. Au remplissage, les particules qui atteignent le bout du tuyau d'entrée **tombent** dans la cuve (gravité) le long d'un filet d'accent, jusqu'à la surface.

**Matière vivante** : particules en suspension qui dérivent lentement (`x += vx + 0.05 · sin(tick·0.02 + phase)`), rebondissent sur les parois et restent **toujours sous la surface**. Une particule qui remonte trop haut est repoussée vers le bas, et son opacité s'efface sur les 8 px sous la surface, pour qu'elle ne « touche » jamais l'air. Quand le liquide circule, le brassage s'accélère (`× (1 + 3 · débit)`) et la matière dérive vers la sortie. Un mélange de points clairs (bulles) et de quelques points sombres (impuretés) donne du réalisme.

**Signal** : ce par quoi un lien (§6.6) montre qu'une information passe. Il en existe deux sortes, qui ne se signalent pas pareil :

- *Un événement* (l'utilisateur vient de changer un réglage) se signale **une seule fois, au moment où il se produit**, pour accuser réception : « ton geste est pris en compte ». **Pourquoi :** un signal qui se répète sans cause est un mensonge (il suggère une activité qui n'existe pas) et il distrait. On le lie donc à la variation de la consigne, jamais à une horloge.
- *Un état continu* (quelque chose circule réellement) se montre en continu, d'après l'état **rapporté par l'appareil** et non d'après le geste, et reste **en arrière-plan**.

**L'intensité est proportionnée au rôle.** L'accusé de réception doit se remarquer sans surprendre ; le signal continu ne doit jamais concurrencer l'objet. Dans la pratique, le premier réglage est presque toujours trop brillant, trop rapide, trop marqué : on part bas et on monte, plutôt que l'inverse.

**Le rendu évoque la nature de ce qui circule** (une charge électrique ne ressemble pas à un fluide ni à un gaz), pour que le signal *dise* quelque chose. Une forme abstraite sans référence (un point, un trait) ne signifie rien et se lit comme un défaut.

### 8.4 L'objet comme afficheur de conséquences

Pour chaque paramètre, posez-vous la question : *qu'est-ce qui change physiquement quand on le règle ?* Puis faites-le changer **sur l'objet**, d'après des formules continues :

| Paramètre | Conséquence visible |
|---|---|
| Durée d'exposition, puissance lumineuse | Largeur et opacité d'un faisceau, éclairement d'une chambre, lueur d'une lentille |
| Gain, amplification | Les éléments s'éclaircissent **et** se pixellisent (rendu en basse résolution agrandi sans lissage) |
| Vitesse, débit | Opacité du flux dans le tuyau, densité et vitesse des particules emportées, brassage dans la cuve |
| Niveau, volume | Hauteur du liquide, halo proportionnel |
| Cadence | Rythme des impulsions sur le câble |
| Direction | Sens de l'écoulement, voie allumée |
| Vitesse d'un organe tournant | Au-delà de ce que l'œil suit, il se **floute** : une rotation restée nette à grande vitesse se lit comme un objet figé ou saccadé |

Ajoutez un **conseil contextuel** (encadré ambre, icône ampoule, une phrase) qui n'apparaît que lorsque la combinaison de réglages est mauvaise. Exemple : « Augmentez l'exposition avant le gain pour garder une image propre. »

### 8.5 Honnêteté de l'état

- Si l'appareil ne renvoie pas sa position, **estimez** la progression (elle tend vers 97 % tant que l'étape suivante n'a pas commencé) au lieu d'afficher une fausse certitude.
- Pas de nouvelle commande pendant un mouvement : l'objet le refuse et revient à l'état confirmé.
- Une commande impossible est refusée par le ViewModel (vider une cuve vide, remplir une cuve pleine), et l'appareil coupe de lui-même aux limites. Comme l'afficheur montre les débits **rapportés**, l'écoulement s'arrête à l'écran même si l'utilisateur pousse encore le levier.
- Pendant une opération automatique, la commande manuelle passe en grisé (opacité 0,4) et le bouton d'action devient « Annuler ».
- Les événements « modifié par l'utilisateur » ne sont levés **que** par une action de l'utilisateur, jamais quand le code applique une valeur venue d'ailleurs.

---

## 9. Thèmes clair et sombre, couleur d'accent

### 9.1 ThemeDictionaries

```xml
<UserControl.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Dark">
                <!-- toutes les matières, version sombre -->
            </ResourceDictionary>
            <ResourceDictionary x:Key="Light">
                <!-- mêmes clés, version claire -->
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
        <!-- matières identiques dans les deux thèmes (reflets blancs, chrome fixe…) -->
        <!-- pinceaux construits sur l'accent -->
    </ResourceDictionary>
</UserControl.Resources>
```

Ces ressources s'utilisent avec `{DynamicResource …}`. Les pinceaux indépendants du thème peuvent rester en `{StaticResource …}`.

### 9.2 Transposer une matière du sombre vers le clair

- Décalez toute la rampe de **2 crans vers le clair** (le corps `#3B4558` devient `#CBD3E0`).
- Les ombres passent de `#000000` à `#283250` et perdent environ la moitié de leur alpha.
- Le fond du verre devient presque transparent, teinté `#6F7F9E`.
- Les liserés blancs du verre deviennent des liserés bleu-gris (`#607090`).
- Les inscriptions claires sur plaque sombre restent lisibles : en clair, la plaque devient `#5E6A84` et le texte reste blanc.
- Un rendu dessiné en code (`Render`) doit lui aussi s'inverser : fond clair, éléments sombres.

### 9.3 Réagir aux changements de thème et d'accent

```csharp
public MyCard()
{
    InitializeComponent();
    AttachedToVisualTree   += (_, _) => { ApplyAccentGlows(); _timer.Start(); };
    DetachedFromVisualTree += (_, _) => _timer.Stop();                     // pas d'animation hors écran
    ActualThemeVariantChanged += (_, _) => { ApplyAccentGlows(); RebuildTicks(); };
}

/// Ressource Suki du thème courant, qu'elle soit un pinceau ou une couleur.
private IBrush ThemeBrush(string key, IBrush fallback) =>
    this.TryFindResource(key, ActualThemeVariant, out var res)
        ? res switch { IBrush b => b, Color c => new SolidColorBrush(c), _ => fallback }
        : fallback;
```

Tout ce qui est **créé en code** (graduations, segments, bulles, glows) doit être recoloré à chaque changement de thème.

---

## 10. Pièges techniques Avalonia

| Piège | Solution |
|---|---|
| `BoxShadow` ne peut pas référencer une couleur dynamique | Construisez les `BoxShadows` d'accent en code (§5.11). Les ombres neutres peuvent être des ressources `BoxShadows` dans les ThemeDictionaries. |
| `SukiPrimaryColor` / `SukiText` sont des `Color` | Dans un `GradientStop`, `Color="{DynamicResource …}"`. En code, convertissez avec `ThemeBrush`. |
| Animer un `RenderTransform` avec `Animation` ne marche pas toujours selon la version | Tween manuel par `DispatcherTimer` (§8.1) qui écrit directement `RotateTransform.Angle` ou `TranslateTransform.Y`. |
| Une lueur est coupée net | Parents en `ClipToBounds="False"`, lueur placée hors du conteneur clippé. |
| Le `Canvas` ne reçoit pas les clics | `Background="Transparent"` (un fond `null` n'est pas testable). |
| Une décoration vole les clics | `IsHitTestVisible="False"` sur tout ce qui n'est pas interactif. |
| `StrokeDashArray` / `StrokeDashOffset` semblent faux | Ils sont exprimés en **multiples de l'épaisseur du trait**. |
| Les coordonnées de pointeur sont décalées sous un `Viewbox` | `e.GetPosition(canvasDeLaScene)`. |
| Le glissé se bloque après un alt-tab | Gérez `PointerCaptureLost` comme un relâchement. |
| Les commandes matérielles bloquent l'interface | `Task.Run` pour la commande, `Dispatcher.UIThread.Post` pour revenir à l'interface. |
| Des timers tournent sur un contrôle invisible | Start/Stop sur `AttachedToVisualTree` / `DetachedFromVisualTree`. |
| Un mélange `RenderTargetBitmap` + `DrawImage` est flou | Pour une pixellisation volontaire, rendez en basse résolution puis agrandissez sans lissage. |
| Un traducteur automatique traduit des gravures | Marquez-les (par exemple `Tag="NoTranslate"`) selon la convention de votre app. |
| `CS0102 : le type contient déjà une définition pour 'Flow'` | Le générateur de noms d'Avalonia crée un champ pour chaque `Name`. Un élément nommé comme une propriété du contrôle entre en collision avec elle : nommez les éléments par leur rôle visuel (`OutletFlow`, `FlowLine`), jamais comme une propriété de l'API. |
| `CS0104 : 'Path' est une référence ambiguë` | Avec les usings implicites, `System.IO.Path` est en conflit avec `Avalonia.Controls.Shapes.Path`. Ajoutez `using Path = Avalonia.Controls.Shapes.Path;`. |
| Le binding TwoWay d'une valeur émise par le contrôle se casse | Le contrôle écrit sa propre valeur avec `SetCurrentValue(ValueProperty, v)`, jamais avec `SetValue` ou le setter : sinon la valeur locale remplace le binding. Déclarez la propriété avec `defaultBindingMode: BindingMode.TwoWay`. |
| Un geste rotatif « part n'importe où » au-delà de 180° | L'angle du pointeur est périodique : cumulez des pas bornés au lieu de comparer au clic de départ (§8.3). |
| Une lueur ou un halo est tronqué par le repère du contrôle | Le repère contient tout ce qui rayonne : dimensionnez-le pour le halo (et non pour la seule pièce), et n'enfermez pas la lueur dans un parent qui rogne (`ClipToBounds`). |
| Un label recoloré en code ne suit plus le thème | Basculez une classe et laissez un style avec `{DynamicResource …}` porter la couleur (§6.4). |
| Un liquide « carré » dépasse des épaules arrondies du récipient | Découpez le contenu avec un `Border` intérieur `ClipToBounds="True"` et `CornerRadius` (§5.7), au lieu d'arrondir le liquide. |
| Le bout d'un tube dépasse d'une bride ou d'un collier | `StrokeLineCap="Round"` prolonge le trait de la moitié de son épaisseur : arrêtez le tracé plus tôt (§5.8). |
| Un tube coudé est foncé et uni en thème clair | Le corps du tube, posé sur le bord, est trop transparent : rendez-le opaque et clair, et ajoutez un cœur plus clair (§3.2). |

---

## 11. Méthode de travail

1. **Observer le vrai appareil.** Notez 3 à 6 détails caractéristiques et les pièces qui bougent.
2. **Lister les gestes et les conséquences.** Pour chaque réglage : quel geste, quelle valeur exacte, quel changement visible ?
3. **Maquetter en SVG interactif** avant d'écrire du XAML. Gardez le même repère (la maquette devient le `Canvas`). Itérer en SVG est dix fois plus rapide.
4. **Valider la silhouette** en aplats, sans dégradés : si on ne reconnaît pas l'objet, aucun dégradé ne le sauvera.
5. **Appliquer la rampe métal, puis la lumière, puis le verre, puis l'accent**, dans cet ordre.
6. **Itérer une seule valeur à la fois** (une couleur, un alpha, un rayon) et comparer. Ce style se règle au détail près.
7. **Découper en contrôles** : un afficheur, un actionneur, chacun avec son API universelle et son propre repère (§6.5). Écrivez les contrats `X.axaml.cs` avant le dessin.
8. **Porter en XAML** avec les tokens par rôle et les ThemeDictionaries.
9. **Ajouter le comportement** (§8), puis tester la physique à la main : ressort, crans, relâchement brutal, alt-tab.
9bis. **Relier les pièces** (§6.6) : pour chaque actionneur, son appareil et le lien qui les unit.
10. **Regarder tourner l'application réelle**, pas seulement compiler : les défauts qui comptent ne se voient pas dans le code (coupures, longueur d'un fondu, proportions, brillance d'un signal, halos tronqués). Une revue d'écran dans les deux thèmes avant de présenter évite des allers-retours entiers.
11. **Tester les trois variations** : thème sombre, thème clair, et **une autre couleur d'accent**. Si le changement d'accent ne recolore pas l'énergie de l'objet, une couleur est codée en dur.

---

## 12. Anti-patterns

- ❌ Textures bitmap, bruit, grain, photo du vrai appareil.
- ❌ Gris neutres (`#808080`), métaux chauds, bois, cuir.
- ❌ Plusieurs sources de lumière, ou une lumière qui vient d'en bas.
- ❌ Ombres portées longues et dures, noir pur dans le thème clair.
- ❌ Un sol, un plateau ou un décor sous l'objet (les pieds et platines de l'appareil lui-même sont permis).
- ❌ Une pièce mobile qui sort d'un simple rond posé dans le vide : un levier repose sur une platine (§5.13).
- ❌ Un tube ou un câble coupé net au bord de la carte, un fondu trop court, un embout qui dépasse d'une bride.
- ❌ Des pointillés qui défilent pour simuler un écoulement : une file prévisible n'est pas un liquide.
- ❌ L'énergie générique (liquide quelconque, remplissage, rail) dans une couleur fixe au lieu de l'accent.
- ❌ L'accent imposé à ce qui a une couleur conventionnelle forte (lumière, niveau de charge, gaz) : on perd la reconnaissance (§3.5).
- ❌ Une métaphore empruntée à un autre objet (un accumulateur qui se remplit comme un réservoir, un gaz dessiné comme un liquide).
- ❌ Un indicateur surchargé de matérialité (capuchons, ombrages) au lieu d'un affichage sobre.
- ❌ Un actionneur sans lien visible avec son appareil, ou un lien qui n'a pas la nature de ce qu'il transporte.
- ❌ Un signal qui se répète sans cause, ou un signal continu qui concurrence l'objet.
- ❌ Un geste rotatif mesuré depuis le clic de départ (il saute au-delà de 180°).
- ❌ Un halo ou une lueur tronqué par le repère du contrôle.
- ❌ Un bouton rotatif pour saisir une valeur exacte. Des boutons +/- à côté d'un champ.
- ❌ Un champ exact ajouté par principe à un actionneur qui n'a pas de valeur à retenir.
- ❌ Un objet qui ne réagit pas aux réglages : c'est de la décoration, pas du Skeumo-Suki.
- ❌ Une silhouette qui évoque un autre objet (une cuve qui ressemble à une seringue).
- ❌ Un levier qui saute vers le pointeur au lieu de tourner avec la main, ou qui ne va que d'un côté quand l'appareil a deux actions opposées.
- ❌ Un relâchement qui laisse l'appareil tourner pendant l'animation de retour.
- ❌ Un seul contrôle qui fusionne l'afficheur et l'actionneur, ou une API qui trahit l'autre contrôle.
- ❌ Deux appareils sans rapport dans la même carte, ou du skeuo sur la navigation et les formulaires.
- ❌ Des pièces collées les unes aux autres dans une scène composée.
- ❌ Une seule version de thème.
- ❌ Des détails sans signification (rivets décoratifs partout, vis sur chaque pied, cerclages qui masquent le liquide).

---

## 13. Checklist de validation

**Intégration Suki**
- [ ] La carte est une `GlassCard` standard (titre, padding, badges en verre).
- [ ] L'objet flotte, sans sol ni cadre. Les entrées et sorties se fondent dans le verre, par un fondu long.
- [ ] Les valeurs exactes et les actions utilisent des composants Suki, et seulement quand elles sont utiles.

**Organisation**
- [ ] Un contrôle par responsabilité : afficheur d'un côté, actionneur de l'autre, reliés par la page et son ViewModel.
- [ ] Chaque API est universelle : aucun nom ne trahit l'autre contrôle.
- [ ] Chaque contrôle tient en trois fichiers : `.axaml`, `.axaml.cs` (contrat seul), `.Rendering.cs`.
- [ ] L'afficheur montre l'état rapporté par l'appareil, pas la demande de l'utilisateur.

**Forme**
- [ ] La silhouette se reconnaît en aplat (une cuve est large, avec épaules, bride, sortie et pieds).
- [ ] Les pièces mobiles reposent sur une base solide. Les pièces d'une scène composée ont de l'air entre elles.
- [ ] Les tuyaux passent derrière ou sous ce qu'ils croisent, et leurs embouts sont cachés.

**Liaisons**
- [ ] Chaque actionneur est relié à son appareil, et la nature du lien correspond à ce qu'il transporte.
- [ ] Le lien démarre et finit sous une pièce, et passe derrière les pièces qu'il rejoint.
- [ ] Événement et état continu sont distingués : l'événement est signalé une fois, le continu reste discret.
- [ ] Les fondus atteignent zéro au bord de la carte et ont été **vus** à l'écran.

**Matière et lumière**
- [ ] Toutes les matières dures utilisent la rampe bleu-ardoise.
- [ ] Une seule source de lumière en haut à gauche, un filet spéculaire par pièce.
- [ ] Les ombres sont courtes et douces, teintées `#283250` en clair.
- [ ] Aucun bitmap. Grands rayons d'angle. 3 à 6 détails, tous significatifs.

**Énergie**
- [ ] L'énergie sans couleur conventionnelle (liquide quelconque, remplissage, perçage actif) est en `SukiPrimaryColor` / `75` ; ce qui a une couleur conventionnelle suit le réel (§3.5).
- [ ] Changer la couleur d'accent recolore l'énergie de l'objet.
- [ ] Le rouge est réservé à l'arrêt et vient de `SukiDangerColor`.

**Thèmes**
- [ ] Chaque matière existe en `Dark` et en `Light`, et la version claire a été regardée pour de vrai (tubes notamment).
- [ ] Les éléments créés en code se recolorent au changement de thème.
- [ ] Les glows d'accent sont reconstruits au changement de thème.

**Comportement**
- [ ] Chaque réglage a une conséquence visible sur l'objet.
- [ ] Les mouvements sont amortis (ease-out, ressort, crans), jamais instantanés. Les flux apparaissent et disparaissent en fondu.
- [ ] Les particules (en suspension, emportées par un flux) n'ont ni rythme ni trajectoire prévisibles.
- [ ] Le levier va dans les deux sens si l'appareil a deux actions opposées.
- [ ] Un geste rotatif reste juste au-delà de 180° depuis le départ.
- [ ] Une vitesse qui dépasse ce que l'œil suit est rendue par du flou.
- [ ] Rien n'est tronqué : halos, lueurs et reflets tiennent dans le repère du contrôle.
- [ ] Relâchement et perte de capture = arrêt matériel immédiat.
- [ ] Pas de commande pendant un mouvement. L'état affiché est honnête.
- [ ] Les timers s'arrêtent quand le contrôle quitte l'écran.

**Accessibilité**
- [ ] Les contrôles gestuels sont focusables et utilisables au clavier.
- [ ] Les champs ont un nom d'automatisation avec l'unité.
- [ ] L'état se lit aussi par le texte (`SukiText` / `SukiLowText`), pas seulement par la couleur.

---

## 14. Exemple complet : cuve et levier de remplissage / vidange

Une carte qui montre une cuve en verre (le niveau est l'accent) et un levier à ressort : poussé à gauche, il remplit par l'entrée du haut ; poussé à droite, il vide par la sortie du bas. Plus on pousse, plus le débit est fort. Au relâchement, les vannes se ferment immédiatement et le levier revient au centre.

L'exemple applique le découpage du §6.5 : **deux contrôles indépendants** et une page qui les relie. Les sources complètes sont dans la démo : `SukiUI.Demo/Features/Helpers/Skeumorphism/` (menu Helpers → Skeumorphism). Ci-dessous figurent les contrats, la page et le ViewModel en entier, ainsi que la structure commentée du rendu.

```
Features/Helpers/Skeumorphism/
├── Controls/
│   ├── Tank/          Tank.axaml · Tank.axaml.cs · Tank.Rendering.cs
│   └── SpringLever/   SpringLever.axaml · SpringLever.axaml.cs · SpringLever.Rendering.cs
├── SkeumorphismPage.axaml(.cs)
└── SkeumorphismViewModel.cs
```

### 14.1 Contrat de la cuve : `Tank.axaml.cs`

Afficheur pur. La cuve ne sait pas qui la remplit.

```csharp
/// <summary>
/// Skeumo-Suki glass tank with an inlet on top and an outlet at the bottom.
/// Display only: it shows the level and the flows it receives. Drawing and animation live in <c>Tank.Rendering.cs</c>.
/// </summary>
public partial class Tank : UserControl
{
    /// <summary>Liquid level (0 = empty .. 1 = full).</summary>
    public static readonly StyledProperty<double> LevelProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Level), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>Flow entering through the top inlet (0 = closed .. 1 = full flow).</summary>
    public static readonly StyledProperty<double> InflowProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Inflow), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>Flow leaving through the bottom outlet (0 = closed .. 1 = full flow).</summary>
    public static readonly StyledProperty<double> OutflowProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Outflow), coerce: (_, v) => Math.Clamp(v, 0, 1));

    public double Level   { get => GetValue(LevelProperty);   set => SetValue(LevelProperty, value); }
    public double Inflow  { get => GetValue(InflowProperty);  set => SetValue(InflowProperty, value); }
    public double Outflow { get => GetValue(OutflowProperty); set => SetValue(OutflowProperty, value); }

    public Tank()
    {
        InitializeComponent();
        InitializeRendering();
    }
}
```

### 14.2 Contrat du levier : `SpringLever.axaml.cs`

Actionneur pur. Le levier ne sait pas ce qu'il pilote.

```csharp
/// <summary>
/// Skeumo-Suki spring-loaded lever (jog). The user pushes it left or right; released, it springs back to the center.
/// It only reports the user's intent: what to do with it (response curve, anti-spam, safety) belongs to the host.
/// Drawing and animation live in <c>SpringLever.Rendering.cs</c>.
/// </summary>
public partial class SpringLever : UserControl
{
    /// <summary>
    /// Lever position set by the user: -1 (fully left) .. 1 (fully right), 0 inside the center dead zone.
    /// Set back to 0 immediately on release, before the spring-back animation.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SpringLever, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Label shown above the left end of the travel.</summary>
    public static readonly StyledProperty<string?> LeftLabelProperty =
        AvaloniaProperty.Register<SpringLever, string?>(nameof(LeftLabel));

    /// <summary>Label shown above the right end of the travel.</summary>
    public static readonly StyledProperty<string?> RightLabelProperty =
        AvaloniaProperty.Register<SpringLever, string?>(nameof(RightLabel));

    public double Value       { get => GetValue(ValueProperty);      set => SetValue(ValueProperty, value); }
    public string? LeftLabel  { get => GetValue(LeftLabelProperty);  set => SetValue(LeftLabelProperty, value); }
    public string? RightLabel { get => GetValue(RightLabelProperty); set => SetValue(RightLabelProperty, value); }

    /// <summary>Raised when the user lets go of the lever (pointer released, capture lost or key released).</summary>
    public event EventHandler? Released;

    public SpringLever()
    {
        InitializeComponent();
        InitializeRendering();
    }
}
```

### 14.3 La page qui relie les deux : `SkeumorphismPage.axaml`

La carte compose la scène. Le levier est superposé à droite de la cuve, avec de l'air entre les deux, et la sortie de la cuve passe sous sa platine.

```xml
<suki:GlassCard Width="320" Padding="16" TextElement.Foreground="{DynamicResource SukiText}">
    <StackPanel>
        <!-- En-tête : titre + badge de niveau -->
        <Grid ColumnDefinitions="*,Auto">
            <TextBlock VerticalAlignment="Center" FontSize="17" FontWeight="DemiBold" Text="Tank" />
            <suki:GlassCard Grid.Column="1" Padding="10,3" CornerRadius="10">
                <StackPanel Orientation="Horizontal" Spacing="6">
                    <Border Width="7" Height="7" VerticalAlignment="Center" CornerRadius="4"
                            Background="{DynamicResource SukiPrimaryColor}" />
                    <TextBlock FontSize="11" FontWeight="DemiBold" Text="{Binding LevelText}" />
                </StackPanel>
            </suki:GlassCard>
        </Grid>

        <!-- Scène : la sortie de la cuve passe sous le levier -->
        <Panel Width="288" Height="200" Margin="0,16,0,0">
            <controls:Tank Level="{Binding Level}"
                           Inflow="{Binding Inflow}"
                           Outflow="{Binding Outflow}" />
            <controls:SpringLever Margin="0,46,0,0"
                                  HorizontalAlignment="Right"
                                  VerticalAlignment="Top"
                                  LeftLabel="Fill"
                                  RightLabel="Drain"
                                  AutomationProperties.Name="Fill and drain lever"
                                  Value="{Binding LeverValue}"
                                  Released="Lever_OnReleased" />
        </Panel>
    </StackPanel>
</suki:GlassCard>
```

```csharp
public partial class SkeumorphismPage : UserControl
{
    public SkeumorphismPage() => InitializeComponent();

    private void Lever_OnReleased(object? sender, EventArgs e) =>
        (DataContext as SkeumorphismViewModel)?.CloseValves();
}
```

Il n'y a pas de champ exact : un levier de jog n'a pas de valeur à retenir (Loi 10).

### 14.4 Toute la logique : `SkeumorphismViewModel.cs`

```csharp
/// <summary>
/// Simulated tank with a fill valve and a drain valve, driven by a spring lever.
/// All the logic lives here (response curve, anti-spam, stop on release, automatic stop when empty or full):
/// the Tank only displays values and the SpringLever only reports the user's intent.
/// </summary>
public partial class SkeumorphismViewModel : ObservableObject
{
    private const double MaxRatePerSecond = 0.12;    // fraction of the tank per second at full opening
    private const double MinFlowChange = 0.03;       // anti-spam threshold on valve commands

    private readonly DispatcherTimer _deviceTimer;
    private DateTime _lastTick = DateTime.UtcNow;

    /// <summary>Lever intent: -1 = fill .. 1 = drain.</summary>
    [ObservableProperty] private double _leverValue;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(LevelText))] private double _level = 0.7;
    [ObservableProperty] private double _inflow;
    [ObservableProperty] private double _outflow;

    public string LevelText => $"{Level * 100:0} %";

    public SkeumorphismViewModel()
    {
        _deviceTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(30), DispatcherPriority.Background, (_, _) => SimulateDevice());
        _deviceTimer.Start();
    }

    /// <summary>Safety first: the valves close as soon as the user lets go of the lever.</summary>
    public void CloseValves()
    {
        Inflow = 0;
        Outflow = 0;
    }

    partial void OnLeverValueChanged(double value)
    {
        // Refuse commands that cannot be honoured: nothing to drain, or no room left.
        if (value == 0 || (value > 0 && Level <= 0) || (value < 0 && Level >= 1))
        {
            CloseValves();
            return;
        }

        var flow = value * Math.Abs(value);           // quadratic response: fine control near the dead zone
        var current = Outflow - Inflow;
        if (Math.Abs(flow - current) < MinFlowChange && Math.Abs(flow) < 1) return;

        Outflow = Math.Max(0, flow);
        Inflow = Math.Max(0, -flow);
    }

    private void SimulateDevice()
    {
        var now = DateTime.UtcNow;
        var dt = (now - _lastTick).TotalSeconds;
        _lastTick = now;

        if (Inflow == 0 && Outflow == 0) return;

        Level = Math.Clamp(Level + MaxRatePerSecond * (Inflow - Outflow) * dt, 0, 1);

        // The device closes its valves by itself at the limits.
        if (Level <= 0) Outflow = 0;
        if (Level >= 1) Inflow = 0;
    }
}
```

Avec un vrai appareil, `SimulateDevice` disparaît : `Level`, `Inflow` et `Outflow` sont alimentés par l'appareil, et `OnLeverValueChanged` / `CloseValves` envoient les commandes (`Task.Run` pour la commande, `Dispatcher.UIThread.Post` pour revenir à l'interface).

### 14.5 Le dessin de la cuve : `Tank.axaml`

Repère 288 × 200. Les couches suivent le §6.3 :

| Couche | Contenu | Recette |
|---|---|---|
| 1. Lueur | `Aura`, `BoxShadow` d'accent posé en code, hauteur = niveau | §5.11 |
| 2. Pièces cachées | entrée (gauche → bride, fondu `0.02 → 0.24`), sortie (fond → coude → sous le pied droit à y = 192 → fondu vers la droite), jambes inclinées et patins | §5.8, §5.14 |
| 3. Fond du verre | corps 100 × 130, `CornerRadius="34,34,16,16"` : large, épaules rondes | §5.6 |
| 4. Contenu | `Border` intérieur `ClipToBounds`, avec le filet qui tombe, le liquide ancré en bas et les particules | §5.7 |
| 5. Parois | liserés extérieur et intérieur, reflet vertical, reflet de dôme, graduations 25/50/75 % | §5.6 |
| 6. Métal fixe | bride vissée en haut (sous laquelle se cache l'embout de l'entrée), raccord de sortie, pattes des jambes sans vis | §5.2, §5.14 |

Pas de cerclages sur le verre : ils cachaient le liquide. Les tokens sont préfixés `Tk` et déclarés dans les deux `ThemeDictionaries`. Les tubes utilisent `TkTubeRim`, `TkTubeBg` et `TkTubeCore` (§3.2).

### 14.6 La mécanique de la cuve : `Tank.Rendering.cs`

```csharp
private void Tick()
{
    _tick++;

    // The flows fade in and out, they never switch abruptly.
    _shownIn = Approach(_shownIn, Inflow);
    _shownOut = Approach(_shownOut, Outflow);

    this.Get<Path>("OutletFlow").Opacity = 0.85 * _shownOut;
    this.Get<Path>("InletFlow").Opacity = 0.85 * _shownIn;
    var fall = this.Get<Line>("FallStream");
    fall.Opacity = 0.8 * _shownIn;
    fall.StrokeThickness = 1.5 + 2.5 * _shownIn;
    fall.EndPoint = new Point(FallX, SurfaceY + 2);

    UpdatePipe(_outletSpecks, _outletCanvas, OutletRoute, _shownOut, onExit: null);
    UpdatePipe(_inletSpecks, _inletCanvas, InletRoute, _shownIn, onExit: SpawnFallSpeck);
    UpdateFall();     // particles falling from the inlet to the surface
    UpdateTank();     // suspended matter, always below the surface
}

// Particles carried by a pipe flow: random spawn, speed, lateral offset and wobble, so they never form a queue.
private void UpdatePipe(List<PipeSpeck> specks, Canvas canvas, Route route, double flow, Action? onExit)
{
    if (flow > 0.02)
    {
        var expected = 0.9 * flow;                           // Poisson-like spawning
        while (_rng.NextDouble() < expected)
        {
            specks.Add(CreatePipeSpeck(canvas));
            expected -= 1;
        }
    }

    for (var i = specks.Count - 1; i >= 0; i--)
    {
        var p = specks[i];
        p.S += p.Speed * (0.25 + 3.2 * flow);
        p.Offset = Math.Clamp(p.Offset + (_rng.NextDouble() - 0.5) * 0.35, -2.6, 2.6);   // brownian drift

        var exited = p.S >= route.Length;
        if (flow < 0.02) p.Fade -= 0.05;                     // flow stopped: fade out in place
        if (exited || p.Fade <= 0)
        {
            canvas.Children.Remove(p.E);
            specks.RemoveAt(i);
            if (exited) onExit?.Invoke();                    // inlet: the particle falls into the tank
            continue;
        }

        var (pos, normal) = route.At(p.S);
        var lateral = p.Offset + 0.9 * Math.Sin(p.S / p.WobbleLength + p.Phase);
        p.E.Opacity = p.Alpha * Math.Min(1, p.S / 8) * p.Fade;
        Canvas.SetLeft(p.E, pos.X + normal.X * lateral - p.E.Width / 2);
        Canvas.SetTop(p.E, pos.Y + normal.Y * lateral - p.E.Height / 2);
    }
}
```

`Route` aplatit la géométrie des `Path` du XAML (lignes et courbe `Q` échantillonnée en 12 segments) et donne, pour une distance `s`, une position et une normale. Elle est déclarée avec **exactement** les mêmes points que le XAML :

```csharp
private static readonly Route OutletRoute = new Route(new Point(96, 160))
    .LineTo(new Point(96, 180)).QuadTo(new Point(96, 192), new Point(108, 192)).LineTo(new Point(288, 192));
```

Les particules créées en code sont recolorées dans `ApplyTheme()` à chaque changement de thème, et le timer s'arrête quand la cuve quitte l'arbre visuel.

### 14.7 Le levier : `SpringLever.axaml` et `SpringLever.Rendering.cs`

Repère 110 × 104, pivot en (55,82), rayon 60, course ±35°, zone morte 4°. Du fond vers l'avant : rail d'arc et repère central, arc rempli d'accent, lueur sous le pommeau, levier (tige et pommeau, `RenderTransformOrigin="50%,100%"`), bossage avec axe chromé, puis platine vissée (§5.13) et labels d'extrémité au-dessus de chaque bout du rail (§6.4). La mécanique est celle du §8.3.

### 14.8 Ce que cet exemple démontre

| Loi / règle | Où |
|---|---|
| 1. Carte Suki, objet exposé | `GlassCard`, badge de niveau en verre, entrée et sortie fondues dans la carte, pieds et platine qui appartiennent aux appareils |
| 2. Métal teinté | rampe bleu-ardoise sur la bride, les jambes, les patins, le bossage et la platine |
| 3. L'énergie est l'accent | liquide, halo, flux dans les tuyaux, filet de remplissage, arc et lueur du levier |
| 4. Une seule lumière | origines radiales à 35 %/30 %, filets spéculaires, ombres `0 3 6` |
| 5. Le verre devient objet | cuve en couches, tubes de verre à trois épaisseurs |
| 6. Vectoriel et sobre | aucun bitmap, grands rayons, détails comptés (bride vissée, platine vissée, graduations) |
| 7. Ressembler au vrai matériel | silhouette de cuve : large, épaules, bride, sortie basse, pieds |
| 8. Deux thèmes | mêmes clés dans `Dark` et `Light`, tubes clairs opaques en thème clair |
| 9. Comportement | ancrage relatif, deux sens, zone morte, réponse quadratique, ressort, particules imprévisibles, fondus |
| 10. Précision Suki | aucun champ exact, car il n'y a pas de valeur à retenir |
| §6.5 Boîte noire | deux contrôles à une responsabilité chacun, API universelles, logique dans le ViewModel |
