# State D Freeze — Molz, Paper 1

**Status:** FROZEN · **Date:** 2026-09-03

This document records the frozen production snapshots of the controlled architectural
refactoring experiment, together with the environment and the measurement instrument used
to evaluate them. From this point on, `Assets/Scripts` must not change before the formal
measurement of C × D0 × D.

---

## Production snapshots

| State | Commit | Tag | Role |
|---|---|---|---|
| **C** | `9b9816d319f5e60935edcb92dccb1e10e7506c37` | `analise-estatica/versao-C` | Experimental baseline |
| **D0** | `a788ea8927d4c20afeba7d3b2e3e536d362025d2` | — | Dead-code cleanup checkpoint (confounder control) |
| **D** | `934eea6aa8e732db57f4575379ba1acd1dd6b58f` | `analise-estatica/versao-D` | Architectural simplification |

The D tag is annotated; its tag object is `da578ac977791a547292d99bef866fec70fe0803` and it
dereferences to `934eea6aa8e732db57f4575379ba1acd1dd6b58f`.

### Reading the three states

```
C  → D0    effect of removing dead code          (control)
D0 → D     effect of the architectural intervention  (principal comparison)
C  → D     total observed transformation
```

`C → D0` is a control, not an architectural result: the components removed in Stage A had no
consumers and belong exclusively to the `DEAD CODE REMOVED` category.

---

## Intervention chain

| Stage | Intervention | Outcome |
|---|---|---|
| **A** | Dead-code removal — 11 files, 20 types | → **D0** |
| **B** | Data layer: `QuestionDto` and the four mappers removed; transformation localized in the repositories | B1 `d3d949c`, B2 `2ec5f5f` |
| **C** | Audio boundary adopted — 56 concrete accesses → 0 | C1 `3b88c1e`, C2 `8d93dc8`, C3 `cad84f7` |
| **D** | Partial explicit composition — 3 dead registrations removed, `MenuPresenter` composed by constructor; registry deliberately retained | D1 `a1a8bb3`, D2 `1dd343b` |
| **E** | Score multiplier consolidated into `ScoringService` | E1 `224fe4f` |
| **F** | Indicator reagent semantics given a single owner in the domain | F1 `89787bb` |
| **G** | View ↔ orchestrator cycles broken via role contracts | G1 `ef73cee`, G2 `cbfb3fd`, G3 `21ec7e6` |
| **H** | `ResetFlowState` reuses the existing panel collaborator | H1 `934eea6` |

Two stages produced **deliberate retention** decisions backed by implementation evidence
rather than removal: the Service Locator (Stage D, see
[`ADR-service-locator-cross-scene-lifetime.md`](ADR-service-locator-cross-scene-lifetime.md))
and `TreeSolventGroupButtons` (Stage F2).

---

## Measurement instrument

**Final instrument commit:** `5f1496927193de7f8d45c38068c1a8ef7bbb22d4`

| Component | Version / commit |
|---|---|
| `Tools/static-analysis/analisar.ps1` | `5f14969` (C, D0 and D pinned to fixed refs) |
| `Tools/static-analysis/ferramentas/genproj.js` | `b26b1dd1` (no `ProjectReference` copied) |
| `CodeMetrics` project definition | `9efc4ce` — net9.0, `Microsoft.CodeAnalysis.CSharp` 4.11.0 |
| SonarAnalyzer.CSharp | 10.31.0.145097 (pinned in `genproj.js`) |
| Microsoft.Unity.Analyzers | from the Unity-generated `Assembly-CSharp.csproj` |

The instrument lives entirely in `Tools/` and is never read from the measured worktrees. Each
state is recovered with `git worktree add --detach` at its fixed ref, so production content and
measurement tooling stay conceptually separate — no snapshot is modified to incorporate the
instrument.

**Equivalence evidence.** After the `genproj.js` fix, state C was re-measured through the real
entry point (`analisar.ps1 -Apenas C`): 197 violations, mean MI 72.5, and `git diff` on
`Tools/static-analysis/resultados/estado-C` came back empty — byte for byte identical to the
measurement produced before the fix.

---

## Environment

```
Unity     : 6000.4.1f1
.NET SDK  : 9.0.306
OS        : Windows 11 Home 10.0.26200
```

---

## Functional validation

| Evidence | Result |
|---|---|
| Characterization + invariant suite | **118/118** |
| Compilation (isolated `genproj` build) | 0 errors |
| Compilation (Unity `Assembly-CSharp`) | succeeded |
| Unity project import | successful; 0 `Missing Script`, 0 serialization errors |
| Manual full-flow parity (through Stage G) | **PASS** |
| Stage H1 smoke — restart phase, panel reset, return to menu | **PASS** |
| New game-related Console errors | 0 |

