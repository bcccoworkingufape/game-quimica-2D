param([string]$UnityData = 'C:/Program Files/Unity/Hub/Editor/6000.4.1f1/Editor/Data')
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Push-Location $repoRoot
try {
    $output = Join-Path $repoRoot 'Temp/ScoringValidation'
    New-Item -ItemType Directory -Force $output | Out-Null
    $framework = Join-Path $UnityData 'MonoBleedingEdge/lib/mono/4.8-api'
    $sourceFiles = @(
        'Assets/Scripts/Domain/Enums/GameMode.cs',
        'Assets/Scripts/Domain/Entities/Question.cs',
        'Assets/Scripts/Domain/Services/IScoringService.cs',
        'Assets/Scripts/Domain/Services/ScoringService.cs',
        'Assets/Scripts/Domain/Services/ScoringRun.cs',
        'Assets/Scripts/Domain/Services/StarRecordBook.cs',
        'Assets/Scripts/Domain/UseCases/SubmitAnswerUseCase.cs',
        'Assets/Scripts/Core/QuestionRunTracker.cs',
        'Tools/scoring-tests/ScoringTests.cs'
    )
    $response = @('/nologo','/nostdlib+','/langversion:9.0','/target:exe',
        ('/out:"' + $output + '/ScoringTests.exe"'))
    foreach ($name in @('mscorlib.dll','System.dll','System.Core.dll')) {
        $response += '/reference:"' + (Join-Path $framework $name) + '"'
    }
    $response += $sourceFiles | ForEach-Object { '"' + $_ + '"' }
    $responsePath = Join-Path $output 'tests.rsp'
    Set-Content $responsePath $response
    & "$UnityData/NetCoreRuntime/dotnet.exe" "$UnityData/DotNetSdkRoslyn/csc.dll" "@$responsePath"
    if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
    & "$UnityData/MonoBleedingEdge/bin/mono.exe" "$output/ScoringTests.exe"
    if ($LASTEXITCODE -ne 0) { throw 'Scoring tests failed.' }
} finally { Pop-Location }

