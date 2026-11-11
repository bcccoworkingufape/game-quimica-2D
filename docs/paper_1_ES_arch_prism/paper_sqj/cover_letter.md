# Cover letter — Software Quality Journal

> Plain-text draft for Editorial Manager. Items in [AUTHOR: ...] require the
> author's decision or data before submission; delete the markers afterwards.

---

Dear Editors of the Software Quality Journal,

Please consider the enclosed manuscript, **"Architectural Simplification and
Internal Software Quality in a Unity-Based Serious Game: A Comparative Case
Study"**, for publication as an empirical case study.

## What the paper contributes

Architectural refactoring is usually studied as the addition of structure;
removing abstractions that no longer carry a responsibility has received
little empirical attention, and the reading of before-and-after metrics when
the intervention itself changes the set of measured components has received
less. The study reports a responsibility-driven architectural simplification
of Molz, a released Unity-based serious game, measured across three pinned
states with one frozen instrument: a baseline, a dead-code-removal control,
and the simplified state. Its three contributions are methodological as much
as empirical: (i) a control checkpoint that separates population cleanup from
architectural change — in this system the removed dead code was unrepresentative
enough that deleting it alone moved every aggregate mean; (ii) a
matched-population analysis showing that composition, not the surviving code,
set the direction of the aggregate maintainability and complexity movements;
and (iii) a record of which abstractions were removed and which retained,
including two rejected a-priori hypotheses, under a verified
behaviour-preservation protocol.

## Fit to the journal

The paper works squarely within the journal's own lines of inquiry: internal
quality indicators and their limits (Baggen et al., SQJ 2012; Stevenson and
Wood, SQJ 2018), static-analysis-based quality measurement (Berciu et al.,
SQJ 2026; Rantala et al., SQJ 2024), maintainability assessment and prediction
(Şengül and Yürüm, SQJ 2026), refactoring (Baqais and Alshayeb, SQJ 2020;
Yang et al., SQJ 2026), quality trade-offs in practice (Sas and Avgeriou,
SQJ 2020), and serious games as maintainable software (Cerioli et al.,
SQJ 2026). Nine of its references are Software Quality Journal articles.

[AUTHOR: optional — keep or delete this paragraph.] The study may also suit
the journal's open collection *"Software Quality in an AI-Driven World"*: the
intervention was implemented with an LLM-based coding assistant under a
staged, per-stage human-approval protocol, disclosed in the Methods section in
line with Springer Nature policy. The paper's measurement caution — that an
intervention which changes the measured population can reverse the sign of
aggregate quality indicators — applies directly to the evaluation of
AI-assisted refactoring, where interventions become cheap to produce and
their assessment becomes the bottleneck.

## Originality and related manuscripts

The manuscript is not published and is not under consideration elsewhere. Two
related outputs exist and are declared for transparency:

- A separate manuscript, currently under review at a multimedia venue
  (WebMedia 2026), reports a **user evaluation with 103 participants** of the
  same game. Its research questions, data and claims are disjoint from this
  study's; the present manuscript cites that evaluation only as context and
  draws no evidence from it. [AUTHOR: update the status of the WebMedia
  manuscript — under review / accepted / published — before submitting.]
- An institutional undergraduate research report (PIBIC, in Portuguese,
  non-archival) describes the engineering cycle that produced the baseline
  state. No results are reused from it.

## Reproducibility

All three states are pinned by commit, annotated tag and frozen branch in the
public repository (https://github.com/bcccoworkingufape/game-quimica-2D). The
replication package — source snapshots of the three states, the frozen
measurement instrument, the raw per-type and per-member records, the analysis
and figure scripts, the characterization test suite with execution logs, and
the study records — is archived at [AUTHOR: Zenodo DOI]. A versioned
verification script re-derives all 142 published values from the raw records,
and the three result figures are generated from the same code path.

## Declarations

An LLM-based coding agent assisted the implementation of the studied
intervention under per-stage human review and approval; this is documented in
the Methods section, examined in Threats to Validity, and stated in the
Declarations, per Springer Nature policy on AI use. The authors declare no
competing interests. The study analyses source code only and involves no human
participants.

Corresponding author: [AUTHOR: name, affiliation, e-mail].

[AUTHOR: optional — suggested reviewers, if Editorial Manager asks.]

Thank you for your consideration.

Sincerely,
[AUTHOR: names]
