"""Result figures for the Paper 1 manuscript.

Every plotted value is derived through the same modules that verify_paper.py
validates (common, aggregate, matched, rq3); nothing is recomputed here and no
number is hard-coded. Rounding follows the convention of the table each figure
accompanies, so figure and table always print the same value.

Output: three vector PDFs in docs/paper_1_ES_arch_prism/paper_sqj/figures/.
Deterministic: fixed SOURCE_DATE_EPOCH and seeded jitter, so re-running
produces byte-identical files.
"""
import os

os.environ['SOURCE_DATE_EPOCH'] = '0'  # reproducible PDF metadata

import matplotlib
matplotlib.use('pdf')
import matplotlib.pyplot as plt
import random

import common as c
import aggregate
from matched import matched_members

OUT = os.path.join(c.ROOT, 'docs', 'paper_1_ES_arch_prism', 'paper_sqj', 'figures')

# sn-jnl: \textwidth = 372 pt, body 10 pt
TEXTWIDTH_IN = 372.0 / 72.27
GRAY = '#666666'
LIGHT = '#bbbbbb'

plt.rcParams.update({
    'font.family': 'serif',
    'font.serif': ['STIXGeneral', 'Times New Roman', 'DejaVu Serif'],
    'mathtext.fontset': 'stix',
    'font.size': 8,
    'axes.titlesize': 8,
    'axes.labelsize': 8,
    'xtick.labelsize': 8,
    'ytick.labelsize': 7.5,
    'axes.linewidth': 0.6,
    'xtick.major.width': 0.6,
    'ytick.major.width': 0.6,
    'pdf.fonttype': 42,
})


def despine(ax, keep=('left', 'bottom')):
    for side in ('top', 'right', 'left', 'bottom'):
        ax.spines[side].set_visible(side in keep)


# ---------------------------------------------------------------------------
# Figure 1 -- aggregate indicator trajectories across C, D0, D (Table 6 values)
# ---------------------------------------------------------------------------

def fig_trajectories():
    p = {s: aggregate.profile(s) for s in ('C', 'D0', 'D')}
    panels = [
        ('Maintainability index (mean)', 'mi_mean', '%.1f'),
        ('Cyclomatic complexity (mean)', 'cc_mean', '%.2f'),
        ('Coupling proxy (mean)', 'coupling_mean', '%.1f'),
    ]
    fig, axes = plt.subplots(1, 3, figsize=(TEXTWIDTH_IN, 1.95))
    for ax, (title, key, fmt) in zip(axes, panels):
        ys = [p[s][key] for s in ('C', 'D0', 'D')]
        ax.plot([0, 1], ys[:2], color=GRAY, lw=1.2, zorder=2)
        ax.plot([1, 2], ys[1:], color='black', lw=1.2, zorder=2)
        ax.scatter([0, 1, 2], ys, s=14, color='black', zorder=3)
        lo, hi = min(ys), max(ys)
        pad = max((hi - lo) * 0.65, 0.02)
        ax.set_ylim(lo - pad, hi + pad * 1.45)
        for x, y in zip((0, 1, 2), ys):
            ax.annotate(fmt % y, (x, y), textcoords='offset points',
                        xytext=(0, 5), ha='center', fontsize=7.5)
        ax.set_xlim(-0.35, 2.35)
        ax.set_xticks([0, 1, 2])
        ax.set_xticklabels(['C', 'D0', 'D'])
        ax.set_yticks([])
        ax.set_title(title, fontsize=8, pad=8)
        despine(ax, keep=('bottom',))
        ax.tick_params(axis='x', length=0)
        tr = ax.get_xaxis_transform()
        ax.annotate('control', (0.5, -0.17), xycoords=tr,
                    ha='center', fontsize=7, color=GRAY, style='italic')
        ax.annotate('intervention', (1.5, -0.17), xycoords=tr,
                    ha='center', fontsize=7, style='italic')
    fig.subplots_adjust(left=0.02, right=0.98, top=0.86, bottom=0.21, wspace=0.18)
    fig.savefig(os.path.join(OUT, 'fig-trajectories.pdf'))
    plt.close(fig)


