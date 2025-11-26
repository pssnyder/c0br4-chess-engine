#!/usr/bin/env python3
"""
C0BR4 v3.3 Adaptive Time Management Test Suite
Tests the engine's depth and time allocation across various positions and time controls
"""

import subprocess
import time
import sys
from dataclasses import dataclass
from typing import List, Tuple

@dataclass
class TestPosition:
    name: str
    fen: str
    phase: str  # "opening", "middlegame", "endgame"

@dataclass
class TimeControl:
    name: str
    wtime: int  # milliseconds
    btime: int
    winc: int
    binc: int
    expected_depth: int

@dataclass
class TestResult:
    position: str
    phase: str
    time_control: str
    target_depth: int
    reached_depth: int
    time_allocated: int
    time_used: int
    nodes: int
    move: str

# Test positions representing different game phases
TEST_POSITIONS = [
    TestPosition(
        "Starting Position",
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
        "opening"
    ),
    TestPosition(
        "King's Pawn Opening",
        "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1",
        "opening"
    ),
    TestPosition(
        "Complex Middlegame",
        "r3k2r/p1ppqpb1/Bn2pnp1/3PN3/1p2P3/2N2Q2/PPPB1PpP/R3K2R w KQkq - 0 1",
        "middlegame"
    ),
    TestPosition(
        "Tactical Middlegame",
        "r1bqk2r/ppp2ppp/2n2n2/2bpp3/2B1P3/2NP1N2/PPP2PPP/R1BQK2R w KQkq - 0 1",
        "middlegame"
    ),
    TestPosition(
        "Queen Endgame",
        "8/8/4k3/8/8/3K4/3Q4/8 w - - 0 1",
        "endgame"
    ),
    TestPosition(
        "Rook and Pawn Endgame",
        "8/5pk1/6p1/7p/R7/5PPP/r5PK/8 w - - 0 1",
        "endgame"
    ),
    TestPosition(
        "King and Pawn Endgame",
        "8/8/8/4k3/4P3/4K3/8/8 w - - 0 1",
        "endgame"
    )
]

# Time controls from bullet to classical
TIME_CONTROLS = [
    TimeControl("Ultra-Bullet", 15000, 15000, 0, 0, 2),      # 15s total
    TimeControl("Bullet", 30000, 30000, 0, 0, 4),            # 30s total
    TimeControl("Bullet+Inc", 60000, 60000, 1000, 1000, 5),  # 1+1
    TimeControl("Blitz", 180000, 180000, 2000, 2000, 6),     # 3+2
    TimeControl("Blitz Fast", 300000, 300000, 0, 0, 7),      # 5+0
    TimeControl("Rapid", 600000, 600000, 0, 0, 8),           # 10+0
    TimeControl("Rapid Long", 900000, 900000, 0, 0, 9),      # 15+0
    TimeControl("Classical", 1800000, 1800000, 0, 0, 10),    # 30+0
]

