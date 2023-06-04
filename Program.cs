using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CustomerDbContext>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var logger = LoggerFactory.Create(config =>
    {
        config.AddConsole();
    }).CreateLogger("CustomerApi");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet("/customers", async (CustomerDbContext context) =>
{
    logger.LogInformation("Getting customers...");
    var customers = await context.Customer.ToListAsync();
    logger.LogInformation("Retrieved {Count} customers", customers.Count);
    return Results.Ok(customers);
});

app.MapGet("/customers/{id}", async (string id, CustomerDbContext context) =>
{
    var customer = await context.Customer.FindAsync(id);
    if (customer == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(customer);
});

app.MapPost("/customers", async (Customer customer, CustomerDbContext context) =>
{
    context.Customer.Add(customer);
    await context.SaveChangesAsync();
    return Results.Created($"/customers/{customer.Id}", customer);
});

app.MapPut("/customers/{id}", async (string id, Customer customer, CustomerDbContext context) =>
{
    var currentCustomer = await context.Customer.FindAsync(id);
    if (currentCustomer == null)
    {
        return Results.NotFound();
    }

    context.Entry(currentCustomer).CurrentValues.SetValues(customer);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/customers/{id}", async (string id, CustomerDbContext context) =>
{
    var currentCustomer = await context.Customer.FindAsync(id);

    if (currentCustomer == null)
    {
        return Results.NotFound();
    }

    context.Customer.Remove(currentCustomer);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