# ---------------------------------------------------------------------------
# Figure 2 -- aggregate vs matched-population movement, D0 -> D (Table 7 values)
# ---------------------------------------------------------------------------

def fig_matched():
    m0, md, _a0, _ad, mt = matched_members()
    rows_d0 = c.members('D0')
    rows_d = c.members('D')

    def agg(rows, col):
        return c.mean([c.fnum(x, col) for x in rows])

    def mat(side, col):
        return c.mean([c.fnum(side[k], col) for k in mt])

    # Endpoints rounded exactly as Table 7 prints them.
    data = {
        'MI': [
            ('Aggregate mean', round(agg(rows_d0, 'maintainability_index'), 1),
             round(agg(rows_d, 'maintainability_index'), 1), '%.1f'),
            ('Matched members', round(mat(m0, 'maintainability_index'), 2),
             round(mat(md, 'maintainability_index'), 2), '%.2f'),
        ],
        'CC': [
            ('Aggregate mean', round(agg(rows_d0, 'cc'), 2),
             round(agg(rows_d, 'cc'), 2), '%.2f'),
            ('Matched members', round(mat(m0, 'cc'), 2),
             round(mat(md, 'cc'), 2), '%.2f'),
        ],
    }
    titles = {'MI': 'Maintainability index (mean)',
              'CC': 'Cyclomatic complexity (mean)'}

    fig, axes = plt.subplots(1, 2, figsize=(TEXTWIDTH_IN, 2.0))
    for ax, key in zip(axes, ('MI', 'CC')):
        series = data[key]
        for (label, y0, y1, fmt), x0, color, mfc in (
                (series[0], 0.0, GRAY, 'white'),
                (series[1], 1.6, 'black', 'black')):
            ax.annotate('', xy=(x0 + 0.52, y1), xytext=(x0, y0),
                        arrowprops=dict(arrowstyle='-|>', color=color,
                                        lw=1.2, shrinkA=4, shrinkB=0))
            ax.plot([x0], [y0], marker='o', ms=4.5, color=color,
                    mfc=mfc, mew=1.0, ls='none')
            ax.annotate(fmt % y0, (x0, y0), textcoords='offset points',
                        xytext=(-4, -2), ha='right', fontsize=7.5)
            ax.annotate(fmt % y1, (x0 + 0.52, y1), textcoords='offset points',
                        xytext=(6, -2), ha='left', fontsize=7.5)
            ax.annotate(label + ('\n($n$ = %d)' % len(mt) if 'Matched' in label else ''),
                        (x0 + 0.26, 1.06), xycoords=('data', 'axes fraction'),
                        ha='center', va='bottom', fontsize=7.5,
                        color=color)
        ys = [v for _l, y0, y1, _f in series for v in (y0, y1)]
        lo, hi = min(ys), max(ys)
        pad = max((hi - lo) * 0.8, 0.02)
        ax.set_ylim(lo - pad, hi + pad)
        ax.set_xlim(-0.75, 2.9)
        ax.set_xticks([0, 0.52, 1.6, 2.12])
        ax.set_xticklabels(['D0', 'D', 'D0', 'D'])
        ax.set_yticks([])
        ax.set_title(titles[key], fontsize=8, pad=26)
        despine(ax, keep=('bottom',))
        ax.tick_params(axis='x', length=0)
    fig.subplots_adjust(left=0.03, right=0.97, top=0.72, bottom=0.12, wspace=0.3)
    fig.savefig(os.path.join(OUT, 'fig-matched.pdf'))
    plt.close(fig)


# ---------------------------------------------------------------------------
# Figure 3 -- per-member MI distributions (RQ3 categories; Tables 8-9 context)
# ---------------------------------------------------------------------------

def _strip(ax, x, values, rng, color=GRAY):
    xs = [x + rng.uniform(-0.13, 0.13) for _ in values]
    ax.plot(xs, values, ls='none', marker='o', ms=2.2, mew=0,
            color=color, alpha=0.55, zorder=2)


