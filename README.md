# 🌐 Portfolio Website

A full-stack personal portfolio website showcasing projects, skills, blog posts, and professional experience. Built with Angular and ASP.NET Core, featuring a comprehensive content management system with authentication, media management, and analytics.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-20.3-DD0031?logo=angular)](https://angular.io/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Future Enhancements](#-future-enhancements)
- [Contributing](#-contributing)
- [Contact](#-contact)

---

## ✨ Features

### 🎨 Frontend
- **Modern Angular Application** - Built with Angular 20+ and Server-Side Rendering (SSR)
- **Responsive Design** - Mobile-first approach with Angular Material components
- **Rich User Experience** - Interactive project showcase, blog with likes, and contact forms
- **PDF Generation** - Export capabilities using jsPDF and html2canvas
- **Real-time Updates** - Reactive forms and state management with RxJS
- **Authentication** - JWT-based authentication with secure token management

### 🔧 Backend
- **RESTful API** - Built with ASP.NET Core 8.0
- **Clean Architecture** - Layered design with separation of concerns
- **Entity Framework Core** - PostgreSQL database with code-first migrations
- **Azure Blob Storage** - Cloud-based media storage and management
- **Email Integration** - SMTP/IMAP support for contact messages and notifications
- **Analytics** - Page view tracking and visitor analytics
- **Blog System** - Full-featured blog with posts, likes, and status management
- **Authentication & Authorization** - JWT tokens with refresh token support
- **Validation** - FluentValidation for comprehensive request validation

### 📊 Content Management
- Projects portfolio with images, tags, and technologies
- Blog posts with draft/published status
- Work experience and education history
- Skills and certifications showcase
- Testimonials and social links
- Newsletter subscriber management
- Site settings and customizable content

---

## 🛠 Tech Stack

### Frontend
| Technology | Version | Purpose |
|------------|---------|---------|
| Angular | 20.3.x | Frontend framework |
| TypeScript | 5.9.x | Type-safe development |
| Angular Material | 20.2.x | UI component library |
| RxJS | 7.8.x | Reactive programming |
| Chart.js | 4.5.x | Data visualization |
| JWT-Decode | 4.0.x | Token handling |
| FontAwesome | 7.1.x | Icons |

### Backend
| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Backend framework |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 9.0.x | ORM |
| PostgreSQL | - | Database |
| AutoMapper | 12.0.x | Object mapping |
| FluentValidation | 11.3.x | Validation |
| BCrypt.Net | 4.0.x | Password hashing |
| Azure Storage Blobs | - | Media storage |
| Swashbuckle | 9.0.x | API documentation |

---

## 🏗 Architecture

### Backend Structure
```
PortfolioSolution/
├── Portfolio.Models/          # DTOs, Requests, Responses, Enums
│   ├── Configuration/         # Settings models
│   ├── Requests/             # API request models
│   ├── Responses/            # API response models
│   └── SearchObjects/        # Search and filter models
├── Portfolio.Services/        # Business logic layer
│   ├── Database/             # EF Core DbContext
│   ├── Services/             # Service implementations
│   ├── Interfaces/           # Service contracts
│   ├── Migrations/           # Database migrations
│   └── Mapping/              # AutoMapper profiles
└── PortfolioApi/             # Web API layer
    ├── Controllers/          # API endpoints
    ├── Middleware/           # Custom middleware
    ├── Extensions/           # Service extensions
    └── Filter/               # Action filters
```

### Frontend Structure
```
portfolio-frontend/
├── src/
│   ├── app/
│   │   ├── components/       # Feature components
│   │   ├── guards/           # Route guards
│   │   ├── interceptors/     # HTTP interceptors
│   │   ├── models/           # TypeScript models
│   │   ├── pipes/            # Custom pipes
│   │   ├── services/         # API services
│   │   └── shared/           # Shared components
│   └── environments/         # Environment configs
```

---

## 🚀 Getting Started

### Prerequisites
- [Node.js](https://nodejs.org/) (v18 or higher)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (v12 or higher)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)

### Installation

#### 1. Clone the repository
```bash
git clone https://github.com/NedimJugo/Portofolio-website.git
cd Portofolio-website
```

#### 2. Backend Setup

```powershell
# Navigate to the API project
cd PortfolioSolution/PortfolioApi

# Restore dependencies
dotnet restore

# Update database connection string in appsettings.json
# Then run migrations
dotnet ef database update --project .\Portfolio.Services\Portfolio.Services.csproj --startup-project .\PortfolioApi\Portfolio.WebAPI.csproj

# Run the API
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`

#### 3. Frontend Setup

```powershell
# Navigate to the frontend project
cd portfolio-frontend

# Install dependencies
npm install

# Start development server
npm start
```

The application will be available at `http://localhost:4200`

---

## ⚙ Configuration

### Backend Configuration (`appsettings.json`)

#### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=PortfolioDb;Username=your_username;Password=your_password"
  }
}
```

#### JWT Settings (Environment Variables Recommended)
```json
{
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_HERE",
    "Issuer": "PortfolioApi",
    "Audience": "PortfolioClient",
    "ExpirationMinutes": 60
  }
}
```

#### Azure Storage (Optional)
```json
{
  "AzureStorageSettings": {
    "ConnectionString": "YOUR_AZURE_STORAGE_CONNECTION_STRING",
    "ContainerName": "portfolioimagecontainer"
  }
}
```

#### Email Configuration
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your_email@example.com",
    "Password": "YOUR_APP_PASSWORD"
  }
}
```

### Frontend Configuration (`environment.ts`)
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api'
};
```

---

## 📚 API Documentation

Once the backend is running, visit the Swagger UI documentation:
- **Swagger UI**: `https://localhost:5001/swagger`

