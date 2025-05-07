# Путь к итоговому файлу
$outputFile = "code_export.txt"

# Массив расширений файлов с кодом (при необходимости расширьте список)
$extensions = @("*.cs", "*.json", "*.yaml")

# Список имен папок, которые нужно игнорировать
$ignoreFolders = @("obj", "bin", "utils")

# Если итоговый файл уже существует – удаляем его
if (Test-Path $outputFile) {
    Remove-Item $outputFile
}

# Функция для проверки, должен ли файл/каталог игнорироваться
function Should-IgnorePath($fullPath, $ignoreList) {
    foreach ($folder in $ignoreList) {
        # Проверяем, содержит ли путь сегмент с именем папки
        if ($fullPath -match "(\\|/)$folder(\\|/|$)") {
            return $true
        }
    }
    return $false
}

# Функция для рекурсивного построения дерева каталогов с учётом игнорирования
function Get-CustomTree {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Path,
        [string[]]$IgnoreDirs = @(),
        [string]$Prefix = ""
    )
    # Получаем каталоги и файлы, игнорируя скрытые (начинающиеся с .)
    $items = Get-ChildItem -Path $Path -Force | Where-Object { $_.Name -notmatch '^\.' } | Sort-Object Name

    # Фильтруем элементы: если это каталог и его имя есть в списке игнорирования – исключаем
    $items = $items | Where-Object {
        if ($_.PSIsContainer) {
            -not ($IgnoreDirs -contains $_.Name)
        }
        else {
            $true
        }
    }
    
    $count = $items.Count
    for ($i = 0; $i -lt $count; $i++) {
        $item = $items[$i]
        $isLast = ($i -eq $count - 1)
        if ($isLast) {
            $line = "$Prefix└── $($item.Name)"
        }
        else {
            $line = "$Prefix├── $($item.Name)"
        }
        Write-Output $line

        if ($item.PSIsContainer) {
            $newPrefix = $Prefix
            if ($isLast) {
                $newPrefix += "    "
            }
            else {
                $newPrefix += "│   "
            }
            Get-CustomTree -Path $item.FullName -IgnoreDirs $IgnoreDirs -Prefix $newPrefix
        }
    }
}

# Получаем дерево каталогов, начиная с текущего каталога, с игнорированием папок из $ignoreFolders
$treeOutput = Get-CustomTree -Path (Get-Location) -IgnoreDirs $ignoreFolders

# Сохраняем дерево каталогов в итоговый файл
$treeOutput | Out-File -FilePath $outputFile -Encoding utf8

# Добавляем разделитель перед выводом файлов с кодом
Add-Content -Path $outputFile -Value "`n===== Кодовые файлы =====`n"

# Обходим файлы с заданными расширениями, игнорируя те, что находятся в папках из списка игнорирования
foreach ($ext in $extensions) {
    Get-ChildItem -Recurse -Filter $ext -File | Where-Object {
        -not (Should-IgnorePath $_.FullName $ignoreFolders)
    } | ForEach-Object {
        $filePath = $_.FullName

        # Добавляем заголовок с путем к файлу
        Add-Content -Path $outputFile -Value "----------------------------------------"
        Add-Content -Path $outputFile -Value "File: $filePath"
        Add-Content -Path $outputFile -Value "----------------------------------------"

        # Добавляем содержимое файла
        Get-Content $_.FullName | Out-File -FilePath $outputFile -Append -Encoding utf8
        Add-Content -Path $outputFile -Value "`n"
    }
}