class C0BR4Tester:
    def __init__(self, engine_path: str):
        self.engine_path = engine_path
        self.results: List[TestResult] = []
        
    def start_engine(self) -> subprocess.Popen:
        """Start the C0BR4 engine"""
        return subprocess.Popen(
            [self.engine_path],
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            bufsize=1
        )
    
    def send_command(self, proc: subprocess.Popen, command: str):
        """Send a command to the engine"""
        print(f"  → {command}")
        proc.stdin.write(command + "\n")
        proc.stdin.flush()
    
    def read_until(self, proc: subprocess.Popen, keywords: List[str], timeout: float = 60.0) -> List[str]:
        """Read engine output until one of the keywords is found"""
        lines = []
        start_time = time.time()
        
        while time.time() - start_time < timeout:
            try:
                line = proc.stdout.readline().strip()
                if line:
                    print(f"  ← {line}")
                    lines.append(line)
                    if any(keyword in line for keyword in keywords):
                        break
            except:
                break
                
        return lines
    
    def parse_test_result(self, lines: List[str], position: TestPosition, tc: TimeControl) -> TestResult:
        """Parse the engine output to extract test results"""
        target_depth = 0
        reached_depth = 0
        time_allocated = 0
        time_used = 0
        nodes = 0
        best_move = "none"
        
        for line in lines:
            if "Target depth:" in line:
                parts = line.split("Target depth:")
                if len(parts) > 1:
                    target_depth = int(parts[1].split(",")[0].strip())
                    
            if "Time allocation:" in line:
                parts = line.split("Time allocation:")
                if len(parts) > 1:
                    time_allocated = int(parts[1].split("ms")[0].strip())
                    
            if line.startswith("info depth"):
                parts = line.split()
                try:
                    depth_idx = parts.index("depth") + 1
                    reached_depth = max(reached_depth, int(parts[depth_idx]))
                    
                    time_idx = parts.index("time") + 1
                    time_used = int(parts[time_idx])
                    
                    nodes_idx = parts.index("nodes") + 1
                    nodes = int(parts[nodes_idx])
                except:
                    pass
                    
            if line.startswith("bestmove"):
                parts = line.split()
                if len(parts) > 1:
                    best_move = parts[1]
        
        return TestResult(
            position=position.name,
            phase=position.phase,
            time_control=tc.name,
            target_depth=target_depth,
            reached_depth=reached_depth,
            time_allocated=time_allocated,
            time_used=time_used,
            nodes=nodes,
            move=best_move
        )
    
    def test_position(self, position: TestPosition, tc: TimeControl) -> TestResult:
        """Test a single position with a specific time control"""
        print(f"\n{'='*80}")
        print(f"Testing: {position.name} ({position.phase})")
        print(f"Time Control: {tc.name} (Expected depth: {tc.expected_depth})")
        print(f"FEN: {position.fen}")
        print(f"{'='*80}")
        
        proc = self.start_engine()
        
        try:
            # Initialize engine
            self.send_command(proc, "uci")
            self.read_until(proc, ["uciok"])
            
            self.send_command(proc, "isready")
            self.read_until(proc, ["readyok"])
            
            # Set position
            self.send_command(proc, f"position fen {position.fen}")
            
            # Start search
            go_cmd = f"go wtime {tc.wtime} btime {tc.btime} winc {tc.winc} binc {tc.binc}"
            self.send_command(proc, go_cmd)
            
            # Wait for result
            lines = self.read_until(proc, ["bestmove"], timeout=120.0)
            
            # Parse result
            result = self.parse_test_result(lines, position, tc)
            
            # Quit engine
            self.send_command(proc, "quit")
            proc.wait(timeout=5)
            
            return result
            
        except Exception as e:
            print(f"  ERROR: {e}")
            proc.kill()
            return None
    
    def run_full_test_suite(self):
        """Run all combinations of positions and time controls"""
        print("\n" + "="*80)
        print("C0BR4 v3.3 ADAPTIVE TIME MANAGEMENT TEST SUITE")
        print("="*80)
        
        # Test each time control with representative positions
        # To save time, we'll test each time control with one position per phase
        
        for tc in TIME_CONTROLS:
            print(f"\n{'#'*80}")
            print(f"# TIME CONTROL: {tc.name}")
            print(f"{'#'*80}")
            
            # Test one position per phase for this time control
            opening_pos = TEST_POSITIONS[0]  # Starting position
            middlegame_pos = TEST_POSITIONS[2]  # Complex middlegame
            endgame_pos = TEST_POSITIONS[4]  # Queen endgame
            
            for pos in [opening_pos, middlegame_pos, endgame_pos]:
                result = self.test_position(pos, tc)
                if result:
                    self.results.append(result)
                time.sleep(1)  # Brief pause between tests
    
    def print_summary(self):
        """Print a summary table of all test results"""
        print("\n" + "="*80)
        print("TEST RESULTS SUMMARY")
        print("="*80)
        print()
        
        # Group by time control
        by_tc = {}
        for result in self.results:
            if result.time_control not in by_tc:
                by_tc[result.time_control] = []
            by_tc[result.time_control].append(result)
        
        for tc_name, results in by_tc.items():
            print(f"\n{tc_name}:")
            print(f"{'Position':<30} {'Phase':<12} {'Target':<8} {'Reached':<8} {'Time(ms)':<10} {'Nodes':<10}")
            print("-" * 80)
            
            for r in results:
                depth_status = "✓" if r.reached_depth >= r.target_depth else "⚠"
                print(f"{r.position:<30} {r.phase:<12} {r.target_depth:<8} {r.reached_depth:<8} {r.time_used:<10} {r.nodes:<10} {depth_status}")
        
        print("\n" + "="*80)
        print("DEPTH ACHIEVEMENT ANALYSIS")
        print("="*80)
        
        met_target = sum(1 for r in self.results if r.reached_depth >= r.target_depth)
        total = len(self.results)
        print(f"Reached target depth: {met_target}/{total} ({100*met_target//total}%)")
        
        avg_depth = sum(r.reached_depth for r in self.results) / len(self.results)
        print(f"Average depth reached: {avg_depth:.1f}")
        
        # Check specific goals
        blitz_results = [r for r in self.results if "Blitz" in r.time_control]
        classical_results = [r for r in self.results if "Classical" in r.time_control]
        
        if blitz_results:
            avg_blitz_depth = sum(r.reached_depth for r in blitz_results) / len(blitz_results)
            print(f"\nBlitz (3min) average depth: {avg_blitz_depth:.1f} (Goal: ≥6)")
            
        if classical_results:
            avg_classical_depth = sum(r.reached_depth for r in classical_results) / len(classical_results)
            print(f"Classical (30min) average depth: {avg_classical_depth:.1f} (Goal: ≥10)")

def main():
    if len(sys.argv) < 2:
        print("Usage: python test_adaptive_time_management.py <path_to_C0BR4_v3.3.exe>")
        print("Example: python test_adaptive_time_management.py ../deployed/C0BR4_v3.3/C0BR4_v3.3.exe")
        sys.exit(1)
    
    engine_path = sys.argv[1]
    tester = C0BR4Tester(engine_path)
    
    try:
        tester.run_full_test_suite()
        tester.print_summary()
    except KeyboardInterrupt:
        print("\n\nTest interrupted by user")
        tester.print_summary()
    except Exception as e:
        print(f"\n\nTest failed with error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
