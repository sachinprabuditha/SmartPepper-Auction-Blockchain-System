# 🎯 SmartPepper Research Implementation Roadmap (60% → 90%)

**Aligned with Research Objectives & Methodology**  
**Current Progress**: 60.3%  
**Target Progress**: 90%  
**Required Work**: 29.7% (≈30%)

---

## 📋 Research Alignment Analysis

### ✅ Already Implemented (60.3% Complete)

| Research Sub-Objective                  | Implementation Status | Evidence                                                              |
| --------------------------------------- | --------------------- | --------------------------------------------------------------------- |
| **1. Blockchain-Backed Traceability**   | ✅ COMPLETE           | Smart contracts deployed, IPFS integration, processing stages tracked |
| **2. Real-Time Auction Engine**         | ✅ COMPLETE           | WebSocket <150ms latency, live bidding, transparent price discovery   |
| **3. Compliance Rule Engine**           | ✅ COMPLETE           | 17 validators, EU/FDA/Middle East checks, automated validation        |
| **4. Digital Pepper Passport (QR/NFC)** | 🟡 PARTIAL            | Blockchain traceability exists, QR/NFC scanning not yet implemented   |
| **5. Smart Contract Enforcement**       | 🟡 PARTIAL            | Auction contracts complete, escrow settlement not yet implemented     |
| **6. Farmer-Centric Platform**          | 🟡 PARTIAL            | Web UI complete, mobile app not yet developed                         |

---

## 🎯 Remaining Implementation (60% → 90%)

Based on your research description, here are the **5 critical priorities** to complete:

---

## 🔒 **Priority 6: Security Hardening (10%)**

### Research Alignment

- **Supports**: Non-Functional Requirement - "Security, Reliability, Accessibility"
- **Methodology**: "Secure payment settlements, smart contract automation"
- **Test Cases**: "Security: Conduct penetration testing and smart contract audits"

### Implementation Details

#### 1. Authentication & Authorization (4%)

**JWT + Wallet Signature Verification (SIWE - Sign-In With Ethereum)**

```javascript
// backend/src/middleware/auth.js
- Implement nonce-based challenge-response authentication
- Farmers/exporters sign message with MetaMask
- Backend verifies signature and issues JWT token
- Token contains: wallet address, role (farmer/exporter/regulator), expiry
```

**Role-Based Access Control (RBAC)**

```javascript
// Roles defined in research:
- FARMER: Can list lots, upload certificates, participate in auctions
- EXPORTER: Can bid on auctions, view compliance, track shipments
- CUSTOMS_AUTHORITY: View-only access to traceability and compliance
- ADMIN: Full system access, audit logs, compliance monitoring
```

#### 2. Input Validation & SQL Injection Prevention (3%)

**Schema Validation with Joi**

```javascript
// backend/src/middleware/validation.js
- Validate all API inputs (lot registration, bid placement, certificate upload)
- Sanitize user inputs to prevent XSS attacks
- Parameterized SQL queries (already using PostgreSQL with pg library)
- File upload validation: type (PDF/image), size (<10MB), content verification
```

**Example Validation Schema**

```javascript
const lotRegistrationSchema = Joi.object({
  lotId: Joi.string()
    .pattern(/^LOT-\d{4}-[A-Z]{2}-\d{3}$/)
    .required(),
  farmerAddress: Joi.string()
    .pattern(/^0x[a-fA-F0-9]{40}$/)
    .required(),
  variety: Joi.string().valid("Tellicherry", "Malabar", "Panniyur").required(),
  quantity: Joi.number().positive().max(10000).required(),
  quality: Joi.string().valid("A", "AA", "AAA", "Premium").required(),
  harvestDate: Joi.date().max("now").required(),
  organicCertified: Joi.boolean(),
});
```

#### 3. Rate Limiting & DDoS Protection (2%)

**API Rate Limiting**

```javascript
// backend/src/middleware/rateLimiter.js
import rateLimit from 'express-rate-limit';
import RedisStore from 'rate-limit-redis';

// General API: 100 requests/minute
const generalLimiter = rateLimit({
  store: new RedisStore({ client: redisClient }),
  windowMs: 60 * 1000,
  max: 100
});

// Bid placement: 10 bids/minute (prevent auction manipulation)
const bidLimiter = rateLimit({
  windowMs: 60 * 1000,
  max: 10,
  keyGenerator: (req) => req.body.bidderAddress
});

// WebSocket connection: 5 concurrent per IP
const wsLimiter = /* socket.io rate limiting */
```

#### 4. Security Audit Documentation (1%)

**SECURITY_AUDIT.md**

- OWASP Top 10 compliance checklist
- Dependency vulnerability scanning (`npm audit` results)
- Smart contract security audit (Slither/Mythril analysis)
- Penetration testing guidelines
- Incident response plan

### Files to Create/Modify

```
backend/src/middleware/auth.js (NEW)
backend/src/middleware/rateLimiter.js (NEW)
backend/src/middleware/validation.js (NEW)
backend/src/utils/signature.js (NEW - SIWE implementation)
web/src/lib/auth.ts (NEW - Frontend auth)
SECURITY_AUDIT.md (NEW)
SECURITY_BEST_PRACTICES.md (NEW)
```

### Success Metrics

- ✅ All endpoints protected with JWT + wallet signature
- ✅ 100% of inputs validated with Joi schemas
- ✅ Rate limiting on all public endpoints
- ✅ Zero high/critical vulnerabilities in `npm audit`
- ✅ OWASP Top 10 compliance verified

---

