# Stock Monitoring and Analytics System (SMAS)

A comprehensive full-stack e-commerce platform with advanced stock monitoring, sales analytics, and business intelligence capabilities. Built with modern technologies for scalability, security, and real-time data processing.

---

## Features

### Core Functionality
- **Role-Based Access Control**: Admin, Salesman, and Buyer roles with granular permissions
- **Product Management**: Complete CRUD operations with categorization and inventory tracking
- **Order Management**: Full order lifecycle from creation to delivery tracking
- **Shopping Cart & Wishlist**: Persistent cart and wishlist management
- **Real-Time Notifications**: SignalR-powered live notifications for order updates and alerts

### Stock & Inventory
- **Inventory Monitoring**: Real-time stock level tracking and low-stock alerts
- **Automated Audit Logs**: Track all inventory changes with timestamps and user information
- **Refresh Inventory System**: Automated inventory refresh mechanism with audit trails

### Sales & Analytics
- **Sales Dashboard**: Real-time sales metrics and KPIs
- **Advanced Analytics**: Sales trends, revenue analysis, and performance metrics
- **Forecast Engine**: AI-powered demand forecasting and stock predictions
- **Commission Management**: Automated commission calculation for salesmen
- **Report Generation**: Export reports as Excel files with multiple formats

### Business Management
- **Customer Management**: Customer profiles, purchase history, and preferences
- **Supplier Management**: Supplier information and order coordination
- **Discount Management**: Create and manage promotional discounts
- **Complaint Management**: Track and resolve customer complaints
- **Discount Analytics**: Analyze discount impact on sales

### Security Features
- **JWT Authentication**: Secure token-based authentication
- **Password Security**: BCrypt-encrypted passwords
- **Rate Limiting**: API rate limiting to prevent abuse
- **Error Handling**: Comprehensive error responses with detailed logging

### Admin Tools
- **Settings Management**: Configurable system settings
- **Audit Logs**: Complete audit trail of system operations
- **User Guides**: In-app user documentation
- **System Notifications**: Administrative alerts and notifications
- **Health Checks**: API health monitoring endpoint

