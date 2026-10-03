# Paper 1 — Replication Package Checklist

**Status:** NOT PUBLISHED · **Last verified:** 2026-10-03
Source of truth for every definition: [`PROVENANCE.md`](PROVENANCE.md).

Nothing here has been uploaded anywhere. This is the list of what the deposit
must contain and what must be done to each item first.

---

## 1. States, commits and tags

| Item | Value | Public? |
|---|---|---|
| State C | `9b9816d319f5e60935edcb92dccb1e10e7506c37` | **yes** — it is `origin/main` |
| State D0 | `a788ea8927d4c20afeba7d3b2e3e536d362025d2` | no — local only |
| State D | `934eea6aa8e732db57f4575379ba1acd1dd6b58f` | no — local only |
| Tag `analise-estatica/versao-C` | → `9b9816d` | no |
| Tag `analise-estatica/versao-D0` | → `a788ea8` | no |
| Tag `analise-estatica/versao-D` | → `934eea6` | no |
| Branches `experiment/state-{C,D0,D}` | pinned | no |

**Action:** push the branches and tags, or carry the three trees in the deposit.
Today only C is reachable by anyone else.

---

## 2. Measurement artefacts

| Item | Path | Action |
|---|---|---|
| Per-member metrics | `Tools/static-analysis/resultados/estado-{C,D0,D}/metricas-metodos.csv` | ship as is |
| Per-type metrics | `.../metricas-tipos.csv` | ship as is |
| Summaries | `.../metricas-resumo.txt` | ship as is |
| Filtered analyzer output | `.../sonar-resumo.txt`, `.../sonar-regras.csv` | ship as is |
| Raw SARIF | `.../sonar.sarif` (~764 KB each) | **SANITISE FIRST** |

### SARIF sanitisation — required

Two problems, both verified:

1. **Absolute local paths including the username**, e.g.
   `file:///C:/Users/ben/AppData/Local/Temp/molz-estado-C/Assets/...`
   Rewrite every `artifactLocation.uri` to a repository-relative path.
2. **Third-party results retained.** The raw SARIF still contains findings over
   LeanTween, QuickOutline and EasyLoadingScreen, which are compiled but
   excluded from the reported counts. Either filter to `Assets/Scripts` or state
   in the README that they are present and were not analysed.

---

## 3. Instrument

| Item | Path | Note |
|---|---|---|
| Metrics tool | `Tools/static-analysis/ferramentas/CodeMetrics/Program.cs` | 260 lines, Roslyn syntax-only |
| Project generator | `.../ferramentas/genproj.js` | |
| SARIF post-processor | `.../ferramentas/sarif.js` | |
| Comparison generator | `.../ferramentas/comparativo.js` | |
| Pipeline entry point | `Tools/static-analysis/analisar.ps1` | |

**Version manifest to include:**

```
Unity                  6000.4.1f1
.NET SDK               9.0.306
SonarAnalyzer.CSharp   10.31.0.145097
Microsoft.CodeAnalysis.CSharp  4.11.0
Microsoft.Unity.Analyzers      from Unity-generated Assembly-CSharp.csproj
OS                     Windows 11 Home 10.0.26200
```

---

## 4. Analysis scripts

Aggregation, matched-population and category analysis. **Action: recover and
version them.** They were written in a session scratchpad and are not currently
in the repository; the results they produced are, but the scripts are not. This
is the weakest link in the package — the numbers are reproducible from the CSVs,
but not yet by running a committed script.

---

## 5. Validation suite

| Item | Path | Status |
|---|---|---|
| Tests | `Tools/characterization/Tests/*.cs` (12 files) | versioned |
| Runner | `Tools/characterization/testar.ps1` | versioned |
| Project | `Tools/characterization/Molz.Characterization.csproj` | versioned |
| **`.trx` for C / D0 / D** | — | **MISSING** |

Executed 2026-10-03 on .NET SDK 9.0.306, each state in a detached worktree:

```
C   (0a869bc1)   Passed: 68   Failed: 0
D0  (a788ea8)    Passed: 68   Failed: 0
D   (934eea6)    Passed: 118  Failed: 0
```

**Action:** re-run with `--logger "trx;LogFileName=<state>.trx"` and version the
three files under `Tools/characterization/evidence/`. `TestResults/` is
currently in `.gitignore`, so add an exception for that folder.

---

## 6. Records

| Item | Path |
|---|---|
| State freeze | `docs/paper/STATE-D-FREEZE.md` |
| Results analysis | `docs/paper/RESULTS-ANALYSIS-FINAL.md` |
| Service-locator ADR | `docs/paper/ADR-service-locator-cross-scene-lifetime.md` |
| Scope firewall | `docs/paper/PAPER1-PAPER2-SCOPE.md` |
| Provenance and definitions | `docs/paper_1_ES_arch_prism/PROVENANCE.md` |
| Claim traceability | `docs/paper_1_ES_arch_prism/TRACEABILITY.md` |

`TRACEABILITY.md` is an internal audit artefact. It may go in the deposit as
supplementary material but must not be merged into the manuscript.

---

## 7. Licensing — **BLOCKER**

**There is no `LICENSE` file in the repository.** A public GitHub repository
without a licence is "all rights reserved" by default, which blocks a Zenodo
deposit that invites reuse.

**Action:** choose and add a licence before archiving. The code and the research
artefacts may warrant different ones (for example a permissive code licence plus
CC BY for the data and records).

---

## 8. Third-party material — must not be redistributed

`Assets/` beyond `Scripts/` contains LeanTween, TextMesh Pro, TutorialInfo,
Adaptive Performance, fonts, GUI and image assets under their own terms.

**Recommended deposit shape:** `Assets/Scripts` at the three pinned commits plus
`Assets/Resources/Data`, the instrument, the measurements, the analysis scripts,
the test suite and the records. Not a clone of the whole Unity project. That is
sufficient to re-derive every reported number and keeps the deposit free of
third-party material.

---

## 9. Excluded from this package

| Item | Reason |
|---|---|
| 103-participant evaluation spreadsheet | Human-subject data belonging to the separate user study; needs its own consent and anonymisation basis |
| PIBIC report `.docx`, research project `.pdf` | Institutional documents, not research artefacts of this paper |
| `v1-sbc-template/` | Pre-migration archive, superseded |

---

## 10. Zenodo

| Step | Status |
|---|---|
| Sanitise SARIF | **TODO** |
| Add LICENSE | **TODO** |
| Recover and version analysis scripts | **TODO** |
| Generate and version `.trx` | **TODO** |
| Push branches and tags, or bundle trees | **TODO** |
| Assemble deposit | **TODO** |
| Mint DOI | **TODO** |
| Insert DOI into the manuscript's Data availability statement | **TODO** |

The manuscript currently states that the artefacts "will be archived in a
public, DOI-minted snapshot for submission". That sentence is accurate only
until submission; it must become a real DOI before the paper goes out.

---

## Status

```
Reproducibility   BLOCKER
```

Blocking items: LICENSE absent · SARIF unsanitised · analysis scripts not
versioned · `.trx` not generated · D0 and D not reachable publicly · no DOI.
