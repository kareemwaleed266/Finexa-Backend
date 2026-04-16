# 💰 Finexa – Personal Finance Management System with AI Integration

> A comprehensive personal finance management system that combines traditional financial tracking with AI-powered input processing. Built as a graduation project at Helwan University, Faculty of Computers and Artificial Intelligence.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=JSON%20web%20tokens&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)



## 📋 Table of Contents

- [About The Project](#about-the-project)
- [System Architecture](#system-architecture)
- [Core Features](#core-features)
- [AI Integration](#ai-integration)
- [Tech Stack](#tech-stack)
- [Database Schema](#database-schema)
- [API Endpoints](#api-endpoints)
- [Security](#security)
- [License](#license)

---

## 🎯 About The Project

Finexa is a **Personal Finance Management System** that helps users track their income, expenses, saving goals, and financial analytics — all enhanced with **AI-powered features** including:

- 🎤 **Speech-to-Text** transaction input
- 🧾 **OCR** receipt scanning
- 💬 **Natural Language** transaction parsing via chat agent
- 📊 **Smart Dashboard** with financial analytics

### 🧠 Core Design Philosophy
AI assists, never decides.
Backend validates everything.
User always confirms before saving.

---

## ⭐ Core Features

### 💰 Transaction Management
- Create, update, and delete financial transactions
- Support for **Income** and **Expense** types
- Multiple input sources: **Manual, Chat, OCR, Speech**
- Advanced **filtering, pagination, and sorting**

### 🎯 Goal Management
- Create saving goals with target amounts
- Contribute to goals (creates expense transactions)
- Cancel goals with automatic refund (creates income transactions)
- Track goal progress and status

### 📊 Dashboard & Analytics
- Real-time financial overview
- Total income, expenses, and balance
- Category-wise spending breakdown
- Balance rebuild capability from transaction history

### 💳 Smart Balance System
- Redis-cached balance for fast access
- Balance = cache, Transactions = source of truth
- Full rebuild capability at any time

### 📂 Category Management
- Default system categories
- Custom user-created categories
- Separate categories for Income and Expense

---

## 🤖 AI Integration

### Design Principle
AI → Preview → User Confirms → Backend Validates → Save


> **AI never saves data directly.** All AI outputs go through a preview step
> where the user must confirm before any data is persisted.

---

### 🎤 Speech-to-Text Flow
Audio File → AI Speech Service → Text → Parse → Preview → Confirm → Save


| Step | Handler |
|------|---------|
| Audio → Text | AI Service |
| Text → Transaction Preview | ParseTransactionAppService |
| Preview → Validate | ConfirmTransactionService |
| Validate → Save | TransactionService |

---

### 🧾 OCR Flow
Receipt Image → AI OCR Service → Extracted Data → Preview → Confirm → Save



| Step | Handler |
|------|---------|
| Image → Data | AI OCR Service |
| Data → Transaction Preview | ParseTransactionAppService |
| Preview → Validate | ConfirmTransactionService |
| Validate → Save | TransactionService |

---

### 💬 Chat Flow
User Message → AI Chat Service → Parsed Transaction → Preview → Confirm → Save


| Step | Handler |
|------|---------|
| Text → Parse | AI Chat Service |
| Parse → Transaction Preview | ParseTransactionAppService |
| Preview → Validate | ConfirmTransactionService |
| Validate → Save | TransactionService |

---

### 💬 Chat Module Features

- Session management (create, list, delete)
- Message history with pagination
- AI-powered conversation summaries
- Smart memory management (summarize old messages, keep recent)

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend Framework** | ASP.NET Core 6+ |
| **Language** | C# |
| **Database** | SQL Server |
| **ORM** | Entity Framework Core (Code-First) |
| **Authentication** | ASP.NET Identity + JWT |
| **Architecture** | Clean Architecture |
| **API Documentation** | Swagger / Swashbuckle |
| **Deployment** | MonsterASP (HTTPS) |
| **Version Control** | Git & GitHub |
---

## 🔌 API Endpoints

### 🔐 Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login and get JWT token |

### 👤 User
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/user/profile` | Get user profile |
| PUT | `/api/user/profile` | Update profile |
| POST | `/api/user/upload-image` | Upload profile image |
| PUT | `/api/user/change-password` | Change password |
| DELETE | `/api/user/account` | Delete account |

### 💰 Transactions
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/transactions` | Get all transactions (filtered) |
| POST | `/api/transactions` | Create transaction |
| PUT | `/api/transactions/{id}` | Update transaction |
| DELETE | `/api/transactions/{id}` | Delete transaction |

### 🎯 Goals
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/goals` | Get all goals |
| POST | `/api/goals` | Create goal |
| POST | `/api/goals/{id}/contribute` | Contribute to goal |
| POST | `/api/goals/{id}/cancel` | Cancel goal |
| POST | `/api/goals/{id}/refund` | Refund goal |

### 📊 Dashboard
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard` | Get dashboard analytics |
| POST | `/api/dashboard/rebuild` | Rebuild balance from transactions |

### 📂 Categories
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get all categories |
| POST | `/api/categories` | Create custom category |

### 🤖 AI
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/ai/voice-to-text` | Convert audio to text |
| POST | `/api/ai/parse-transaction` | Parse text to transaction preview |
| POST | `/api/ai/ocr` | Extract data from receipt image |

### 💬 Chat
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/chat/sessions` | Create chat session |
| GET | `/api/chat/sessions` | Get all sessions |
| POST | `/api/chat/sessions/{id}/messages` | Send message |
| GET | `/api/chat/sessions/{id}/messages` | Get message history |

---

## 🔐 Security

### Security Rules
- ✅ No sensitive data stored in JWT tokens
- ✅ All endpoints require authentication (except auth routes)
- ✅ Role-based authorization
- ✅ Category ownership validation
- ✅ AI outputs always require user confirmation
- ✅ Input validation on all endpoints
- ✅ HTTPS enforced

### JWT Configuration
```json
{
  "Claims": ["sub (UserId)", "email", "username", "role", "jti", "iat"],
  "Expiration": "Short-lived tokens",
  "Secret": "Strong secret key (256-bit minimum)"
}
🧠 Key Design Decisions
1. Single Source of Truth
Transactions are the single source of truth.
Balance is just a cached calculation that can be rebuilt at any time.

2. Single Save Logic
All transaction creation flows through TransactionService —
whether the input comes from manual entry, speech, OCR, or chat.

3. AI Safety
AI never writes to the database directly.
Every AI suggestion goes through:
Preview → User Confirmation → Backend Validation → Save

4. Clean Architecture
Strict dependency rules: outer layers depend on inner layers, never the reverse.
Domain has zero external dependencies.

📄 License
This project is part of the graduation requirements at Helwan University, Faculty of Computers and Artificial Intelligence — Class of 2026.
