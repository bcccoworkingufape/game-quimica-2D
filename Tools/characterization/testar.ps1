<#
    Testes de caracterizacao do Molz — Paper 1, Fase 0.

    Roda a suite que fotografa o comportamento do Estado C antes da
    intervencao arquitetural C -> D0 -> D.

        powershell -ExecutionPolicy Bypass -File .\testar.ps1

    Pre-requisito: .NET SDK 8+ no PATH. Nao precisa de Unity nem de licenca.
#>

param(
  [switch]$Detalhado,
  [switch]$Listar
)

$ErrorActionPreference = 'Stop'
$proj = Join-Path $PSScriptRoot 'Molz.Characterization.csproj'

Write-Host ''
Write-Host '  Testes de caracterizacao — Estado C' -ForegroundColor Cyan
Write-Host '  ----------------------------------' -ForegroundColor DarkGray

$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Push-Location $repo
try {
  $sha = (git rev-parse HEAD).Trim()
  $branch = (git rev-parse --abbrev-ref HEAD).Trim()
  $sujo = (git status --porcelain -- Assets/) -ne $null
  Write-Host "  branch : $branch"
  Write-Host "  commit : $sha"
  if ($sujo) {
    Write-Host '  AVISO  : ha alteracoes nao commitadas em Assets/' -ForegroundColor Yellow
  } else {
    Write-Host '  Assets : limpo' -ForegroundColor DarkGray
  }
} finally {
  Pop-Location
}
Write-Host ''

if ($Listar) {
  dotnet test $proj --nologo --list-tests
  exit $LASTEXITCODE
}

if ($Detalhado) {
  dotnet test $proj --nologo --logger "console;verbosity=detailed"
} else {
  dotnet test $proj --nologo
}

$code = $LASTEXITCODE
Write-Host ''
if ($code -eq 0) {
  Write-Host '  Paridade preservada: todos os testes passaram.' -ForegroundColor Green
} else {
  Write-Host '  DIVERGENCIA FUNCIONAL detectada. Nao prossiga para a proxima etapa.' -ForegroundColor Red
}
exit $code
