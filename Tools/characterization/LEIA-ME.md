# Testes de Caracterização — Molz, Paper 1

Fotografa o comportamento do **Estado C** antes da intervenção arquitetural, para que
cada etapa de C → D0 → D possa provar que não alterou comportamento observável.

Estes **não** são testes de especificação. Vários deles registram deliberadamente
comportamento defeituoso, porque o critério do experimento é `Behaviour(C) = Behaviour(D)`,
não "o código está correto".

---

## Como rodar

```powershell
cd C:\UnityProjects\bcc\game-quimica-2D\Tools\characterization
powershell -ExecutionPolicy Bypass -File .\testar.ps1
```

Ou direto: `dotnet test`. Leva poucos segundos.

| Comando | O que faz |
|---|---|
| `.\testar.ps1` | Roda a suíte e reporta paridade |
| `.\testar.ps1 -Detalhado` | Saída com um nome de teste por linha |
| `.\testar.ps1 -Listar` | Só lista os testes, sem executar |

**Pré-requisito:** .NET SDK 8+ no `PATH`. Não precisa de Unity, de licença nem de editor.

---

## Por que fora do Unity Test Framework

O UTF 1.6.0 está resolvido no projeto (transitivamente, via `com.unity.ide.visualstudio`),
mas não pode testar este código sem violar o protocolo do experimento:

1. Um assembly definido por `.asmdef` **não pode referenciar um assembly predefinido**.
   Os 91 arquivos de produção estão em `Assembly-CSharp`, que é predefinido.
2. Para o UTF enxergá-los, o código de produção teria que ir para um `.asmdef` próprio.
3. Isso muda a identidade de assembly dos 23 `MonoBehaviour`, o que reescreve
   `m_TargetAssemblyTypeName` em 63 bindings de `UnityEvent` gravados nas cenas
   (29 em `LabUIController`, 22 em `MenuUIController`, 10 em `TreeSolventGroupButtons`,
   2 em `WinSceneUIController`).
4. A seção 6 do protocolo proíbe exatamente isso.

A alternativa adotada compila os fontes **livres de `UnityEngine`** direto de
`Assets/Scripts`, sem copiar código e sem alterar nada dentro de `Assets/`.

Vantagens colaterais: roda headless, é reproduzível por linha de comando e não
depende de licença — o que serve à reprodutibilidade exigida pela seção 25.

---

## O que é compilado

51 dos 91 arquivos de produção, todos sem nenhuma referência a `UnityEngine`:

| Origem | Arquivos |
|---|---:|
| `Assets/Scripts/Domain/**` | 31 |
| `Assets/Scripts/Data/Dtos` | 4 |
| `Assets/Scripts/Data/Mappers` | 4 |
| `Assets/Scripts/Data/Repositories` | 8 |
| `Assets/Scripts/Data/Providers` (só `IJsonProvider` e `FileJsonProvider`) | 2 |
| `Assets/Scripts/Data/DataIndex.cs` | 1 |
| `Assets/Scripts/Core/QuestionRunTracker.cs` | 1 |

Deliberadamente **fora**, por serem os dois únicos arquivos de `Data/` que tocam a engine:
`Data/Providers/ResourcesJsonProvider.cs` (usa `Resources.Load`) e
`Data/DifficultyLevelData.cs` (herda `ScriptableObject`).

O `FileJsonProvider` — classificado como *útil* no Decision Gate justamente por
documentar a decisão do Android — é o que substitui o `ResourcesJsonProvider` nos testes.
Os dois leem o mesmo arquivo do mesmo diretório.

O dataset usado é o **real**: `Assets/Resources/Data/*.json`, sem fixtures nem cópias.

---

## Impacto no experimento: nenhum

O pipeline de métricas compila apenas `Assets/Scripts/**/*.cs`
(`Tools/static-analysis/ferramentas/genproj.js`, linha 62). `Tools/` está fora do escopo
de medição, então este projeto não altera nenhuma métrica de C, D0 ou D.

Nenhum arquivo dentro de `Assets/` foi criado, alterado ou removido. Os três arquivos
de 0 bytes em `Assets/Tests/` continuam intocados — mexer neles não faria diferença
para o UTF pelo motivo explicado acima.

---

## Cobertura

68 testes, mapeados aos IDs do Functional Parity Manifest do Decision Gate.

| Arquivo | Cobre | Testes |
|---|---|---:|
| `DataLoadingTests.cs` | F02, F03, F04, F05 | 18 |
| `MappingTests.cs` | políticas de conversão e de erro | 14 |
| `SolubilityTests.cs` | F06, F07, F08 | 9 |
| `GameplayRulesTests.cs` | F09, F11, F12, F21 | 14 |
| `SessionStateTests.cs` | F14, F15, `MixSolutionUseCase` | 13 |

`F07_AllEightyPairs_MatchAuthoredJson` é o **teste-âncora**: percorre os 80 pares de
`SolubilityData.json` e exige que o `SolubilityService` reproduza exatamente o valor
autorado. Se a intervenção quebrar a tabela de solubilidade, ele acusa.

### Defeitos registrados de propósito

Estes testes passam porque o Estado C se comporta assim. **Corrigir qualquer um deles
durante D é desvio e exige registro.**

| Teste | Comportamento fotografado |
|---|---|
| `CompoundMapper_UnknownState_SilentlyFallsBackToLiquid` | `"GAS"`, `"solid"` e `null` viram `LIQUID` sem aviso |
| `SolventMapper_UnknownFlaskType_Throws` | `flaskType` lança, `state` tolera — duas políticas de erro no mesmo método |
| `SolutionMapper_UnknownSolubilityResult_Throws` | Um typo no JSON derruba a inicialização do jogo inteira |
| `F03_Solvents_LitmusReagentIsIdentifiedOnlyByName` | Nada no domínio marca o Tornassol; `chemical_class` não discrimina |
| `F06_DerivedMixtureType_AgreesWithIgnoredJsonField` | O campo `mixtureType` do JSON é descartado e recalculado por heurística |
| `F12_Factory_AllocatesANewInstancePerCall` | `GameModeRulesFactory` aloca a cada chamada, inclusive dentro de render |

---

## Fora do alcance desta suíte

Cobertura por roteiro manual reproduzível, descrito na seção 21 do Decision Gate:
F01 (bootstrap em cena), F10 e F13 (regras hoje dentro do `GameManager`, que é
`MonoBehaviour`), F16 e F17 (estado entre cenas), F18 a F26 (UI, `Animator`, áudio,
`PlayerPrefs`).

As regras de F10 e F13 passam a ser automatizáveis **depois** da Etapa E, quando
migrarem do `GameManager` para o domínio — e é justamente a suíte que deverá provar
que a migração preservou o comportamento.
