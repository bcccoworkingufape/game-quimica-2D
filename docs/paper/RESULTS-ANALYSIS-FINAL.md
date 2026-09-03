# Results Analysis — Final

**Status:** FROZEN · **Date:** 2026-09-03 · Molz, Paper 1

Frozen record of the empirical analysis of the controlled architectural refactoring
experiment. Companion to [`STATE-D-FREEZE.md`](STATE-D-FREEZE.md) and
[`ADR-service-locator-cross-scene-lifetime.md`](ADR-service-locator-cross-scene-lifetime.md).

---

## 1. Framing

**Working title.** Architectural Simplification and Internal Software Quality in a
Unity-Based Serious Game: A Comparative Case Study

**Objective.** To investigate how responsibility-driven architectural simplification of a
functionally preserved Unity-based serious game changed its structural organization and
internal software quality indicators, distinguishing the effect of dead-code removal from
that of the subsequent architectural intervention.

**RQ1.** How did the system's architectural organization change when existing abstractions
were reassessed according to their concrete responsibilities and redundant architectural
elements were simplified?

**RQ2.** How did internal software quality indicators — maintainability, cyclomatic
complexity, structural coupling, engine dependency, and structural size — differ between the
pre-intervention state D0 and the simplified state D, and how do those differences relate to
changes in component population?

**RQ3.** How were changes in internal software quality distributed across components removed
as dead code, directly refactored, indirectly affected, structurally preserved, and newly
introduced by the architectural intervention?

### What this study is not

D did not replace one architectural family with another. State C was already a functioning
MVP system with a Unity-free domain layer, separated Data / Domain / Presentation folders,
and abstractions carrying concrete responsibilities. The intervention reassessed those
abstractions against the responsibilities they actually performed. The initial hypothesis of
migrating to Clean Architecture / Ports and Adapters was rejected during the audit, before any
code was written.

**D0 is not a third competing architecture.** It is a dead-code control checkpoint.

```
C  → D0    dead-code removal control
D0 → D     principal architectural intervention
C  → D     total observed transformation
```

---

## 2. States and instrument

| | Commit | Tag | Branch |
|---|---|---|---|
| C | `9b9816d319f5e60935edcb92dccb1e10e7506c37` | `analise-estatica/versao-C` | `experiment/state-C` |
| D0 | `a788ea8927d4c20afeba7d3b2e3e536d362025d2` | `analise-estatica/versao-D0` | `experiment/state-D0` |
| D | `934eea6aa8e732db57f4575379ba1acd1dd6b58f` | `analise-estatica/versao-D` | `experiment/state-D` |

Final measurement instrument: `5f1496927193de7f8d45c38068c1a8ef7bbb22d4`
(`genproj.js` `b26b1dd1`, CodeMetrics `9efc4ce`, SonarAnalyzer.CSharp 10.31.0.145097,
.NET SDK 9.0.306, Unity 6000.4.1f1).

Determinism: D re-executed through the same pipeline; all six artefacts byte-identical,
including the SARIF.

Functional parity: 118/118 characterization and invariant tests, Unity project import with
0 missing scripts and 0 serialization errors, manual full-flow PASS, H1 smoke PASS.

---

## 3. Aggregate measurements

| Metric | C | D0 | D |
|---|---:|---:|---:|
| Source files | 91 | 80 | 77 |
| LOC (non-blank) | 5 093 | 4 738 | 4 729 |
| Comment lines | 695 | 649 | 825 |
| Classes | 79 | 65 | 60 |
| Interfaces | 16 | 12 | 14 |
| Structs/records | 7 | 5 | 5 |
| Enums | 9 | 9 | 9 |
| Members | 496 | 451 | 457 |
| CC mean | 2.33 | 2.39 | 2.38 |
| CC median | 2.0 | 2.0 | 2.0 |
| CC max | 17 | 17 | 17 |
| CC total | 1 158 | 1 078 | 1 089 |
| MI mean | 72.5 | 71.7 | 71.8 |
| MI median | 71.2 | 70.2 | 70.9 |
| Efferent coupling mean | 11.4 | 12.8 | 13.4 |
| Unity-coupled classes | 24 (30.4%) | 23 (35.4%) | 23 (38.3%) |
| Members with CC > 10 | 6 (1.2%) | 6 (1.3%) | 5 (1.1%) |
| Members with MI ≥ 65 | 323 (65.1%) | 287 (63.6%) | 293 (64.1%) |

