<p align="left"><img src="https://github.com/user-attachments/assets/126db070-6cf7-4720-98ab-182dc2c077a1" width="192" alt="Performance Tweaks logo"></p>

# Performance Tweaks

**Performance Tweaks** optimizes Sailwind and makes it run smoother, most of all in busy ports with your boats moored and shops full of goods. The game updates a lot of things every frame that haven't changed or that nobody is looking at: items lying still, ropes on a moored boat, shop goods on their stalls, cameras that draw nothing. This mod skips that work and picks it up again the moment it matters. Ropes are handled smarter too: distant ones are drawn simplified and the ones out of sight update only a few times a second.

**Tip:** if you want better performance, don't set **Streetlight quality** to high in the game settings. Medium still looks nice and barely affects performance.

## Where you'll notice it

Measured in a capital with several boats moored and a full mod list; your numbers depend on your PC and your mods:

| Tweak | Saves per frame, up to | Helps most |
|---|---|---|
| Skip redundant item updates | ~2 ms | Lots of cargo on your boats |
| Skip unmoved crate contents | ~0.5 ms | Many full crates |
| Freeze idle shop items | ~2.7 ms | Capitals and big markets in daytime, more with mods that add to items |
| Food descriptions on demand | ~0.35 ms and less stutter | Lots of food around |
| Skip unchanged ropes | ~2.5 ms | Several boats nearby and big ships with many ropes |
| Simplify distant ropes | ~1.5 ms | Ports full of boats and big ships with many ropes |
| Pause hidden ropes | ~0.8 ms | Most ropes out of sight |
| Skip sync for sail wind shadows | ~0.7 ms | Under sail, and with boats under sail nearby |
| Dirt camera on demand | ~1 ms | Everywhere |
| Outline cameras on demand | ~0.8 ms | Everywhere |

Together that's several milliseconds in a busy port: at 30 FPS each millisecond saved gives roughly one more frame per second, at 60 FPS roughly four.

## Installation

**With a mod manager:** grab Performance Tweaks from Thunderstore with r2modman or Thunderstore Mod Manager — BepInEx comes along automatically.

**By hand:**

1. Install [BepInEx 5](https://github.com/BepInEx/BepInEx/releases).
2. Extract the release zip into `Sailwind/BepInEx/plugins`.
3. Set sail.

## Settings

Every tweak is on by default and can be switched off on its own. Change them in `BepInEx/config/com.keevi.performancetweaks.cfg`, or in-game with [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager).

| Section | Setting | Default | What it does |
|---|---|---|---|
| Items | Skip redundant item updates | On | Skips item and crate updates that would change nothing. |
| Items | Skip unmoved crate contents | On | Puts the items inside a crate back into place only after the crate has moved or its contents changed. |
| Items | Freeze idle shop items | On | Pauses the updates of shop goods while nobody holds or looks at them. |
| Items | Food descriptions on demand | On | Updates a food's description only while you look at it or hold it. |
| Boats | Skip unchanged ropes | On | Rebuilds a 3D rope only when it has moved on its boat. |
| Boats | Simplify distant ropes | Default | Draws distant ropes as the game's flat ropes: Quality beyond 50 m, Default beyond 30 m, Performance beyond 15 m, or Off. |
| Boats | Rope distance measured from | Camera | Where the distance for Simplify distant ropes is measured from: the camera or your character. |
| Boats | Pause hidden ropes | On | Updates ropes you can't see only a few times a second. |
| Boats | Skip sync for sail wind shadows | On | Checks the wind shadow between sails against positions from the last physics step. |
| Rendering | Dirt camera on demand | On | Turns the hidden dirt camera on only when it is really needed. |
| Rendering | Outline cameras on demand | On | Renders the highlight outline cameras only while something is highlighted. |

## For mod makers

**Freeze idle shop items** switches off the game's own components of a shop item (`ShipItem`, `ItemRigidbody`, its collision checker and food components) while it sits on a stall, so anything those run every frame — including your Harmony patches on them — pauses until the item is looked at, picked up or sold. Your own components on the item keep running.

If your item should keep moving on the stall, for example a needle or a clock hand driven from your `ShipItem` subclass, add an empty component named `KeepUpdatingInShop` to its prefab or template:

```csharp
public class KeepUpdatingInShop : MonoBehaviour { }
```

Any namespace works, and your mod doesn't need to reference Performance Tweaks. The component can sit on the item or any of its children.
