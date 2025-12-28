const express = require('express');
const router = express.Router();
const db = require('../db/database');
const BlockchainService = require('../services/blockchainService');
const ComplianceService = require('../services/complianceService');
const logger = require('../utils/logger');
const { v4: uuidv4 } = require('uuid');

const blockchainService = new BlockchainService();
const complianceService = new ComplianceService();

// Initialize services
blockchainService.initialize().catch(err => logger.error('Blockchain init failed:', err));
complianceService.initialize().catch(err => logger.error('Compliance init failed:', err));

/**
 * GET /api/auctions
 * Get all auctions with optional filters
 */
router.get('/', async (req, res) => {
  try {
    const { status, farmer, limit = 50, offset = 0 } = req.query;
    
    let query = `
      SELECT 
        a.*,
        p.variety,
        p.quantity,
        p.quality,
        p.origin
      FROM auctions a
      LEFT JOIN pepper_lots p ON a.lot_id = p.lot_id
      WHERE 1=1
    `;
    let countQuery = 'SELECT COUNT(*) FROM auctions WHERE 1=1';
    const params = [];
    const countParams = [];
    let paramIndex = 1;
    let countParamIndex = 1;

    if (status) {
      query += ` AND a.status = $${paramIndex++}`;
      countQuery += ` AND status = $${countParamIndex++}`;
      params.push(status);
      countParams.push(status);
    }

    if (farmer) {
      query += ` AND LOWER(a.farmer_address) = LOWER($${paramIndex++})`;
      countQuery += ` AND LOWER(farmer_address) = LOWER($${countParamIndex++})`;
      params.push(farmer);
      countParams.push(farmer);
    }

    query += ` ORDER BY a.created_at DESC LIMIT $${paramIndex++} OFFSET $${paramIndex++}`;
    params.push(limit, offset);

    const [result, countResult] = await Promise.all([
      db.query(query, params),
      db.query(countQuery, countParams)
    ]);

    res.json({
      success: true,
      count: parseInt(countResult.rows[0].count),
      auctions: result.rows
    });
  } catch (error) {
    logger.error('Error fetching auctions:', error);
    res.status(500).json({
      success: false,
      error: 'Failed to fetch auctions'
    });
  }
});

/**
 * GET /api/auctions/:id
 * Get auction details by ID
 */
router.get('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    
    const result = await db.query(
      'SELECT * FROM auctions WHERE auction_id = $1',
      [id]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({
        success: false,
        error: 'Auction not found'
      });
    }

    // Get bids
    const bidsResult = await db.query(
      'SELECT * FROM bids WHERE auction_id = $1 ORDER BY placed_at DESC',
      [id]
    );

    res.json({
      success: true,
      auction: result.rows[0],
      bids: bidsResult.rows
    });
  } catch (error) {
    logger.error('Error fetching auction:', error);
    res.status(500).json({
      success: false,
      error: 'Failed to fetch auction'
    });
  }
});

/**
 * GET /api/auctions/check-eligibility/:lotId
 * Check if a lot is eligible for auction creation
 * Validates all preconditions before allowing auction creation
 */