All 42 values were recomputed from `metricas-tipos.csv` and `metricas-metodos.csv` and matched
`metricas-resumo.txt` in all three states.

**Units.** LOC, efferent coupling and type counts are measured per type; CC and MI are measured
per member. Group-level CC and MI are means over the members belonging to the group, never
aggregated per type.

**Denominator.** The Unity percentage uses *classes* as denominator: 24/79, 23/65, 23/60.

### Auxiliary pipeline data

Analyser violation counts were C 197, D0 191, D 143. These are raw pipeline output. They do
**not** answer RQ2; interpreting individual findings belongs to a separate study.

---

## 4. Matched-population analysis

Aggregate means mix two phenomena: change inside surviving components, and change in the
population being averaged. The following separates them.

### 4.1 Types — DIRECTLY REFACTORED group

```
types in D0             : 24
types in D              : 19
matched surviving types : 19
removed types           :  5   QuestionDto, CompoundMapper, QuestionMapper,
                               SolutionMapper, SolventMapper
new types               :  0   (IMixingView and IQuestionView belong to
                                NEW INTERVENTION ARTIFACT)
```

### 4.2 Efferent coupling

| Basis | D0 | D | Δ |
|---|---:|---:|---:|
| **Matched surviving types (n = 19)** | 26.84 | 28.37 | **+1.53** |
| Raw group mean (24 → 19 types) | 22.29 | 28.37 | +6.08 |
| Types removed from the group | 5.00 | — | — |

Both phenomena are present. Of the +6.08 raw increase, +1.53 is within-type change; the
remainder follows from removing five types whose mean coupling (5.00) was far below the group.

Per-type variation among survivors — 14 of 19 changed, 12 up and 2 down:

```
+7  JsonSolventRepository     12 → 19
+5  JsonSolutionRepository    14 → 19
+3  JsonCompoundRepository    12 → 15
+3  QuestionFlowPresenter     55 → 58
+3  MixingRoundController     34 → 37
+3  MenuUIController          56 → 59
+2  NavbarController          19 → 21
+2  SolventClickHandler       11 → 13
+1  TestMessageHandler, LabUIController, Solvent, GameManager
−1  MenuPresenter             23 → 22
−2  JsonQuestionRepository    13 → 11
```

The mapper consolidation redistributed and concentrated dependencies into the repositories,
and the coupling metric of several surviving repository types also increased. These are
related but distinct phenomena: architectural responsibility redistribution is not the same as
measured coupling increase.

### 4.3 Members — MI and CC

Member identity `file|type|member` is stable for 449 of 451 keys in D0 and 455 of 457 in D.
The two ambiguous keys in each state are overloads (`LabUIController.ShowQuestionVictoryPanel`,
`UIAnimator.ScaleTo`); they were **excluded** from matching rather than resolved by assumption.

**System-wide**

```
member population D0 : 451   (2 ambiguous keys excluded)
member population D  : 457   (2 ambiguous keys excluded)
matched members      : 442
removed members      :   5   (4 × ToDomain from the mappers, 1 Awake)
new members          :  11   (SetView ×2, SetQuestionTitle, SetSolutionResultText,
                              IsSfxAvailable, IsMusicAvailable, DifficultyMultiplier, …)

MI, matched members  : 71.87 → 71.80    Δ = −0.06    (65 of 442 changed value)
CC, matched members  :  2.41 →  2.42    Δ = +0.01    (12 of 442 changed value)
```

**DIRECTLY REFACTORED group**

```
matched members : 192     removed: 5     new: 10
MI : 71.51 → 71.39    Δ = −0.12    (63 of 192 changed value)
CC :  2.46 →  2.48    Δ = +0.02    (12 of 192 changed value)
```

