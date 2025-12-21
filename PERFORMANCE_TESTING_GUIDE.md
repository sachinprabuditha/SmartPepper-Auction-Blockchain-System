# 🧪 Performance Testing Guide

## Overview

This testing suite validates SmartPepper's critical performance requirements:

1. **Auction Latency**: <300ms for real-time bid updates (Research Requirement)
2. **Compliance Speed**: <2 seconds for compliance validation
3. **Gas Efficiency**: Optimized smart contract costs

## Prerequisites

### Required Services Running:

- ✅ PostgreSQL (port 5432)
- ✅ Redis (port 6379)
- ✅ Backend server (port 3002)
- ✅ Hardhat local blockchain (port 8545)

### Quick Setup:

```powershell
# Terminal 1: Start blockchain
cd blockchain
npm run node

# Terminal 2: Deploy contract
cd blockchain
npm run deploy:local
# Copy contract address to backend/.env and web/.env.local

# Terminal 3: Start backend
cd backend
npm run dev

# Terminal 4: Ready for tests
cd backend
```

---

## Test Suite 1: Auction Latency 🏎️

**Purpose:** Validate WebSocket real-time bid propagation meets <300ms requirement

**Research Requirement:** Sub-Objective 2 - Real-time auction engine with <300ms latency

### Run Test:

```powershell
cd backend
npm run test:latency
```

### What It Tests:

- ✅ Connects 10 concurrent auction participants
- ✅ Measures connection time
- ✅ Places 50 bids sequentially
- ✅ Tests concurrent bid storm (20 simultaneous bids)
- ✅ Calculates p50, p95, p99 latencies
- ✅ Validates p95 < 300ms

### Expected Output:

```
🚀 Starting Auction Latency Performance Tests

Configuration:
   Backend:      http://localhost:3002
   Auction ID:   1
   Participants: 10
   Bids/Client:  5
   Threshold:    300ms

📡 Connecting 10 participants...

✓ Client 1 connected in 45.23ms
✓ Client 2 connected in 38.91ms
...

📊 Testing bid propagation latency...

🔨 Bid #1: 1.00 ETH
   ⏱  Avg: 85.34ms | Max: 142.56ms
🔨 Bid #2: 1.10 ETH
   ⏱  Avg: 92.18ms | Max: 156.73ms
...

============================================================
📈 PERFORMANCE TEST RESULTS
============================================================

⚡ Bid Propagation Latency:
   Measurements: 450
   Average:      95.23ms
   Median (p50): 88.45ms
   p95:          187.34ms
   p99:          245.67ms
   Max:          289.12ms

🎯 Requirement Validation:
   Threshold:    300ms (p95)
   Actual:       187.34ms
   Status:       ✅ PASSED

============================================================
```

### Interpreting Results:

**✅ PASSED** - p95 < 300ms

- System meets research requirements
- Real-time auction feasible

**❌ FAILED** - p95 > 300ms

- Check Redis connection (should be localhost)
- Verify network latency
- Review backend WebSocket implementation
- Consider server hardware upgrade

### Troubleshooting:

**Error: "connect ECONNREFUSED"**

```powershell
# Backend not running
cd backend
npm run dev
```

**Error: "Redis connection failed"**

```powershell
# Start Redis
docker run -d --name smartpepper-redis -p 6379:6379 redis:7-alpine
```

---

## Test Suite 2: Compliance Engine ⚖️

**Purpose:** Measure compliance validation performance

**Research Requirement:** Sub-Objective 3 - Automated compliance blocking

### Run Test:

```powershell
cd backend
npm run test:compliance
```

### What It Tests:

- ✅ Creates test lot with full data
- ✅ Adds processing stages (harvest, drying, grading)
- ✅ Adds 3 certifications (organic, fumigation, phytosanitary)
- ✅ Runs compliance checks for EU, FDA, Middle East
- ✅ Tests consistency (3 iterations per destination)
- ✅ Measures duration for each check

### Expected Output:

