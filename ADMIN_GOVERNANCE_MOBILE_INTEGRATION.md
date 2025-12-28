# Admin Governance Integration with Mobile Auction Creation

## Overview

This document explains how the admin governance features (rule templates, settings, approvals) affect the auction creation process for farmers in the mobile app.

---

## 🎯 How Admin Features Affect Farmers

### 1. **Rule Templates Selection**

**Admin Side:**

- Admins create templates like "Standard Auction", "Premium Lot Auction"
- Each template defines: min/max duration, bid increment, reserve price limits, approval requirement

**Mobile App Impact:**

```dart
// Farmers should see available templates during auction creation
Future<List<AuctionRuleTemplate>> fetchAuctionTemplates() async {
  final response = await apiService.get('/admin/auction-templates');
  return (response['templates'] as List)
      .map((t) => AuctionRuleTemplate.fromJson(t))
      .toList();
}

// UI shows template selector
DropdownButton<String>(
  items: templates.map((template) => DropdownMenuItem(
    value: template.id,
    child: Column(
      children: [
        Text(template.name),
        Text(template.description, style: TextStyle(fontSize: 12)),
      ],
    ),
  )).toList(),
  onChanged: (templateId) {
    // Apply template constraints
    final template = templates.firstWhere((t) => t.id == templateId);
    setState(() {
      _minDuration = template.minDuration;
      _maxDuration = template.maxDuration;
      _bidIncrement = template.minBidIncrement;
      _requiresApproval = template.requiresApproval;
    });
  },
)
```

---

### 2. **Allowed Durations (Admin-Defined)**

**Admin Side:**

- Admin sets: `allowedDurations: [24h, 48h, 72h, 96h, 168h]`

**Mobile App Impact:**

```dart
// create_auction_screen.dart - UPDATED _durationOptions

// OLD - Hardcoded durations
final List<Map<String, dynamic>> _durationOptions = [
  {'days': 3, 'label': '3 Days'},
  {'days': 7, 'label': '7 Days'},
  {'days': 14, 'label': '14 Days'},
];

// NEW - Fetch from backend governance settings
List<Map<String, dynamic>> _durationOptions = [];

@override
void initState() {
  super.initState();
  _fetchGovernanceSettings();
}

Future<void> _fetchGovernanceSettings() async {
  try {
    final settings = await apiService.get('/admin/governance-settings');

    // Convert hours to days and populate duration options
    setState(() {
      _durationOptions = (settings['allowedDurations'] as List<int>)
          .map((hours) => {
                'hours': hours,
                'days': hours ~/ 24,
                'label': _formatDuration(hours),
                'subtitle': _getDurationSubtitle(hours),
              })
          .toList();

      // Set min/max constraints
      _minReservePrice = settings['minReservePrice'];
      _maxReservePrice = settings['maxReservePrice'];
      _defaultBidIncrement = settings['defaultBidIncrement'];
      _requiresAdminApproval = settings['requiresAdminApproval'];
    });
  } catch (e) {
    print('Failed to load governance settings: $e');
  }
}

String _formatDuration(int hours) {
  if (hours < 24) return '$hours Hours';
  final days = hours ~/ 24;
  return '$days Day${days > 1 ? 's' : ''}';
}
```

**UI Update:**

```dart
// Duration selection respects admin-defined options
GridView.builder(
  itemCount: _durationOptions.length,
  itemBuilder: (context, index) {
    final option = _durationOptions[index];
    return Card(
      color: _durationDays == option['days']
          ? AppTheme.forestGreen
          : Colors.white,
      child: InkWell(
        onTap: () {
          setState(() {
            _durationDays = option['days'];
          });
        },
        child: Column(
          children: [
            Text(option['label']),
            Text(option['subtitle']),
          ],
        ),
      ),
    );
  },
)
```

---

### 3. **Bid Increment Enforcement**

**Admin Side:**

- Admin sets: `defaultBidIncrement: 5%` (minimum)

**Mobile App Impact:**

```dart
// Display bid increment info (read-only, not farmer-configurable)
Widget _buildBidIncrementInfo() {
  return Container(
    padding: EdgeInsets.all(16),
    decoration: BoxDecoration(
      color: Colors.blue.shade50,
      borderRadius: BorderRadius.circular(8),
    ),
    child: Row(
      children: [
        Icon(Icons.trending_up, color: Colors.blue),
        SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Minimum Bid Increment',
                style: TextStyle(fontWeight: FontWeight.bold),
              ),
              Text(
                '$_defaultBidIncrement% of current bid',
                style: TextStyle(color: Colors.grey[600]),
              ),
            ],
          ),
        ),
      ],
    ),
  );
}
```

