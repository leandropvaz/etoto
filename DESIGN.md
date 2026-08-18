---
name: E-toto
description: Modern, light, rounded SaaS for LOTOTO lockout/tagout — white cards floating on blue-grey, lockout state as a solid colour pill.
colors:
  pw-paper: "#F5F7FA"
  pw-sheet: "#FFFFFF"
  pw-vellum: "#F0F3F8"
  pw-vellum-2: "#F8FAFC"
  pw-ink: "#1C2333"
  pw-ink-2: "#4E5875"
  pw-ink-3: "#5C6580"
  pw-ink-4: "#A3ABBF"
  pw-ink-deep: "#10162A"
  pw-hair: "#EDF0F5"
  pw-line: "#DFE4EE"
  pw-heavy: "#C6CDDC"
  pw-notice: "#1570DB"
  pw-notice-ink: "#0F55A8"
  pw-notice-bg: "#EAF2FE"
  pw-danger: "#D1344B"
  pw-danger-ink: "#A8253A"
  pw-danger-bg: "#FDECEF"
  pw-warn: "#9C5607"
  pw-warn-ink: "#7E4506"
  pw-warn-bg: "#FDF2E5"
  pw-caution: "#8A6A00"
  pw-caution-bg: "#FDF7E3"
  pw-safe: "#0F7B4F"
  pw-safe-ink: "#0B6440"
  pw-safe-bg: "#E8F6EF"
  pw-idle: "#626B82"
  pw-idle-bg: "#F0F2F7"
  brand-blue: "#006CB5"
typography:
  display:
    fontFamily: "Figtree, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "2rem"
    fontWeight: 800
    lineHeight: 1.1
    letterSpacing: "-0.02em"
  headline:
    fontFamily: "Figtree, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "1.5rem"
    fontWeight: 700
    lineHeight: 1.25
    letterSpacing: "-0.015em"
  title:
    fontFamily: "Figtree, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "1.125rem"
    fontWeight: 700
    lineHeight: 1.25
    letterSpacing: "-0.015em"
  body:
    fontFamily: "Figtree, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 400
    lineHeight: 1.5
    letterSpacing: "normal"
  field:
    fontFamily: "Figtree, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.5
    letterSpacing: "normal"
  label:
    fontFamily: "Figtree, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "0.8125rem"
    fontWeight: 600
    lineHeight: 1.4
    letterSpacing: "0"
  state:
    fontFamily: "Figtree, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 700
    lineHeight: 1.5
    letterSpacing: "0"
  meta:
    fontFamily: "Figtree, system-ui, -apple-system, Segoe UI, sans-serif"
    fontSize: "0.6875rem"
    fontWeight: 600
    lineHeight: 1.4
    letterSpacing: "normal"
rounded:
  xs: "6px"
  sm: "8px"
  md: "10px"
  lg: "14px"
  xl: "20px"
  pill: "999px"
spacing:
  "1": "4px"
  "2": "8px"
  "3": "12px"
  "4": "16px"
  "5": "24px"
  "6": "32px"
  "7": "48px"
  "8": "64px"
