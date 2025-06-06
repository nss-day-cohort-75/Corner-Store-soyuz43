using Npgsql;
using CornerStore;
using CornerStore.Models;
using CornerStore.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddDbContext<CornerStoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing"));

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ! Endpoints

// Endpoint: GET /products
app.MapGet("/products", async (CornerStoreDbContext db) =>
{
    try
    {
        List<ProductDetailDTO> products = await db.Products
            .Include(p => p.Category)
            .Select(p => new ProductDetailDTO
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Price = p.Price,
                Brand = p.Brand,
                CategoryName = p.Category!.CategoryName
            })
            .ToListAsync();

        return Results.Ok(products);
    }
    catch (Exception)
    {
        return Results.Problem("Unable to retrieve products. Please try again later.");
    }
});

// Endpoint: POST /products
app.MapPost("/products", async (ProductCreateDTO dto, CornerStoreDbContext db) =>
{
    try
    {
        Product product = new Product
        {
            ProductName = dto.ProductName,
            Price = dto.Price,
            Brand = dto.Brand,
            CategoryId = dto.CategoryId
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        ProductDTO result = new ProductDTO
        {
            Id = product.Id,
            ProductName = product.ProductName,
            Price = product.Price,
            Brand = product.Brand,
            CategoryId = product.CategoryId
        };

        return Results.Created($"/products/{product.Id}", result);
    }
    // Catches foreign key violations — e.g., when the provided CategoryId doesn't exist.
    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23503")
    {
        return Results.BadRequest("Invalid categoryId: foreign key constraint violated.");
    }
});

// Endpoint: PUT /products
app.MapPut("/products/{id}", async (int id, ProductCreateDTO dto, CornerStoreDbContext db) =>
{
    try
    {
        Product? product = await db.Products.FindAsync(id);
        if (product == null)
        {
            return Results.NotFound($"No product found with ID {id}.");
        }

        product.ProductName = dto.ProductName;
        product.Price = dto.Price;
        product.Brand = dto.Brand;
        product.CategoryId = dto.CategoryId;

        await db.SaveChangesAsync();

        ProductDTO result = new ProductDTO
        {
            Id = product.Id,
            ProductName = product.ProductName,
            Price = product.Price,
            Brand = product.Brand,
            CategoryId = product.CategoryId
        };

        return Results.Ok(result);
    }
    // Handles FK constraint violation if CategoryId is invalid
    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23503")
    {
        return Results.BadRequest("Invalid categoryId: foreign key constraint violated.");
    }
});

// Endpoint: POST /cashiers
app.MapPost("/cashiers", async (CashierDTO dto, CornerStoreDbContext db) =>
{
    Cashier cashier = new Cashier
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName
    };

    db.Cashiers.Add(cashier);
    await db.SaveChangesAsync();

    dto.Id = cashier.Id; // round-trip the ID
    return Results.Created($"/cashiers/{cashier.Id}", dto);
});

// Endpoint: GET /cashiers/{id}
app.MapGet("/cashiers/{id}", async (int id, CornerStoreDbContext db) =>
{
    Cashier? cashier = await db.Cashiers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (cashier == null)
    {
        return Results.NotFound($"No cashier found with ID {id}.");
    }

    CashierDetailDTO cashierDetail = new CashierDetailDTO
    {
        Id = cashier.Id,
        FirstName = cashier.FirstName,
        LastName = cashier.LastName,
        Orders = cashier.Orders.Select(order => new OrderDTO
        {
            Id = order.Id,
            CashierId = order.CashierId,
            CashierFullName = $"{cashier.FirstName} {cashier.LastName}",
            PaidOnDate = order.PaidOnDate,
            Total = order.Total,
            Products = order.OrderProducts.Select(op => new OrderProductDTO
            {
                ProductId = op.ProductId,
                ProductName = op.Product!.ProductName,
                Quantity = op.Quantity,
                Price = op.Product.Price
            }).ToList()
        }).ToList()
    };

    return Results.Ok(cashierDetail);
});


// Endpoint: POST /orders
app.MapPost("/orders", async (OrderCreateDTO dto, CornerStoreDbContext db) =>
{
    try
    {
        // Step 1: Validate cashier exists
        var cashier = await db.Cashiers.FindAsync(dto.CashierId);
        if (cashier == null)
            return Results.BadRequest("Invalid CashierId.");

        // Step 2: Fetch all products in request
        var productIds = dto.Products.Select(p => p.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
            return Results.BadRequest("One or more productIds are invalid.");

        // Step 3: Create the order and attach OrderProducts
        var order = new Order
        {
            CashierId = dto.CashierId,
            PaidOnDate = DateTime.UtcNow,
            OrderProducts = dto.Products.Select(p => new OrderProduct
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity
            }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Step 4: Map back to OrderDTO for response
        OrderDTO result = new OrderDTO
        {
            Id = order.Id,
            CashierId = cashier.Id,
            CashierFullName = $"{cashier.FirstName} {cashier.LastName}",
            PaidOnDate = order.PaidOnDate,
            Total = order.Total,
            Products = order.OrderProducts.Select(op => new OrderProductDTO
            {
                ProductId = op.ProductId,
                ProductName = products[op.ProductId].ProductName,
                Quantity = op.Quantity,
                Price = products[op.ProductId].Price
            }).ToList()
        };

        return Results.Created($"/orders/{order.Id}", result);
    }
    // Catches FK constraint violations (e.g. deleted cashier or product)
    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23503")
    {
        return Results.BadRequest("Foreign key constraint violated.");
    }
    // Fallback for unexpected server/db issues
    catch (Exception)
    {
        return Results.Problem("An error occurred while processing the order.");
    }
})
.Produces<OrderDTO>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

// Endpoint: GET /all orders with date query strig param
app.MapGet("/orders", async (string? orderDate, CornerStoreDbContext db) =>
{
    // Step 1: Base query — includes navigation
    var query = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
        .AsQueryable();

    // Step 2: Filter if orderDate query param is provided
    if (!string.IsNullOrEmpty(orderDate) &&
        DateTime.TryParse(orderDate, out DateTime parsedDate))
    {
        query = query.Where(o => o.PaidOnDate.HasValue && o.PaidOnDate.Value.Date == parsedDate.Date);
    }

    // Step 3: Execute query and project to DTOs
    var orders = await query.Select(order => new OrderDTO
    {
        Id = order.Id,
        CashierId = order.CashierId,
        CashierFullName = $"{order.Cashier!.FirstName} {order.Cashier.LastName}",
        PaidOnDate = order.PaidOnDate,
        Total = order.Total,
        Products = order.OrderProducts.Select(op => new OrderProductDTO
        {
            ProductId = op.ProductId,
            ProductName = op.Product!.ProductName,
            Quantity = op.Quantity,
            Price = op.Product.Price
        }).ToList()
    }).ToListAsync();

    return Results.Ok(orders);
});


// Endpoint: Del /orders by id
app.MapDelete("/orders/{id}", async (int id, CornerStoreDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);

    if (order == null)
        return Results.NotFound($"No order found with ID {id}");

    db.Orders.Remove(order);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.Run();

//don't move or change this!
public partial class Program { }