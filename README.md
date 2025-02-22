# Book Manager API

A RESTful API for managing books, built with ASP.NET Core 7.0.

## Features

- CRUD operations for books
- Bulk operations (create/delete multiple books)
- Soft delete functionality
- Pagination and sorting by popularity
- JWT Authentication (coming soon)
- Swagger documentation

## Prerequisites

- .NET 7.0 SDK
- SQL Server (2019 or later)
- Visual Studio 2022 or VS Code

## Initial Setup

1. Create `appsettings.json` and `appsettings.Development.json` in the project root:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=BookManager;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Note: These files are git-ignored for security reasons. Make sure to replace `YOUR_SERVER` with your SQL Server instance name.

## Getting Started

1. Clone the repository
2. Set up the appsettings.json files as described in the Initial Setup section
3. Create a new database named 'BookManager' in SQL Server
4. Run migrations:

   ```bash
   dotnet ef database update
   ```

1. Run the application:

   ```bash
   dotnet run
   ```

## API Endpoints

### Books

- `GET /api/books` - Get paginated list of books
- `GET /api/books/{id}` - Get book by ID
- `POST /api/books` - Create a new book
- `POST /api/books/bulk` - Create multiple books
- `PATCH /api/books/{id}` - Update a book
- `DELETE /api/books/{id}` - Soft delete a book
- `DELETE /api/books/bulk` - Soft delete multiple books

### Authentication (Coming Soon)

The API will implement JWT (JSON Web Token) authentication with the following features:

- User registration and login
- Role-based authorization
- Token refresh mechanism
- Secure endpoints protection

## Environment Variables (Auth Coming Soon)

```env
CONNECTION_STRING=your_connection_string
JWT_SECRET=your_jwt_secret
JWT_ISSUER=your_issuer
JWT_AUDIENCE=your_audience
```

## Error Handling

The API implements global error handling with appropriate HTTP status codes:

- 400 - Bad Request
- 401 - Unauthorized (JWT) Coming Soon
- 403 - Forbidden (JWT) Coming Soon
- 404 - Not Found
- 409 - Conflict
- 500 - Internal Server Error

## Project Structure

```bash
BookManager/
├── Controllers/
├── Models/
│   ├── Domain/
│   └── DTOs/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── db/
└── Exceptions/
```

## Future Enhancements

- [ ] JWT Authentication implementation
- [ ] User management
- [ ] Rate limiting
- [ ] Caching
- [ ] API versioning
- [ ] Unit tests
- [ ] Integration tests

## License

This project is licensed under the MIT License.
