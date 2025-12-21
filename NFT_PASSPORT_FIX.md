# ✅ NFT Passport Issue - FIXED

## Problem

**Issue**: NFT passports created successfully during harvest registration, but not appearing in "My NFT Passport" page (`/dashboard/farmer/passports`).

## Root Cause

**Case-sensitivity mismatch** in wallet address comparison:

- Ethereum wallet addresses can be stored in different cases (checksummed vs lowercase)
- Backend was doing **exact case-sensitive** comparisons
- If a lot was created with address `0xABC...` but queried with `0xabc...`, it wouldn't match

## Fixes Applied

### 1. Backend API (`backend/src/routes/lot.js`)

#### Fix 1: Case-Insensitive Farmer Lookup

```javascript
// BEFORE (Line 139)
SELECT id FROM users WHERE wallet_address = $1

// AFTER
SELECT id FROM users WHERE LOWER(wallet_address) = LOWER($1)
```

#### Fix 2: Case-Insensitive Lot Filtering

```javascript
// BEFORE (Line 27)
query += ` AND farmer_address = $${paramIndex++}`;

// AFTER
query += ` AND LOWER(farmer_address) = LOWER($${paramIndex++})`;
```

#### Fix 3: Enhanced Logging

Added detailed logging to trace:

- Lot creation with farmer address
- Query execution with parameters
- Results returned from database

### 2. Frontend (`web/src/app/dashboard/farmer/passports/page.tsx`)

#### Fix 1: Better Error Handling

```typescript
// Added validation
if (!walletAddress) {
  setError("No wallet address found. Please connect your wallet.");
  setLoading(false);
  return;
}
```

#### Fix 2: Enhanced Debugging

```typescript
console.log("=== Fetching NFT Passports ===");
console.log("User wallet address:", walletAddress);
console.log("✅ Lots found:", lots.length);
console.log("📦 Processing lot:", lot);
```

#### Fix 3: Re-fetch on Wallet Change

```typescript
// Updated dependency array
useEffect(() => {
  if (user?.walletAddress) {
    fetchMyPassports();
  }
}, [user, user?.walletAddress]); // Re-fetch when wallet changes
```

## How to Verify the Fix

### Step 1: Start Backend (Already Running)

```bash
cd backend
npm run dev
```

**Expected Output:**

```
Server running on port 3002
Database: Using PostgreSQL (or in-memory mock)
```

### Step 2: Open Browser with Console

1. Navigate to http://localhost:3001
2. Login as farmer
3. Open DevTools (F12) → Console tab

### Step 3: Go to NFT Passports Page

1. Click "Dashboard" in header
2. Go to `/dashboard/farmer/passports`

**Expected Console Output:**

```
=== Fetching NFT Passports ===
User wallet address: 0xYourAddress
User role: farmer
API URL: http://localhost:3002/api/lots?farmer=0xYourAddress
API response status: 200
✅ Lots found: 1 (or however many you created)
📦 Processing lot: { lot_id: 'LOT-...', variety: 'Black Pepper', ... }
```

### Step 4: Create New Harvest

1. Go to "Register Harvest" or `/harvest/register`
2. Fill in all 5 steps:
   - Step 1: Harvest Details
   - Step 2: Processing Stages
   - Step 3: Certificate Upload
   - Step 4: Compliance Check
   - Step 5: Create NFT Passport
3. Click "Create NFT Passport"

**Backend Console (Expected):**

```
info: Creating new lot: { lotId: 'LOT-1733...', farmerAddress: '0x...', variety: 'Black Pepper' }
info: Found existing farmer with ID: 1
info: ✅ Lot created successfully: { lotId: 'LOT-...', farmer_id: 1, lot_db_id: 5 }
```

### Step 5: Verify Passport Appears

1. You'll be redirected to `/dashboard/farmer/passports`
2. Your new lot should appear as a passport card
3. Click "View Details" to see full passport
4. Click QR code icon to see QR code

## Testing Scenarios

### Scenario 1: First Time Registration

- [ ] Register first harvest
- [ ] NFT passport created successfully
- [ ] Redirected to passports page
- [ ] Passport card appears immediately

### Scenario 2: Multiple Harvests

- [ ] Register 2-3 harvests
- [ ] All passports appear in grid
- [ ] Can view details for each
- [ ] QR codes work for all

### Scenario 3: Refresh/Reload

- [ ] Create passport
- [ ] Close browser
- [ ] Reopen and login
- [ ] Navigate to passports page
- [ ] All passports still visible

### Scenario 4: Different Wallet

- [ ] Logout
- [ ] Connect different MetaMask account
- [ ] Login as different farmer
- [ ] See only that farmer's passports

## Troubleshooting

### Issue: Still No Passports After Fix

**Check 1: Wallet Address Match**

```bash
# In browser console (on passport page)
console.log('Wallet:', user?.walletAddress);

# In backend logs, find the lot creation entry:
# "✅ Lot created successfully: { farmerAddress: '0x...' }"
#
# Compare both addresses - they should match (case-insensitive)
```

**Check 2: Database Query**
If you have direct database access:

```sql
-- See all lots
SELECT lot_id, farmer_address, variety, quantity, created_at
FROM pepper_lots
ORDER BY created_at DESC;

-- Check specific farmer
SELECT lot_id, farmer_address, variety
FROM pepper_lots
WHERE LOWER(farmer_address) = LOWER('0xYourWalletAddress');
```

**Check 3: API Response**

```bash
# Test API directly
curl "http://localhost:3002/api/lots?farmer=0xYourWalletAddress"

# Should return:
# {
#   "success": true,
#   "count": 1,
#   "lots": [ { "lot_id": "LOT-...", ... } ]
# }
```

### Issue: "No wallet address found"

**Solution:**

1. Make sure MetaMask is connected
2. Try disconnecting and reconnecting MetaMask
3. Logout and login again
4. Check AuthContext is properly initialized

### Issue: Error 500 or Failed to Fetch

**Check:**

- Is backend running? Should be on port 3002
- Check backend console for errors
- Check Network tab in DevTools for failed requests

## Files Modified

### Backend

- `backend/src/routes/lot.js` (Lines 27, 118-120, 139-141, 186-192)

### Frontend

- `web/src/app/dashboard/farmer/passports/page.tsx` (Lines 41-90)

## Additional Improvements Made

1. **Success Messages**: Added message in API response
2. **Error Logging**: Comprehensive error logging throughout
3. **Validation**: Added wallet address validation before API call
4. **User Feedback**: Better loading states and error messages

## Expected Behavior After Fix

✅ Create harvest → NFT passport generated → Immediately visible in passports page  
✅ Refresh page → Passports still load correctly  
✅ Multiple harvests → All appear in chronological order  
✅ Case-insensitive wallet matching → Works regardless of address case  
✅ Clear error messages → User knows what went wrong

## Next Steps

1. **Test the fix** with the scenarios above
2. **Report back** if passports now appear correctly
3. If still issues, **share console logs**:
   - Browser console output
   - Backend terminal output
   - Any error messages

## Status

🔧 **Fixes Deployed**: December 5, 2025  
✅ **Backend**: Running with enhanced logging  
✅ **Frontend**: Updated with better debugging  
📊 **Testing**: Ready for verification

---

**Need Help?**
If passports still don't appear:

1. Copy the browser console output
2. Copy the backend logs (last 20 lines)
3. Share what you see vs what you expect

The enhanced logging will help us pinpoint exactly where the issue is!
