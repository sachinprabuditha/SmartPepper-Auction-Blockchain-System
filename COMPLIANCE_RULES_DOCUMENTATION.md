# 📋 Compliance Rules Documentation

## Overview

SmartPepper's compliance engine automatically validates pepper lots against destination-specific export requirements. This ensures only compliant lots can proceed to auction, preventing costly rejections and maintaining market reputation.

## Compliance Architecture

```
┌─────────────────┐
│  Lot Creation   │
│   (5-step       │
│    wizard)      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Processing     │ ◄── Captures: moisture, packaging material
│   Stages        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Certifications  │ ◄── Uploads: organic, fumigation, pesticide tests
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Compliance      │
│   Check         │ ◄── Validates against destination rules
│ (Step 4)        │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
 PASSED    FAILED
    │         │
    │         └──► Cannot list for auction
    │               Shows failed rules
    │               Farmer must fix issues
    │
    ▼
┌─────────────────┐
│ Auction Listing │ ◄── Only compliant lots visible
│   (Available)   │
└─────────────────┘
```

---

## Compliance Rules by Destination

### 🇪🇺 European Union (EU)

**Total Rules:** 6 rules (3 critical, 3 major)

#### 1. EU_ORGANIC_CERT ⚠️ CRITICAL

- **Requirement:** Valid organic certification
- **Database Check:** `certifications` table
  ```sql
  WHERE cert_type = 'organic'
  AND is_valid = true
  AND expiry_date > NOW()
  ```
- **Pass Criteria:** Certificate exists and not expired
- **Farmer Action:** Upload organic certification from approved body (e.g., APEDA)

#### 2. EU_FUMIGATION_CERT ⚠️ CRITICAL

- **Requirement:** Fumigation treatment certificate
- **Database Check:** `certifications` table
  ```sql
  WHERE cert_type = 'fumigation'
  AND is_valid = true
  AND expiry_date > NOW()
  ```
- **Pass Criteria:** Valid fumigation certificate
- **Farmer Action:** Provide fumigation certificate with treatment details

#### 3. EU_PESTICIDE_RESIDUE ⚠️ CRITICAL

- **Requirement:** Pesticide residue test showing compliance with EU MRLs
- **Database Check:** `certifications` table
  ```sql
  WHERE cert_type = 'pesticide_test'
  AND is_valid = true
  ```
- **Pass Criteria:** Lab test certificate exists
- **Farmer Action:** Get pesticide residue lab test from accredited laboratory

#### 4. EU_QUALITY_GRADE ⚡ MAJOR

- **Requirement:** Quality grade A, AA, or AAA
- **Database Check:** `pepper_lots` table
  ```sql
  WHERE quality IN ('A', 'AA', 'AAA')
  ```
- **Pass Criteria:** Lot quality meets premium standards
- **Farmer Action:** Ensure proper grading during processing

#### 5. EU_MOISTURE_LIMIT ⚡ MAJOR

- **Requirement:** Moisture content ≤ 12.5%
- **Database Check:** `processing_stages` table
  ```sql
  SELECT quality_metrics->>'moisture'
  WHERE stage_type = 'drying'
  ORDER BY timestamp DESC LIMIT 1
  ```
- **Pass Criteria:** Most recent drying stage shows moisture ≤ 12.5%
- **Farmer Action:**
  - Record moisture during drying stage
  - Ensure adequate drying time
  - Use moisture meter for accuracy

#### 6. EU_PACKAGING_STANDARD ⚡ MAJOR

- **Requirement:** Food-grade packaging materials
- **Database Check:** `processing_stages` table
  ```sql
  SELECT quality_metrics->>'package_material'
  WHERE stage_type = 'packaging'
  ```
- **Pass Criteria:** Material in approved list:
  - HDPE (High-Density Polyethylene)
  - PP (Polypropylene)
  - PET (Polyethylene Terephthalate)
  - Glass
  - Jute_with_liner
  - Food_grade_plastic
- **Farmer Action:** Select approved material during packaging stage

#### 7. EU_TRACEABILITY ⚡ MAJOR

- **Requirement:** Complete processing chain documentation
- **Database Check:** `processing_stages` table
  ```sql
  SELECT stage_type GROUP BY lot_id
  ```