## 💰 **Priority 7: Smart Contract Escrow System (8%)**

### Research Alignment

- **Sub-Objective 5**: "Smart contracts for automatic enforcement - payment releases post-delivery"
- **Methodology**: "Smart Contract Settlements - Payments held in blockchain-based escrow"
- **Feature**: "On-chain escrow via smart contracts" (Table 2 comparison)

### Implementation Details

#### 1. Smart Contract Development (4%)

**PepperEscrow.sol**

```solidity
// blockchain/contracts/PepperEscrow.sol
pragma solidity ^0.8.0;

contract PepperEscrow {
    struct Escrow {
        address buyer;
        address seller;
        uint256 amount;
        uint256 auctionId;
        EscrowStatus status;
        uint256 createdAt;
    }

    enum EscrowStatus { LOCKED, RELEASED, REFUNDED, DISPUTED }

    mapping(uint256 => Escrow) public escrows;

    // Lock funds when bid is placed
    function lockFunds(uint256 auctionId, address seller) external payable {
        require(msg.value > 0, "Must send ETH");
        escrows[auctionId] = Escrow({
            buyer: msg.sender,
            seller: seller,
            amount: msg.value,
            auctionId: auctionId,
            status: EscrowStatus.LOCKED,
            createdAt: block.timestamp
        });
        emit FundsLocked(auctionId, msg.sender, msg.value);
    }

    // Release funds to seller after successful delivery
    function releaseFunds(uint256 auctionId) external {
        Escrow storage escrow = escrows[auctionId];
        require(escrow.status == EscrowStatus.LOCKED, "Invalid status");
        require(msg.sender == escrow.buyer || isAdmin(msg.sender), "Unauthorized");

        escrow.status = EscrowStatus.RELEASED;
        payable(escrow.seller).transfer(escrow.amount);
        emit FundsReleased(auctionId, escrow.seller, escrow.amount);
    }

    // Refund to buyer if auction cancelled or compliance failed
    function refundBuyer(uint256 auctionId) external {
        Escrow storage escrow = escrows[auctionId];
        require(escrow.status == EscrowStatus.LOCKED, "Invalid status");

        escrow.status = EscrowStatus.REFUNDED;
        payable(escrow.buyer).transfer(escrow.amount);
        emit FundsRefunded(auctionId, escrow.buyer, escrow.amount);
    }

    // Handle disputes (requires admin intervention)
    function raiseDispute(uint256 auctionId) external {
        Escrow storage escrow = escrows[auctionId];
        require(msg.sender == escrow.buyer || msg.sender == escrow.seller);
        escrow.status = EscrowStatus.DISPUTED;
        emit DisputeRaised(auctionId, msg.sender);
    }
}
```

**Integration with PepperAuction.sol**

```solidity
// Modify existing PepperAuction.sol to work with escrow
function placeBid(uint256 auctionId) external payable {
    // ... existing validation ...

    // Lock funds in escrow contract
    escrowContract.lockFunds{value: msg.value}(auctionId, auction.seller);

    emit BidPlaced(auctionId, msg.sender, msg.value);
}

function endAuction(uint256 auctionId) external {
    // ... existing logic ...

    // Trigger automatic escrow release after compliance check
    if (complianceCheck(auctionId)) {
        escrowContract.releaseFunds(auctionId);
    } else {
        escrowContract.refundBuyer(auctionId);
    }
}
```

#### 2. Backend Escrow Management (2%)

**Escrow Service**

```javascript
// backend/src/services/escrowService.js
class EscrowService {
  async trackEscrowStatus(auctionId) {
    // Listen to blockchain events
    escrowContract.on("FundsLocked", async (auctionId, buyer, amount) => {
      await db.query(
        `INSERT INTO escrow_transactions 
         (auction_id, buyer_address, amount, status, created_at)
         VALUES ($1, $2, $3, 'locked', NOW())`,
        [auctionId, buyer, amount]
      );
    });
  }

  async triggerSettlement(auctionId) {
    // Called after shipment approval
    const auction = await getAuction(auctionId);
    if (auction.status === "ended" && auction.compliance_passed) {
      await escrowContract.releaseFunds(auctionId);
    }
  }

  async processRefund(auctionId, reason) {
    // Called if compliance fails or auction cancelled
    await escrowContract.refundBuyer(auctionId);
    await db.query(
      `UPDATE escrow_transactions 
       SET status = 'refunded', refund_reason = $1 
       WHERE auction_id = $2`,
      [reason, auctionId]
    );
  }
}
```

**Database Schema**

```sql
-- backend/db/migrations/add_escrow_tables.sql
CREATE TABLE escrow_transactions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  auction_id INTEGER REFERENCES auctions(auction_id),
  buyer_address VARCHAR(42) NOT NULL,
  seller_address VARCHAR(42) NOT NULL,
  amount DECIMAL(20, 8) NOT NULL,
  status VARCHAR(20) NOT NULL, -- locked, released, refunded, disputed
  blockchain_tx_hash VARCHAR(66),
  refund_reason TEXT,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_escrow_auction ON escrow_transactions(auction_id);
CREATE INDEX idx_escrow_status ON escrow_transactions(status);
```

#### 3. Frontend Escrow UI (2%)

**EscrowStatus.tsx Component**

