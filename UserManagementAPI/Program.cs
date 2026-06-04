using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var users = new List<User>();
var idCounter = 1;

app.MapPost("/users", (User user) =>
{
    user.Id = idCounter++;
    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

app.MapGet("/users", () =>
{
    return Results.Ok(users);
});

app.MapGet("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(Users => Users.Id == id);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

app.MapPut("/users/{id:int}", (int id, User updated) =>
{
    var existing = users.FirstOrDefault(Users => Users.Id == id);
    if (existing is null) return Results.NotFound();

    existing.FirstName = updated.FirstName;
    existing.LastName = updated.LastName;
    existing.Email = updated.Email;

    return Results.Ok(existing);
});

app.MapDelete("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(Users => Users.Id == id);
    if (user is null) return Results.NotFound();

    users.Remove(user);
    return Results.NoContent();
});

app.Run();

// User class
public class User
{
    public int Id { get; set; }
    required public string FirstName { get; set; }
    required public string LastName { get; set; }
    required public string Email { get; set; }
}