### Main API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/login` | POST | User authentication |
| `/api/auth/register` | POST | User registration |
| `/api/projects` | GET | Get all projects |
| `/api/blogposts` | GET | Get blog posts |
| `/api/experiences` | GET | Get work experience |
| `/api/skills` | GET | Get skills |
| `/api/contactmessages` | POST | Submit contact message |
| `/api/media/upload` | POST | Upload media files |

---

## 📁 Project Structure

### Key Components

**Controllers** (26 endpoints):
- Authentication & User Management
- Blog System (Posts, Likes)
- Project Portfolio (Projects, Images, Tags, Technologies)
- Contact Messages & Email Management
- Experience & Education
- Skills & Certifications
- Media Management
- Analytics (Page Views)
- Settings & Site Content

**Services Layer**:
- Base CRUD operations with generic repository pattern
- Business logic implementation
- Data validation and transformation
- Email sending and synchronization
- Azure Blob Storage integration

**Models Layer**:
- Request/Response DTOs
- Domain models
- Configuration settings
- Search and pagination objects

---

## � Future Enhancements

- [ ] SEO optimization (meta tags, sitemaps, structured data)
- [ ] Dark mode theme
- [ ] Multi-language support (i18n)
- [ ] Comment system for blog posts

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the [issues page](https://github.com/NedimJugo/Portofolio-website/issues).

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📫 Contact

**Nedim Jugo**

- 💼 LinkedIn: [linkedin.com/in/nedim-jugo-492b99277](https://www.linkedin.com/in/nedim-jugo-492b99277)
- 🐙 GitHub: [github.com/NedimJugo](https://github.com/NedimJugo)
- 📧 Email: nedim.jugo@edu.fit.ba

---

## 🙏 Acknowledgments

- Angular team for the amazing framework
- Microsoft for .NET and Entity Framework Core
- The open-source community for incredible tools and libraries

---

<div align="center">

**⭐️ If you like this project, please give it a star! ⭐️**

Made with ❤️ by Nedim Jugo

</div>