```typescript
// web/src/components/auction/EscrowStatus.tsx
export function EscrowStatus({ auctionId, userAddress }: Props) {
  const [escrow, setEscrow] = useState<EscrowData | null>(null);

  useEffect(() => {
    // Fetch escrow status from blockchain
    const fetchEscrowStatus = async () => {
      const escrowData = await escrowContract.escrows(auctionId);
      setEscrow({
        amount: ethers.utils.formatEther(escrowData.amount),
        status: escrowData.status,
        buyer: escrowData.buyer,
        seller: escrowData.seller,
      });
    };

    // Listen to escrow events
    escrowContract.on("FundsReleased", (id, seller, amount) => {
      if (id === auctionId) {
        toast.success("Funds released to farmer!");
        setEscrow((prev) => ({ ...prev, status: "RELEASED" }));
      }
    });
  }, [auctionId]);

  return (
    <div className="escrow-status-card">
      <h3>Escrow Protection</h3>
      {escrow?.status === "LOCKED" && (
        <div className="bg-blue-50 p-4">
          <Shield className="text-blue-600" />
          <p>Your bid of {escrow.amount} ETH is safely held in escrow</p>
          <p className="text-sm">
            Funds will be released to farmer after successful delivery
          </p>
        </div>
      )}
      {escrow?.status === "RELEASED" && (
        <div className="bg-green-50 p-4">
          <CheckCircle className="text-green-600" />
          <p>Payment released to farmer</p>
          <p className="text-sm">Transaction complete</p>
        </div>
      )}
    </div>
  );
}
```

### Files to Create/Modify

```
blockchain/contracts/PepperEscrow.sol (NEW)
blockchain/scripts/deploy-escrow.js (NEW)
blockchain/test/escrow.test.js (NEW)
backend/src/services/escrowService.js (NEW)
backend/src/routes/escrow.js (NEW)
backend/db/migrations/add_escrow_tables.sql (NEW)
web/src/components/auction/EscrowStatus.tsx (NEW)
web/src/hooks/useEscrow.ts (NEW)
ESCROW_GUIDE.md (NEW)
```

### Success Metrics

- ✅ Funds locked within 3 seconds of bid placement
- ✅ Automatic release within 1 minute of shipment approval
- ✅ 100% refund success rate for losing bidders
- ✅ Zero fund loss incidents
- ✅ Dispute resolution mechanism tested

---

## 🏷️ **Priority 8: Digital Pepper Passport (QR/NFC Tags) (5%)**

### Research Alignment

- **Sub-Objective 4**: "Integrate digital pepper passports via QR/NFC tagging"
- **Feature**: "Assign unique QR/NFC tags to each lot containing farmer/harvest details"
- **Benefit**: "Instant verification by buyers, customs, or consumers"

### Implementation Details

#### 1. QR Code Generation & Blockchain Linking (2%)

**Backend QR Generation**

```javascript
// backend/src/services/passportService.js
import QRCode from "qrcode";

class DigitalPassportService {
  async generatePepperPassport(lotId) {
    const lot = await db.query("SELECT * FROM pepper_lots WHERE lot_id = $1", [
      lotId,
    ]);

    // Create unique passport data
    const passportData = {
      lotId: lot.lot_id,
      farmerId: lot.farmer_id,
      variety: lot.variety,
      quantity: lot.quantity,
      harvestDate: lot.harvest_date,
      blockchainTxHash: lot.blockchain_tx_hash,
      verificationUrl: `https://smartpepper.io/verify/${lotId}`,
    };

    // Generate QR code
    const qrCodeDataURL = await QRCode.toDataURL(JSON.stringify(passportData));

    // Store QR code in database
    await db.query(
      `UPDATE pepper_lots 
       SET qr_code_data = $1, digital_passport_generated = true 
       WHERE lot_id = $2`,
      [qrCodeDataURL, lotId]
    );

    return qrCodeDataURL;
  }

  async verifyPassport(lotId) {
    // Fetch data from blockchain and database
    const lot = await db.query("SELECT * FROM pepper_lots WHERE lot_id = $1", [
      lotId,
    ]);
    const processingStages = await db.query(
      "SELECT * FROM processing_stages WHERE lot_id = $1 ORDER BY timestamp",
      [lotId]
    );
    const certifications = await db.query(
      "SELECT * FROM certifications WHERE lot_id = $1",
      [lotId]
    );

    return {
      lot,
      processingStages,
      certifications,
      blockchainVerified: true,
    };
  }
}
```

#### 2. Public Verification Dashboard (2%)

**Verification Page**

```typescript
// web/src/app/verify/[lotId]/page.tsx
export default function VerifyPassportPage({
  params,
}: {
  params: { lotId: string };
}) {
  const [passportData, setPassportData] = useState<PassportData | null>(null);

  useEffect(() => {
    async function fetchPassportData() {
      const response = await fetch(`/api/verify/${params.lotId}`);
      const data = await response.json();
      setPassportData(data);
    }
    fetchPassportData();
  }, [params.lotId]);

  return (
    <div className="passport-verification-page">
      <h1>Digital Pepper Passport</h1>
      <div className="passport-header">
        <QRCodeDisplay lotId={params.lotId} />
        <BlockchainBadge verified={passportData?.blockchainVerified} />
      </div>

      <section className="farmer-details">
        <h2>Farmer Information</h2>
        <p>Farmer ID: {passportData?.lot.farmer_id}</p>
        <p>Location: {passportData?.lot.origin}</p>
        <p>Farm: {passportData?.lot.farm_location}</p>
      </section>

      <section className="harvest-details">
        <h2>Harvest Information</h2>
        <p>Variety: {passportData?.lot.variety}</p>
        <p>Quality Grade: {passportData?.lot.quality}</p>
        <p>Harvest Date: {passportData?.lot.harvest_date}</p>
        <p>Quantity: {passportData?.lot.quantity} kg</p>
        {passportData?.lot.organic_certified && (
          <Badge variant="success">Organic Certified ✓</Badge>
        )}
      </section>

      <section className="processing-timeline">
        <h2>Processing Journey</h2>
        <Timeline stages={passportData?.processingStages} />
      </section>

      <section className="certifications">
        <h2>Certifications</h2>
        {passportData?.certifications.map((cert) => (
          <CertificateCard
            key={cert.id}
            type={cert.cert_type}
            number={cert.cert_number}
            issuer={cert.issuer}
            ipfsUrl={cert.ipfs_url}
            verified={cert.is_valid}
          />
        ))}
      </section>

      <section className="blockchain-proof">
        <h2>Blockchain Verification</h2>
        <p>Transaction Hash: {passportData?.lot.blockchain_tx_hash}</p>
        <a
          href={`https://etherscan.io/tx/${passportData?.lot.blockchain_tx_hash}`}
        >
          View on Blockchain →
        </a>
      </section>
    </div>
  );
}
```

#### 3. NFC Tag Integration (1%)

**NFC Writing for Physical Tags**

```javascript
// Mobile app (Flutter) for writing NFC tags at packaging stage
// web/mobile-app/lib/services/nfc_service.dart
import 'package:nfc_manager/nfc_manager.dart';

