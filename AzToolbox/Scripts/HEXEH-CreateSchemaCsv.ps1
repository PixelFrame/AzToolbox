$Root = Resolve-Path $PSScriptRoot\..\wwwroot\assets\HEXEH
$schemas = Get-ChildItem -Path $Root -Filter *.json -Recurse
$linebreak = "`n"
$result = ''
foreach ($schema in $schemas)
{
    $relative = [System.IO.Path]::GetRelativePath($Root, $schema.FullName).Replace('\', '/')
    $result += "$($schema.Name),$relative"
    $result += $linebreak
}
Out-File -FilePath $Root\schemas.csv -Encoding utf8NoBOM -InputObject $result -NoNewline