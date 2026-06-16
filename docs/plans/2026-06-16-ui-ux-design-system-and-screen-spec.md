# UI/UX Design System and Screen Specification

## Purpose

This document defines the UI/UX direction for the full current Jigsaw ViNa game flow. It is intended to be detailed enough to guide:

- Unity UI implementation.
- Future UI review passes.
- AI-assisted mood-board and layered asset generation.
- Consistent screen-by-screen design decisions across Home, Daily Reward, Difficulty Select, Gameplay, Reward, Collection, and Pause/Settings.

This is a design source of truth, not an implementation plan. It does not prescribe final prefab hierarchies, serialized field names, test routes, or exact asset filenames.

Where this document says "should", treat it as the approved design direction for future UI work. Implementation plans can still phase the work, but they should call out any intentional deviation from this spec.

## Approved Direction

The game should use a clear casual mobile UX foundation, layered with a warm Vietnamese handmade identity. Gameplay should remain clean and relaxing.

The chosen direction combines:

- Casual mobile clarity for readability, large controls, familiar rewards, and direct feedback.
- Warm Vietnamese handmade craft for paper, album, stamp, label, and light traditional accent motifs.
- Relaxed premium restraint for the actual puzzle screen, keeping the board readable and free from unnecessary decoration.

The game should not feel like a debug tool or a generic puzzle template in the release build. Developer-only controls can remain available in Editor or Development Build, but the player-facing UI should be polished, icon-first, readable, and warm.

## Device and Interaction Model

### Primary Device Target

- Primary orientation: mobile landscape.
- Primary aspect ratio: 16:9.
- Supported safe range: 16:9 to 4:3.
- Interaction model: two-handed landscape play.

The 16:9 layout is the main design target and should be the most polished composition. The 4:3 version must remain usable, readable, and non-overlapping, but it does not need to match the 16:9 layout pixel-for-pixel.

### Responsive Principles

- Preserve the puzzle board as the highest-priority gameplay element.
- Treat 1920x1080-style 16:9 composition as the primary visual target and 1440x1080-style 4:3 composition as the main resilience check.
- Avoid controls that require precise tapping near screen edges unless they are large enough for touch.
- Prefer proportional layout regions over fixed pixel-only positions.
- Use stable dimensions for repeated UI elements such as cards, reward slots, item slots, icon buttons, and tray pieces.
- Do not rely on viewport-width-scaled font sizes.
- Text must wrap, truncate, or use designed fallback states instead of overflowing.
- 4:3 can increase spacing or grid density where it helps; it should not introduce a new visual hierarchy.

### Touch Ergonomics

- Important buttons should be comfortably tappable on phone landscape.
- Gameplay controls should not conflict with drag-and-drop gestures.
- Repeated actions such as Hint, Pause, Back, and Return to Tray should be reachable without crossing over the board.
- Confirmation should be used only for destructive or progress-losing actions.

## Language and Localization

The UI should be icon-first with Vietnamese supporting text. The player should never need to guess what a core icon does.

Rules:

- Common gameplay controls use icons first, with short Vietnamese labels where needed.
- Popups, titles, reward explanations, lock reasons, and empty states can use Vietnamese text.
- Avoid long text inside buttons, cards, reward slots, and item slots.
- Design components to tolerate longer localized strings later.
- Text rendering should happen in Unity/TextMeshPro, not inside generated bitmap assets.
- AI-generated assets must not include final UI text.

## Visual Design System

### Visual Motif

Use a controlled mix of three materials:

1. Handmade paper / album / stamp
   - Primary motif for Home, Difficulty Select, Collection, Daily Reward, Reward Popup, and Settings panels.
   - Should feel like a warm notebook, album page, stamp collection, or crafted label system.

2. Light wood / tabletop / craft tray
   - Primary support motif for Gameplay board and tray.
   - Should feel tactile but not heavy or dark.

3. Vietnamese traditional accents
   - Use only as small accents, about 5-10% of a screen.
   - Good locations: frame corners, dividers, badge edges, stamp marks, reward glows, panel trim.
   - Avoid using dense pattern fields behind text, puzzle pieces, or the board.

### Palette

The palette should feel warm and Vietnamese-inspired, without becoming a one-note beige, brown, or yellow UI.

Recommended color families:

- Base paper: warm off-white, ivory, light handmade paper beige.
- Primary accent: restrained brick red or muted vermilion.
- Secondary accent: muted jade, deep green, or blue-green.
- Reward accent: soft rice-gold or warm gold.
- Text: charcoal, warm dark gray, or deep ink brown.
- Gameplay wood: light neutral wood, not dark espresso brown.
- Error feedback: clear red-brick outline or glow.
- Hint feedback: soft golden glow.

Avoid:

- Large saturated purple/blue gradients.
- Heavy brown/orange dominance.
- Low-contrast beige text on beige panels.
- Busy full-screen patterns.
- Purely decorative glow blobs or bokeh backgrounds.

### Typography

Typography should feel readable and friendly, not overly decorative.

Rules:

- Use short titles and compact labels.
- Keep hero-scale type only for true titles or celebration moments.
- Use smaller, tighter type inside cards, panels, slots, and buttons.
- No negative letter spacing.
- Text must fit its container across 16:9 and 4:3.
- Vietnamese diacritics must render cleanly.
- Later localization must not require redesigning every component.

### Component Families

The game should reuse a small set of component families:

- Picture Card: Home gallery card with thumbnail, title, lock state, progress, and missing item hint.
- Album Detail Panel: Difficulty Select composition with large image frame and metadata/difficulty cards.
- Difficulty Card: playable/locked/completed/recommended difficulty entry.
- Stamp Reward Slot: Daily reward and result reward item display.
- Collection Item Slot: album/stamp item slot for owned, missing, and newly acquired items.
- Gameplay Header Button: compact icon-first touch control.
- Resource Chip: coin/hint counters in the header.
- Popup Panel: paper/album panel for Daily, Reward, Pause, and confirmation dialogs.
- CTA Button: primary red-brick/gold button and secondary paper/outline button.
- Badge: new reward, available daily reward, ready unlock, and collection notification.

### Layering Rules for AI Assets

AI can generate visual materials and decorative layers, but Unity owns layout and information.

AI should generate:

- Background panels.
- Paper and wood textures.
- Frames, cards, stamps, button skins, reward slots, badge skins.
- Transparent overlays for glow, lock, hint, invalid feedback, and stamped states.
- Mood-board references.

Unity should render:

- Text.
- Numbers.
- Final semantic icons that need consistent meaning across screens.
- Reward amounts.
- Puzzle thumbnails and puzzle pieces.
- Stars, timers, buttons, sliders, toggles, badges, and selection states when these need data binding.

Do not use AI-generated full-screen UI mockups as source of truth for final layout. They are useful for mood and review only.

Asset-generation constraints:

- Prefer transparent-background PNGs for overlays, frames, highlights, stamps, badges, and button skins.
- Prefer 9-slice-friendly edges for reusable panels, cards, frames, and buttons.
- Keep content-safe center areas clean so Unity text, icons, thumbnails, and reward amounts remain readable.
- Do not bake Vietnamese or English text into bitmap assets.
- Do not bake fixed reward amounts, timers, stars, lock reasons, or item names into bitmap assets.
- Use AI-generated icons only as style references unless the icon has been reviewed for semantic clarity.
- Provide normal, highlighted, locked/disabled, and newly acquired variants only when the implementation plan explicitly needs them.

## Animation System

### DOTween-First Rule

DOTween should be the primary animation system for UI transitions and feedback.

Use DOTween for:

- Popup fade/scale in and out.
- Card press, selected, unlock, and focus feedback.
- Reward icon pop and stagger.
- Daily stamp sequence.
- Difficulty card highlight.
- Collection item reveal and selection.
- Gameplay snap, invalid drop, hint pulse, preview opacity, and return-to-tray movement.
- Sequence timing for reward and unlock moments.

Use Unity Particle System or equivalent for:

- Confetti or celebratory particles.
- Ambient reward sparkle, if needed.

DOTween should coordinate particle timing but does not need to replace particle systems.

### Cleanup Rules

Every animated view must own and clean up its tweens.

Rules:

- Kill tweens when the view hides, disables, or is destroyed.
- Avoid leaving looping tweens active on disabled GameObjects.
- Avoid tweening objects directly controlled by active drag input.
- Avoid tweening layout properties that fight `LayoutGroup` or active layout rebuilds.
- Prefer animating `CanvasGroup.alpha`, `RectTransform.localScale`, `RectTransform.anchoredPosition`, image color/alpha, overlay alpha, and explicit highlight objects.
- Gameplay tweens must be short and must not block player input except during win/result transition.