components:
  button-primary:
    backgroundColor: "{colors.pw-notice}"
    textColor: "#FFFFFF"
    typography: "{typography.body}"
    rounded: "{rounded.md}"
    padding: "0 16px"
    height: "40px"
  button-primary-hover:
    backgroundColor: "{colors.pw-notice-ink}"
    textColor: "#FFFFFF"
  button-secondary:
    backgroundColor: "{colors.pw-sheet}"
    textColor: "{colors.pw-ink-2}"
    typography: "{typography.body}"
    rounded: "{rounded.md}"
    padding: "0 16px"
    height: "40px"
  button-secondary-hover:
    backgroundColor: "{colors.pw-vellum}"
    textColor: "{colors.pw-ink}"
  button-danger:
    backgroundColor: "{colors.pw-danger}"
    textColor: "#FFFFFF"
    typography: "{typography.body}"
    rounded: "{rounded.md}"
    padding: "0 16px"
    height: "40px"
  button-row:
    backgroundColor: "transparent"
    textColor: "{colors.pw-ink-3}"
    rounded: "{rounded.sm}"
    padding: "0"
    height: "32px"
    width: "32px"
  button-row-hover:
    backgroundColor: "{colors.pw-notice-bg}"
    textColor: "{colors.pw-notice-ink}"
  status-pill:
    backgroundColor: "{colors.pw-danger}"
    textColor: "#FFFFFF"
    typography: "{typography.state}"
    rounded: "{rounded.pill}"
    padding: "4px 12px"
  status-pill-void:
    backgroundColor: "transparent"
    textColor: "{colors.pw-ink-3}"
    typography: "{typography.state}"
    rounded: "{rounded.pill}"
    padding: "4px 12px"
  card:
    backgroundColor: "{colors.pw-sheet}"
    textColor: "{colors.pw-ink}"
    rounded: "{rounded.lg}"
    padding: "24px"
  modal:
    backgroundColor: "{colors.pw-sheet}"
    textColor: "{colors.pw-ink}"
    rounded: "{rounded.xl}"
    padding: "24px"
  input:
    backgroundColor: "{colors.pw-sheet}"
    textColor: "{colors.pw-ink}"
    typography: "{typography.field}"
    rounded: "{rounded.sm}"
    padding: "0 12px"
    height: "40px"
  nav-item:
    backgroundColor: "transparent"
    textColor: "{colors.pw-ink-2}"
    typography: "{typography.body}"
    rounded: "{rounded.md}"
    padding: "0 12px"
    height: "38px"
  nav-item-active:
    backgroundColor: "{colors.pw-notice-bg}"
    textColor: "{colors.pw-notice-ink}"
  badge:
    backgroundColor: "{colors.pw-notice-bg}"
    textColor: "{colors.pw-notice-ink}"
    typography: "{typography.state}"
    rounded: "{rounded.pill}"
    padding: "4px 10px"
  tag:
    backgroundColor: "{colors.pw-vellum}"
    textColor: "{colors.pw-ink-2}"
    typography: "{typography.state}"
    rounded: "{rounded.pill}"
    padding: "4px 10px"
  state-band:
    backgroundColor: "{colors.pw-danger-bg}"
    textColor: "{colors.pw-danger-ink}"
    typography: "{typography.body}"
    rounded: "{rounded.lg}"
    padding: "16px 24px"
---

# Design System: E-toto

## Overview

**Creative North Star: "The Clear Board"**

E-toto looks like the tools its users already trust for everything else in their working
day. It is a modern light SaaS surface: a soft blue-grey ground, white cards floating on
diffuse shadow, generously rounded corners, one humanist sans in sentence case, and a
single saturated blue that means *act* or *go here*. The finish bar is explicit and
permanent — Monday, Asana, ClickUp — recorded as a brand commitment in PRODUCT.md after
the user saw and rejected an authorial technical-drawing direction. Nothing here should
read as engineering paper, and nothing should read as a legacy admin theme either.

What keeps it from being generic is that colour is rationed to one job. This is lockout /
tagout software: a person reads a screen next to an energised machine and has to know,
without hesitating, whether the isolation is active, being released, or finished. So the
lockout state is the only thing on the page allowed to wear a saturated red, amber, olive
or green, and it wears it as a filled pill with white text — the Monday status pattern —
carrying its written label inside. Everything else is grey, and everything actionable is
blue. Scanning a list, the eye finds the state column first because nothing competes with
it.

Density is comfortable rather than tight: 40px controls, 16px fields, 24px card padding,
a 4px-based rhythm. The system is built to survive both scenes the product serves — a
long office session on a wide monitor, and a gloved thumb on a phone beside the equipment.
Separation between things always comes from elevation and breathing room. Heavy frames,
grid lines and coloured card borders are not part of the vocabulary.

**Key Characteristics:**
- White cards floating on a blue-grey ground, 8-20px radius, soft two-layer shadows
- Lockout state as a solid colour pill with a written label, never colour alone
- Colour has an owner: blue acts, red/amber/green report lockout state, grey does the rest
- One typeface (Figtree), sentence case throughout, no tracked capitals
- Comfortable touch density: 40/48px targets, 16px fields, 4px spacing rhythm
- Phone-first shell: pill nav rail becomes a bottom tab bar, tables become record cards

## Colors

A cool, low-chroma neutral field with exactly one blue for action and a tightly rationed
safety palette for lockout state.

### Primary

- **Signal Blue** (`pw-notice`): The single action colour. Primary buttons, links, focus
  rings, the `accent-color` of every checkbox and radio, the spinner, the selected table
  row, and the active navigation pill. Nothing decorative is ever painted with it.
