# C0BR4 v3.4 - Time Management Crisis Fix
**Release Date:** November 28, 2025  
**Priority:** URGENT - Addresses 18.7% time forfeit loss rate

## Executive Summary

C0BR4 v3.4 is an **emergency release** addressing critical time management issues discovered in production analysis. The bot was losing **17 out of 91 games (18.7%)** due to time forfeits, significantly impacting ELO performance.

## Critical Issues Identified

### Performance Metrics (Nov 28, 2025)
- **Total Games:** 91
- **Time Forfeit Losses:** 17 (18.7% of all games)
- **Impact:** Losing to opponents 200+ ELO lower due to time pressure
- **Pattern:** Losses concentrated in 3+5 and 5+5 blitz games

### Root Causes
1. **Adaptive depth targeting too aggressive** - Trying to reach depth 9-10 in time-limited games
2. **Concurrent game overload** - 2 simultaneous games causing time pressure
3. **Poor opponent selection** - Accepting challenges from 1900+ ELO bots (400+ rating difference)

## v3.4 Changes

### 1. Aggressive Time Conservation (`TimeManager.cs`)

#### Emergency Time Handling
```csharp
// OLD: 2s threshold with 5% usage
if (remainingTime < 2000) return Math.Max(50, remainingTime / 20);

// NEW: 3s threshold with 3% usage
if (remainingTime < 3000) return Math.Max(30, remainingTime / 30);
```

#### Low Time Optimization
```csharp
// OLD: 10s threshold with 7% usage
if (remainingTime < 10000) return remainingTime / 15 + increment / 2;

// NEW: 15s threshold with 5% usage  
if (remainingTime < 15000) return remainingTime / 20 + increment / 3;
```

#### Maximum Time Per Move
```csharp
// OLD: Up to 1/4 of remaining time
Math.Min(baseTime, remainingTime / 4)

// NEW: Up to 1/5 of remaining time
Math.Min(baseTime, remainingTime / 5)
```

### 2. Resource Conservation (`config.yml`)

#### Concurrency Reduction
```yaml
# OLD: concurrency: 2
# NEW: concurrency: 1  
# Reason: Prevents time pressure from managing multiple games
```

#### Stricter Opponent Filtering
```yaml
# OLD: opponent_rating_difference: 400
# NEW: opponent_rating_difference: 200
# Reason: Stop accepting games from 1900+ ELO bots we can't beat
```

#### Time Control Requirements
```yaml
# NEW: min_increment: 1  (was 0)
# NEW: min_base: 120     (was 60)
# NEW: max_base: 3600    (was 5400)
# Reason: Avoid ultra-fast games and overly long resource drain
```

## Expected Improvements

### Time Forfeit Rate
- **Current:** 18.7% forfeit loss rate
- **Target:** <5% forfeit loss rate
- **Mechanism:** More aggressive time conservation + single-game focus

### ELO Stability
- **Current:** Losing to 1200-1400 ELO bots on time
- **Target:** Maintain rating against similarly-rated opponents
- **Mechanism:** Better time management + opponent filtering

### Resource Usage
- **Current:** e2-small VM running 2 concurrent games
- **Target:** Same VM with 1 concurrent game (50% load reduction)
- **Benefit:** Can potentially downgrade to e2-micro (~$6/month savings)

## VM Downgrade Analysis

### Current Configuration
- **Machine Type:** e2-small
- **Specs:** 2 vCPUs, 2GB RAM
- **Monthly Cost:** ~$12-15/month
- **Usage:** Running 2 concurrent games + Docker overhead

### Recommended Downgrade Path
**Option A: e2-micro** (Recommended for v3.4)
- **Specs:** 2 vCPUs (shared), 1GB RAM
- **Monthly Cost:** ~$6-7/month  
- **Savings:** ~$6-8/month (50% reduction)
- **Viability:** With concurrency=1, C0BR4 (7MB binary) + lichess-bot (Python) should fit comfortably

**Option B: Stay on e2-small**
- Maintain current configuration
- Better headroom for spikes
- Allows future concurrent game increase