### Timing and Feel

Recommended duration ranges:

- Button press: 0.06s to 0.12s.
- Popup in/out: 0.18s to 0.30s.
- Card focus/unlock: 0.18s to 0.40s.
- Invalid drop shake: 0.15s to 0.25s.
- Correct snap settle: 0.12s to 0.25s.
- Reward item pop: 0.12s to 0.25s per item, staggered.
- Daily stamp: 0.25s to 0.45s.
- Win celebration lead-in: 0.5s to 1.2s total before result is actionable.

Recommended feel:

- Gameplay: restrained, fast, responsive.
- Home and Difficulty Select: soft and clear.
- Daily, Reward, Unlock: more expressive but still brief.
- Settings: minimal, functional.

## Core Flow

The intended player flow is:

1. App opens.
2. If a daily login reward is claimable, show Daily Login Reward before Home.
3. Player claims the reward, then continues to Home. A skip/close-before-claim behavior is not part of the default design and should only be added by an explicit later decision.
4. Home gallery appears.
5. Player selects a picture card.
6. Difficulty Select opens as an album detail screen.
7. Player starts a difficulty.
8. Gameplay uses board-left / tray-right layout.
9. On win, show a short celebration and Reward Popup.
10. Reward Popup proposes the best next step:
    - play next difficulty,
    - open/unlock a new picture,
    - return to Home,
    - choose another difficulty.

## Screen Specification: Daily Login Reward Popup

### UX Goal

Daily Login Reward is a daily event moment. It should feel rewarding, visible, and warm, but not slow. The player should immediately understand:

- Today is which day in the 7-day streak.
- What reward is available today.
- Which rewards were already claimed.
- Which rewards are upcoming.
- Which button continues the flow.

### Entry Conditions

Show automatically before Home when:

- A daily login reward is claimable.
- The player has not claimed today's daily reward.

Allow optional manual viewing from Home through the Daily button, especially if the player wants to review the streak.

Default claim policy:

- When the popup is shown because today's reward is claimable, the primary action is to claim the reward.
- Do not provide an ambiguous close action that can look like claiming but silently skips the reward.
- If a future design allows skipping, it must be an explicit secondary action with clear copy and a documented save-state outcome.
- If the popup is opened manually after the reward was already claimed, the primary action can be Continue/Close.

### Layout: 16:9

- Background: the loaded Home scene or current shell UI behind a dim overlay or light blur. Do not require a separate background illustration for this popup.
- Popup: centered, large, around 70-80% screen width and 65-75% screen height.
- Header: title at top, close/continue affordance at top-right if appropriate.
- Main area: 7 reward slots as a horizontal row.
- Current day: slightly larger, brighter, or framed more strongly.
- CTA area: bottom center with primary "Claim" action.
- Secondary text: short streak message if useful.

### Layout: 4:3

Prefer keeping seven slots in one row if the reward icon and amount remain readable. If not, use a 4+3 layout while preserving clear day order.

### Slot Content

Each day slot should include:

- Day label.
- Reward icon.
- Reward amount.
- State treatment.

States:

- Claimed: stamped/check overlay, reduced emphasis.
- Today claimable: highlighted border, soft glow, active CTA linkage.
- Future: muted, locked, or lower opacity.
- Missed/recovered state: not required for current scope unless the service later supports it.

### Visual Style

Use a stamp book / check-in card style:

- Popup background resembles handmade paper or a notebook page.
- Slots resemble stamps, coupons, or paper reward cards.
- Claimed state uses a red ink stamp or check mark.
- Today uses warm gold and brick-red accents.
- Future slots are softer, not harshly disabled.
- Decorative Vietnamese pattern appears only in corners, divider trims, or stamp edges.

### Interaction

- Claim button grants today's reward.
- After claim, slot changes to claimed.
- CTA changes to Continue or proceeds after a short reward moment.
- Close should not accidentally skip a claim in the default design. If skipping is allowed later, make that explicit.

### DOTween Animation

- Overlay fades in.
- Popup scales from about 0.96 to 1.0 and fades in.
- Today slot has a slow, subtle pulse while waiting.
- On claim:
  - reward icon pops,
  - stamp lands on today's slot,
  - reward amount briefly highlights,
  - optional reward flyout moves toward Home resource chips later.
