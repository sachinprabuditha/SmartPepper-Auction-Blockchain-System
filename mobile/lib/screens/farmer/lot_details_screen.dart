import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:qr_flutter/qr_flutter.dart';
import '../../providers/lot_provider.dart';
import '../../config/theme.dart';

class LotDetailsScreen extends StatelessWidget {
  final Lot lot;

  const LotDetailsScreen({super.key, required this.lot});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[50],
      appBar: AppBar(
        backgroundColor: AppTheme.forestGreen,
        elevation: 0,
        title: const Text(
          'Lot Details',
          style: TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.white),
          onPressed: () => Navigator.pop(context),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.share, color: Colors.white),
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                    content: Text('Share functionality coming soon')),
              );
            },
          ),
        ],
      ),
      body: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header Card
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(24),
              decoration: const BoxDecoration(
                color: AppTheme.forestGreen,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              lot.lotId,
                              style: const TextStyle(
                                fontSize: 24,
                                fontWeight: FontWeight.bold,
                                color: AppTheme.pepperGold,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              lot.variety,
                              style: const TextStyle(
                                fontSize: 18,
                                color: Colors.white,
                              ),
                            ),
                          ],
                        ),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 8,
                        ),
                        decoration: BoxDecoration(
                          color: _getStatusColor(lot.status),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Text(
                          lot.status.toUpperCase(),
                          style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                            fontSize: 12,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),

            // Digital Passport QR Code
            Container(
              margin: const EdgeInsets.all(20),
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 10,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Column(
                children: [
                  Row(
                    children: [
                      Icon(
                        Icons.qr_code_2,
                        color: AppTheme.forestGreen,
                        size: 28,
                      ),
                      const SizedBox(width: 12),
                      const Text(
                        'Digital Passport',
                        style: TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.bold,
                          color: AppTheme.forestGreen,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 20),
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(
                        color: AppTheme.forestGreen.withOpacity(0.3),
                        width: 2,
                      ),
                    ),
                    child: QrImageView(
                      data: _generateQRData(lot),
                      version: QrVersions.auto,
                      size: 200.0,
                      backgroundColor: Colors.white,
                      eyeStyle: const QrEyeStyle(
                        eyeShape: QrEyeShape.square,
                        color: AppTheme.forestGreen,
                      ),
                      dataModuleStyle: const QrDataModuleStyle(
                        dataModuleShape: QrDataModuleShape.square,
                        color: AppTheme.forestGreen,
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    'Scan to view complete traceability',
                    style: TextStyle(
                      color: Colors.grey[600],
                      fontSize: 14,
                    ),
                  ),
                  const SizedBox(height: 12),
                  OutlinedButton.icon(
                    onPressed: () {
                      Clipboard.setData(ClipboardData(text: lot.lotId));
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text('Lot ID copied to clipboard'),
                          duration: Duration(seconds: 2),
                        ),
                      );
                    },
                    icon: const Icon(Icons.copy, size: 16),
                    label: const Text('Copy Lot ID'),
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppTheme.forestGreen,
                      side: BorderSide(color: AppTheme.forestGreen),
                    ),
                  ),
                ],
              ),
            ),

            // Lot Information
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Lot Information',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.forestGreen,
                    ),
                  ),
                  const SizedBox(height: 16),
                  _buildInfoCard(context),
                  const SizedBox(height: 20),
                  const Text(
                    'Farmer Details',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.forestGreen,
                    ),
                  ),
                  const SizedBox(height: 16),
                  _buildFarmerCard(context),
                  const SizedBox(height: 20),
                  const Text(
                    'Compliance Status',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.forestGreen,
                    ),
                  ),
                  const SizedBox(height: 16),
                  _buildComplianceCard(context),
                  const SizedBox(height: 20),
                  const Text(
                    'Blockchain Traceability',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.forestGreen,
                    ),
                  ),
                  const SizedBox(height: 16),
                  _buildTraceabilityCard(context),
                  const SizedBox(height: 40),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  String _generateQRData(Lot lot) {
    // Generate JSON data for QR code containing all lot information including blockchain data
    return '''
{
  "lotId": "${lot.lotId}",
  "variety": "${lot.variety}",
  "quantity": ${lot.quantity},
  "harvestDate": "${lot.harvestDate.toIso8601String()}",
  "quality": "${lot.quality ?? 'N/A'}",
  "farmerName": "${lot.farmerName}",
  "farmerAddress": "${lot.farmerAddress}",
  "status": "${lot.status}",
  "complianceStatus": "${lot.complianceStatus}",
  "createdAt": "${lot.createdAt?.toIso8601String() ?? ''}",
  "blockchainTxHash": "${lot.blockchainTxHash ?? ''}",
  "certificateHash": "${lot.certificateHash ?? ''}",
  "certificateIpfsUrl": "${lot.certificateIpfsUrl ?? ''}",
  "origin": "${lot.origin ?? ''}",
  "farmLocation": "${lot.farmLocation ?? ''}",
  "organicCertified": ${lot.organicCertified ?? false}
}
''';
  }

  Color _getStatusColor(String status) {
    switch (status.toLowerCase()) {
      case 'created':
        return Colors.blue;
      case 'approved':
        return Colors.green;
      case 'auction':
        return Colors.orange;
      case 'sold':
        return Colors.purple;
      default:
        return Colors.grey;
    }
  }

  Widget _buildInfoCard(BuildContext context) {
    final harvestDate =
        '${lot.harvestDate.year}-${lot.harvestDate.month.toString().padLeft(2, '0')}-${lot.harvestDate.day.toString().padLeft(2, '0')}';

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          _buildDetailRow(
            Icons.scale,
            'Quantity',
            '${lot.quantity.toStringAsFixed(0)} kg',
          ),
          const Divider(height: 24),
          _buildDetailRow(
            Icons.star,
            'Quality Grade',
            lot.quality ?? 'N/A',
          ),
          const Divider(height: 24),
          _buildDetailRow(
            Icons.calendar_today,
            'Harvest Date',
            harvestDate,
          ),
          const Divider(height: 24),
          _buildDetailRow(
            Icons.access_time,
            'Created',
            _getTimeAgo(lot.createdAt ?? DateTime.now()),
          ),
        ],
      ),
    );
  }

  Widget _buildFarmerCard(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          _buildDetailRow(
            Icons.person,
            'Farmer Name',
            lot.farmerName,
          ),
          const Divider(height: 24),
          _buildDetailRow(
            Icons.account_balance_wallet,
            'Wallet Address',
            lot.farmerAddress.length > 18
                ? '${lot.farmerAddress.substring(0, 10)}...${lot.farmerAddress.substring(lot.farmerAddress.length - 8)}'
                : lot.farmerAddress,
            copyable: lot.farmerAddress,
          ),
        ],
      ),
    );
  }

  Widget _buildComplianceCard(BuildContext context) {
    final complianceColor = lot.complianceStatus.toLowerCase() == 'approved'
        ? Colors.green
        : lot.complianceStatus.toLowerCase() == 'pending'
            ? Colors.orange
            : Colors.red;

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: complianceColor.withOpacity(0.1),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(
              lot.complianceStatus.toLowerCase() == 'approved'
                  ? Icons.check_circle
                  : lot.complianceStatus.toLowerCase() == 'pending'
                      ? Icons.pending
                      : Icons.cancel,
              color: complianceColor,
              size: 32,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  lot.complianceStatus.toUpperCase(),
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: complianceColor,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  _getComplianceMessage(lot.complianceStatus),
                  style: TextStyle(
                    fontSize: 13,
                    color: Colors.grey[600],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTraceabilityCard(BuildContext context) {
    final hasBlockchainData =
        lot.blockchainTxHash != null && lot.blockchainTxHash!.isNotEmpty;

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: hasBlockchainData
                      ? Colors.green.withOpacity(0.1)
                      : Colors.orange.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Icon(
                  hasBlockchainData ? Icons.verified : Icons.pending,
                  color: hasBlockchainData ? Colors.green : Colors.orange,
                  size: 24,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      hasBlockchainData
                          ? 'Blockchain Verified'
                          : 'Pending Blockchain',
                      style: const TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 16,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      hasBlockchainData
                          ? 'Immutable on-chain record'
                          : 'Awaiting blockchain confirmation',
                      style: TextStyle(
                        color: Colors.grey[600],
                        fontSize: 13,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          if (hasBlockchainData) ...[
            const SizedBox(height: 20),
            const Divider(),
            const SizedBox(height: 16),

            // Transaction Hash
            _buildTraceabilityRow(
              icon: Icons.receipt_long,
              label: 'Transaction Hash',
              value: lot.blockchainTxHash!.length > 20
                  ? '${lot.blockchainTxHash!.substring(0, 10)}...${lot.blockchainTxHash!.substring(lot.blockchainTxHash!.length - 8)}'
                  : lot.blockchainTxHash!,
              fullValue: lot.blockchainTxHash,
              context: context,
            ),

            const SizedBox(height: 16),

            // Certificate Hash
            if (lot.certificateHash != null &&
                lot.certificateHash!.isNotEmpty) ...[
              _buildTraceabilityRow(
                icon: Icons.fingerprint,
                label: 'Certificate Hash',
                value: lot.certificateHash!.length > 20
                    ? '${lot.certificateHash!.substring(0, 10)}...${lot.certificateHash!.substring(lot.certificateHash!.length - 8)}'
                    : lot.certificateHash!,
                fullValue: lot.certificateHash,
                context: context,
              ),
              const SizedBox(height: 16),
            ],

            // IPFS URL
            if (lot.certificateIpfsUrl != null &&
                lot.certificateIpfsUrl!.isNotEmpty) ...[
              InkWell(
                onTap: () {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text('IPFS: ${lot.certificateIpfsUrl}'),
                      action: SnackBarAction(
                        label: 'Copy',
                        onPressed: () {
                          Clipboard.setData(
                              ClipboardData(text: lot.certificateIpfsUrl!));
                        },
                      ),
                    ),
                  );
                },
                child: Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.blue.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.blue.withOpacity(0.3)),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.cloud, color: Colors.blue, size: 20),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              'IPFS Certificate',
                              style: TextStyle(
                                fontWeight: FontWeight.w600,
                                fontSize: 13,
                              ),
                            ),
                            const SizedBox(height: 2),
                            Text(
                              'Tap to view details',
                              style: TextStyle(
                                fontSize: 11,
                                color: Colors.grey[600],
                              ),
                            ),
                          ],
                        ),
                      ),
                      const Icon(Icons.open_in_new,
                          color: Colors.blue, size: 18),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
            ],

            // Origin & Farm Location
            if (lot.origin != null || lot.farmLocation != null) ...[
              const Divider(),
              const SizedBox(height: 16),
              Row(
                children: [
                  const Icon(Icons.location_on,
                      color: AppTheme.forestGreen, size: 20),
                  const SizedBox(width: 8),
                  const Text(
                    'Origin Information',
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 14,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              if (lot.origin != null)
                Padding(
                  padding: const EdgeInsets.only(left: 28),
                  child: Text(
                    'Origin: ${lot.origin}',
                    style: const TextStyle(fontSize: 13),
                  ),
                ),
              if (lot.farmLocation != null)
                Padding(
                  padding: const EdgeInsets.only(left: 28, top: 4),
                  child: Text(
                    'Farm: ${lot.farmLocation}',
                    style: const TextStyle(fontSize: 13),
                  ),
                ),
              if (lot.organicCertified == true)
                Padding(
                  padding: const EdgeInsets.only(left: 28, top: 8),
                  child: Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.green.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(4),
                    ),
                    child: const Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.eco, color: Colors.green, size: 14),
                        SizedBox(width: 4),
                        Text(
                          'Organic Certified',
                          style: TextStyle(
                            color: Colors.green,
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
            ],

            const SizedBox(height: 16),
            const Divider(),
            const SizedBox(height: 12),

            // View on Blockchain Button
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: () {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('Opening blockchain explorer...'),
                    ),
                  );
                  // TODO: Open blockchain explorer with tx hash
                },
                icon: const Icon(Icons.open_in_new),
                label: const Text('View on Blockchain'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AppTheme.forestGreen,
                  side: const BorderSide(color: AppTheme.forestGreen),
                  padding: const EdgeInsets.symmetric(vertical: 12),
                ),
              ),
            ),
          ] else ...[
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.orange.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Row(
                children: [
                  Icon(Icons.info_outline, size: 18, color: Colors.orange[700]),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      'This lot will be recorded on blockchain once approved',
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.orange[900],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildTraceabilityRow({
    required IconData icon,
    required String label,
    required String value,
    String? fullValue,
    required BuildContext context,
  }) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, color: AppTheme.forestGreen, size: 18),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: TextStyle(
                  fontSize: 12,
                  color: Colors.grey[600],
                ),
              ),
              const SizedBox(height: 4),
              Text(
                value,
                style: const TextStyle(
                  fontSize: 13,
                  fontWeight: FontWeight.w500,
                  fontFamily: 'monospace',
                ),
              ),
            ],
          ),
        ),
        if (fullValue != null)
          IconButton(
            icon: const Icon(Icons.copy, size: 16),
            padding: EdgeInsets.zero,
            constraints: const BoxConstraints(),
            onPressed: () {
              Clipboard.setData(ClipboardData(text: fullValue));
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('$label copied'),
                  duration: const Duration(seconds: 2),
                ),
              );
            },
          ),
      ],
    );
  }

  Widget _buildDetailRow(IconData icon, String label, String value,
      {String? copyable}) {
    return Row(
      children: [
        Icon(icon, color: AppTheme.forestGreen, size: 20),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: TextStyle(
                  fontSize: 12,
                  color: Colors.grey[600],
                ),
              ),
              const SizedBox(height: 4),
              Text(
                value,
                style: const TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ),
        if (copyable != null)
          IconButton(
            icon: const Icon(Icons.copy, size: 16),
            onPressed: () {
              Clipboard.setData(ClipboardData(text: copyable));
            },
            color: AppTheme.forestGreen,
          ),
      ],
    );
  }

  String _getTimeAgo(DateTime dateTime) {
    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inDays > 0) {
      return '${difference.inDays}d ago';
    } else if (difference.inHours > 0) {
      return '${difference.inHours}h ago';
    } else if (difference.inMinutes > 0) {
      return '${difference.inMinutes}m ago';
    } else {
      return 'Just now';
    }
  }

  String _getComplianceMessage(String status) {
    switch (status.toLowerCase()) {
      case 'approved':
        return 'All compliance checks passed';
      case 'pending':
        return 'Awaiting compliance verification';
      case 'rejected':
        return 'Compliance issues detected';
      default:
        return 'Status unknown';
    }
  }
}
