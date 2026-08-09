// Consolida os resultados das tres versoes em resultados/COMPARATIVO.md
const fs = require('fs');
const path = require('path');

const base = path.resolve(__dirname, '..', 'resultados');
const alvos = [
  { dir: '3D', nome: '3D (ChemistryLab)', col: 'A: 3D' },
  { dir: '2D-pre-mvp', nome: '2D pré-reestruturação (1226b59)', col: 'B: 2D pré' },
  { dir: '2D-atual', nome: '2D atual', col: 'C: 2D atual' },
];

function lerResumo(dir) {
  const p = path.join(base, dir, 'metricas-resumo.txt');
  if (!fs.existsSync(p)) return {};
  const txt = fs.readFileSync(p, 'utf8');
  const get = (rot) => {
    const m = txt.match(new RegExp('^' + rot.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + '\\s*:\\s*(.+)$', 'm'));
    return m ? m[1].trim() : '—';
  };
  return {
    arquivos: get('arquivos analisados'),
    loc: get('linhas de codigo (sem branco)'),
    coment: get('linhas de comentario'),
    dens: get('densidade de comentarios (%)'),
    classes: get('classes'),
    interfaces: get('interfaces'),
    unity: get('tipos acoplados a Unity'),
    membros: get('membros analisados'),
    ccMedia: get('CC media'),
    ccMax: get('CC maxima'),
    ccAlta: get('membros com CC > 10'),
    miMedio: get('Maintainability Index medio'),
    miMediano: get('MI mediano'),
    miBom: get('membros com MI >= 65 (bom)'),
    locMembro: get('LOC media por membro'),
    aninh: get('membros com aninhamento >= 4'),
    acopl: get('acoplamento eferente medio'),
  };
}

function lerSonar(dir) {
  const p = path.join(base, dir, 'sonar-regras.csv');
  if (!fs.existsSync(p)) return { S: 0, U: 0, C: 0, tot: 0, regras: 0, mapa: {} };
  const rows = fs.readFileSync(p, 'utf8').split('\n').slice(1).filter(Boolean).map(l => l.split(';'));
  let S = 0, U = 0, C = 0; const mapa = {};
  for (const [id, n] of rows) {
    const q = parseInt(n, 10); mapa[id] = q;
    if (/^S\d+$/.test(id)) S += q; else if (/^UNT\d+$/.test(id)) U += q; else if (/^CS\d+$/.test(id)) C += q;
  }
  return { S, U, C, tot: S + U + C, regras: rows.length, mapa };
}

const M = {}, Q = {};
for (const a of alvos) { M[a.dir] = lerResumo(a.dir); Q[a.dir] = lerSonar(a.dir); }

const num = s => { const m = String(s).match(/-?[\d.,]+/); return m ? parseFloat(m[0].replace(',', '.')) : NaN; };
const linha = (rot, get) => '| ' + rot + ' | ' + alvos.map(a => get(a.dir)).join(' | ') + ' |';
const kloc = d => num(M[d].loc) / 1000;

const out = [];
const W = s => out.push(s);