- Total animation should not delay player control for long.

### AI Asset Brief

Create layered assets only. Do not include final text.

Assets:

- Daily popup paper panel:
  - warm Vietnamese handmade paper notebook page,
  - clean central area,
  - subtle paper fibers,
  - light brick-red and muted jade accents,
  - no text.
- Day reward slot frame:
  - small collectible stamp or coupon frame,
  - handmade paper,
  - soft shadow,
  - clear center for Unity icon/text.
- Claimed stamp overlay:
  - red ink stamp or check style,
  - transparent background,
  - no readable text.
- Today highlight frame:
  - warm golden and brick-red glow,
  - subtle,
  - transparent center.
- Corner ornament:
  - minimal Vietnamese traditional pattern,
  - low contrast,
  - not busy.

### Acceptance Criteria

- Seven days are visible and ordered clearly.
- Today's reward is unmistakable.
- Reward icon and amount are readable on mobile landscape.
- Claimed and future states cannot be confused.
- Text is Unity-rendered and can be localized.
- A claimable popup cannot be dismissed in a way that silently loses or skips the reward unless a later approved design explicitly adds skip behavior.
- Popup does not block Home longer than necessary after claim.

## Screen Specification: Home Gallery-First Hub

### UX Goal

Home is the fast entry point into the game. The player should see the picture gallery first, then understand meta options such as Collection, Daily Reward, currency, hints, and Settings without those options taking over the screen.

### Layout: 16:9

- Top/header area:
  - small game title/logo or location title on the left,
  - resource chips and action icons on the right.
- Main area:
  - picture gallery takes most of the space.
  - cards can be arranged as a scrollable grid or horizontal gallery depending on count.
- Utility entries:
  - Daily Reward button,
  - Collection button,
  - Settings button,
  - coin/hint chips.

The Home screen should not use a large side panel for meta systems in the default design. Daily Reward already appears before Home when claimable, so Home can remain gallery-first.

### Layout: 4:3

- Preserve header.
- Increase card spacing or visible card count where practical.
- Avoid adding a new major meta panel unless the later implementation needs one.

### Picture Card Content

Each card should show enough information for a decision:

- Thumbnail.
- Picture name.
- Lock/unlock/completed state.
- Best progress or best stars.
- Missing key item when locked.
- Ready-to-unlock affordance when requirements are met.

Do not put rich drop tables, all difficulty details, or long reward lists on Home cards. That belongs in Difficulty Select.

### Card States

- Unlocked:
  - full-color thumbnail,
  - normal card frame,
  - tap opens Difficulty Select.
- Completed:
  - full-color thumbnail,
  - completion mark or stars,
  - still playable.
- Locked:
  - dim or parchment overlay,
  - key item requirement shown compactly,
  - tap focuses the requirement hint instead of navigating.
- Ready to unlock:
  - soft glow,
  - clear unlock affordance,
  - tap unlocks or opens lightweight confirmation if needed.
- New/unseen:
  - small badge if the picture recently unlocked.

### Visual Style

Home should feel like an album gallery:

- Warm paper or subtle album surface background.
- Picture cards resemble pasted photo cards or album entries.
- Lock overlay resembles translucent parchment or a stamp.
- Ready-to-unlock uses gold/brick-red glow.
- Daily and Collection buttons can resemble small stamps or album tabs.

### Interaction

- Tap unlocked/completed card: open Difficulty Select.
- Tap locked card: focus it and show compact missing-item hint.
- Tap ready-to-unlock card: unlock the picture or show a confirm if future economy requires it.
- Tap Daily: open Daily Reward popup, especially for review or unclaimed reward.
- Tap Collection: open Collection.
- Tap Settings: open Settings popup.

### DOTween Animation

- Cards enter with light staggered fade/scale.
- Button/card press uses short scale feedback.
- Locked tap uses brief shake or requirement highlight.
- Ready-to-unlock card has slow glow pulse.
- Unlock success fades out lock overlay and pops card gently.
- Daily/Collection badges pulse very subtly.

### AI Asset Brief

Assets:

- Home background:
  - warm handmade paper album surface,
  - subtle Vietnamese craft texture,
  - clean and low contrast.
- Picture card frame:
  - collectible photo album card,
  - small-radius corners,
  - paper border,
  - soft shadow,
  - transparent or empty center for thumbnail.
