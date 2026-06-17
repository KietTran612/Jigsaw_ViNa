# Plan Index

This file lists detailed implementation plans under `docs/plans/`.

## Plans

- [2026-06-10-jigsaw-vietnam-kickoff-decisions.md](2026-06-10-jigsaw-vietnam-kickoff-decisions.md) - Confirmed kickoff decisions for MVP rules, puzzle UX, progression, rewards, and data direction.
- [2026-06-10-thin-vertical-slice-design.md](2026-06-10-thin-vertical-slice-design.md) - Design for Thin Vertical Slice (2-Scene VContainer DI and screen flow).
- [2026-06-10-thin-vertical-slice-implementation.md](2026-06-10-thin-vertical-slice-implementation.md) - Implementation plan for Thin Vertical Slice (2-Scene VContainer DI and screen flow).
- [2026-06-11-jigsaw-gameplay.md](2026-06-11-jigsaw-gameplay.md) - Implementation plan for Jigsaw Puzzle Gameplay (procedural generation, drag-and-drop, snapping, timer, hints, win transition).
- [2026-06-11-preview-opacity-slider.md](2026-06-11-preview-opacity-slider.md) - Add player-controlled original-image opacity with a 20% default.
- [2026-06-11-piece-shuffle-invalid-feedback.md](2026-06-11-piece-shuffle-invalid-feedback.md) - Shuffle tray pieces and add persistent invalid-drop feedback.
- [2026-06-11-extended-editor-tools.md](2026-06-11-extended-editor-tools.md) - Extend Game Data Editor to manage custom categories, global items, and save cheats with robust validation and safety policies.
- [2026-06-12-reward-item-dropdown-images.md](2026-06-12-reward-item-dropdown-images.md) - Display key item thumbnails inside difficulty reward dropdowns and show preview box in editor.
- [2026-06-12-localization-keys-and-reviews.md](2026-06-12-localization-keys-and-reviews.md) - Expose localization keys for Picture, Category, and Key Items on GUI, and address outstanding reviews.
- [2026-06-12-dynamic-home-ui-design.md](2026-06-12-dynamic-home-ui-design.md) - Design dynamic Home picture selection using reusable cards loaded from static data with explicit lifecycle cleanup.
- [2026-06-12-dynamic-home-ui-implementation.md](2026-06-12-dynamic-home-ui-implementation.md) - Implement and verify dynamic Home cards, VContainer event cleanup, scene regeneration, and targeted tests.
- [2026-06-15-dynamic-home-ui-reviews.md](2026-06-15-dynamic-home-ui-reviews.md) - Address review feedback on card horizontal clipping, manual lifecycle calls in test, layout verification, and task documentation inconsistencies.
- [2026-06-15-meta-progression-and-locking.md](2026-06-15-meta-progression-and-locking.md) - Implement Milestone 2 Picture Unlock Progression: picture unlock states, non-consumable unlock, sequential difficulties and missing item hints.
- [2026-06-15-daily-drop-and-inventory.md](2026-06-15-daily-drop-and-inventory.md) - Implement Milestone 3 Daily Drop Decay: drop tables serialization preservation, PlayerSave migration, IDropRewardService TDD & implementation, and Collection/Inventory UI.
- [2026-06-15-game-test-case-system-design.md](2026-06-15-game-test-case-system-design.md) - Design a Markdown-first QA test case system with validated, manually requested Excel export and later NUnit mapping.
- [2026-06-15-game-test-case-system-implementation.md](2026-06-15-game-test-case-system-implementation.md) - Implement the Living Test Plan, 31 initial Smoke/Critical cases, validated Markdown parser, and manually invoked Excel exporter.
- [2026-06-16-daily-login-reward-system.md](2026-06-16-daily-login-reward-system.md) - Implement Task 47 Daily Login Reward System: configure daily rewards in static data, track login streak in PlayerSave, implement DailyRewardService, and add Popup UI to Home screen.
- [2026-06-16-ui-ux-design-system-and-screen-spec.md](2026-06-16-ui-ux-design-system-and-screen-spec.md) - Define the mobile landscape UI/UX direction, mini design system, DOTween animation rules, screen specs, and AI asset generation briefs for the full game flow.
- [2026-06-16-ui-ux-ai-image-brief.md](2026-06-16-ui-ux-ai-image-brief.md) - Companion brief for AI image assistants to interpret the UI/UX design system and produce suitable mood boards, asset plans, and layered Unity-friendly image outputs.
- [2026-06-16-long-csharp-files-refactor.md](2026-06-16-long-csharp-files-refactor.md) - Refactor the longest project-owned C# files with focused helper extraction while excluding third-party plugins and line-count-only test churn.
- [2026-06-17-map-planned-test-cases.md](2026-06-17-map-planned-test-cases.md) - Map the 31 approved planned test cases defined in docs/test-cases/ modules to NUnit tests and update their automation statuses.
- [2026-06-17-game-settings-audio-localization.md](2026-06-17-game-settings-audio-localization.md) - Implement game settings popup, pause system, audio manager service (BGM/SFX), and runtime localization service.