```
🚀 Starting Compliance Engine Performance Tests

Configuration:
   Backend:      http://localhost:3002
   Test Lot:     TEST-LOT-PERF-001
   Target:       <2000ms

📋 Setting up test data...
✓ Created test lot: TEST-LOT-PERF-001
✓ Added 3 processing stages
✓ Added 3 certifications
✅ Test data ready

🧪 Testing compliance checks...

🔄 Testing EU consistency (3 iterations)...

EU Compliance Check:
   Duration:   345.67ms
   Status:     passed
   Rules:      3 checked

📊 EU Consistency Metrics:
   Average:    352.34ms
   Min:        345.67ms
   Max:        361.23ms
   Std Dev:    6.45ms
   Range:      15.56ms

============================================================
📈 COMPLIANCE ENGINE PERFORMANCE RESULTS
============================================================

⚡ Overall Performance:
   Total Checks: 9
   Average:      368.45ms
   Min:          345.67ms
   Max:          412.89ms

📍 Per Destination:
   EU              356.23ms (3 checks)
   FDA             371.45ms (3 checks)
   MIDDLE_EAST     377.67ms (3 checks)

🎯 Performance Target:
   Target:       2000ms
   Actual:       368.45ms
   Status:       ✅ PASSED

============================================================
```

### Interpreting Results:

**✅ PASSED** - Avg < 2000ms

- Fast enough for user experience
- No blocking delays

**⚠️ SLOW** - Avg > 1000ms

- Check database indexes
- Review SQL query optimization
- Consider caching compliance rules

---

## Test Suite 3: Gas Cost Analysis ⛽

**Purpose:** Measure smart contract gas consumption

**Research Requirement:** Ensure affordable transaction costs

### Run Test:

```powershell
# Make sure blockchain is running first
cd blockchain
npm run node

# In another terminal:
cd blockchain
npx hardhat run test/gas-analysis.test.js --network localhost
```

### What It Tests:

- ✅ Creates lot (NFT minting)
- ✅ Creates auction
- ✅ Places first bid
- ✅ Places second bid (with escrow refund)
- ✅ Ends auction
- ✅ Settles auction (transfers NFT + funds)
- ✅ Calculates costs at 20, 50, 100, 200 gwei

### Expected Output:

```
⛽ Starting Smart Contract Gas Cost Analysis

📝 Deploying PepperAuction contract...
✅ Contract deployed at: 0x5FbDB2315678afecb367f032d93F642f64180aa3

📦 Testing createLot()...
   Estimated:    245,678
   Actual:       247,891

createLot:
   Gas Used:     247,891
   Costs (ETH):
     low      (20 gwei):  0.004958 ETH
     medium   (50 gwei):  0.012395 ETH
     high     (100 gwei): 0.024789 ETH
     extreme  (200 gwei): 0.049578 ETH

🔨 Testing createAuction()...
   Estimated:    156,234
   Actual:       158,456

💰 Testing placeBid()...
   Estimated:    98,567
   Actual:       99,234

   Second bid (with refund):
   Actual:       112,345

============================================================
📊 GAS COST SUMMARY
============================================================

⛽ Total Gas Used: 925,678

💵 Total Cost for Full Workflow:
   low      ( 20 gwei): 0.018514 ETH
   medium   ( 50 gwei): 0.046284 ETH
   high     (100 gwei): 0.092568 ETH
   extreme  (200 gwei): 0.185136 ETH

📋 Operation Breakdown:

   createLot:
     Gas: 247,891
     % of Total: 26.8%

   createAuction:
     Gas: 158,456
     % of Total: 17.1%

   placeBid (first):
     Gas: 99,234
     % of Total: 10.7%

   settleAuction:
     Gas: 178,234
     % of Total: 19.3%

============================================================

💾 Results saved to: backend/logs/gas-analysis-results.json
```

### Interpreting Results:

**Gas Usage Benchmarks:**

- createLot: 200k-300k gas (NFT minting is expensive)
- createAuction: 150k-200k gas
- placeBid: 80k-120k gas
- settleAuction: 150k-200k gas (NFT transfer + payments)

**Cost Analysis (at 50 gwei):**

- Full workflow: ~0.05 ETH (~$120 at $2400 ETH)
- Single bid: ~0.005 ETH (~$12)

**Optimization Opportunities:**

- If total > 1M gas: Consider reducing storage operations
- If createLot > 300k: Review NFT minting logic
- If placeBid > 150k: Optimize escrow refund mechanism

---

## Running All Tests

### Quick Test Suite:

```powershell
cd backend
npm run test:all
```

This runs:

1. Auction latency test
2. Compliance timing test

### Full Analysis:

