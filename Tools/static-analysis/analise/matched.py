"""RQ2 matched-population analysis, D0 -> D.

An aggregate mean confounds two things: change inside components that exist in
both states, and change in which components are counted. This script separates
them by matching on a stable identity key and recomputing over the
intersection only. Reproduces Table 7 of the manuscript.
"""
import common as c


def matched_members():
    m0, a0 = c.unique_by(c.members('D0'), c.member_key)
    md, ad = c.unique_by(c.members('D'), c.member_key)
    return m0, md, a0, ad, set(m0) & set(md)


def matched_types(group=None):
    t0 = c.types('D0')
    td = c.types('D')
    if group:
        t0 = [x for x in t0 if c.group(x['arquivo']) == group]
        td = [x for x in td if c.group(x['arquivo']) == group]
    d0 = {c.type_key(x): x for x in t0}
    dd = {c.type_key(x): x for x in td}
    return d0, dd, set(d0) & set(dd)


if __name__ == '__main__':
    m0, md, a0, ad, mt = matched_members()

    print('=' * 68)
    print('MEMBERS, SYSTEM-WIDE')
    print('=' * 68)
    print('  population D0        : %d  (%d ambiguous rows excluded)' % (len(c.members('D0')), len(a0)))
    print('  population D         : %d  (%d ambiguous rows excluded)' % (len(c.members('D')), len(ad)))
    print('  matched              : %d' % len(mt))
    print('  removed              : %d' % len(set(m0) - set(md)))
    print('  added                : %d' % len(set(md) - set(m0)))
    print()
    print('  excluded ambiguous keys (overloads, no parameter signature in the key):')
    for k in sorted({c.member_key(x) for x in a0}):
        print('     ' + k.split('|', 1)[1])

    agg0 = c.mean([c.fnum(x, 'maintainability_index') for x in c.members('D0')])
    aggd = c.mean([c.fnum(x, 'maintainability_index') for x in c.members('D')])
    mat0 = c.mean([c.fnum(m0[k], 'maintainability_index') for k in mt])
    matd = c.mean([c.fnum(md[k], 'maintainability_index') for k in mt])
    cagg0 = c.mean([c.fnum(x, 'cc') for x in c.members('D0')])
    caggd = c.mean([c.fnum(x, 'cc') for x in c.members('D')])
    cmat0 = c.mean([c.fnum(m0[k], 'cc') for k in mt])
    cmatd = c.mean([c.fnum(md[k], 'cc') for k in mt])

    print()
    print('  %-26s %8s %8s %9s' % ('basis', 'D0', 'D', 'delta'))
    print('  ' + '-' * 54)
    for lbl, a, b in (('MI, aggregate mean', agg0, aggd),
                      ('MI, matched members', mat0, matd),
                      ('CC, aggregate mean', cagg0, caggd),
                      ('CC, matched members', cmat0, cmatd)):
        print('  %-26s %8.2f %8.2f %+9.2f' % (lbl, a, b, b - a))

    changed_mi = sum(1 for k in mt
                     if c.fnum(m0[k], 'maintainability_index') != c.fnum(md[k], 'maintainability_index'))
    changed_cc = sum(1 for k in mt if c.fnum(m0[k], 'cc') != c.fnum(md[k], 'cc'))
    print()
    print('  members whose MI changed at all : %d of %d (%.1f%%)'
          % (changed_mi, len(mt), 100.0 * changed_mi / len(mt)))
    print('  members whose CC changed at all : %d of %d (%.1f%%)'
          % (changed_cc, len(mt), 100.0 * changed_cc / len(mt)))

    print()
    print('=' * 68)
    print('TYPES, DIRECTLY REFACTORED GROUP')
    print('=' * 68)
    d0, dd, surv = matched_types('DIRECT')
    removed = set(d0) - set(dd)
    ac = lambda d, ks: c.mean([c.fnum(d[k], 'acoplamento_eferente') for k in ks])
    print('  types in D0            : %d' % len(d0))
    print('  types in D             : %d' % len(dd))
    print('  matched surviving      : %d' % len(surv))
    print('  removed                : %d  %s'
          % (len(removed), sorted(k.split('|')[1] for k in removed)))
    print()
    print('  coupling proxy, matched (n=%d) : %.2f -> %.2f   delta %+.2f'
          % (len(surv), ac(d0, surv), ac(dd, surv), ac(dd, surv) - ac(d0, surv)))
    print('  coupling proxy, raw group mean : %.2f -> %.2f   delta %+.2f'
          % (ac(d0, d0), ac(dd, dd), ac(dd, dd) - ac(d0, d0)))
    print('  coupling proxy of removed types: %.2f' % ac(d0, removed))

    ups = sum(1 for k in surv
              if c.fnum(dd[k], 'acoplamento_eferente') > c.fnum(d0[k], 'acoplamento_eferente'))
    dns = sum(1 for k in surv
              if c.fnum(dd[k], 'acoplamento_eferente') < c.fnum(d0[k], 'acoplamento_eferente'))
    print('  survivors up / down / unchanged: %d / %d / %d' % (ups, dns, len(surv) - ups - dns))

    print()
    print('=' * 68)
    print('ENGINE-DEPENDENT SET (direct MonoBehaviour/ScriptableObject inheritance)')
    print('=' * 68)
    us = lambda s: {c.type_key(x) for x in c.types(s)
                    if (x.get('unity') or '').strip() not in ('', '0')}
    a, b = us('D0'), us('D')
    print('  D0: %d   D: %d   intersection: %d' % (len(a), len(b), len(a & b)))
    print('  only in D0: %s' % (sorted(k.split('|')[1] for k in a - b) or '[] (empty)'))
    print('  only in D : %s' % (sorted(k.split('|')[1] for k in b - a) or '[] (empty)'))