class NFCService {
  Future<void> writePassportToNFC(String lotId) async {
    bool isAvailable = await NfcManager.instance.isAvailable();

    if (isAvailable) {
      NfcManager.instance.startSession(onDiscovered: (NfcTag tag) async {
        var ndef = Ndef.from(tag);

        if (ndef != null && ndef.isWritable) {
          NdefMessage message = NdefMessage([
            NdefRecord.createUri(Uri.parse('https://smartpepper.io/verify/$lotId')),
            NdefRecord.createText('SmartPepper Digital Passport'),
          ]);

          await ndef.write(message);
          NfcManager.instance.stopSession();
          print('NFC tag written successfully');
        }
      });
    }
  }

  Future<String?> readPassportFromNFC() async {
    String? lotId;

    NfcManager.instance.startSession(onDiscovered: (NfcTag tag) async {
      var ndef = Ndef.from(tag);

      if (ndef != null) {
        NdefMessage? message = await ndef.read();
        String? url = message.records.first.payload.toString();
        lotId = url?.split('/').last; // Extract lotId from URL
        NfcManager.instance.stopSession();
      }
    });

    return lotId;
  }
}
```

### Files to Create/Modify

```
backend/src/services/passportService.js (NEW)
backend/src/routes/passport.js (NEW)
web/src/app/verify/[lotId]/page.tsx (NEW)
web/src/components/passport/QRCodeDisplay.tsx (NEW)
web/src/components/passport/Timeline.tsx (NEW)
web/src/components/passport/CertificateCard.tsx (NEW)
mobile-app/lib/services/nfc_service.dart (NEW - Flutter)
DIGITAL_PASSPORT_GUIDE.md (NEW)
```

### Success Metrics

- ✅ QR codes generated for 100% of registered lots
- ✅ Public verification accessible without login
- ✅ NFC tags readable by standard smartphones
- ✅ Verification page load time <2 seconds
- ✅ Blockchain verification successful for all scans

---

## 📱 **Priority 9: Farmer-Centric Mobile App & Analytics (4%)**

### Research Alignment

- **Sub-Objective 6**: "Design a farmer-centric auction platform - participate via mobile devices"
- **Non-Functional Requirement**: "Usability - farmer-friendly mobile-first interface"
- **System Requirement**: "Flutter for mobile application for Farmer/Exporter"

### Implementation Details

#### 1. Flutter Mobile App Development (2%)

**Core Features**

```dart
// mobile-app/lib/main.dart
import 'package:flutter/material.dart';

void main() => runApp(SmartPepperApp());

class SmartPepperApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'SmartPepper',
      theme: ThemeData(primarySwatch: Colors.green),
      home: FarmerDashboard(),
      routes: {
        '/lot-registration': (context) => LotRegistrationScreen(),
        '/certificate-upload': (context) => CertificateUploadScreen(),
        '/auction-list': (context) => AuctionListScreen(),
        '/auction-details': (context) => AuctionDetailsScreen(),
        '/earnings': (context) => EarningsScreen(),
        '/nfc-scanner': (context) => NFCScannerScreen(),
      },
    );
  }
}
```

**Farmer Dashboard**

```dart
// mobile-app/lib/screens/farmer_dashboard.dart
class FarmerDashboard extends StatefulWidget {
  @override
  _FarmerDashboardState createState() => _FarmerDashboardState();
}

class _FarmerDashboardState extends State<FarmerDashboard> {
  int totalLots = 0;
  int activeAuctions = 0;
  double totalEarnings = 0.0;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('SmartPepper Farmer')),
      body: ListView(
        padding: EdgeInsets.all(16),
        children: [
          // Quick Stats Cards
          Row(
            children: [
              StatCard(title: 'Total Lots', value: '$totalLots'),
              StatCard(title: 'Active Auctions', value: '$activeAuctions'),
            ],
          ),
          StatCard(title: 'Total Earnings', value: '$totalEarnings ETH'),

          // Quick Actions
          ActionButton(
            label: 'Register New Lot',
            icon: Icons.add_circle,
            onPressed: () => Navigator.pushNamed(context, '/lot-registration'),
          ),
          ActionButton(
            label: 'Upload Certificates',
            icon: Icons.file_upload,
            onPressed: () => Navigator.pushNamed(context, '/certificate-upload'),
          ),
          ActionButton(
            label: 'View Active Auctions',
            icon: Icons.gavel,
            onPressed: () => Navigator.pushNamed(context, '/auction-list'),
          ),
          ActionButton(
            label: 'Scan NFC Tag',
            icon: Icons.nfc,
            onPressed: () => Navigator.pushNamed(context, '/nfc-scanner'),
          ),

          // Recent Activity
          RecentActivityList(),
        ],
      ),
    );
  }
}
```

**Lot Registration Screen**

```dart
// mobile-app/lib/screens/lot_registration_screen.dart
class LotRegistrationScreen extends StatefulWidget {
  @override
  _LotRegistrationScreenState createState() => _LotRegistrationScreenState();
}