- **Deep Signal Blue** (`pw-notice-ink`): The pressed/hover partner. Text on tinted blue
  chips, hovered link text, hovered primary button.
- **Washed Signal Blue** (`pw-notice-bg`): The tint. Active nav pill background, hovered
  row-action button, informational alerts, the numbered isolation-point balloon, and the
  selected-row wash in tables.
- **Institutional Blue** (`brand-blue`): The Power Wave corporate blue. It lives on the
  logo and institutional material only. It is deliberately *not* the interface blue.

### Secondary — Lockout State

These four are the only saturated non-blue hues in the system, and they mean one thing
each. Each has a solid value for the pill, an optional darker `-ink` for text and button
hover, and a very pale `-bg` for tinted bands, alerts and badges.

- **Lockout Red** (`pw-danger` / `pw-danger-ink` / `pw-danger-bg`): Lockout active. The
  loudest state in the product and the one that means a machine is isolated.
- **Release Amber** (`pw-warn` / `pw-warn-ink` / `pw-warn-bg`): Release in progress, and
  the expiry warnings shown at login.
- **Pending Olive** (`pw-caution` / `pw-caution-bg`): Outstanding items, warning badges,
  and the staging-environment marker in the top bar.
- **Cleared Green** (`pw-safe` / `pw-safe-ink` / `pw-safe-bg`): Finalised, valid, in force.
- **Idle Grey** (`pw-idle` / `pw-idle-bg`): Created, no state yet. The absence of a state
  is still shown as a pill, so the column never has a hole in it.

### Neutral

- **Ground** (`pw-paper`): The application background. Everything floats on it.
- **Sheet** (`pw-sheet`): Cards, the top bar, the nav rail, fields, modals.
- **Recessed** (`pw-vellum`): Top-bar info pastilles, tags, input add-ons, disabled fields.
- **Whisper** (`pw-vellum-2`): Zebra and hover rows, card footers, modal footers, the
  info block, the empty state, the legend strip.
- **Ink** (`pw-ink`): Headings and body copy on white (≈15:1).
- **Ink Soft** (`pw-ink-2`): Secondary copy, field labels, ghost-button text.
- **Ink Muted** (`pw-ink-3`): Labels, table headers, meta lines, placeholders, resting
  row-action icons.
- **Ink Faint** (`pw-ink-4`): Unavailable only — locked nav items and disabled icons. It
  does not meet text contrast and must never carry reading content.
- **Ink Deep** (`pw-ink-deep`): Hover state of the dark button.
- **Hairline** (`pw-hair`): The default divider — table rows, card headers, shell edges.
  Almost invisible on purpose.
- **Outline** (`pw-line`): Field and card outlines, timeline connectors, the void-pill border.
- **Outline Strong** (`pw-heavy`): Hover/focus outline of fields and ghost buttons.

### Named Rules

**The Colour Has an Owner Rule.** Blue is action and navigation. Red, amber, olive and
green are lockout state, never decoration. Grey is everything else. A new colour needs a
new job, and there are no new jobs.

**The Solid Pill Rule.** Lockout state renders as a fully filled pill with white text
(`.pw-sig--*`), pill radius, 12px bold. Not a bordered chip, not coloured text, not an
icon. Cancelled records are the single exception — `.pw-sig--void` is a transparent pill
with a hairline border and a strikethrough, because the record exists but no longer counts.

**The Written Label Rule.** Every state pill contains its label in words. Colour is
reinforcement, never the only carrier. A state list gets a `.pw-legend` strip above it.

**The Reserved Brand Blue Rule.** `#006CB5` stays on the logo and institutional material.
The interface uses `#1570DB` from the same family. Do not paint UI with the logo blue.

**The White-Text Floor Rule.** Every solid state fill is chosen so white text on it clears
4.5:1. Two carried values sit just under the bar — Release Amber (≈4.4:1) and Idle Grey
(≈4.4:1) — and should be darkened before the palette is treated as verified; do not add a
fifth fill below that line, and do not lower the bar to match them.

## Typography

**Display Font:** Figtree — loaded from the Google Fonts CDN (weights 400/500/600/700/800)
**Body Font:** Figtree
**Fallback:** `system-ui, -apple-system, 'Segoe UI', sans-serif` — the page is legible and
correctly proportioned if the CDN is unreachable.

**Character:** One geometric humanist sans doing every job. Figtree is round enough to
belong in the modern-SaaS world and neutral enough that a safety record never looks
styled. Weight and colour do all the hierarchy work; case and tracking do none of it.

