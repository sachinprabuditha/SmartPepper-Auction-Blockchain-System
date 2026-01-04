import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:skr_frontend_mobile/core/network/api_client.dart';
import '../models/chat_model.dart';
import 'package:skr_frontend_mobile/features/auth/models/api_response_model.dart';

final chatServiceProvider = Provider<ChatService>((ref) {
  return ChatService(ApiClient());
});

class ChatService {
  final ApiClient _apiClient;

  ChatService(this._apiClient);

  Future<RagChatResponse> sendMessage(String message, {String? activeFarmId}) async {
    try {
      final requestBody = RagChatRequest(
        message: message,
        activeFarmId: activeFarmId,
      ).toJson();

      final response = await _apiClient.dio.post(
        '/chat',
        data: requestBody,
      );

      final apiResponse = ApiResponseModel<RagChatResponse>.fromJson(
        response.data, 
        (json) => RagChatResponse.fromJson(json as Map<String, dynamic>)
      );
      
      if (apiResponse.success && apiResponse.data != null) {
        return apiResponse.data!;
      } else {
        throw Exception(apiResponse.message ?? 'Unknown error');
      }
    } catch (e) {
      throw Exception('Failed to send message: $e');
    }
  }
}