- Locked overlay:
  - translucent parchment overlay,
  - subtle lock motif,
  - no text.
- Ready unlock glow:
  - soft gold and brick-red frame/glow,
  - transparent center.
- Resource chip:
  - small handmade label/chip frame,
  - clean center.
- Daily/Collection button skin:
  - album tab or stamp-like button,
  - icon-friendly center.

### Acceptance Criteria

- Picture gallery is the dominant first impression.
- Meta actions are visible but secondary.
- Lock, ready-to-unlock, and completed states are readable at a glance.
- Home remains usable if there are more pictures than fit on one screen.
- Card text does not overflow in Vietnamese or likely English localization.

## Screen Specification: Difficulty Select Album Detail

### UX Goal

Difficulty Select is the richer, more beautiful detail screen after selecting a picture. It should make the selected picture feel valuable and present enough meta information for the player to choose what to play next.

### Layout: 16:9

- Left side, about 50-60% width:
  - large picture in an album-style frame.
- Right side, about 40-50% width:
  - picture title,
  - category/source line,
  - completion summary,
  - difficulty cards,
  - reward and drop hints.
- Header:
  - Back button,
  - optional compact location/title.

### Layout: 4:3

- Keep the same left/right concept.
- Allow the image frame to shrink slightly so difficulty cards remain readable.
- Difficulty cards can use tighter vertical spacing.
- Avoid placing text over the image.

### Information Hierarchy

1. Picture image.
2. Picture name.
3. Current completion/progress.
4. Available recommended difficulty.
5. Locked/completed/playable difficulty states.
6. Rewards and drop hints.
7. Secondary metadata.

### Difficulty Card Content

Each difficulty card should include:

- Difficulty name.
- Piece count or short difficulty description.
- Play state:
  - playable,
  - locked,
  - completed,
  - recommended next.
- Best stars.
- Best time.
- First-clear reward if unclaimed.
- Drop hint or replay reward icon if configured.
- Main play CTA.

### Difficulty Card States

- Playable:
  - clear CTA,
  - normal brightness.
- Recommended:
  - soft highlight,
  - not distracting.
- Completed:
  - stars and best time visible,
  - still replayable.
- Locked:
  - dimmed card,
  - lock icon,
  - short reason such as previous difficulty requirement or picture locked.

### Visual Style

This screen should be an album detail page:

- Large picture frame resembles paper/wood hybrid album mount.
- Right panel resembles a note page or label panel.
- Difficulty cards resemble tickets or paper cards attached to the page.
- Reward icons sit inside small stamp frames.
- Decorative patterns remain in corners/dividers only.

### Interaction

- Back returns to Home.
- Tap Play on a playable card starts Gameplay.
- Tap locked card shakes briefly and highlights lock reason.
- Tap reward/drop icon can later open tooltip/source details.
- If arriving after unlock, play a short intro animation.

### DOTween Animation

- Image frame fades/scales in.
- Info panel slides in from the right.
- Difficulty cards stagger in.
- Play button press scales quickly.
- Locked card shakes and flashes lock reason.
- Completed stars can pop when returning from recent completion.

### AI Asset Brief

Assets:

- Album detail background:
  - warm handmade paper album spread,
  - clean left image area and right info area,
  - subtle Vietnamese craft aesthetic,
  - no text.
- Large picture frame:
  - elegant handmade paper and light wood hybrid frame,
  - soft shadow,
  - transparent center.
- Info note panel:
  - parchment note panel,
  - clean center,
  - subtle brick-red/jade accents,
  - 9-slice-friendly.
- Difficulty card frame:
  - small paper ticket/card frame,
  - variants for normal, locked, completed, recommended.
- Reward stamp holder:
  - small stamp/seal frame,
  - transparent center.
- Divider ornament:
  - subtle Vietnamese pattern line,
  - low contrast.

### Acceptance Criteria

- The picture is the hero of the screen.
- The player can identify the best next playable difficulty quickly.
- Locked reasons are understandable without long text.
- Reward/drop info is visible but does not overwhelm difficulty choice.
- Layout remains readable in 16:9 and 4:3.

## Screen Specification: Gameplay Board-Left Tray-Right

### UX Goal

Gameplay must be the cleanest screen. The player should focus on solving the puzzle, dragging pieces reliably, and receiving clear feedback.

### Current Direction

Keep the current board-left / tray-right concept:

- Board area on the left, about 70-75% width.
- Tray on the right, about 25-30% width.
- Header on top.
- Drag layer above all gameplay UI.

This direction fits landscape mobile and two-handed play better than tray-bottom for the current project.

### Layout: 16:9

- Header:
  - compact height,
  - icon-first controls,
  - no dev/debug controls in release.
- Main area:
  - board centered in left area,
  - tray scroll panel on right,
  - preview overlay aligned to board.
- Board:
  - preserve picture aspect ratio.
  - keep enough empty margin for drag settling and visual feedback.
- Tray:
  - vertical scroll,
  - pieces large enough for touch,
  - clear separation between pieces.

### Layout: 4:3

- Board remains highest priority.
- Tray can gain relative width or larger piece spacing.
- Header remains compact.

### Player-Facing Header

Release header should include:

- Pause/Settings icon as the preferred in-game entry point for continue/restart/home actions.
- Back icon only if it has an unambiguous, confirmation-protected behavior; otherwise route leaving the puzzle through Pause.
- Picture name and difficulty, shortened if needed.
- Timer.
- Hint icon with count.
- Preview opacity icon/slider or compact control.
- Return-to-tray icon.

Header controls should use icon-first design with short supporting labels only where needed.

### Dev-Only Controls

Developer controls such as Cheat Win:

- Must only appear in Editor or Development Build.
- Should not consume release layout spacing.
- Can live in a debug strip or debug menu.
- Must not alter player-facing hierarchy.

### Board Visual

- Board frame should be clear but understated.
- Use light wood or handmade frame treatment.
- Drop zones should be visible enough to support snapping but not visually loud.
- Preview image opacity defaults to 20% and can be adjusted.
- Locked pieces on the board should visually settle into the image.

### Tray Visual

- Tray should feel like a light craft tray.
- Use soft separators and shadows.
- Pieces should retain strong visibility against tray background.
- Avoid busy patterns behind pieces.

### Gameplay Interaction

- Pick up piece from tray or board.
- Drag with stable finger offset.
- Drop near correct slot to snap.
- Wrong drop shows invalid feedback and returns or remains according to existing behavior.
- Hint highlights a relevant piece/cell.
- Return-to-tray moves unlocked/unplaced pieces back to tray.
- Pause/Home/Restart actions with progress should confirm before losing state if required.

### DOTween Animation

- Piece pickup:
  - scale up slightly,
  - shadow stronger,
  - bring to front.
- Correct snap:
  - quick move/snap,
  - small settle scale,
  - warm highlight.
- Invalid drop:
  - red-brick outline,
  - short shake,
  - no long punishment.
- Hint:
  - 1-2 pulse cycles on piece or target cell.
- Return-to-tray:
  - staggered move back,
  - short duration,
  - no excessive waiting.
- Preview opacity:
  - tween alpha smoothly.
- Win:
  - board glow short,
  - then result popup.

### AI Asset Brief

Assets:

- Gameplay background:
  - warm neutral tabletop,
  - subtle handmade craft feel,
  - very clean,
  - no busy patterns.
- Board frame:
  - light wood or handmade frame,
  - rectangular transparent center,
  - soft shadow,
  - 9-slice-friendly.
- Tray panel:
  - vertical wooden/paper tray,
  - clean,
  - soft separators,
  - no text.
- Invalid feedback overlay:
  - red-brick outline or glow,
  - transparent background.
- Hint highlight:
  - warm golden pulse ring/glow,
  - transparent background.
- Header button skin:
  - compact icon button frame,
  - handmade paper/wood style,
  - clean center.

### Acceptance Criteria

- Board is the visual priority.
- Tray pieces are large enough for touch.
- Header does not feel like a debug toolbar in release.
- Leaving or restarting a puzzle with meaningful progress has confirmation-protected UX.
- Dev-only controls are hidden from release UI.
- Drag/drop feedback is clear and fast.
- Puzzle image remains easy to inspect.

## Screen Specification: Reward / Win Popup

### UX Goal

After completing a puzzle, the player should feel completion, see the finished picture, understand rewards, and know what to do next.

The moment should be celebratory but not long.

### Layout: 16:9

- Overlay over gameplay or transition to centered result popup.
- Popup about 65-75% screen width and 65-75% screen height.
- Completed picture displayed in an album frame.
- Result section includes:
  - completion title,
  - stars,
  - time,
  - first-clear reward,
  - replay/daily drop reward,
  - item/key item acquired,
  - unlock hints if relevant.
