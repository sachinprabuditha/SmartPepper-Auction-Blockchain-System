# 🚀 Quick Start: Testing New Auction Creation System

## Prerequisites

Ensure you have a test lot that meets **ALL** requirements:

```bash
# Check your test lot status
curl http://192.168.8.116:3002/api/auctions/check-eligibility/LOT-1766820145306
```

Expected response:

```json
{
  "eligible": true,
  "reasons": [],
  "lot": {
    "lotId": "LOT-1766820145306",
    "certificateCount": 6,
    "stageCount": 4,
    "hasBlockchainPassport": true
  }
}
```

---

## Step 1: Restart Backend (Apply Changes)

```powershell
# Navigate to backend directory
cd d:\Campus\Research\SmartPepper\SmartPepper-Auction-Blockchain-System\backend

# Kill any running Node processes
Get-Process -Name node -ErrorAction SilentlyContinue | Stop-Process -Force

# Start backend
npm start
```

Expected output:

```
✓ Server running on port 3002
✓ PostgreSQL connected
✓ Blockchain service initialized
✓ Compliance service initialized
```

---

## Step 2: Rebuild Mobile App

```bash
# Navigate to mobile directory
cd d:\Campus\Research\SmartPepper\SmartPepper-Auction-Blockchain-System\mobile

# Clean build
flutter clean
flutter pub get

# Build and run
flutter run
```

Or hot-restart if already running:

```
Press 'R' in terminal (hot restart)
Or press Ctrl+\ (stop) then flutter run again
```

---

## Step 3: Test Eligibility Check (Happy Path)

### 3.1 **Open App**

1. Login as farmer
2. Navigate to **"Auctions"** tab
3. Tap **"Create Auction"** (+ button or menu)

### 3.2 **Select Eligible Lot**

- Should see list of approved lots
- Select `LOT-1766820145306` (or your test lot)

### 3.3 **Wait for Automatic Check**

You should see:

```
┌────────────────────────────────────┐
│  🔄 Checking Auction Eligibility   │
│                                    │
│  Verifying compliance, certificates│
│  and lot status                    │
└────────────────────────────────────┘
```

Then (if passing):

```
┌────────────────────────────────────┐
│  ✅ Lot Eligible for Auction ✓     │
│                                    │
│  All preconditions met. You can    │
│  proceed with auction creation.    │
└────────────────────────────────────┘
```

### 3.4 **Fill Form**

- **Reserve Price:** `8.50`
- **Duration:** Select `7 Days` (radio button)
- **Quantity:** `500` (pre-filled from lot)
- **Destinations:** Tap `🇪🇺 European Union` and `🇺🇸 United States`

### 3.5 **Create Auction**

1. Tap **"Create Auction"** button
2. Review confirmation dialog
3. Tap **"Confirm"**
4. Wait for blockchain transaction (~5-10 seconds)

### 3.6 **Verify Success**

Should see green snackbar:

```
✅ Auction Created Successfully! 🎉
Status: Scheduled
Auction ID: 1766820145
```

---

## Step 4: Test Eligibility Failure Scenarios

### Scenario A: Missing Certificates

```sql
-- Connect to PostgreSQL
psql -U postgres -d smartpepper_blockchain

-- Delete certificates to have only 1
DELETE FROM certifications
WHERE lot_id = 'LOT-1766820145306'
AND certification_id NOT IN (
  SELECT certification_id
  FROM certifications
  WHERE lot_id = 'LOT-1766820145306'
  LIMIT 1
);
```

**Test:**

1. Select the lot
2. Should see red eligibility card:

   ```
   ❌ Not Eligible for Auction

   Requirements:
   ❌ Minimum 3 certificates required (found 1)
   ```

3. Form should be **hidden**
4. Only "Go Back" and "Retry Check" buttons visible

**Recovery:**

```sql
-- Re-add certificates (or use your add certificate UI)
INSERT INTO certifications (lot_id, certification_type, issued_by, ...)
VALUES ('LOT-1766820145306', 'quality', 'Bureau Veritas', ...);
-- Repeat 2 more times
```

Then tap "Retry Check" → Should pass ✅

---

### Scenario B: Failed Compliance

```sql
-- Set compliance to 'pending'
UPDATE compliance_checks
SET status = 'pending'
WHERE lot_id = 'LOT-1766820145306'
ORDER BY checked_at DESC
LIMIT 1;
```

**Test:**

1. Select lot
2. Should see:
   ```
   ❌ Compliance status is "pending" (must be "passed" or "approved")
   ```

**Recovery:**

1. Go to **Lot Details** screen
2. Tap **"Run Compliance Check"**
3. Wait for 7/7 checks to pass
4. Return to Create Auction
5. Tap "Retry Check" → Should pass ✅

---

### Scenario C: Active Auction Exists

```sql
-- Create fake active auction
INSERT INTO auctions (
  auction_id, lot_id, farmer_address,
  start_price, reserve_price,
  start_time, end_time, status
) VALUES (
  999999,
  'LOT-1766820145306',
  '0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb',
  10, 8,
  NOW(),
  NOW() + INTERVAL '7 days',
  'active'
);
```

**Test:**

1. Try to create auction
2. Should see:
   ```
   ❌ This lot already has an active or scheduled auction
   ```

**Recovery:**

```sql
-- End the fake auction
UPDATE auctions
SET status = 'ended', end_time = NOW() - INTERVAL '1 hour'
WHERE auction_id = 999999;
```

---

## Step 5: Backend API Testing (Postman/cURL)

### Test Eligibility Check

