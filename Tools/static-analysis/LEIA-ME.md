# Análise Estática do Molz

Mede a qualidade estrutural do código de três versões do jogo e gera as tabelas usadas
no relatório final do PIBIC.

---

## Como rodar

```powershell
cd C:\UnityProjects\bcc\game-quimica-2D\Tools\static-analysis
powershell -ExecutionPolicy Bypass -File .\analisar.ps1
```

Leva cerca de 4 minutos. Ao final, leia **`resultados/COMPARATIVO.md`** — é o arquivo com
todas as tabelas prontas.

### Variações

| Comando | O que faz |
|---|---|
| `.\analisar.ps1` | Tudo: as três versões, as duas ferramentas, e o comparativo |
| `.\analisar.ps1 -Apenas 2D` | Só a versão atual |
| `.\analisar.ps1 -Apenas 3D` | Só a versão tridimensional |
| `.\analisar.ps1 -Apenas 2Dpre` | Só a versão anterior à reestruturação |
| `.\analisar.ps1 -SemSonar` | Só as métricas (mais rápido, dispensa a Unity) |
| `.\analisar.ps1 -SemMetricas` | Só as violações de regras |

### Pré-requisitos

- **.NET SDK 8+**, **Node.js** e **Git** no `PATH`
- O projeto 2D precisa ter sido aberto na Unity **ao menos uma vez**, para existirem
  `Assembly-CSharp.csproj` e `Library\ScriptAssemblies\`. É de lá que o script extrai as
  308 referências de montagem necessárias para compilar o código fora do editor.
  Sem isso, use `-SemMetricas` desligado e `-SemSonar` ligado.

---

## O que é medido

### 1. Violações de regras

Executadas durante a compilação, por dois analisadores Roslyn:

- **SonarAnalyzer.CSharp 10.31** — pacote oficial da SonarSource, com o mesmo conjunto de regras
  que o SonarQube aplica a C#. Reporta identificadores `S####`.
- **Microsoft.Unity.Analyzers** — regras específicas para projetos Unity, já declarado pela própria
  Unity no `Assembly-CSharp.csproj`. Reporta identificadores `UNT####`.

O resultado sai em formato **SARIF**, o padrão da indústria para relatórios de análise estática.

### 2. Métricas agregadas

Calculadas por uma rotina própria sobre as árvores sintáticas do Roslyn, porque os analisadores
de regras reportam violações mas não computam métricas por membro:

| Métrica | Fonte |
|---|---|
| Complexidade Ciclomática | McCABE (1976) |
| Volume | HALSTEAD (1977) |
| Índice de Manutenibilidade | OMAN; HAGEMEISTER (1992), variante Microsoft, escala 0–100 |
| LOC, aninhamento, acoplamento eferente, acoplamento à engine | contagem direta |

---

## Versões comparadas

| | Repositório / commit | Papel |
|---|---|---|
| **A — 3D** | `game-quimica`, `ChemistryLab/Assets/Scripts` | versão tridimensional anterior |
| **B — 2D pré-MVP** | `game-quimica-2D`, commit `1226b59` (05/05/2026) | antes da reestruturação |
| **C — 2D atual** | `game-quimica-2D`, `HEAD` | versão atual |

**A × C** compara dois produtos diferentes. **B × C** é um estudo antes/depois no mesmo produto e
isola o efeito da reestruturação — é a comparação metodologicamente mais forte.

A versão B é recuperada automaticamente com `git worktree` em uma pasta temporária e removida ao
final. Sua árvore de trabalho não é alterada.

---

## Estrutura da pasta

```text
Tools/static-analysis/
├── LEIA-ME.md                    <- este arquivo
├── analisar.ps1                  <- único script a executar
│
├── ferramentas/                  <- não precisa mexer
│   ├── CodeMetrics/              rotina Roslyn de métricas (C#)
│   ├── genproj.js                monta o projeto de compilação isolado
│   ├── sarif.js                  lê o SARIF e filtra o código próprio
│   └── comparativo.js            consolida as três versões
│
└── resultados/
    ├── COMPARATIVO.md            <- LEIA ESTE: todas as tabelas
    ├── 3D/
    ├── 2D-pre-mvp/
    └── 2D-atual/
        ├── sonar.sarif           relatório bruto, padrão SARIF
        ├── sonar-regras.csv      violações agrupadas por regra
        ├── sonar-resumo.txt      resumo legível
        ├── metricas-resumo.txt   resumo das métricas
        ├── metricas-metodos.csv  uma linha por método
        └── metricas-tipos.csv    uma linha por classe
```

---

## Notas de método

**Bibliotecas de terceiros são compiladas, mas não entram no relatório.** LeanTween, TextMesh Pro e
QuickOutline são distribuídos como código-fonte e precisam ser compilados para que o código próprio
resolva seus tipos. O `sarif.js` os exclui na filtragem, junto com `TutorialInfo` e
`EasyLoadingScreen`.

**A análise não roda em integração contínua.** É uma execução pontual e reprodutível por linha de
comando. O plano de trabalho previa monitoramento contínuo, o que exigiria um pipeline de CI não
implantado no período.

**Não há métricas de cobertura**, pois dependem de testes automatizados, que não existem em nenhum
dos dois repositórios.

**A complexidade ciclomática total não é comparável entre versões**, porque cada membro contribui
com no mínimo uma unidade e as versões têm números diferentes de membros. Use a média, o percentual
de membros com CC > 10 e o Índice de Manutenibilidade, que são normalizados.