- **Pass Criteria:** All stages recorded: harvest → drying → grading → packaging
- **Farmer Action:** Complete all processing stages in wizard

---

### 🇺🇸 United States (FDA)

**Total Rules:** 5 rules (4 critical, 1 major)

#### 1. FDA_PHYTOSANITARY ⚠️ CRITICAL

- **Requirement:** Phytosanitary certificate for plant health
- **Database Check:** `certifications` table
  ```sql
  WHERE cert_type = 'phytosanitary'
  AND is_valid = true
  AND expiry_date > NOW()
  ```
- **Pass Criteria:** Valid phytosanitary certificate
- **Farmer Action:** Obtain from national plant protection organization

#### 2. FDA_FUMIGATION ⚠️ CRITICAL

- **Requirement:** Fumigation documentation
- **Database Check:** `certifications` table
  ```sql
  WHERE cert_type = 'fumigation'
  AND is_valid = true
  ```
- **Pass Criteria:** Fumigation certificate on file
- **Farmer Action:** Provide fumigation treatment records

#### 3. FDA_PESTICIDE_MRL ⚠️ CRITICAL

- **Requirement:** Pesticide Maximum Residue Levels (MRL) compliance
- **Database Check:** `certifications` table
  ```sql
  WHERE cert_type = 'pesticide_test'
  AND is_valid = true
  ```
- **Pass Criteria:** Lab test showing compliance with FDA tolerances
- **Farmer Action:** Get pesticide residue analysis from FDA-recognized lab

#### 4. FDA_PACKAGING ⚠️ CRITICAL

- **Requirement:** FDA-approved food contact materials
- **Database Check:** `processing_stages` table
  ```sql
  SELECT quality_metrics->>'package_material'
  WHERE stage_type = 'packaging'
  ```
- **Pass Criteria:** Material in FDA-approved list:
  - HDPE
  - PP
  - PET
  - Glass
  - FDA_approved_plastic
- **Farmer Action:** Use FDA-compliant packaging materials

#### 5. FDA_MOISTURE_LIMIT ⚡ MAJOR

- **Requirement:** Moisture content ≤ 13.0%
- **Database Check:** `processing_stages` table
  ```sql
  SELECT quality_metrics->>'moisture'
  WHERE stage_type = 'drying'
  ```
- **Pass Criteria:** Moisture ≤ 13.0%
- **Farmer Action:** Monitor and record moisture during drying

---

### 🕌 Middle East Markets

**Total Rules:** 5 rules (all major - stricter than EU/FDA in some aspects)

#### 1. ME_HALAL_CERT ⚡ MAJOR

- **Requirement:** Halal certification
- **Database Check:** `certifications` table
  ```sql
  WHERE cert_type = 'halal'
  AND is_valid = true
  AND expiry_date > NOW()
  ```
- **Pass Criteria:** Valid halal certificate from recognized body
- **Farmer Action:** Obtain halal certification for processing facility
- **Note:** Major (not critical) - recommended but not mandatory

#### 2. ME_QUALITY_GRADE ⚡ MAJOR

- **Requirement:** Premium quality grades only
- **Database Check:** `pepper_lots` table
  ```sql
  WHERE quality IN ('AA', 'AAA', 'Premium')
  ```
- **Pass Criteria:** Only premium grades accepted
- **Farmer Action:** Maintain high quality standards, proper sorting

#### 3. ME_MOISTURE_LIMIT ⚡ MAJOR

- **Requirement:** Moisture content ≤ 11.0% (STRICTEST)
- **Database Check:** `processing_stages` table
  ```sql
  SELECT quality_metrics->>'moisture'
  WHERE stage_type = 'drying'
  ```
- **Pass Criteria:** Moisture ≤ 11.0%
- **Farmer Action:** Extended drying period compared to EU/FDA

#### 4. ME_PACKAGING ⚡ MAJOR

- **Requirement:** Traditional or premium packaging materials
- **Database Check:** `processing_stages` table
  ```sql
  SELECT quality_metrics->>'package_material'
  WHERE stage_type = 'packaging'
  ```
- **Pass Criteria:** Preferred materials:
  - Jute_with_liner (traditional, preferred)
  - PP
  - HDPE
  - Food_grade_plastic
