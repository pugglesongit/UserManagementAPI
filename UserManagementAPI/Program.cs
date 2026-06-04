// Initial copilot generated code
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory user store
var users = new List<User>();
var idCounter = 1;

// CREATE
app.MapPost("/users", (User user) =>
{
    user.Id = idCounter++;
    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

// READ ALL
app.MapGet("/users", () =>
{
    return Results.Ok(users);
});

// READ ONE
app.MapGet("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

// UPDATE
app.MapPut("/users/{id:int}", (int id, User updated) =>
{
    var existing = users.FirstOrDefault(u => u.Id == id);
    if (existing is null) return Results.NotFound();

    existing.FirstName = updated.FirstName;
    existing.LastName = updated.LastName;
    existing.Email = updated.Email;

    return Results.Ok(existing);
});

// DELETE
app.MapDelete("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();

    users.Remove(user);
    return Results.NoContent();
});

app.Run();

// User model (still inside Program.cs)
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}