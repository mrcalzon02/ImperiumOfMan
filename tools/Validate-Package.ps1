[CmdletBinding()]
param(
    [string]$PackagePath = "package/ImperiumOfMan",
    [string]$OutputPath = "artifacts"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$warnings = [System.Collections.Generic.List[string]]::new()
$reportLines = [System.Collections.Generic.List[string]]::new()

function Write-Gate {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][bool]$Passed,
        [Parameter(Mandatory = $true)][string]$Details
    )

    if (-not $Passed) {
        throw "PACKAGE GATE FAILED: $Name - $Details"
    }

    Write-Host "[PASS] $Name - $Details"
    $script:reportLines.Add("- PASS - **$Name:** $Details")
}

function Test-BytePrefix {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][byte[]]$Prefix
    )

    $stream = [System.IO.File]::OpenRead($Path)
    try {
        if ($stream.Length -lt $Prefix.Length) {
            return $false
        }

        foreach ($expected in $Prefix) {
            if ($stream.ReadByte() -ne $expected) {
                return $false
            }
        }

        return $true
    }
    finally {
        $stream.Dispose()
    }
}

function Get-ThumbnailFormat {
    param([Parameter(Mandatory = $true)][string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)

    if ($bytes.Length -ge 8 -and
        $bytes[0] -eq 0x89 -and $bytes[1] -eq 0x50 -and
        $bytes[2] -eq 0x4E -and $bytes[3] -eq 0x47) {
        return "PNG"
    }

    if ($bytes.Length -ge 12 -and
        [System.Text.Encoding]::ASCII.GetString($bytes, 0, 4) -eq "RIFF" -and
        [System.Text.Encoding]::ASCII.GetString($bytes, 8, 4) -eq "WEBP") {
        return "WEBP"
    }

    if ($bytes.Length -ge 3 -and
        $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xD8 -and $bytes[2] -eq 0xFF) {
        return "JPEG"
    }

    return "UNKNOWN"
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$packageRoot = Join-Path $repositoryRoot $PackagePath
$outputRoot = Join-Path $repositoryRoot $OutputPath
$stageRoot = Join-Path $outputRoot "ImperiumOfMan"

Write-Gate "Runtime package directory" (Test-Path -LiteralPath $packageRoot -PathType Container) $packageRoot

$requiredFiles = @(
    "modmanifest.json",
    "ImperiumOfMan.dll",
    "imperiumofman.bundle",
    "thumbnail.png"
)

foreach ($requiredFile in $requiredFiles) {
    $requiredPath = Join-Path $packageRoot $requiredFile
    Write-Gate "Required file: $requiredFile" (Test-Path -LiteralPath $requiredPath -PathType Leaf) $requiredPath
    Write-Gate "Non-empty file: $requiredFile" ((Get-Item -LiteralPath $requiredPath).Length -gt 0) "File contains data"
}

$manifestPath = Join-Path $packageRoot "modmanifest.json"
try {
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
}
catch {
    throw "PACKAGE GATE FAILED: modmanifest.json is not valid JSON. $($_.Exception.Message)"
}

Write-Gate "Manifest unique name" (
    -not [string]::IsNullOrWhiteSpace([string]$manifest.UniqueModName) -and
    [string]$manifest.UniqueModName -match '^[A-Za-z0-9_]+$'
) "UniqueModName '$($manifest.UniqueModName)' uses only letters, numbers, and underscores"

$assemblies = @($manifest.Assemblies)
Write-Gate "Manifest assembly list" ($assemblies.Count -gt 0) "Manifest declares $($assemblies.Count) assembly file(s)"

foreach ($assembly in $assemblies) {
    $assemblyName = [string]$assembly
    $safeAssemblyPath = -not [System.IO.Path]::IsPathRooted($assemblyName) -and $assemblyName -notmatch '(^|[\\/])\.\.([\\/]|$)'
    Write-Gate "Safe assembly path: $assemblyName" $safeAssemblyPath "Assembly path is relative and does not traverse directories"

    $assemblyPath = Join-Path $packageRoot $assemblyName
    Write-Gate "Manifest assembly exists: $assemblyName" (Test-Path -LiteralPath $assemblyPath -PathType Leaf) $assemblyPath
    Write-Gate "Managed PE signature: $assemblyName" (Test-BytePrefix -Path $assemblyPath -Prefix ([byte[]](0x4D, 0x5A))) "Assembly begins with the Windows PE MZ signature"
}

$bundlePath = Join-Path $packageRoot "imperiumofman.bundle"
$unityFsPrefix = [System.Text.Encoding]::ASCII.GetBytes("UnityFS")
Write-Gate "Unity asset-bundle signature" (Test-BytePrefix -Path $bundlePath -Prefix $unityFsPrefix) "imperiumofman.bundle begins with UnityFS"

$thumbnailPath = Join-Path $packageRoot "thumbnail.png"
$thumbnailFormat = Get-ThumbnailFormat -Path $thumbnailPath
Write-Gate "Readable thumbnail format" ($thumbnailFormat -ne "UNKNOWN") "Detected $thumbnailFormat image data"
if ($thumbnailFormat -ne "PNG") {
    $warnings.Add("thumbnail.png contains $thumbnailFormat data rather than PNG data. The package is staged, but this should be converted to a real PNG before Workshop publication if Quasimorph or Steam rejects it.")
}

$forbiddenExtensions = @(".cs", ".csproj", ".sln")
$forbiddenFiles = Get-ChildItem -LiteralPath $packageRoot -Recurse -File | Where-Object {
    $forbiddenExtensions -contains $_.Extension.ToLowerInvariant()
}
Write-Gate "No development source files in runtime package" ($forbiddenFiles.Count -eq 0) "No C# source, project, or solution files are being distributed"

$forbiddenDirectoryNames = @("Assets", "Library", "Temp", "ProjectSettings", "Packages")
$forbiddenDirectories = Get-ChildItem -LiteralPath $packageRoot -Recurse -Directory | Where-Object {
    $forbiddenDirectoryNames -contains $_.Name
}
Write-Gate "No Unity project directories in runtime package" ($forbiddenDirectories.Count -eq 0) "Runtime package is separate from the Unity development project"

if (Test-Path -LiteralPath $stageRoot) {
    Remove-Item -LiteralPath $stageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $stageRoot -Force | Out-Null
Copy-Item -Path (Join-Path $packageRoot "*") -Destination $stageRoot -Recurse -Force

$packagedFiles = Get-ChildItem -LiteralPath $stageRoot -Recurse -File | Sort-Object FullName
$reportLines.Insert(0, "# ImperiumOfMan local-test package report")
$reportLines.Insert(1, "")
$reportLines.Insert(2, "Commit: $($env:GITHUB_SHA)")
$reportLines.Insert(3, "Generated: $([DateTime]::UtcNow.ToString('u')) UTC")
$reportLines.Insert(4, "Package source: `$PackagePath`")
$reportLines.Insert(5, "")
$reportLines.Add("")
$reportLines.Add("## Runtime files")
$reportLines.Add("")

foreach ($file in $packagedFiles) {
    $relativeName = [System.IO.Path]::GetRelativePath($stageRoot, $file.FullName).Replace('\\', '/')
    $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    $reportLines.Add("- `$relativeName` - $($file.Length) bytes - SHA256 `$hash`")
}

$reportLines.Add("")
$reportLines.Add("## Warnings")
$reportLines.Add("")
if ($warnings.Count -eq 0) {
    $reportLines.Add("- None")
}
else {
    foreach ($warning in $warnings) {
        Write-Warning $warning
        $reportLines.Add("- $warning")
    }
}

$reportLines.Add("")
$reportLines.Add("## Live-test boundary")
$reportLines.Add("")
$reportLines.Add("This gate proves that the repository contains a structurally valid local-test package with a readable manifest, PE assembly, UnityFS bundle, and thumbnail. It cannot prove that Quasimorph successfully executes the assembly or registers every object. Final integration acceptance requires launching the current game build with only this ImperiumOfMan copy enabled and reviewing the game/BepInEx log.")

$reportPath = Join-Path $stageRoot "PACKAGE_REPORT.md"
$reportLines | Set-Content -LiteralPath $reportPath -Encoding UTF8

Write-Host ""
Write-Host "PACKAGE VALIDATION PASSED"
Write-Host "Staged local-test package: $stageRoot"
Write-Host "Files staged: $($packagedFiles.Count)"
Write-Host "Warnings: $($warnings.Count)"
