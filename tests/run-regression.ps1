<#
  Chạy bộ test hồi quy (không cần phần cứng) trên một hoặc hai bản build của Vision Image Tool.

  Dùng:
    .\tests\run-regression.ps1 -Exe "Vision Image Tool\bin\Debug\Vision Image Tool.exe"
    .\tests\run-regression.ps1 -Exe "<bản mới>.exe" -BaselineExe "<bản cũ>.exe"   # so sánh 2 bản
#>
param(
    [Parameter(Mandatory = $true)][string]$Exe,
    [string]$BaselineExe
)
$ErrorActionPreference = 'Stop'
$here = if ($PSScriptRoot) { $PSScriptRoot } else { Join-Path (Get-Location) 'tests' }
$out = Join-Path $env:TEMP 'vit_regression'
New-Item -ItemType Directory -Force $out | Out-Null
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$runner = Join-Path $out 'RegressionTests.exe'
& $csc -nologo -codepage:65001 -platform:x64 -target:exe "-out:$runner" `
    -r:System.Windows.Forms.dll -r:System.Drawing.dll (Join-Path $here 'RegressionTests\RegressionTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Biên dịch bộ test thất bại.' }

function Invoke-Suite([string]$exePath, [string]$tag) {
    # Chạy trong thư mục của exe để nạp được các DLL đi kèm (Newtonsoft.Json...).
    $exeFull = (Resolve-Path $exePath).Path
    $dir = Split-Path -Parent $exeFull
    $copy = Join-Path $dir 'RegressionTests.exe'
    Copy-Item $runner $copy -Force
    $result = Join-Path $out "result_$tag.txt"
    Push-Location $dir
    try { & $copy $exeFull (Join-Path $out "work_$tag") | Out-File -Encoding utf8 $result; $code = $LASTEXITCODE }
    finally { Pop-Location; Remove-Item $copy -Force -ErrorAction SilentlyContinue }
    # Bỏ đường dẫn thư mục tạm khỏi output để 2 bản so sánh được với nhau.
    (Get-Content $result -Encoding utf8) -replace [regex]::Escape((Join-Path $out "work_$tag")), '<work>' | Set-Content -Encoding utf8 $result
    "[$tag] exit=$code -> $result"
    return $result
}

$new = Invoke-Suite $Exe 'current'
Get-Content $new[-1] -Encoding utf8 | Select-Object -Last 1
if ($BaselineExe) {
    $base = Invoke-Suite $BaselineExe 'baseline'
    $diff = Compare-Object (Get-Content $base[-1] -Encoding utf8) (Get-Content $new[-1] -Encoding utf8)
    if ($diff) { 'HỒI QUY: output khác baseline:'; $diff | Format-Table -AutoSize | Out-String -Width 250 }
    else { 'REGRESSION PASS: output giống hệt baseline.' }
}