---

### 4. **Reserve Price Limits**

**Admin Side:**

- Admin sets: `minReservePrice: 100 LKR`, `maxReservePrice: 1,000,000 LKR`

**Mobile App Impact:**

```dart
// Update reserve price validation
TextFormField(
  controller: _reservePriceController,
  keyboardType: TextInputType.number,
  decoration: InputDecoration(
    labelText: 'Reserve Price (LKR)',
    hintText: 'Min: $_minReservePrice, Max: $_maxReservePrice',
  ),
  validator: (value) {
    if (value == null || value.isEmpty) {
      return 'Reserve price is required';
    }

    final price = double.tryParse(value);
    if (price == null) {
      return 'Invalid price';
    }

    // Admin-defined constraints
    if (price < _minReservePrice) {
      return 'Price must be at least LKR $_minReservePrice';
    }

    if (price > _maxReservePrice) {
      return 'Price cannot exceed LKR $_maxReservePrice';
    }

    return null;
  },
)
```

---

### 5. **Admin Approval Requirement**

**Admin Side:**

- Admin toggles: `requiresAdminApproval: true/false`
- Or template-specific: `template.requiresApproval: true`

**Mobile App Impact:**

#### A. Update Auction Status Flow

```dart
// Auction creation now has 3 possible initial statuses:
enum AuctionStatus {
  pending_approval,  // NEW - Waiting for admin approval
  created,           // Approved, not started
  active,            // Live auction
  ended,             // Finished
}

Future<void> _createAuction() async {
  try {
    setState(() => _loading = true);

    final auctionData = {
      'lotId': _selectedLot!.lotId,
      'startPrice': double.parse(_reservePriceController.text),
      'reservePrice': double.parse(_reservePriceController.text),
      'durationDays': _durationDays,
      'templateId': _selectedTemplateId, // NEW
      'status': _requiresAdminApproval ? 'pending_approval' : 'created',
    };

    final response = await apiService.createAuction(auctionData);

    if (_requiresAdminApproval) {
      _showApprovalPendingDialog();
    } else {
      _showSuccessDialog();
    }
  } catch (e) {
    _showErrorDialog(e.toString());
  } finally {
    setState(() => _loading = false);
  }
}

void _showApprovalPendingDialog() {
  showDialog(
    context: context,
    barrierDismissible: false,
    builder: (context) => AlertDialog(
      title: Row(
        children: [
          Icon(Icons.schedule, color: Colors.orange),
          SizedBox(width: 8),
          Text('Pending Admin Approval'),
        ],
      ),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            'Your auction has been submitted and is pending admin approval.',
            textAlign: TextAlign.center,
          ),
          SizedBox(height: 16),
          Container(
            padding: EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.orange.shade50,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Column(
              children: [
                Icon(Icons.info_outline, color: Colors.orange),
                SizedBox(height: 8),
                Text(
                  'This auction requires admin review due to:',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                SizedBox(height: 8),
                Text('• Premium lot value'),
                Text('• Extended duration'),
                Text('• System governance rules'),
              ],
            ),
          ),
          SizedBox(height: 16),
          Text(
            'You will be notified when the admin approves or rejects your auction.',
            style: TextStyle(fontSize: 12, color: Colors.grey[600]),
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: () {
            Navigator.pop(context); // Close dialog
            Navigator.pop(context); // Return to previous screen
          },
          child: Text('OK'),
        ),
      ],
    ),
  );
}
```

#### B. Add Approval Status Screen

```dart
// New screen: pending_approvals_screen.dart
class PendingApprovalsScreen extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Pending Approvals'),
        backgroundColor: AppTheme.forestGreen,
      ),
      body: FutureBuilder<List<Auction>>(
        future: apiService.getAuctions(status: 'pending_approval'),
        builder: (context, snapshot) {
          if (!snapshot.hasData) return CircularProgressIndicator();

          final pendingAuctions = snapshot.data!;

          return ListView.builder(
            itemCount: pendingAuctions.length,
            itemBuilder: (context, index) {
              final auction = pendingAuctions[index];
              return Card(
                child: ListTile(
                  leading: Icon(Icons.schedule, color: Colors.orange),
                  title: Text('Lot: ${auction.lotId}'),
                  subtitle: Text('Submitted: ${auction.createdAt}'),
                  trailing: Chip(
                    label: Text('Pending'),
                    backgroundColor: Colors.orange.shade100,
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
```