- **Farmer Action:** Use traditional jute bags for premium markets

#### 5. ME_ORIGIN_CERT ⚡ MAJOR

- **Requirement:** Certificate of origin for customs
- **Database Check:** `certifications` table
  ```sql
  WHERE cert_type = 'origin'
  AND is_valid = true
  ```
- **Pass Criteria:** Origin certificate exists
- **Farmer Action:** Obtain from chamber of commerce

---

## Compliance Severity Levels

### ⚠️ CRITICAL

- **Impact:** Must pass to proceed to auction
- **Blocking:** Lot cannot be listed if failed
- **Fix Required:** Immediately
- **Examples:** Safety (pesticides), legal requirements (fumigation)

### ⚡ MAJOR

- **Impact:** Strongly recommended
- **Blocking:** Lot can be listed but with warnings
- **Fix Recommended:** Before auction
- **Examples:** Quality standards, packaging preferences

### ℹ️ MINOR (Future)

- **Impact:** Advisory only
- **Blocking:** No
- **Fix Optional:** For premium pricing
- **Examples:** Optional certifications, enhanced documentation

---

## Compliance Check Workflow

### 1. Lot Creation (Steps 1-3)

Farmer completes:

- Harvest details
- Processing stages (captures moisture, packaging material)
- Certificate uploads

### 2. Compliance Validation (Step 4)

```typescript
POST /api/compliance/check/:lotId
{
  "destination": "EU" | "FDA" | "MIDDLE_EAST"
}

Response:
{
  "complianceStatus": "passed" | "failed",
  "results": [
    {
      "code": "EU_ORGANIC_CERT",
      "name": "EU Organic Certification Required",
      "category": "certification",
      "severity": "critical",
      "passed": true,
      "details": "Valid organic certificate found: ORG-2025-001"
    },
    {
      "code": "EU_MOISTURE_LIMIT",
      "name": "Moisture Content Standard",
      "category": "quality",
      "severity": "major",
      "passed": false,
      "details": "Moisture content 14.2% exceeds EU limit of 12.5%"
    }
  ]
}
```

### 3. Results Interpretation

**All Critical Rules Passed:**

```javascript
complianceStatus = "passed";
// Lot can proceed to auction
```

**Any Critical Rule Failed:**

```javascript
complianceStatus = "failed";
// Lot BLOCKED from auction
// Farmer sees failed rules with remediation steps
```

**Only Major Rules Failed:**

```javascript
complianceStatus = "passed"; // (with warnings)
// Lot can proceed
// Warning shown to farmer
```

---

## Database Schema

### compliance_checks Table

```sql
CREATE TABLE compliance_checks (
  id UUID PRIMARY KEY,
  lot_id VARCHAR(50) REFERENCES pepper_lots(lot_id),
  destination VARCHAR(50),         -- EU, FDA, MIDDLE_EAST
  results JSONB,                   -- Array of check results
  compliance_status VARCHAR(20),   -- passed, failed
  checked_at TIMESTAMP,
  check_duration_ms INTEGER        -- Performance metric
);
```

### compliance_rules Table (Future Enhancement)

```sql
CREATE TABLE compliance_rules (
  id UUID PRIMARY KEY,
  destination VARCHAR(50),
  rule_code VARCHAR(50),
  rule_name VARCHAR(255),
  category VARCHAR(50),
  severity VARCHAR(20),
  enabled BOOLEAN DEFAULT true,
  description TEXT,
  remediation_steps TEXT,
  created_at TIMESTAMP,
  updated_at TIMESTAMP
);
```

---

## Farmer Guidance

### Quick Compliance Checklist

**For EU Export:**

- [ ] Organic certification uploaded and valid
- [ ] Fumigation certificate uploaded
- [ ] Pesticide residue test completed
- [ ] Quality grade A or better
- [ ] Moisture ≤ 12.5% (record during drying)
- [ ] Food-grade packaging (HDPE, PP, PET, or approved)
- [ ] All stages documented (harvest → drying → grading → packaging)

**For FDA (US) Export:**

- [ ] Phytosanitary certificate from NPPO
- [ ] Fumigation documentation
- [ ] Pesticide MRL test from FDA-recognized lab
- [ ] FDA-approved packaging material
- [ ] Moisture ≤ 13.0%

