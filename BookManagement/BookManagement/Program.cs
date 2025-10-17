using System.Text.Json.Serialization;
using BookManagement.Features.Books;
using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<BookManagementContext>(options =>
    options.UseSqlite("Data Source=bookmanagement1.db"));
builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<GetAllBooksHandler>();
builder.Services.AddScoped<GetBookByIdHandler>();
builder.Services.AddScoped<DeleteBookByIdHandler>();
builder.Services.AddScoped<UpdateBookByIdHandler>();

var app = builder.Build();

// Ensure the database is created at runtime
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookManagementContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/books/new", async (CreateBookRequest request, CreateBookHandler handler) => 
    await handler.Handle(request));
app.MapGet("/books/all/{page}/{pageSize}", async (int page, int pageSize, GetAllBooksHandler handler) =>
    await handler.Handle(page, pageSize));
app.MapGet("/books/{id:int}", async (int id, GetBookByIdHandler handler) =>
    await handler.Handle(new GetBookByIdRequest(id)));
app.MapDelete("/books/{id:int}", async (int id, DeleteBookByIdHandler handler) =>
    await handler.Handle(new DeleteBookByIdRequest(id)));
app.MapPut("/books/{id:int}", async (int id, UpdateBookByIdRequest request, UpdateBookByIdHandler handler) =>
    await handler.Handle(id, request));


app.Run();
