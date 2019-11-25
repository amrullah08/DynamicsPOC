param(
$solutionImportPath
)


foreach($file in Get-ChildItem -Path $solutionImportPath)
{
    $uniqueName=$file.Name
    Write-Host $file.Name
    Write-Host "##vso[task.setvariable variable=ReleaseSolutionName]$uniqueName"
}