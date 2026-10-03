"""Sanitise the analyzer SARIF for publication.

Two problems in the raw output:

1. Absolute local paths that carry the operating-system user name, e.g.
   file:///C:/Users/<name>/AppData/Local/Temp/molz-estado-C/Assets/...
   Every artifactLocation URI is rewritten to a repository-relative path.

2. Results over third-party source. LeanTween, QuickOutline and
   EasyLoadingScreen are compiled because the project needs them, but they are
   not part of the measured system and are excluded from every reported count.
   The raw SARIF still carries their findings; they are dropped here.

Writes <state>/sonar-sanitised.sarif next to the original. The original is left
untouched so the pipeline output stays auditable.

Usage:  python sanitize_sarif.py
"""
import json
import os
import re
import common as c

THIRD_PARTY = ('leantween', 'quickoutline', 'easyloadingscreen',
               'textmesh pro', 'tutorialinfo')

# the worktree roots the pipeline used, in URI form
ROOT_RE = re.compile(r'^file:///.*?/molz-estado-[A-Za-z0-9]+/', re.I)
ANY_ABS = re.compile(r'^file:///.*?/(Assets/)', re.I)


def rel(uri):
    """Repository-relative path for an artifact URI."""
    u = uri.replace('\\', '/')
    u = ROOT_RE.sub('', u)
    u = ANY_ABS.sub(r'\1', u)
    return u.lstrip('/')


def locations(result):
    for loc in result.get('locations', []):
        al = loc.get('physicalLocation', {}).get('artifactLocation')
        if al:
            yield al
    for rel_loc in result.get('relatedLocations', []):
        al = rel_loc.get('physicalLocation', {}).get('artifactLocation')
        if al:
            yield al


def sanitise(state):
    src = os.path.join(c.RESULTS, c.STATES[state], 'sonar.sarif')
    dst = os.path.join(c.RESULTS, c.STATES[state], 'sonar-sanitised.sarif')
    with open(src, encoding='utf-8') as f:
        doc = json.load(f)

    kept, dropped, rewritten = 0, 0, 0
    for run in doc.get('runs', []):
        out = []
        for res in run.get('results', []):
            uris = [al.get('uri', '') for al in locations(res)]
            if any(t in u.lower() for u in uris for t in THIRD_PARTY):
                dropped += 1
                continue
            for al in locations(res):
                before = al.get('uri', '')
                after = rel(before)
                if after != before:
                    al['uri'] = after
                    rewritten += 1
            out.append(res)
            kept += 1
        run['results'] = out

        # artifact table, when present, carries the same absolute paths
        arts = []
        for art in run.get('artifacts', []) or []:
            u = art.get('location', {}).get('uri', '')
            if any(t in u.lower() for t in THIRD_PARTY):
                continue
            if u:
                art['location']['uri'] = rel(u)
            arts.append(art)
        if 'artifacts' in run:
            run['artifacts'] = arts

        # invocation records can hold the working directory
        for inv in run.get('invocations', []) or []:
            inv.pop('workingDirectory', None)
            inv.pop('commandLine', None)
            inv.pop('arguments', None)

    with open(dst, 'w', encoding='utf-8') as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)

    blob = json.dumps(doc)
    leaks = re.findall(r'file:///[A-Za-z]:|/Users/|/home/|AppData', blob)
    return kept, dropped, rewritten, len(leaks), os.path.getsize(dst)


if __name__ == '__main__':
    total_leaks = 0
    for s in ('C', 'D0', 'D'):
        kept, dropped, rew, leaks, size = sanitise(s)
        total_leaks += leaks
        print('  %-3s kept=%-5d dropped(3rd party)=%-4d uris rewritten=%-5d '
              'residual absolute paths=%d  (%d KB)'
              % (s, kept, dropped, rew, leaks, size // 1024))
    print()
    print('  residual absolute paths across all three: %d  -> %s'
          % (total_leaks, 'CLEAN' if total_leaks == 0 else 'STILL LEAKING'))
