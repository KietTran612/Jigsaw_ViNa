# AI Image Brief for UI/UX Asset Generation

## Purpose

This document is a companion brief for AI image generation and AI art-direction review.

Use it together with:

- `docs/plans/2026-06-16-ui-ux-design-system-and-screen-spec.md`

The design-system document defines the approved UI/UX direction and screen behavior. This companion document explains what an AI image assistant should understand, what it should produce, and what it must avoid when helping create images, mood boards, or production-oriented layered UI assets.

This document does not contain final generation prompts. The AI image assistant should create prompts from this brief and the main design-system spec.

## What The AI Should Understand

The game is a mobile landscape jigsaw puzzle game with meta progression, daily rewards, collection items, picture unlocks, and reward flow.

The approved art direction is:

- Clear casual mobile UX.
- Warm Vietnamese handmade identity.
- Clean relaxing puzzle gameplay.
- Icon-first UI with Vietnamese supporting text rendered by Unity, not baked into images.
- Mobile landscape primary layout, 16:9 first and 4:3 safe.
- DOTween-first animation in Unity; generated images should support animated UI layers rather than trying to show motion in one flat image.

The AI should not treat this as a generic fantasy game, casino game, match-3 game, RPG UI, or children-only game. It is a warm, approachable, culturally inspired jigsaw puzzle game.

## Core Visual Intent

The image direction should feel like:

- A handmade paper album.
- A stamp collection notebook.
- A warm craft table.
- A Vietnamese-inspired keepsake game UI.
- Clean, readable, touch-friendly mobile UI.

It should not feel like:

- A dark fantasy interface.
- A sci-fi, neon, or glassmorphism app.
- A corporate dashboard.
- A noisy festival poster.
- A generic beige/brown craft template.
- A children's toy UI with excessive bounce, candy colors, or oversized cartoon elements.

## Visual Motifs To Use

Use these motifs carefully and consistently:

- Handmade paper / paper fiber texture.
- Album pages.
- Photo/card frames.
- Stamp and seal shapes.
- Light craft wood.
- Soft shadows.
- Muted brick red.
- Muted jade or deep green.
- Soft rice-gold.
- Charcoal or warm dark text areas.
- Subtle Vietnamese traditional corner or divider accents.

Traditional Vietnamese accents should be restrained. They should appear as trim, corner marks, dividers, badge edges, or stamp details. Do not use dense traditional patterns behind text, puzzle pieces, reward amounts, or important icons.

## Color Direction

The image assistant should keep the palette warm but balanced.

Use:

- Warm ivory and handmade paper tones.
- Muted brick red or vermilion accents.
- Muted jade, deep green, or blue-green accents.
- Soft rice-gold for reward/highlight states.
- Light neutral wood for gameplay surfaces.
- Charcoal or warm dark areas where text will sit.

Avoid:

- Overly brown/orange screens.
- Low-contrast beige-on-beige UI.
- Saturated purple/blue gradients.
- Neon glow.
- Heavy black/dark UI.
- Busy high-contrast patterns.
- Photorealistic clutter that distracts from UI readability.

## Text And Icon Rules

The AI must not bake final text into generated assets.

Do not generate:

- Vietnamese words.
- English words.
- Fake UI labels.
- Numbers.
- Reward amounts.
- Timers.
- Item names.
- Lock reasons.
- Button labels.
- Streak day labels.

Unity will render all final text and dynamic values with TextMeshPro.

The AI may create:

- Empty panels with clean text-safe center areas.
- Decorative frames.
- Style references for icons.
- Non-semantic decorative stamps or marks.
- Placeholder-free UI skins.

If icon assets are requested later, they must be simple, readable, and reviewed for semantic clarity. Until then, AI-generated icons should be treated as style references, not final authoritative UI symbols.

## Asset Layering Rules

The AI should create layered assets that can be assembled in Unity.

Prefer:

- Transparent-background PNGs for frames, overlays, highlights, stamps, badges, button skins, and item slot skins.
- Opaque background images only for large screen backgrounds or mood-board references.
- 9-slice-friendly panels, cards, frames, and buttons.
- Clean center regions where Unity can place thumbnails, text, icons, reward amounts, and gameplay content.
- Reusable asset families with consistent edges, shadows, and material treatment.

Avoid:

- One flat screenshot containing final UI.
- Text baked into the image.
- Fixed reward numbers or day labels.
- Dense illustrations inside areas that need dynamic content.
- Cropped or irregular edges that cannot be reused.
- Shadows that make 9-slice scaling look broken.
- Perspective distortion on reusable UI panels.

## Production Asset Requirements

When the AI image assistant creates or proposes asset prompts, it should classify each output as one of:

- Mood/reference image.
- Opaque background.
- Transparent overlay.
- 9-slice-friendly panel.
- Button skin.
- Card/frame skin.
- Highlight/glow overlay.
- Decorative accent.

For each proposed asset, the AI should specify:

- Intended screen or component.
- Whether it is for Unity production use or mood reference only.
- Transparent or opaque background.
- Whether it should be 9-slice-friendly.
- Whether the center must remain clean.
- State variant if applicable: normal, highlighted, locked, claimed, disabled, new, selected.
- Any important safe-zone notes.

The AI does not need to choose final Unity import settings, but it should avoid designs that would obviously be hard to import, slice, scale, or layer.

## Screen-Level Image Goals

### Daily Login Reward

The AI should help create a 7-day stamp-book reward look.

Needed visual ideas:

- A large handmade paper popup panel.
- Seven reward slot frames that look like stamps or coupons.
- A claimed stamp/check overlay.
- A today highlight frame.
- Small decorative corner accents.

Important:

- Reward slots need clean centers for Unity reward icons and text.
- Claimed, today, and future states must be visually distinct.
- Do not draw day numbers or reward amounts.
- Do not make the popup look like a casino bonus screen.

### Home Gallery

The AI should help create an album-gallery feeling.

Needed visual ideas:

- Warm handmade album background.
- Picture card frames.
- Locked parchment overlay.
- Ready-to-unlock glow frame.
- Resource chips.
- Daily and Collection button skins.

Important:

- Picture thumbnail areas must stay clean and open.
- Cards must not include fake titles.
- Meta buttons should be visible but secondary.
- The screen should not become a map/progression path.

### Difficulty Select

The AI should help create a beautiful album detail page.

Needed visual ideas:

- Album detail background.
- Large picture frame.
- Right-side info panel.
- Difficulty card frames.
- Reward stamp holders.
- Subtle divider ornament.

Important:

- The picture is the hero.
- Right panel must remain readable and not over-decorated.
- Difficulty card variants should be visually related.
- Do not draw fake difficulty labels, stars, or times.

### Gameplay

The AI should help create clean, tactile gameplay support assets.

Needed visual ideas:

- Warm neutral tabletop background.
- Light wood or handmade board frame.
- Vertical tray panel.
- Invalid drop feedback overlay.
- Hint highlight overlay.
- Compact header button skins.

Important:

- Gameplay must be visually quieter than meta screens.
- Board and puzzle pieces must remain easy to inspect.
- Tray background must not reduce piece readability.
- Do not add large decorative motifs behind the board.
- Do not create debug-looking toolbar art.

### Reward / Win Popup

The AI should help create a moderate celebration result panel.

Needed visual ideas:

- Result popup panel.
- Completed picture frame.
- Star/seal holder.
- Reward item slot.
- Primary CTA button skin.
- Sparse celebration accent overlay.

Important:

- Celebration should be warm and satisfying, not noisy.
- Confetti/glow should be sparse.
- Reward slots must stay readable.
- Do not bake stars, text, reward numbers, or item names into the image unless explicitly requested as a non-production mockup.

### Collection

The AI should help create a stamp/item album.

Needed visual ideas:

- Open handmade album background.
- Item slot frames.
- Missing silhouette overlay.
- Detail note panel.
- Source link chip/button skin.
- New badge/glow.

Important:

- Owned, missing, and new states must be distinguishable.
- Item slots need clean centers for item icons.
- Source/detail panel must be readable.
- The screen should look like a collection album first, not a technical inventory table.

### Pause / Settings

The AI should help create compact utility popup assets.

Needed visual ideas:

- Small paper note settings panel.
- Utility button skins.
- Toggle switch skins.
- Warning confirmation panel.

Important:

- This screen should be simpler and less decorative than Daily or Collection.
- It should feel practical, readable, and safe.
- Do not add celebration effects.

## Large Mockup Images For AI Review Only

The AI image assistant may also create large full-screen mockup images, but these are for AI review, art-direction alignment, and prompt iteration only.