**The signs invert once population is controlled.** The aggregate MI mean rises (+0.1) while
matched-member MI falls (−0.06); the aggregate CC mean falls (−0.01) while matched-member CC
rises (+0.01). Both aggregate movements are composition effects driven by the eleven new
members — largely simple properties with high MI — and the five removed ones.

All magnitudes are negligible on scales of 0–100 and roughly 2.4. Only 14.7% of matched
members changed MI at all, and 2.7% changed CC.

---

## 5. Engine-coupled set identity

```
Unity-coupled types in D0 : 23
Unity-coupled types in D  : 23
intersection              : 23
D0 only                   : empty
D only                    : empty
```

The absolute engine-coupled set remained unchanged between D0 and D — the same 23 types. The
percentage rose from 35.4% to 38.3% because the class population fell from 65 to 60 while the
absolute count stayed constant.

---

## 6. RQ1 — architectural decisions

| Concern | Initial hypothesis | Evidence | Decision | Consequence |
|---|---|---|---|---|
| `QuestionMapper` | REDUNDANT | Purely positional pass-through; invariants live in the entity | REMOVE | JSON→entity binding now rests on constructor parameter names |
| `QuestionDto` | REDUNDANT | 1:1 copy, no type conversion | REMOVE | Verified by a nine-criterion spike before removal |
| `CompoundMapper`, `SolventMapper`, `SolutionMapper` | DUPLICATED | One caller each; real conversion present | MERGE | Transformation became a private repository method |
| Remaining three DTOs | USEFUL | snake_case, string→enum, one rename | KEEP | Representation boundary preserved |
| Repository surface | cleanup candidate | Four methods without consumers, three of which lost theirs **in Stage A** | KEEP | Removing them would credit D0→D with an effect caused by C→D0 |
| `IAudioService` | NECESSARY but bypassed | 56 concrete accesses against 2 port consumers | STRENGTHEN | 56 → 0; port gained two availability members |
| `ServiceLocator` | **REDUNDANT** | Full removal would require scene scanning, layer inversion, or a lifetime change | **A-PRIORI HYPOTHESIS REJECTED** → KEEP + SIMPLIFY | Three dead registrations removed; registry retained for cross-scene lifetime |
| `GameManager` responsibilities | broad decomposition | Only scoring was split across layers | Selective SIMPLIFY | One MOVE, seven documented KEEPs |
| Litmus reagent identity | duplicated in Presentation | Confirmed at two sites | MOVE | `Solvent.IsIndicator`; coupling shifts from display text to dataset id |
| `TreeSolventGroupButtons` | DUPLICATED | The consumer sustaining the duplication was **removed in Stage A** | **A-PRIORI HYPOTHESIS REJECTED** → KEEP | Content is editorial, carrying TMP markup |
| View ⇄ orchestrator cycles | breakable | Two concrete reverse dependencies, two TMP leaks | STRENGTHEN | Two role contracts; `ILabView` untouched at seven members |
| God Object candidates | decomposition | Only one duplication had a better existing owner | Local SIMPLIFY | One consolidation, nine KEEPs |
| `ApplySelectionVisuals`, `PlayerScore`, `GoBack` | dead | Already dead in C | PRESERVE FOR PARITY | Retained so control and intervention stay separable |

### Observed pattern

The usefulness of an abstraction was context-dependent rather than determined by the pattern it
resembled. Four distinct mechanisms appeared:

1. **Abstraction without current responsibility** → removed (`QuestionMapper`).
2. **Abstraction with concrete responsibility but bypassed** → strengthened (`IAudioService`).
3. **Abstraction whose responsibility only the removal attempt revealed** → retained
   (`ServiceLocator`): static inspection was *incomplete*.
4. **Abstraction whose justification an earlier transformation had already removed** → retained
   (`TreeSolventGroupButtons`): static inspection was *correct at the time* and became obsolete.

Cases 3 and 4 are epistemologically different and should be reported as such.

---

