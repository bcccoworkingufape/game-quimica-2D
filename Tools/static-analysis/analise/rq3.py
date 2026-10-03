"""RQ3 distribution of change across component categories.

Reproduces Tables 8 and 9 of the manuscript. A component absent from a state is
reported as absent, never as zero: substituting zero would inject artificial
values into every mean.
"""
import common as c


def file_counts(state):
    files = {c.norm(x['arquivo']) for x in c.types(state)}
    return {g: sum(1 for f in files if c.group(f) == g) for g in c.ORDER}


def group_metrics(state, g):
    t = [x for x in c.types(state) if c.group(x['arquivo']) == g]
    m = [x for x in c.members(state) if c.group(x['arquivo']) == g]
    if not t and not m:
        return None
    cc = [c.fnum(x, 'cc') for x in m]
    mi = [c.fnum(x, 'maintainability_index') for x in m]
    return {
        'types': len(t), 'members': len(m),
        'loc': int(sum(c.fnum(x, 'loc') for x in t)),
        'cc': round(c.mean(cc), 2) if cc else None,
        'mi': round(c.mean(mi), 1) if mi else None,
        'coupling': round(c.mean([c.fnum(x, 'acoplamento_eferente') for x in t]), 2),
        'unity': sum(1 for x in t if (x.get('unity') or '').strip() not in ('', '0')),
    }


if __name__ == '__main__':
    print('=' * 72)
    print('FILE DISTRIBUTION BY CATEGORY   (- = absent, not zero)')
    print('=' * 72)
    fc = {s: file_counts(s) for s in ('C', 'D0', 'D')}
    print('  %-28s %6s %6s %6s' % ('Category', 'C', 'D0', 'D'))
    print('  ' + '-' * 48)
    for g in c.ORDER:
        row = []
        for s in ('C', 'D0', 'D'):
            n = fc[s][g]
            row.append(str(n) if n else ('-' if g in ('DEAD', 'NEW') else '0'))
        print('  %-28s %6s %6s %6s' % (c.LABELS[g], row[0], row[1], row[2]))
    print('  %-28s %6d %6d %6d' % ('TOTAL',
                                   *[len({c.norm(x['arquivo']) for x in c.types(s)})
                                     for s in ('C', 'D0', 'D')]))

    print()
    print('=' * 72)
    print('GROUP METRICS')
    print('=' * 72)
    hdr = ('Group', 'Types', 'Memb', 'LOC', 'CC', 'MI', 'Coupl', 'Engine')
    print('  %-30s %6s %6s %7s %6s %6s %7s %7s' % hdr)
    print('  ' + '-' * 76)

    for g in ('DIRECT', 'INDIRECT', 'PRESERVED', 'NEW'):
        for s in ('D0', 'D'):
            v = group_metrics(s, g)
            if v is None:
                continue
            print('  %-30s %6d %6d %7d %6s %6s %7.2f %7d'
                  % (c.LABELS[g][:24] + ', ' + s, v['types'], v['members'], v['loc'],
                     ('%.2f' % v['cc']) if v['cc'] is not None else 'n/a',
                     ('%.1f' % v['mi']) if v['mi'] is not None else 'n/a',
                     v['coupling'], v['unity']))

    print()
    print('  control, C -> D0:')
    v = group_metrics('C', 'DEAD')
    print('  %-30s %6d %6d %7d %6.2f %6.1f %7.2f %7s'
          % ('DEAD CODE REMOVED (C only)', v['types'], v['members'], v['loc'],
             v['cc'], v['mi'], v['coupling'], '-'))
    tc, mc = c.types('C'), c.members('C')
    print('  %-30s %6d %6d %7s %6.2f %6.1f %7.2f %7s'
          % ('whole project in C', len(tc), len(mc), '-',
             c.mean([c.fnum(x, 'cc') for x in mc]),
             c.mean([c.fnum(x, 'maintainability_index') for x in mc]),
             c.mean([c.fnum(x, 'acoplamento_eferente') for x in tc]), '-'))

    print()
    print('  Note: the PRESERVED group is DEFINED by the absence of relevant source')
    print('  changes, so its metric identity across D0->D follows partly from that')
    print('  definition. It is evidence of containment, not an independent')
    print('  validation of the classification and not a causal attribution.')
