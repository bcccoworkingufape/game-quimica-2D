# Paper 1 — Submission Checklist

**Target:** Software Quality Journal (Springer Nature)
**Manuscript:** `paper_sqj/main.tex` · **Class:** `sn-jnl.cls` v3.1 (December 2024)
**Last verified:** 2026-10-03

---

## Formatting

| Item | Status | Detail |
|---|---|---|
| Class | **READY** | `\documentclass[pdflatex,sn-basic]{sn-jnl}` |
| Reference style | **READY** | `sn-basic.bst` — Basic Springer Nature style, **Namedate** (author-year) mode, which is the form SQJ articles use. Add `Numbered` to the class options if the journal asks for numbered. |
| Bibliography source | **READY** | `paper1-references.bib`, 32 entries, unchanged by the migration |
| Previous template removed | **READY** | zero occurrences of `sbc2019`, `JSERD`, `10.5753`, `fontawesome`, `ccicons`, `fontspec`, `ifsym`, `frontmatter` |
| Compilation | **READY** | `cd paper_sqj && latexmk -pdf main.tex` → exit 0, **0 errors**, 0 undefined citations, 0 undefined references |
| Output | **READY** | `main.pdf`, 32 pages |
| Remaining overfull boxes | **READY** | 8, largest 14.3 pt — cosmetic; Springer re-typesets |
| Single `.tex` file | **READY** | no `\input`; figures are inline TikZ, so no external artwork to attach |

### Files to submit

Everything in `paper_sqj/`, which builds standalone:

```
paper_sqj/main.tex                 manuscript
paper_sqj/paper1-references.bib    bibliography
paper_sqj/main.bbl                 resolved bibliography
paper_sqj/main.pdf                 compiled output
paper_sqj/sn-jnl.cls               class (only if Editorial Manager asks)
paper_sqj/sn-basic.bst             style (idem)
```

`main.bbl` is included deliberately: Springer does not always run BibTeX, and
without it the 32 citations render as `[?]`.

`springer-template/` holds the upstream reference material and `v1-sbc-template/`
the pre-migration archive; neither is submitted. `PROVENANCE.md`,
`TRACEABILITY.md` and the two checklists are internal artefacts and are **not**
submitted.

---

## Scientific content

| Item | Status | Detail |
|---|---|---|
| Title | **READY** | Architecture-focused, unchanged through the migration |
| Abstract | **READY** | 227 words (journal band 150–250; internal target 210–235) |
| Keywords | **READY** | 6: Software architecture · Architectural refactoring · Software quality · Software metrics · Game software engineering · Unity |
| Research questions | **READY** | RQ1/RQ2/RQ3 unchanged since freeze |
| Experimental states | **READY** | C `9b9816d`, D0 `a788ea8`, D `934eea6` — all present, all unchanged |
| Numeric results | **READY** | 23 key values verified present; multiset check shows no experimental number lost in the migration |
| Tables | **READY** | 9 (8 `table` + 1 `longtable`); the RQ1 responsibility table became a `longtable` so all 13 decisions survive the page break |
| Figures | **READY** | 2 inline TikZ: experimental design, architecture C vs D |
| Engine-dependency definition | **READY** | direct inheritance from `MonoBehaviour`/`ScriptableObject`; zero occurrences of the superseded wording |
| Coupling terminology | **READY** | "approximate efferent coupling" / "structural coupling proxy"; both `$C_e$` mentions are disclaimers |
| Threats to validity | **READY** | 9 items, each with mitigation and residual risk |
| Prohibited claims | **READY** | zero occurrences of *improved software quality*, *no previous study*, *State E*, *p-value*, *statistically significant*, LLM/AI mentions |

---

## Administrative metadata — **BLOCKER**

Ten `NEEDS SOURCE` markers remain in `paper_sqj/main.tex`. None can be filled without you.

| Field | What is missing |
|---|---|
| Author list and order | Provisional: Rezende, Rocha. Confirm who else belongs and in what order. |
| Corresponding author | Currently marked on the first author; confirm. |
| E-mail addresses | First author's e-mail is `NEEDS-SOURCE@ufape.edu.br`. The personal address previously in the file was removed as inappropriate for a submission. |
| ORCID identifiers | Absent for both authors. Springer expects at least the corresponding author's. |
| Affiliation detail | Only institution, city, state and country are filled. `\orgdiv` (academic unit), street and postcode are missing. |
| Funding | Programme and grant identifiers. |
| Author contributions | CRediT statements per author. |
| Acknowledgements | Definitive list of acknowledged contributors. |

Competing interests, ethics approval and data availability **are** written.

---

## Journal policy items not yet verified

The SQJ submission-guidelines page sits behind an authentication gate and could
not be read from this environment. Before submitting, confirm from an
authenticated session:

- accepted article types and any length limit;
- whether SQJ wants `sn-basic` in Namedate or Numbered mode;
- figure and table technical requirements;
- the current declarations block required at submission.

Springer Nature's general policy requiring that use of an LLM be documented in
the Methods section was verified and is recorded in `PAPER1-PAPER2-SCOPE.md`;
the disclosure was removed from the manuscript at the author's instruction and
is the author's call to reinstate.

---

## Status summary

```
Scientific scope           READY
Novelty positioning        READY
Study design               READY
Results                    READY
Discussion                 READY
Threats                    READY
Journal formatting         READY
Reproducibility            BLOCKER   (see REPLICATION_CHECKLIST.md)
Administrative metadata    BLOCKER   (10 NEEDS SOURCE markers)
```
