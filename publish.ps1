param(
    [Parameter(Mandatory)]
    [System.String]$Version,

    [Parameter(Mandatory)]
    [ValidateSet('Debug', 'Release')]
    [System.String]$Target,
    
    [Parameter(Mandatory)]
    [System.String]$TargetPath,
    
    [Parameter(Mandatory)]
    [System.String]$TargetAssembly,

    [Parameter(Mandatory)]
    [System.String]$RoundsPath,
    
    [Parameter(Mandatory)]
    [System.String]$ProjectPath
)

# Make sure Get-Location is the script path
Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Path)

# Test some preliminaries
("$TargetPath",
"$RoundsPath",
"$ProjectPath"
) | % {
    if (!(Test-Path "$_")) { Write-Error -ErrorAction Stop -Message "$_ folder is missing" }
}

# Go
Write-Host "Publishing for $Target from $TargetPath"

# Plugin name without ".dll"
$name = "$TargetAssembly" -Replace ('.dll')
$version = "$Version"
$versionMajor = $version.split('.')[0]
$versionMinor = $version.split('.')[1]
$versionPatch = $version.split('.')[2]

# Debug copies the dll to ROUNDS
if ($name.Equals("LocalGameModes")) {
    Write-Host "Updating local installation in $RoundsPath"
    
    $plug = New-Item -Type Directory -Path "$RoundsPath\profiles\Dev Modded\BepInEx\plugins\zeeke-$name.$version" -Force
    Write-Host "Copy $TargetAssembly to $plug"
    Copy-Item -Path "$TargetPath\$name.dll" -Destination "$plug" -Force
    Copy-Item -Path "$ProjectPath\LocalGameModes\Maps\*" -Destination "$plug" -Force

    # icon, readme
    Copy-Item -Path "$ProjectPath\README.md" -Destination "$plug" -Force
    Copy-Item -Path "$ProjectPath\icon.png" -Destination "$plug" -Force

    # Manifest Json
    Copy-Item -Path "$ProjectPath\manifest.json" -Destination "$plug" -Force 
    ((Get-Content -path "$plug\manifest.json" -Raw) -replace "#VERSION#", "$Version") | Set-Content -Path "$plug\manifest.json"

    #Copy-Item -Path "$ProjectPath\mm_v2_manifest.json" -Destination "$plug" -Force 
    #((Get-Content -path "$plug\mm_v2_manifest.json" -Raw) -replace "#VERSION_MAJOR#", "$versionMajor") | Set-Content -Path "$plug\mm_v2_manifest.json"
    #((Get-Content -path "$plug\mm_v2_manifest.json" -Raw) -replace "#VERSION_MINOR#", "$versionMinor") | Set-Content -Path "$plug\mm_v2_manifest.json"
    #((Get-Content -path "$plug\mm_v2_manifest.json" -Raw) -replace "#VERSION_PATCH#", "$versionPatch") | Set-Content -Path "$plug\mm_v2_manifest.json"

    # Compress and Zip folder
    Compress-Archive -Path "$plug\*" -DestinationPath "$plug.zip" -Force
    $plug.Delete($true)
}

# Release package for ThunderStore
# if ($Target.Equals("Release") -and $name.Equals("LocalGameModes")) {
#     $package = "$ProjectPath\release"
#     
#     Write-Host "Packaging for ThunderStore"
#     New-Item -Type Directory -Path "$package\Thunderstore" -Force
#     $thunder = New-Item -Type Directory -Path "$package\Thunderstore\package"
#     $thunder.CreateSubdirectory('plugins')
#     Copy-Item -Path "$TargetPath\$name.dll" -Destination "$thunder\plugins\"
#     Copy-Item -Path "$ProjectPath\README.md" -Destination "$thunder\README.md"
#     Copy-Item -Path "$ProjectPath\manifest.json" -Destination "$thunder\manifest.json"
#     Copy-Item -Path "$ProjectPath\icon.png" -Destination "$thunder\icon.png"
# 
#     ((Get-Content -path "$thunder\manifest.json" -Raw) -replace "#VERSION#", "$Version") | Set-Content -Path "$thunder\manifest.json"
# 
#     Remove-Item -Path "$package\Thunderstore\$name.$Version.zip" -Force
#     Compress-Archive -Path "$thunder\*" -DestinationPath "$package\Thunderstore\$name.$Version.zip"
#     $thunder.Delete($true)
# }

# Release package for GitHub
# if ($Target.Equals("Release") -and $name.Equals("LocalGameModes")) {
#     $package = "$ProjectPath\release"
# 
#     Write-Host "Packaging for GitHub"
#     $pkg = New-Item -Type Directory -Path "$package\package"
#     $pkg.CreateSubdirectory('BepInEx\plugins')
#     Copy-Item -Path "$TargetPath\$name.dll" -Destination "$pkg\BepInEx\plugins\$name.dll"
# 
#     Remove-Item -Path "$package\$name.$Version.zip" -Force
#     Compress-Archive -Path "$pkg\*" -DestinationPath "$package\$name.$Version.zip"
#     $pkg.Delete($true)
# }


Pop-Location