router.get('/check-eligibility/:lotId', async (req, res) => {
  try {
    const { lotId } = req.params;
    const reasons = [];
    let eligible = true;

    // 1. Check if lot exists
    const lotResult = await db.query(
      'SELECT * FROM pepper_lots WHERE lot_id = $1',
      [lotId]
    );

    if (lotResult.rows.length === 0) {
      eligible = false;
      reasons.push('Pepper lot does not exist in the system');
      return res.json({ eligible, reasons });
    }

    const lot = lotResult.rows[0];

    // 2. Check if lot is approved/available
    if (lot.status !== 'approved' && lot.status !== 'available') {
      eligible = false;
      reasons.push(`Lot status is "${lot.status}". Only approved or available lots can be auctioned.`);
    }

    // 3. Check if lot already has an active auction
    const activeAuctionResult = await db.query(
      `SELECT * FROM auctions 
       WHERE lot_id = $1 
       AND status IN ('created', 'active', 'pending')
       LIMIT 1`,
      [lotId]
    );

    if (activeAuctionResult.rows.length > 0) {
      eligible = false;
      reasons.push('This lot already has an active auction');
    }

    // 4. Check if required certificates are uploaded
    const certsResult = await db.query(
      `SELECT COUNT(*) as cert_count 
       FROM certifications 
       WHERE lot_id = $1`,
      [lotId]
    );

    const certCount = parseInt(certsResult.rows[0].cert_count);
    if (certCount < 3) {
      eligible = false;
      reasons.push(`Insufficient certificates uploaded. Found ${certCount}, minimum 3 required.`);
    }

    // 5. Check compliance status
    const complianceResult = await db.query(
      `SELECT passed, rule_name, rule_type, details, checked_at
       FROM compliance_checks
       WHERE lot_id = $1
       ORDER BY checked_at DESC
       LIMIT 1`,
      [lotId]
    );

    if (complianceResult.rows.length === 0) {
      eligible = false;
      reasons.push('No compliance checks have been performed for this lot');
    } else {
      const compliance = complianceResult.rows[0];
      
      if (!compliance.passed) {
        const details = compliance.details || {};
        eligible = false;
        reasons.push(`Compliance check "${compliance.rule_name}" failed. Rule type: ${compliance.rule_type}`);
      }
    }

    // 6. Check if lot has processing stages (traceability)
    const stagesResult = await db.query(
      `SELECT COUNT(*) as stage_count 
       FROM processing_stages 
       WHERE lot_id = $1`,
      [lotId]
    );

    const stageCount = parseInt(stagesResult.rows[0].stage_count);
    if (stageCount < 2) {
      eligible = false;
      reasons.push(`Insufficient processing stages. Found ${stageCount}, minimum 2 required for traceability.`);
    }

    // 7. Check if lot has blockchain passport (minted NFT)
    if (!lot.blockchain_tx_hash || lot.blockchain_tx_hash.trim() === '') {
      eligible = false;
      reasons.push('Lot does not have a blockchain passport (NFT not minted)');
    }

    res.json({
      eligible,
      reasons: eligible ? [] : reasons,
      lot: {
        lotId: lot.lot_id,
        variety: lot.variety,
        quantity: lot.quantity,
        status: lot.status,
        certificateCount: certCount,
        stageCount: stageCount,
        hasBlockchainPassport: !!lot.blockchain_tx_hash
      }
    });

  } catch (error) {
    logger.error('Error checking auction eligibility:', error);
    res.status(500).json({
      eligible: false,
      reasons: ['Internal server error while checking eligibility'],
      error: error.message
    });
  }
});

/**
 * POST /api/auctions
 * Create a new auction (Governance-based approach)
 * 
 * Flow:
 * 1. Validate farmer inputs
 * 2. Check all preconditions (eligibility)
 * 3. Prepare auction payload
 * 4. Create immutable on-chain record
 * 5. Store off-chain volatile data
 * 6. Schedule auction activation
 */
