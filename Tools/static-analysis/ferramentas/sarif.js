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
W('TOP 20 regras mais acionadas:');
Object.entries(byRule).sort((a, b) => b[1] - a[1]).slice(0, 20).forEach(([id, n]) => {
  const r = rules[id] || {};
  W('  ' + String(n).padStart(4) + 'x  ' + id.padEnd(7) + ' ' + (r.name || '').slice(0, 92));
});
W('');
W('TOP 15 arquivos com mais violacoes:');
Object.entries(byFile).sort((a, b) => b[1] - a[1]).slice(0, 15).forEach(([f, n]) => {
  W('  ' + String(n).padStart(4) + 'x  ' + f);
});

fs.writeFileSync(path.replace(/\.sarif$/, '-resumo.txt'), out.join('\n'), 'utf8');
const csv = ['regra;ocorrencias;descricao'].concat(
  Object.entries(byRule).sort((a, b) => b[1] - a[1])
    .map(([id, n]) => id + ';' + n + ';' + ((rules[id] && rules[id].name) || '').replace(/;/g, ','))
).join('\n');
fs.writeFileSync(path.replace(/\.sarif$/, '-regras.csv'), csv, 'utf8');