def _box(ax, x, values):
    if len(values) < 10:
        return
    bp = ax.boxplot([values], positions=[x], widths=0.42, zorder=3,
                    showfliers=False, patch_artist=True,
                    medianprops=dict(color='black', lw=1.1),
                    boxprops=dict(facecolor='none', lw=0.7),
                    whiskerprops=dict(lw=0.7), capprops=dict(lw=0.7))
    for part in bp['boxes']:
        part.set_edgecolor('black')


def fig_rq3():
    rng = random.Random(1)

    def mi(state, grp=None, invert=False):
        rows = c.members(state)
        if grp is not None:
            keep = (lambda g: g != grp) if invert else (lambda g: g == grp)
            rows = [x for x in rows if keep(c.group(x['arquivo']))]
        return [c.fnum(x, 'maintainability_index') for x in rows]

    fig, (axa, axb) = plt.subplots(
        1, 2, figsize=(TEXTWIDTH_IN, 2.5), width_ratios=[1, 2.2])

    # Panel (a): state C -- members of removed dead code vs all other members
    dead = mi('C', 'DEAD')
    rest = mi('C', 'DEAD', invert=True)
    for x, vals in ((0, dead), (1, rest)):
        _strip(axa, x, vals, rng)
        _box(axa, x, vals)
    axa.set_xticks([0, 1])
    axa.set_xticklabels(['removed\ndead code\n($n$ = %d)' % len(dead),
                         'remaining\nmembers\n($n$ = %d)' % len(rest)])
    axa.set_title('(a) State C', fontsize=8)
    axa.set_ylabel('Maintainability index per member')

    # Panel (b): D0 vs D by RQ3 category. The NEW category (two interface
    # files) contributes no measured members and is therefore not plotted.
    groups = [('DIRECT', 'directly\nrefactored'),
              ('INDIRECT', 'indirectly\naffected'),
              ('PRESERVED', 'preserved')]
    ticks, labels = [], []
    base = 0.0
    for i, (grp, label) in enumerate(groups):
        base = i * 1.9
        for dx, state, color in ((0, 'D0', GRAY), (0.7, 'D', 'black')):
            vals = mi(state, grp)
            if not vals:
                continue
            _strip(axb, base + dx, vals, rng, color=color)
            _box(axb, base + dx, vals)
        ticks.append(base + 0.35)
        n0, nd = len(mi('D0', grp)), len(mi('D', grp))
        labels.append('%s\n($n$ = %s)' % (label, n0 if n0 == nd else '%d, %d' % (n0, nd)))
    axb.set_xticks(ticks)
    axb.set_xticklabels(labels)
    axb.set_xlim(-0.6, base + 0.7 + 0.6)
    axb.set_title('(b) D0 (grey, left) vs D (black, right)', fontsize=8)

    ylo = min(axa.get_ylim()[0], axb.get_ylim()[0])
    yhi = max(axa.get_ylim()[1], axb.get_ylim()[1])
    for ax in (axa, axb):
        ax.set_ylim(ylo, yhi)
        despine(ax)
        ax.tick_params(axis='x', length=0, labelsize=7)
        ax.grid(axis='y', color=LIGHT, lw=0.4, alpha=0.5)
        ax.set_axisbelow(True)
    axb.tick_params(axis='y', labelleft=False, length=0)
    fig.subplots_adjust(left=0.085, right=0.99, top=0.9, bottom=0.17, wspace=0.06)
    fig.savefig(os.path.join(OUT, 'fig-rq3.pdf'))
    plt.close(fig)


if __name__ == '__main__':
    os.makedirs(OUT, exist_ok=True)
    fig_trajectories()
    fig_matched()
    fig_rq3()
    for name in ('fig-trajectories.pdf', 'fig-matched.pdf', 'fig-rq3.pdf'):
        path = os.path.join(OUT, name)
        print('%s  %d bytes' % (path, os.path.getsize(path)))
