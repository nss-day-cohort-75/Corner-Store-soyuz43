# Corner Store API

A minimal RESTful API for managing a point-of-sale system for a convenience store, built with C# and ASP.NET Core (.NET 8). Data is stored in PostgreSQL using Entity Framework Core.

## Setup

1. Clone the repository
2. Restore dependencies:

   ```bash
   dotnet restore
    ```

3. Configure your PostgreSQL connection string using user secrets:

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "CornerStoreDbConnectionString" "Host=localhost;Port=5432;Database=CornerStore;Username=postgres;Password=yourpassword"
   ```

4. Apply migrations and create the database:

   ```bash
   dotnet ef database update
   ```

5. Run the application:

   ```bash
   dotnet run
   ```

6. Open Swagger UI in your browser:

   ```
   http://localhost:5000/swagger
   ```

## Endpoints

### /products

* `GET /products`
  Returns all products with their associated category names.

* `POST /products`
  Creates a new product. Requires `ProductCreateDTO`.

* `PUT /products/{id}`
  Updates a product. Requires `ProductCreateDTO`.

### /cashiers

* `POST /cashiers`
  Creates a new cashier.

* `GET /cashiers/{id}`
  Returns a cashier with their full order history, including each order's products and total.

### /orders

* `POST /orders`
  Creates an order with one or more products. Requires `OrderCreateDTO`.

* `GET /orders`
  Returns all orders. If `orderDate` query string is provided (format `YYYY-MM-DD`), returns only orders placed on that date.

* `DELETE /orders/{id}`
  Deletes an order by ID.

## Technologies

* ASP.NET Core Minimal APIs
* Entity Framework Core + Npgsql
* PostgreSQL
* Swagger (Swashbuckle)

## Project Structure

```
CornerStore/
├── Models/       // Entity models
├── DTOs/         // Request/response shapes
├── Program.cs    // All endpoints registered here
├── Migrations/   // EF Core migration history
```

## Notes

* All endpoints are implemented using minimal APIs inside `Program.cs` (no controllers).
* The database is seeded with one cashier, two categories, and two products.
* Foreign key constraints are guarded with specific error handling for clean validation.

## Development

* Default environment is `Development` (set in `launchSettings.json`)
* Nullable reference types are enabled
* Swagger is enabled in development mode for API testing


