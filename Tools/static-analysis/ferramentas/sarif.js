// Le um SARIF do SonarAnalyzer e resume as violacoes do codigo proprio.
const fs = require('fs');
const path = process.argv[2];
const label = process.argv[3] || path;
const includeDir = (process.argv[4] || 'Assets/Scripts').toLowerCase().replace(/\\/g, '/');
const excludes = ['leantween', 'textmesh pro', 'tutorialinfo', 'quickoutline', 'easyloadingscreen'];

const sarif = JSON.parse(fs.readFileSync(path, 'utf8'));
const run = sarif.runs[0];

// mapa de regras -> descricao e severidade
const rules = {};
const driver = run.tool && run.tool.driver;
for (const r of (driver && driver.rules) || []) {
  rules[r.id] = {
    name: r.shortDescription ? r.shortDescription.text : (r.name || ''),
    sev: (r.defaultConfiguration && r.defaultConfiguration.level) || 'warning',
    tags: (r.properties && r.properties.tags) || [],
  };
}

const byRule = {}, byFile = {};
let total = 0;
for (const res of run.results || []) {
  const loc = res.locations && res.locations[0];
  const uri = loc && loc.physicalLocation && loc.physicalLocation.artifactLocation
    ? decodeURIComponent(loc.physicalLocation.artifactLocation.uri) : '';
  const u = uri.toLowerCase().replace(/\\/g, '/');
  if (!u.includes(includeDir)) continue;
  if (excludes.some(e => u.includes(e))) continue;
  const id = res.ruleId;
  total++;
  byRule[id] = (byRule[id] || 0) + 1;
  const short = uri.replace(/\\/g, '/').split(includeDir).pop().replace(/^\//, '');
  byFile[short] = (byFile[short] || 0) + 1;
}

const cat = { bug: 0, 'code-smell': 0, vulnerability: 0, outro: 0 };
for (const id in byRule) {
  const t = (rules[id] && rules[id].tags) || [];
  const s = t.join(',').toLowerCase();
  if (s.includes('bug')) cat.bug += byRule[id];
  else if (s.includes('vulnerability') || s.includes('security')) cat.vulnerability += byRule[id];
  else if (s.includes('code-smell') || s.includes('convention') || s.includes('design') || s.includes('clumsy') || s.includes('suspicious')) cat['code-smell'] += byRule[id];
  else cat.outro += byRule[id];
}

const out = [];
const W = s => { out.push(s); console.log(s); };
W('===== SonarAnalyzer.CSharp 10.31 — ' + label + ' =====');
W('violacoes no codigo proprio (' + includeDir + '): ' + total);
W('regras distintas acionadas                  : ' + Object.keys(byRule).length);
W('');
W('por categoria (tags da regra):');
W('  bug           : ' + cat.bug);
W('  code smell    : ' + cat['code-smell']);
W('  vulnerability : ' + cat.vulnerability);
W('  nao rotulado  : ' + cat.outro);
W('');
// ordenacao deterministica: quantidade desc, depois id da regra asc
const porQtd = (a, b) => b[1] - a[1] || a[0].localeCompare(b[0], 'en');

W('TOP 20 regras mais acionadas:');
Object.entries(byRule).sort(porQtd).slice(0, 20).forEach(([id, n]) => {
  const r = rules[id] || {};
  W('  ' + String(n).padStart(4) + 'x  ' + id.padEnd(7) + ' ' + (r.name || '').slice(0, 92));
});
W('');
W('TOP 15 arquivos com mais violacoes:');
Object.entries(byFile).sort(porQtd).slice(0, 15).forEach(([f, n]) => {
  W('  ' + String(n).padStart(4) + 'x  ' + f);
});

// Reprodutibilidade: o Roslyn emite os diagnosticos em ordem nao deterministica
// (compilacao paralela). Reescreve o SARIF com os achados ordenados, para que duas
// execucoes do mesmo codigo produzam arquivos byte a byte identicos.
const chave = (x) => {
  const l = (x.locations || [])[0];
  const pl = l && l.physicalLocation;
  const uri = pl && pl.artifactLocation ? pl.artifactLocation.uri : '';
  const ln = pl && pl.region ? pl.region.startLine || 0 : 0;
  const col = pl && pl.region ? pl.region.startColumn || 0 : 0;
  return [x.ruleId || '', uri, String(ln).padStart(7, '0'), String(col).padStart(5, '0')].join('|');
};
if (run.results) run.results.sort((a, b) => chave(a).localeCompare(chave(b), 'en'));
if (driver && driver.rules) driver.rules.sort((a, b) => (a.id || '').localeCompare(b.id || '', 'en'));

// Remove telemetria de tempo de execucao (medicao de relogio, varia a cada execucao
// e nao descreve nenhum achado).
if (run.properties) {
  delete run.properties.analyzerExecutionTime;
  if (Object.keys(run.properties).length === 0) delete run.properties;
}
JSON.stringify(sarif, (k, v) => {
  if (v && typeof v === 'object' && !Array.isArray(v)) {
    delete v.executionTimeInSeconds;
    delete v.executionTimeInPercentage;
  }
  return v;
});
fs.writeFileSync(process.argv[2], JSON.stringify(sarif, null, 2), 'utf8');

fs.writeFileSync(path.replace(/\.sarif$/, '-resumo.txt'), out.join('\n'), 'utf8');
const csv = ['regra;ocorrencias;descricao'].concat(
  Object.entries(byRule).sort(porQtd)
    .map(([id, n]) => id + ';' + n + ';' + ((rules[id] && rules[id].name) || '').replace(/;/g, ','))
).join('\n');
fs.writeFileSync(path.replace(/\.sarif$/, '-regras.csv'), csv, 'utf8');