### Hierarchy

- **Display** (800, 32px, 1.1, -0.02em, tabular figures): Exactly one thing — the anchor
  value of an information block, in practice the PLE number. Drops to 24px on phone.
- **Headline** (700, 24px, 1.25, -0.015em): Page-level `h1`.
- **Title** (700, 18px / 16px, 1.25): `h2` and `h3`; card and modal titles.
- **Body** (400, 14px, 1.5): The interface default — table cells, paragraphs, buttons,
  navigation, checkbox labels.
- **Field** (400, 16px): Every text input, select and textarea.
- **Label** (600, 13px, sentence case, no tracking): Field labels, table headers, info-block
  keys, section titles. Rendered in Ink Muted, so it reads as a caption, not a heading.
- **State** (700, 12px): Status pills, badges, tags.
- **Meta** (600, 11px): Top-bar pastille keys, timeline timestamps, balloon numbers.

### Named Rules

**The One Family Rule.** This system has a single typeface. `--pw-font-nar` exists only as
an alias of `--pw-font` and resolves to the same stack; it is a leftover name, not a second
family. Do not introduce a display, condensed or monospace face.

**The Sentence Case Rule.** No uppercase, no letter-spaced capitals, anywhere. Labels,
table headers, buttons, tabs and pills all set `text-transform: none` and
`letter-spacing: 0` explicitly, because the framework underneath sets otherwise.

**The 16px Field Rule.** Text inputs, selects and textareas are 16px and never smaller —
including `.form-control-sm`. Below 16px iOS zooms the page on focus, which is unusable
when someone is filling a lockout form one-handed at the machine.

**The Tabular Number Rule.** PLE numbers, timestamps and any figure that appears in a
column carry `.pw-num` (tabular figures, weight 600) so digits line up between rows.

## Layout

The shell is three distinct planes, not one white slab: a sticky white top bar (60px)
that floats on a soft shadow, carrying a 3px signal-blue rule at the very top edge, the
brand at the left behind a hairline divider, and context pastilles plus account actions at
the right; a sticky navigation rail directly beneath it that sits on the blue-grey ground,
so its pills read as lifting off it — they turn white on hover; and the content in a
fluid container that caps at 1680px above 1400px. The footer answers the top bar: the
same recessed band as the rail, carrying the Power Wave mark, and the same 3px rule —
opening the app above, closing it below. The rule is hidden under 768px, where the fixed
tab bar covers that edge. There is no sidebar — AdminLTE's left margins are neutralised.

Spacing runs on a 4px rhythm (4 / 8 / 12 / 16 / 24 / 32 / 48 / 64). Cards use 24px body
padding and 16px/24px headers, dropping to 16px and 12px/16px below 992px. Content sits
24px from the rail and 48px from the bottom, plus a reserved band for the mobile tab bar
and the device safe area.

Touch targets are 40px standard (`--pw-tap`) and 48px for large and stacked actions
(`--pw-tap-lg`); small buttons floor at 34-36px, and row-action icon buttons at 32px.

Breakpoints, and what actually changes at each:

- **≤991.98px** — optional top-bar pastilles hide; card padding tightens; shell gutters
  drop to 16px.
- **≤767.98px** — the whole phone treatment: bottom tab bar, record cards, stacked
  actions, 10px card radius, tighter state bands, 24px anchor value.
- **≥1400px** — the fluid container caps at 1680px so long tables do not run edge to edge.

### Named Rules

**The Thumb Rail Rule.** The navigation is one component with two forms. At 768px and up
it is a horizontal strip of pills below the top bar, horizontally scrollable with the
scrollbar hidden. Below 768px it detaches, fixes to the bottom of the viewport, spreads
its items evenly, stacks icon over 11px label in 54px targets, and pads itself for the
home indicator. The page reserves that height (`--pw-bottom-h: 62px`) so nothing is ever
hidden behind it.

**The Record Card Rule.** On phone, a table marked `.pw-table-cards` stops being a table:
each row becomes a white record card with its own hairline, 10px radius and soft shadow,
and each cell prints its `data-label` as a 13px muted key at 40% width beside the value.
Empty cells disappear. The state cell is ordered first (`order: -1`), so the lockout state
is the first thing read on every card. The header row stays in the DOM, visually hidden.

