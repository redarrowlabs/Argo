Param(
    [string][Parameter(Mandatory = $true)]
    $Path,
    [string][Parameter(Mandatory = $true)]
    $BuildNumber,
    [switch][Parameter(Mandatory = $false)]
    $IsPreRelease
)

function Update-AssemblyInfo
{
    Param(
        [string]$assemblyVersion,
        [switch]$isPreRelease
    )

    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $assemblyfileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $assemblyInformationalVersionPattern = 'AssemblyInformationalVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'

    $assemblyVersionReplacement = "AssemblyVersion(`"$($assemblyVersion)`")"
    $assemblyFileVersionReplacement = "AssemblyFileVersion(`"$($assemblyVersion)`")"
    $assemblyInformationalVersionReplacement = "AssemblyInformationalVersion(`"$($assemblyVersion)`")"

    if($isPreRelease)
    {
        $patchDelimiterIndex = $assemblyVersion.LastIndexOf('.')
        $assemblyInformationalVersionReplacement = "AssemblyInformationalVersion(`"$($assemblyVersion.Substring(0, $patchDelimiterIndex))-pre`")"
    }
 
    Write-Host -NoNewline "Patching with version "
    Write-Host $($assemblyVersion) -ForegroundColor Green
    foreach($assemblyFile in $input) {
        $fileName = $assemblyFile.FullName
        Write-Host -NoNewline "Patching $($fileName)...."
        (Get-Content $fileName) | ForEach-Object  { 
           % {$_ -replace $assemblyVersionPattern, $assemblyVersionReplacement } |
           % {$_ -replace $assemblyfileVersionPattern, $assemblyFileVersionReplacement } |
           % {$_ -replace $assemblyInformationalVersionPattern, $assemblyInformationalVersionReplacement }
        } | Out-File $fileName -Encoding UTF8 -Force
        Write-Host "Success!" -ForegroundColor Green
    }
}

function Update-AllAssemblyInfoFiles
{
   Param (
        [string]$assemblyVersion,
        [string]$path,
        [switch]$isPreRelease
   )

   Write-Host "Searching $($path) for AssemblyInfo files"
   Get-Childitem "$($env:BUILD_REPOSITORY_LOCALPATH)$path" -Recurse | Update-AssemblyInfo $assemblyVersion -isPreRelease:$isPreRelease.IsPresent
}

Write-Verbose "Entering script $($MyInvocation.MyCommand.Name)"

Write-Verbose "Parameter values:"
foreach($key in $PSBoundParameters.Keys) {
    Write-Verbose ($key + ' = ' + $PSBoundParameters[$key])
}

# Extract the versions from the parameters.
function Find-Version
{
    [OutputType([string])]
    Param (
        [string]$Version,
        [string]$Description
    )

    $VersionData = [regex]::matches($Version, "\d+\.\d+\.\d+\.\d+")
    switch($VersionData.Count)
    {
        0
        {
            Write-Error "Could not find version number data in $($Description) '$($Version)'"
            exit 1
        }
        1 {}
        default
        {
            Write-Warning "Found more than instance of version data in $($Description) '$($Version)'"
            Write-Warning "Will assume first instance is version"
        }
    }

    Write-Verbose "Found $($Description) '$($VersionData[0])'"

    return $VersionData[0]
}

$AssemblyVersion = Find-Version $BuildNumber "AssemblyVersion"

Update-AllAssemblyInfoFiles $AssemblyVersion $Path -isPreRelease:$IsPreRelease.IsPresent