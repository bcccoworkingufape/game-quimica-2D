# Paper 1 — Claim Traceability Matrix

**Internal audit artefact.** Not part of the manuscript. Every important claim
in `main.tex` is listed with the evidence that supports it and the analysis that
connects the two. A claim with no evidence row must not be in the paper.

Verified against the working tree on 2026-10-03. Numeric sources are
`Tools/static-analysis/resultados/estado-{C,D0,D}/`; definitions are in
`PROVENANCE.md`.

---

## RQ1 — Architectural transformation

| Evidence | Analysis | Result | Supported claim |
|---|---|---|---|
| 13 audited concerns; audit records + stage commits `d3d949c`…`934eea6` | Per-concern: initial hypothesis → evidence found → decision → consequence | 3 removals/merges, 2 strengthenings, 3 local simplifications or ownership moves, 5 retentions | "Thirteen concerns were audited…" (Table 5) |
| `Data/Mappers/` present at `9b9816d`, absent at `934eea6`; `QuestionDto.cs` likewise | File-level diff between pinned trees | 4 mappers + 1 DTO removed; 3 DTOs retained | Mapper consolidation; representation boundary preserved |
| grep `SfxManager.Instance` / `MusicManager.Instance` outside the adapter | Count per state | C = 56 (MenuUI 24, LabUI 23, Navbar 3, Mixing 2, QFP 2, GameManager 1); D = 0 | "56 concrete access sites reduced to zero" — **structural**, not runtime |
| `IAudioService.cs` member signatures at both commits | Set difference | 17 → 19 members (`IsSfxAvailable`, `IsMusicAvailable` added) | Audio port strengthened, not replaced |
| `Register<>` in `Bootstrapper.cs` at `a788ea8` vs `934eea6` | Count | 8 → 5 | "Three dead registrations removed; registry retained" |
| `ADR-service-locator-cross-scene-lifetime.md` | Removal attempt record | Cross-scene lifetime responsibility nothing else discharged | ServiceLocator retained — **a-priori hypothesis rejected** |
| Stage A deleted the consumer sustaining the duplication | Commit-order analysis | Justification disappeared before the stage that would have acted | `TreeSolventGroupButtons` retained — second rejected hypothesis |
| `ultiplier` occurrences in `GameManager.cs` / `ScoringService.cs` | Count per state | C: GM 3 / SS 0 → D: GM 0 / SS 1 | Score multiplier ownership consolidated into the domain |
| `IsIndicator` absent in C; in D present in `Solvent.cs` and `TestMessageHandler.cs` | Symbol presence | Single domain owner introduced | Indicator semantics centralised |
| `MixingView.cs`, `QuestionView.cs` exist only at `934eea6`; fields are `IMixingView` / `IQuestionView` | Field-type inspection; residual `LabUIController` mentions are comments only | Concrete reverse dependency eliminated | Two role contracts introduced; cycles broken |
| Class/interface/member counts, 3 states | Aggregation from `metricas-tipos.csv` | 65→60 classes, 12→14 interfaces, 451→457 members | **"Simplification did not mean minimising abstractions"** |

---

## RQ2 — Internal quality indicators

