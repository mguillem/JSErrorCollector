# Add-Zip function written by Daiken, copied from:
# http://blogs.msdn.com/b/daiken/archive/2007/02/12/compress-files-with-windows-powershell-then-package-a-windows-vista-sidebar-gadget.aspx
function Add-Zip
{
	param([string]$zipfilename)


	if(-not (test-path($zipfilename)))
	{
		set-content $zipfilename ("PK" + [char]5 + [char]6 + ("$([char]0)" * 18))
		(dir $zipfilename).IsReadOnly = $false	
	}
	
	$shellApplication = new-object -com shell.application
	$zipPackage = $shellApplication.NameSpace($zipfilename)
	
	foreach($file in $input) 
	{ 
            $zipPackage.CopyHere($file.FullName)
            Start-sleep -milliseconds 500
	}
}



# determine path for source files and target
$invocation = (Get-Variable MyInvocation).Value
$buildPath = Split-Path $invocation.MyCommand.Path

$zipPath = [System.IO.Path]::GetFullPath($buildPath + '\..\..\dist\JSErrorCollector.zip')
$xpiPath = [System.IO.Path]::GetFullPath($buildPath + '\..\..\dist\JSErrorCollector.xpi')

$sourceDir = $buildPath + '\..\'

# build xpi
if (test-path($zipPath))
{
    rm $zipPath
}

if (test-path($xpiPath))
{
    rm $zipPath
}

dir $sourceDir | add-zip $zipPath
mv $zipPath $xpiPath


