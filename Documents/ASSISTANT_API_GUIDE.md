# Tài liệu API Trợ lý Ảo AI (AI Assistant) - IGCSE Platform

Tài liệu này mô tả module **Trợ lý Ảo AI** cho nền tảng học tập IGCSE, bao gồm các REST API endpoints và **SignalR Hub** cho real-time chat.

---

## Mục lục

1. [Tổng quan](#1-tổng-quan)
2. [Danh sách API](#2-danh-sách-api)
3. [REST API Endpoints](#3-rest-api-endpoints)
4. [SignalR Real-time Communication](#4-signalr-real-time-communication)
5. [Tích hợp Frontend](#5-tích-hợp-frontend)
6. [Business Rules](#6-business-rules)
7. [Ví dụ sử dụng](#7-ví-dụ-sử-dụng)
8. [Error Handling](#8-error-handling)

---

## 1. Tổng quan

### 1.1. Giới thiệu

Module **Trợ lý Ảo AI** cung cấp tính năng chat real-time với trợ lý AI thông minh, hỗ trợ:

- **Học viên (Student)**: Hỏi đáp bài học, gợi ý học tập, động viên
- **Giáo viên (Teacher)**: Hỗ trợ giảng dạy, phân tích dữ liệu, tư vấn
- **Phụ huynh (Parent)**: Theo dõi tiến độ, lời khuyên giáo dục

### 1.2. Tính năng chính

- ✅ **Real-time Chat**: Giao tiếp tức thời qua **SignalR WebSocket**
- ✅ **Streaming Response**: Phản hồi từng phần (typing effect)
- ✅ **Multi-Persona**: Tính cách khác nhau cho từng loại người dùng
- ✅ **RAG System**: Trả lời chính xác dựa trên nội dung khóa học
- ✅ **Context-Aware**: Hiểu ngữ cảnh học tập của từng học viên
- ✅ **Conversation History**: Lưu trữ lịch sử trò chuyện

### 1.3. Base URL

```
REST API: /api/assistant
SignalR Hub: /chatHub
```

### 1.4. Authentication

Tất cả các API và SignalR Hub yêu cầu:

- **JWT Token** trong header: `Authorization: Bearer <token>`
- **SignalR**: Token có thể truyền qua query string: `?access_token=<token>`
- **Roles**: `Student`, `Teacher`, `Parent` (tất cả đều có thể sử dụng)

### 1.5. Response Format

Tất cả REST API trả về format chuẩn:

```json
{
  "message": "Success message",
  "statusCode": 200,
  "data": { ... }
}
```

---

## 2. Danh sách API

Module này cung cấp **5 REST API endpoints** và **1 SignalR Hub**:

| #   | Method   | Endpoint                                     | Mô tả                         | Streaming |
| --- | -------- | -------------------------------------------- | ----------------------------- | --------- |
| 1   | `GET`    | `/api/assistant/conversations`               | Lấy danh sách cuộc trò chuyện | ❌        |
| 2   | `GET`    | `/api/assistant/conversations/{id}/messages` | Lấy lịch sử tin nhắn          | ❌        |
| 3   | `POST`   | `/api/assistant/conversations`               | Tạo cuộc trò chuyện mới       | ❌        |
| 4   | `DELETE` | `/api/assistant/conversations/{id}`          | Xóa cuộc trò chuyện           | ❌        |
| 5   | `POST`   | `/api/assistant/conversations/{id}/messages` | Gửi tin nhắn (REST)           | ❌        |
| 6   | SignalR  | `/chatHub`                                   | Real-time chat với streaming  | ✅        |

**Lưu ý**: Để có trải nghiệm tốt nhất, nên sử dụng **SignalR Hub** cho real-time chat với streaming response.

---

## 3. REST API Endpoints

### 3.1. Lấy danh sách cuộc trò chuyện

**Endpoint:** `GET /api/assistant/conversations`

**Mô tả:** Lấy tất cả cuộc trò chuyện của user hiện tại, sắp xếp theo thời gian cập nhật mới nhất.

**Request:**

```http
GET /api/assistant/conversations
Authorization: Bearer <token>
```

**Response:**

```json
{
  "message": "Lấy danh sách cuộc trò chuyện thành công",
  "statusCode": 200,
  "data": [
    {
      "conversationId": 1,
      "title": "Hỏi về phương trình bậc hai",
      "personaType": "StudentHelper",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T11:00:00Z",
      "isActive": true
    },
    {
      "conversationId": 2,
      "title": "Tư vấn về phương pháp giảng dạy",
      "personaType": "TeacherAssistant",
      "createdAt": "2024-01-14T09:00:00Z",
      "updatedAt": "2024-01-14T09:30:00Z",
      "isActive": true
    }
  ]
}
```

**Response Schema:**

| Field            | Type       | Mô tả                                                                    |
| ---------------- | ---------- | ------------------------------------------------------------------------ |
| `conversationId` | `int`      | ID của cuộc trò chuyện                                                   |
| `title`          | `string?`  | Tiêu đề cuộc trò chuyện (lấy từ tin nhắn đầu tiên)                       |
| `personaType`    | `string?`  | Loại persona được sử dụng (StudentHelper, TeacherAssistant, ParentGuide) |
| `createdAt`      | `DateTime` | Thời gian tạo                                                            |
| `updatedAt`      | `DateTime` | Thời gian cập nhật lần cuối                                              |
| `isActive`       | `bool`     | Trạng thái hoạt động (false = đã xóa)                                    |

---

### 3.2. Lấy lịch sử tin nhắn

**Endpoint:** `GET /api/assistant/conversations/{conversationId}/messages`

**Mô tả:** Lấy lịch sử tin nhắn của một cuộc trò chuyện cụ thể.

**Request:**

```http
GET /api/assistant/conversations/1/messages?limit=20
Authorization: Bearer <token>
```

**Query Parameters:**

| Parameter | Type  | Required | Default | Mô tả                            |
| --------- | ----- | -------- | ------- | -------------------------------- |
| `limit`   | `int` | ❌       | `20`    | Số lượng tin nhắn tối đa (1-100) |

**Response:**

```json
{
  "message": "Lấy lịch sử tin nhắn thành công",
  "statusCode": 200,
  "data": [
    {
      "messageId": 1,
      "content": "Tôi không hiểu về phương trình bậc hai",
      "role": "user",
      "createdAt": "2024-01-15T10:30:00Z",
      "metadata": null,
      "isRelevant": true
    },
    {
      "messageId": 2,
      "content": "Phương trình bậc hai có dạng ax² + bx + c = 0...",
      "role": "assistant",
      "createdAt": "2024-01-15T10:30:05Z",
      "metadata": "{\"relevantKnowledgeIds\":[1,2,3]}",
      "isRelevant": true
    }
  ]
}
```

**Response Schema:**

| Field        | Type       | Mô tả                                                   |
| ------------ | ---------- | ------------------------------------------------------- |
| `messageId`  | `int`      | ID của tin nhắn                                         |
| `content`    | `string`   | Nội dung tin nhắn                                       |
| `role`       | `string`   | Vai trò: `"user"` hoặc `"assistant"`                    |
| `createdAt`  | `DateTime` | Thời gian gửi                                           |
| `metadata`   | `string?`  | Metadata JSON (chứa relevantKnowledgeIds cho assistant) |
| `isRelevant` | `bool`     | Tin nhắn có liên quan đến IGCSE không                   |

---

### 3.3. Tạo cuộc trò chuyện mới

**Endpoint:** `POST /api/assistant/conversations`

**Mô tả:** Tạo một cuộc trò chuyện mới với trợ lý AI.

**Request:**

```http
POST /api/assistant/conversations
Authorization: Bearer <token>
Content-Type: application/json

{
  "initialMessage": "Xin chào, tôi cần hỗ trợ về bài học"
}
```

**Request Body:**

| Field            | Type      | Required | Mô tả                                |
| ---------------- | --------- | -------- | ------------------------------------ |
| `initialMessage` | `string?` | ❌       | Tin nhắn đầu tiên (tối đa 500 ký tự) |

**Response:**

```json
{
  "message": "Tạo cuộc trò chuyện thành công",
  "statusCode": 201,
  "data": {
    "conversationId": 1,
    "title": "Xin chào, tôi cần hỗ trợ về bài học",
    "personaType": null,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "isActive": true
  }
}
```

**Lưu ý:**

- `personaType` sẽ được set tự động khi gửi tin nhắn đầu tiên (dựa trên role của user)
- Nếu không có `initialMessage`, title sẽ là "Cuộc trò chuyện mới"

---

### 3.4. Xóa cuộc trò chuyện

**Endpoint:** `DELETE /api/assistant/conversations/{conversationId}`

**Mô tả:** Xóa một cuộc trò chuyện (soft delete - set `IsActive = false`).

**Request:**

```http
DELETE /api/assistant/conversations/1
Authorization: Bearer <token>
```

**Response:**

```json
{
  "message": "Xóa cuộc trò chuyện thành công",
  "statusCode": 200,
  "data": "Cuộc trò chuyện đã được xóa"
}
```

**Business Rules:**

- ✅ Chỉ có thể xóa cuộc trò chuyện của chính mình
- ✅ Xóa mềm (soft delete) - dữ liệu vẫn còn trong database
- ❌ Không thể xóa cuộc trò chuyện của user khác

---

### 3.5. Gửi tin nhắn (REST API)

**Endpoint:** `POST /api/assistant/conversations/{conversationId}/messages`

**Mô tả:** Gửi tin nhắn đến trợ lý AI qua REST API (không streaming). **Khuyến nghị sử dụng SignalR Hub** để có trải nghiệm tốt hơn với streaming.

**Request:**

```http
POST /api/assistant/conversations/1/messages
Authorization: Bearer <token>
Content-Type: application/json

"Tôi không hiểu về phương trình bậc hai"
```

**Request Body:**

- Type: `string` (raw JSON string)
- Nội dung tin nhắn

**Response:**

```json
{
  "message": "Gửi tin nhắn thành công",
  "statusCode": 200,
  "data": {
    "content": "Phương trình bậc hai có dạng ax² + bx + c = 0...",
    "isRelevant": true,
    "conversationId": "1",
    "relevantKnowledgeIds": [1, 2, 3]
  }
}
```

**Response Schema:**

| Field                  | Type     | Mô tả                                  |
| ---------------------- | -------- | -------------------------------------- |
| `content`              | `string` | Nội dung phản hồi từ AI                |
| `isRelevant`           | `bool`   | Câu hỏi có liên quan đến IGCSE không   |
| `conversationId`       | `string` | ID cuộc trò chuyện                     |
| `relevantKnowledgeIds` | `int[]?` | Danh sách ID kiến thức liên quan (RAG) |

**Lưu ý:**

- ⚠️ API này **KHÔNG** có streaming - phải đợi toàn bộ response
- ✅ **Khuyến nghị**: Sử dụng **SignalR Hub** để có streaming response

---

## 4. SignalR Real-time Communication

### 4.1. Tổng quan

**SignalR Hub** cung cấp real-time chat với các tính năng:

- ✅ **Streaming Response**: Nhận phản hồi từng phần (typing effect)
- ✅ **Typing Indicator**: Hiển thị khi AI đang trả lời
- ✅ **Error Handling**: Xử lý lỗi real-time
- ✅ **Group Management**: Quản lý nhiều cuộc trò chuyện

### 4.2. Hub URL

```
ws://localhost:5000/chatHub?access_token=<token>
```

### 4.3. CORS Configuration

**Quan trọng**: SignalR yêu cầu CORS được cấu hình đúng để hỗ trợ credentials.

**Backend Configuration** (`appsettings.json`):

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:3000",
      "http://localhost:5174",
      "https://localhost:5173"
    ]
  }
}
```

**Lưu ý**:

- ✅ Phải sử dụng **specific origins** (không được dùng wildcard `*`)
- ✅ Phải bật `AllowCredentials()` trong CORS policy
- ✅ Thêm tất cả frontend URLs vào `AllowedOrigins`
- ⚠️ Nếu gặp lỗi CORS, kiểm tra:
  - Frontend URL có trong `AllowedOrigins` không
  - CORS policy có `AllowCredentials()` không
  - SignalR client có gửi credentials không

**Lỗi thường gặp**:

```
Access to fetch at 'http://localhost:5121/chatHub/negotiate' from origin 'http://localhost:5173'
has been blocked by CORS policy: Response to preflight request doesn't pass access control check:
The value of the 'Access-Control-Allow-Origin' header in the response must not be the wildcard '*'
when the request's credentials mode is 'include'.
```

**Giải pháp**: Đảm bảo backend sử dụng specific origins thay vì `AllowAnyOrigin()`.

### 4.4. Authentication

Token có thể truyền qua:

1. **Query String**: `?access_token=<token>` (đã config trong Program.cs)
2. **Header**: `Authorization: Bearer <token>` (nếu client hỗ trợ)

### 4.5. Hub Methods

#### **4.5.1. SendMessage**

Gửi tin nhắn đến trợ lý AI và nhận streaming response.

**Method:** `SendMessage(string conversationId, string message)`

**Parameters:**

| Parameter        | Type     | Mô tả              |
| ---------------- | -------- | ------------------ |
| `conversationId` | `string` | ID cuộc trò chuyện |
| `message`        | `string` | Nội dung tin nhắn  |

**Client Events:**

| Event                 | Payload  | Mô tả                                            |
| --------------------- | -------- | ------------------------------------------------ |
| `ReceiveMessageChunk` | `string` | Nhận từng phần của response (streaming)          |
| `Typing`              | `bool`   | Trạng thái typing (true = đang gõ, false = dừng) |
| `Error`               | `string` | Thông báo lỗi                                    |

**Ví dụ (JavaScript/TypeScript):**

```typescript
// Kết nối
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/chatHub", {
    accessTokenFactory: () => token,
  })
  .withAutomaticReconnect()
  .build();

// Lắng nghe streaming response
connection.on("ReceiveMessageChunk", (chunk: string) => {
  // Append chunk to UI
  appendMessage(chunk);
});

// Lắng nghe typing indicator
connection.on("Typing", (isTyping: boolean) => {
  showTypingIndicator(isTyping);
});

// Lắng nghe lỗi
connection.on("Error", (error: string) => {
  showError(error);
});

// Gửi tin nhắn
await connection.invoke("SendMessage", "1", "Xin chào");
```

#### **4.5.2. JoinConversation**

Tham gia vào một cuộc trò chuyện (để nhận typing indicators).

**Method:** `JoinConversation(string conversationId)`

**Ví dụ:**

```typescript
await connection.invoke("JoinConversation", "1");
```

#### **4.5.3. LeaveConversation**

Rời khỏi cuộc trò chuyện.

**Method:** `LeaveConversation(string conversationId)`

**Ví dụ:**

```typescript
await connection.invoke("LeaveConversation", "1");
```

### 4.6. Flow xử lý

```
1. Client gửi: SendMessage("1", "Câu hỏi")
   ↓
2. Server lưu user message vào DB
   ↓
3. Server gửi: Typing(true)
   ↓
4. Server xử lý với AI (streaming)
   ↓
5. Server gửi từng chunk: ReceiveMessageChunk("chunk1"), ReceiveMessageChunk("chunk2"), ...
   ↓
6. Server lưu assistant response vào DB
   ↓
7. Server gửi: Typing(false)
```

---

## 5. Tích hợp Frontend

### 5.1. Cài đặt Dependencies

**React:**

```bash
npm install @microsoft/signalr
# hoặc
yarn add @microsoft/signalr
```

### 5.2. SignalR Client Setup

**File: `src/services/signalr/chatHubService.ts`**

```typescript
import * as signalR from "@microsoft/signalr";

class ChatHubService {
  private connection: signalR.HubConnection | null = null;

  constructor(private token: string) {}

  async connect(): Promise<void> {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.REACT_APP_API_URL}/chatHub`, {
        accessTokenFactory: () => this.token,
      })
      .withAutomaticReconnect()
      .build();

    // Register event handlers
    this.connection.on("ReceiveMessageChunk", (chunk: string) => {
      this.onMessageChunk?.(chunk);
    });

    this.connection.on("Typing", (isTyping: boolean) => {
      this.onTyping?.(isTyping);
    });

    this.connection.on("Error", (error: string) => {
      this.onError?.(error);
    });

    await this.connection.start();
  }

  async sendMessage(conversationId: string, message: string): Promise<void> {
    if (!this.connection) {
      throw new Error("Connection not established");
    }

    await this.connection.invoke("SendMessage", conversationId, message);
  }

  async joinConversation(conversationId: string): Promise<void> {
    if (!this.connection) {
      throw new Error("Connection not established");
    }

    await this.connection.invoke("JoinConversation", conversationId);
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  // Event handlers
  onMessageChunk?: (chunk: string) => void;
  onTyping?: (isTyping: boolean) => void;
  onError?: (error: string) => void;
}

export default ChatHubService;
```

### 5.3. React Hook

**File: `src/hooks/useChat.ts`**

```typescript
import { useState, useEffect, useCallback, useRef } from "react";
import ChatHubService from "../services/signalr/chatHubService";
import { getAuthToken } from "../utils/auth";

interface Message {
  messageId?: number;
  content: string;
  role: "user" | "assistant";
  createdAt?: string;
}

export const useChat = (conversationId: string | null) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const [isConnected, setIsConnected] = useState(false);
  const [currentMessage, setCurrentMessage] = useState("");
  const chatHubRef = useRef<ChatHubService | null>(null);
  const streamingMessageRef = useRef<string>("");

  useEffect(() => {
    const token = getAuthToken();
    if (!token || !conversationId) return;

    const chatHub = new ChatHubService(token);
    chatHubRef.current = chatHub;

    // Setup event handlers
    chatHub.onMessageChunk = (chunk: string) => {
      streamingMessageRef.current += chunk;
      setMessages((prev) => {
        const lastMessage = prev[prev.length - 1];
        if (lastMessage?.role === "assistant" && !lastMessage.messageId) {
          // Update streaming message
          return [
            ...prev.slice(0, -1),
            { ...lastMessage, content: streamingMessageRef.current },
          ];
        } else {
          // New assistant message
          return [...prev, { role: "assistant" as const, content: chunk }];
        }
      });
    };

    chatHub.onTyping = (typing: boolean) => {
      setIsTyping(typing);
    };

    chatHub.onError = (error: string) => {
      console.error("Chat error:", error);
    };

    // Connect
    chatHub.connect().then(() => {
      setIsConnected(true);
      if (conversationId) {
        chatHub.joinConversation(conversationId);
      }
    });

    return () => {
      chatHub.disconnect();
    };
  }, [conversationId]);

  const sendMessage = useCallback(
    async (message: string) => {
      if (!chatHubRef.current || !conversationId || !message.trim()) return;

      // Add user message to UI
      const userMessage: Message = {
        content: message,
        role: "user",
      };
      setMessages((prev) => [...prev, userMessage]);
      setCurrentMessage("");

      // Reset streaming message
      streamingMessageRef.current = "";

      // Add placeholder for assistant response
      setMessages((prev) => [...prev, { role: "assistant", content: "" }]);

      try {
        await chatHubRef.current.sendMessage(conversationId, message);
      } catch (error) {
        console.error("Failed to send message:", error);
        // Remove placeholder on error
        setMessages((prev) => prev.slice(0, -1));
      }
    },
    [conversationId]
  );

  return {
    messages,
    isTyping,
    isConnected,
    currentMessage,
    setCurrentMessage,
    sendMessage,
  };
};
```

### 5.4. React Component

**File: `src/components/Chat/ChatWindow.tsx`**

```typescript
import React from "react";
import { useChat } from "../../hooks/useChat";
import "./ChatWindow.css";

interface ChatWindowProps {
  conversationId: string;
}

const ChatWindow: React.FC<ChatWindowProps> = ({ conversationId }) => {
  const {
    messages,
    isTyping,
    isConnected,
    currentMessage,
    setCurrentMessage,
    sendMessage,
  } = useChat(conversationId);

  const messagesEndRef = React.useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  React.useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (currentMessage.trim()) {
      sendMessage(currentMessage);
    }
  };

  return (
    <div className="chat-window">
      <div className="chat-messages">
        {messages.map((message, index) => (
          <div
            key={index}
            className={`message ${
              message.role === "user" ? "user-message" : "assistant-message"
            }`}
          >
            <div className="message-content">
              {message.content || (isTyping && "...")}
            </div>
          </div>
        ))}
        {isTyping && (
          <div className="typing-indicator">
            <span></span>
            <span></span>
            <span></span>
          </div>
        )}
        <div ref={messagesEndRef} />
      </div>

      <form onSubmit={handleSubmit} className="chat-input-form">
        <input
          type="text"
          value={currentMessage}
          onChange={(e) => setCurrentMessage(e.target.value)}
          placeholder="Nhập câu hỏi của bạn..."
          disabled={!isConnected}
        />
        <button type="submit" disabled={!isConnected || !currentMessage.trim()}>
          Gửi
        </button>
      </form>

      {!isConnected && <div className="connection-status">Đang kết nối...</div>}
    </div>
  );
};

export default ChatWindow;
```

### 5.5. Environment Variables

**File: `.env`**

```env
REACT_APP_API_URL=http://localhost:5000
REACT_APP_SIGNALR_URL=http://localhost:5000
```

---

## 6. Business Rules

### 6.1. Relevance Filtering

Trợ lý **CHỈ** trả lời về:

- ✅ Nội dung liên quan đến IGCSE
- ✅ Khóa học, bài học, bài tập
- ✅ Học tập, phương pháp học
- ✅ Giảng dạy, phân tích dữ liệu (cho Teacher)
- ✅ Tiến độ, thành tích (cho Parent)

Trợ lý **KHÔNG** trả lời về:

- ❌ Tin tức, thời tiết, giải trí
- ❌ Chủ đề ngoài phạm vi IGCSE
- ❌ Câu hỏi không liên quan đến học tập

**Cơ chế**: RAG system chỉ trả về context có similarity > 0.7

### 6.2. Persona Selection

Persona được chọn tự động dựa trên **role** của user:

| Role      | Persona            | Tính cách                         |
| --------- | ------------------ | --------------------------------- |
| `Student` | `StudentHelper`    | Thân thiện, nhiệt tình, kiên nhẫn |
| `Teacher` | `TeacherAssistant` | Chuyên nghiệp, hiệu quả, hỗ trợ   |
| `Parent`  | `ParentGuide`      | Thông tin, hỗ trợ, rõ ràng        |

### 6.3. Context Awareness

Trợ lý tự động nhận biết:

- **Khóa học đang học**: Tập trung vào nội dung liên quan
- **Tiến độ học tập**: Gợi ý phù hợp với trình độ
- **Lịch sử trò chuyện**: Hiểu ngữ cảnh câu hỏi
- **Thành tích**: Động viên dựa trên kết quả

### 6.4. Conversation Management

- ✅ Mỗi user có thể có nhiều cuộc trò chuyện
- ✅ Chỉ có thể xóa cuộc trò chuyện của chính mình
- ✅ Xóa mềm (soft delete) - dữ liệu vẫn còn trong database
- ✅ Lịch sử tin nhắn được lưu vĩnh viễn

### 6.5. Rate Limiting

- ⚠️ Có thể có rate limiting để tránh abuse (tùy cấu hình server)
- ⚠️ Nên implement rate limiting ở client side

---

## 7. Ví dụ sử dụng

### 7.1. Tạo cuộc trò chuyện và gửi tin nhắn (REST API)

```typescript
// 1. Tạo cuộc trò chuyện
const createResponse = await fetch("/api/assistant/conversations", {
  method: "POST",
  headers: {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  },
  body: JSON.stringify({
    initialMessage: "Xin chào, tôi cần hỗ trợ về bài học",
  }),
});

const conversation = await createResponse.json();
const conversationId = conversation.data.conversationId;

// 2. Gửi tin nhắn
const messageResponse = await fetch(
  `/api/assistant/conversations/${conversationId}/messages`,
  {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify("Tôi không hiểu về phương trình bậc hai"),
  }
);

const response = await messageResponse.json();
console.log(response.data.content); // Phản hồi từ AI
```

### 7.2. Sử dụng SignalR (Khuyến nghị)

```typescript
// 1. Kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/chatHub", {
    accessTokenFactory: () => token,
  })
  .withAutomaticReconnect()
  .build();

await connection.start();

// 2. Tham gia cuộc trò chuyện
await connection.invoke("JoinConversation", conversationId);

// 3. Lắng nghe streaming response
connection.on("ReceiveMessageChunk", (chunk: string) => {
  // Append chunk to UI
  appendToMessage(chunk);
});

// 4. Lắng nghe typing indicator
connection.on("Typing", (isTyping: boolean) => {
  showTypingIndicator(isTyping);
});

// 5. Gửi tin nhắn
await connection.invoke("SendMessage", conversationId, "Câu hỏi của tôi");
```

### 7.3. Lấy lịch sử tin nhắn

```typescript
const response = await fetch(
  `/api/assistant/conversations/${conversationId}/messages?limit=50`,
  {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  }
);

const data = await response.json();
const messages = data.data; // Array of messages
```

---

## 8. Error Handling

### 8.1. HTTP Status Codes

| Status Code | Mô tả                                            |
| ----------- | ------------------------------------------------ |
| `200`       | Thành công                                       |
| `201`       | Tạo thành công                                   |
| `400`       | Bad Request - Dữ liệu không hợp lệ               |
| `401`       | Unauthorized - Chưa đăng nhập hoặc token hết hạn |
| `404`       | Not Found - Không tìm thấy resource              |
| `500`       | Internal Server Error - Lỗi server               |

### 8.2. Error Response Format

```json
{
  "message": "Error message",
  "statusCode": 400,
  "data": null
}
```

### 8.3. SignalR Error Handling

```typescript
connection.on("Error", (error: string) => {
  // Handle error
  console.error("Chat error:", error);
  showErrorNotification(error);
});

// Connection errors
connection.onclose((error) => {
  if (error) {
    console.error("Connection closed with error:", error);
    // Attempt to reconnect
  }
});
```

### 8.4. Common Errors

| Error                            | Nguyên nhân                                           | Giải pháp                                                        |
| -------------------------------- | ----------------------------------------------------- | ---------------------------------------------------------------- |
| `Unauthorized`                   | Token không hợp lệ hoặc hết hạn                       | Refresh token hoặc đăng nhập lại                                 |
| `Không tìm thấy cuộc trò chuyện` | ConversationId không tồn tại hoặc không thuộc về user | Kiểm tra conversationId                                          |
| `Connection failed`              | SignalR không kết nối được                            | Kiểm tra network, CORS, hoặc server status                       |
| `CORS policy error`              | Backend dùng `AllowAnyOrigin()` với credentials       | Sửa CORS policy để dùng specific origins và `AllowCredentials()` |

---

## 9. Tóm tắt

### 9.1. API Summary

| Endpoint                                     | Method  | Mô tả                         | Streaming |
| -------------------------------------------- | ------- | ----------------------------- | --------- |
| `/api/assistant/conversations`               | GET     | Lấy danh sách cuộc trò chuyện | ❌        |
| `/api/assistant/conversations/{id}/messages` | GET     | Lấy lịch sử tin nhắn          | ❌        |
| `/api/assistant/conversations`               | POST    | Tạo cuộc trò chuyện mới       | ❌        |
| `/api/assistant/conversations/{id}`          | DELETE  | Xóa cuộc trò chuyện           | ❌        |
| `/api/assistant/conversations/{id}/messages` | POST    | Gửi tin nhắn (REST)           | ❌        |
| `/chatHub`                                   | SignalR | Real-time chat với streaming  | ✅        |

### 9.2. Best Practices

1. ✅ **Sử dụng SignalR** cho real-time chat (khuyến nghị)
2. ✅ **Lưu conversationId** sau khi tạo cuộc trò chuyện
3. ✅ **Load lịch sử** khi mở cuộc trò chuyện
4. ✅ **Xử lý reconnection** khi mất kết nối SignalR
5. ✅ **Hiển thị typing indicator** để cải thiện UX
6. ✅ **Validate input** trước khi gửi tin nhắn
7. ✅ **Error handling** cho tất cả API calls

### 9.3. Performance Tips

- **Pagination**: Sử dụng `limit` parameter để giới hạn số lượng tin nhắn
- **Caching**: Cache conversation list và recent messages
- **Lazy Loading**: Load messages khi cần (scroll to top)
- **Debouncing**: Debounce typing indicators

---

**Tài liệu này sẽ được cập nhật thường xuyên khi có thay đổi trong hệ thống.**

**Liên hệ**: Backend Team - IGCSE Platform