class _LotRegistrationScreenState extends State<LotRegistrationScreen> {
  final _formKey = GlobalKey<FormState>();
  String variety = 'Tellicherry';
  double quantity = 0;
  String quality = 'AAA';
  DateTime harvestDate = DateTime.now();
  bool organicCertified = false;

  Future<void> submitLot() async {
    if (_formKey.currentState!.validate()) {
      final response = await http.post(
        Uri.parse('https://api.smartpepper.io/lots'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'variety': variety,
          'quantity': quantity,
          'quality': quality,
          'harvestDate': harvestDate.toIso8601String(),
          'organicCertified': organicCertified,
        }),
      );

      if (response.statusCode == 201) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lot registered successfully!')),
        );
        Navigator.pop(context);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Register Pepper Lot')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: EdgeInsets.all(16),
          children: [
            DropdownButtonFormField(
              value: variety,
              items: ['Tellicherry', 'Malabar', 'Panniyur']
                  .map((v) => DropdownMenuItem(value: v, child: Text(v)))
                  .toList(),
              onChanged: (value) => setState(() => variety = value as String),
              decoration: InputDecoration(labelText: 'Pepper Variety'),
            ),
            TextFormField(
              decoration: InputDecoration(labelText: 'Quantity (kg)'),
              keyboardType: TextInputType.number,
              validator: (value) => double.tryParse(value!) == null ? 'Enter valid number' : null,
              onChanged: (value) => quantity = double.parse(value),
            ),
            DropdownButtonFormField(
              value: quality,
              items: ['A', 'AA', 'AAA', 'Premium']
                  .map((q) => DropdownMenuItem(value: q, child: Text(q)))
                  .toList(),
              onChanged: (value) => setState(() => quality = value as String),
              decoration: InputDecoration(labelText: 'Quality Grade'),
            ),
            // ... Date picker, organic checkbox, etc.
            ElevatedButton(
              onPressed: submitLot,
              child: Text('Register Lot'),
            ),
          ],
        ),
      ),
    );
  }
}
```

#### 2. Analytics Dashboard (1%)

**Price Trends & Demand Forecasting**

```typescript
// web/src/components/dashboard/AnalyticsChart.tsx
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
} from "recharts";

export function PriceTrendsChart() {
  const [priceData, setPriceData] = useState<PriceData[]>([]);

  useEffect(() => {
    async function fetchPriceTrends() {
      const response = await fetch("/api/analytics/price-trends");
      const data = await response.json();
      setPriceData(data);
    }
    fetchPriceTrends();
  }, []);

  return (
    <div className="analytics-chart">
      <h3>Price Trends - Last 30 Days</h3>
      <LineChart width={600} height={300} data={priceData}>
        <XAxis dataKey="date" />
        <YAxis />
        <Tooltip />
        <Legend />
        <Line
          type="monotone"
          dataKey="avgPrice"
          stroke="#8884d8"
          name="Average Price (ETH)"
        />
        <Line
          type="monotone"
          dataKey="maxPrice"
          stroke="#82ca9d"
          name="Highest Bid (ETH)"
        />
      </LineChart>
    </div>
  );
}

export function ComplianceSuccessRate() {
  const [complianceData, setComplianceData] = useState<ComplianceData[]>([]);

  return (
    <div className="compliance-chart">
      <h3>Compliance Success Rate by Market</h3>
      <BarChart width={600} height={300} data={complianceData}>
        <XAxis dataKey="market" />
        <YAxis />
        <Tooltip />
        <Bar dataKey="successRate" fill="#82ca9d" name="Success Rate %" />
      </BarChart>
    </div>
  );
}
```

**Farmer Earnings Dashboard**

```typescript
// web/src/app/dashboard/farmer/page.tsx
export default function FarmerDashboard() {
  return (
    <div className="farmer-dashboard">
      <div className="stats-grid">
        <StatCard
          title="Total Lots Registered"
          value={stats.totalLots}
          icon={<Package />}
        />
        <StatCard
          title="Active Auctions"
          value={stats.activeAuctions}
          icon={<Gavel />}
        />
        <StatCard
          title="Total Revenue"
          value={`${stats.revenue} ETH`}
          icon={<DollarSign />}
        />
        <StatCard
          title="Compliance Success"
          value={`${stats.complianceRate}%`}
          icon={<CheckCircle />}
        />
      </div>

      <div className="charts-section">
        <PriceTrendsChart />
        <RevenueByMonthChart />
        <ComplianceSuccessRate />
      </div>

      <div className="recent-activity">
        <h3>Recent Auctions</h3>
        <AuctionHistoryTable farmerId={currentUser.id} />
      </div>
    </div>
  );
}
```

#### 3. Multilingual Support (0.5%)

**i18n Implementation**

```typescript
// web/src/i18n/translations.ts
export const translations = {
  en: {
    "lot.register": "Register Lot",
    "auction.placeBid": "Place Bid",
    "dashboard.earnings": "Total Earnings",
  },
  si: {
    // Sinhala
    "lot.register": "කැබලිය ලියාපදිංචි කරන්න",
    "auction.placeBid": "ලංසුව තබන්න",
    "dashboard.earnings": "සම්පූර්ණ ඉපැයීම්",
  },
  ta: {
    // Tamil
    "lot.register": "லாட் பதிவு செய்யவும்",
    "auction.placeBid": "விலைப்பட்டியல்",
    "dashboard.earnings": "மொத்த வருவாய்",
  },
  hi: {
    // Hindi
    "lot.register": "लॉट पंजीकृत करें",
    "auction.placeBid": "बोली लगाएं",
    "dashboard.earnings": "कुल कमाई",
  },
};
```

#### 4. Offline Support (0.5%)

**Service Worker for PWA**

```javascript
// web/public/service-worker.js
self.addEventListener("install", (event) => {
  event.waitUntil(
    caches.open("smartpepper-v1").then((cache) => {
      return cache.addAll(["/", "/dashboard", "/auctions", "/offline.html"]);
    })
  );
});