| Evidence | Analysis | Result | Supported claim |
|---|---|---|---|
| `metricas-{tipos,metodos}.csv`, 3 states | Recomputed from raw rows; cross-checked against `metricas-resumo.txt` | 48/48 summary values matched | Instrument output is internally consistent |
| Same, C → D0 | Control comparison | MI 72.5→71.7; coupling 11.4→12.8 | Dead-code removal alone moved every aggregate mean |
| Dead-code subset at C (20 types, 45 members) | Group aggregation | MI 80.3 vs 72.5; CC 1.78 vs 2.33; coupling 4.50 vs 11.4 | Removed subset was **unrepresentative** → composition effect |
| Same, D0 → D | Principal comparison | MI 71.7→71.8; CC 2.39→2.38; coupling 12.8→13.4; files 80→77; LOC 4738→4729 | Aggregate indicators moved little |
| 442 members matched on `file\|type\|member` | Matched-population means | MI 71.87→71.80 (−0.06); CC 2.41→2.42 (+0.01) | **Direction reverses when population is held fixed** |
| 65 of 442 changed MI; 12 of 442 changed CC | Change counting | 14.7 % and 2.7 % | Magnitudes small in absolute terms; no practical-significance claim |
| 19 surviving DIRECT types | Matched coupling | 26.84 → 28.37 (+1.53); raw group +6.08; removed types mean 5.00 | Within-type rise is **real**, not only redistribution |
| `unity` column, D0 vs D | Set identity, not count | 23 = 23; intersection 23; both differences empty | Engine-instantiated set unchanged |
| Class population 65 → 60 | Denominator analysis | 35.4 % → 38.3 % with constant numerator | Percentage movement is a **denominator artefact** |

---

## RQ3 — Population-aware distribution

| Evidence | Analysis | Result | Supported claim |
|---|---|---|---|
| Category assignment derived from stage commits | Mechanical, not post-hoc | 5 categories mapping to removed / added / modified / preserved | Classification independent of the metric values |
| File counts per category, 3 states | Distribution table | C 24/1/55/11/– ; D0 24/1/55/–/– ; D 19/1/55/–/2 | Totals reconcile to 91 / 80 / 77 |
| PRESERVED group, D0 vs D | Group metrics | 64 types, 241 members, 3286 LOC, CC 2.40, MI 71.5, coupling 9.56, engine 15 — **identical** | Measured change contained outside the preserved group |
| Same group defined by absence of source change | Circularity check | Containment follows partly from the definition | Stated explicitly as **not** independent validation and **not** causal attribution |
| NEW artefacts are pure interfaces | Member counting | 0 members; no CC/MI assigned | Absent ≠ zero; no artificial values injected |

---

## Behavioural preservation

| Evidence | Analysis | Result | Supported claim |
|---|---|---|---|
| `dotnet test` at `0a869bc1`, `a788ea8`, `934eea6` (2026-10-03) | Execution, .NET SDK 9.0.306 | C 68/68, D0 68/68, D 118/118 — 0 failed | Behaviour **covered by the suite** preserved |
| Suite composition at D | Attribute counting | 103 behavioural + 15 architecture invariants | Reported separately; invariants excluded from the behavioural argument |
| Suite growth history | Commit-by-commit | 68 baseline → 118, with 13 mapping tests retired with their mappers | Only the 68 constrained every stage — stated as a limitation |
| Unity import at D | Editor import | 0 missing scripts, 0 serialization errors | No GUID or serialized binding invalidated |

---

## Claims deliberately **not** made

| Not claimed | Why |
|---|---|
| "The refactoring improved software quality" | Indicators moved in both directions; no quality construct was measured |
| "MI improved" / "CC decreased" | Matched-population movement is the opposite of the aggregate |
| "Engine coupling decreased" | Set identical, 23 = 23 |
| "38.3 % shows greater engine dependence" | Denominator artefact |
| Formal functional equivalence | Suite coverage only |
| Causal attribution to intervened components | No randomisation, no control system, sequential stages |
| Any statistical significance | Complete population of one system; no sampling model |
| 56 runtime calls eliminated | Static call sites, not runtime counts |
| Generalisation beyond this case | Analytical generalisation only |

---

## Unsupported-claim scan

Run over `main.tex` on 2026-10-03: zero occurrences of *improved software quality*,
*no previous study*, *proves*, *optimal*, *best architecture*, *statistically
significant*, *p-value*, *State E*, *LLM / Claude / AI-assisted*, and zero
occurrences of the superseded engine-dependency wording (*references
UnityEngine*). Both `$C_e$` occurrences are disclaimers stating that our figures
are **not** $C_e$.
