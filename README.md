# Navperks Backend

**Navperks Backend** is a RESTful API built with ASP.NET Core, designed to support the Navperks platform. It manages core functionalities such as user authentication, data management, and business logic processing.

## 🚀 Features

- User authentication and authorization  
- Comprehensive API endpoints for data operations  
- Modular architecture with Controllers, Models, and Services  
- Environment-based configuration using `appsettings.json`  
- Containerization support with Docker  

## 📁 Project Structure

Navperks-Backend/
├── Controllers/ # API endpoint controllers
├── Models/ # Data models
├── Services/ # Business logic services
├── Properties/ # Project properties
├── appsettings.json # Application configuration
├── Dockerfile # Docker configuration
├── Program.cs # Application entry point
├── FYP-Navperks.csproj # Project file
└── FYP-Navperks.sln # Solution file


## 🛠️ Getting Started

### Prerequisites

- .NET 6 SDK or later  
- Docker (optional, for containerization)  

### Installation

Clone the repository:

```bash
git clone https://github.com/HammmadH/Navperks-Backend.git

cd Navperks-Backend

dotnet restore

dotnet build

dotnet run
