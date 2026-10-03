"""Verify every number reported in the manuscript against the raw records.

This is the reproducibility gate: it re-derives each published value from the
per-type and per-member CSVs and compares it with the figure written in the
paper. Any mismatch is a defect in the paper, not in the data.

Usage:  python verify_paper.py
Exit code 0 if every check passes, 1 otherwise.
"""
import sys
import common as c
import aggregate
import matched

FAILS = []


def chk(label, got, expected, tol=0.0051):
    ok = abs(float(got) - float(expected)) <= tol
    if not ok:
        FAILS.append((label, got, expected))
    print('  %s %-46s raw=%-10s paper=%s'
          % ('OK ' if ok else 'XX ', label, got, expected))


# --- Table 6: aggregate profile ------------------------------------------
PAPER_AGG = {
    'C':  dict(files=91, classes=79, interfaces=16, structs=7, enums=9, members=496,
               cc_mean=2.33, cc_median=2.0, cc_max=17, cc_total=1158,
               mi_mean=72.5, mi_median=71.2, mi_ge65=323, cc_gt10=6,
               coupling_mean=11.4, unity=24, unity_pct=30.4),
    'D0': dict(files=80, classes=65, interfaces=12, structs=5, enums=9, members=451,
               cc_mean=2.39, cc_median=2.0, cc_max=17, cc_total=1078,
               mi_mean=71.7, mi_median=70.2, mi_ge65=287, cc_gt10=6,
               coupling_mean=12.8, unity=23, unity_pct=35.4),
    'D':  dict(files=77, classes=60, interfaces=14, structs=5, enums=9, members=457,
               cc_mean=2.38, cc_median=2.0, cc_max=17, cc_total=1089,
               mi_mean=71.8, mi_median=70.9, mi_ge65=293, cc_gt10=5,
               coupling_mean=13.4, unity=23, unity_pct=38.3),
}

print('=' * 78)
print('TABLE 6 -- aggregate profile')
print('=' * 78)
for s in ('C', 'D0', 'D'):
    p = aggregate.profile(s)
    for k, v in PAPER_AGG[s].items():
        chk('%s %s' % (s, k), p[k], v, 0.051)

# --- Table 7: matched population -----------------------------------------
print()
print('=' * 78)
print('TABLE 7 -- matched population, D0 -> D')
print('=' * 78)
m0, md, a0, ad, mt = matched.matched_members()
chk('matched members', len(mt), 442)
chk('removed members', len(set(m0) - set(md)), 5)
chk('added members', len(set(md) - set(m0)), 11)
chk('ambiguous rows D0', len(a0), 4)
chk('ambiguous rows D', len(ad), 4)
chk('MI matched D0', round(c.mean([c.fnum(m0[k], 'maintainability_index') for k in mt]), 2), 71.87)
chk('MI matched D', round(c.mean([c.fnum(md[k], 'maintainability_index') for k in mt]), 2), 71.80)
chk('CC matched D0', round(c.mean([c.fnum(m0[k], 'cc') for k in mt]), 2), 2.41)
chk('CC matched D', round(c.mean([c.fnum(md[k], 'cc') for k in mt]), 2), 2.42)
chk('members changing MI', sum(1 for k in mt if c.fnum(m0[k], 'maintainability_index')
                               != c.fnum(md[k], 'maintainability_index')), 65)
chk('members changing CC', sum(1 for k in mt if c.fnum(m0[k], 'cc') != c.fnum(md[k], 'cc')), 12)

d0, dd, surv = matched.matched_types('DIRECT')
ac = lambda d, ks: c.mean([c.fnum(d[k], 'acoplamento_eferente') for k in ks])
chk('DIRECT types D0', len(d0), 24)
chk('DIRECT types D', len(dd), 19)
chk('matched surviving types', len(surv), 19)
chk('coupling matched D0', round(ac(d0, surv), 2), 26.84)
chk('coupling matched D', round(ac(dd, surv), 2), 28.37)
chk('coupling raw group D0', round(ac(d0, d0), 2), 22.29)
chk('coupling of removed types', round(ac(d0, set(d0) - set(dd)), 2), 5.00)

# --- engine-dependent set -------------------------------------------------
print()
print('=' * 78)
print('ENGINE-DEPENDENT SET')
print('=' * 78)
us = lambda s: {c.type_key(x) for x in c.types(s)
                if (x.get('unity') or '').strip() not in ('', '0')}
a, b = us('D0'), us('D')
chk('engine-dependent D0', len(a), 23)
chk('engine-dependent D', len(b), 23)
chk('intersection', len(a & b), 23)
chk('only in D0', len(a - b), 0)
chk('only in D', len(b - a), 0)

# --- Tables 8 and 9: RQ3 --------------------------------------------------
print()
print('=' * 78)
print('TABLES 8 and 9 -- RQ3')
print('=' * 78)
import rq3
PAPER_FILES = {'C': dict(DIRECT=24, INDIRECT=1, PRESERVED=55, DEAD=11, NEW=0),
               'D0': dict(DIRECT=24, INDIRECT=1, PRESERVED=55, DEAD=0, NEW=0),
               'D': dict(DIRECT=19, INDIRECT=1, PRESERVED=55, DEAD=0, NEW=2)}
for s in ('C', 'D0', 'D'):
    fc = rq3.file_counts(s)
    for g, v in PAPER_FILES[s].items():
        chk('%s files %s' % (s, g), fc[g], v)

PAPER_GROUPS = {
    ('DIRECT', 'D0'): (24, 199, 2357, 2.44, 71.2, 22.29, 8),
    ('DIRECT', 'D'): (19, 204, 2505, 2.43, 71.4, 28.37, 8),
    ('INDIRECT', 'D0'): (3, 11, 58, 1.36, 83.5, 7.00, 0),
    ('INDIRECT', 'D'): (3, 12, 67, 1.33, 84.0, 7.67, 0),
    ('PRESERVED', 'D0'): (64, 241, 3286, 2.40, 71.5, 9.56, 15),
    ('PRESERVED', 'D'): (64, 241, 3286, 2.40, 71.5, 9.56, 15),
    ('NEW', 'D'): (2, 0, 30, None, None, 0.50, 0),
    ('DEAD', 'C'): (20, 45, 356, 1.78, 80.3, 4.50, None),
}
for (g, s), exp in PAPER_GROUPS.items():
    v = rq3.group_metrics(s, g)
    chk('%s %s types' % (g, s), v['types'], exp[0])
    chk('%s %s members' % (g, s), v['members'], exp[1])
    chk('%s %s LOC' % (g, s), v['loc'], exp[2])
    if exp[3] is not None:
        chk('%s %s CC' % (g, s), v['cc'], exp[3])
        chk('%s %s MI' % (g, s), v['mi'], exp[4], 0.051)
    chk('%s %s coupling' % (g, s), v['coupling'], exp[5])
    if exp[6] is not None:
        chk('%s %s engine' % (g, s), v['unity'], exp[6])

print()
print('=' * 78)
if FAILS:
    print('RESULT: %d MISMATCH(ES)' % len(FAILS))
    for f in FAILS:
        print('   XX %s: raw=%s paper=%s' % f)
    sys.exit(1)
print('RESULT: ALL MATCH')
print('=' * 78)
