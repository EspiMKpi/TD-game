# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

2D Tower Defense game ("Quantall") built in Unity (C#, URP 2D). `KeHoach_LapTrinh_Quantall_Unity.md` is the authoritative design doc: it defines the target OOP architecture as a series of phases ("Giai đoạn N") and use cases ("UCn"). Most of that architecture is now implemented (through Giai đoạn 7 — SaveSystem/level loading). Source comments are in Vietnamese and reference these phase/UC numbers — when a comment says `UC6` or `Giai đoạn 7`, cross-reference the design doc for intent.

## Unity Workflow

There is no CLI build. Compilation and tests run inside the Unity Editor (2022 LTS / URP). Two ways to drive it:

- **Editor directly:** Unity auto-compiles on save; watch the Console. Tests run from Window → General → Test Runner.
- **Unity MCP bridge** (`ai-game-developer` MCP server, configured in `.mcp.json`, talks to a running Editor on `localhost:26604`): exposes tools/skills to compile, run tests, read the console, manipulate scenes/GameObjects/prefabs, and execute C# in the live Editor. Use `tests-run` to run tests, `console-get-logs` to read compile/runtime errors, `assets-refresh` to force a recompile after editing `.cs` files on disk. **Precondition for `tests-run`: all open scenes must be saved** (dirty scenes abort the run).

### Tests

Tests live in `Assets/Tests/` split into two assemblies that reference the game assembly `Quantall.Runtime`:

- `Assets/Tests/EditMode/` (`Quantall.Tests.EditMode`) — fast NUnit logic tests (economy, data layer, enemy, power-up, save). EditMode does **not** run `Awake`/`Start`, so tests wire singletons by hand (e.g. `Level_Manager.main = lm; lm.currency = 100;`) and rely on field initializers. Mirror this pattern when adding EditMode tests.
- `Assets/Tests/PlayMode/` (`Quantall.Tests.PlayMode`) — lifecycle tests that need the real game loop.

Run a single test via Test Runner (filter by class/method) or the MCP `tests-run` tool, which supports filtering by assembly/namespace/class/method.

### Assembly layout

All game code is in `Assets/Code/GameScripts/` under one assembly definition, `Quantall.Runtime.asmdef` (references `Unity.TextMeshPro`, `UnityEngine.UI`). It is **not** the default `Assembly-CSharp` — new game scripts placed here are picked up automatically, but anything that must see this code (tests, editor tools) needs an explicit asmdef reference to `Quantall.Runtime`. Keep runtime code free of `UnityEditor` dependencies so the assembly stays clean (see TurretScript note below).

## Code Architecture

### Coordinator-over-legacy-singletons pattern (most important)

The codebase is mid-migration from a flat set of `static main` singletons toward a central `GameSession` coordinator (design Giai đoạn 2). Rather than a big-bang rewrite, **`GameSession` wraps the legacy singletons and delegates to them**, so both call styles coexist:

| Class | Access | Role |
|---|---|---|
| `GameSession` | `GameSession.Instance` | Central coordinator (UC4–UC11): build/upgrade/sell towers, power-ups, pause, win/lose status. `[DefaultExecutionOrder(100)]` so it initializes **after** the order-0 managers and can override their defaults via `ApplyLevel`. |
| `Level_Manager` | `Level_Manager.main` | **Still the source of truth for currency** + holds enemy path (`startPoint` + `path[]`). `GameSession.currentResources` and the spend/add helpers just delegate here. |
| `EnemySpawner` | `EnemySpawner.main` | Wave spawning/scaling; fires the static `onEnemyDestroy` UnityEvent. Calls back into `GameSession` (`SetCurrentWaveIndex`, `OnAllWavesCleared`). |
| `BuildManager` | `BuildManager.main` | Tower shop: array of `Tower` data objects + selected index. |
| `Base` | `Base.main` | Player base HP (UC11). `takeDamage` → `onBaseDestroyed` → `GameSession.OnBaseDestroyed()` sets `GameStatus.Lost`. |

**Null-guard fallback is pervasive and intentional:** systems route through `GameSession.Instance` *if present*, otherwise fall back to the legacy path so a gameplay scene without a `GameSession` still runs. See `Plot.OnMouseDown` (GameSession.buildTower vs. direct Level_Manager spend) and `Health.TakeDamage` (adds score to GameSession only if non-null). Preserve this pattern — do not assume `GameSession.Instance`/`Base.main` exist.

### Scene flow

`Boot.unity` → `MainMenu.unity` → gameplay (`Gameplay.unity`) — all three under `Assets/GameScenes/` and registered in Build Settings (indices 0/1/2) — driven by `GameFlow` (static helper). Scene transitions load **by name**, so each name must match its Build Settings entry: `Boot.mainMenuScene` = "MainMenu", `LevelDetailView.gameplaySceneName` = "Gameplay", `ResultView.menuSceneName` = "MainMenu". `MainMenuView`/`LevelSelectView` set `GameFlow.SelectedLevel` (a `Level` ScriptableObject) before loading the gameplay scene; `GameSession.Start` reads it via `ApplyLevel` to set currency, base HP, and total wave count. If no level was selected (scene played directly), defaults are used.

> **Rename note (2026-06-10, verified ✓):** the gameplay scene was moved from `Assets/_Recovery/0 (1).unity` → `Assets/GameScenes/Gameplay.unity`. Build Settings index 2 and `LevelDetailView.gameplaySceneName` = "Gameplay" were updated; the move preserved the scene GUID, so all in-scene wiring (`GameSession`→`Base`, `Level_Manager` path, `EnemySpawner` prefabs, `GameplayView`/`ResultView`) stayed intact — re-verified via MCP with no broken references. `SampleScene.unity` is a leftover and is **not** part of the shipped flow.

### Gameplay loop

- **Enemy:** `EnemySpawner` instantiates at `Level_Manager.main.startPoint`; `EnemyMovement` follows `path[]`. Reaching the end calls `Base.main.takeDamage`, fires `onEnemyDestroy`, and self-destructs. `EnemyMovement` also supports `PushBack(steps)` (Portal power-up) and `ApplySlow` (Slow tower/bullets).
- **Tower:** `Plot` (per tile) → `GameSession.buildTower(towerData, pos)` checks `currentResources`, spends, instantiates the prefab. `TurretScript` finds enemies via `Physics2D` on `enemyMask`, rotates, fires `Bullet`s. Tower behavior keys off `TowerType` (Single/Multi/Explosive/Slow) — `Multi` fires at up to `maxTargets`; the rest fire one bullet whose effect is decided by the bullet's `ProjectileType`. Towers support `upgrade()` (UC6) and `BoostFireRate` (SpeedBoost power-up).
- **Bullet:** homing — holds a `Transform target`, homes each `FixedUpdate`, self-destructs if the target dies. Collision only counts if the hit collider belongs to the tracked target (`IsTargetCollider` checks `.transform`, `.attachedRigidbody.transform`, `.transform.root`). `Explosive` bullets do an `OverlapCircleAll` AoE on impact; `Slow` bullets call `EnemyMovement.ApplySlow`.
- **Economy/score:** `Health.TakeDamage` on enemy death → `Level_Manager.main.IncreaseCurrency` **and** `GameSession.AddScore`.
- **Power-ups (UC9):** `PowerUp` is a ScriptableObject holding only data + cooldown; the spatial effect runs in `GameSession.activatePowerUp` (it needs scene `Physics2D` access) — `Portal` (push enemies back), `Airstrike` (AoE damage), `SpeedBoost` (buff towers in radius).
- **Win/lose:** last wave cleared → `EnemySpawner` → `GameSession.OnAllWavesCleared` (computes stars from remaining base HP, `GameStatus.Won`); base destroyed → `GameStatus.Lost`.

### Data & persistence

- ScriptableObjects (created via `Assets/Create → Quantall/...`): `Level` (waves, resources, base HP, unlock/best-score progress), `PowerUp`, `Settings`. `Wave` is a plain `[Serializable]` class nested in `Level`. `Tower` is a plain `[Serializable]` data container in `BuildManager` (not a MonoBehaviour).
- `SaveSystem` (static) persists `Level` progress and `Settings` via `PlayerPrefs` with key prefixes (`lvl_<id>_*`, `set_*`). `Settings` are loaded + applied in `Boot.Start` (`Settings.Apply` → `AudioListener`/`QualitySettings`/`Screen`) and saved by `SettingsView`/`Player.exit`. `Level.loadLevel(id)` loads by id from `Resources/Levels/` (loadable level assets live there — e.g. `Level_01`); winning calls `Level.unlockNext()` (set `nextLevel` in the asset). Only `Player.checkClick` remains a stub (search `TODO Giai đoạn`).

### Enums

Design "type: String/boolean" fields were replaced with enums (`Assets/Code/GameScripts/Enums/`): `TowerType`, `ProjectileType`, `PowerUpType`, `EnemyType`, `GameStatus` (Playing/Won/Lost replaces the original boolean `status`).

### Naming conventions

- Design-doc method names are kept in lowerCamelCase (`takeDamage`, `buildTower`, `upgradeTower`, `activate`, `saveResult`) even though Unity lifecycle methods are PascalCase — match the surrounding style of the class you edit.
- **Never name a field `name` on a `MonoBehaviour`/`Object` subclass** — it shadows `UnityEngine.Object.name`. Use `towerName`, `enemyName`, `powerUpName`, `playerName`, etc. (the design doc's `name` fields are renamed accordingly).
- `TurretScript.OnDrawGizmosSelected` uses `Gizmos` (UnityEngine), **not** `Handles`/`UnityEditor`, specifically so the runtime assembly carries no editor dependency. Keep editor-only API out of `Quantall.Runtime`; if unavoidable, wrap in `#if UNITY_EDITOR`.
- `Plot` hover/click uses the **legacy** input (`OnMouseEnter/Exit/Down`). If the project moves fully to the new Input System, these break and must be replaced.

## Giai đoạn 7–8 status (updated 2026-06-10)

GĐ7 (save/progress) and the **code** side of GĐ8 are implemented; verified by 35 EditMode + 7 PlayMode tests (all green). Key pieces added beyond the base architecture:

- **Settings asset:** `Assets/ScriptableObjects/Settings/GameSettings.asset` is the shared `Settings`, wired into `Boot`, `SettingsView`, `Player`. `Settings.Apply()` applies music volume via `AudioListener.volume` (master), `graphicsQuality`→`QualitySettings`, `displayMode`→`Screen.fullScreenMode`.
- **In-game tower actions (UC6/UC7):** clicking an occupied `Plot` opens `TowerActionView` (`Canvas/TowerAction`) → `GameSession.upgradeTower/sellTower`; `Plot.ClearTower()` frees the tile on sell.
- **Power-ups (UC9):** `PowerUpView` (`Canvas/PowerUps`) shows one slot per `GameSession.powerUps[i]`; arm a slot, then click the map to target (right-click cancels). Slots auto-hide when there is no power-up data.
- **Wave-driven spawning:** `EnemySpawner` reads the selected `Level.waves` (per-wave `enemyCount`/`spawnRate`/`enemyPrefab` → multiple enemy types) via `SetLevel`, falling back to its procedural formula when a wave index is unconfigured. `Wave.difficultyMultiplier` scales spawned enemy HP/damage (`Health.ApplyDifficulty`, `EnemyMovement.ApplyDifficulty`).
- **Object pooling:** `SimplePool` (+ `PooledObject` marker, both in `Core/`) recycles enemies and bullets. `EnemySpawner`/`TurretScript` spawn via `SimplePool.Get`; `Health`/`EnemyMovement`/`Bullet` call `SimplePool.Release` instead of `Destroy` and **reset their state in `OnEnable`** — when adding fields to a pooled type, reset them in `OnEnable` too.

### Content still needed (art/data — NOT code)

GĐ8 *polish* is intentionally **not** done: it needs assets the project doesn't have. When authoring these, reproduce the wiring with MCP/`script-execute` and remember to load an asset **after** `OpenScene` (Unity unloads unreferenced assets on scene open, turning an earlier-loaded reference into a fake-null).

- **VFX/SFX:** no particle/audio assets exist — shoot/explosion/death effects and sound are unimplemented.
- **AudioMixer:** `Settings.Apply` only sets a master volume; separate music/SFX buses need an `AudioMixer` asset (then route `musicVolume`/`sfxVolume` to its exposed params).
- **Power-up data (UC9):** create `PowerUp` assets (Portal/Airstrike/SpeedBoost) and assign them to `GameSession.powerUps` — UI/logic are ready but inert without data.
- **Wave data:** `Level_01` has 0 configured waves (spawning uses the formula fallback); author `Level.waves` for designed encounters and multiple enemy types.
- **More levels:** only `Level_01` exists. Add more level assets (under `Resources/Levels/`, set each `nextLevel`) so unlock/progression is visible.
