function global:Trace-VstsEnteringInvocation {
 [CmdletBinding()]
 param ([string]$invocation)

	Write-Host $myInvocation.InvocationName
}

function global:Trace-VstsLeavingInvocation {
 [CmdletBinding()]
 param ([string]$invocation)

	Write-Host $myInvocation.InvocationName
}

function global:Import-VstsLocStrings {
 [CmdletBinding()]
 param ([string]$path)
}

function global:Get-VstsInput {
 [CmdletBinding()]
 param ([string]$name,
        [switch]$AsBool,
        [switch]$Require)

    return Get-Variable -Name $name
}

function globaL:Assert-VstsPath {
 [CmdletBinding()]
 param (
 [string]$LiteralPath,
 [string]$PathType)
}

function globaL:Assert-VstsAgent {
 [CmdletBinding()]
 param (
 [string]$Minimum)
}


function global:Get-VstsLocString {
 [CmdletBinding()]
 param ([string]$Key)
}

function global:Get-VstsTaskVariable {
 [CmdletBinding()]
 param (
        [string]$name,
        [switch]$Require)

    try {
        $value = Get-Variable -Name $name

        if([string]::IsNullOrEmpty($value))
        {
            return "c:\temp"
        }
        
        return $value
    }
    catch
    {
        return "c:\temp";
    }
}

function global:Invoke-VstsTool {
 [CmdletBinding()]
 param (
        [string]$FileName,
        [string]$Arguments,
        [string]$WorkingDirectory
       
        )
    Write-Host $value.FileName
}

function global:Write-VstsTaskError {
    [CmdletBinding()]
    param ([string]$Message)
    
    Write-Host $Message;
}

function global:Write-VstsSetResult {
[CmdletBinding()]
    param (
           [string]$Result,
           [string]$Message,
           [switch]$DoNotThrow)
    
    Write-Host $Result + " " + $Message;
}