router.post('/', async (req, res) => {
  try {
    const {
      lotId,
      farmerAddress,
      reservePrice,
      quantity,
      duration, // in days
      startTime,
      endTime,
      preferredDestinations = [],
      templateId // NEW - Governance template
    } = req.body;

    // === STEP 1: Validate Required Inputs ===
    if (!lotId || !farmerAddress || !reservePrice || !quantity || !duration) {
      return res.status(400).json({
        success: false,
        error: 'Missing required fields',
        required: ['lotId', 'farmerAddress', 'reservePrice', 'quantity', 'duration']
      });
    }

    // Validate numeric inputs
    const reservePriceNum = parseFloat(reservePrice);
    const quantityNum = parseFloat(quantity);
    const durationNum = parseInt(duration);

    if (isNaN(reservePriceNum) || reservePriceNum <= 0) {
      return res.status(400).json({
        success: false,
        error: 'Reserve price must be a positive number'
      });
    }

    if (isNaN(quantityNum) || quantityNum <= 0) {
      return res.status(400).json({
        success: false,
        error: 'Quantity must be a positive number'
      });
    }

    if (isNaN(durationNum) || durationNum < 1 || durationNum > 30) {
      return res.status(400).json({
        success: false,
        error: 'Duration must be between 1 and 30 days'
      });
    }

    // === GOVERNANCE VALIDATION ===
    // Fetch governance settings
    const settingsResult = await db.query('SELECT * FROM governance_settings LIMIT 1');
    const settings = settingsResult.rows[0];

    if (!settings) {
      return res.status(500).json({
        success: false,
        error: 'Governance settings not configured'
      });
    }

    // Convert duration to hours for validation
    const durationHours = durationNum * 24;

    // Validate against global settings
    if (!settings.allowed_durations.includes(durationHours)) {
      return res.status(400).json({
        success: false,
        error: `Duration not allowed. Allowed durations: ${settings.allowed_durations.map(h => h / 24).join(', ')} days`
      });
    }

    if (reservePriceNum < parseFloat(settings.min_reserve_price)) {
      return res.status(400).json({
        success: false,
        error: `Reserve price must be at least ${settings.min_reserve_price} LKR`
      });
    }

    if (reservePriceNum > parseFloat(settings.max_reserve_price)) {
      return res.status(400).json({
        success: false,
        error: `Reserve price cannot exceed ${settings.max_reserve_price} LKR`
      });
    }

    // Template-specific validation
    let template = null;
    let requiresApproval = settings.requires_admin_approval;
    let minBidIncrement = parseFloat(settings.default_bid_increment);

    if (templateId) {
      const templateResult = await db.query(
        'SELECT * FROM auction_rule_templates WHERE template_id = $1 AND active = true',
        [templateId]
      );

      if (templateResult.rows.length === 0) {
        return res.status(400).json({
          success: false,
          error: 'Invalid or inactive template'
        });
      }

      template = templateResult.rows[0];

      // Validate against template rules
      if (durationHours < template.min_duration_hours) {
        return res.status(400).json({
          success: false,
          error: `Duration must be at least ${template.min_duration_hours / 24} days for ${template.name}`
        });
      }

      if (durationHours > template.max_duration_hours) {
        return res.status(400).json({
          success: false,
          error: `Duration cannot exceed ${template.max_duration_hours / 24} days for ${template.name}`
        });
      }

      if (template.max_reserve_price && reservePriceNum > parseFloat(template.max_reserve_price)) {
        return res.status(400).json({
          success: false,
          error: `Reserve price cannot exceed ${template.max_reserve_price} LKR for ${template.name}`
        });
      }

      requiresApproval = template.requires_approval;
      minBidIncrement = parseFloat(template.min_bid_increment);
    }

    // === STEP 2: Check Eligibility (All Preconditions) ===
    const lotResult = await db.query(
      'SELECT * FROM pepper_lots WHERE lot_id = $1',
      [lotId]
    );

    if (lotResult.rows.length === 0) {
      return res.status(404).json({
        success: false,
        error: 'Lot not found'
      });
    }

    const lot = lotResult.rows[0];

    // Verify farmer ownership
    if (lot.farmer_address.toLowerCase() !== farmerAddress.toLowerCase()) {
      return res.status(403).json({
        success: false,
        error: 'You do not own this lot'
      });
    }

    // Check quantity availability
    if (quantityNum > lot.quantity) {
      return res.status(400).json({
        success: false,
        error: `Requested quantity (${quantityNum} kg) exceeds available quantity (${lot.quantity} kg)`
      });
    }

    // Run full eligibility check
    const eligibilityReasons = [];
    let eligible = true;

    // Check lot status
    if (lot.status !== 'approved' && lot.status !== 'available') {
      eligible = false;
      eligibilityReasons.push(`Lot status must be "approved" or "available", currently "${lot.status}"`);
    }

    // Check for active auctions
    const activeAuctionResult = await db.query(
      `SELECT * FROM auctions 
       WHERE lot_id = $1 
       AND status IN ('created', 'active', 'pending', 'scheduled')
       LIMIT 1`,
      [lotId]
    );

    if (activeAuctionResult.rows.length > 0) {
      eligible = false;
      eligibilityReasons.push('This lot already has an active or scheduled auction');
    }

    // Check certificates (minimum 3)
    const certsResult = await db.query(
      `SELECT COUNT(*) as cert_count FROM certifications WHERE lot_id = $1`,
      [lotId]
    );
    const certCount = parseInt(certsResult.rows[0].cert_count);
    if (certCount < 3) {
      eligible = false;
      eligibilityReasons.push(`Minimum 3 certificates required (found ${certCount})`);
    }

    // Check compliance status
    const complianceResult = await db.query(
      `SELECT passed, rule_name, rule_type, details
       FROM compliance_checks
       WHERE lot_id = $1
       ORDER BY checked_at DESC
       LIMIT 1`,
      [lotId]
    );

    if (complianceResult.rows.length === 0) {
      eligible = false;
      eligibilityReasons.push('No compliance checks performed');
    } else {
      const compliance = complianceResult.rows[0];
      if (!compliance.passed) {
        eligible = false;
        eligibilityReasons.push(`Compliance check "${compliance.rule_name}" failed (rule type: ${compliance.rule_type})`);
      }
    }

    // Check processing stages (minimum 2)
    const stagesResult = await db.query(
      `SELECT COUNT(*) as stage_count FROM processing_stages WHERE lot_id = $1`,
      [lotId]
    );
    const stageCount = parseInt(stagesResult.rows[0].stage_count);
    if (stageCount < 2) {
      eligible = false;
      eligibilityReasons.push(`Minimum 2 processing stages required for traceability (found ${stageCount})`);
    }

    // Check blockchain passport
    if (!lot.blockchain_tx_hash || lot.blockchain_tx_hash.trim() === '') {
      eligible = false;
      eligibilityReasons.push('Blockchain passport (NFT) not minted for this lot');
    }

    if (!eligible) {
      return res.status(400).json({
        success: false,
        error: 'Lot is not eligible for auction',
        reasons: eligibilityReasons
      });
    }

    // === STEP 3: Calculate Timestamps ===
    const calculatedStartTime = startTime ? new Date(startTime) : new Date(Date.now() + 3600000); // 1 hour from now
    const calculatedEndTime = endTime 
      ? new Date(endTime) 
      : new Date(calculatedStartTime.getTime() + (durationNum * 24 * 3600000));

    // Validate time range
    if (calculatedEndTime <= calculatedStartTime) {
      return res.status(400).json({
        success: false,
        error: 'End time must be after start time'
      });
    }

    const durationSeconds = Math.floor((calculatedEndTime - calculatedStartTime) / 1000);

    // === STEP 4: Create Immutable On-Chain Record ===
    logger.info('Creating auction on blockchain', {
      lotId,
      reservePrice: reservePriceNum,
      durationSeconds
    });

    const blockchainResult = await blockchainService.createAuction({
      lotId,
      startPrice: reservePriceNum, // Starting price = reserve price in this model
      reservePrice: reservePriceNum,
      duration: durationSeconds
    });

    // Validate and parse auction ID
    let auctionIdNum;
    if (!blockchainResult.auctionId || blockchainResult.auctionId === '0') {
      // Fallback: Use timestamp if blockchain doesn't return ID
      auctionIdNum = Math.floor(Date.now() / 1000);
      logger.warn('Using timestamp-based auction ID', { auctionIdNum });
    } else {
      auctionIdNum = parseInt(blockchainResult.auctionId);
      if (isNaN(auctionIdNum)) {
        throw new Error(`Invalid auction ID from blockchain: ${blockchainResult.auctionId}`);
      }
    }

    logger.info('Blockchain auction created', {
      auctionId: auctionIdNum,
      txHash: blockchainResult.txHash
    });

    // === STEP 5: Store Off-Chain Data (Volatile + UI preferences) ===
    // Determine initial status based on approval requirement
    const initialStatus = requiresApproval 
      ? 'pending_approval' 
      : (calculatedStartTime > new Date() ? 'created' : 'active');

    const insertResult = await db.query(
      `INSERT INTO auctions (
        auction_id, lot_id, farmer_address,
        start_price, reserve_price, start_time, end_time,
        status, compliance_passed, blockchain_tx_hash,
        template_id, min_bid_increment, admin_approved
      ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13)
      RETURNING *`,
      [
        auctionIdNum,
        lotId,
        farmerAddress,
        reservePriceNum, // start_price = reserve_price
        reservePriceNum,
        calculatedStartTime,
        calculatedEndTime,
        initialStatus,
        true, // compliance_passed - lot is eligible if we reach this point
        blockchainResult.txHash,
        templateId || null,
        minBidIncrement,
        !requiresApproval // admin_approved = true if no approval needed
      ]
    );

    const auction = insertResult.rows[0];

    logger.info('Auction stored in database', {
      auctionId: auction.auction_id,
      status: auction.status,
      startTime: auction.start_time,
      endTime: auction.end_time
    });

    // === STEP 6: Return Success Response ===
    res.status(201).json({
      success: true,
      message: 'Auction created successfully',
      auction: {
        auctionId: auction.auction_id,
        lotId: auction.lot_id,
        farmerAddress: auction.farmer_address,
        reservePrice: auction.reserve_price,
        quantity: quantityNum,
        startTime: auction.start_time,
        endTime: auction.end_time,
        status: auction.status,
        blockchainTxHash: auction.blockchain_tx_hash,
        preferredDestinations,
        onChainData: {
          immutable: true,
          auctionId: auctionIdNum,
          reservePrice: reservePriceNum,
          startTime: calculatedStartTime,
          endTime: calculatedEndTime,
          txHash: blockchainResult.txHash
        },
        offChainData: {
          volatile: true,
          preferredDestinations,
          quantity: quantityNum,
          certificateCount: certCount,
          stageCount
        }
      }
    });

  } catch (error) {
    logger.error('Error creating auction:', error);
    res.status(500).json({
      success: false,
      error: 'Failed to create auction',
      details: error.message
    });
  }
});