```bash
curl http://192.168.8.116:3002/api/auctions/check-eligibility/LOT-1766820145306
```

**Expected (eligible):**

```json
{
  "eligible": true,
  "reasons": [],
  "lot": {
    "lotId": "LOT-1766820145306",
    "variety": "Black Pepper",
    "quantity": 500,
    "status": "approved",
    "certificateCount": 6,
    "stageCount": 4,
    "hasBlockchainPassport": true
  }
}
```

### Test Auction Creation

```bash
curl -X POST http://192.168.8.116:3002/api/auctions \
  -H "Content-Type: application/json" \
  -d '{
    "lotId": "LOT-1766820145306",
    "farmerAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
    "reservePrice": 8.50,
    "quantity": 500,
    "duration": 7,
    "preferredDestinations": ["EU", "USA"]
  }'
```

**Expected Success:**

```json
{
  "success": true,
  "message": "Auction created successfully",
  "auction": {
    "auctionId": 1766820145,
    "lotId": "LOT-1766820145306",
    "status": "scheduled",
    "startTime": "2025-01-28T10:00:00Z",
    "endTime": "2025-02-04T10:00:00Z",
    "blockchainTxHash": "0x1a2b3c4d5e...",
    "onChainData": {
      "immutable": true,
      "reservePrice": 8.5
    },
    "offChainData": {
      "volatile": true,
      "preferredDestinations": ["EU", "USA"]
    }
  }
}
```

---

## Step 6: Verify Database

```sql
-- Check auction was created
SELECT
  auction_id,
  lot_id,
  reserve_price,
  status,
  start_time,
  end_time,
  blockchain_tx_hash
FROM auctions
WHERE lot_id = 'LOT-1766820145306'
ORDER BY created_at DESC
LIMIT 1;
```

**Expected:**

- `auction_id`: Large number (timestamp-based)
- `status`: `'scheduled'` or `'active'`
- `reserve_price`: `8.50`
- `blockchain_tx_hash`: Present (not null)
- `start_time`: ~1 hour from now
- `end_time`: start_time + 7 days

---

## Step 7: Verify Blockchain

```bash
cd d:\Campus\Research\SmartPepper\SmartPepper-Auction-Blockchain-System\blockchain

# Open Hardhat console
npx hardhat console --network localhost
```

```javascript
// Get PepperAuction contract
const PepperAuction = await ethers.getContractFactory("PepperAuction");
const auction = PepperAuction.attach(
  "0x8A791620dd6260079BF849Dc5567aDC3F2FdC318"
);

// Get auction ID from database (e.g., 1766820145)
const auctionId = 1766820145;

// Check auction exists
const auctionData = await auction.getAuction(auctionId);
console.log("Auction on-chain:", auctionData);
```

**Expected:**

- `seller`: Farmer's wallet address
- `reservePrice`: 8.5 ETH equivalent
- `isActive`: true (if started) or false (if scheduled)
- `endTime`: Unix timestamp

---

## Troubleshooting

### ❌ "Cannot read property 'quality' of null"

**Cause:** Lot not selected properly  
**Fix:** Ensure lot selection sets `_selectedLot` state

### ❌ "404 Not Found: /api/auctions/check-eligibility"

**Cause:** Backend not restarted with new route  
**Fix:** Stop backend (Ctrl+C) and `npm start` again

### ❌ "Eligibility check returns 500 error"

**Cause:** Database connection or missing tables  
**Fix:**

```sql
-- Verify tables exist
\dt
-- Should see: auctions, pepper_lots, certifications, compliance_checks, processing_stages
```

### ❌ "Form shows but Create Auction button is disabled"

**Cause:** `_isEligible` is false  
**Fix:** Check eligibility result in console/logs

### ❌ "Blockchain transaction fails"

**Cause:** Local blockchain not running or out of gas  
**Fix:**

```powershell
# Start local blockchain
cd d:\Campus\Research\SmartPepper\SmartPepper-Auction-Blockchain-System\blockchain
npx hardhat node
```

---

## Success Criteria

All of these should work:

- [x] **Eligibility Check Endpoint** returns correct status
- [x] **Mobile UI** shows eligibility cards (blue → green/red)
- [x] **Form Validation** prevents invalid inputs
- [x] **Duration Presets** work (3/7/14/21 days)
- [x] **Destination Chips** are selectable
- [x] **Confirmation Dialog** shows summary
- [x] **Blockchain Transaction** succeeds
- [x] **Database Record** created with correct status
- [x] **Success Notification** appears
- [x] **Navigation** returns to previous screen

---

## Next Actions

After successful testing:

1. **Deploy to staging:** Update staging server with new code
2. **User acceptance testing:** Get farmer feedback
3. **Performance testing:** Load test eligibility endpoint
4. **Documentation:** Update user manual with new flow
5. **Training:** Train support team on new validation logic

---

## 📞 Support

If you encounter issues:

1. **Check backend logs:** `d:\...\backend\logs\app.log`
2. **Check mobile logs:** Flutter debug console
3. **Check database:** Verify lot exists and has required data
4. **Check blockchain:** Ensure contracts deployed and accessible

**Files to review:**

- Backend: [backend/src/routes/auction.js](./backend/src/routes/auction.js)
- Mobile: [mobile/lib/screens/farmer/create_auction_screen.dart](./mobile/lib/screens/farmer/create_auction_screen.dart)
- Docs: [NEW_AUCTION_CREATION_SYSTEM.md](./NEW_AUCTION_CREATION_SYSTEM.md)

---

_Testing Guide v1.0_  
_Last Updated: January 28, 2025_  
_Status: ✅ Ready for Testing_
