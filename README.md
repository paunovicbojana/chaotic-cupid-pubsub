# ChaoticCupid - Pub/Sub Matchmaking with SignalR

A real-time matchmaking application built with ASP.NET Core and SignalR. Cupid runs as a background worker, periodically finding the best match for each connected user and delivering love letters over a persistent WebSocket connection. The matching algorithm weighs same-city proximity, age similarity, and a cryptographically random factor to keep things unpredictable.

---

## How It Works

1. Clients connect to the SignalR hub and register their profile (username, city, age, gender, preferences).
2. The **CupidWorker** runs every 10 seconds, iterates over all connected participants, and for each one finds the best available match using a scoring system.
3. A love letter is sent to the recipient via `ReceiveLetter`. The recipient must confirm receipt before they can receive the next letter.
4. The sender's phone number is included in the letter only if the message is positive - it is withheld for rejection messages.
5. Users can block others at any time with the `/block <username>` command. Blocked users are skipped during matching in both directions. Users cannot block themselves.

### Matching Score

| Criterion                    | Points |
| ---------------------------- | ------ |
| Same city                    | +30    |
| Age within 2 years           | +20    |
| Cryptographic random (0-100) | +0-100 |

Only mutually compatible genders are considered (both users must have each other's gender in their `InterestedIn` list). Blocked users are excluded on both sides.

---

## Project Structure

```
chaotic-cupid-pubsub/
├── ChaoticCupid/          # ASP.NET Core server
│   ├── Hubs/
│   │   └── CupidHub.cs        # SignalR hub - registration, blocking, confirmation
│   ├── Models/
│   │   ├── Person.cs          # Server-side user model (includes ConnectionId, block list)
│   │   └── Letter.cs
│   ├── Services/
│   │   ├── IMatchmakingService.cs
│   │   └── MatchmakingService.cs   # Thread-safe in-memory matchmaking logic
│   ├── Workers/
│   │   └── CupidWorker.cs     # BackgroundService - sends letters every minute
│   └── Program.cs
├── Client/                # Interactive console client
├── TestClient/            # Automated test client (5 pre-defined users)
├── Shared/                # DTOs shared between server and clients
│   └── Models/
│       ├── PersonDTO.cs
│       ├── LetterDTO.cs
│       └── Gender.cs
└── Tests/
```

---

## Getting Started

**Prerequisites:** .NET 8 SDK

### Run the server

```bash
cd ChaoticCupid
dotnet run
```

The server starts at `https://localhost:7250` and `http://localhost:5188`.

### Run the interactive client

```bash
cd Client
dotnet run
```

You will be prompted to enter your username, city, age, phone number, gender, and who you are interested in. Once registered, the client waits for letters. Incoming letters are displayed in the console and must be confirmed by pressing Enter.

**Available commands:**

| Command             | Description                                               |
| ------------------- | --------------------------------------------------------- |
| `/block <username>` | Block a user - they will no longer appear in your matches |
| `Enter`             | Confirm receipt of the current letter                     |

### Run the automated test client

```bash
cd TestClient
dotnet run
```

Registers 5 pre-defined users simultaneously, waits for Cupid to send letters, then runs block tests (Ana blocks Boban & Ana blocks herself). Press Enter to disconnect all clients.

---

## Hub Methods (Server API)

| Method             | Parameters                                         | Description                               |
| ------------------ | -------------------------------------------------- | ----------------------------------------- |
| `InitSinglePerson` | username, city, years, phone, gender, interestedIn | Register a new participant                |
| `ConfirmLetter`    | -                                                  | Acknowledge receipt of the current letter |
| `BlockUser`        | usernameToBlock                                    | Block a user by username                  |

## Client Events (Server → Client)

| Event             | Payload          | Description                                               |
| ----------------- | ---------------- | --------------------------------------------------------- |
| `InitConfirmed`   | message (string) | Registration successful                                   |
| `InitFailed`      | message (string) | Username already taken                                    |
| `ReceiveLetter`   | `LetterDTO`      | A new love letter arrived                                 |
| `UserBlocked`     | message (string) | Confirmation that a user was successfully blocked         |
| `UserBlockFailed` | message (string) | Blocking failed (e.g. user attempted to block themselves) |

---

## Notes

- All participant state is held in memory - restarting the server clears all registered users.
- `MatchmakingService` is registered as a singleton and uses a lock for thread safety.
- The `CupidWorker` skips users that are currently waiting for a letter confirmation (`IsWaitingConfirmation = true`), ensuring each user processes one letter at a time.