The suite lives in `Tools/characterization/` and is **not** part of the production source
population measured by the instrument, which compiles only `Assets/Scripts`.

---

## Frozen inputs

SHA-256 of the blob contents at `934eea6`:

| File | SHA-256 |
|---|---|
| `Assets/Resources/Data/CompoundsData.json` | `4b81773c5c19b2aa383ceb3b51bb52dad659895ec2c07b82a7f3646fc4fe52e7` |
| `Assets/Resources/Data/QuestionsData.json` | `0aac89714d20e8a5f058bb61c745ab2cadbd8255e268ab516e3c620eac30a193` |
| `Assets/Resources/Data/SolubilityData.json` | `fa8c2273ece93ecba056270b95316a9cabf526d3ef78111f9f3868ce953ecabe` |
| `Assets/Resources/Data/SolventsData.json` | `326e917002c1dd103bf98d5f8ec65107e8d6dfdb22d66241994fd7502339fc4e` |
| `Assets/Scenes/1_MenuScene.unity` | `370694ad5c6f4ade060b2f599fb284f1a6f60f59093f0cde9f108328774545ac` |
| `Assets/Scenes/2_LabScene.unity` | `be130db1114843a5fcae141d62c3ca55fd5f1b51ed8b5015fcbf3388db59b4af` |
| `Assets/Scenes/3_Win_Game.unity` | `9529c5f8caba995d7e3ad54d5bcdf9fa39c8ff83cca98e137e40dd8501a8d943` |
| `Assets/Scripts/Data/Facil.asset` | `d4711f13da3cedb108d85dc1eebfa0d62bfab84f2d8bdffdeeeb364123cc7e50` |
| `Assets/Scripts/Data/Medio.asset` | `777cd6249765dbe313f5aaadb5a952e0c08eb8bdfa5f375d6b2b407e24b4625f` |
| `Assets/Scripts/Data/Dificil.asset` | `b7f6528e70362e31fc8b46689875193082ea2b35c588b5309b8e32a232587991` |
| `Packages/manifest.json` | `818730a07884e6a5ed0ae10af30d3224f1c2b05378de774257f22e319cfe15e2` |
| `Packages/packages-lock.json` | `fad7b38744ec09f7ecd34bc2fdf47e11df77029c516c3b983e96fc65c344c37f` |

### Confounder control

Verified with `git diff --name-only a788ea8 934eea6`:

| Input | Changes since D0 |
|---|---:|
| `Assets/Resources/Data` | **0** |
| `Assets/Scenes` | **0** |
| Difficulty `.asset` files | **0** |
| `Packages/manifest.json`, `Packages/packages-lock.json` | **0** |

No scene was edited, no UnityEvent binding was altered, no GUID changed, and no `MonoBehaviour`
was renamed or moved between namespaces across the entire intervention.

---

## Architectural state boundary

The architectural State D is **`Assets/Scripts @ 934eea6`**.

Commits after `934eea6` are experimental infrastructure only:

| Commit | Scope |
|---|---|
| `b26b1dd1` | `Tools/static-analysis/ferramentas/genproj.js` |
| `5f14969` | `Tools/static-analysis/analisar.ps1` |

Verified: `git diff 934eea6..HEAD -- Assets/` is empty. Neither commit touches production code.

---

## Known items deliberately left untouched

Recorded so they are not mistaken for oversights. None may be acted upon before the formal
measurement; all belong to **POST-STUDY / PRODUCT CLEANUP**.

- Stale serialized `uiController` properties in `2_LabScene.unity` and `3_Win_Game.unity`,
  orphaned by Stage G. Unity loads the scenes without reserializing them.
- Pre-existing dead surface unrelated to any intervention: `PlayerScore`,
  `TotalQuestionsInRun`, `GoBack`, `ApplySelectionVisuals`, `Compound.Group`.
- Seven types without a namespace, and four folder/namespace divergences.
- No `.asmdef` per layer — layer boundaries remain a convention, not compiler-enforced.
- Residual duplication of the `state` ternary between `JsonCompoundRepository` and
  `JsonSolventRepository`, accepted because the cost of a shared abstraction exceeds the cost
  of the duplication.

---

## Freeze declaration

```
NO ARCHITECTURAL CHANGES AFTER D FREEZE
```

`Assets/Scripts` must not be modified before the formal measurement of C × D0 × D. Any finding
that would normally justify a change is to be recorded as post-study cleanup instead.
