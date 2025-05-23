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
                CategoryName = p.Category.CategoryName
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


app.Run();

//don't move or change this!
public partial class Program { }