#### C. Update Auctions Screen to Show Pending Tab

```dart
// auctions_screen.dart - Add new tab
TabBar(
  tabs: [
    Tab(text: 'Active'),
    Tab(text: 'Upcoming'),
    Tab(text: 'Pending Approval'), // NEW
    Tab(text: 'Completed'),
  ],
)
```

---

### 6. **Emergency Cancellation Requests**

**Mobile App - New Feature:**

#### A. Add Cancellation Request Button in Farmer Auction Monitor

```dart
// auction_monitor_screen.dart
FloatingActionButton(
  onPressed: _requestEmergencyCancellation,
  backgroundColor: Colors.red,
  child: Icon(Icons.cancel),
  label: Text('Request Cancellation'),
)

Future<void> _requestEmergencyCancellation() async {
  // Show reason dialog
  final reason = await showDialog<String>(
    context: context,
    builder: (context) => _EmergencyCancellationDialog(),
  );

  if (reason == null) return;

  try {
    await apiService.post('/auctions/request-cancellation', {
      'auctionId': widget.auctionId,
      'reason': reason,
    });

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Cancellation request submitted. Admin will review.'),
        backgroundColor: Colors.orange,
      ),
    );
  } catch (e) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Failed to submit request: $e'),
        backgroundColor: Colors.red,
      ),
    );
  }
}
```

#### B. Cancellation Request Dialog

```dart
class _EmergencyCancellationDialog extends StatefulWidget {
  @override
  State<_EmergencyCancellationDialog> createState() =>
      __EmergencyCancellationDialogState();
}

class __EmergencyCancellationDialogState
    extends State<_EmergencyCancellationDialog> {
  final _reasonController = TextEditingController();
  String _selectedReason = '';

  final List<String> _predefinedReasons = [
    'Quality issue discovered',
    'Lot damaged or compromised',
    'Error in lot details',
    'Force majeure (natural disaster, etc.)',
    'Other',
  ];

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text('Request Emergency Cancellation'),
      content: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              '⚠️ Emergency cancellations require admin approval',
              style: TextStyle(color: Colors.red, fontSize: 12),
            ),
            SizedBox(height: 16),
            DropdownButtonFormField<String>(
              decoration: InputDecoration(
                labelText: 'Reason',
                border: OutlineInputBorder(),
              ),
              items: _predefinedReasons
                  .map((reason) => DropdownMenuItem(
                        value: reason,
                        child: Text(reason),
                      ))
                  .toList(),
              onChanged: (value) {
                setState(() => _selectedReason = value!);
              },
            ),
            if (_selectedReason == 'Other') ...[
              SizedBox(height: 16),
              TextField(
                controller: _reasonController,
                decoration: InputDecoration(
                  labelText: 'Please specify',
                  border: OutlineInputBorder(),
                ),
                maxLines: 3,
              ),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: Text('Cancel'),
        ),
        ElevatedButton(
          onPressed: () {
            final reason = _selectedReason == 'Other'
                ? _reasonController.text
                : _selectedReason;
            Navigator.pop(context, reason);
          },
          style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
          child: Text('Submit Request'),
        ),
      ],
    );
  }
}
```

---

## 📡 Backend API Endpoints Needed

### 1. Get Governance Settings

```javascript
// GET /api/admin/governance-settings
router.get("/governance-settings", async (req, res) => {
  try {
    const settings = await GovernanceSettings.findOne();
    res.json(settings);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});
```

### 2. Get Auction Templates

```javascript
// GET /api/admin/auction-templates
router.get("/auction-templates", async (req, res) => {
  try {
    const templates = await AuctionRuleTemplate.find({ active: true });
    res.json({ templates });
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});
```

### 3. Create Auction (Updated)

