# Changelog

### Changed

- A tweak that runs into an error now switches itself off until the game restarts instead of failing every frame

### Fixed

- Food and pipes in shops no longer keep smoking until you look at them when they appeared at night

## [1.0.0] - 2026-10-24

### Initial release

- Skipping of item and crate updates that would change nothing
- Skipping of crate content updates while the crate doesn't move
- Freezing of idle shop goods until they are looked at, picked up or sold
- Food descriptions refreshed only while the food is looked at or held
- Ropes rebuilt only when they move, with stowed mooring lines hanging still while the boat rocks
- Simplified distant ropes with Quality, Default and Performance presets
- Pausing of ropes out of sight
- Cheaper wind shadow checks between sails
- Dirt and outline cameras that render only when needed

[1.0.0]: https://github.com/KeeviDev/SailwindPerformanceTweaks/releases/tag/v1.0.0
