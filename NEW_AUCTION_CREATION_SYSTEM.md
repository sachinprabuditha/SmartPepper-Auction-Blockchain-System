# 🎯 New Governance-Based Auction Creation System

## Overview

The auction creation system has been completely redesigned based on a **governance-first** approach. Instead of a simple form submission, auctions now go through a rigorous validation pipeline ensuring transparency, compliance, and immutability.

---

## 🏗️ System Architecture

### 1. **Actor Roles**

| Actor                 | Responsibility                                      |
| --------------------- | --------------------------------------------------- |
| **Farmer**            | Initiates auction request with simple inputs        |
| **Backend**           | Validates all preconditions and orchestrates flow   |
| **Compliance Engine** | Verifies certificates and processing stages         |
| **Smart Contract**    | Stores immutable auction terms on-chain             |
| **Admin**             | Defines governance rules (not involved in creation) |

---

## 📋 Preconditions (7 Checks)

Before a farmer can create an auction, **ALL** of the following must be true:

### 1. ✅ Lot Exists on Blockchain

- Lot must have a valid NFT passport (blockchain_tx_hash)
- Minted on PepperPassport contract

### 2. ✅ Farmer Owns the Lot

- `lot.farmer_address` must match logged-in farmer's wallet
- Prevents unauthorized auction creation

### 3. ✅ Required Certificates Uploaded

- Minimum **3 certificates** from these types:
  - Organic certification
  - Fumigation certificate
  - Quality inspection
  - Export permit
  - Phytosanitary certificate
  - Pesticide test results
  - Certificate of origin
  - Halal certification

### 4. ✅ Pre-Compliance Check Passed

- Latest compliance status must be `"passed"` or `"approved"`
- EU/FDA/Middle East market standards validated
- Example: 7/7 EU checks passing

### 5. ✅ Processing Stages Recorded

- Minimum **2 processing stages** for traceability:
  - Harvest
  - Drying
  - Grading
  - Packaging
  - Storage (optional)
- Ensures complete supply chain tracking

### 6. ✅ Lot Status Approved

- Lot status must be `"approved"` or `"available"`
- Not `"pending"`, `"rejected"`, or `"sold"`

### 7. ✅ No Active Auction Exists

- Only one auction per lot at a time
- Previous auction must be `"completed"`, `"cancelled"`, or `"ended"`

---

## 🎨 Farmer Input (Simplified)

Farmers provide **5 simple inputs** (non-technical):

| Input                       | Description                         | Example             |
| --------------------------- | ----------------------------------- | ------------------- |
| **Lot ID**                  | Select from eligible lots           | `LOT-1766820145306` |
| **Reserve Price**           | Minimum acceptable price            | `$8.50/kg`          |
| **Duration**                | Auction length (predefined options) | `7 days`            |
| **Quantity**                | Amount to auction                   | `500 kg`            |
| **Destinations** (Optional) | Preferred export markets            | `EU, USA, UAE`      |

**What Farmers DON'T Need to Provide:**

- ❌ Starting price (uses reserve price)
- ❌ Complex time pickers (calculated from duration)
- ❌ Technical blockchain details
- ❌ Compliance rules
- ❌ Gas fees or transaction details

---

## 🔄 Creation Flow (6 Steps)