**The No Grid Rule.** Tables have no vertical rules and no filled header. The header is
muted 13px text over a single Outline underline; rows are separated by hairlines and
distinguished on hover by the Whisper wash. Weight and colour do the work borders would.

## Elevation & Depth

The system is layered, not flat. Depth is the primary separator: a white sheet sits on the
blue-grey ground and is lifted by a soft, wide, low-opacity shadow with no visible offset
direction. Every shadow is built from two blurred layers over a single ink RGB channel
(`--pw-shadow-rgb: 28, 35, 51`), so the shade is cool and matches the ground rather than
reading as black. Borders exist but are hairlines whose job is definition at the edge, not
separation.

### Shadow Vocabulary

- **Rest** (`--pw-sh-1`: `0 1px 2px rgba(28,35,51,.05), 0 1px 3px rgba(28,35,51,.06)`):
  Cards, primary buttons at rest, mobile record cards, the empty-state icon disc.
- **Raised** (`--pw-sh-2`: `0 2px 6px rgba(28,35,51,.06), 0 4px 12px rgba(28,35,51,.06)`):
  Hovered primary button. The only hover elevation in the system.
- **Floating** (`--pw-sh-3`: `0 8px 20px rgba(28,35,51,.09), 0 2px 6px rgba(28,35,51,.05)`):
  The login card — a single surface alone on the ground.
- **Lifted** (`--pw-lift`: `0 20px 48px rgba(28,35,51,.16), 0 6px 16px rgba(28,35,51,.08)`):
  Modals only. The largest jump in the system, and the only one that reads as "above".
- **Focus ring** (`--pw-ring`: `0 0 0 3px rgba(21,112,219,.22)`): A soft blue halo on
  `:focus-visible`, replacing the native outline everywhere, with 8px radius so it hugs
  rounded controls.

The mobile tab bar carries an upward shadow (`0 -2px 12px rgba(28,35,51,.06)`) — the same
diffuse language, inverted, because it sits above content rather than below it.

### Named Rules

**The Floating Surface Rule.** Separation comes from elevation and breathing room, never
from a heavy frame. A card is a white sheet with a hairline, a 14px radius and the Rest
shadow. If two things need to feel distinct, raise one or space them — do not draw a line.

**The Diffuse Shadow Rule.** Shadows are wide, soft and low-opacity, tinted with the ink
channel. No hard offsets, no dark edges, no coloured glows except the blue focus ring.

**The Four Steps Rule.** Rest, Raised, Floating, Lifted — and nothing between. Elevation
is a ladder with four rungs, not a continuum.

## Shapes

Nothing in this system has a square corner. The radius ladder is a deliberate scale where
each step maps to a size of thing: 6px for the smallest controls (checkbox, radio), 8px
for fields, small buttons and row-action buttons, 10px for standard buttons, navigation
pills, tabs, alerts and phone-sized cards, 14px for cards, information blocks, state bands
and empty states, and 20px for modals — the largest surface gets the softest corner. Pill
radius (999px) is reserved for things that are read as a token rather than a container:
status pills, badges, tags, isolation-point capsules, and the top-bar info pastilles.
Truly round (50%) is reserved for markers: the balloon number, the timeline dot, the state
band's leading dot, the empty-state icon disc.

Strokes are hairlines. Cards, tables and dividers use the near-invisible Hairline; fields
and outlined elements use Outline; hover and focus deepen to Outline Strong. Nothing in
the system uses a stroke thicker than 1px except the 2px timeline connector and the 2px
focus outline on a selected record card.

### Named Rules

**The Nothing Is Square Rule.** Every container, control and token carries radius. `0` is
not an available corner value — a `--pw-r-0` token exists in the stylesheet but is
referenced nowhere and should be treated as dead, not as permission.

**The One Selection Mechanism Rule.** There is exactly one checkbox and one radio in this
product: the native input, sized to 18px, tinted with `accent-color`, sitting in the normal
flow with an 8px gap to its label. The icheck-bootstrap and Bootstrap `custom-control`
plugins draw their own boxes on label pseudo-elements; both are switched off with
`content: none !important`. That was not a shortcut — competing with those plugins on
specificity failed repeatedly, and re-enabling them will reintroduce the bug where the
drawn box loses its tick. Do not restore them, and do not add a third mechanism.

## Components

### Buttons

- **Shape:** Softly rounded (10px); small and extra-small variants tighten to 8px.
- **Size:** 40px standard, 48px large, 36px small, 34px extra-small, all full-width when
  stacked on phone. Icon and label sit in a flex row with an 8px gap.
