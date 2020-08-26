if (-NOT (Test-Path secrets.json)) {
    Copy-Item secrets.template.json secrets.json
}

$global:secrets = Get-Content secrets.json | ConvertFrom-Json

function Get-Secret {
    param([String]$secretPropertyPath)

    $paths = $secretPropertyPath.Split(".")
    $value = $global:secrets
    ForEach ($path in $paths) {
        $value = $value | Select-Object -ExpandProperty $path
    }
    return $value
}
function Transform {
    param([String]$templateFilePath)

    $template = Get-Content $templateFilePath -Raw

    $final = [regex]::Replace($template, "<([\w\d\.]+)>", {
        $match = $args[0];
        $property = $match.Groups[1]
        $value = Get-Secret $property
        $value
    })

    $finalFilePath = $templateFilePath -replace ".template", ""

    Set-Content $finalFilePath $final
}

# shared
Transform PowerPlannerAppDataLibrary\App\Secrets.template.cs

# Android
Transform PowerPlannerAndroid\App\Secrets.template.cs
Transform PowerPlannerAndroid\google-services.template.json

# iOS
Transform PowerPlanneriOS\App\Secrets.template.cs

# UWP
Transform PowerPlannerUWP\App\Secrets.template.cs