+-------------------------+ HTTPS +------------------------+ DB Calls +-----------------+
| .NET MAUI Blazor Hybrid | <-------------> | ASP.NET Core Minimal API | <---------------> | SQL Server |
| (Android, Windows) | (API Calls) | (Business Logic, Auth) | (Stored Procs) | (Encrypted Data)|
| - UI (Blazor/MudBlazor) | | - SignalR Hub | | |
| - Key Gen & Crypto | <-------------> | | | |
| - SignalR Client | (SignalR WS) | | | |
+-------------------------+ +------------------------+ +-----------------+

```*Figure 1: High-Level System Architecture*

**3. Technology Stack**

*   **Frontend:** .NET MAUI, Blazor Hybrid, MudBlazor (for UI components)
*   **Backend:** ASP.NET Core 8 (Minimal API)
*   **Real-time:** ASP.NET Core SignalR
*   **Database:** Microsoft SQL Server
*   **Database Interaction:** Stored Procedures, ADO.NET or Dapper (via Repository Pattern)
*   **Authentication:** Token-based (JWT assumed) via ASP.NET Core Identity or custom implementation.
*   **Cryptography:** .NET `System.Security.Cryptography` libraries (RSA, AES).
*   **Language:** C#
*   **Development Environment:** Visual Studio 2022

**4. Core Functionality**

*   **User Registration & Login:** Users can register with unique credentials. During registration and subsequent logins, the client application generates a new RSA public/private key pair *locally* on the device. The public key is sent to the backend during login/registration, while the private key remains solely on the client device for the duration of the session. Authentication is token-based, requiring a valid token for most API interactions.
*   **Contact Management:** Users can search for other registered users and view their list of contacts.
*   **Conversation Initiation:** Users can select a contact to open a one-on-one conversation view. The backend facilitates retrieving conversation history (metadata and encrypted messages).
*   **Messaging:** Users can type and send messages within a conversation. Messages undergo the end-to-end encryption process before being sent to the backend. Received messages are decrypted locally.
*   **Real-time Updates:** New messages received while a conversation is open are pushed to the client in real-time using SignalR, decrypted, and displayed without requiring a manual refresh.

**5. Security Implementation: End-to-End Encryption**

The core security feature is end-to-end encryption, ensuring message confidentiality between the sender and recipient. A hybrid encryption approach is utilized, combining the strengths of asymmetric (RSA) and symmetric (AES) cryptography:

*   **Key Management:**
    *   Each user generates a unique 2048-bit RSA key pair (public and private) on their device upon login or registration.
    *   The **Public Key** is shared with the backend and made available to other users (e.g., when opening a conversation) to allow them to encrypt messages *for* the key owner.
    *   The **Private Key** NEVER leaves the user's device during the session. It is essential for decrypting messages received by the user. It is *not* stored on the server.
    *   Crucially, a *new* RSA key pair is generated for *each login session*.

*   **Encryption Process (Sender's Device):**
    1.  **Generate Symmetric Key:** For each message, generate a cryptographically secure 256-bit AES key and a unique Initialization Vector (IV).
    2.  **Encrypt Content:** Encrypt the plaintext message content using AES (CBC or GCM mode recommended, assuming CBC based on IV presence) with the generated AES key and IV.
    3.  **Retrieve Recipient's Public Key:** Obtain the recipient's RSA public key (fetched from the backend API).
    4.  **Encrypt Symmetric Key:** Encrypt the generated AES key using the recipient's RSA public key with OAEP padding (RSAEncryptionPadding.OaepSHA256).
    5.  **Transmit:** Send the following to the backend API via HTTPS:
        *   Encrypted message content (from step 2).
        *   Encrypted AES key (from step 4).
        *   The unique IV (from step 1).
        *   Other metadata (Conversation ID, Sender ID, Timestamp).

*   **Data at Rest (Server/Database):** The backend receives and stores the encrypted content, the RSA-encrypted AES key, and the IV. The server *cannot* decrypt the message content or the AES key.

*   **Decryption Process (Recipient's Device):**
    1.  **Receive Data:** Receive the encrypted payload (encrypted content, encrypted AES key, IV) from the backend (either via initial fetch or SignalR push).
    2.  **Decrypt Symmetric Key:** Use the recipient's **RSA Private Key** (held locally) to decrypt the encrypted AES key (using RSA with OAEP padding).
    3.  **Decrypt Content:** Use the now-decrypted AES key and the received IV to decrypt the message content using AES.
    4.  **Display:** Show the decrypted plaintext message to the user.

This process ensures that message content is only ever in plaintext on the sender's and recipient's devices during composition and viewing, respectively.

**6. Real-time Communication (SignalR)**

To avoid constant polling and provide an instant messaging experience, SignalR is employed:

*   **Hub:** An ASP.NET Core SignalR Hub (`ChatHub`) is defined on the server.
*   **Connection:** The client establishes a persistent WebSocket connection (with fallbacks) to the `/chathub` endpoint upon entering a conversation view. Authentication tokens are used to secure the connection.
*   **Groups:** To ensure messages are only sent to relevant clients, SignalR Groups are used. When a client opens a conversation, it requests to join a group specific to that conversation (e.g., `conversation-{conversationID}`).
*   **Broadcasting:** When the backend API's `SendMessage` endpoint successfully saves a message, it uses the injected `IHubContext<ChatHub>` to send the (still encrypted) message payload specifically to the SignalR group associated with the conversation ID (`hubContext.Clients.Group(...).SendAsync("ReceiveMessage", message)`).
*   **Receiving:** The client listens for the `ReceiveMessage` event. Upon receiving a message, it performs the decryption process described above and updates the UI dynamically using `InvokeAsync(StateHasChanged)` to handle thread marshalling.
*   **Lifecycle:** The client joins the group when the conversation is initialized and leaves the group when the component is disposed (`DisposeAsync`) to manage resources efficiently.

**7. Database Interaction**

Microsoft SQL Server serves as the data persistence layer. The backend interacts with the database primarily through stored procedures, accessed via a repository pattern. This encapsulates data access logic. The database stores:

*   User information (IDs, usernames, hashed passwords, public keys).
*   Contact relationships.
*   Conversation metadata (IDs, participants).
*   Messages (ConversationID, SenderID, Timestamp, IV, Encrypted AES Key, Encrypted Content).

**8. Platform Support**

The application is developed using .NET MAUI Blazor Hybrid, currently supporting:

*   **Android:** Tested and functional.
*   **Windows:** Tested and functional.
*   **macOS:** Untested due to development environment constraints.
*   **Browser (WebAssembly):** Explicitly **not supported**. This decision was made due to limitations in the standard .NET WebAssembly runtime regarding the `System.Security.Cryptography.RSA.Create()` method needed for client-side key generation. While JavaScript interop could be used as a workaround, it was deemed out of scope for this project phase due to time constraints.

**9. Limitations and Challenges**

*   **Message History Decryption:** The most significant limitation stems directly from the security decision to generate ephemeral RSA keys on each login and not store private keys server-side. While enhancing security against server compromise, it means a user **cannot decrypt messages** (sent or received) from sessions prior to their current login, as they no longer possess the necessary RSA private key for that session. This impacts the user experience regarding viewing historical conversations after logging out and back in.
*   **No Browser Support:** As detailed above, the inability to easily perform client-side RSA key generation in .NET WASM without JS interop prevents browser deployment.
*   **Key Management:** The ephemeral nature of keys simplifies server security but places the onus of potential future key backup/restore entirely on the client-side (which is not implemented), impacting message history persistence.
*   **Untested Platforms:** Lack of testing on macOS means potential platform-specific issues are unknown.

**10. Future Work**

*   **Address History Limitation:** Investigate secure client-side key backup/restore mechanisms (e.g., encrypting the private key with a user-derived password/phrase and storing it locally or in a user-controlled cloud storage) to allow decryption of older messages. This adds significant complexity.
*   **Browser Support:** Implement the JavaScript interop workaround for RSA key generation if browser support becomes a requirement.
*   **macOS Testing & Support:** Test and resolve any issues for macOS deployment.
*   **Enhanced Real-time Features:** Implement read receipts, typing indicators, and online presence using SignalR.
*   **Group Chats:** Extend the architecture to support multi-user group conversations.
*   **Error Handling & UX:** Improve robustness and provide clearer user feedback for connection issues, decryption failures, etc.

**11. Conclusion**

This project successfully demonstrates the implementation of a secure messaging application using .NET MAUI Blazor Hybrid and ASP.NET Core Minimal API, featuring end-to-end encryption with a hybrid RSA/AES approach. The system prioritizes confidentiality by performing key generation and decryption client-side and ensuring the server only handles encrypted data. Real-time communication is effectively handled by SignalR. The primary limitation regarding message history decryption is a direct consequence of the security design choice to use ephemeral, client-only private keys, representing a trade-off between maximum server-side security and user experience for message persistence.

---
```