self.addEventListener("fetch", (event) => {
  event.respondWith(
    caches.match(event.request).then((response) => {
      return response || fetch(event.request);
    })
  );
});
```

### Files to Create/Modify

```
mobile-app/ (NEW - Flutter project)
  ├── lib/
  │   ├── main.dart
  │   ├── screens/
  │   │   ├── farmer_dashboard.dart
  │   │   ├── lot_registration_screen.dart
  │   │   ├── auction_list_screen.dart
  │   │   └── earnings_screen.dart
  │   ├── services/
  │   │   ├── api_service.dart
  │   │   └── nfc_service.dart
  │   └── widgets/
  │       ├── stat_card.dart
  │       └── action_button.dart
web/src/components/dashboard/AnalyticsChart.tsx (NEW)
web/src/components/dashboard/PriceTrendsChart.tsx (NEW)
web/src/app/dashboard/farmer/page.tsx (NEW)
web/src/app/dashboard/exporter/page.tsx (NEW)
web/src/i18n/translations.ts (NEW)
web/public/service-worker.js (NEW)
web/public/manifest.json (UPDATE for PWA)
MOBILE_APP_GUIDE.md (NEW)
```

### Success Metrics

- ✅ Mobile app functional on Android/iOS
- ✅ Farmer can register lot in <2 minutes
- ✅ Analytics dashboard load time <2s
- ✅ 4 languages supported (EN, SI, TA, HI)
- ✅ Offline lot listing with auto-sync
- ✅ Lighthouse PWA score >90

---

## 🚀 **Priority 10: Production Infrastructure & DevOps (3%)**

### Research Alignment

- **System Requirements**: "Minimum server configuration: 8-core CPU, 32GB RAM, SSD storage"
- **Feasibility Study**: "Operational Feasibility - cloud infrastructure"
- **Methodology**: "Deploy on scalable cloud infrastructure"

### Implementation Details

#### 1. Docker Containerization (1%)

**Backend Dockerfile**

```dockerfile
# Dockerfile.backend
FROM node:18-alpine AS builder
WORKDIR /app
COPY backend/package*.json ./
RUN npm ci --only=production
COPY backend/ ./
RUN npm run build

FROM node:18-alpine
WORKDIR /app
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/package.json ./
EXPOSE 3002
CMD ["node", "dist/server.js"]
```

**Frontend Dockerfile**

```dockerfile
# Dockerfile.web
FROM node:18-alpine AS builder
WORKDIR /app
COPY web/package*.json ./
RUN npm ci
COPY web/ ./
RUN npm run build

FROM node:18-alpine
WORKDIR /app
RUN npm install -g serve
COPY --from=builder /app/out ./out
EXPOSE 3000
CMD ["serve", "-s", "out", "-l", "3000"]
```

**Blockchain Node Dockerfile**

```dockerfile
# Dockerfile.blockchain
FROM node:18-alpine
WORKDIR /app
COPY blockchain/package*.json ./
RUN npm ci
COPY blockchain/ ./
EXPOSE 8545
CMD ["npx", "hardhat", "node"]
```

**Docker Compose**

```yaml
# docker-compose.yml
version: "3.8"

services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: smartpepper
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s

  blockchain:
    build:
      context: .
      dockerfile: Dockerfile.blockchain
    ports:
      - "8545:8545"
    environment:
      - NODE_ENV=development

  backend:
    build:
      context: .
      dockerfile: Dockerfile.backend
    ports:
      - "3002:3002"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      blockchain:
        condition: service_started
    environment:
      - DATABASE_URL=postgresql://${DB_USER}:${DB_PASSWORD}@postgres:5432/smartpepper
      - REDIS_URL=redis://redis:6379
      - BLOCKCHAIN_RPC=http://blockchain:8545
      - JWT_SECRET=${JWT_SECRET}
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:3002/health"]
      interval: 30s

  web:
    build:
      context: .
      dockerfile: Dockerfile.web
    ports:
      - "3000:3000"
    depends_on:
      - backend
    environment:
      - NEXT_PUBLIC_API_URL=http://backend:3002

  ipfs:
    image: ipfs/kubo:latest
    ports:
      - "5001:5001" # API
      - "8080:8080" # Gateway
    volumes:
      - ipfs_data:/data/ipfs

volumes:
  postgres_data:
  ipfs_data:
```

#### 2. CI/CD Pipeline (1%)

**GitHub Actions - CI**

```yaml
# .github/workflows/ci.yml
name: CI Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: "18"
      - name: Install dependencies
        run: cd backend && npm ci
      - name: Run linter
        run: cd backend && npm run lint
      - name: Run tests
        run: cd backend && npm test
      - name: Check security vulnerabilities
        run: cd backend && npm audit --audit-level=high

  test-web:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: "18"
      - name: Install dependencies
        run: cd web && npm ci
      - name: Run TypeScript check
        run: cd web && npm run type-check
      - name: Run tests
        run: cd web && npm test
      - name: Build
        run: cd web && npm run build

  test-contracts:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: "18"
      - name: Install dependencies
        run: cd blockchain && npm ci
      - name: Compile contracts
        run: cd blockchain && npx hardhat compile
      - name: Run contract tests
        run: cd blockchain && npx hardhat test
      - name: Security audit
        run: cd blockchain && npx slither .

  docker-build:
    runs-on: ubuntu-latest
    needs: [test-backend, test-web, test-contracts]
    steps:
      - uses: actions/checkout@v3
      - name: Build Docker images
        run: docker-compose build
      - name: Run containers
        run: docker-compose up -d
      - name: Health check
        run: |
          sleep 30
          curl -f http://localhost:3002/health || exit 1
```

**GitHub Actions - Deployment**

```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  workflow_dispatch:
  push:
    tags:
      - "v*"

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/checkout@v3

      - name: Build and push Docker images
        run: |
          docker build -t smartpepper/backend:${{ github.sha }} -f Dockerfile.backend .
          docker build -t smartpepper/web:${{ github.sha }} -f Dockerfile.web .
          docker push smartpepper/backend:${{ github.sha }}
          docker push smartpepper/web:${{ github.sha }}

      - name: Deploy to server
        uses: appleboy/ssh-action@master
        with:
          host: ${{ secrets.PRODUCTION_HOST }}
          username: ${{ secrets.PRODUCTION_USER }}
          key: ${{ secrets.SSH_PRIVATE_KEY }}
          script: |
            cd /opt/smartpepper
            docker-compose pull
            docker-compose up -d
            docker-compose exec backend npm run migrate
```

#### 3. Monitoring & Logging (0.5%)

**Prometheus Configuration**

```yaml
# monitoring/prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: "smartpepper-backend"
    static_configs:
      - targets: ["backend:3002"]
    metrics_path: "/metrics"

  - job_name: "postgres"
    static_configs:
      - targets: ["postgres:5432"]

  - job_name: "redis"
    static_configs:
      - targets: ["redis:6379"]

  - job_name: "blockchain"
    static_configs:
      - targets: ["blockchain:8545"]
```

**Grafana Dashboard**

```json
// monitoring/grafana-dashboard.json
{
  "dashboard": {
    "title": "SmartPepper System Monitoring",
    "panels": [
      {
        "title": "API Response Time",
        "targets": [
          {
            "expr": "http_request_duration_seconds"
          }
        ]
      },
      {
        "title": "Active Auctions",
        "targets": [
          {
            "expr": "auction_count{status='active'}"
          }
        ]
      },
      {
        "title": "WebSocket Latency",
        "targets": [
          {
            "expr": "websocket_latency_milliseconds"
          }
        ]
      },
      {
        "title": "Compliance Check Success Rate",
        "targets": [
          {
            "expr": "compliance_checks_total{result='passed'} / compliance_checks_total"
          }
        ]
      }
    ]
  }
}
```

**Backend Metrics**

```javascript
// backend/src/config/monitoring.js
import promClient from "prom-client";

const register = new promClient.Registry();

// Default metrics (CPU, memory, etc.)
promClient.collectDefaultMetrics({ register });

// Custom metrics
export const httpRequestDuration = new promClient.Histogram({
  name: "http_request_duration_seconds",
  help: "Duration of HTTP requests in seconds",
  labelNames: ["method", "route", "status_code"],
  registers: [register],
});

export const auctionCount = new promClient.Gauge({
  name: "auction_count",
  help: "Number of auctions by status",
  labelNames: ["status"],
  registers: [register],
});

export const websocketLatency = new promClient.Histogram({
  name: "websocket_latency_milliseconds",
  help: "WebSocket message latency",
  buckets: [10, 50, 100, 200, 300, 500, 1000],
  registers: [register],
});

export const complianceChecks = new promClient.Counter({
  name: "compliance_checks_total",
  help: "Total compliance checks performed",
  labelNames: ["destination", "result"],
  registers: [register],
});

// Metrics endpoint
app.get("/metrics", async (req, res) => {
  res.set("Content-Type", register.contentType);
  res.end(await register.metrics());
});
```

#### 4. Production Documentation (0.5%)

**Deployment Guide**

````markdown
# DEPLOYMENT_GUIDE.md

## Infrastructure Requirements

### Minimum Server Configuration

- CPU: 8 cores (as per research requirements)
- RAM: 32GB (as per research requirements)
- Storage: 500GB SSD
- Network: 1Gbps connection

### Recommended Cloud Providers

- AWS: EC2 c5.2xlarge instance
- Azure: Standard_F8s_v2
- Google Cloud: n2-standard-8

## Deployment Steps

### 1. Server Setup

```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```
````

### 2. Clone Repository

```bash
git clone https://github.com/your-org/smartpepper.git
cd smartpepper
```

### 3. Environment Configuration

```bash
cp .env.example .env
# Edit .env with production values
```

### 4. Database Migration

```bash
docker-compose up -d postgres
docker-compose exec backend npm run migrate
```

### 5. Deploy Contracts

```bash
cd blockchain
npx hardhat run scripts/deploy.js --network mainnet
```

### 6. Start Services

```bash
docker-compose up -d
```

### 7. Verify Deployment

```bash
curl http://localhost:3002/health
curl http://localhost:3000
```

## Scaling Guidelines

### Horizontal Scaling

- Backend: Multiple instances behind load balancer
- Database: PostgreSQL read replicas
- Redis: Redis Cluster for distributed caching

### Vertical Scaling

- Increase instance size based on load
- Monitor CPU/RAM usage via Grafana

```