```powershell
# Terminal 1: Blockchain
cd blockchain
npm run node

# Terminal 2: Backend
cd backend
npm run dev

# Terminal 3: Run tests
cd backend
npm run test:latency
npm run test:compliance

# Terminal 4: Gas analysis
cd blockchain
npx hardhat run test/gas-analysis.test.js --network localhost
```

---

## Performance Metrics Summary

After running all tests, you should have:

### ✅ Auction Latency

- **Metric:** p95 latency < 300ms
- **Result:** \_\_\_ ms (PASSED/FAILED)

### ✅ Compliance Speed

- **Metric:** Average check < 2000ms
- **Result:** \_\_\_ ms (PASSED/FAILED)

### ✅ Gas Costs

- **Full Workflow:** \_\_\_ gas
- **Cost at 50 gwei:** \_\_\_ ETH
- **Per Bid:** \_\_\_ gas

---

## Next Steps After Testing

### If All Tests Pass ✅

1. Document results in research paper
2. Move to Priority 2: Enhanced Compliance Rules
3. Implement IPFS integration (Priority 3)

### If Latency Test Fails ❌

**Optimization Checklist:**

- [ ] Enable Redis caching
- [ ] Optimize WebSocket event broadcasting
- [ ] Review database query performance
- [ ] Consider server hardware upgrade
- [ ] Test with fewer concurrent connections

### If Compliance Test Slow ⚠️

**Optimization Checklist:**

- [ ] Add database indexes on lot_id, cert_type
- [ ] Cache compliance rules in memory
- [ ] Batch certificate validation queries
- [ ] Consider async validation for non-critical rules

### If Gas Costs Too High 💰

**Optimization Checklist:**

- [ ] Review storage vs memory usage
- [ ] Minimize on-chain data (use IPFS hashes)
- [ ] Batch operations where possible
- [ ] Consider L2 solution (Polygon, Arbitrum)

---

## Automated Performance Monitoring

### Setting Up Continuous Monitoring:

```javascript
// backend/scripts/monitor-performance.js
setInterval(async () => {
  const metrics = await collectMetrics();

  if (metrics.auctionLatency.p95 > 300) {
    sendAlert("Latency threshold exceeded!");
  }

  await logMetrics(metrics);
}, 60000); // Every minute
```

### Grafana Dashboard (Future):

- Real-time latency graph
- Compliance check duration trends
- Gas cost tracking
- Error rate monitoring

---

## Troubleshooting Common Issues

### "Cannot connect to backend"

```powershell
# Check backend is running
curl http://localhost:3002/api/health

# If not running:
cd backend
npm run dev
```

### "Database connection error"

```powershell
# Check PostgreSQL
docker ps | grep postgres

# Restart if needed
docker restart smartpepper-postgres
```

### "WebSocket connection failed"

```powershell
# Check Redis
docker ps | grep redis

# Restart if needed
docker restart smartpepper-redis
```

### "Smart contract not found"

```powershell
# Deploy contract
cd blockchain
npm run deploy:local

# Update .env files with new address
```

---

## Performance Testing Checklist

Before submitting research results:

- [ ] Auction latency test passed (p95 < 300ms)
- [ ] Compliance timing test passed (avg < 2s)
- [ ] Gas costs analyzed and documented
- [ ] Results saved to logs directory
- [ ] Performance metrics added to research paper
- [ ] Screenshots/graphs prepared for presentation
- [ ] All tests run on clean system (no dev tools skewing results)
- [ ] Multiple test runs for consistency (run 3 times, report average)

---

## Research Documentation Template

```markdown
### Performance Validation Results

**Auction Real-Time Performance:**

- Measurement period: [date]
- Concurrent participants: 10
- Total bids measured: 450
- p95 latency: XXX ms ✅ (requirement: <300ms)
- p99 latency: XXX ms
- Maximum latency: XXX ms

**Compliance Validation Performance:**

- Average check duration: XXX ms ✅
- EU checks: XXX ms
- FDA checks: XXX ms
- Middle East checks: XXX ms

**Smart Contract Gas Efficiency:**

- Full workflow gas: XXX
- Cost at 50 gwei: XXX ETH (~$XXX USD)
- Per-bid cost: XXX gas (~$XXX USD)

**Conclusion:** System meets all performance requirements for
real-time auction operations with automated compliance validation.
```

---

**Ready to test?** Make sure PostgreSQL, Redis, Backend, and Blockchain are running, then start with:

```powershell
cd backend
npm run test:latency
```

Good luck! 🚀
