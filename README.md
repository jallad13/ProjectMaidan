# PROJECT MAIDAN — Unity Project

**Product Owner:** Ahmad Jallad  
**Game Design Document:** [GDD-v1.0.md](../GDD-v1.0.md)  
**Technical Architecture:** [technical-architecture-v1.0.md](../technical-architecture-v1.0.md)  
**Project Management:** https://jallad.youtrack.cloud (project: MAI)  
**Current Phase:** Phase 2 — Prototype

---

## Unity Version

This project is pinned to **Unity 6000.3.18f1** (Unity 6.3 LTS).
Use this exact editor version so package resolution and serialized project settings remain consistent across the team.

---

## How to Open This Project

This scaffold was pre-built before Unity initialization. To open it:

1. Open **Unity Hub**
2. Click **Open** → **Add project from disk**
3. Select this folder (`ProjectMaidan/`)
4. Unity will auto-generate `Library/`, `ProjectSettings/`, and `Temp/` on first open — this is normal
5. Resolve any Package Manager errors (see packages section below)

Do NOT use "New Project" — that creates a new empty folder. Use "Open" on this existing folder.

---

## Package Setup (Required on First Open)

The `Packages/manifest.json` includes packages available via Unity Package Manager.  
**Current prototype status:** Unity Package Manager dependencies are resolved. Photon Fusion, PlayFab, and Firebase remain intentionally unimported until Phase 3, matching the prototype scope below.
The following require **manual installation** — Package Manager cannot fetch them automatically:

### 1. Photon Fusion
- Download from: https://dashboard.photonengine.com (Photon Fusion SDK)
- Create a Fusion app, copy your **App ID**
- Import the `.unitypackage` into Unity
- **PROTOTYPE NOTE:** Do NOT enter the App ID yet. Leave config as placeholder. Networking is out of scope for prototype.

### 2. PlayFab Unity SDK
- Download the official `UnitySDK.unitypackage` from the PlayFab Unity SDK releases: https://github.com/PlayFab/UnitySDK/releases
- Import it via **Assets → Import Package → Custom Package**
- **PROTOTYPE NOTE:** Do NOT configure Title ID or Secret Key yet. PlayFab is out of scope for prototype.

### 3. Firebase Unity SDK
- Download from: https://firebase.google.com/docs/unity/setup
- Import `FirebaseAnalytics.unitypackage` and `FirebaseCrashlytics.unitypackage` only
- **PROTOTYPE NOTE:** Do NOT add `google-services.json` or `GoogleService-Info.plist` yet. Firebase is out of scope for prototype.

---

## Folder Structure

```
Assets/_Project/
├── Scripts/
│   ├── Core/           ← Match state, territory, mana, AI, deployment zones
│   ├── Units/          ← UnitBase, unit types, UnitManager, UnitAI utilities
│   ├── Networking/     ← Photon Fusion (Phase 3 — stubs only in prototype)
│   ├── Backend/        ← PlayFab (Phase 3 — stubs only in prototype)
│   ├── UI/             ← HUD, Menus, Tutorial, Progression
│   ├── Audio/          ← Audio manager (Phase 3+)
│   └── Utilities/      ← General helpers, extensions
├── Prefabs/
│   ├── Units/          ← Placeholder unit prefabs (colored circles + HP bar)
│   ├── UI/             ← HUD, cards, result screen prefabs
│   └── Effects/        ← Attack/heal VFX
├── ScriptableObjects/
│   ├── UnitDefinitions/ ← One UnitDefinition.asset per unit (Ifrit, Marid, Roc)
│   ├── GameConfig/     ← Global config (create GameConfig.asset here)
│   └── MatchConfig/    ← Match tuning (create MatchConfig.asset here — used by all systems)
├── Scenes/
│   ├── Bootstrap.unity  ← Entry point, loads persistent managers, transitions to MainMenu
│   ├── MainMenu.unity   ← Start button → Match scene
│   ├── Match.unity      ← All gameplay happens here
│   └── Tutorial/        ← Tutorial match scenes (Phase 3+)
└── AddressableAssets/  ← Future: unit art, battlefield assets, audio (Phase 3+)
```

---

## Scene Creation Checklist (Required in Unity Editor)

These scenes must be created via **File → New Scene** in Unity Editor. They cannot be pre-created as text files reliably:

