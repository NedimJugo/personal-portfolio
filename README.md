# 🌐 Personal Portfolio

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-20.3-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoft-azure&logoColor=white)

**A full-stack personal portfolio website with comprehensive CMS**

*Showcasing projects, skills, blog posts, and professional experience*

---

</div>

## 📖 About

**Personal Portfolio** is a full-stack personal portfolio platform built with modern web technologies. The application combines a sleek Angular frontend with a robust ASP.NET Core backend, providing a complete content management system for showcasing professional work, writing blog posts, and managing visitor engagement.

Built with Angular 20+ featuring Server-Side Rendering (SSR) and ASP.NET Core 8.0, the platform offers a comprehensive solution for managing projects, blog content, work experience, skills, and certifications. The system includes advanced features like Azure Blob Storage integration, email management, visitor analytics, and JWT-based authentication.

The application demonstrates enterprise-level architecture with clean separation of concerns, Entity Framework Core for data management, PostgreSQL database, and cloud storage capabilities. Perfect for developers, designers, and professionals looking to establish a strong online presence with full control over their content.

---

## ✨ Features

### 🎨 Frontend Capabilities
- **Modern Angular Application** - Built with Angular 20+ and Server-Side Rendering (SSR)
- **Responsive Design** - Mobile-first approach with Angular Material components
- **Interactive Portfolio** - Dynamic project showcase with filtering and search
- **Blog System** - Full-featured blog with likes and engagement tracking
- **PDF Generation** - Export capabilities using jsPDF and html2canvas
- **Real-time Updates** - Reactive forms and state management with RxJS
- **Smooth Animations** - Enhanced UX with Angular animations

### 🔧 Backend Infrastructure
- **RESTful API** - Built with ASP.NET Core 8.0 Web API
- **Clean Architecture** - Layered design with separation of concerns
- **Entity Framework Core** - Code-first approach with PostgreSQL
- **Azure Blob Storage** - Cloud-based media storage and CDN
- **Email Integration** - SMTP/IMAP support for notifications
- **JWT Authentication** - Secure token-based auth with refresh tokens
- **Comprehensive Validation** - FluentValidation for request validation
- **API Documentation** - Swagger/OpenAPI integration

### 📊 Content Management
- **Project Portfolio** - Showcase with images, tags, and technologies
- **Blog Management** - Posts with draft/published status and likes
- **Experience Tracking** - Work history and education timeline
- **Skills Showcase** - Technical skills and certifications
- **Testimonials** - Client feedback and recommendations
- **Analytics Dashboard** - Page views and visitor tracking
- **Newsletter System** - Subscriber management
- **Site Settings** - Customizable content and configuration

### 🔐 Security & Performance
- **Authentication & Authorization** - Role-based access control
- **Password Security** - BCrypt encryption for credentials
- **Token Management** - JWT with refresh token rotation
- **Input Validation** - Server-side validation with FluentValidation
- **SSR Optimization** - Fast initial page loads
- **Azure CDN** - Optimized media delivery

---

## 🛠️ Built With

### Frontend Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| ![Angular](https://img.shields.io/badge/Angular-20.3-DD0031?style=flat&logo=angular&logoColor=white) | 20.3.x | Frontend Framework |
| ![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?style=flat&logo=typescript&logoColor=white) | 5.9.x | Type-Safe Development |
| **Angular Material** | 20.2.x | UI Component Library |
| **RxJS** | 7.8.x | Reactive Programming |
| **Chart.js** | 4.5.x | Data Visualization |
| **jsPDF** | Latest | PDF Generation |
| **FontAwesome** | 7.1.x | Icon Library |
| **JWT-Decode** | 4.0.x | Token Handling |

### Backend Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white) | 8.0 | Backend Framework |
| ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=flat&logo=postgresql&logoColor=white) | Latest | Primary Database |
| **Entity Framework Core** | 9.0.x | ORM & Data Access |
| **AutoMapper** | 12.0.x | Object Mapping |
| **FluentValidation** | 11.3.x | Request Validation |
| **BCrypt.Net** | 4.0.x | Password Hashing |
| **Azure Storage Blobs** | Latest | Cloud Media Storage |
| **Swashbuckle** | 9.0.x | API Documentation |

---

## 🏗️ Architecture

### Backend Structure

