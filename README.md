# 💰 Finexa – Personal Finance Management System with AI Integration

> A comprehensive personal finance management system that combines traditional financial tracking with AI-powered input processing. Built as a graduation project at Helwan University, Faculty of Computers and Artificial Intelligence.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=JSON%20web%20tokens&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

---

## 📋 Table of Contents

- [About The Project](#about-the-project)
- [System Architecture](#system-architecture)
- [Core Features](#core-features)
- [AI Integration](#ai-integration)
- [Tech Stack](#tech-stack)
- [Database Schema](#database-schema)
- [API Endpoints](#api-endpoints)
- [Security](#security)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Screenshots](#screenshots)
- [Team](#team)
- [License](#license)

---

## 🎯 About The Project

Finexa is a **Personal Finance Management System** that helps users track their income, expenses, saving goals, and financial analytics — all enhanced with **AI-powered features** including:

- 🎤 **Speech-to-Text** transaction input
- 🧾 **OCR** receipt scanning
- 💬 **Natural Language** transaction parsing via chat
- 📊 **Smart Dashboard** with financial analytics

### 🧠 Core Design Philosophy
AI assists, never decides.
Backend validates everything.
User always confirms before saving.

text

---

## 🏗️ System Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:
┌─────────────────────────────────────────┐
│ API Layer │
│ (Controllers & Endpoints) │
├─────────────────────────────────────────┤
│ Application Layer │
│ (Services & Business Logic) │
├──────────────────┬──────────────────────┤
│ Infrastructure │ AI Integration │
│ (DB, Identity, │ (Speech, OCR, │
│ Repos, JWT) │ Chat, Parse) │
├──────────────────┴──────────────────────┤
│ Domain Layer │
│ (Entities, Enums, Rules) │
└─────────────────────────────────────────┘

text

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

text

> **AI never saves data directly.** All AI outputs go through a preview step
> where the user must confirm before any data is persisted.

---

### 🎤 Speech-to-Text Flow
Audio File → AI Speech Service → Text → Parse → Preview → Confirm → Save

text

| Step | Handler |
|------|---------|
| Audio → Text | AI Service |
| Text → Transaction Preview | ParseTransactionAppService |
| Preview → Validate | ConfirmTransactionService |
| Validate → Save | TransactionService |

---

### 🧾 OCR Flow
Receipt Image → AI OCR Service → Extracted Data → Preview → Confirm → Save

text

| Step | Handler |
|------|---------|
| Image → Data | AI OCR Service |
| Data → Transaction Preview | ParseTransactionAppService |
| Preview → Validate | ConfirmTransactionService |
| Validate → Save | TransactionService |

---

### 💬 Chat Flow
User Message → AI Chat Service → Parsed Transaction → Preview → Confirm → Save

text

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
| **Caching** | Redis |
| **Authentication** | ASP.NET Identity + JWT |
| **Architecture** | Clean Architecture |
| **API Documentation** | Swagger / Swashbuckle |
| **Deployment** | MonsterASP (HTTPS) |
| **Version Control** | Git & GitHub |

---

## 🗄️ Database Schema

### Core Entities
┌──────────────┐ ┌──────────────┐
│ AppUser │────<│ Transaction │
│ │ │ │
│ Id │ │ Id │
│ Email │ │ Amount │
│ UserName │ │ Type │
│ FirstName │ │ CategoryId │
│ LastName │ │ Notes │
└──────┬───────┘ │ OccurredAt │
│ │ Source │
│ │ GoalId? │
│ └──────────────┘
│
├────────<┌──────────────┐
│ │ Goal │
│ │ │
│ │ Id │
│ │ Title │
│ │ TargetAmount │
│ │ CurrentAmount│
│ │ Status │
│ └──────────────┘
│
├────────<┌──────────────┐
│ │ UserBalance │
│ │ │
│ │ TotalIncome │
│ │ TotalExpense │
│ │ TotalBalance │
│ └──────────────┘
│
└────────<┌──────────────┐
│ Category │
│ │
│ Id │
│ Name │
│ Type │
│ IsDefault │
└──────────────┘

text

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

### JWT Configuration
```json
{
  "Claims": ["sub (UserId)", "email", "username", "role", "jti", "iat"],
  "Expiration": "Short-lived tokens",
  "Secret": "Strong secret key (256-bit minimum)"
}
Security Rules
✅ No sensitive data stored in JWT tokens
✅ All endpoints require authentication (except auth routes)
✅ Role-based authorization
✅ Category ownership validation
✅ AI outputs always require user confirmation
✅ Input validation on all endpoints
✅ HTTPS enforced
🚀 Getting Started
Prerequisites
.NET 6+ SDK
SQL Server
Redis Server
Git
Installation
Clone the repository
bash
git clone https://github.com/YOUR_USERNAME/finexa.git
cd finexa
Update connection strings in appsettings.json
json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string",
    "Redis": "Your Redis connection string"
  },
  "JWT": {
    "SecretKey": "Your secret key",
    "Issuer": "Finexa",
    "Audience": "FinexaUsers",
    "ExpirationInMinutes": 60
  }
}
Apply database migrations
bash
dotnet ef database update
Run the application
bash
dotnet run
Open Swagger
text
https://localhost:xxxx/swagger
📁 Project Structure
text
Finexa/
├── src/
│   ├── Finexa.Domain/              # Entities, Enums, Core Rules
│   │   ├── Entities/
│   │   │   ├── AppUser.cs
│   │   │   ├── Transaction.cs
│   │   │   ├── Goal.cs
│   │   │   ├── UserBalance.cs
│   │   │   └── Category.cs
│   │   └── Enums/
│   │       ├── TransactionType.cs
│   │       ├── TransactionSource.cs
│   │       └── GoalStatus.cs
│   │
│   ├── Finexa.Application/         # Business Logic & DTOs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── TransactionService.cs
│   │   │   ├── GoalService.cs
│   │   │   ├── DashboardService.cs
│   │   │   ├── CategoryService.cs
│   │   │   └── UserService.cs
│   │   ├── AI/
│   │   │   ├── SpeechAppService.cs
│   │   │   ├── OcrAppService.cs
│   │   │   ├── ParseTransactionAppService.cs
│   │   │   └── ConfirmTransactionService.cs
│   │   └── DTOs/
│   │
│   ├── Finexa.Infrastructure/      # DB, Identity, Repos
│   │   ├── Data/
│   │   │   └── FinexaDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── GenericRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Identity/
│   │   │   └── JwtTokenGenerator.cs
│   │   └── Seeders/
│   │
│   └── Finexa.API/                 # Controllers & Configuration
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── TransactionController.cs
│       │   ├── GoalController.cs
│       │   ├── DashboardController.cs
│       │   ├── CategoryController.cs
│       │   ├── AIController.cs
│       │   └── ChatController.cs
│       ├── Program.cs
│       └── appsettings.json
