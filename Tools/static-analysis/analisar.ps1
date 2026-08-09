<#
    ANALISE ESTATICA DO MOLZ  —  script unico de execucao

    Roda, nas tres versoes do software:
      1. SonarAnalyzer.CSharp (SonarSource) + Microsoft.Unity.Analyzers  -> violacoes de regras
      2. Rotina Roslyn (ferramentas/CodeMetrics)                          -> metricas agregadas
      3. Consolidacao                                                     -> resultados/COMPARATIVO.md

    USO
      powershell -ExecutionPolicy Bypass -File .\analisar.ps1              # tudo
      powershell -ExecutionPolicy Bypass -File .\analisar.ps1 -Apenas 2D   # so a versao atual
      powershell -ExecutionPolicy Bypass -File .\analisar.ps1 -SemSonar    # so as metricas
      powershell -ExecutionPolicy Bypass -File .\analisar.ps1 -SemMetricas # so o Sonar

    PRE-REQUISITOS
      .NET SDK 8+, Node.js, Git
      O projeto 2D precisa ter sido aberto na Unity ao menos uma vez, para existirem
      Assembly-CSharp.csproj e Library\ScriptAssemblies (usados como fonte de referencias).
#>
[CmdletBinding()]
param(
  [ValidateSet('Tudo', '3D', '2D', '2Dpre')] [string]$Apenas = 'Tudo',
  [switch]$SemSonar,
  [switch]$SemMetricas
)

$ErrorActionPreference = 'Stop'

# Reprodutibilidade: sem isto, as descricoes das regras do compilador (CS####) saem
# no idioma do sistema, e o mesmo codigo gera relatorios diferentes em maquinas diferentes.
$env:DOTNET_CLI_UI_LANGUAGE = 'en'
$env:VSLANG = '1033'

$here  = Split-Path -Parent $MyInvocation.MyCommand.Path
$ferr  = Join-Path $here 'ferramentas'
$res   = Join-Path $here 'resultados'
$repo2D = Split-Path -Parent (Split-Path -Parent $here)
$repo3D = Join-Path (Split-Path -Parent $repo2D) 'game-quimica'
$tpl    = Join-Path $repo2D 'Assembly-CSharp.csproj'
$tmp    = Join-Path $env:TEMP 'molz-analise'
$wt     = Join-Path $env:TEMP 'molz-baseline'

function Titulo($t) { Write-Host "`n$('=' * 62)`n  $t`n$('=' * 62)" -ForegroundColor Cyan }
function Passo($t)  { Write-Host "  -> $t" -ForegroundColor DarkGray }

# ---------- pre-requisitos ----------
Titulo 'Verificando pre-requisitos'
foreach ($c in 'dotnet', 'node', 'git') {
  if (-not (Get-Command $c -ErrorAction SilentlyContinue)) { throw "'$c' nao encontrado no PATH." }
  Passo "$c OK"
}
if (-not $SemSonar -and -not (Test-Path $tpl)) {
  Write-Warning "Assembly-CSharp.csproj nao existe. Abra o projeto 2D na Unity uma vez, ou use -SemSonar."
  $SemSonar = $true
}
New-Item -ItemType Directory -Force $tmp, $res | Out-Null

# ---------- alvos ----------
$alvos = @(
  @{ id = '3D';    pasta = '3D';          rotulo = '3D (ChemistryLab)';      scripts = (Join-Path $repo3D 'ChemistryLab\Assets\Scripts') }
  @{ id = '2Dpre'; pasta = '2D-pre-mvp';  rotulo = '2D pre-MVP (1226b59)';   scripts = (Join-Path $wt 'Assets\Scripts') }
  @{ id = '2D';    pasta = '2D-atual';    rotulo = '2D (Molz atual)';        scripts = (Join-Path $repo2D 'Assets\Scripts') }
) | Where-Object { $Apenas -eq 'Tudo' -or $_.id -eq $Apenas }

# ---------- worktree da versao historica, se necessario ----------
$precisaWorktree = $alvos | Where-Object { $_.id -eq '2Dpre' }
if ($precisaWorktree) {
  Titulo 'Recuperando a versao anterior a reestruturacao (commit 1226b59)'
  $ErrorActionPreference = 'Continue'
  git -C $repo2D worktree prune | Out-Null
  if (Test-Path $wt) { git -C $repo2D worktree remove $wt --force | Out-Null }
  git -C $repo2D worktree add $wt 1226b59 --quiet
  $okWt = ($LASTEXITCODE -eq 0)
  $ErrorActionPreference = 'Stop'
  if ($okWt) { Passo "worktree em $wt" }
  else { Write-Warning 'nao foi possivel criar o worktree; a versao pre-MVP sera ignorada'
         $alvos = $alvos | Where-Object { $_.id -ne '2Dpre' } }
}

# ---------- compila o CodeMetrics uma vez ----------
if (-not $SemMetricas) {
  Titulo 'Compilando a rotina de metricas (Roslyn)'
  dotnet build (Join-Path $ferr 'CodeMetrics') -c Release -v q --nologo | Out-Null
  Passo 'CodeMetrics OK'
}

# ---------- execucao ----------
foreach ($a in $alvos) {
  Titulo $a.rotulo
  if (-not (Test-Path $a.scripts)) { Write-Warning "nao encontrado: $($a.scripts)"; continue }
  $saida = Join-Path $res $a.pasta
  New-Item -ItemType Directory -Force $saida | Out-Null

  if (-not $SemSonar) {
    Passo 'SonarAnalyzer.CSharp + Microsoft.Unity.Analyzers'
    $proj  = Join-Path $tmp "$($a.id)\anl.csproj"
    $sarif = Join-Path $saida 'sonar.sarif'
    node (Join-Path $ferr 'genproj.js') $tpl $a.scripts $proj $sarif | Out-Null
    $log = dotnet build $proj -c Debug --nologo -v q 2>&1
    $err = ($log | Select-String -Pattern ': error ').Count
    if ($err -gt 0) { Write-Warning "$err erro(s) de compilacao; as regras semanticas podem ficar incompletas" }
    node (Join-Path $ferr 'sarif.js') $sarif $a.rotulo | Out-Null
    $tot = (Get-Content (Join-Path $saida 'sonar-resumo.txt') | Select-String 'violacoes no codigo proprio').Line
    Passo ($tot -replace '\s+', ' ').Trim()
  }

  if (-not $SemMetricas) {
    Passo 'Metricas Roslyn (complexidade, manutenibilidade, acoplamento)'
    dotnet run --project (Join-Path $ferr 'CodeMetrics') -c Release --no-build -- `
      $a.rotulo $a.scripts (Join-Path $saida 'metricas') | Out-Null
    $mi = (Get-Content (Join-Path $saida 'metricas-resumo.txt') | Select-String 'Maintainability Index medio').Line
    Passo ($mi -replace '\s+', ' ').Trim()
  }
}

# ---------- limpeza do worktree ----------
if (Test-Path $wt) {
  $ErrorActionPreference = 'Continue'
  git -C $repo2D worktree remove $wt --force | Out-Null
  $ErrorActionPreference = 'Stop'
}

# ---------- consolidacao ----------
if ($Apenas -eq 'Tudo') {
  Titulo 'Consolidando'
  node (Join-Path $ferr 'comparativo.js') | Out-Null
  Write-Host "  -> resultados\COMPARATIVO.md" -ForegroundColor Green
}

Titulo 'Concluido'
Write-Host "  Tabelas consolidadas : resultados\COMPARATIVO.md"
Write-Host "  Dados brutos         : resultados\<versao>\"
Write-Host ''
