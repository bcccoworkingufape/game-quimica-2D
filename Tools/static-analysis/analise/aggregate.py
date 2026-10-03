"""RQ2 aggregate profile of states C, D0 and D.

Recomputes every summary value from the raw per-type and per-member records and
cross-checks it against the instrument's own metricas-resumo.txt. Reproduces
Table 6 of the manuscript.
"""
import re
import os
import common as c


def summary(state):
    path = os.path.join(c.RESULTS, c.STATES[state], 'metricas-resumo.txt')
    out = {}
    with open(path, encoding='utf-8-sig') as f:
        for line in f:
            m = re.match(r'^(.+?)\s*:\s*(.+)$', line.strip())
            if m:
                out[m.group(1).strip()] = m.group(2).strip()
    return out


def profile(state):
    t, m = c.types(state), c.members(state)
    cc = [c.fnum(x, 'cc') for x in m]
    mi = [c.fnum(x, 'maintainability_index') for x in m]
    ac = [c.fnum(x, 'acoplamento_eferente') for x in t]
    classes = sum(1 for x in t if x['especie'] == 'class')
    unity = sum(1 for x in t if (x.get('unity') or '').strip() not in ('', '0'))
    return {
        'files': len({c.norm(x['arquivo']) for x in t}),
        'classes': classes,
        'interfaces': sum(1 for x in t if x['especie'] == 'interface'),
        'structs': sum(1 for x in t if x['especie'] in ('struct', 'record')),
        'enums': sum(1 for x in t if x['especie'] == 'enum'),
        'members': len(m),
        'cc_mean': round(c.mean(cc), 2), 'cc_median': c.median(cc),
        'cc_max': int(max(cc)), 'cc_total': int(sum(cc)),
        'mi_mean': round(c.mean(mi), 1), 'mi_median': round(c.median(mi), 1),
        'mi_ge65': sum(1 for v in mi if v >= 65),
        'cc_gt10': sum(1 for v in cc if v > 10),
        'coupling_mean': round(c.mean(ac), 1),
        'unity': unity,
        'unity_pct': round(100.0 * unity / classes, 1),
    }


ROWS = [
    ('Source files', 'files'), ('Classes', 'classes'), ('Interfaces', 'interfaces'),
    ('Structs/records', 'structs'), ('Enums', 'enums'), ('Members', 'members'),
    ('CC mean', 'cc_mean'), ('CC median', 'cc_median'), ('CC max', 'cc_max'),
    ('CC total', 'cc_total'), ('MI mean', 'mi_mean'), ('MI median', 'mi_median'),
    ('Members MI >= 65', 'mi_ge65'), ('Members CC > 10', 'cc_gt10'),
    ('Coupling proxy mean', 'coupling_mean'),
    ('Engine-dependent types', 'unity'), ('  as % of classes', 'unity_pct'),
]

if __name__ == '__main__':
    p = {s: profile(s) for s in ('C', 'D0', 'D')}
    print('%-26s %>10s' % ('', ''))
    print('%-26s %10s %10s %10s' % ('Indicator', 'C', 'D0', 'D'))
    print('-' * 60)
    for label, key in ROWS:
        print('%-26s %10s %10s %10s' % (label, p['C'][key], p['D0'][key], p['D'][key]))

    # cross-check against the instrument's own summary
    print()
    bad = 0
    checks = [('arquivos analisados', 'files'), ('classes', 'classes'),
              ('interfaces', 'interfaces'), ('membros analisados', 'members'),
              ('CC media', 'cc_mean'), ('CC total (soma)', 'cc_total'),
              ('Maintainability Index medio', 'mi_mean'),
              ('acoplamento eferente medio', 'coupling_mean')]
    for s in ('C', 'D0', 'D'):
        rep = summary(s)
        for field, key in checks:
            raw = rep.get(field, '')
            mm = re.match(r'^([\d.]+)', raw)
            if not mm:
                continue
            if abs(float(mm.group(1)) - float(p[s][key])) > 0.051:
                print('MISMATCH %s %s: csv=%s resumo=%s' % (s, field, p[s][key], raw))
                bad += 1
    print('cross-check against metricas-resumo.txt: %s'
          % ('ALL MATCH' if bad == 0 else '%d mismatches' % bad))
