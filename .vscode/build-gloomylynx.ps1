# GloomyFurniture 1.6 — MSBuild Rebuild. Invoked from VS Code/Cursor Run Build Task (Ctrl+Shift+B = Debug).
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$proj = Join-Path $repoRoot 'GloomyFurniture\1.6\Source\Gloomylynx.csproj'
if (-not (Test-Path $proj)) {
    Write-Error "Project not found: $proj"
    exit 1
}

$msb = $null
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (Test-Path $vswhere) {
    $msb = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $msb -or -not (Test-Path $msb)) {
    foreach ($ed in @('Community', 'Professional', 'Enterprise', 'BuildTools')) {
        $p = Join-Path $env:ProgramFiles "Microsoft Visual Studio\2022\$ed\MSBuild\Current\Bin\MSBuild.exe"
        if (Test-Path $p) {
            $msb = $p
            break
        }
    }
}
if (-not $msb -or -not (Test-Path $msb)) {
    Write-Error 'MSBuild.exe not found. Install Visual Studio 2022 (Desktop development with C++) or Build Tools with MSBuild.'
    exit 1
}

& $msb $proj /t:Rebuild /p:"Configuration=$Configuration" /restore:false /v:m
exit $LASTEXITCODE
