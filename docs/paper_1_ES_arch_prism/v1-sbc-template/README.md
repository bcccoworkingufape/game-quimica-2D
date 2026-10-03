# Paper 1 — Version 1 archive (SBC / JSERD template)

Frozen record of the manuscript as it stood **before** the migration to the
Springer Nature template. Nothing here is submitted anywhere, and nothing here
is on the build path of the current paper.

The live manuscript is `../paper_sqj/main.tex`, built with `sn-jnl.cls`.

---

## What this is

Version 1 was written on `sbc2019.cls`, the SBC/JSERD journal class. The
scientific content of v1 and of the current version is the same: the migration
in commit `9287c8c` changed structure and metadata only, and the numeric audit
confirmed that no experimental value, research question, table figure or
experimental state was altered by it.

`main-v1.tex` is the manuscript at commit `9f104d3`, which is the last state
before the migration. It already contains every scientific correction made
during the review stages — the engine-dependency definition, the coupling
proxy naming, the four-strand Related Work, the nine threats, the 227-word
abstract and the six keywords.

## Contents

| File | Size | Role in v1 |
|---|---:|---|
| `main-v1.tex` | 100 KB | the manuscript at `9f104d3` |
| `sbc2019.cls` | 5.4 KB | the journal class |
| `academicons.sty` | 11 KB | ORCID icon package |
| `academicons.tex` | 78 KB | icon definitions |
| `academicons.ttf` | 48 KB | icon font |
| `tuacademicons.fd` | 465 B | font descriptor for the above |
| `aas_macros.sty` | 5.0 KB | astronomy journal macros (`\aap`, `\apj`, …) |
| `pasa-mnras.bst` | 44 KB | astronomy bibliography style |
| `fpo.eps` / `fpo.pdf` | 98 KB | placeholder figure |

The last four shipped inside the SBC template package but were never used by
this manuscript: it has no `\includegraphics`, cites no astronomy journal
macro, and uses `apalike` rather than `pasa-mnras`. They are kept here for
archival fidelity, not because v1 needed them.

## Not included

`1r_lamboo_notes.bib` — 563 KB of 969 astronomy bibliography entries that came
with the template and that this paper never cited. It was removed in commit
`b414108` and deliberately left out of this archive. To restore it:

```bash
git show b414108^:docs/paper_1_ES_arch_prism/1r_lamboo_notes.bib \
  > docs/paper_1_ES_arch_prism/v1-sbc-template/1r_lamboo_notes.bib
```

## Building v1

```bash
cd v1-sbc-template
cp ../paper_sqj/paper1-references.bib .
latexmk -xelatex main-v1.tex
```

The bibliography is not duplicated here: v1 and the current version cite the
same 32 references, so the archive borrows the file from `paper_sqj/`.

**XeLaTeX is required** — the preamble loads `fontspec`. `pdflatex` will fail.

Known issue: in some installations `fontspec` cannot resolve the font named
`FontAwesome` even when `FontAwesome.otf` is present, which aborts the build.
That defect is one of the reasons the Springer template drops `fontawesome`
altogether. The current `main.tex` builds cleanly with `pdflatex`.

## History

| Commit | What changed |
|---|---|
| `b414108` | dropped the unused astronomy leftovers |
| `0dc9631` | dropped `academicons` when the ORCIDs became `NEEDS SOURCE` |
| `9f104d3` | last v1 state — stages 3–5 of the scientific closure |
| `9287c8c` | migration to `sn-jnl` |
| `f2f5418` | dropped `sbc2019.cls` from the working tree |