---

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0 (C#)
- **Database**: PostgreSQL 15+
- **ORM**: Entity Framework Core 8.0
- **Authentication**: JWT (JSON Web Tokens)
- **Real-Time Communication**: SignalR
- **Logging**: Serilog
- **Validation**: FluentValidation
- **Password Hashing**: BCrypt.Net
- **Report Generation**: ClosedXML (Excel export)
- **Swagger/OpenAPI**: API documentation

### Frontend
- **Framework**: React 18.2+ with TypeScript
- **Build Tool**: Vite
- **State Management**: Redux Toolkit
- **HTTP Client**: Axios
- **Styling**: Tailwind CSS 3.3
- **UI Components**: Headless UI, Heroicons, Lucide React
- **Animations**: Framer Motion
- **Charts**: Recharts
- **Real-Time Communication**: SignalR Client
- **Routing**: React Router v6

### DevOps & Deployment
- **Containerization**: Docker
- **Container Orchestration**: Docker Compose
- **Web Server**: Nginx
- **Frontend Hosting**: Netlify
- **CI/CD**: Docker multi-stage builds

---

## Prerequisites

Before you begin, ensure you have the following installed:

- **Node.js** 18+ and npm 9+
- **.NET SDK** 8.0 or higher
- **PostgreSQL** 15 or higher
- **Docker** and Docker Compose (optional, for containerized deployment)
- **Python** 3.8+ (for setup script)

---

## Installation & Setup

### Option 1: Quick Setup (Automated)

```bash
# Navigate to project root
cd Stock-Monitoring-Analytic-System

# Run the setup script (handles both backend and frontend)
python setup_all.py
```

This script will:
- Install .NET dependencies
- Install Node.js/npm dependencies
- Set up the Python virtual environment
- Apply database migrations

### Option 2: Manual Setup

#### 1. Configure Environment Variables

Create a `.env` file in the project root:

```env
# Database Configuration
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=smas_db;Username=postgres;Password=your_password

# JWT Configuration
JWT_KEY=your_super_secret_jwt_key_here_must_be_long_enough
JWT_ISSUER=smas_api
JWT_AUDIENCE=smas_client
JWT_EXPIRY_MINUTES=60
JWT_REFRESH_EXPIRY_DAYS=7

# API Configuration
ASPNETCORE_ENVIRONMENT=Development
```

#### 2. Setup Backend

```bash
# Navigate to backend directory
cd SMAS.API

# Restore NuGet packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the API
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`

#### 3. Setup Frontend

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at `http://localhost:5173`

#### 4. Setup Database

PostgreSQL database will be created automatically via Entity Framework Core migrations on first API run.

To manually create and seed the database:

```bash
cd SMAS.API
dotnet ef database update
```

---

## Docker Deployment

### Build and Run with Docker Compose

```bash
# Build all services
docker-compose up --build

# Run in background
docker-compose up -d

# Stop services
docker-compose down
```

### Production Build

```bash
# Build production image
docker build -t smas:latest .

# Run production container
docker run -p 5000:5000 \
  -e DB_CONNECTION_STRING="your_connection_string" \
  -e JWT_KEY="your_jwt_key" \
  smas:latest
```

---

## Project Structure

```
Stock-Monitoring-Analytic-System/
├── SMAS.API/                          # ASP.NET Core Backend
│   ├── Controllers/                   # API Endpoints
│   ├── Services/                      # Business Logic
│   ├── Repositories/                  # Data Access Layer
│   ├── Models/                        # Entity Models
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Data/                          # DbContext & Seeder
│   ├── Middleware/                    # Custom Middleware
│   ├── Migrations/                    # EF Core Migrations
│   └── Program.cs                     # Application Entry Point
│
├── frontend/                          # React TypeScript Frontend
│   ├── src/
│   │   ├── components/               # Reusable React Components
│   │   ├── pages/                    # Page Components
│   │   ├── services/                 # API Integration
│   │   ├── context/                  # React Context (Auth, etc.)
│   │   ├── hooks/                    # Custom React Hooks
│   │   ├── lib/                      # Utilities & Helpers
│   │   ├── types/                    # TypeScript Interfaces
│   │   └── App.tsx                   # Main App Component
│   └── config/                       # Build Configuration
│
├── migrations/                        # Database Migrations
├── scripts/                          # Shell Scripts
├── deployment/                       # Nginx Configuration
├── SMAS.API.Tests/                   # Backend Unit Tests
├── schema.sql                        # Database Schema
├── Dockerfile                        # Docker Configuration
├── docker-compose.yml                # Multi-container Setup
└── setup_all.py                      # Automated Setup Script
```

---

## 📊 Key Controllers & Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/signup` - User registration
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/logout` - User logout

### Products
- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get product details
- `POST /api/products` - Create product (Admin)
- `PUT /api/products/{id}` - Update product (Admin)
- `DELETE /api/products/{id}` - Delete product (Admin)

### Orders
- `GET /api/orders` - List user orders
- `POST /api/orders` - Create new order
- `GET /api/orders/{id}` - Get order details
- `PUT /api/orders/{id}/status` - Update order status (Admin)

### Analytics
- `GET /api/analytics/sales` - Sales analytics
- `GET /api/analytics/revenue` - Revenue metrics
- `GET /api/forecast` - Demand forecasts
- `GET /api/reports` - Generate reports

### Inventory
- `GET /api/products/{id}/stock` - Check stock level
- `POST /api/products/{id}/restock` - Restock product (Admin)
- `GET /api/audit-logs` - Inventory audit logs

### Additional Endpoints
- `GET /api/cart` - Get shopping cart
- `GET /api/wishlist` - Get wishlist
- `POST /api/complaints` - File complaint
- `GET /api/notifications` - Get notifications
- `GET /api/discounts` - List discounts
- `GET /api/health` - Health check

See Swagger documentation at `/swagger` for complete API reference.

---

## Authentication & Roles

The system uses **JWT (JSON Web Tokens)** for authentication with three main roles:

### Admin
- Full system access
- Manage products, categories, suppliers
- View all analytics and reports
- Manage users and settings
- Configure discounts and commissions

### Salesman
- View assigned products
- Create and manage orders
- View personal sales analytics
- Earn commissions
- View customer interactions

### Buyer
- Browse products
- Create orders
- Manage cart and wishlist
- Track orders
- File complaints
- View purchase history

---

## 🗄️ Database Schema

Key tables include:
- **Users** - User accounts with roles
- **Products** - Product catalog with inventory levels
- **Orders** - Order records and status tracking
- **OrderItems** - Individual items in orders
- **Cart** - Shopping cart items
- **Wishlist** - User wish lists
- **Inventory** - Stock tracking with audit logs
- **Sales** - Sales transactions and metrics
- **Commissions** - Salesman commission records
- **Discounts** - Promotional discounts
- **Complaints** - Customer complaints
- **AuditLogs** - System audit trail
- **Notifications** - User notifications

---

## Testing

### Run Backend Tests

```bash
cd SMAS.API.Tests
dotnet test
```

### Run Frontend Tests

```bash
cd frontend
npm run test
```

---

## 📈 Development Workflow

### Backend Development
```bash
cd SMAS.API
dotnet run --configuration Debug
```

### Frontend Development
```bash
cd frontend
npm run dev
```

### Watch Mode with Auto-Reload
The backend supports hot-reload during development. Changes are automatically compiled and the API restarts.

### Database Migrations
```bash
cd SMAS.API

# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Revert last migration
dotnet ef migrations remove
```

---

## Deployment

### Production Deployment Steps

1. **Prepare Environment**
   ```bash
   # Set production environment variables
   export ASPNETCORE_ENVIRONMENT=Production
   export ASPNETCORE_URLS=http://+:5000
   ```

2. **Build Backend**
   ```bash
   cd SMAS.API
   dotnet publish -c Release -o ../publish
   ```

3. **Build Frontend**
   ```bash
   cd frontend
   npm run build
   ```

4. **Deploy with Docker**
   ```bash
   docker build -t smas:latest .
   docker run -d -p 5000:5000 \
     -e DB_CONNECTION_STRING="prod_connection_string" \
     -e JWT_KEY="prod_jwt_key" \
     smas:latest
   ```

5. **Configure Nginx** (see `deployment/nginx/default.conf`)

6. **Frontend Deployment** (Netlify)
   ```bash
   # Build optimized frontend
   cd frontend
   npm run build
   
   # Deploy to Netlify
   netlify deploy --prod --dir=dist
   ```

---

## API Documentation

### Swagger/OpenAPI

Access interactive API documentation:
- **Development**: `http://localhost:5000/swagger`
- **Production**: `https://your-domain.com/swagger`

API documentation is automatically generated from controller comments and models.

---

## Troubleshooting

### Database Connection Issues
```bash
# Check PostgreSQL is running
psql -U postgres

# Verify connection string format:
# Host=localhost;Port=5432;Database=smas_db;Username=postgres;Password=password
```

### dotnet run fails
```bash
# Clear build cache
dotnet clean

# Restore packages
dotnet restore

# Try running again
dotnet run
```

### Frontend npm errors
```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### Database migration errors
```bash
# Drop and recreate database
dotnet ef database drop
dotnet ef database update
```

### Port Already in Use
```bash
# Change API port in launchSettings.json
# Change frontend port in vite.config.ts
```

---

## Support & Contribution

### Reporting Issues
Create an issue with:
- Clear title describing the problem
- Steps to reproduce
- Expected vs. actual behavior
- Screenshots/logs if applicable

### Contributing
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 👥 Team & Attribution

**Stock Monitoring and Analytics System** developed as a comprehensive full-stack application.

---

## Acknowledgments

- ASP.NET Core team for the excellent framework
- React community for innovative UI libraries
- PostgreSQL for reliable database
- All open-source contributors

---

## Contact

For questions or support, please open an issue in the repository or contact the development team.

---

 ## Author 
 **Muneeb Bin Anjum**
 - Github:  [MuneebbinAnjum](https:/github.com/MuneebbinAnjum)
