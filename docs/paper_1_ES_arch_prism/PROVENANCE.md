# Paper 1 — Provenance and Operational Definitions

**Status:** verification record · **Date:** 2026-10-03 · **Verified against:** working tree at `2606cdc`

Answers to the reproducibility questions, each traced to the artefact that
establishes it. Every claim here was re-derived from the repository today; none
is quoted from an earlier summary.

---

## 1. Is C (`9b9816d`) exactly the final state of the PIBIC report?

**Yes, with one qualification about how it was measured.**

| Evidence | Result |
|---|---|
| `analise-estatica/versao-C` → `9b9816d319f5e60935edcb92dccb1e10e7506c37` | tag object `2eb42be`, dereferences to C |
| `main` | `9b9816d` |
| `origin/main` | `9b9816d` |
| `resultados/estado-C/metricas-resumo.txt` vs `resultados/2D-atual/metricas-resumo.txt` | **byte-identical except the label line** (`Estado C (baseline)` vs `2D (Molz atual)`) |
| `resultados/estado-C/sonar-resumo.txt` | 197 violations, 18 distinct rules, `93x UNT0008` |

The PIBIC report's "C — final" column (interfaces 16, Unity dependency 30.4 %,
efferent coupling 11.4, MI 72.5, 197 findings) reproduces exactly.

**Qualification.** The PIBIC run measured target `2D-atual`, whose source path
in `analisar.ps1` is the **working tree** (`$repo2D\Assets\Scripts`), not a
pinned ref. Target `estado-C` measures a detached worktree at
`analise-estatica/versao-C^{}`. The two outputs being byte-identical is what
establishes that the working tree at PIBIC time was `9b9816d`. It is derived
evidence, not a pinned guarantee — but it is strong, because any difference in
`Assets/Scripts` would have changed at least one of the 42 summary values.

---

## 2. Engine coupling: the implemented definition

> **The manuscript currently misstates this. The number is right; the
> definition printed next to it is not.**

### What the instrument actually computes

`Tools/static-analysis/ferramentas/CodeMetrics/Program.cs`, lines 56–58:

```csharp
var bases = td.BaseList?.Types.Select(b => b.Type.ToString()) ?? Enumerable.Empty<string>();
bool isMono = bases.Any(b => b.Contains("MonoBehaviour") || b.Contains("ScriptableObject"));
if (isMono) nMonoBehaviour++;
```

The `unity` column is **1 when the type's declared base list textually contains
`MonoBehaviour` or `ScriptableObject`**, and 0 otherwise. The internal counter
is named `nMonoBehaviour`; only the CSV header (`unity`) and the summary label
(`tipos acoplados a Unity`) suggest otherwise.

Operationally, therefore:

> **engine coupling = the type directly declares `MonoBehaviour` or
> `ScriptableObject` as a base type** — i.e. the type is one the engine itself
> instantiates and serializes.

### What the manuscript says

Three places state a different definition:

- Measurement Instrument: *"whether the type references `UnityEngine`"*
- Discussion §5.2: *"counts types holding an explicit structural reference to a
  `UnityEngine` API"*
- Threats, Construct validity: *"a count of types referencing `UnityEngine` as
  determined by the instrument"*

### Empirical check

| State | `unity = 1` | Inherit `MonoBehaviour`/`ScriptableObject` | Types in files mentioning `UnityEngine` |
|---|---:|---:|---:|
| C | 24 | 24 | 37 |
| D | 23 | 23 | 35 |

Set differences between columns 1 and 2 are **empty in both directions, in both
states**. The third column is strictly larger: 13 types in C and 12 in D
reference `UnityEngine` without being flagged — among them `ServiceLocator`,
`MenuPresenter`, `LabPresenter`, `SceneRouter`, `ResourcesJsonProvider`,
`LabHudView`, `LabPanelsView`, `LabAudioToggleView`.

### Did it change relative to the PIBIC report?

