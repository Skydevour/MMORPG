$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$relativeTargets = @(
    'Builds/DEV-006', 'Builds/DEV-007', 'Builds/DEV-008',
    'Builds/DEV-009/MMORPG_BurstDebugInformation_DoNotShip',
    'Plan/Validation/DEV-006', 'Plan/Validation/DEV-007',
    'Plan/Validation/DEV-008', 'Plan/Validation/DEV-009-CD',
    'Assets/InitTestScene15805fef-0408-460e-8444-a7fdcf4231cf.unity',
    'Assets/InitTestScene15805fef-0408-460e-8444-a7fdcf4231cf.unity.meta'
)
$targets = @($relativeTargets | ForEach-Object { Join-Path $projectRoot $_ })
$targets += @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Plan') -File |
    Where-Object { $_.Name -match '^(unity_|player_|playmode_results_)' -and $_.Name -notmatch 'DEV-009' } |
    Select-Object -ExpandProperty FullName)
$records = @()
foreach ($target in $targets) {
    $absolute = [IO.Path]::GetFullPath($target)
    if (-not $absolute.StartsWith($projectRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "清理路径超出本项目：$absolute"
    }
    if (-not (Test-Path -LiteralPath $absolute)) { continue }
    $item = Get-Item -LiteralPath $absolute
    $files = if ($item.PSIsContainer) { @(Get-ChildItem -LiteralPath $absolute -File -Recurse) } else { @($item) }
    $records += [pscustomobject]@{ 路径 = $absolute; 文件数 = $files.Count; 字节数 = ($files | Measure-Object Length -Sum).Sum; 状态 = '待删除' }
}
$manifest = Join-Path $projectRoot 'Plan/Validation/DEV-009/旧产物清理.json'
$records | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifest -Encoding utf8
foreach ($record in $records) {
    Remove-Item -LiteralPath $record.路径 -Recurse -Force
    if (Test-Path -LiteralPath $record.路径) { throw "清理失败：$($record.路径)" }
    $record.状态 = '已删除'
    $records | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifest -Encoding utf8
}
Write-Output "已清理 $($records.Count) 个明确目标；保留当前包、测试源代码、PM-008设计图及其他项目。"