```
┌─────────────────────────────────────────────────────────────┐
│  Step 1: FARMER INITIATES                                   │
│  ├─ Selects eligible lot                                    │
│  ├─ Sets reserve price                                      │
│  ├─ Chooses duration (3, 7, 14, or 21 days)                │
│  └─ Specifies quantity and destinations                     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  Step 2: ELIGIBILITY CHECK (Backend)                        │
│  ├─ GET /api/auctions/check-eligibility/:lotId             │
│  ├─ Validates all 7 preconditions                          │
│  ├─ Returns eligible=true/false + reasons                   │
│  └─ Blocks UI if not eligible                              │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  Step 3: BACKEND VALIDATION                                 │
│  ├─ POST /api/auctions                                      │
│  ├─ Validates farmer inputs                                 │
│  ├─ Verifies lot ownership                                  │
│  ├─ Checks quantity availability                            │
│  └─ Re-validates eligibility (double-check)                 │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  Step 4: BLOCKCHAIN DEPLOYMENT                              │
│  ├─ Calls PepperAuction.createAuction()                    │
│  ├─ Stores immutable auction terms                          │
│  ├─ Receives auction ID + transaction hash                  │
│  └─ Smart contract locks reserve price                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  Step 5: DATABASE STORAGE (Off-Chain)                       │
│  ├─ Stores volatile data (UI preferences, states)          │
│  ├─ Records preferred destinations                          │
│  ├─ Saves quantity and certificate counts                   │
│  └─ Links to blockchain transaction                         │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  Step 6: AUCTION SCHEDULED                                  │
│  ├─ Status: "scheduled" (if start > now)                   │
│  ├─ Status: "active" (if start ≤ now)                      │
│  ├─ Returns success response                                │
│  └─ Farmer receives confirmation                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔐 On-Chain vs Off-Chain Data

### **On-Chain (Immutable)**

Stored in PepperAuction smart contract:

- `auctionId` (uint256)
- `lotId` (string)
- `farmerAddress` (address)
- `reservePrice` (uint256)
- `startTime` (uint256 timestamp)
- `endTime` (uint256 timestamp)
- `status` (enum: Active, Ended, Settled)
- `blockchainTxHash` (string)

**Cannot be changed after deployment** ⚠️

### **Off-Chain (Volatile)**

Stored in PostgreSQL `auctions` table:

- `preferredDestinations` (array)
- `quantity` (float)
- `description` (text)
- `certificateCount` (int)
- `stageCount` (int)
- `currentBidCount` (int)
- `highestBidAmount` (float)
- `lastActivityAt` (timestamp)

**Can be updated during auction lifecycle** ✅

---

## 📱 Mobile UI Flow

### **1. Lot Selection**

- Farmer sees only **eligible lots** (approved/available)
- If no eligible lots:
  ```
  ╔══════════════════════════════════════╗
  ║  📦 No Eligible Lots                 ║
  ║                                      ║
  ║  You need approved lots to create    ║
  ║  an auction. Please create and get   ║
  ║  approval for your lots first.       ║
  ║                                      ║
  ║  [ Go Back ]  [ Create New Lot ]     ║
  ╚══════════════════════════════════════╝
  ```

### **2. Eligibility Check (Automatic)**

When lot selected:

```dart
Future<void> _checkAuctionEligibility() async {
  final response = await _apiService.get(
    '/auctions/check-eligibility/${_selectedLot!.lotId}',
  );

  if (!response['eligible']) {
    _showEligibilityDialog(); // Show reasons
  }
}
```

**Passing:**

```
╔══════════════════════════════════════╗
║  ✅ Lot Eligible for Auction         ║
║                                      ║
║  All preconditions met. You can      ║
║  proceed with auction creation.      ║
╚══════════════════════════════════════╝
```

**Failing:**

```
╔══════════════════════════════════════╗
║  ❌ Not Eligible for Auction         ║
║                                      ║
║  Requirements:                       ║
║  ❌ Minimum 3 certificates required  ║
║     (found 1)                        ║
║  ❌ Compliance status is "pending"   ║
║     (must be "passed")               ║
║                                      ║
║  [ Go Back ]  [ Retry Check ]        ║
╚══════════════════════════════════════╝
```

### **3. Form Inputs**

Only shown if **eligible**:

**Reserve Price:**

```
┌─────────────────────────────────────┐
│ Minimum Price (Reserve Price) *     │
│ ┌─────────────────────────────────┐ │
│ │ $ 8.50                     USD/kg│ │
│ └─────────────────────────────────┘ │
│ The minimum price you will accept   │
└─────────────────────────────────────┘
```

**Duration (Radio Buttons):**

```
○ 3 Days  - Quick sale
● 7 Days  - Standard auction ✓
○ 14 Days - Extended bidding
○ 21 Days - Maximum duration
```

**Destinations (Optional Chips):**

```
[🇪🇺 European Union] [🇺🇸 United States]
[🇦🇪 Middle East] [ 🇬🇧 United Kingdom]
```

### **4. Confirmation Dialog**

```
╔══════════════════════════════════════╗
║  Create Auction                      ║
║                                      ║
║  Summary:                            ║
║  Lot: LOT-1766820145306              ║
║  Reserve Price: $8.50                ║
║  Duration: 7 days                    ║
║  Quantity: 500 kg                    ║
║  Export to: EU, USA                  ║
║                                      ║
║  ⚠️ Once created, auction terms      ║
║     cannot be changed.               ║
║                                      ║
║  [ Cancel ]  [ Confirm ]             ║
╚══════════════════════════════════════╝
```

### **5. Success Notification**

```
╔══════════════════════════════════════╗
║  ✅ Auction Created Successfully! 🎉  ║
║                                      ║
║  Status: Scheduled                   ║
║  Auction ID: 1766820145              ║
║  Starts in: 1 hour                   ║
║  Ends in: 7 days                     ║
╚══════════════════════════════════════╝
```

---

## 🔧 Backend API Changes

### **New Endpoint: Check Eligibility**

```http
GET /api/auctions/check-eligibility/:lotId
```

**Response (Eligible):**

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

**Response (Not Eligible):**

```json
{
  "eligible": false,
  "reasons": [
    "Minimum 3 certificates required (found 1)",
    "Compliance status is 'pending' (must be 'passed')",
    "Minimum 2 processing stages required (found 0)"
  ],
  "lot": {
    "lotId": "LOT-1766820145306",
    "certificateCount": 1,
    "stageCount": 0,
    "hasBlockchainPassport": true
  }
}
```

### **Updated Endpoint: Create Auction**

```http
POST /api/auctions
Content-Type: application/json