**No.** The instrument computed `isMono` the same way for the PIBIC run; the
30.4 % in the report and the 30.4 % in `estado-C` come from the same code path
and the same 24 types. What changed is only the prose description introduced
when the manuscript was written.

### Consequences

1. **No measured value changes.** 24 / 23 / 23 and 30.4 / 35.4 / 38.3 % stand.
2. **Three sentences in the manuscript must be corrected** to the inheritance
   definition.
3. **The Discussion argument gets stronger, not weaker.** §5.2 already explains
   the result by saying "a component a scene instantiates must be a
   `MonoBehaviour`". Under the correct definition that is no longer an analogy —
   it is precisely what the metric measures. The set is invariant because the
   intervention never changed which types the engine instantiates.
4. **A limitation must be added:** the check is syntactic and direct-only. A
   class inheriting from an intermediate class that itself extends
   `MonoBehaviour` is **not** counted, and no such case was audited.

### Additional caveat

Syntax-only analysis means `MonoBehaviour` is matched as text in the base list.
An unrelated type whose name contains that substring would be a false positive;
none exists in this codebase.

---

## 3. Exact formulas and counting rules

### Maintainability Index

`Program.cs` lines 252–257:

```csharp
double v = volume <= 0 ? 1 : volume;
double raw = 171 - 5.2 * Math.Log(v) - 0.23 * cc - 16.2 * Math.Log(loc);
return Math.Max(0, raw * 100.0 / 171.0);
```

$$\mathrm{MI} = \max\left(0,\; \frac{100}{171}\left(171 - 5.2\ln V - 0.23\,\mathrm{CC} - 16.2\ln \mathrm{LOC}\right)\right)$$

- Microsoft variant: clamped at 0, rescaled to 0–100. No comment-percentage term.
- `Math.Log` is the **natural** logarithm.
- Computed **per member**, from that member's own $V$, CC and LOC.
- $V \le 0$ is replaced by 1 before the logarithm.
- Rounded to one decimal on output.

### Halstead volume

Lines 231–249. $V = (N_1 + N_2)\log_2(n_1 + n_2)$, returning 0 when the
vocabulary is $\le 1$.

- **Operands:** identifier, numeric/string/char literal, `true`, `false`, `null`.
- **Operators:** any punctuation or keyword token.
- $n_1, n_2$ are counts of distinct **token texts**, not semantic symbols.
- Deviates from classical Halstead, which uses $\log_2$ of distinct operators
  and operands separately in the same way but normally excludes declarations.

### Cyclomatic complexity

Lines 184–211. Starts at 1 and adds 1 for each of: `if`, `while`, `for`,
`foreach`, `do`, `catch`, ternary, `case` label, pattern `case`, switch-expression
arm, `&&`, `||`, `??`, and `?.`.

- Counting `??` and `?.` is a deliberate extension beyond McCabe; both create a
  branch in the compiled form.
- `else` adds nothing. `switch` adds per arm, not per statement.
- Computed over the member **body**; for properties, over the accessor list.

### Efferent coupling

Lines 72–75, labelled `acoplamento eferente aproximado` in the source:

```csharp
var referenced = td.DescendantNodes().OfType<IdentifierNameSyntax>()
    .Select(i => i.Identifier.Text)
    .Where(n => n.Length > 1 && char.IsUpper(n[0]))
    .Distinct().Count();
```

> **Count of distinct identifier names inside the type declaration whose text is
> longer than one character and begins with an uppercase letter.**

This is a **proxy, not Martin's $C_e$**. It is not a count of external types
depended upon. It over-counts, because a capitalised `IdentifierNameSyntax`
also matches: invoked method names (`Log` in `Debug.Log`), property names,
enum members, constants, and the type's own nested names. It under-counts
fully-qualified references, generic arguments reached through other node types,
and any dependency expressed without an identifier.

It is computed **per type**, over the whole type declaration including nested
members. Project-level "efferent coupling mean" is the arithmetic mean over all
type rows, interfaces and enums included.

This is the weakest construct in the instrument and the manuscript should say so
explicitly in Construct validity.

### LOC

