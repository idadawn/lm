$path = "e:\project\2025\lm\web\src\views\lab\intermediateDataFormula\components\AdvancedJudgmentEditor.vue"
$lines = Get-Content $path
# Index 80 is Line 81.
if ($lines[80] -notmatch "<!-- 原条件组卡片实现") { 
    Write-Host "Error: Line 81 mismatch. Got: $($lines[80])"; 
    exit 1 
}

# Index 311 is Line 312.
if ($lines[311].Trim() -ne "</div>") { 
    Write-Host "Error: Line 312 mismatch. Got: $($lines[311])"; 
    exit 1 
}

# Delete range [80, 311] inclusive (Lines 81-312)
# Keep [0..79] + [312..End]
$newLines = $lines[0..79] + $lines[312..($lines.Count - 1)]

# Using [System.IO.File] to write UTF8 without BOM
[System.IO.File]::WriteAllLines($path, $newLines)

Write-Host "Success"