### Downgrade Recommendation
✅ **PROCEED with e2-micro downgrade**
- C0BR4 is explicitly a "secondary project" to v7p3r
- Single concurrent game significantly reduces memory pressure
- Engine binary is only ~7MB with minimal runtime memory
- Cost savings align with secondary project status
- Can always upgrade back if performance degrades

## Deployment Steps

### 1. Build v3.4 Binary (Linux x64)
```bash
cd "s:/Programming/Chess Engines/C0BR4 Chess Engine/cobra-chess-engine/src"
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

### 2. Upload to GCP
```bash
gcloud compute scp \
  ./publish/C0BR4_v3.4 \
  c0br4-production-bot:/home/c0br4/engines/C0BR4_v3.4/ \
  --project c0br4-lichess-bot \
  --zone us-central1-a

gcloud compute scp \
  ../lichess/config.yml \
  c0br4-production-bot:/home/c0br4/lichess-bot/ \
  --project c0br4-lichess-bot \
  --zone us-central1-a
```

### 3. Restart Bot
```bash
gcloud compute ssh c0br4-production-bot --project c0br4-lichess-bot --zone us-central1-a
docker restart lichess-bot-container
docker logs -f lichess-bot-container
```

### 4. (Optional) Downgrade VM
```bash
# Stop instance
gcloud compute instances stop c0br4-production-bot \
  --project c0br4-lichess-bot \
  --zone us-central1-a

# Change machine type
gcloud compute instances set-machine-type c0br4-production-bot \
  --machine-type e2-micro \
  --project c0br4-lichess-bot \
  --zone us-central1-a

# Start instance
gcloud compute instances start c0br4-production-bot \
  --project c0br4-lichess-bot \
  --zone us-central1-a
```

## Monitoring Plan

### Week 1 Post-Deployment
- **Daily:** Check time forfeit rate (target <5%)
- **Daily:** Monitor ELO stability (target: no rapid drops)
- **Daily:** Verify VM resource usage (RAM <80% on e2-micro)

### Week 2+
- **Weekly:** Review game statistics for time management
- **Weekly:** Assess if e2-micro is sufficient or needs e2-small
- **Monthly:** Compare cost savings vs performance

## Rollback Plan

If time forfeits remain >10% after 50 games:
1. Revert `TimeManager.cs` to v3.3 settings
2. Keep concurrency=1 and stricter opponent filter
3. Investigate search depth calculation issues

## Version History

- **v3.3** (Nov 27, 2025): Adaptive depth 6-10, conservative time management
- **v3.4** (Nov 28, 2025): **URGENT** - Aggressive time conservation, concurrency=1, stricter filtering

## Technical Details

### Files Modified
1. `src/C0BR4ChessEngine/Search/TimeManager.cs`
   - Emergency threshold: 2s → 3s
   - Max time per move: 1/4 → 1/5
   - Low time usage: 7% → 5%

2. `lichess/config.yml`
   - Concurrency: 2 → 1
   - Opponent difference: 400 → 200
   - Min increment: 0 → 1
   - Min base: 60s → 120s
   - Max base: 90m → 60m

### Binary Specifications
- **Platform:** Linux x64 (self-contained)
- **Size:** ~7MB (single-file deployment)
- **Dependencies:** None (includes .NET runtime)
- **UCI Protocol:** Fully compliant

## Cost-Benefit Analysis

### With e2-micro Downgrade
- **Monthly Savings:** ~$6-8/month (~50% reduction)
- **Annual Savings:** ~$72-96/year
- **Performance Trade-off:** Minimal (single concurrent game)
- **Risk:** Low (can upgrade in minutes if needed)

### Without Downgrade
- **Cost:** Maintain current ~$12-15/month
- **Benefit:** More headroom for future expansion
- **Alignment:** Not aligned with "secondary project" status

**Recommendation:** Downgrade to e2-micro for secondary project alignment.

---

**Deployment Authority:** v3.4 ready for immediate deployment  
**Next Review:** After 50 games or 1 week, whichever comes first
