param(
    [string]$Engine = "$PSScriptRoot\..\src\bin\Release\net6.0\win-x64\C0BR4_v4.0.exe"
)

$ErrorActionPreference = 'Stop'
$commands = @(
    'position startpos', 'perft', 'perft 4', 'perft 1',
    'position fen r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1',
    'perft 3', 'go movetime 100', 'perft 2',
    'go movetime 100', 'perft 2', 'quit'
)
$output = ($commands | & $Engine 2>&1) -join "`n"
if ($LASTEXITCODE -ne 0) { throw "Engine failed: $output" }
foreach ($required in @(
    'Depth 0: 1 nodes', 'Depth 1: 20 nodes', 'Depth 2: 400 nodes',
    'Depth 3: 8902 nodes', 'Perft(4): 197281 nodes', 'Perft(3): 97862 nodes'
)) {
    if (-not $output.Contains($required)) { throw "Missing $required`n$output" }
}
if ([regex]::Matches($output, 'Perft\(2\): 2039 nodes').Count -ne 2) {
    throw "Board not restored after interrupted searches:`n$output"
}
if ([regex]::Matches($output, '(?m)^bestmove [a-h][1-8][a-h][1-8][qrbn]?$').Count -ne 2) {
    throw "Expected two completed UCI responses:`n$output"
}
if (-not $output.Contains('Search deadline reached')) {
    throw "Deadline interruption was not exercised:`n$output"
}
Write-Output 'PASS: startpos and Kiwipete perft; deadline interruption; board restoration'