{
  "lotId": "LOT-1766820145306",
  "farmerAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
  "reservePrice": 8.50,
  "quantity": 500,
  "duration": 7,
  "preferredDestinations": ["EU", "USA", "UAE"]
}
```

**Success Response:**

```json
{
  "success": true,
  "message": "Auction created successfully",
  "auction": {
    "auctionId": 1766820145,
    "lotId": "LOT-1766820145306",
    "farmerAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
    "reservePrice": 8.5,
    "quantity": 500,
    "startTime": "2025-01-28T10:00:00Z",
    "endTime": "2025-02-04T10:00:00Z",
    "status": "scheduled",
    "blockchainTxHash": "0x1a2b3c...",
    "preferredDestinations": ["EU", "USA", "UAE"],
    "onChainData": {
      "immutable": true,
      "auctionId": 1766820145,
      "reservePrice": 8.5,
      "txHash": "0x1a2b3c..."
    },
    "offChainData": {
      "volatile": true,
      "preferredDestinations": ["EU", "USA", "UAE"],
      "quantity": 500,
      "certificateCount": 6,
      "stageCount": 4
    }
  }
}
```

**Error Response (Not Eligible):**

```json
{
  "success": false,
  "error": "Lot is not eligible for auction",
  "reasons": [
    "Compliance status is 'pending' (must be 'passed' or 'approved')",
    "Minimum 3 certificates required (found 1)"
  ]
}
```

---

## 🛡️ Admin Governance (Future Enhancement)

### **Admin Cannot:**

- ❌ Create auctions for farmers
- ❌ Modify active auctions
- ❌ Change on-chain terms

### **Admin Can:**

- ✅ Define auction rules (duration limits, bid increments)
- ✅ Set global reserve price minimums
- ✅ Audit auction activities
- ✅ Suspend fraudulent accounts
- ✅ Generate compliance reports

### **Rule Templates (Planned)**

```sql
CREATE TABLE auction_rules (
  rule_id SERIAL PRIMARY KEY,
  rule_type VARCHAR(50) NOT NULL, -- e.g., 'min_duration', 'max_lots_per_farmer'
  rule_value JSONB NOT NULL,      -- e.g., {"days": 3}
  effective_from TIMESTAMP NOT NULL,
  created_by VARCHAR(100),
  created_at TIMESTAMP DEFAULT NOW()
);
```

Example rules:

- Minimum duration: 3 days
- Maximum duration: 21 days
- Bid increment: 2.5% of current bid
- Max concurrent auctions per farmer: 5

---

## 📊 Auction Status Lifecycle

```
┌─────────────┐
│  SCHEDULED  │  (startTime > now)
└──────┬──────┘
       │
       │ (Time reaches startTime)
       ▼
┌─────────────┐
│    ACTIVE   │  (Bidding allowed)
└──────┬──────┘
       │
       │ (Time reaches endTime)
       ▼
┌─────────────┐
│    ENDED    │  (Determine winner)
└──────┬──────┘
       │
       │ (Escrow released)
       ▼
┌─────────────┐
│   SETTLED   │  (Funds transferred)
└─────────────┘

