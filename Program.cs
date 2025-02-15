using TodoApi;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://client-ggbs.onrender.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("tododb"), 
     ServerVersion.Parse("8.0.41-mysql"))); 

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

app.MapGet("/items", async (ToDoDbContext context) =>
{
    return await context.Items.ToListAsync();
});

app.MapPost("/items", async (ToDoDbContext context, Item item) =>
{
    context.Items.Add(item);
    await context.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});
app.MapPut("/items/{id}", async (ToDoDbContext context, int id, Item item) =>
{
    if (id != item.Id)
    {
        return Results.BadRequest();
    }
     item.IsComplete = true;

    context.Entry(item).State = EntityState.Modified;
    await context.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/items/{id}", async (ToDoDbContext context, int id) =>
{
    var item = await context.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound();
    }

    context.Items.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();