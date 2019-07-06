function Trace-VstsEnteringInvocation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [System.Management.Automation.InvocationInfo]$InvocationInfo,
        [string[]]$Parameter = '*')

    Write-Verbose "Entering $(Get-InvocationDescription $InvocationInfo)."
    $OFS = ", "
    if ($InvocationInfo.BoundParameters.Count -and $Parameter.Count) {
        if ($Parameter.Count -eq 1 -and $Parameter[0] -eq '*') {
            # Trace all parameters.
            foreach ($key in $InvocationInfo.BoundParameters.Keys) {
                Write-Verbose " $($key): '$($InvocationInfo.BoundParameters[$key])'"
            }
        } else {
            # Trace matching parameters.
            foreach ($key in $InvocationInfo.BoundParameters.Keys) {
                foreach ($p in $Parameter) {
                    if ($key -like $p) {
                        Write-Verbose " $($key): '$($InvocationInfo.BoundParameters[$key])'"
                        break
                    }
                }
            }
        }
    }

    # Trace all unbound arguments.
    if (@($InvocationInfo.UnboundArguments).Count) {
        for ($i = 0 ; $i -lt $InvocationInfo.UnboundArguments.Count ; $i++) {
            Write-Verbose " args[$i]: '$($InvocationInfo.UnboundArguments[$i])'"
        }
    }
}

function Trace-VstsLeavingInvocation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [System.Management.Automation.InvocationInfo]$InvocationInfo)

    Write-Verbose "Leaving $(Get-InvocationDescription $InvocationInfo)."
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

	$variableName = "input" + $name.replace(".", "")

	$variableValue = Get-Variable -Name $variableName -ValueOnly

	Write-Verbose "The value for variable: '$name': $variableValue"

	return $variableValue
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


function Get-VstsLocString {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 1)]
        [string]$Key,
        [Parameter(Position = 2)]
        [object[]]$ArgumentList = @( ))

		return $Key
}

function global:Get-VstsTaskVariable {
 [CmdletBinding()]
 param (
        [string]$name,
        [switch]$Require)

	$variableName = "task" + $name.replace(".", "")

	$variableValue = Get-Variable -Name $variableName -ValueOnly

	Write-Verbose "The value for variable: '$name': $variableValue"

	return $variableValue
}

function Invoke-VstsTool {
    [CmdletBinding()]
    param(
        [ValidatePattern('^[^\r\n]*$')]
        [Parameter(Mandatory = $true)]
        [string]$FileName,
        [ValidatePattern('^[^\r\n]*$')]
        [Parameter()]
        [string]$Arguments,
        [string]$WorkingDirectory,
        [System.Text.Encoding]$Encoding,
        [switch]$RequireExitCodeZero)

    Trace-VstsEnteringInvocation $MyInvocation
    $isPushed = $false
    $originalEncoding = $null
    try {
        if ($Encoding) {
            $originalEncoding = [System.Console]::OutputEncoding
            [System.Console]::OutputEncoding = $Encoding
        }

        if ($WorkingDirectory) {
            Push-Location -LiteralPath $WorkingDirectory -ErrorAction Stop
            $isPushed = $true
        }

        $FileName = $FileName.Replace('"', '').Replace("'", "''")
        Write-Host "##[command]""$FileName"" $Arguments"
        Invoke-Expression "& '$FileName' --% $Arguments"
        Write-Verbose "Exit code: $LASTEXITCODE"
        if ($RequireExitCodeZero -and $LASTEXITCODE -ne 0) {
            Write-Error (Get-LocString -Key PSLIB_Process0ExitedWithCode1 -ArgumentList ([System.IO.Path]::GetFileName($FileName)), $LASTEXITCODE)
        }
    } finally {
        if ($originalEncoding) {
            [System.Console]::OutputEncoding = $originalEncoding
        }

        if ($isPushed) {
            Pop-Location
        }

        Trace-VstsLeavingInvocation $MyInvocation
    }
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
    
    Write-Host "The result of the Task is: $Result, Message: $Message";
}

function Get-InvocationDescription {
    [CmdletBinding()]
    param([System.Management.Automation.InvocationInfo]$InvocationInfo)

    if ($InvocationInfo.MyCommand.Path) {
        $InvocationInfo.MyCommand.Path
    } elseif ($InvocationInfo.MyCommand.Name) {
        $InvocationInfo.MyCommand.Name
    } else {
        $InvocationInfo.MyCommand.CommandType
    }
}