Alternative paths:
- SCHEDULED → CANCELLED (Before activation)
- ACTIVE → CANCELLED (Admin intervention)
- ENDED → NO_BIDS (Reserve not met)
```

---

## 🧪 Testing Scenarios

### **Scenario 1: Happy Path**

1. Farmer has approved lot with 6 certificates
2. 4 processing stages recorded
3. Compliance: 7/7 EU checks passed
4. No active auction
5. ✅ Result: Auction created, status="scheduled"

### **Scenario 2: Missing Certificates**

1. Farmer selects lot with only 1 certificate
2. Eligibility check fails
3. UI shows: ❌ "Minimum 3 certificates required (found 1)"
4. Farmer cannot proceed
5. ✅ Result: Prevented premature auction

### **Scenario 3: Failed Compliance**

1. Lot has 5 certificates but compliance status="pending"
2. Eligibility check fails
3. UI shows: ❌ "Compliance status must be 'passed'"
4. ✅ Result: Enforces quality standards

### **Scenario 4: Active Auction Exists**

1. Farmer already has auction for LOT-123
2. Tries to create another auction
3. Backend rejects: "Lot already has an active auction"
4. ✅ Result: Prevents double-auctioning

---

## 🚀 Deployment Checklist

### **Backend**

- [x] Add `GET /auctions/check-eligibility/:lotId`
- [x] Update `POST /auctions` with governance flow
- [x] Validate all 7 preconditions
- [x] Separate on-chain vs off-chain storage
- [ ] Add admin rule enforcement (future)

### **Mobile App**

- [x] Replace `create_auction_screen.dart`
- [x] Add eligibility check on lot selection
- [x] Simplify farmer inputs (5 fields)
- [x] Add duration presets (3/7/14/21 days)
- [x] Show eligibility status cards
- [ ] Add "View Auction Rules" link (future)

### **Smart Contract**

- [ ] Verify `createAuction()` stores reserve price
- [ ] Ensure immutability of auction terms
- [ ] Add governance event logging

### **Database**

- [ ] Add `auction_rules` table (future)
- [ ] Index `auctions.status` for queries
- [ ] Add audit log table

---

## 📝 Migration Notes

### **From Old System:**

```dart
// OLD: Direct form submission
final auctionData = {
  'lotId': lotId,
  'startingPrice': _startingPriceController.text,
  'reservePrice': _reservePriceController.text,
  'startTime': _startTime.toIso8601String(),
  'endTime': _endTime.toIso8601String(),
};
await auctionProvider.createAuction(auctionData);
```

### **To New System:**

```dart
// NEW: Eligibility-gated with simplified inputs
// Step 1: Check eligibility
await _checkAuctionEligibility();

// Step 2: Show form only if eligible
if (_isEligible) {
  // Simplified inputs
  final auctionData = {
    'lotId': _selectedLot!.lotId,
    'farmerAddress': authProvider.user?.walletAddress,
    'reservePrice': double.parse(_reservePriceController.text),
    'quantity': double.parse(_quantityController.text),
    'duration': _durationDays, // Just days, not complex timestamps
    'preferredDestinations': _selectedDestinations,
  };

  // Backend handles all orchestration
  await _apiService.post('/auctions', auctionData);
}
```

---

## 🎓 Key Benefits

### **For Farmers:**

- ✅ Simplified 5-field form (was 8+ fields)
- ✅ Clear eligibility feedback before investing time
- ✅ No technical blockchain knowledge needed
- ✅ Confidence that rules are enforced fairly

### **For Buyers:**

- ✅ All auctioned lots are compliance-verified
- ✅ Transparent traceability (certificates + stages)
- ✅ Immutable auction terms (no bait-and-switch)
- ✅ Blockchain-secured ownership

### **For Platform:**

- ✅ Reduced fraudulent listings
- ✅ Enforceable governance rules
- ✅ Audit trail for disputes
- ✅ Scalable compliance system

---

## 📖 Related Documentation

- **Compliance System:** [COMPLIANCE_SUCCESS_SUMMARY.md](./COMPLIANCE_SUCCESS_SUMMARY.md)
- **Certification Guide:** [HOW_TO_ADD_CERTIFICATIONS.md](./HOW_TO_ADD_CERTIFICATIONS.md)
- **Processing Stages:** [HOW_TO_ADD_PROCESSING_STAGES_MOBILE.md](./HOW_TO_ADD_PROCESSING_STAGES_MOBILE.md)
- **API Documentation:** [API_DOCUMENTATION.yaml](./API_DOCUMENTATION.yaml)
- **Blockchain Contracts:** [blockchain/contracts/](./blockchain/contracts/)

---

## 🆘 Troubleshooting

### **Issue: Eligibility check fails with "No compliance checks performed"**

**Solution:**

1. Go to Lot Details screen
2. Tap "Run Compliance Check" button
3. Wait for 7/7 checks to pass
4. Retry auction creation

### **Issue: "Blockchain passport not minted"**

**Solution:**

1. Ensure lot was created via blockchain (not just database)
2. Check `pepper_lots.blockchain_tx_hash` is not null
3. Re-mint NFT if needed: `POST /api/lots/:lotId/mint`

### **Issue: "Lot already has an active auction"**

**Solution:**

1. Wait for current auction to end
2. OR cancel existing auction (if before start time)
3. Retry creation

---

## 📞 Support

For questions about the new auction system:

- **Backend API:** Check [backend/src/routes/auction.js](./backend/src/routes/auction.js)
- **Mobile UI:** Check [mobile/lib/screens/farmer/create_auction_screen.dart](./mobile/lib/screens/farmer/create_auction_screen.dart)
- **Smart Contract:** Check [blockchain/contracts/PepperAuction.sol](./blockchain/contracts/PepperAuction.sol)

---

_Last Updated: January 28, 2025_
_Version: 2.0 (Governance-Based)_