Large mockup images are not production assets. Do not plan to cut UI components out of these mockups for Unity.

Use large mockups to:

- Validate the overall art direction.
- Compare screen composition and hierarchy.
- Check whether paper, stamp, album, wood, and Vietnamese accents feel consistent.
- Identify if any screen is too busy, too brown/beige, too childish, too generic, or too hard to read.
- Help the user choose between visual treatments before requesting individual layered assets.
- Help another AI write better per-asset generation prompts.

Recommended large mockup targets:

- One full-flow style board showing the shared visual language across Daily, Home, Difficulty, Gameplay, Reward, Collection, and Pause.
- One large Home gallery mockup.
- One large Difficulty Select album-detail mockup.
- One large Gameplay board-left/tray-right mockup.
- One large Daily Reward stamp-book popup mockup.
- One large Reward/Win popup mockup.
- One large Collection album mockup.

Large mockup requirements:

- Use mobile landscape composition.
- Prefer 16:9 large images such as 1920x1080.
- Optionally create 4:3 review variants such as 1440x1080 for layout resilience.
- Keep text either absent or clearly marked as placeholder-only; do not rely on AI-generated text accuracy.
- Do not bake final Vietnamese UI copy into the mockup.
- Do not treat the mockup as exact Unity layout, exact pixel spacing, or final prefab hierarchy.
- Do not use mockups as the only source for production assets.

When producing large mockups, the AI should label them clearly as:

- `MOOD / REVIEW ONLY`
- `NOT FOR UNITY SLICING`
- `NOT A PRODUCTION ASSET`

After large mockups are reviewed, the next step should be generating individual layered assets, such as panel frames, card frames, button skins, overlays, stamps, highlights, badges, board frames, and tray panels.

## Mood Board Expectations

If creating mood-board images, the AI should focus on:

- Overall material direction.
- Palette balance.
- How paper, stamps, album cards, and light wood work together.
- How Vietnamese accents can be subtle.
- How gameplay can stay clean while meta screens feel warmer.

Mood-board images can be illustrative and do not need to be directly importable. They still should avoid fake text and clutter unless explicitly labeled as rough reference only.

## Usable Asset Expectations

If creating production-oriented assets, the AI should focus on:

- Clean edges.
- Transparent backgrounds when needed.
- Repeatable component families.
- Minimal internal decoration.
- Clear safe zones.
- Consistent shadows and corner radius.
- Variants that can layer cleanly in Unity.

Production-oriented assets should not include:

- Full screen mockup text.
- Fake buttons with unreadable labels.
- Complex perspective.
- Heavy baked lighting that conflicts with Unity layering.
- Content that assumes one exact resolution only.

## Review Checklist For AI Output

Before accepting any generated image or prompt set, verify:

- Does it match warm Vietnamese handmade casual puzzle direction?
- Is it suitable for mobile landscape UI?
- Does it avoid baked text, numbers, and fake labels?
- Are dynamic content areas clean?
- Can the asset be layered in Unity?
- Is the center safe for Unity-rendered text or icons?
- Are traditional accents restrained?
- Is the palette balanced and readable?
- Does gameplay art stay quieter than meta-screen art?
- Are state variants visually distinct but still part of the same family?
- Would the asset still work when scaled or 9-sliced?
- Does it avoid debug/tool UI aesthetics?
- Does it avoid generic fantasy, casino, sci-fi, or corporate dashboard styles?

## Expected AI Deliverable

The AI image assistant should not merely produce a single pretty mockup.

It should provide a structured output such as:

- A short interpretation of the target style.
- A proposed asset list by screen/component.
- Classification of each asset: mood reference, background, transparent overlay, 9-slice panel, button skin, card skin, highlight overlay, or decorative accent.
- Notes about transparency, 9-slice suitability, center safe zone, and state variants.
- Prompt drafts or prompt strategy if requested by the user.
- Warnings when a requested image would likely be hard to use in Unity.

The AI should ask for clarification if it cannot determine whether the user wants mood/reference images or production-oriented layered assets.

## Key Source Of Truth

The companion design-system spec remains the source of truth for:

- Screen behavior.
- UI hierarchy.
- Flow logic.
- DOTween animation direction.
- Responsive 16:9-to-4:3 layout rules.
- What belongs in Unity-rendered UI versus generated image assets.

This AI image brief exists to help another AI create better image prompts and asset plans without misreading the design direction.
