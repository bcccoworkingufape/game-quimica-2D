# ADR — Retaining the Service Locator for Cross-Scene Application-Scoped Services

| | |
|---|---|
| **Status** | Accepted for experimental State D |
| **Date** | 2026-08-26 |
| **Stage** | Etapa D — Explicit Composition |
| **Commits** | `a1a8bb3` (D1, remove unused service registrations) · `1dd343b` (D2, pass audio dependency explicitly to presenter) |
| **Baseline** | D0 `a788ea8` · State C `9b9816d` |

---

## Context

The Molz codebase creates its application-scoped services in a single place,
`Core/Bootstrap/Bootstrapper.cs`. That class is a `MonoBehaviour` present in all three
build scenes, and it behaves as follows:

- it builds one `IJsonProvider`, four repositories, and four domain/infrastructure services
  (`Bootstrapper.cs:38-50`);
- a `static bool _initialized` guard makes it run **exactly once**, in whichever scene is
  loaded first; in the other two scenes the duplicate instance destroys its own GameObject
  (`Bootstrapper.cs:22-27`);
- the surviving GameObject is marked `DontDestroyOnLoad` (`:30`), so the services it created
  live for the whole application session.

The consumers of those services, however, are not created by application code. They are
`MonoBehaviour`s authored into scenes, and Unity instantiates them when their scene loads —
which for `2_LabScene` happens **after** the composition root has already run and become
persistent. Scene presence, verified by GUID:

| Consumer | `1_MenuScene` | `2_LabScene` | `3_Win_Game` |
|---|---:|---:|---:|
| `Bootstrapper`, `GameManager` | 1 | 1 | 1 |
| `MenuUIController`, `NavbarController` | 1 | 0 | 0 |
| `LabUIController` | 0 | 1 | 1 |
| `MixingRoundController`, `QuestionFlowPresenter` | 0 | 1 | 1 |
| `HistoryPanelController` | 0 | 1 | 0 |
| `SolventClickHandler` | 0 | **8** | 0 |

Two constraints follow, and together they are the whole of this decision:

1. **There is no moment at which the composition root can reach the Lab-scene consumers.**
   The `Bootstrapper` has already executed and its duplicate in `2_LabScene` destroys itself
   before doing any work.
2. **A serialized reference cannot cross a scene boundary.** A field authored on an object in
   `2_LabScene` cannot point at the surviving `Bootstrapper`, which belongs to `1_MenuScene`.
   So the obvious escape — wiring the dependency in the Inspector — is not available either.

The `ServiceLocator` is what closes that gap. Because it is a static dictionary, the services
registered during the first scene remain reachable from objects instantiated in later scenes.

---

## Initial Hypothesis

The Decision Gate classified the component *a priori* as:

```
ServiceLocator — REDUNDANTE
```

The evidence supporting that classification was real and remains true:

- **dead registrations** — three of eight registered types (`ICompoundRepository`,
  `ISolventRepository`, `ISolutionRepository`) had zero `Resolve` call sites anywhere;
- **hidden dependencies** — consumers pull from the registry inside `Awake`/`Start` instead of
  declaring what they need, so the runtime dependency graph is not inspectable from signatures;
- **runtime resolution failure** — `Resolve<T>` logs and returns `default(T)`, so a missing
  registration surfaces later as a `NullReferenceException` far from its cause
  (`ServiceLocator.cs:53-55`);
- **no lifecycle management** — `Clear()` has zero call sites, so the static dictionary is
  never reset, including between Play sessions in the Editor;
- **spread** — 15 `Resolve`/`TryResolve` call sites across 10 production files.

The hypothesis was that explicit composition could replace all of it.

---

## Alternatives Evaluated

### 1. Full explicit composition

*Description.* Remove the registry entirely; every consumer receives its dependencies through
a constructor or an explicit `Initialize`/`Configure` call from a composition point.

*Expected benefit.* Dependencies visible in signatures; no global state; runtime graph
readable without `grep`.

*Cost.* Blocked at the root. The scene-local composition trees each need a root that holds the
services, and that root is itself a `MonoBehaviour` instantiated by Unity after the
`Bootstrapper` ran. Neither serialized references (cannot cross scenes) nor construction
(Unity owns it) can deliver the services to it.

*Parity risk.* Not reached — the alternative is not implementable without one of the others.

*Verdict.* **Rejected — not achievable within the constraints.**

### 2. `sceneLoaded` + scene scanning

*Description.* A persistent object subscribes to `SceneManager.sceneLoaded` and, on each load,
finds the consumers (`FindObjectsByType`) and pushes dependencies into them.

*Expected benefit.* Removes the static dictionary.

*Cost.* Replaces a dictionary lookup with a scene scan — an implicit mechanism traded for
another implicit mechanism. Worse, it requires `Core` to reference `Presentation`, `LabScripts`
and `UI` by concrete type, adding four layer inversions on top of the two the audit already
recorded at `GameManager.cs:122,125`. `FindAnyObjectByType` as a dependency-resolution
mechanism was itself flagged as one of the strongest smells in State C.

*Parity risk.* Moderate — injection timing relative to consumer `Awake` becomes a new
ordering concern.

*Verdict.* **Rejected — amplifies a coupling form already identified as problematic.**

### 3. Services per scene

*Description.* Drop the `_initialized` guard so each scene's `Bootstrapper` builds its own
services; wire scene-local consumers by serialized reference.

*Expected benefit.* Serialized references become viable, since everything lives in one scene.

*Cost.* Changes service lifetime from application-scoped to scene-scoped, and reloads the four
JSON files on every scene transition.

