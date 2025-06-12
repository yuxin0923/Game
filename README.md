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

## 🚀 Extending the Game

| Task | How-to |
|------|--------|
| **Add enemy behaviour** | `class ShootStrategy : IEnemyStrategy` → return in `StrategyFactory.Create()` |
| **New player action** | `class DashCommand` → instantiate in `InputHandler` → bind key/axis |
| **Extra HUD layer** | Add Canvas prefab → expose `ShowMyCanvas()` in **UIManager** |
| **Listen to key pickup** | `GameEvents.OnKeyCollected += MyMethod;` |
| **Replace physics** | Implement new solver inside **Physic**; gameplay code unchanged |

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