- CTA area:
  - primary recommended next step,
  - secondary Home/Difficulty option.

### Layout: 4:3

- If horizontal space is tight, picture can sit above result details.
- Reward list can wrap or scroll if needed.
- CTA remains visible without scrolling.

### Information Groups

Group result info clearly:

1. Completion result:
   - stars,
   - time.
2. Completion rewards:
   - coins,
   - hints,
   - key items,
   - first-clear items.
3. Extra drops:
   - replay/daily drop rewards if granted.
4. Progression impact:
   - new picture ready,
   - new difficulty available,
   - collection item added.

### CTA Rules

Use context-aware primary action:

- If next difficulty is playable: "Play Next Difficulty".
- If new picture is ready to unlock: guide to Home/focused unlock.
- If all relevant content is completed: "Back to Home".
- Always provide a secondary route to Home or Difficulty Select.

### Visual Style

- Popup resembles an album result card.
- Completed picture sits in a smaller display frame.
- Stars/rewards use stamp or seal styling.
- New item has soft glow or "new" badge.
- Sparse celebration accents only.

### DOTween Animation

- Board glow before popup.
- Popup fade/scale in.
- Stars pop one by one.
- Reward icons stagger in.
- New item glow.
- CTA appears after reward summary, but input should become available quickly.
- Confetti/particles are optional and restrained.

### AI Asset Brief

Assets:

- Result popup panel:
  - warm handmade paper result card,
  - album style,
  - celebratory but clean,
  - no text.
- Completed picture frame:
  - small elegant album photo frame,
  - transparent center.
- Star/seal holder:
  - golden/brick-red stamp-like medal frame,
  - transparent center.
- Reward item slot:
  - small paper/stamp reward slot,
  - variants normal/new/rare.
- CTA button skin:
  - warm brick-red primary button,
  - handmade paper edge,
  - readable center.
- Celebration accent:
  - sparse glow/confetti overlay,
  - transparent background.

### Acceptance Criteria

- Player sees the completed image.
- Stars/time/rewards are readable.
- Next action is obvious.
- Celebration does not delay interaction too long.
- Reward groups do not become a dense technical list.

## Screen Specification: Collection Album

### UX Goal

Collection should feel like a beautiful item album while still helping progression. The player should see owned and missing key items, understand where items come from, and know which pictures or difficulties they help unlock.

### Layout: 16:9

- Header:
  - Back,
  - title,
  - optional filter/category later.
- Main:
  - item grid or album page area,
  - detail panel for selected item.
- Grid:
  - item slots in album/stamp style,
  - scroll or paging if needed.
- Detail:
  - larger icon,
  - name,
  - short description,
  - owned/missing state,
  - source list,
  - used-to-unlock list,
  - navigation CTA if available.

### Layout: 4:3

- Increase grid columns or spacing if useful.
- Keep detail panel readable.
- If detail panel becomes too narrow in a future layout, it can become a popup on item tap.

### Item Slot States

- Owned:
  - full-color icon,
  - stamped/check treatment.
- Missing:
  - silhouette or muted empty slot.
- Newly acquired:
  - small new badge,
  - soft glow.
- Source available:
  - optional small marker if the item can currently be pursued.

### Detail Panel Content

For selected item:

- Icon.
- Name.
- Description.
- Owned/missing status.
- "Can be found from" source list.
- "Used to unlock" related picture list.
- CTA:
  - go to source,
  - focus related picture,
  - back to Home.

If no source is known, show a soft empty state instead of an error.

### Visual Style

- Strong album/stamp identity.
- Item slots resemble collectible stamps or album spaces.
- Missing slots resemble empty silhouettes.
- Detail panel resembles a note card.
- Traditional accents can be slightly more visible than on Home, but still restrained.

### DOTween Animation

- Collection opens with album/page fade or slide.
- Item slots stagger in.
- Selected item scales/highlights.
- Detail panel content crossfades.
- New item badge pulses subtly.
- Invalid/unavailable source tap shows small shake or hint.

### AI Asset Brief

Assets:

- Collection album background:
  - open handmade Vietnamese paper album,
  - warm,
  - clean grid areas,
  - no text.
- Item slot frame:
  - collectible stamp/album slot,
  - variants owned/missing/new,
  - transparent center.
