param (
    [Parameter(Mandatory=$true)]
    [string]$GameDir
)

# Update local references to the latest version

# Check if the game directory exists
if (-not (Test-Path -Path $GameDir -PathType Container)) {
    Write-Host "Error: The game directory does not exist"
    exit 1
}

# Expected files
#   GameDir/7DaysToDie_Data/Managed/0Harmony.dll
#   GameDir/7DaysToDie_Data/Managed/Assembly-CSharp.dll
#   GameDir/7DaysToDie_Data/Managed/LogLibrary.dll
#   GameDir/7DaysToDie_Data/Managed/UnityEngine.dll
#   GameDir/7DaysToDie_Data/Managed/UnityEngine.CoreModule.dll

function Verify-File {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Path
    )

    Write-Host "Verifying $Path"
    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        Write-Host "Error: $Path does not exist"
        exit 1
    }
}

Verify-File "$GameDir/7DaysToDie_Data/Managed/0Harmony.dll"
Verify-File "$GameDir/7DaysToDie_Data/Managed/Assembly-CSharp.dll"
Verify-File "$GameDir/7DaysToDie_Data/Managed/LogLibrary.dll"
Verify-File "$GameDir/7DaysToDie_Data/Managed/UnityEngine.dll"
Verify-File "$GameDir/7DaysToDie_Data/Managed/UnityEngine.CoreModule.dll"

Write-Host "All files exist"

$ScriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$LocalRefsDir = "$ScriptDir/../localrefs"

Write-Host "Updating local references..."
Write-Host "`tGame directory: $GameDir"
Write-Host "`tLocal references directory: $LocalRefsDir"

# Ensure the local references directory exists
New-Item -ItemType Directory -Force -Path $LocalRefsDir

function Copy-File {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Source,
        [Parameter(Mandatory=$true)]
        [string]$Destination
    )

    Write-Host "Copying $Source to $Destination"
    Copy-Item -Path $Source -Destination $Destination -Force -ErrorAction Stop
}

Copy-File "$GameDir/7DaysToDie_Data/Managed/0Harmony.dll" "$LocalRefsDir/0Harmony.dll"
Copy-File "$GameDir/7DaysToDie_Data/Managed/Assembly-CSharp.dll" "$LocalRefsDir/Assembly-CSharp.dll"
Copy-File "$GameDir/7DaysToDie_Data/Managed/LogLibrary.dll" "$LocalRefsDir/LogLibrary.dll"
Copy-File "$GameDir/7DaysToDie_Data/Managed/UnityEngine.dll" "$LocalRefsDir/UnityEngine.dll"
Copy-File "$GameDir/7DaysToDie_Data/Managed/UnityEngine.CoreModule.dll" "$LocalRefsDir/UnityEngine.CoreModule.dll"

Write-Host "Local references updated successfully"

# Give some time to read the message
Start-Sleep -Seconds 2
