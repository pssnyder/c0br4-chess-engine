#!/bin/bash

# C0BR4 v2.0 Validation Integration Test Script
# Tests the integration of MoveValidator and IllegalMoveDebugger

echo "=== C0BR4 v2.0 Validation Integration Test ==="
echo "Testing MoveValidator and IllegalMoveDebugger integration"
echo

cd "s:/Maker Stuff/Programming/Chess Engines/C0BR4 Chess Engine/cobra-chess-engine/src"

# Build the engine first
echo "Building C0BR4 engine..."
dotnet build
if [ $? -ne 0 ]; then
    echo "Build failed! Aborting test."
    exit 1
fi

echo "Build successful!"
echo

# Test basic UCI functionality with our new validation
echo "Testing basic UCI functionality with validation..."
echo "position startpos" | dotnet run
echo

# Create a simple test for move validation
cat > test_moves.uci << 'EOF'
position startpos
go depth 1
position startpos moves e2e4
go depth 1
position startpos moves e2e4 e7e5
go depth 1
quit
EOF

echo "Running UCI test with validation..."
cat test_moves.uci | dotnet run

echo
echo "=== Test completed ==="
echo "Check 'illegal_moves.log' for any validation issues"
echo "Check console output for validation results"

# Clean up
rm -f test_moves.uci
