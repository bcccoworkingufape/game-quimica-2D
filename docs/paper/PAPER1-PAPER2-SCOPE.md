# Paper 1 / Paper 2 — Scope Firewall

**Status:** BINDING · **Date:** 2026-09-04 · Molz research programme

Both papers draw on the same measurement run over the same three pinned states
of the same system. That shared provenance is what makes an overlap easy to
commit by accident, and an overlap between two manuscripts from one dataset is
a self-plagiarism and salami-slicing risk. This document fixes which claims
belong to which paper, and — more importantly — which claims each paper is
**forbidden** to make.

The rule in one line:

> **Paper 1 owns the architecture and the metric aggregates. Paper 2 owns the
> analyzer findings and their semantics. Neither may cross into the other's
> evidence to support its own conclusion.**

---

## Shared substrate (belongs to neither exclusively)

These may be described in both papers, because both need them to be
intelligible. They must be described **as context**, not as a result.

| Artefact | Note |
|---|---|
| The Molz system and its domain | Both may describe the game in a paragraph |
| States C (`9b9816d`), D0 (`a788ea8`), D (`934eea6`) | Both may name them; only Paper 1 may use them as a comparison design |
| The measurement instrument and its versions | Both may describe it |
| Unity 6000.4.1f1 / .NET SDK 9.0.306 environment | Both |
| The replication package | One package serves both |

Whichever paper is published second cites the first for the shared substrate
rather than restating it at length.

---

## Paper 1 — Architectural Simplification and Internal Software Quality

**Target:** Software Quality Journal
**Unit of contribution:** what a responsibility-driven simplification did to the
structure and to the internal quality indicators of a functionally preserved
system.

### Includes

- The C / D0 / D comparative design, and the argument for the D0 control.
- The responsibility-driven decision rule and the eight-stage protocol.
- The thirteen architectural concerns: evidence, decision, consequence.
- KEEP decisions reported as outcomes.
- The two rejected a-priori hypotheses (`ServiceLocator`,
  `TreeSolventGroupButtons`) and the four mechanisms of abstraction utility.
- Structural size: files, LOC, classes, interfaces, members.
- Maintainability index, cyclomatic complexity, efferent coupling.
- Engine dependency **as a set of types referencing `UnityEngine`**.
- Matched-population analysis.
- RQ3 component categories and their distribution.
- Behavioural preservation: the 68-test baseline, its growth to 118, the
  architecture invariant tests reported separately, engine import, manual flow.

### Explicit exclusions — Paper 1 may NOT

- Interpret the semantics of any individual analyzer finding.
- Classify findings as actionable, context-dependent or false positive.
- Name a specific rule (`UNT0008`, `CS0649`, `S2190`) as evidence for an
  architectural claim.
- Argue that fewer findings means better architecture.
- Report inter-rater agreement of any kind.

The analyzer counts (C 197, D0 191, D 143) appear **once**, labelled auxiliary
pipeline output, with an explicit statement that interpreting them belongs to a
separate study. That single sentence is the entire permitted footprint.

---

## Paper 2 — Context-Sensitive Static Analysis in Unity-Based Software

**Unit of contribution:** what an analyzer finding means in an engine-based
system, and how reliably that meaning can be judged.

### Includes

- The finding population and its distribution by rule and by file.
- Finding semantics: what each rule asserts and under what assumption.
- The actionable / context-dependent / non-actionable classification.
- Unity-specific semantics that invalidate a general-purpose rule's assumption
  (serialized fields, lifecycle, editor-assigned references).
- `UNT0008` and its decomposition (93 occurrences, 45 real DIP violations).
- `CS0649` as an editor-context false positive.
- `S2190` and the remaining rules in scope.
- Independent evaluators, the rating protocol, Cohen's $\kappa$, the prevalence
  effect on $\kappa$, and disagreement resolution.

### Explicit exclusions — Paper 2 may NOT

- Re-evaluate the architecture of C, D0 or D.
- Use the finding counts to argue that D is architecturally better than C.
- Report MI, CC or efferent coupling as its own results.
- Reuse the matched-population analysis.
- Reuse the RQ3 component classification as an analytic instrument.
- Claim behavioural preservation as its own contribution.

Paper 2 may state that the states differ architecturally **and cite Paper 1**
for what that difference is. It may not derive the difference itself.

---

## The boundary case: `UNT0008`

This is the one place the two papers genuinely touch, and where an overlap is
most likely to slip in.

`UNT0008` flags null-propagation on a Unity object. In Paper 1's terms, 45 of
the 93 occurrences are Dependency Inversion violations — which is an
architectural fact. In Paper 2's terms, the same 93 occurrences are a
population to be classified, and the 45/93 split is the *result* of applying
the classification protocol.

**Allocation.** The decomposition belongs to **Paper 2**. Paper 1 must not
present the 45/93 split, because deriving it requires exactly the
finding-by-finding semantic judgement Paper 1 is forbidden to make.

Paper 1 reaches the same architectural conclusion by a different route it does
own: 56 concrete call sites to the engine audio singletons, counted by source
scanning and reduced to zero, verified by an architecture invariant test. That
is a structural measurement, not a finding interpretation, and it is the form
in which the audio-boundary result appears in Paper 1.

---

## Terminology that must not drift

| Term | Paper 1 meaning | Paper 2 meaning |
|---|---|---|
| *finding* | not used as evidence | the unit of analysis |
| *violation* | avoid; use "concrete access site" | an analyzer's assertion |
| *engine dependency* | a set of types referencing `UnityEngine` | not used |
| *context-dependent* | not used | a finding classification |
| *actionable* | not used | a finding classification |

---

## Static structure versus runtime behaviour

Both papers must respect this distinction, and Paper 1 already does.

`56 direct access sites → 0` is a **static structural** result: source-level
call sites counted by scanning the tree. It is **not** a claim that 56 runtime
calls became zero. The runtime call count was never measured, and no sentence
in either paper may imply it was.

The same applies to any future statement about "number of calls", "number of
dependencies" or "number of accesses": name the unit, and name whether it was
counted statically or observed at runtime.

---

## Future studies — recorded, not executed

Neither is part of Paper 1, and neither may be retrofitted into it.

### LLM-based readability or maintainability assessment

Adding an AI-produced readability or maintainability rating to Paper 1 now
would introduce a **new dependent variable defined after the results were
seen**, which is a serious internal-validity problem regardless of how the
rating is produced. It also carries construct problems of its own: model
dependence, non-determinism, prompt sensitivity, ordering effects, prior
exposure to the repository, version drift, and no established construct
validity against human judgement.

A defensible protocol would require, at minimum: C and D presented blind with
state names stripped; randomized order; a frozen prompt and rubric; multiple
models or multiple runs; fixed sampling configuration where the provider allows
it; evaluation over equivalent units; repeated ratings; a reported
agreement/reliability figure; and comparison against human raters or an
external criterion. That is a separate study with its own design, not a late
metric appended to this one.

### Runtime performance

Execution time, frame rate, memory and runtime call counts are excluded for the
same reason. The Paper 1 interventions were not designed as a performance
intervention, no hypothesis was registered, and no controlled benchmark exists.
Adding performance now would be a second construct measured without a protocol.

If pursued, it needs a pre-registered hypothesis, a controlled benchmark
harness, a defined workload, repeated runs with variance reported, and a
statement of what a difference would mean given that neither state was tuned
for performance.

---

## Enforcement

Before either manuscript is submitted, re-read it against the exclusion list
for that paper. A sentence that would need the other paper's evidence to be
true is a scope violation, and the fix is to cite the other paper rather than
to import the evidence.