- Missing silhouette overlay:
  - soft parchment/gray silhouette mask,
  - transparent background.
- Detail note panel:
  - handmade note card,
  - clean center,
  - subtle brick-red/jade accents.
- Source link chip:
  - small paper label button,
  - clean center.
- New badge/glow:
  - small warm stamp-like badge,
  - transparent background.

### Acceptance Criteria

- The screen feels like a collection album first.
- Missing and owned states are immediately distinguishable.
- Sources and unlock relationships are discoverable.
- Navigation from item source to relevant content is clear.
- Grid remains readable with growing item counts.

## Screen Specification: Pause / Settings Popup

### UX Goal

Pause/Settings should be simple, fast, and safe. It should not become a full settings hub in this phase.

### Gameplay Pause Layout

- Open from Gameplay header.
- Dim overlay over gameplay.
- Centered popup, smaller than Daily/Reward.
- Title: Pause/Settings.
- Actions:
  - Continue,
  - Restart,
  - Back to Home.
- Toggles:
  - Sound,
  - Music.
- Close button.

### Home Settings Layout

Can reuse the same popup component but omit Gameplay-only actions:

- Sound toggle.
- Music toggle.
- Close.
- Future language/credits/reset options are out of scope unless explicitly requested.

### Confirmation Rules

Confirm before:

- Restarting a puzzle with progress.
- Returning Home from a puzzle with progress if progress would be lost.

Confirmation popup:

- Short message.
- Primary confirm CTA.
- Secondary cancel CTA.
- No long text.

### Visual Style

- Compact paper note panel.
- Utility buttons are clear and minimal.
- Toggles use switch or checkbox states.
- Less decoration than Daily/Collection.

### DOTween Animation

- Popup fade/scale in.
- Button press scale.
- Toggle slide/snap.
- Confirmation can stack above Pause or temporarily replace it.
- No celebration effects.

### AI Asset Brief

Assets:

- Settings panel:
  - compact handmade paper note,
  - clean,
  - minimal,
  - no text.
- Utility button skin:
  - simple paper/wood frame,
  - normal/pressed/disabled variants.
- Toggle skin:
  - warm handmade toggle,
  - on/off states,
  - no text.
- Warning confirm panel:
  - small parchment panel,
  - subtle brick-red accent,
  - clean center.

### Acceptance Criteria

- Pause can be opened and closed quickly.
- Destructive actions require confirmation.
- Sound/music states are visible.
- Popup remains readable and touch-friendly on mobile landscape.

## Implementation Guidance for Future Plans

When this design becomes implementation work, break it into smaller tasks instead of attempting the whole UI/UX pass at once.

Recommended order:

1. Establish shared UI style assets and component primitives.
2. Polish Gameplay header, board frame, tray, and feedback because gameplay readability is highest risk.
3. Polish Home cards and header hub.
4. Polish Difficulty Select as Album Detail.
5. Polish Daily Reward popup.
6. Polish Reward/Win popup and next-step CTA flow.
7. Polish Collection Album.
8. Add Pause/Settings popup.
9. Add final responsive and animation pass across 16:9 and 4:3.

Recommended technical direction:

- Keep current uGUI and VContainer patterns.
- Use DOTween for animation.
- Add small, reusable UI animation helpers only if repeated cleanup/timing logic becomes duplicated.
- Prefer serializable view components with presenter-driven state.
- Keep debug/development controls behind build flags or debug-only containers.
- Add focused tests for state binding and event cleanup where presenters/views are changed.
- Use Unity visual/manual checks for layout-sensitive changes.

## Non-Goals

This design does not require:

- A new brand name.
- A portrait layout.
- A full map/progression-path Home.
- A fully custom localization system.
- A complete art asset pack in this document.
- A full validation suite by default.
- Rewriting existing scene architecture before a scoped implementation plan exists.

## Open Follow-Up Decisions

These can be decided during implementation planning or visual review:

- Exact card/grid sizing for Home at target phone resolutions.
- Whether a future Daily Reward design should allow an explicit "skip for now" action before claim.
- Whether Difficulty Select shows drop hints inline or behind a small info affordance.
- Exact behavior for returning Home from an in-progress puzzle.
- Final font choice.
- Final icon set and whether to use generated icons, a Unity icon package, or a hybrid.
- Whether Reward Popup should focus newly unlockable pictures directly or route through Home first.
