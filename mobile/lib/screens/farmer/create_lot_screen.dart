import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';
import '../../providers/auth_provider.dart';
import '../../providers/lot_provider.dart';
import '../../config/theme.dart';

class CreateLotScreen extends StatefulWidget {
  const CreateLotScreen({super.key});

  @override
  State<CreateLotScreen> createState() => _CreateLotScreenState();
}

class _CreateLotScreenState extends State<CreateLotScreen> {
  final _formKey = GlobalKey<FormState>();
  final _varietyController = TextEditingController();
  final _quantityController = TextEditingController();
  final _harvestDateController = TextEditingController();
  final _descriptionController = TextEditingController();

  String _selectedQuality = 'AAA';
  DateTime? _selectedHarvestDate;
  List<File> _certificateImages = [];
  bool _isLoading = false;

  final List<String> _qualityGrades = ['AAA', 'AA', 'A', 'B'];
  final List<String> _pepperVarieties = [
    'Black Pepper Premium',
    'Black Pepper Standard',
    'Black Pepper Organic',
    'White Pepper',
    'Green Pepper',
  ];

  @override
  void dispose() {
    _varietyController.dispose();
    _quantityController.dispose();
    _harvestDateController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  Future<void> _selectHarvestDate() async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: DateTime.now(),
      firstDate: DateTime.now().subtract(const Duration(days: 365)),
      lastDate: DateTime.now(),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: ColorScheme.light(
              primary: AppTheme.forestGreen,
              onPrimary: Colors.white,
              surface: Colors.white,
              onSurface: Colors.black,
            ),
          ),
          child: child!,
        );
      },
    );

    if (picked != null) {
      setState(() {
        _selectedHarvestDate = picked;
        _harvestDateController.text =
            '${picked.year}-${picked.month.toString().padLeft(2, '0')}-${picked.day.toString().padLeft(2, '0')}';
      });
    }
  }

  Future<void> _pickCertificateImage() async {
    final ImagePicker picker = ImagePicker();

    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading: const Icon(Icons.photo_camera),
              title: const Text('Take Photo'),
              onTap: () async {
                Navigator.pop(context);
                final XFile? image = await picker.pickImage(
                  source: ImageSource.camera,
                  imageQuality: 80,
                );
                if (image != null) {
                  setState(() {
                    _certificateImages.add(File(image.path));
                  });
                }
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_library),
              title: const Text('Choose from Gallery'),
              onTap: () async {
                Navigator.pop(context);
                final List<XFile> images = await picker.pickMultiImage(
                  imageQuality: 80,
                );
                if (images.isNotEmpty) {
                  setState(() {
                    _certificateImages.addAll(images.map((e) => File(e.path)));
                  });
                }
              },
            ),
          ],
        ),
      ),
    );
  }

  void _removeCertificateImage(int index) {
    setState(() {
      _certificateImages.removeAt(index);
    });
  }

  Future<void> _submitLot() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    if (_selectedHarvestDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please select harvest date'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    setState(() {
      _isLoading = true;
    });

    try {
      final authProvider = context.read<AuthProvider>();
      final lotProvider = context.read<LotProvider>();

      // Prepare lot data - generate unique lot ID
      final lotId = 'LOT-${DateTime.now().millisecondsSinceEpoch}';

      final lotData = {
        'lotId': lotId,
        'farmerAddress': authProvider.user?.walletAddress ?? '',
        'variety': _varietyController.text.trim(),
        'quantity': double.parse(_quantityController.text.trim()),
        'quality': _selectedQuality,
        'harvestDate': _selectedHarvestDate!.toIso8601String(),
        'origin': 'Sri Lanka',
        'farmLocation': authProvider.user?.name ?? 'Unknown Farm',
        'organicCertified': false,
        'metadataURI': '',
        'certificateHash': '',
        'certificateIpfsUrl': '',
        'txHash': '',
      };

      final success = await lotProvider.createLot(lotData);

      if (success && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Lot created successfully!'),
            backgroundColor: Colors.green,
          ),
        );
        Navigator.pop(context);
      } else if (mounted) {
        throw Exception('Failed to create lot');
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Error: ${e.toString()}'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[50],
      appBar: AppBar(
        backgroundColor: AppTheme.forestGreen,
        elevation: 0,
        title: const Text(
          'Register New Lot',
          style: TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.white),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Info Card
                    Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: AppTheme.forestGreen.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                          color: AppTheme.forestGreen.withOpacity(0.3),
                        ),
                      ),
                      child: Row(
                        children: [
                          Icon(
                            Icons.info_outline,
                            color: AppTheme.forestGreen,
                            size: 24,
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Text(
                              'Register your pepper lot with complete details for blockchain traceability',
                              style: TextStyle(
                                color: AppTheme.forestGreen,
                                fontSize: 13,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),

                    const SizedBox(height: 24),

                    // Variety Selection
                    const Text(
                      'Pepper Variety *',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: AppTheme.forestGreen,
                      ),
                    ),
                    const SizedBox(height: 8),
                    DropdownButtonFormField<String>(
                      value: _varietyController.text.isEmpty
                          ? null
                          : _varietyController.text,
                      decoration: InputDecoration(
                        hintText: 'Select variety',
                        prefixIcon: const Icon(Icons.grass),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        filled: true,
                        fillColor: AppTheme.forestGreen,
                      ),
                      items: _pepperVarieties.map((variety) {
                        return DropdownMenuItem(
                          value: variety,
                          child: Text(variety),
                        );
                      }).toList(),
                      onChanged: (value) {
                        setState(() {
                          _varietyController.text = value ?? '';
                        });
                      },
                      validator: (value) {
                        if (value == null || value.isEmpty) {
                          return 'Please select a variety';
                        }
                        return null;
                      },
                    ),

                    const SizedBox(height: 20),

                    // Quantity Input
                    const Text(
                      'Quantity (kg) *',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: AppTheme.forestGreen,
                      ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _quantityController,
                      keyboardType: TextInputType.number,
                      style: const TextStyle(color: Colors.white),
                      inputFormatters: [
                        FilteringTextInputFormatter.allow(
                            RegExp(r'^\d+\.?\d{0,2}')),
                      ],
                      decoration: InputDecoration(
                        hintText: 'Enter quantity in kg',
                        prefixIcon: const Icon(Icons.scale),
                        suffixText: 'kg',
                        iconColor: AppTheme.forestGreen,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        filled: true,
                        fillColor: AppTheme.forestGreen,
                      ),
                      validator: (value) {
                        if (value == null || value.isEmpty) {
                          return 'Please enter quantity';
                        }
                        final quantity = double.tryParse(value);
                        if (quantity == null || quantity <= 0) {
                          return 'Please enter a valid quantity';
                        }
                        return null;
                      },
                    ),

                    const SizedBox(height: 20),

                    // Quality Grade
                    const Text(
                      'Quality Grade *',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: AppTheme.forestGreen,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Wrap(
                      spacing: 8,
                      children: _qualityGrades.map((grade) {
                        final isSelected = _selectedQuality == grade;
                        return ChoiceChip(
                          label: Text(grade),
                          selected: isSelected,
                          onSelected: (selected) {
                            setState(() {
                              _selectedQuality = grade;
                            });
                          },
                          selectedColor: AppTheme.pepperGold,
                          backgroundColor: Colors.grey[200],
                          labelStyle: TextStyle(
                            color: isSelected ? Colors.white : Colors.black87,
                            fontWeight: isSelected
                                ? FontWeight.bold
                                : FontWeight.normal,
                          ),
                        );
                      }).toList(),
                    ),

                    const SizedBox(height: 20),

                    // Harvest Date
                    const Text(
                      'Harvest Date *',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: AppTheme.forestGreen,
                      ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _harvestDateController,
                      readOnly: true,
                      style: const TextStyle(color: Colors.white),
                      decoration: InputDecoration(
                        hintText: 'Select harvest date',
                        prefixIcon: const Icon(Icons.calendar_today),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        filled: true,
                        fillColor: AppTheme.forestGreen,
                      ),
                      onTap: _selectHarvestDate,
                      validator: (value) {
                        if (value == null || value.isEmpty) {
                          return 'Please select harvest date';
                        }
                        return null;
                      },
                    ),

                    const SizedBox(height: 20),

                    // Description
                    const Text(
                      'Description (Optional)',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: AppTheme.forestGreen,
                      ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _descriptionController,
                      maxLines: 3,
                      style: const TextStyle(color: Colors.black87),
                      decoration: InputDecoration(
                        hintText: 'Add additional notes about this lot',
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        filled: true,
                        fillColor: Colors.white,
                      ),
                    ),

                    const SizedBox(height: 20),

                    // Certification Upload Section
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text(
                          'Certificates',
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                            color: AppTheme.forestGreen,
                          ),
                        ),
                        TextButton.icon(
                          onPressed: _pickCertificateImage,
                          icon: const Icon(Icons.add_photo_alternate),
                          label: const Text('Add'),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),

                    if (_certificateImages.isEmpty)
                      Container(
                        padding: const EdgeInsets.all(32),
                        decoration: BoxDecoration(
                          color: Colors.grey[100],
                          borderRadius: BorderRadius.circular(12),
                          border: Border.all(
                            color: Colors.grey[300]!,
                            style: BorderStyle.solid,
                          ),
                        ),
                        child: Center(
                          child: Column(
                            children: [
                              Icon(
                                Icons.cloud_upload_outlined,
                                size: 48,
                                color: Colors.grey[400],
                              ),
                              const SizedBox(height: 8),
                              Text(
                                'No certificates added yet',
                                style: TextStyle(color: Colors.grey[600]),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                'Tap "Add" to upload certificates',
                                style: TextStyle(
                                  color: Colors.grey[500],
                                  fontSize: 12,
                                ),
                              ),
                            ],
                          ),
                        ),
                      )
                    else
                      SizedBox(
                        height: 120,
                        child: ListView.builder(
                          scrollDirection: Axis.horizontal,
                          itemCount: _certificateImages.length,
                          itemBuilder: (context, index) {
                            return Stack(
                              children: [
                                Container(
                                  width: 120,
                                  height: 120,
                                  margin: const EdgeInsets.only(right: 12),
                                  decoration: BoxDecoration(
                                    borderRadius: BorderRadius.circular(12),
                                    image: DecorationImage(
                                      image:
                                          FileImage(_certificateImages[index]),
                                      fit: BoxFit.cover,
                                    ),
                                  ),
                                ),
                                Positioned(
                                  top: 4,
                                  right: 16,
                                  child: GestureDetector(
                                    onTap: () => _removeCertificateImage(index),
                                    child: Container(
                                      padding: const EdgeInsets.all(4),
                                      decoration: const BoxDecoration(
                                        color: Colors.red,
                                        shape: BoxShape.circle,
                                      ),
                                      child: const Icon(
                                        Icons.close,
                                        color: Colors.white,
                                        size: 16,
                                      ),
                                    ),
                                  ),
                                ),
                              ],
                            );
                          },
                        ),
                      ),

                    const SizedBox(height: 32),

                    // Submit Button
                    SizedBox(
                      width: double.infinity,
                      height: 50,
                      child: ElevatedButton(
                        onPressed: _isLoading ? null : _submitLot,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppTheme.forestGreen,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        child: _isLoading
                            ? const SizedBox(
                                width: 24,
                                height: 24,
                                child: CircularProgressIndicator(
                                  color: Colors.white,
                                  strokeWidth: 2,
                                ),
                              )
                            : const Text(
                                'Register Lot',
                                style: TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.bold,
                                  color: Colors.white,
                                ),
                              ),
                      ),
                    ),

                    const SizedBox(height: 20),
                  ],
                ),
              ),
            ),
    );
  }
}
