#!/usr/bin/env python3
"""
Test Current Chess AI Performance

Compare our actual chess_ai.py implementation against the baseline goals.
"""

import chess
import time
import chess_ai

def test_chess_ai_performance():
    """Test our actual chess_ai implementation"""
    
    print("=" * 80)
    print("CHESS AI PERFORMANCE TEST")
    print("=" * 80)
    print("Testing our actual chess_ai.py implementation")
    print("Target: Minimax <2s/<4M nodes, AB <0.25s/<500k nodes, Full <0.025s/<5k nodes")
    print("=" * 80)
    
    # Test position: Complex middle game position
    test_fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"
    board = chess.Board(test_fen)
    depth = 4
    
    print(f"\\nTest Position: Complex Middle Game")
    print(f"FEN: {test_fen}")
    print(f"Search Depth: {depth}")
    print(f"Legal moves: {len(list(board.legal_moves))}")
    
    configs = [
        ("Base (Simple Negamax)", "base", 2.0, 4000000),
        ("Alpha-Beta Basic", "alphabeta", 0.25, 500000),
        ("Full Optimizations", "full", 0.025, 5000)
    ]
    
    results = []
    
    for config_name, config_type, time_goal, nodes_goal in configs:
        print(f"\\n--- Testing {config_name} ---")
        print(f"Goal: <{time_goal}s, <{nodes_goal:,} nodes")
        
        # Capture metrics
        final_nodes = 0
        final_score = 0
        final_nps = 0
        
        def capture_info(**info):
            nonlocal final_nodes, final_score, final_nps
            final_nodes = info.get('nodes', 0)
            final_score = info.get('score', 0)
            final_nps = info.get('nps', 0)
        
        # Run search with timeout protection
        try:
            start_time = time.time()
            best_move = chess_ai.search(
                board=board,
                depth=depth,
                info_callback=capture_info,
                config=config_type,
                time_limit=time_goal * 2  # 2x timeout safety
            )
            search_time = time.time() - start_time
            
            # Check if we got a result
            if best_move is None:
                print("  Result: FAILED - No move returned")
                continue
                
        except Exception as e:
            print(f"  Result: ERROR - {str(e)}")
            continue
        
        # Evaluate results
        time_status = "✓ PASS" if search_time < time_goal else "✗ FAIL"
        nodes_status = "✓ PASS" if final_nodes < nodes_goal else "✗ FAIL"
        
        print(f"  Time: {search_time:.3f}s (goal: <{time_goal}s) {time_status}")
        print(f"  Nodes: {final_nodes:,} (goal: <{nodes_goal:,}) {nodes_status}")
        print(f"  NPS: {final_nps:,}")
        print(f"  Score: {final_score}")
        print(f"  Best Move: {best_move}")
        
        # Calculate efficiency vs goals
        time_ratio = search_time / time_goal
        nodes_ratio = final_nodes / nodes_goal
        required_nps = nodes_goal / time_goal
        
        print(f"  Analysis:")
        print(f"    Time ratio: {time_ratio:.1f}x goal")
        print(f"    Nodes ratio: {nodes_ratio:.1f}x goal")
        print(f"    Required NPS: {required_nps:,.0f}")
        print(f"    Actual NPS: {final_nps:,}")
        print(f"    NPS gap: {required_nps/max(final_nps, 1):.1f}x needed")
        
        results.append({
            'name': config_name,
            'time': search_time,
            'nodes': final_nodes,
            'nps': final_nps,
            'time_status': time_status,
            'nodes_status': nodes_status,
            'move': best_move
        })
    
    # Summary
    print("\\n" + "="*80)
    print("PERFORMANCE SUMMARY")
    print("="*80)
    print(f"{'Configuration':<25} {'Time':<8} {'Nodes':<12} {'NPS':<12} {'Status':<10}")
    print("-" * 80)
    
    for result in results:
        status = f"{result['time_status'][0]}{result['nodes_status'][0]}"
        print(f"{result['name']:<25} {result['time']:<8.3f} {result['nodes']:<12,} "
              f"{result['nps']:<12,} {status:<10}")
    
    # Analysis
    print("\\n" + "="*50)
    print("ANALYSIS")
    print("="*50)
    
    if results:
        base_result = results[0] if results else None
        if base_result and base_result['nps'] > 0:
            print(f"\\nBase search NPS: {base_result['nps']:,}")
            print(f"Required NPS for goals: 2,000,000")
            print(f"Performance gap: {2000000/base_result['nps']:.1f}x slower than needed")
            
            if base_result['nps'] < 100000:
                print("\\n⚠️  Python vs C++ Performance Issue:")
                print("   Our Python implementation is significantly slower than")
                print("   the reference C++ engine benchmarks.")
                print("   Consider: JIT compilation (Numba), Cython, or revised goals.")
            
            if len(results) >= 2:
                ab_result = results[1]
                if ab_result['nodes'] > 0 and base_result['nodes'] > 0:
                    efficiency = (base_result['nodes'] - ab_result['nodes']) / base_result['nodes'] * 100
                    print(f"\\nAlpha-Beta efficiency: {efficiency:.1f}% node reduction")
                    
                if len(results) >= 3:
                    full_result = results[2]
                    if full_result['nodes'] > 0 and ab_result['nodes'] > 0:
                        ordering_efficiency = (ab_result['nodes'] - full_result['nodes']) / ab_result['nodes'] * 100
                        print(f"Move ordering efficiency: {ordering_efficiency:.1f}% additional reduction")

if __name__ == "__main__":
    test_chess_ai_performance()