- **Primary:** Signal Blue fill, white label, Rest shadow. Hover deepens to Deep Signal
  Blue and lifts to Raised.
- **Secondary / Info:** White fill, Outline border, Ink Soft label. Hover fills Recessed,
  border deepens to Outline Strong. This is the default for anything not the main action.
- **Success / Warning / Danger / Dark:** Solid fills from the state palette with white
  labels, for the action that *causes* that state — start, release, cancel, finalise.
- **Outline variants:** Not outlines at all — a pale tinted fill with a transparent border
  and dark tinted text, which fills solid on hover. A quieter destructive or informational
  action that still reads as clickable.
- **Press:** 1px downward nudge (`translateY(1px)`). Disabled drops to 50% opacity with no
  shadow and no nudge.
- **Focus:** The blue halo ring, forced over the framework's own focus styles.

### Row Actions

- **Style:** 32px square ghost icon buttons in a 2px-gap row. Ink Muted at rest, no fill,
  no border.
- **Hover:** Washed blue fill with Deep Signal Blue icon; the destructive variant washes red.
- **Touch:** Under `@media (hover: none)` the tinted state is permanent, because a device
  without hover would otherwise show only grey icons and hide the destructive one.

### Status Pill (signature)

- **Style:** Solid state fill, white text, pill radius, 12px/700, 4px 12px padding, no
  border. Six filled variants (danger, warn, caution, safe, notice, idle) plus the outlined
  `void` for cancelled records.
- **Behaviour:** Always contains its written label. Appears in tables, in the phone record
  card's first cell, in the legend strip, and inline in the login form.

### State Band (signature)

- **Style:** A full-width tinted block (14px radius, 16px/24px padding) in the pale state
  tint, with the matching dark state ink for text and a 10px round dot in `currentColor`
  haloed by a 4px white ring.
- **Behaviour:** Carries the record identity (PLE number, tabular figures), the state name
  and a short note. The `--sheet` variant clips to the top corners of a card and sticks
  below the shell as the page scrolls, so the state stays on screen while the user reads
  the record. Enters with `.pw-stamp` — a 280ms rise-and-fade.

### Timeline (signature)

- **Style:** Horizontal, shared unchanged by the PLE and Risk Assessment surfaces. Fixed
  140px event columns (124px on phone) that overflow into a horizontal scroll rather than
  stretching — two events spread across a wide screen destroyed the sense of sequence.
- **Marker:** 36px circle with a 12px icon; Idle wash by default, filled with the state
  colour and white icon for danger / warn / safe / notice. Connected by a 2px Outline
  thread drawn edge-to-edge between markers, suppressed on the last item.
- **Rule — The Timeline Echo Rule.** A timeline dot uses exactly the same colour as the
  status pill for the same state. The sequence and the status must tell one story.

### Cards / Containers

- **Corner Style:** 14px (10px on phone).
- **Background:** White sheet on the blue-grey ground.
- **Shadow Strategy:** Rest. See Elevation & Depth.
- **Border:** 1px Hairline. AdminLTE's coloured `card-outline` top bar is explicitly
  neutralised — state colour lives in the pill, never in the frame.
- **Internal Padding:** 24px body, 16px/24px header, 16px/24px footer on the Whisper wash.
- **Nested cards:** flatten — Outline border, no shadow, 10px radius.

### Information Block

A recessed panel (Whisper wash, 14px radius, hairline) that holds a record's identity
fields. One column on phone, two at 576px, four at 992px. The `--anchor` field spans the
full width and renders its value at Display size with tabular figures.

### Inputs / Fields

- **Style:** White fill, Outline border, 8px radius, 40px tall, 16px text, 12px horizontal
  padding, no inner shadow. Textareas grow from a 96px floor.
- **Label:** 13px/600 Ink Soft, sentence case, 6px above the field.
- **Hover:** Border deepens to Outline Strong. **Focus:** Border turns Signal Blue and the
  blue halo ring appears.
- **Disabled / readonly:** Recessed fill, Ink Muted text, not-allowed cursor.
- **Error:** Lockout Red border with a 12px Deep Red message below. Valid-and-modified
  fields take a Cleared Green border.

### Selection Controls

Native checkbox and radio at 18px with `accent-color` set to Signal Blue, 8px from a 14px
medium-weight label, in a 30px-minimum row. See **The One Selection Mechanism Rule**.

