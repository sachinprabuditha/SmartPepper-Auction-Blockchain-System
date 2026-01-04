class ChatMessage {
  final String text;
  final bool isUser;
  final DateTime timestamp;
  final List<String>? sources;

  ChatMessage({
    required this.text,
    required this.isUser,
    required this.timestamp,
    this.sources,
  });
}

class RagChatRequest {
  final String message;
  final String? activeFarmId; // UUID string

  RagChatRequest({
    required this.message,
    this.activeFarmId,
  });

  Map<String, dynamic> toJson() {
    return {
      'message': message,
      'activeFarmId': activeFarmId,
    };
  }
}

class RagChatResponse {
  final String reply;
  final List<String> sources;

  RagChatResponse({
    required this.reply,
    required this.sources,
  });

  factory RagChatResponse.fromJson(Map<String, dynamic> json) {
    return RagChatResponse(
      reply: json['reply'] ?? '',
      sources: (json['sources'] as List<dynamic>?)?.map((e) => e.toString()).toList() ?? [],
    );
  }
}