```
PortfolioSolution/
│
├── 📂 Portfolio.Models/              # DTOs and Data Models
│   ├── 📂 Configuration/             # Settings and config models
│   ├── 📂 Requests/                  # API request models
│   ├── 📂 Responses/                 # API response models
│   └── 📂 SearchObjects/             # Search and filter models
│
├── 📂 Portfolio.Services/            # Business Logic Layer
│   ├── 📂 Database/                  # EF Core DbContext
│   ├── 📂 Services/                  # Service implementations
│   ├── 📂 Interfaces/                # Service contracts
│   ├── 📂 Migrations/                # Database migrations
│   └── 📂 Mapping/                   # AutoMapper profiles
│
└── 📂 PortfolioApi/                  # Web API Layer
    ├── 📂 Controllers/               # API endpoints (26 controllers)
    ├── 📂 Middleware/                # Custom middleware
    ├── 📂 Extensions/                # Service extensions
    └── 📂 Filter/                    # Action filters
```

### Frontend Structure

```
portfolio-frontend/
│
├── 📂 src/
│   ├── 📂 app/
│   │   ├── 📂 components/           # Feature components
│   │   ├── 📂 guards/               # Route guards
│   │   ├── 📂 interceptors/         # HTTP interceptors
│   │   ├── 📂 models/               # TypeScript interfaces
│   │   ├── 📂 pipes/                # Custom pipes
│   │   ├── 📂 services/             # API services
│   │   └── 📂 shared/               # Shared components
│   ├── 📂 environments/             # Environment configs
│   └── 📂 assets/                   # Static assets
```

---

## 🚀 Getting Started

### Prerequisites

