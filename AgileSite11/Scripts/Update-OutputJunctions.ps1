<#
.Synopsis
	Creates junctions to CMS for both Utilities and Internal Utilities solutions.
	If a junction already exist then nothing happens for the solution.
#>
Param(
	# Full path to the solution the currently build project is in
	[Parameter(Mandatory=$true)]
	[string] $SolutionDirectory,
	
	# Build configuration (Debug/Release)
	[Parameter(Mandatory=$true)]
	[string] $Configuration,

    # Optional name of the junction. If this parameter is missing, the usual setup is created.
    [string] $JunctionName
)


<#
.Synopsis
    Main method. See script description.
#>
function Update-OutputJunctions
{
    [CmdletBinding()]
    param(
	    # Solution directory
        [string] $SolutionDirectory,
	
	    # Build configuration (Debug/Release)
        [string] $Configuration,

        # Optional name of the junction. If this parameter is missing, the usual setup is created.
        [string] $JunctionName)

    Set-Variable OUTPUT_ROOT -Option Constant -Value "Output"
    
    $outputDirectory = Join-Path -Path (Join-Path -Path $SolutionDirectory -ChildPath $OUTPUT_ROOT) -ChildPath $Configuration
    Write-Verbose "Output directory in which CMS and other solution folders should be present was resolved to `"$outputDirectory`"."

    if ($JunctionName)
    {
        Update-TestOutputJunctionPoint -OutputDirectory $outputDirectory -JunctionName $JunctionName
    }
    else
    {
        Update-UtilitiesOutputJunctionPoint -OutputDirectory $outputDirectory
    }
}

<#
.Synopsis
    Creates junction from tests output that points to "<Output>\<JunctionName>".
#>
function Update-TestOutputJunctionPoint
{
    param(
        # Directory in which output for the tests should be present.
        [string]$OutputDirectory,

        # Name of the junction point where tested assemblies will be available.
        [string]$JunctionName)

    Update-OutputJunction -OutputDirectory $OutputDirectory -SolutionName "Tests" -JunctionName $JunctionName
}

<#
.Synopsis
    Creates junctions from utilities and internal utilities output folders that point to "<Output>\CMS".
#>
function Update-UtilitiesOutputJunctionPoint
{
    param(
        # Directory in which output for the tests should be present.
        [string]$OutputDirectory)

    @("Utilities", "InternalUtilities") |
        % { Update-OutputJunction -OutputDirectory $OutputDirectory -SolutionName $_ -JunctionName "CMS" }
}

<#
.Synopsis
    Creates junction "<OutputDirectory>\<SolutionName>\<JunctionName>" that points to the "<Output>\<JunctionName>".
#>
function Update-OutputJunction
{
    [CmdletBinding()]
    param(
	    # Directory in which output for the SolutionName should be present.
        [string] $OutputDirectory,
	
	    # Name of a solution that requires junction to CMS solution binaries
        [string] $SolutionName,

        # Optional name of the junction. If this parameter is missing, the usual setup is created.
        [string] $JunctionName)

    Write-Verbose "Work on `"$SolutionName`" started."
    
    $targetFolder = Join-Path -Path $OutputDirectory -ChildPath $SolutionName
    if(-not (Test-Path $targetFolder))
    {
        New-Item -Path $targetFolder -ItemType "Directory"
        Write-Verbose "Folder `"$targetFolder`" created."
    }
    else
    {
        Write-Verbose "Folder `"$targetFolder`" exists."
    }

    $junctionTarget = "..\$JunctionName"
    $junctionLink = Join-Path -Path $targetFolder -ChildPath $JunctionName
    
    if(-not (Test-Path $junctionLink))
    {
        ."cmd" /c mklink /J "$junctionLink" "$junctionTarget"
    }    
    else
    {
        Write-Verbose "Junction `"$junctionLink`" <==> `"$junctionTarget`" exists."
    }

    Write-Verbose "Work on `"$SolutionName`" is done."
}


Update-OutputJunctions -SolutionDirectory $SolutionDirectory -Configuration $Configuration -JunctionName $JunctionName -Verbose:$VerbosePreference