*Parity risk.* **High and disqualifying.** `InMemoryHistoryService` keeps mixture history in a
list; recreating it per scene resets state that currently survives scene loads. That is an
observable behavioral change, which the experiment's criterion `Behaviour(C) = Behaviour(D)`
forbids.

*Verdict.* **Rejected — breaks functional parity.**

### 4. `Bootstrapper` / `GameManager` scene search

*Description.* Narrower form of (2): `Bootstrapper.Awake` calls
`FindAnyObjectByType<GameManager>()` and pushes `IHistoryService` and `IAudioService` into it,
removing the two lookups `GameManager` performs today.

*Expected benefit.* Two fewer registry consumers, with no layer inversion — both classes are
in `Core`.

*Cost.* Still substitutes one implicit mechanism for another. The dependency becomes no more
visible in the signature than it was before; it merely arrives by a different implicit route.

*Parity risk.* Low.

*Verdict.* **Rejected — no clear simplification.** Explicitly excluded from scope by the
research protocol.

### 5. Partial explicit composition + retained registry

*Description.* Remove registrations that no consumer resolves; use explicit composition where
application code owns the object's construction; retain the registry only where Unity's scene
lifecycle makes a simpler alternative unavailable.

*Expected benefit.* Eliminates the ceremony that is provably useless, makes explicit the one
dependency that can be made explicit, and introduces no new mechanism.

*Cost.* The registry survives; eight `MonoBehaviour`s keep hidden dependencies.

*Parity risk.* Minimal — no behavioral path changes.

*Verdict.* **Accepted.**

---

## Decision

Adopt **partial explicit composition**:

1. **Remove registrations with no consumer.** `ICompoundRepository`, `ISolventRepository` and
   `ISolutionRepository` are no longer registered. The *registry entry* was dead; the objects
   were not. They continue to be constructed at `Bootstrapper.cs:40-42` and injected through
   the constructor of `SolubilityService` (`SolubilityService.cs:15-17`) — an explicit
   composition that already existed and was always the real delivery path.

2. **Make ownership-controlled dependencies explicit.** `MenuPresenter` is a POCO constructed
   by `MenuUIController.Awake`. Because application code creates it, its dependency can be
   supplied at construction time. Its constructor changed from `MenuPresenter(IMenuView)` to
   `MenuPresenter(IMenuView, IAudioService)` and the internal lookup was removed.

3. **Retain the registry for the rest.** The remaining consumers are `MonoBehaviour`s whose
   construction the application does not control and which, in the Lab and Win scenes, come
   into existence after the composition root has run.

No new global mechanism was introduced, no scene was modified, no service lifetime changed.

### Resulting counts

| | Before | After |
|---|---:|---:|
| `Register` | 8 | **5** |
| `Resolve` | 7 | 7 |
| `TryResolve` | 8 | **7** |
| Unique registry consumers | 10 | **9** |
| Production files referencing `ServiceLocator` | 10 | **9** |

---

## Consequences

### Positive

- Three registrations that no consumer resolved were eliminated.
- `MenuPresenter` now declares `IAudioService` in its signature instead of fetching it from a
  global.
- No replacement global registry was introduced — enforced by the
  `NoReplacementGlobalRegistryWasIntroduced` invariant.
- No scene scanning was introduced as a dependency mechanism.
- No change to service lifetime, scenes, `.asset` files, JSON datasets or packages.
- Functional parity preserved: 93/93 characterization and invariant tests, 0 compile errors.

### Negative

- `Core/ServiceLocator.cs` remains, with 14 call sites across 8 files.
- Eight `MonoBehaviour`s still acquire dependencies from a global registry rather than
  declaring them, so the runtime graph for those classes is still not readable from signatures.
- Four of them (`LabUIController`, `MenuUIController`, `NavbarController`,
  `SolventClickHandler`) resolve during `Awake`, which races with `Bootstrapper.Awake`; the
  `TryResolve`-with-fallback pattern masks the race and is only harmless because
  `UnityAudioService` is stateless.
- `Resolve<T>` still fails silently, and `Clear()` is still never called.
- The registry remains an infrastructure compromise carried into State D by context rather
  than by design preference.

---

## Research Interpretation

The *a priori* classification was **not** rewritten. It stands as recorded:

```
A-priori classification : REDUNDANT
Implementation outcome  : PARTIALLY REJECTED
Final D decision        : KEEP + SIMPLIFY
```

The structural inspection was correct about everything it observed: the dead registrations,
the hidden dependencies, the silent failure mode, the absent lifecycle management. What it
could not observe was a runtime responsibility that only became visible when removal was
attempted — **transporting application-scoped services across scene transitions to consumers
that the engine instantiates after the composition root has already executed.**

The same class is therefore simultaneously ceremony and necessary infrastructure. Its
partially ceremonial surface was removed; the part that carries a real responsibility was kept.

This is a result for **RQ1**, not a failure of the intervention: the usefulness of an
abstraction could not be determined from its structural form alone, and the engine's scene
lifecycle revealed a concrete runtime responsibility that static inspection did not expose.

Two readings this ADR does **not** support:

- it is not evidence that the Service Locator pattern is generally advisable;
- it is not evidence that the intervention failed.

The conclusion holds for the context observed in Molz: a Unity project whose services are
application-scoped, whose consumers are scene-authored `MonoBehaviour`s, and whose scenes are
loaded sequentially rather than additively.

---

## Status

**Accepted for experimental State D.**

Any reassessment belongs to a post-study stage, after State D is frozen and measured, and
would most likely require revisiting how scenes obtain their composition root — a change of a
different nature from the one this experiment set out to make.
