using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(x => x.AddPolicy("all", a =>
    a.AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()));
builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("TodoDb"),
    new MySqlServerVersion(new Version(8, 0, 21))));
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
    });
});

var app = builder.Build();
app.UseStaticFiles();
app.UseCors("all");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (builder.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
app.MapGet("/", () => "Hello World!");

app.MapGet("/GetItems", async (ToDoDbContext context) =>
{
    return await context.Items.ToListAsync();
});

app.MapPost("/AddItem/{name}", async (ToDoDbContext context, string name) =>
{
    Item item = new Item { Name = name, IsComplete = false };
    await context.AddAsync(item);
    await context.SaveChangesAsync();
    return TypedResults.Created($"{item.Id}", item);
});

app.MapPut("/UpdateItem/{Id}", async (ToDoDbContext context,int Id) =>
{
    var item = await context.Items.FindAsync(Id);
    if (item == null)
    {
        return Results.NotFound();
    }
    item.IsComplete = !item.IsComplete;
    context.Items.Update(item);
    await context.SaveChangesAsync();
 return TypedResults.Created($"{item.Id}", item);
    
});

app.MapDelete("/DeleteItem/{Id}", async (ToDoDbContext context, int Id) =>
{
    var item = await context.Items.FindAsync(Id);
    if (item == null)
    {
        return Results.NotFound();
    }
    context.Items.Remove(item);
    await context.SaveChangesAsync();

    return Results.NoContent();
});





app.Run();