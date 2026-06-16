[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$NodePath,

    [Parameter(Mandatory = $true)]
    [string]$NodeModulesPath,

    [string]$SourcePath = (Join-Path $PSScriptRoot '..\..\docs\test-cases'),

    [string]$OutputPath = (Join-Path $PSScriptRoot '..\..\docs\test-cases\generated\jigsaw-vina-test-cases.xlsx'),

    [switch]$ValidateOnly
)

$ErrorActionPreference = 'Stop'

function Resolve-RequiredPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LiteralPath,

        [Parameter(Mandatory = $true)]
        [string]$Description,

        [switch]$Directory
    )

    if (-not (Test-Path -LiteralPath $LiteralPath)) {
        throw "$Description does not exist: $LiteralPath"
    }

    $resolved = (Resolve-Path -LiteralPath $LiteralPath).Path
    if ($Directory -and -not (Test-Path -LiteralPath $resolved -PathType Container)) {
        throw "$Description must be a directory: $resolved"
    }
    if (-not $Directory -and -not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
        throw "$Description must be a file: $resolved"
    }

    return $resolved
}

$resolvedNodePath = Resolve-RequiredPath -LiteralPath $NodePath -Description 'Node executable'
$resolvedNodeModulesPath = Resolve-RequiredPath -LiteralPath $NodeModulesPath -Description 'Node modules path' -Directory
$resolvedSourcePath = Resolve-RequiredPath -LiteralPath $SourcePath -Description 'Test case source path' -Directory
$sourceReadmePath = Join-Path $resolvedSourcePath 'README.md'
$null = Resolve-RequiredPath -LiteralPath $sourceReadmePath -Description 'Living Test Plan README'

$exporterPath = Join-Path $PSScriptRoot 'export_test_cases_to_excel.mjs'
$resolvedExporterPath = Resolve-RequiredPath -LiteralPath $exporterPath -Description 'Node exporter'

$tempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$tempPrefix = 'jigsaw-vina-test-case-export-'
$tempDirectory = Join-Path $tempRoot ($tempPrefix + [Guid]::NewGuid().ToString('N'))

try {
    $null = New-Item -ItemType Directory -Path $tempDirectory
    $tempNodeModules = Join-Path $tempDirectory 'node_modules'
    $null = New-Item -ItemType Junction -Path $tempNodeModules -Target $resolvedNodeModulesPath

    $tempExporterPath = Join-Path $tempDirectory 'export_test_cases_to_excel.mjs'
    Copy-Item -LiteralPath $resolvedExporterPath -Destination $tempExporterPath

    $nodeArguments = @(
        $tempExporterPath,
        '--source',
        $resolvedSourcePath
    )

    if ($ValidateOnly) {
        $nodeArguments += '--validate-only'
    }
    else {
        $resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
        $previewDirectory = Join-Path $tempDirectory 'preview'
        $nodeArguments += @(
            '--output',
            $resolvedOutputPath,
            '--preview-dir',
            $previewDirectory
        )
    }

    & $resolvedNodePath @nodeArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Test case exporter failed with exit code $LASTEXITCODE."
    }
}
finally {
    if (Test-Path -LiteralPath $tempDirectory) {
        $resolvedTempDirectory = [System.IO.Path]::GetFullPath($tempDirectory)
        $requiredPrefix = Join-Path $tempRoot $tempPrefix
        if (-not $resolvedTempDirectory.StartsWith(
            $requiredPrefix,
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
            throw "Refusing to remove unexpected temporary path: $resolvedTempDirectory"
        }

        if (Test-Path -LiteralPath $tempNodeModules) {
            $junction = Get-Item -LiteralPath $tempNodeModules -Force
            $junctionTarget = [System.IO.Path]::GetFullPath(
                [string]($junction.Target | Select-Object -First 1)
            )
            if (-not $junction.Attributes.HasFlag(
                [System.IO.FileAttributes]::ReparsePoint
            )) {
                throw "Refusing to remove non-junction runtime path: $tempNodeModules"
            }
            if (-not $junctionTarget.Equals(
                $resolvedNodeModulesPath,
                [System.StringComparison]::OrdinalIgnoreCase
            )) {
                throw "Runtime junction target changed unexpectedly: $junctionTarget"
            }

            $junction.Delete()
        }

        Remove-Item -LiteralPath $resolvedTempDirectory -Recurse -Force
    }
}