### Navigation

- **Top bar:** White, sticky, 60px, hairline underline. Brand image at 30px (26px on
  phone). Context data renders as Recessed pill pastilles with an 11px muted key and a
  13px semibold value; the staging marker uses the olive tint and drops its key. Account
  actions are 36px ghost buttons.
- **Rail item:** 38px pill, 14px/600, Ink Soft, icon at 85% opacity. Hover washes Recessed.
  Active takes the Washed Signal Blue fill with Deep Signal Blue text and full-opacity icon.
  Locked items render in Ink Faint with a small padlock and no hover response.
- **Mobile:** See **The Thumb Rail Rule**.
- **Sub-tabs:** `nav-pills` share the rail's language exactly — 38px, 10px radius, washed
  blue when active — so a section's inner tabs read as a smaller echo of the main rail.

### Badges, Tags and Balloons

- **Badge:** Pill radius, 12px/600, pale state tint with dark state text — the *quiet*
  counterpart to the status pill, for counts and inline flags.
- **Tag:** Pill radius, Recessed fill, Ink Soft, for equipment tags and person names.
- **Balloon:** 24px blue-washed circle with an 11px bold tabular number, used to sequence
  isolation points. Paired with a tag inside a Recessed capsule to form an isolation-point
  chip.

### Overlays

Modals: 20px radius, no border, Lifted shadow, clipped content. The header is white with a
hairline underline and Ink title — legacy `bg-primary` / `bg-dark` / `bg-danger` header
classes are all forced back to white. **The Neutral Header Rule:** a modal's intent is
expressed by the button in its footer, never by a coloured header. The footer sits on the
Whisper wash. The backdrop is Ink at 45%.

### Empty and Loading States

Empty: Whisper panel, 14px radius, 64px vertical padding, centred Ink Muted text above a
56px white circular icon disc carrying the Rest shadow. Loading: 2px spinner in Signal Blue.

## Do's and Don'ts

### Do:

- **Do** float a white card on the blue-grey ground with a 14px radius, a hairline and the
  Rest shadow. Elevation and space separate things; lines do not.
- **Do** render every lockout state as a solid pill with white text and its written label.
- **Do** keep colour to its owner: blue for action and navigation, the state palette for
  lockout state only, grey for everything else.
- **Do** set text inputs, selects and textareas at 16px, including small variants.
- **Do** use the native input with `accent-color` for every checkbox and radio.
- **Do** give tables `.pw-table-cards` and every cell a `data-label`, and mark the state
  cell `.pw-cell--state` so it leads the record card on phone.
- **Do** keep touch targets at 40px, and 48px for stacked or primary phone actions.
- **Do** put tabular figures (`.pw-num`) on PLE numbers, timestamps and any column of digits.
- **Do** match a timeline dot's colour to the status pill for the same state.
- **Do** let the state band stick to the top of a long record so the state never scrolls away.

### Don't:

- **Don't** reintroduce square corners, a paper/graphite palette, or technical-drawing
  typography. This is a permanent brand commitment recorded in PRODUCT.md, not a preference.
- **Don't** set `text-transform: uppercase` with positive letter-spacing on labels, headers,
  buttons or tabs. Two page-local styles in Equipamentos and Home still do
  (`.energy-type-text`, `.template-section-title`); they are carried defects, not the pattern.
- **Don't** paint lockout state on a card border, a card header or a modal header. The pill
  and the tinted band are the only carriers.
- **Don't** add a second typeface. `--pw-font-nar` is an alias of `--pw-font`, not a slot.
- **Don't** re-enable the icheck-bootstrap or `custom-control` drawn boxes. They are
  disabled deliberately and their pseudo-elements must stay `content: none`.
- **Don't** use hard-offset, dark-edged or coloured shadows. The only coloured shadow in
  the system is the blue focus ring.
- **Don't** use Ink Faint for anything a user has to read; it is an unavailability signal.
- **Don't** rely on colour alone for state, and don't ship a state pill without its label.
- **Don't** add infinitely looping animation. `.ple-blink` (a 1.6s infinite opacity pulse on
  the maintenance badge) is legacy carried from the previous theme, not a house pattern; it
  survives only because `prefers-reduced-motion` clamps it.
- **Don't** introduce a new accent, a new radius step or a fifth elevation. The ladders are
  closed.