**For Middle East Export:**

- [ ] Halal certification (recommended)
- [ ] Premium quality grade (AA, AAA, or Premium)
- [ ] Moisture ≤ 11.0% (STRICT)
- [ ] Traditional packaging preferred (Jute with liner)
- [ ] Certificate of origin

### Common Compliance Failures & Fixes

#### ❌ "Moisture content 14.2% exceeds EU limit of 12.5%"

**Fix:**

1. Extend drying time
2. Re-test moisture with calibrated meter
3. Update drying stage quality_metrics
4. Re-run compliance check

#### ❌ "Missing pesticide residue test certificate"

**Fix:**

1. Send sample to accredited lab
2. Wait for test results (2-5 days)
3. Upload certificate in Step 3
4. Re-run compliance check

#### ❌ "Package material 'Cardboard' does not meet EU requirements"

**Fix:**

1. Repackage in approved material (HDPE, PP, etc.)
2. Update packaging stage quality_metrics
3. Re-run compliance check

#### ❌ "Missing processing stages: packaging"

**Fix:**

1. Go back to Step 2
2. Add packaging stage with required details
3. Continue to Step 4

---

## API Reference

### Run Compliance Check

```http
POST /api/compliance/check/:lotId
Content-Type: application/json

{
  "destination": "EU"
}
```

**Response (Success):**

```json
{
  "success": true,
  "complianceStatus": "passed",
  "results": [
    {
      "code": "EU_ORGANIC_CERT",
      "name": "EU Organic Certification Required",
      "category": "certification",
      "severity": "critical",
      "passed": true,
      "details": "Valid organic certificate found: ORG-2025-KL-1234"
    }
  ],
  "timestamp": "2025-12-01T10:30:00Z"
}
```

**Response (Failure):**

```json
{
  "success": true,
  "complianceStatus": "failed",
  "results": [
    {
      "code": "EU_PESTICIDE_RESIDUE",
      "name": "Pesticide Residue Limits",
      "category": "safety",
      "severity": "critical",
      "passed": false,
      "details": "Missing pesticide residue test certificate required for EU export"
    }
  ],
  "criticalFailures": 1,
  "majorFailures": 0,
  "timestamp": "2025-12-01T10:30:00Z"
}
```

---

## Performance Metrics

**Target:** <2000ms per compliance check

**Current Performance (estimated):**

- EU (6 rules): ~350ms
- FDA (5 rules): ~370ms
- Middle East (5 rules): ~380ms

**Optimization Strategies:**

- Database indexes on lot_id, cert_type
- Rule caching in memory
- Async validation for non-critical rules
- Batch certificate queries

---

## Future Enhancements

### Planned Rules (V2)

1. **Heavy Metal Limits** - Test for lead, cadmium
2. **Aflatoxin Testing** - Mycotoxin analysis
3. **Microbial Limits** - E.coli, Salmonella tests
4. **Traceability Blockchain** - On-chain processing log verification
5. **Smart Contract Compliance** - Automated blocking on blockchain

### Dynamic Rule Engine

```javascript
// Load rules from database instead of hardcoded
const rules = await loadRulesFromDB(destination);

// Enable/disable rules per customer
if (customer.premiumTier) {
  rules = rules.filter((r) => r.enabled);
}

// Custom thresholds
if (customer.moistureLimit) {
  rules.find((r) => r.code === "MOISTURE").threshold = customer.moistureLimit;
}
```

---

## Testing Compliance Rules

```powershell
# Run compliance timing tests
cd backend
npm run test:compliance

# Expected output:
# EU checks: 350ms average
# All checks complete
# ✅ PASSED (<2000ms target)
```

---

## Conclusion

The compliance engine is a core differentiator for SmartPepper:

- **Prevents auction failures** - Blocks non-compliant lots proactively
- **Builds market trust** - Exporters confident in lot quality
- **Farmer guidance** - Clear remediation steps
- **Regulatory compliance** - Meets international standards

**Next Steps:**

1. Test enhanced rules with real farmer data
2. Add more destination markets (Japan, Australia)
3. Implement smart contract compliance validation
4. Build compliance certificate generator (PDF)

---

_Last Updated: December 1, 2025_
_Version: 2.0 (Enhanced Rules)_
