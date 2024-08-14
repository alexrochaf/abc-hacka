# User Management API

This is a simple User Management API built with ASP.NET Core. It provides basic CRUD operations for managing users.

## Features

- Get all users
- Get a specific user by ID
- Create a new user
- Update an existing user
- Delete a user

## Project Structure

- `Controllers/`: Contains the API controllers
- `Models/`: Contains the data models
- `Repositories/`: Contains the repository interfaces and implementations
- `Validators/`: Contains the FluentValidation validators
- `Data/`: Contains the database context

## Technologies Used

- ASP.NET Core
- Entity Framework Core
- FluentValidation
- xUnit for testing

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio or your preferred IDE
3. Run the application

## API Endpoints

- GET /api/users: Get all users
- GET /api/users/{id}: Get a specific user
- POST /api/users: Create a new user
- PUT /api/users/{id}: Update an existing user