/**
 * POST /api/auctions/:id/bid
 * Place a bid on an auction
 */
router.post('/:id/bid', async (req, res) => {
  try {
    const { id } = req.params;
    const { bidderAddress, amount, txHash } = req.body;

    if (!bidderAddress || !amount || !txHash) {
      return res.status(400).json({
        success: false,
        error: 'Missing required fields'
      });
    }

    // Get bidder ID
    const bidderResult = await db.query(
      'SELECT id FROM users WHERE wallet_address = $1',
      [bidderAddress]
    );

    const bidderId = bidderResult.rows.length > 0 ? bidderResult.rows[0].id : null;

    // Store bid in database
    const result = await db.query(
      `INSERT INTO bids (
        auction_id, bidder_id, bidder_address, amount, blockchain_tx_hash
      ) VALUES ($1, $2, $3, $4, $5)
      RETURNING *`,
      [parseInt(id), bidderId, bidderAddress, amount, txHash]
    );

    // Update auction
    await db.query(
      `UPDATE auctions 
       SET current_bid = $1, current_bidder_address = $2, bid_count = bid_count + 1
       WHERE auction_id = $3`,
      [amount, bidderAddress, parseInt(id)]
    );

    // Broadcast via WebSocket (will be handled by websocket service)
    
    res.status(201).json({
      success: true,
      bid: result.rows[0]
    });
  } catch (error) {
    logger.error('Error placing bid:', error);
    res.status(500).json({
      success: false,
      error: 'Failed to place bid',
      details: error.message
    });
  }
});