### Files to Create/Modify
```

Dockerfile.backend (NEW)
Dockerfile.web (NEW)
Dockerfile.blockchain (NEW)
docker-compose.yml (NEW)
docker-compose.prod.yml (NEW)
.github/workflows/ci.yml (NEW)
.github/workflows/deploy.yml (NEW)
monitoring/prometheus.yml (NEW)
monitoring/grafana-dashboard.json (NEW)
backend/src/config/monitoring.js (NEW)
DEPLOYMENT_GUIDE.md (NEW)
INFRASTRUCTURE.md (NEW)
SCALING_GUIDE.md (NEW)

```

### Success Metrics
- ✅ Docker containers build successfully (<5 min)
- ✅ CI/CD pipeline runs on every commit
- ✅ Automated tests pass (>90% coverage target)
- ✅ Monitoring dashboards operational
- ✅ Production deployment documented
- ✅ Health checks passing (99.9% uptime target)

---

## 📊 Progress Tracking (60% → 90%)

```

Current: 60.3% ████████████░░░░░░░░
Priority 6: 70.3% ██████████████░░░░░░ (Security)
Priority 7: 78.3% ███████████████░░░░░ (Escrow)
Priority 8: 83.3% ████████████████░░░░ (Digital Passport)
Priority 9: 87.3% █████████████████░░░ (Mobile App)
Priority 10: 90.3% ██████████████████░░ (DevOps)
TARGET: 90% ██████████████████░░ ✅

```

---

## 🎯 Implementation Strategy

### Week 1: Security & Escrow (18%)
**Days 1-2**: Priority 6 (Security Hardening)
- Implement JWT + SIWE authentication
- Add input validation middleware
- Set up rate limiting
- Run security audit

**Days 3-5**: Priority 7 (Escrow System)
- Develop PepperEscrow.sol contract
- Integrate with auction system
- Build escrow UI components
- Test escrow flows

### Week 2: Digital Passport & Mobile App (9%)
**Days 6-7**: Priority 8 (Digital Passport)
- QR code generation system
- Public verification dashboard
- NFC tag integration

**Days 8-10**: Priority 9 (Mobile App & Analytics)
- Flutter mobile app development
- Analytics dashboards
- Multilingual support
- PWA implementation

### Week 3: Infrastructure & Testing (3%)
**Days 11-12**: Priority 10 (DevOps)
- Docker containerization
- CI/CD pipeline setup
- Monitoring infrastructure
- Production deployment

**Day 13**: Integration Testing & Documentation
- End-to-end testing
- Update all documentation
- Prepare 90% milestone report

---

## 🏆 Expected Outcomes at 90%

### Research Sub-Objectives Completion

| Sub-Objective | Status | Evidence |
|---------------|--------|----------|
| **1. Blockchain Traceability** | ✅ COMPLETE | Full farm-to-export tracking with QR/NFC |
| **2. Real-Time Auction** | ✅ COMPLETE | <150ms latency, mobile app access |
| **3. Compliance Rule Engine** | ✅ COMPLETE | 17 validators, automated checks |
| **4. Digital Pepper Passport** | ✅ COMPLETE | QR/NFC tags with public verification |
| **5. Smart Contract Enforcement** | ✅ COMPLETE | Escrow, automatic settlements |
| **6. Farmer-Centric Platform** | ✅ COMPLETE | Flutter mobile app, multilingual |

### System Capabilities
- ✅ Production-grade security (JWT, RBAC, rate limiting)
- ✅ Smart contract escrow with automatic settlements
- ✅ QR/NFC digital passports for all lots
- ✅ Mobile app for farmers (Android/iOS)
- ✅ Analytics dashboards (price trends, compliance)
- ✅ Docker containerization
- ✅ CI/CD pipeline operational
- ✅ Monitoring infrastructure (Prometheus/Grafana)

### Research Validation
- ✅ All functional requirements met
- ✅ Non-functional requirements validated
- ✅ Performance targets exceeded
- ✅ Security audit completed
- ✅ Pilot-ready system

---

## 📝 Documentation Deliverables

1. ✅ SECURITY_AUDIT.md - OWASP compliance
2. ✅ ESCROW_GUIDE.md - Smart contract escrow
3. ✅ DIGITAL_PASSPORT_GUIDE.md - QR/NFC implementation
4. ✅ MOBILE_APP_GUIDE.md - Flutter app documentation
5. ✅ DEPLOYMENT_GUIDE.md - Production deployment
6. ✅ INFRASTRUCTURE.md - Server requirements
7. ✅ 90_PERCENT_MILESTONE_REPORT.md - Research validation

---

## 🚀 Ready to Begin?

**Recommended Start**: Priority 6 (Security Hardening)

This aligns perfectly with your research requirements for:
- "Secure payment settlements"
- "Farmer empowerment through trusted platform"
- "Production-ready system for pilot testing"

Shall I begin implementing the JWT authentication and security middleware?
```