- [ ] `Assets/_Project/Scenes/Bootstrap.unity` — Empty scene, add GameManager prefab, set as Scene 0 in Build Settings
- [ ] `Assets/_Project/Scenes/MainMenu.unity` — Simple Start button UI
- [ ] `Assets/_Project/Scenes/Match.unity` — All gameplay (battlefield, HUD, zones, units)

**Build Settings order:** Bootstrap (0) → MainMenu (1) → Match (2)

---

## ScriptableObject Assets to Create (in Unity Editor)

After opening the project, create these assets via right-click in Project window:

- [ ] `Assets/_Project/ScriptableObjects/MatchConfig/MatchConfig.asset` (ProjectMaidan → Match Config)
- [ ] `Assets/_Project/ScriptableObjects/UnitDefinitions/Ifrit.asset` (ProjectMaidan → Unit Definition)
- [ ] `Assets/_Project/ScriptableObjects/UnitDefinitions/Marid.asset`
- [ ] `Assets/_Project/ScriptableObjects/UnitDefinitions/Roc.asset`

Default stats for prototype (to be tuned during testing):

| Unit | Role | HP | Damage | Speed | AttackRange | AttackRate | ManaCost |
|---|---|---|---|---|---|---|---|
| Ifrit | Frontline | 120 | 12 | 3.5 | 2.0 | 1.0/s | 3 |
| Marid | Support | 80 | 0 | 2.5 | 5.0 | — | 3 |
| Roc | Ranged | 60 | 8 | 2.0 | 8.0 | 0.7/s | 4 |

Marid HealAmount = 15, HealRange = 5.0, HealRate = 1/s

---

## Build Targets

Configure via **File → Build Settings**:

- **Android:** Backend = IL2CPP, Minimum API = 24, Target API = latest
- **iOS:** Backend = IL2CPP, Minimum iOS = 14.0

For prototype, Android local build is sufficient. iOS not required until Phase 3.

---

## Git Workflow

- `main` — protected, requires PR
- `develop` — active development baseline
- `feature/epic-XX-name` — one branch per epic

Never commit to `main` directly.  
Never commit credential files (`google-services.json`, `photon-config.json`, `playfab-config.json`).

---

## Current Prototype Scope (Phase 2)

See **YouTrack project MAI** for all user stories (MAI-5 through MAI-37).

**Build:** Epic 1 → Epic 2 → Epic 3 → Epic 4 → Epic 5 → Epic 6 → Epic 7  
**Definition of Done:** Ahmad and Razan can play a complete 2-minute match vs AI without crashing.

**Out of scope for prototype:** Photon networking, PlayFab, Firebase, real art, audio, progression, deck building, tutorial.

---

## Pre-written Scripts

The following C# files are pre-written as stubs. Read the TODO comments to understand what to implement:

| File | Stories | Status |
|---|---|---|
| `Core/GameManager.cs` | — | Singleton shell — wire manager refs in Inspector |
| `Core/MatchManager.cs` | MAI-31, MAI-34 | State machine shell + MatchResult class |
| `Core/TerritorySystem.cs` | MAI-25, MAI-26, MAI-27 | Events defined, logic TODO |
| `Core/ManaSystem.cs` | MAI-29, MAI-30 | TryDeductMana done, regen TODO |
| `Core/MatchConfig.cs` | — | All tunable values — create asset in Editor |
| `Core/DeploymentZone.cs` | MAI-17 | ZoneId enum + tap event defined |
| `Core/AIController.cs` | MAI-36, MAI-37 | Decision loop coroutine shell |
| `Units/UnitDefinition.cs` | MAI-20 | Full SO definition — create Ifrit/Marid/Roc assets |
| `Units/UnitBase.cs` | MAI-20, MAI-21 | TakeDamage/Heal/Die done, HP bar TODO |
| `Units/UnitManager.cs` | — | Registry — Register/Unregister/CanDeploy done |
| `Units/UnitAI.cs` | — | FindBestTarget/FindLowestHPAlly helpers |
| `Units/FrontlineUnit.cs` | MAI-22 | Logic TODO — read comments |
| `Units/SupportUnit.cs` | MAI-23 | Logic TODO — read comments |
| `Units/RangedUnit.cs` | MAI-24 | Logic TODO — read comments |
| `Networking/*.cs` | Phase 3 | Stubs only — do not implement in prototype |
| `Backend/*.cs` | Phase 3 | Stubs only — do not implement in prototype |

---

*This scaffold was pre-built by the project consultant (Claude). Questions → Ahmad Jallad.*
