function Copy-UnityProject {
    param (
        [string] $ProjectPath = "."
    )

    $ProjectPath = Resolve-Path -Path $ProjectPath
    $projectFolder = $ProjectPath | Split-Path -Leaf
    $tempProjectPath = Join-Path -Path $env:TEMP -ChildPath $projectFolder
    & robocopy $ProjectPath $tempProjectPath /mir `
        /xd (Join-Path -Path $ProjectPath -ChildPath Build) `
        /xd (Join-Path -Path $ProjectPath -ChildPath Library) `
        /xd (Join-Path -Path $ProjectPath -ChildPath Logs) `
        /xd (Join-Path -Path $ProjectPath -ChildPath Temp) `
        /nfl /ndl | Write-Host

    return $tempProjectPath
}

function Build-UnityWebGL {
    param (
        [string] $ProjectPath = ".",
        [string] $BuildPath = ""
    )

    $ProjectPath = Resolve-Path -Path $ProjectPath
    if ($BuildPath -eq "") {
        $BuildPath = Join-Path -Path $ProjectPath -ChildPath "Build/WebGL"
    }

    $projectVersion = Get-Content (Join-Path -Path $ProjectPath -ChildPath ProjectSettings/ProjectVersion.txt)
    | Select-String -Pattern "^m_EditorVersion: (.*)$"
    | % { $_.Matches.Groups[1].Value }

    if (Test-Path -Path $BuildPath){
        Remove-Item -Path $BuildPath -Recurse | Out-Null
    }
    New-Item -Path $BuildPath -ItemType "directory" | Out-Null
    $BuildPath = Resolve-Path -Path $BuildPath

    Write-Output "Building Unity WebGL..."
    & "C:\Program Files\Unity\Hub\Editor\$projectVersion\Editor\Unity.exe" -quit -batchmode -projectpath $ProjectPath -logFile - `
        -executeMethod ParallelBuild.WebGLBuilder.Build -buildpath $BuildPath
    | Write-Output
    if ($LastExitCode -ne 0) {
        throw "Error during the build, error code $LastExitCode"
    }
}

function Publish-Itch {
    param (
        [Parameter(Mandatory=$true)] [string] $BuildPath,
        [Parameter(Mandatory=$true)] [string] $ItchUser,
        [Parameter(Mandatory=$true)] [string] $ItchGame
    )

    $BuildPath = Resolve-Path -Path $BuildPath
    Write-Output "Publishing to Itch..."
    & "butler" push $BuildPath $ItchUser/${ItchGame}:webgl
    & "butler" status $ItchUser/${ItchGame}:webgl
}

function Remove-RecentlyUsedProjectPath {
    param (
        [Parameter(Mandatory=$true)] [string] $ProjectPath
    )

    $foundKey = $false
    $key = 'HKCU:\Software\Unity Technologies\Unity Editor 5.x'
    Get-Item -Path $key | Select-Object -ExpandProperty Property | Where-Object {$_.StartsWith("RecentlyUsedProjectPaths")} | % {
        $value = (Get-ItemProperty -Path $key -Name $_).$_
        $stringValue = [System.Text.Encoding]::Default.GetString($value)
        $adjustedPath = $stringValue -replace '/', '\'
        if ($adjustedPath -eq $ProjectPath) {
            Write-Output "Removing recently used project path '$adjustedPath'..."
            Remove-ItemProperty -Path $key -Name $_
            $foundKey = $true
        }
    }
    if ($foundKey -eq $false) {
        Write-Warning "No recently used project found."
    }
}

$ErrorActionPreference = "Stop"

$buildSettingsPath = Join-Path -Path $PSScriptRoot -ChildPath buildsettings.json
$settings = Get-Content -Path $buildSettingsPath | ConvertFrom-Json

$projectPath = $settings.projectPath
if ($projectPath -eq $null) {
    $projectPath = $PSScriptRoot
}
if ($settings.parallel -eq $true) {
    Write-Output "Copying project to temporary folder (do not edit files)..."
    $projectPath = Copy-UnityProject -ProjectPath $projectPath
    Write-Output "Done. You can continue working on your Unity project."
}

if ([System.IO.Path]::IsPathRooted($settings.buildPath)) {
    $buildPath = $settings.buildPath;
} else {
    $buildPath = Join-Path -Path $projectPath -ChildPath $settings.buildPath
}

Build-UnityWebGL -ProjectPath $projectPath -BuildPath $buildPath

if ($settings.parallel -eq $true) {
    Remove-RecentlyUsedProjectPath -ProjectPath $projectPath
}

if ($settings.publishToItch -eq $true) {
    Publish-Itch -BuildPath $buildPath -ItchUser $settings.itchUser -ItchGame $settings.itchGame
}

[System.Console]::Beep()
