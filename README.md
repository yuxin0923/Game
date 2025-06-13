Game Programming
# 🔦 2-D Platformer Framework

> **A three-layer, seven-package Unity project designed for _clarity, reuse,_ and _rapid feature growth_.**  
> Hand this repo to another dev and they can ship a new mechanic without spelunking through spaghetti code.

---

## 🏛 High-Level Architecture


| Layer | Packages | Main Design Patterns |
|-------|----------|----------------------|
| **Infrastructure** | **GameCore** (Singleton, FSM, Event-Bus) <br> **Physic** (Singleton “Service”, Strategy-friendly) | • Singleton <br>• Observer <br>• Service |
| **Gameplay** | **InputSystem** (Command) <br> **Player** (Facade, Composition) <br> **AIEnemy** (State + Strategy + Factory) <br> **World** (Event-driven props) | • Command <br>• Strategy <br>• Factory Method <br>• Finite-State-Machine |
| **Presentation** | **UI** (Mediator/Facade) | • Facade <br>• Observer |

---

## 📂 Package Overview

| Folder | Responsibility (Single purpose) |
|--------|----------------------------------|
| `AIEnemy` | Enemy FSM, vision (Bresenham LOS), **Strategy** behaviours (*Patrol, Chase, Attack, Dead*) |
| `GameCore` | `GameManager` (scene/state), `GameEvents` (global bus), `SceneLoader` |
| `InputSystem` | `ICommand`, concrete commands, `InputHandler` glue |
| `Physic` | Custom `PhysicsEngine`, `CollisionDetector`, `PhysicsMaterial` |
| `Player` | Player façade + `FlashLight` mechanic |
| `UI` | `UIManager` (singleton canvas switch-board) & overlay scripts |
| `World` | Scene objects (`Door`, `Key`, `Torch`, `MovablePlatform`, `DeathWall`) |

**Rule of thumb**: _Packages never reach into each other except via_ **public interfaces** _or_ **GameEvents**.

---

## 🧩 Why This Design Rocks

| Concern | Solution | Benefit |
|---------|----------|---------|
| _Tight coupling_ | Event-Bus (Observer) + Interfaces | Swap a system without recompiling the project |
| _God classes_ | Seven **self-contained** packages | New devs grok each feature in isolation |
| _Input sprawl_ | **Command Pattern** | Binding a new key = create a command, call `Execute()` |
| _Rigid AI_ | **Strategy + Factory** per state | Drop in `HideStrategy`, register in factory, done |
| _Fork-prone UI_ | **UIManager** Facade | Scenes stay UI-agnostic—just call `ShowHUD()` |
| _Physics ambiguity_ | Custom `PhysicsEngine` Service | Deterministic order, no Unity-physics quirks |

---
## 🔧 Extending & Maintaining

Below is a **cook-book style** guide for common feature requests and upkeep
tasks.  Everything is grouped by subsystem so you can jump straight to the
package you care about.

| Area <br>(Package) |  Typical Task  | Where / How   | Why it stays isolated |
|--------------------|---------------|---------------|------------------------|
| **Physics** <br>(`Physic`) | **Add slopes / one-way platforms** | *CollisionDetector* → add a new overlap rule, *PhysicsEngine.Tick* → resolve before step 3 | Engines owns *all* contacts; bodies remain oblivious |
| | **Create an “ice” or “mud” surface** | 1) `Assets/Create/Physic/Physics Material` ↗ <br>2) Set `friction` & drop onto a Tilemap layer in **TilemapWorld** | Material tables decouple surface feel from the solver |
| | **Swap the entire solver** | Write `MyPhysicsEngine : MonoBehaviour`, duplicate registration API; disable the old engine in the scene | Bodies depend only on the **Service interface**, not the impl. |
| | **Debug collisions** | Toggle _Gizmos_ on the `PhysicsEngine`, `SimplePhysicsBody` & `TilemapWorld` inspectors | Built-in wireframes reveal penetration / grounding errors |
| **AI Enemy** <br>(`AIEnemy`) | **New behaviour** e.g., *Shoot* | `class ShootStrategy : IEnemyStrategy` → return it in `StrategyFactory.Create()` | Strategy Pattern means zero edits to *AIEnemyManager* |
| | **Tune vision** | Adjust `sightRadius` / `fov` in the prefab; LOS is Bresenham, no physics raycast cost | Data-driven; no code touch |
| **Player** <br>(`Player`, `InputSystem`) | **Add Dash** | 1) `class DashCommand : ICommand` <br>2) Instantiate in `InputHandler.Awake()` <br>3) Bind key/axis in Unity Input | Command Pattern cleanly separates input from action |
| | **Refactor abilities** | Everything funnels through the Player façade (`Move()`, `Jump()`, etc.)—extend it, not the physics body | Keeps low-level motion code untouched |
| **UI** <br>(`UI`) | **Extra overlay / HUD widget** | 1) Make Canvas prefab <br>2) Reference in `UIManager` inspector <br>3) Expose `ShowMyOverlay()` | UIManager is a *Facade/Mediator*; scenes remain UI-free |
| | **Localise text** | Swap TMP assets; canvases only contain TMPro; scripts expose plain strings | Presentation layer alone changes—logic untouched |
| **Game Flow** <br>(`GameCore`) | **Add new game state** (e.g., *Shop*) | 1) Add enum to `GameState` <br>2) Handle in `GameManager.ChangeState` switch <br>3) Add matching UI canvas | FSM centralises flow; no other system needs edits |
| **World Objects** <br>(`World`) | **New interactive prop** | 1) Derive MonoBehaviour (e.g., `Spring`) <br>2) Publish/subscribe via `GameEvents` | Event-Bus removes direct dependencies |

### ♻️  Maintenance Principles

| Principle | Concrete Example |
|-----------|------------------|
| **Single Responsibility** | `SimplePhysicsBody` only integrates motion & tile queries—no input, no AI. |
| **Open / Closed** | You can add `PhysicsMaterial` types or `IEnemyStrategy` without altering existing code. |
| **Dependency Inversion** | High-level systems (input, AI) call *interfaces* (`ICommand`, `IEnemyStrategy`), never concrete classes. |
| **Data-Driven Tuning** | Most numbers live in the Inspector or ScriptableObjects, enabling designers to iterate without a compile. |
| **Editor-time Validation** | Duplicate singleton instances (`TilemapWorld`, `PhysicsEngine`) self-destruct with a warning. |

Stick to these rules and the project will stay *approachable* for new
contributors and *adaptable* for fresh gameplay ideas.


---

## 🛠 Getting Started

1. **Unity 2021.3 LTS** or later  
2. `git clone` → open → press **▶️**  
3. Launch *LevelSelect* scene to test every level instantly.

> The project ships with the **legacy Input Manager** for brevity.  
> Migrating to the new Input System only touches the `InputSystem` package.

---

## 🤝 Contributing Guidelines

* Respect package boundaries—communicate via interfaces or `GameEvents`.
* Prefer **composition** (add components) to inheritance.
* Keep method names _intent-based_: `Jump()`, `StartRecharge()`, never `SetVelocityY()`.
* Submit PRs with isolated, well-named commits.

---

## 📄 License

MIT. See [LICENSE](LICENSE) for the full text.

Happy hacking! 🎮