/**
 * POST /api/auctions/request-cancellation
 * Request emergency cancellation of an auction
 */
router.post('/request-cancellation', async (req, res) => {
  try {
    const { auctionId, reason, farmerAddress } = req.body;

    if (!auctionId || !reason || !farmerAddress) {
      return res.status(400).json({
        success: false,
        error: 'Missing required fields: auctionId, reason, farmerAddress'
      });
    }

    // Verify auction exists and belongs to farmer
    const auctionResult = await db.query(
      'SELECT * FROM auctions WHERE auction_id = $1',
      [auctionId]
    );

    if (auctionResult.rows.length === 0) {
      return res.status(404).json({
        success: false,
        error: 'Auction not found'
      });
    }

    const auction = auctionResult.rows[0];

    if (auction.farmer_address.toLowerCase() !== farmerAddress.toLowerCase()) {
      return res.status(403).json({
        success: false,
        error: 'You are not authorized to cancel this auction'
      });
    }

    // Check if auction can be cancelled (must be active or created)
    if (!['created', 'active', 'pending_approval'].includes(auction.status)) {
      return res.status(400).json({
        success: false,
        error: `Cannot request cancellation for auction with status "${auction.status}"`
      });
    }

    // Check if cancellation request already exists
    const existingRequest = await db.query(
      `SELECT * FROM cancellation_requests 
       WHERE auction_id = $1 AND status = 'pending'`,
      [auctionId]
    );

    if (existingRequest.rows.length > 0) {
      return res.status(400).json({
        success: false,
        error: 'A cancellation request for this auction is already pending'
      });
    }

    // Create cancellation request
    const requestResult = await db.query(
      `INSERT INTO cancellation_requests 
       (auction_id, lot_id, requested_by, reason, status)
       VALUES ($1, $2, $3, $4, 'pending')
       RETURNING *`,
      [auctionId, auction.lot_id, farmerAddress, reason]
    );

    logger.info('Cancellation request created', {
      requestId: requestResult.rows[0].id,
      auctionId,
      reason
    });

    res.json({
      success: true,
      message: 'Cancellation request submitted successfully. Admin will review.',
      request: {
        id: requestResult.rows[0].id,
        auctionId: requestResult.rows[0].auction_id,
        lotId: requestResult.rows[0].lot_id,
        reason: requestResult.rows[0].reason,
        status: requestResult.rows[0].status,
        createdAt: requestResult.rows[0].created_at
      }
    });
  } catch (error) {
    logger.error('Error creating cancellation request:', error);
    res.status(500).json({
      success: false,
      error: 'Failed to submit cancellation request'
    });
  }
});