## 7. RQ3 — component categories

### Operational definitions

**DEAD CODE REMOVED.** Present in C and removed exclusively in C→D0, after triple evidence of
unreachability: C# references, GUID references in scenes/prefabs/assets, and type names in YAML.
Three surviving files were edited as collateral of that removal (`Bootstrapper`, `Compound`,
`Solvent`); they belong to the control, not to the intervention.

**DIRECTLY REFACTORED.** Component whose code or responsibility was deliberately changed as a
target of stages B–H.

**INDIRECTLY AFFECTED.** Component changed only to accommodate a change whose primary target
was another component.

**PRESERVED.** Existing component deliberately left unmodified by the relevant architectural
interventions.

**NEW INTERVENTION ARTIFACT.** Element absent from the baseline, introduced as a necessary part
of the intervention. Has no C or D0 row — recorded as absent, never as zero.

### Distribution by file

| Group | C | D0 | D |
|---|---:|---:|---:|
| DIRECTLY REFACTORED | 24 | 24 | 19 |
| INDIRECTLY AFFECTED | 1 | 1 | 1 |
| PRESERVED | 55 | 55 | 55 |
| DEAD CODE REMOVED | 11 | 0 | 0 |
| NEW INTERVENTION ARTIFACT | — | — | 2 |
| **Total** | **91** | **80** | **77** |

### Group metrics, D0 → D

| Group | Types | Members | LOC | CC mean | MI mean | MI median | Coupling mean | Unity |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| DIRECTLY REFACTORED D0 | 24 | 199 | 2 357 | 2.44 | 71.2 | 70.2 | 22.29 | 8 |
| DIRECTLY REFACTORED D | 19 | 204 | 2 505 | 2.43 | 71.4 | 72.1 | 28.37 | 8 |
| INDIRECTLY AFFECTED D0 | 3 | 11 | 58 | 1.36 | 83.5 | 93.5 | 7.00 | 0 |
| INDIRECTLY AFFECTED D | 3 | 12 | 67 | 1.33 | 84.0 | 93.5 | 7.67 | 0 |
| **PRESERVED D0** | 64 | 241 | 3 286 | 2.40 | 71.5 | 69.7 | 9.56 | 15 |
| **PRESERVED D** | **64** | **241** | **3 286** | **2.40** | **71.5** | **69.7** | **9.56** | **15** |
| NEW ARTIFACT D | 2 | 0 | 30 | n/a | n/a | n/a | 0.50 | 0 |

`NEW ARTIFACT` has zero members because both are pure interfaces; the instrument assigns no
CC or MI to declarations without bodies.

### Group metrics, C → D0

| Group | Types | Members | LOC | CC mean | MI mean | Coupling mean |
|---|---:|---:|---:|---:|---:|---:|
| **DEAD CODE REMOVED** (C only) | 20 | 45 | 356 | **1.78** | **80.3** | **4.50** |
| *project in C, for contrast* | 111 | 496 | — | 2.33 | 72.5 | 11.4 |

In the remaining groups C→D0 is near zero: DIRECTLY REFACTORED −7 LOC and −0.12 coupling,
PRESERVED −0.02 coupling, INDIRECTLY AFFECTED unchanged.

### Interpretation and its limit

The measured structural changes were confined to components classified as directly or
indirectly affected, while the preserved group remained metric-identical across D0 and D.

**Limitation.** The PRESERVED group is *defined* by the absence of relevant source changes, and
the metrics are derived from static code. Its metric identity is therefore evidence that the
measured changes were contained — not an independent validation of the classification, and not
a causal attribution. Classification and result are not independent.

---

## 8. Unexpected findings

1. **Removing dead code moved every normalized mean unfavourably.** The removed code had MI
   mean 80.3 against 72.5 for the project, and coupling 4.50 against 11.4. Without the D0
   checkpoint this effect would have been credited to the architectural intervention.

2. **Engine dependency did not fall.** The same 23 types in D0 and D; the percentage rose
   through denominator change alone. This contradicted an expectation central to the earlier
   framing of the study.