W('# Comparativo da Análise Estática');
W('');
W('> Gerado automaticamente por `analisar.ps1`. Não editar à mão.');
W('> Ferramentas: SonarAnalyzer.CSharp 10.31 (SonarSource) · Microsoft.Unity.Analyzers · Roslyn.');
W('');
W('| Versão | Identificação | Papel na comparação |');
W('|---|---|---|');
W('| **A** | `game-quimica`, `ChemistryLab/Assets/Scripts` | versão tridimensional anterior |');
W('| **B** | `game-quimica-2D`, commit `1226b59` (05/05/2026) | 2D antes da reestruturação MVP |');
W('| **C** | `game-quimica-2D`, `HEAD` | 2D atual |');
W('');
W('A comparação **A × C** contrasta dois produtos. A comparação **B × C** é um estudo antes/depois no');
W('mesmo produto e isola o efeito da reestruturação.');
W('');
W('## 1. Violações de regras');
W('');
W('| Origem da regra | ' + alvos.map(a => a.col).join(' | ') + ' |');
W('|---|' + alvos.map(() => '---:').join('|') + '|');
W(linha('SonarSource (`S####`)', d => '**' + Q[d].S + '**'));
W(linha('Microsoft.Unity.Analyzers (`UNT####`)', d => Q[d].U));
W(linha('Compilador C# (`CS####`)', d => Q[d].C));
W(linha('**Total**', d => '**' + Q[d].tot + '**'));
W(linha('Regras distintas acionadas', d => Q[d].regras));
W(linha('Linhas de código efetivo', d => M[d].loc));
W(linha('**Violações SonarSource por KLOC**', d => '**' + (Q[d].S / kloc(d)).toFixed(1) + '**'));
W('');
const dS = (Q['3D'].S / kloc('3D') - Q['2D-atual'].S / kloc('2D-atual')) / (Q['3D'].S / kloc('3D')) * 100;
const dS2 = (Q['2D-pre-mvp'].S / kloc('2D-pre-mvp') - Q['2D-atual'].S / kloc('2D-atual')) / (Q['2D-pre-mvp'].S / kloc('2D-pre-mvp')) * 100;
W('Redução da densidade de violações SonarSource: **' + dS.toFixed(1) + '%** de A para C, e **' +
  dS2.toFixed(1) + '%** de B para C.');
W('');
W('### Regras de maior contraste');
W('');
W('| Regra | Descrição | ' + alvos.map(a => a.col).join(' | ') + ' |');
W('|---|---|' + alvos.map(() => '---:').join('|') + '|');
const desc = {
  S125: 'Trechos de código comentados', S2094: 'Classes vazias',
  S2325: 'Métodos que não usam dados de instância deveriam ser estáticos',
  S3903: 'Tipos deveriam estar em namespaces nomeados',
  UNT0008: 'Null propagation em objetos Unity', UNT0026: 'GetComponent sempre aloca',
  S1135: 'Uso de marcações TODO',
};
for (const r of ['S125', 'S2094', 'S2325', 'S3903', 'S1135', 'UNT0008', 'UNT0026']) {
  W('| `' + r + '` | ' + (desc[r] || '') + ' | ' + alvos.map(a => Q[a.dir].mapa[r] || 0).join(' | ') + ' |');
}
W('');
W('## 2. Tamanho, complexidade e acoplamento');
W('');
W('| Métrica | ' + alvos.map(a => a.col).join(' | ') + ' |');
W('|---|' + alvos.map(() => '---:').join('|') + '|');
W(linha('Arquivos `.cs`', d => M[d].arquivos));
W(linha('Linhas de código efetivo', d => M[d].loc));
W(linha('Densidade de comentários (%)', d => M[d].dens));
W(linha('Classes', d => M[d].classes));
W(linha('**Interfaces**', d => '**' + M[d].interfaces + '**'));
W(linha('**Classes acopladas à Unity**', d => '**' + M[d].unity + '**'));
W(linha('Membros analisados', d => M[d].membros));
W(linha('Complexidade ciclomática média', d => M[d].ccMedia));
W(linha('Complexidade ciclomática máxima', d => M[d].ccMax));
W(linha('Membros com CC > 10', d => M[d].ccAlta));
W(linha('**Índice de Manutenibilidade médio**', d => '**' + M[d].miMedio + '**'));
W(linha('Índice de Manutenibilidade mediano', d => M[d].miMediano));
W(linha('**Membros com MI ≥ 65**', d => '**' + M[d].miBom + '**'));
W(linha('LOC média por membro', d => M[d].locMembro));
W(linha('**Membros com aninhamento ≥ 4**', d => '**' + M[d].aninh + '**'));
W(linha('**Acoplamento eferente médio**', d => '**' + M[d].acopl + '**'));
W('');
W('## 3. Onde estão os dados brutos');
W('');
for (const a of alvos) {
  W('- **' + a.nome + '** → `resultados/' + a.dir + '/`');
}
W('');
W('Cada pasta contém: `metricas-resumo.txt`, `metricas-metodos.csv`, `metricas-tipos.csv`,');
W('`sonar-resumo.txt`, `sonar-regras.csv` e `sonar.sarif`.');

fs.writeFileSync(path.join(base, 'COMPARATIVO.md'), out.join('\n') + '\n', 'utf8');
console.log(out.join('\n'));