/**
 * POST /api/auctions/:id/end
 * End an auction
 */
router.post('/:id/end', async (req, res) => {
  try {
    const { id } = req.params;

    const result = await db.query(
      'SELECT * FROM auctions WHERE auction_id = $1',
      [parseInt(id)]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({
        success: false,
        error: 'Auction not found'
      });
    }

    const auction = result.rows[0];

    // Update status
    await db.query(
      'UPDATE auctions SET status = $1 WHERE auction_id = $2',
      ['ended', parseInt(id)]
    );

    res.json({
      success: true,
      message: 'Auction ended successfully',
      auction: { ...auction, status: 'ended' }
    });
  } catch (error) {
    logger.error('Error ending auction:', error);
    res.status(500).json({
      success: false,
      error: 'Failed to end auction'
    });
  }
});

/**
 * POST /api/auctions/:id/settle
 * Settle an auction
 */
router.post('/:id/settle', async (req, res) => {
  try {
    const { id } = req.params;

    const result = await db.query(
      'SELECT * FROM auctions WHERE auction_id = $1',
      [parseInt(id)]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({
        success: false,
        error: 'Auction not found'
      });
    }

    const auction = result.rows[0];

    if (auction.status !== 'ended') {
      return res.status(400).json({
        success: false,
        error: 'Auction must be ended before settling'
      });
    }

    // Update status
    await db.query(
      'UPDATE auctions SET status = $1 WHERE auction_id = $2',
      ['settled', parseInt(id)]
    );

    // Update lot status
    await db.query(
      'UPDATE pepper_lots SET status = $1 WHERE lot_id = $2',
      ['sold', auction.lot_id]
    );

    res.json({
      success: true,
      message: 'Auction settled successfully'
    });
  } catch (error) {
    logger.error('Error settling auction:', error);
    res.status(500).json({
      success: false,
      error: 'Failed to settle auction'
    });
  }
});

module.exports = router;