3. **Simplification increased interfaces and members.** −5 classes, +2 interfaces, +6 members.

4. **Two a-priori hypotheses were rejected by implementation evidence**, for different reasons
   (incomplete inspection versus obsolete justification).

5. **MI and CC signs invert under matched-population analysis.** Aggregate movements are
   composition effects, not changes in surviving code.

6. **Coupling rose in the repositories that absorbed the mappers** — both by redistribution and
   as a genuine within-type increase.

7. **The original architectural premise was wrong.** The domain layer was already free of
   Unity in C; the Clean Architecture hypothesis was rejected before any code changed.

---

## 9. Threats to validity

**Construct.** MI, CC and efferent coupling are structural proxies, not measures of perceived
maintainability. *Engine dependency* is a count of types referencing `UnityEngine` as defined
by the instrument, and its percentage uses classes as denominator, making it sensitive to
population change.

**Internal.** The eight stages were sequential and may interact. D0 isolates dead-code removal
but does not isolate B–H from one another; two concrete interactions were observed, where
Stage A removed the justification for later candidate changes.

**External.** A single Unity serious game, 77 files, a small academic team, three sequentially
loaded scenes. Conclusions about cross-scene lifetime depend on that topology.

**Reliability.** Versioned and frozen instrument; snapshots pinned by commit, tag and branch;
isolated worktrees; determinism confirmed by a byte-identical re-run.

**Researcher bias.** The authors conducted both the intervention and the analysis. Partial
mitigation: a-priori criteria registered before measurement, per-stage human approval gates,
and RQ3 classification derived from commits rather than post-hoc judgement.

**Functional equivalence.** Characterization tests, Unity import, manual full-flow and the H1
smoke provide strong evidence for the behaviour covered — not formal proof of equivalence.
Structural differences on unreachable paths were recorded rather than hidden.

**LLM-assisted implementation.** Implementation was assisted by an LLM coding agent (Claude
Code) under a protocol with per-stage human approval. This may have influenced code shape and
decisions. The study does not measure AI productivity or quality; no design supports that.

---

## 10. Candidate contributions

1. A protocol for responsibility-driven architectural simplification, with a-priori
   classification, per-stage approval gates and recorded KEEP decisions — including the
   mechanism of rejecting the initial hypothesis when implementation contradicts it.

2. A comparative C/D0/D design with a control checkpoint, quantitatively isolating dead-code
   removal from the architectural intervention under verified functional preservation and a
   frozen instrument.

3. Evidence that structural simplification does not produce uniform improvement in aggregate
   indicators, with explicit decomposition of within-component change versus composition
   effect — including cases where the sign of the delta inverts once population is controlled.

4. A component-level analysis showing that measured structural changes were concentrated in the
   directly and indirectly affected groups, while the source-preserved group remained
   metric-identical.

---

## 11. Claims prohibited in the manuscript

| Prohibited | Reason |
|---|---|
| "perfect localization of impact" | Circular with the group definition |
| "the effect is attributable to the intervened components" | Unsupported causality |
| "the intervention did not leak" | Same |
| "reallocation, not growth" | False — surviving types rose by +1.53 |
| "engine coupling decreased" | Identical set, 23 → 23 |
| "38.3% shows greater engine dependency" | Denominator effect |
| "MI improved" / "maintainability improved" | Matched-member MI **fell** by 0.06 |
| "CC decreased" | Matched-member CC **rose** by 0.01 |
| "God Objects were decomposed" | One local consolidation |
| "MVP → Clean Architecture" | Hypothesis rejected during the audit |
| "limited separation of concerns" in the baseline | C already had a Unity-free domain |
| "mathematically proven functionally equivalent" | Strong evidence, not proof |
| Fewer violations therefore better architecture | Auxiliary data; belongs to the companion study |
| A specific LLM model as a reproducible fact | No reproducible evidence; use "LLM-assisted implementation (Claude Code)" |
| 3D × 2D comparison as evidence | Historical context only |
| Any p-value or significance test | Neither proposed nor performed |
