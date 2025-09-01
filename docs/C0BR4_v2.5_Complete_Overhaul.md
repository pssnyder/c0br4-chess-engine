# C0BR4 v2.5 Complete Overhaul Project

## Problem Analysis
The C0BR4 engine is making illegal moves in tournament play, indicating fundamental issues with:
1. Move validation inconsistencies between different parts of the codebase
2. Mixed usage of old array-based and new bitboard-based systems
3. Incomplete UCI move parsing and application
4. Lack of comprehensive move validation at critical points

## Complete Overhaul Plan

### Phase 1: Core Architecture Audit
- [ ] Inventory all move-related classes and methods
- [ ] Identify all non-bitboard move generation/validation code
- [ ] Map dependencies between old and new systems
- [ ] Create comprehensive test suite for move validation

### Phase 2: Bitboard-Only Implementation
- [ ] Ensure Board class uses only bitboards
- [ ] Remove all legacy move generation code
- [ ] Implement robust UCI move parsing with bitboard validation
- [ ] Add comprehensive move validation at every critical point

### Phase 3: UCI Interface Hardening
- [ ] Rewrite UCI move parsing to be bulletproof
- [ ] Add extensive validation before every move application
- [ ] Implement failsafe mechanisms for invalid moves
- [ ] Add comprehensive logging for debugging

### Phase 4: Testing and Validation
- [ ] Create test suite covering all move types (normal, castling, en passant, promotion)
- [ ] Test with problematic positions from tournament failures
- [ ] Validate against known good engines
- [ ] Stress test with rapid move sequences

### Phase 5: Final Integration
- [ ] Clean up all dead code
- [ ] Optimize performance
- [ ] Final tournament validation
- [ ] Documentation and version release

## Critical Success Criteria
1. Engine NEVER makes an illegal move under any circumstances
2. All move generation uses bitboards exclusively
3. UCI interface is bulletproof and handles all edge cases
4. Comprehensive test coverage for all move scenarios
5. Clean, maintainable codebase with no legacy cruft

## Implementation Strategy
- Start with a clean slate approach for move validation
- Implement defense-in-depth: validate at multiple layers
- Use bitboard operations for ALL move-related functionality
- Add extensive error handling and logging
- Test every change immediately with known problematic positions
