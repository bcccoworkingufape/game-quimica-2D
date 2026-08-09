# Comparativo da Análise Estática

> Gerado automaticamente por `analisar.ps1`. Não editar à mão.
> Ferramentas: SonarAnalyzer.CSharp 10.31 (SonarSource) · Microsoft.Unity.Analyzers · Roslyn.

| Versão | Identificação | Papel na comparação |
|---|---|---|
| **A** | `game-quimica`, `ChemistryLab/Assets/Scripts` | versão tridimensional anterior |
| **B** | `game-quimica-2D`, commit `1226b59` (05/05/2026) | 2D antes da reestruturação MVP |
| **C** | `game-quimica-2D`, `HEAD` | 2D atual |

A comparação **A × C** contrasta dois produtos. A comparação **B × C** é um estudo antes/depois no
mesmo produto e isola o efeito da reestruturação.

## 1. Violações de regras

| Origem da regra | A: 3D | B: 2D pré | C: 2D atual |
|---|---:|---:|---:|
| SonarSource (`S####`) | **111** | **52** | **49** |
| Microsoft.Unity.Analyzers (`UNT####`) | 46 | 84 | 100 |
| Compilador C# (`CS####`) | 67 | 48 | 48 |
| **Total** | **224** | **184** | **197** |
| Regras distintas acionadas | 26 | 19 | 18 |
| Linhas de código efetivo | 5590 | 4522 | 5093 |
| **Violações SonarSource por KLOC** | **19.9** | **11.5** | **9.6** |

Redução da densidade de violações SonarSource: **51.5%** de A para C, e **16.3%** de B para C.

### Regras de maior contraste

| Regra | Descrição | A: 3D | B: 2D pré | C: 2D atual |
|---|---|---:|---:|---:|
| `S125` | Trechos de código comentados | 31 | 2 | 0 |
| `S2094` | Classes vazias | 12 | 0 | 0 |
| `S2325` | Métodos que não usam dados de instância deveriam ser estáticos | 36 | 22 | 21 |
| `S3903` | Tipos deveriam estar em namespaces nomeados | 7 | 7 | 7 |
| `S1135` | Uso de marcações TODO | 4 | 3 | 3 |
| `UNT0008` | Null propagation em objetos Unity | 4 | 78 | 93 |
| `UNT0026` | GetComponent sempre aloca | 10 | 2 | 3 |

## 2. Tamanho, complexidade e acoplamento

| Métrica | A: 3D | B: 2D pré | C: 2D atual |
|---|---:|---:|---:|
| Arquivos `.cs` | 81 | 97 | 91 |
| Linhas de código efetivo | 5590 | 4522 | 5093 |
| Densidade de comentários (%) | 3.9 | 10.4 | 12.0 |
| Classes | 75 | 52 | 79 |
| **Interfaces** | **1** | **9** | **16** |
| **Classes acopladas à Unity** | **58  (77.3% das classes)** | **22  (42.3% das classes)** | **24  (30.4% das classes)** |
| Membros analisados | 433 | 398 | 496 |
| Complexidade ciclomática média | 2.00 | 2.44 | 2.33 |
| Complexidade ciclomática máxima | 19 | 17 | 17 |
| Membros com CC > 10 | 6  (1.4%) | 5  (1.3%) | 6  (1.2%) |
| **Índice de Manutenibilidade médio** | **67.5** | **69.9** | **72.5** |
| Índice de Manutenibilidade mediano | 68.0 | 67.5 | 71.2 |
| **Membros com MI ≥ 65** | **237  (54.7%)** | **227  (57.0%)** | **323  (65.1%)** |
| LOC média por membro | 11.0 | 9.7 | 8.3 |
| **Membros com aninhamento ≥ 4** | **45** | **15** | **14** |
| **Acoplamento eferente médio** | **13.4** | **13.1** | **11.4** |

## 3. Onde estão os dados brutos

- **3D (ChemistryLab)** → `resultados/3D/`
- **2D pré-reestruturação (1226b59)** → `resultados/2D-pre-mvp/`
- **2D atual** → `resultados/2D-atual/`

Cada pasta contém: `metricas-resumo.txt`, `metricas-metodos.csv`, `metricas-tipos.csv`,
`sonar-resumo.txt`, `sonar-regras.csv` e `sonar.sarif`.
