if (Test-Path $Root\schemas.csv) { Remove-Item $Root\schemas.csv }
$Root = Resolve-Path $PSScriptRoot\..\wwwroot\assets\HEXEH
$schemas = Get-ChildItem -Path $Root -Filter *.json -Recurse
foreach ($schema in $schemas)
{
    $relative = [System.IO.Path]::GetRelativePath($Root, $schema.FullName)
    "$($schema.Name),$relative" | Out-File $Root\schemas.csv -Append
}