Three different rules, all in use:

| Level | Rule | Includes |
|---|---|---|
| File (`linhas de codigo`) | trimmed line is non-empty **and** does not start with `//`, `///`, `*`, `/*` | code lines only |
| File (`linhas de comentario`) | trimmed line starts with `//`, `///`, `*`, `/*` | comment lines only |
| Type / member (`loc` column) | `endLine - startLine + 1` of the declaration span | **blank and comment lines inside the span** |

Consequences worth stating: a trailing comment (`int x = 1; // note`) counts as
**source**; interior lines of a block comment that do not begin with `*` count
as **source**; and the per-member `loc` feeding MI is a **span**, so a member
padded with blank lines scores a lower MI.

---

## 4. Filters and exclusions

### Metrics tool (`CodeMetrics`)

`Program.cs` lines 10–16 accept excluded folder names as arguments 4+.
`analisar.ps1` invokes it as:

```powershell
dotnet run --project ...CodeMetrics -c Release --no-build -- `
  $a.rotulo $a.scripts (Join-Path $saida 'metricas')
```

**Three arguments only — no exclusion list is ever passed.**

Therefore: **every `*.cs` file under `Assets/Scripts`, recursively, with no
exclusion whatsoever.** The measured population is the whole production script
tree. `Assets/Tests`, `Assets/LeanTween`, `Assets/TextMesh Pro` and the other
third-party folders are outside `Assets/Scripts` and so never enter — not
because they are filtered, but because the root excludes them.

The characterization suite in `Tools/characterization` is likewise outside the
root and is not measured.

### Analyzer pipeline (`genproj.js` → build → `sarif.js`)

Compilation unit (`genproj.js`):

- `netstandard2.1`, `LangVersion 9.0`, `EnableDefaultItems=false`
- `NoWarn: 0169; USG0001; CS8632`
- `NoStandardLibraries`, `NoStdLib`, `NoConfig`, `DisableImplicitFrameworkReferences`
- `TreatWarningsAsErrors=false`
- `SonarAnalyzer.CSharp` 10.31.0.145097, plus the analyzers inherited from
  Unity's generated `Assembly-CSharp.csproj`
- Compiles the target `Assets/Scripts/**/*.cs` **plus** three source-distributed
  libraries required to compile: `LeanTween/Framework`, `QuickOutline/Scripts`,
  `EasyLoadingScreen`

Report filtering (`sarif.js` lines 5–30), applied when producing
`sonar-resumo.txt` and `sonar-regras.csv`:

- keep only results whose URI contains `assets/scripts`
- then drop any URI containing `leantween`, `textmesh pro`, `tutorialinfo`,
  `quickoutline`, `easyloadingscreen`

So the three libraries are **compiled but excluded from the report**. The
reported counts (C 197, D0 191, D 143) are post-filter.

**The raw `sonar.sarif` is unfiltered** and still contains third-party results.

---

## 5. Commits, tags and stages

### States

| State | Commit | Tag object | Branch |
|---|---|---|---|
| C | `9b9816d319f5e60935edcb92dccb1e10e7506c37` | `analise-estatica/versao-C` → `2eb42be` | `experiment/state-C` |
| D0 | `a788ea8927d4c20afeba7d3b2e3e536d362025d2` | `analise-estatica/versao-D0` → `86f83f3` | `experiment/state-D0` |
| D | `934eea6aa8e732db57f4575379ba1acd1dd6b58f` | `analise-estatica/versao-D` → `da578ac` | `experiment/state-D` |

Also present: `analise-estatica/versao-B` → `1226b594` (pre-MVP, outside this
experiment).

All four tags are **local only**; none has been pushed.

### Stages

| Stage | Intervention | Commits |
|---|---|---|
| **A** | Dead-code removal — 11 files, 20 types | `a788ea8` ⇒ **D0** |
| **B** | Question materialization; mapper consolidation | `d3d949c`, `2ec5f5f` |
| **C** | Audio boundary — 56 concrete accesses → 0 | `3b88c1e`, `8d93dc8`, `cad84f7` |
| **D** | Partial explicit composition | `a1a8bb3`, `1dd343b` |
| **E** | Score multiplier into `ScoringService` | `224fe4f` |
| **F** | Indicator reagent semantics into `Solvent` | `89787bb` |
| **G** | View ⇄ orchestrator cycles via role contracts | `ef73cee`, `cbfb3fd`, `21ec7e6` |
| **H** | `ResetFlowState` reuses the panel collaborator | `934eea6` ⇒ **D** |

Non-intervention commits inside the range `9b9816d..934eea6`, which must not be
read as stages: `0a869bc` (baseline suite), `9efc4ce` (CodeMetrics project
definition), `882cd6d` (raw measurements for C and D0), `b6d7c7e`, `0b5d769`,
`059fd06`, `5704b0b` (test additions), `a93aad9`, `0f6225c`, `1916037` (docs).

---

## 6. Where the artefacts are

| Artefact | Path |
|---|---|
| Per-member metrics | `Tools/static-analysis/resultados/estado-{C,D0,D}/metricas-metodos.csv` |
| Per-type metrics | `.../metricas-tipos.csv` |
| Summary | `.../metricas-resumo.txt` |
| Raw SARIF (unfiltered) | `.../sonar.sarif` (~764 KB each) |
| Filtered analyzer summary | `.../sonar-resumo.txt`, `.../sonar-regras.csv` |
| Metrics tool | `Tools/static-analysis/ferramentas/CodeMetrics/Program.cs` (260 lines) |
| Project generator | `Tools/static-analysis/ferramentas/genproj.js` |
| SARIF post-processor | `Tools/static-analysis/ferramentas/sarif.js` |
| Comparison generator | `Tools/static-analysis/ferramentas/comparativo.js` |
| Pipeline entry point | `Tools/static-analysis/analisar.ps1` |
| Validation suite | `Tools/characterization/Tests/*.cs` (12 files), `testar.ps1` |
| Freeze record | `docs/paper/STATE-D-FREEZE.md` |
| Results analysis | `docs/paper/RESULTS-ANALYSIS-FINAL.md` |
| Service-locator ADR | `docs/paper/ADR-service-locator-cross-scene-lifetime.md` |
| Scope firewall | `docs/paper/PAPER1-PAPER2-SCOPE.md` |

Each state directory is 824–868 KB, dominated by the SARIF.

Also present but **outside this experiment**: `resultados/3D/`,
`resultados/2D-pre-mvp/`, `resultados/2D-atual/`.

---

## 7. The two excluded overloads

Both ambiguous keys are the **same two in both states**:

| Key | Rows | `loc` / `cc` / `mi` |
|---|---|---|
| `LabUIController.ShowQuestionVictoryPanel` | 2 | 10 / 3 / 62.3 and 4 / 1 / 76.5 |
| `UIAnimator.ScaleTo` | 2 | 11 / 1 / 63.3 and 10 / 1 / 64.0 |

They are ambiguous because the identity key is `file|type|member` and the
instrument does not emit a parameter signature, so two overloads collapse onto
one key.

**All four rows are identical between D0 and D** (verified by sorted comparison
of `loc`, `cc`, `mi`). Neither overload was touched by the intervention.

### Impact: none of consequence

| Basis | n | MI D0 → D | Δ MI | CC D0 → D | Δ CC |
|---|---:|---|---:|---|---:|
| Excluding them (as reported) | 442 | 71.8661 → 71.8043 | **−0.0618** | 2.4072 → 2.4163 | **+0.0090** |
| Including them, paired by order | 446 | 71.8182 → 71.7570 | −0.0612 | 2.3991 → 2.4081 | +0.0090 |

Direction identical, magnitude identical to three decimals. The exclusion is a
correctness measure, not a result-bearing choice, and the paper can say so.

Arithmetic check: D0 has 451 members, minus 4 ambiguous rows = 447 unique keys =
442 matched + 5 removed. D has 457 − 4 = 453 = 442 matched + 11 new.

---

## 8. Test execution evidence

**No execution log was versioned.** `Tools/characterization/.gitignore` excludes
`TestResults/`, so the 68/68 and 118/118 figures previously rested only on
static attribute counts and on the assertion in `STATE-D-FREEZE.md`.

**Re-executed today** on .NET SDK 9.0.306, each state in a detached worktree:

```
C   (0a869bc1, Assets at 9b9816d)
    Passed! - Failed: 0, Passed:  68, Skipped: 0, Total:  68, Duration: 150 ms

D0  (a788ea8)
    Passed! - Failed: 0, Passed:  68, Skipped: 0, Total:  68, Duration: 162 ms

D   (934eea6, = current HEAD of Assets/Scripts)
    Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118, Duration: 655 ms
```

This confirms all three figures by execution, and confirms that Stage A added no
tests: C and D0 run the identical 68.

**Recommendation.** Capture these as `.trx` files and version them under
`Tools/characterization/evidence/`, or generate them in the replication package
build. Until then the claim is reproducible but not archived.

---

## 9. Zenodo publication — what is unrestricted

### Publishable without restriction

| Artefact | Note |
|---|---|
| `metricas-metodos.csv`, `metricas-tipos.csv`, `metricas-resumo.txt` (3 states) | Own measurements over own source |
| `sonar-resumo.txt`, `sonar-regras.csv` | Already filtered to own code |
| `CodeMetrics/Program.cs`, `genproj.js`, `sarif.js`, `comparativo.js`, `analisar.ps1` | Own tooling |
| Analysis scripts (aggregation, matched-population, audit) | Own |
| `Tools/characterization/Tests/*.cs`, `testar.ps1`, `Molz.Characterization.csproj` | Own |
| `docs/paper/*.md` | Own records |
| Commit identifiers, tag names, branch names | Metadata |

### Requires action before publication

| Artefact | Problem | Action |
|---|---|---|
| **`sonar.sarif` (all 3)** | Contains **absolute local paths with the username**: `file:///C:/Users/ben/AppData/Local/Temp/molz-estado-C/...` | Rewrite URIs to repository-relative before publishing |
| **`sonar.sarif` (all 3)** | Unfiltered: retains results over LeanTween, QuickOutline, EasyLoadingScreen source | Either filter to `Assets/Scripts` or state that third-party results are present and not analysed |
| **The repository itself** | **No `LICENSE` file exists.** Public on GitHub without a licence defaults to all rights reserved | Choose and add a licence before archiving, or Zenodo will carry an unlicensed deposit |
| `Assets/` beyond `Scripts/` | LeanTween, TextMesh Pro, TutorialInfo, Adaptive Performance, Fonts, GUI, Images are third-party or asset-store material with their own terms | Do **not** redistribute; archive `Assets/Scripts` plus `Assets/Resources/Data` only, and point to the repository for the rest |
| Unity packages / NuGet analyzers | Referenced, not redistributed | Pin versions in a manifest; do not vendor |

### Must not be published in this package

| Artefact | Reason |
|---|---|
| The 103-participant evaluation spreadsheet | Human-subject data; belongs to the WebMedia study and needs its own consent and anonymisation basis |
| PIBIC report `.docx` / `.pdf` in `docs/` | Institutional documents, not research artefacts of this paper |

### Recommended deposit shape

Archive **`Assets/Scripts` at the three pinned commits** plus the instrument,
the measurements, the analysis scripts, the test suite and the records — not a
clone of the whole Unity project. That keeps the deposit free of third-party
material while remaining sufficient to re-run every reported number.

---

## Open items

1. **Correct the three engine-coupling sentences in the manuscript** (§2 above).
   Highest priority: it is the only factual misstatement found.
2. **State the efferent-coupling proxy explicitly** in Construct validity.
3. **Add the direct-inheritance-only limitation** for engine coupling.
4. Archive the test logs.
5. Add a `LICENSE`.
6. Sanitise the SARIF URIs.
