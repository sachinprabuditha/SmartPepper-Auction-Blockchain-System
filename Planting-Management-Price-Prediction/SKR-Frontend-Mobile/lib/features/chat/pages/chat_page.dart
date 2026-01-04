import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:skr_frontend_mobile/core/theme/app_theme.dart';
import '../models/chat_model.dart';
import '../services/chat_service.dart';
// Import Farm Logic
import '../../plantation/models/farm_record_model.dart';
import '../../plantation/controllers/plantation_controller.dart';

class ChatPage extends ConsumerStatefulWidget {
  const ChatPage({Key? key}) : super(key: key);

  @override
  ConsumerState<ChatPage> createState() => _ChatPageState();
}

class _ChatPageState extends ConsumerState<ChatPage> {
  final TextEditingController _controller = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  final List<ChatMessage> _messages = [];
  bool _isLoading = false;
  String? _selectedFarmId; // Null = Guide Mode

  @override
  void dispose() {
    _controller.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          _scrollController.position.maxScrollExtent,
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOut,
        );
      }
    });
  }

  Future<void> _sendMessage() async {
    final text = _controller.text.trim();
    if (text.isEmpty) return;

    _controller.clear();
    setState(() {
      _messages.add(ChatMessage(
        text: text,
        isUser: true,
        timestamp: DateTime.now(),
      ));
      _isLoading = true;
    });
    _scrollToBottom();

    try {
      final chatService = ref.read(chatServiceProvider);
      // Pass the selected farm ID (or null for Guide Mode)
      final response = await chatService.sendMessage(text, activeFarmId: _selectedFarmId);

      setState(() {
        _messages.add(ChatMessage(
          text: response.reply,
          isUser: false,
          timestamp: DateTime.now(),
          sources: response.sources
        ));
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _messages.add(ChatMessage(
          text: 'Error: ${e.toString()}',
          isUser: false,
          timestamp: DateTime.now(),
        ));
        _isLoading = false;
      });
    }
    _scrollToBottom();
  }

  @override
  Widget build(BuildContext context) {
    // Fetch farms for the dropdown
    final farmsAsync = ref.watch(farmsProvider);
    
    return Scaffold(
      appBar: AppBar(
        title: const Text('AI Assistant'),
        backgroundColor: AppTheme.primaryGreen,
        foregroundColor: Colors.white,
        actions: [
          // Farm Selector in AppBar
          farmsAsync.when(
            data: (farms) {
               if (farms.isEmpty) return const SizedBox.shrink();
               return DropdownButtonHideUnderline(
                 child: DropdownButton<String?>(
                   dropdownColor: AppTheme.primaryGreen,
                   icon: const Icon(Icons.arrow_drop_down, color: Colors.white),
                   value: _selectedFarmId,
                   hint: const Text("Mode: Guide", style: TextStyle(color: Colors.white70)),
                   items: [
                     const DropdownMenuItem<String?>(
                       value: null,
                       child: Text("Mode: Guide (General)", style: TextStyle(color: Colors.white)),
                     ),
                     ...farms.map((f) => DropdownMenuItem<String?>(
                       value: f.id,
                       child: Text("Mode: ${f.farmName}", style: TextStyle(color: Colors.white)),
                     ))
                   ],
                   onChanged: (value) {
                     setState(() {
                       _selectedFarmId = value;
                     });
                   },
                 ),
               );
            },
            loading: () => const SizedBox.shrink(),
            error: (_,__) => const SizedBox.shrink(),
          ),
          const SizedBox(width: 16),
        ],
      ),
      body: Column(
        children: [
          // Mode Indicator
          Container(
            width: double.infinity,
            padding: const EdgeInsets.symmetric(vertical: 4, horizontal: 16),
            color: _selectedFarmId == null ? Colors.blueGrey[100] : Colors.green[100],
            child: Text(
              _selectedFarmId == null 
                ? "🌱 Guide Mode: General agronomy advice only." 
                : "🌿 Farmer Mode: Advice tailored to this farm.",
              style: TextStyle(
                fontSize: 12, 
                color: _selectedFarmId == null ? Colors.blueGrey[800] : Colors.green[800],
                fontWeight: FontWeight.bold
              ),
              textAlign: TextAlign.center,
            ),
          ),
          
          Expanded(
            child: ListView.builder(
              controller: _scrollController,
              padding: const EdgeInsets.all(16),
              itemCount: _messages.length,
              itemBuilder: (context, index) {
                final msg = _messages[index];
                return _buildMessageBubble(msg);
              },
            ),
          ),
          if (_isLoading)
            const Padding(
              padding: EdgeInsets.all(8.0),
              child: CircularProgressIndicator(),
            ),
          _buildInputArea(),
        ],
      ),
    );
  }

  Widget _buildMessageBubble(ChatMessage msg) {
    return Align(
      alignment: msg.isUser ? Alignment.centerRight : Alignment.centerLeft,
      child: Container(
        margin: const EdgeInsets.symmetric(vertical: 4),
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: msg.isUser ? AppTheme.primaryGreen : Colors.grey[200],
          borderRadius: BorderRadius.circular(12),
        ),
        constraints: BoxConstraints(maxWidth: MediaQuery.of(context).size.width * 0.75),
        child: Column(
           crossAxisAlignment: CrossAxisAlignment.start,
           children: [
             Text(
               msg.text,
               style: TextStyle(color: msg.isUser ? Colors.white : Colors.black87),
             ),
             if (!msg.isUser && msg.sources != null && msg.sources!.isNotEmpty)
                Padding(
                  padding: const EdgeInsets.only(top: 8.0),
                  child: Text(
                    "Sources: ${msg.sources!.join(', ')}",
                    style: TextStyle(fontSize: 10, color: Colors.grey[600], fontStyle: FontStyle.italic),
                  ),
                )
           ],
        ),
      ),
    );
  }

  Widget _buildInputArea() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Row(
        children: [
          Expanded(
            child: TextField(
              controller: _controller,
              decoration: InputDecoration(
                hintText: _selectedFarmId == null ? 'Ask about black pepper...' : 'Ask about your farm...',
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
                contentPadding: const EdgeInsets.symmetric(horizontal: 16),
              ),
              onSubmitted: (_) => _sendMessage(),
            ),
          ),
          const SizedBox(width: 8),
          IconButton(
            onPressed: _sendMessage,
            icon: const Icon(Icons.send),
            color: AppTheme.primaryGreen,
          ),
        ],
      ),
    );
  }
}
