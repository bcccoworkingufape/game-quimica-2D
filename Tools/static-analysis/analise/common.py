"""Shared loading and component classification for the Paper 1 analyses.

Definitions are those recorded in docs/paper_1_ES_arch_prism/PROVENANCE.md.
Nothing here recomputes a metric: the per-type and per-member CSVs produced by
the frozen instrument are the inputs, and these scripts only aggregate them.
"""
import csv
import os
import statistics as st

# repository root, two levels above Tools/static-analysis/analise
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
RESULTS = os.path.join(ROOT, 'Tools', 'static-analysis', 'resultados')

STATES = {'C': 'estado-C', 'D0': 'estado-D0', 'D': 'estado-D'}

COMMITS = {
    'C': '9b9816d319f5e60935edcb92dccb1e10e7506c37',
    'D0': 'a788ea8927d4c20afeba7d3b2e3e536d362025d2',
    'D': '934eea6aa8e732db57f4575379ba1acd1dd6b58f',
}

SEP = chr(92)


def read(state, name):
    """Read one CSV of a state. The instrument writes UTF-8 BOM, ';' separated."""
    path = os.path.join(RESULTS, STATES[state], name)
    with open(path, encoding='utf-8-sig', newline='') as f:
        return list(csv.DictReader(f, delimiter=';'))


def types(state):
    return read(state, 'metricas-tipos.csv')


def members(state):
    return read(state, 'metricas-metodos.csv')


def norm(path):
    """Windows separators in the CSV; compare on forward slashes."""
    return path.replace(SEP, '/')


# ---------------------------------------------------------------------------
# RQ3 component classification.
#
# Derived from the intervention history, not from the measured values: each
# list is the set of files the corresponding stage commits touched. The five
# labels map onto four population roles -- removed (DEAD), added (NEW),
# modified (DIRECT and INDIRECT) and preserved (PRESERVED).
# ---------------------------------------------------------------------------

DEAD = [
    'Domain/Entities/IChemicalSubstance.cs', 'Domain/Entities/PlayerProfile.cs',
    'Domain/Services/IQuestionService.cs', 'Domain/Services/QuestionService.cs',
    'Domain/States/GameStateMachine.cs', 'Domain/States/GameStates.cs',
    'Domain/States/IGameState.cs', 'Domain/UseCases/GenerateMysteryQuestionUseCase.cs',
    'Presentation/Common/SimpleSpriteAnimator.cs',
    'Presentation/Menu/DifficultySelectorPresenter.cs',
    'Presentation/Menu/DifficultySelectorView.cs',
]

DIRECT = [
    'Data/Dtos/QuestionDto.cs', 'Data/Mappers/CompoundMapper.cs',
    'Data/Mappers/QuestionMapper.cs', 'Data/Mappers/SolutionMapper.cs',
    'Data/Mappers/SolventMapper.cs', 'Data/Repositories/JsonCompoundRepository.cs',
    'Data/Repositories/JsonQuestionRepository.cs', 'Data/Repositories/JsonSolutionRepository.cs',
    'Data/Repositories/JsonSolventRepository.cs', 'Core/Audio/IAudioService.cs',
    'Core/Audio/UnityAudioService.cs', 'Core/GameManager.cs', 'Core/Bootstrap/Bootstrapper.cs',
    'Presentation/Menu/MenuPresenter.cs', 'Domain/Services/IScoringService.cs',
    'Domain/Services/ScoringService.cs', 'Domain/Entities/Solvent.cs',
    'Presentation/Lab/TestMessageHandler.cs', 'LabScripts/LabUIController.cs',
    'Presentation/Lab/MixingRoundController.cs', 'Presentation/Lab/QuestionFlowPresenter.cs',
    'MenuScripts/MenuUIController.cs', 'UI/NavbarController.cs',
    'Presentation/Lab/SolventClickHandler.cs',
]

INDIRECT = ['Domain/UseCases/SubmitAnswerUseCase.cs']

NEW = ['Presentation/Lab/MixingView.cs', 'Presentation/Lab/QuestionView.cs']

ORDER = ['DIRECT', 'INDIRECT', 'PRESERVED', 'DEAD', 'NEW']

LABELS = {
    'DEAD': 'DEAD CODE REMOVED',
    'DIRECT': 'DIRECTLY REFACTORED',
    'INDIRECT': 'INDIRECTLY AFFECTED',
    'PRESERVED': 'PRESERVED',
    'NEW': 'NEW INTERVENTION ARTIFACT',
}


def group(path):
    p = norm(path)
    for lst, name in ((DEAD, 'DEAD'), (DIRECT, 'DIRECT'),
                      (INDIRECT, 'INDIRECT'), (NEW, 'NEW')):
        if any(p.endswith(x) for x in lst):
            return name
    return 'PRESERVED'


# ---------------------------------------------------------------------------
# Identity keys for matching across states.
#
# The member key has no parameter signature, so overloads collapse onto one
# key. Ambiguous keys are EXCLUDED from matching rather than resolved by
# assumption; see PROVENANCE.md section 7 for the two affected overloads and
# the measured (nil) impact of excluding them.
# ---------------------------------------------------------------------------

def type_key(row):
    return norm(row['arquivo']) + '|' + row['tipo']


def member_key(row):
    return norm(row['arquivo']) + '|' + row['tipo'] + '|' + row['membro']


def unique_by(rows, keyfn):
    """Return (unique_rows_by_key, ambiguous_rows)."""
    counts = {}
    for r in rows:
        counts[keyfn(r)] = counts.get(keyfn(r), 0) + 1
    uniq = {keyfn(r): r for r in rows if counts[keyfn(r)] == 1}
    amb = [r for r in rows if counts[keyfn(r)] > 1]
    return uniq, amb


def fnum(row, col):
    return float(row[col])


def mean(xs):
    return st.mean(xs) if xs else 0.0


def median(xs):
    return st.median(xs) if xs else 0.0