- [Node.js](https://nodejs.org/) 18+ installed
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [PostgreSQL 12+](https://www.postgresql.org/download/)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)
- Azure Storage Account (optional, for cloud media storage)

### Installation

#### 1. Clone the Repository

```bash
git clone https://github.com/NedimJugo/Portofolio-website.git
cd Portofolio-website
```

#### 2. Backend Setup

**Navigate to API Project:**
```bash
cd PortfolioSolution/PortfolioApi
```

**Restore Dependencies:**
```bash
dotnet restore
```

**Configure Database Connection:**
- Open `appsettings.json`
- Update the PostgreSQL connection string (see [Configuration](#-configuration))

**Run Database Migrations:**
```bash
dotnet ef database update --project ../Portfolio.Services/Portfolio.Services.csproj --startup-project ./Portfolio.WebAPI.csproj
```

**Start the API Server:**
```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger: `https://localhost:5001/swagger`

#### 3. Frontend Setup

**Navigate to Frontend Project:**
```bash
cd portfolio-frontend
```

**Install Dependencies:**
```bash
npm install
```

**Configure Environment:**
- Update `src/environments/environment.ts` with API URL
- See [Configuration](#-configuration) for details

**Start Development Server:**
```bash
npm start
```

**Or for SSR:**
```bash
npm run dev:ssr
```

The application will be available at `http://localhost:4200`

---

## ⚙️ Configuration

### Backend Configuration

**File:** `PortfolioSolution/PortfolioApi/appsettings.json`

#### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=PortfolioDb;Username=your_username;Password=your_password"
  }
}
```

#### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "PortfolioApi",
    "Audience": "PortfolioClient",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

> 💡 **Tip:** Generate secure key with: `openssl rand -base64 32`

#### Azure Storage (Optional)
```json
{
  "AzureStorageSettings": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
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
    "Password": "YOUR_APP_SPECIFIC_PASSWORD",
    "EnableSsl": true
  }
}
```

### Frontend Configuration

**File:** `portfolio-frontend/src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api',
  apiVersion: 'v1'
};
```

**Production:** `environment.prod.ts`
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-production-api.com/api',
  apiVersion: 'v1'
};
```

---

## 📚 API Documentation

### Swagger Documentation

Once the backend is running, access interactive API documentation:
- **Swagger UI:** `https://localhost:5001/swagger`
- **JSON Schema:** `https://localhost:5001/swagger/v1/swagger.json`

### Main API Endpoints

#### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | User authentication |
| POST | `/api/auth/register` | User registration |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | User logout |

#### Content Management
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/projects` | Get all projects |
| POST | `/api/projects` | Create new project |
| GET | `/api/blogposts` | Get blog posts |
| POST | `/api/blogposts` | Create blog post |
| GET | `/api/experiences` | Get work experience |
| GET | `/api/skills` | Get skills |
| GET | `/api/certifications` | Get certifications |

#### Media & Files
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/media/upload` | Upload media file |
| DELETE | `/api/media/{id}` | Delete media file |
| GET | `/api/media/{id}` | Get media file |

#### Analytics & Engagement
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/pageviews` | Track page view |
| GET | `/api/analytics` | Get analytics data |
| POST | `/api/blogposts/{id}/like` | Like blog post |
| POST | `/api/contactmessages` | Submit contact message |

---

## 💻 Key Features Detail

### Project Portfolio
- Create and manage project entries with rich descriptions
- Upload multiple images per project
- Tag projects with relevant technologies
- Filter and search capabilities
- Project status management (Active, Archived)

### Blog System
- Write and publish blog posts with rich text editor
- Draft/Published status workflow
- Like functionality for reader engagement
- Category and tag organization
- Featured post highlighting

### Media Management
- Azure Blob Storage integration for scalability
- Image upload with validation
- Automatic thumbnail generation
- CDN delivery for optimized performance
- Media library with search and filter

### Analytics
- Page view tracking for all routes
- Visitor statistics and trends
- Popular content identification
- Engagement metrics (likes, comments)
- Custom date range reports

---

## 📁 Project Components

### Controllers (26 Endpoints)

**Core Functionality:**
- 🔐 Authentication & User Management
- 📝 Blog System (Posts, Likes, Categories)
- 💼 Project Portfolio (Projects, Images, Tags, Technologies)
- 📧 Contact Messages & Email Management
- 💼 Experience & Education Timeline
- 🎯 Skills & Certifications
- 📸 Media Management (Upload, Delete, Retrieve)
- 📊 Analytics (Page Views, Engagement)
- ⚙️ Settings & Site Configuration

### Services Layer

- **Base CRUD Operations** - Generic repository pattern
- **Business Logic** - Domain-specific implementations
- **Data Validation** - FluentValidation integration
- **Email Services** - SMTP sending and IMAP synchronization
- **Azure Storage** - Blob upload, download, delete
- **Authentication** - JWT token generation and validation
- **Analytics** - View tracking and reporting

---

## 🔮 Future Enhancements

- [ ] SEO optimization (meta tags, sitemaps, structured data)
- [ ] Dark mode theme with user preference persistence
- [ ] Multi-language support (i18n) for global audience
- [ ] Comment system for blog posts with moderation
- [ ] Social media integration and sharing
- [ ] Advanced analytics dashboard with charts
- [ ] Real-time notifications using SignalR
- [ ] Mobile application (React Native/Flutter)
- [ ] GraphQL API option
- [ ] Performance monitoring and logging
- [ ] Automated backups and disaster recovery
- [ ] A/B testing framework

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the [issues page](https://github.com/NedimJugo/Portofolio-website/issues).

### How to Contribute

1. **Fork the project**
   ```bash
   git clone https://github.com/YOUR_USERNAME/Portofolio-website.git
   ```

2. **Create your feature branch**
   ```bash
   git checkout -b feature/AmazingFeature
   ```

3. **Commit your changes**
   ```bash
   git commit -m 'Add some AmazingFeature'
   ```

4. **Push to the branch**
   ```bash
   git push origin feature/AmazingFeature
   ```

5. **Open a Pull Request**

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📫 Contact

**Nedim Jugo**

- 💼 **LinkedIn:** [linkedin.com/in/nedim-jugo-492b99277](https://www.linkedin.com/in/nedim-jugo-492b99277)
- 🐙 **GitHub:** [github.com/NedimJugo](https://github.com/NedimJugo)
- 📧 **Email:** nedim.jugo@edu.fit.ba
- 🌐 **Portfolio:** [Your Portfolio URL]

---

## 🙏 Acknowledgments

- Angular team for the amazing framework and SSR capabilities
- Microsoft for .NET Core and Entity Framework
- PostgreSQL community for the robust database system
- Azure team for cloud storage solutions
- Angular Material for beautiful UI components
- The open-source community for incredible tools and libraries

---

<div align="center">

**⭐ If you like this project, please give it a star! ⭐**

*Built with 💻 by Nedim Jugo*

</div>