```javascript
// POST /api/auctions
router.post("/", async (req, res) => {
  try {
    const { lotId, startPrice, templateId } = req.body;

    // Fetch template
    const template = await AuctionRuleTemplate.findById(templateId);

    // Fetch global settings
    const settings = await GovernanceSettings.findOne();

    // Validate against template rules
    if (
      startPrice < settings.minReservePrice ||
      startPrice > template.maxReservePrice
    ) {
      return res.status(400).json({
        error: "Reserve price outside allowed range",
      });
    }

    // Determine initial status
    const status =
      template.requiresApproval || settings.requiresAdminApproval
        ? "pending_approval"
        : "created";

    const auction = await Auction.create({
      ...req.body,
      status,
      templateId,
      minBidIncrement: template.minBidIncrement,
    });

    res.json({ auction });
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});
```

### 4. Request Emergency Cancellation

```javascript
// POST /api/auctions/request-cancellation
router.post("/request-cancellation", authenticateToken, async (req, res) => {
  try {
    const { auctionId, reason } = req.body;
    const userId = req.user.id;

    // Verify user owns the auction
    const auction = await Auction.findById(auctionId);
    if (auction.farmerAddress !== req.user.walletAddress) {
      return res.status(403).json({ error: "Not authorized" });
    }

    // Create cancellation request
    const request = await CancellationRequest.create({
      auctionId,
      lotId: auction.lotId,
      requestedBy: req.user.walletAddress,
      reason,
      status: "pending",
    });

    // Notify admins
    await notifyAdmins("emergency_cancellation_request", request);

    res.json({ success: true, request });
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});
```

---

## 🔄 Summary of Changes Needed

### Mobile App (`create_auction_screen.dart`):

1. ✅ **Fetch governance settings** on screen init
2. ✅ **Fetch auction templates** and allow selection
3. ✅ **Dynamic duration options** from admin settings
4. ✅ **Display bid increment** (non-editable)
5. ✅ **Validate reserve price** against min/max
6. ✅ **Handle approval status** - show pending dialog
7. ✅ **Add emergency cancellation** request feature

### Backend API:

1. ✅ **Add governance settings endpoint**
2. ✅ **Add templates list endpoint**
3. ✅ **Update auction creation** to validate against templates
4. ✅ **Add cancellation request endpoint**
5. ✅ **Add admin notification system**

### Database Schema:

```sql
-- Add to auctions table
ALTER TABLE auctions ADD COLUMN template_id VARCHAR(255);
ALTER TABLE auctions ADD COLUMN min_bid_increment DECIMAL(5,2);

-- New table for cancellation requests
CREATE TABLE cancellation_requests (
  id SERIAL PRIMARY KEY,
  auction_id VARCHAR(255) NOT NULL,
  lot_id VARCHAR(255) NOT NULL,
  requested_by VARCHAR(255) NOT NULL,
  reason TEXT NOT NULL,
  status VARCHAR(50) DEFAULT 'pending',
  created_at TIMESTAMP DEFAULT NOW(),
  reviewed_at TIMESTAMP,
  reviewed_by VARCHAR(255)
);
```

---

## 🎨 UI Flow Diagram

```
Farmer Opens Create Auction
         ↓
   Fetch Governance Settings & Templates
         ↓
   Select Template (Optional)
         ↓
   Template Applied → Duration/Price Constraints
         ↓
   Fill Auction Details (Respects Limits)
         ↓
   Submit Auction
         ↓
   ┌─────────────────────┐
   │ Requires Approval?  │
   └─────────────────────┘
      Yes ↓         ↓ No
         ↓         ↓
   Status:     Status:
   pending_    created
   approval
         ↓         ↓
   Show Pending  Show Success
   Dialog        Dialog
         ↓
   Farmer Waits for
   Admin Approval
         ↓
   Admin Reviews in
   Web Dashboard
         ↓
   Approve/Reject
         ↓
   Mobile Push
   Notification
```

---

## 📲 Push Notifications (Bonus)

When admin approves/rejects:

```dart
// Add to push notification handler
void handleNotification(RemoteMessage message) {
  if (message.data['type'] == 'auction_approved') {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('🎉 Auction Approved!'),
        content: Text('Your auction has been approved and will start soon.'),
      ),
    );
  } else if (message.data['type'] == 'auction_rejected') {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('❌ Auction Rejected'),
        content: Text('Reason: ${message.data['reason']}'),
      ),
    );
  }
}
```

---

This integration ensures farmers operate within admin-defined governance rules while maintaining transparency and proper approval workflows.
