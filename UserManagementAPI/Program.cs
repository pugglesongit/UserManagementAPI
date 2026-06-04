using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var users = new List<User>();
var idCounter = 0;
{
    users.Add(new User { Id = idCounter++, FirstName = "Chris", LastName = "Justice", Email = "chris.justice@example.com" });
    users.Add(new User { Id = idCounter++, FirstName = "Nikki", LastName = "Foster", Email = "nikki.Foster@example.com" });
}

app.MapGet("/", () => "Welcome to your User Management Program!");

app.MapPost("/users", (User user) =>
{
    user.Id = idCounter++; //Assign a new ID to a user and increases the counter
    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

app.MapGet("/users", () =>
{
    return Results.Ok(users);
});

app.MapGet("/users/{id}", (int id) =>
{
    if (id < 0 || id >= idCounter)
    {
        return Results.NotFound();
    }
    else
    {
        return Results.Ok(users[id]);
    }
});

app.MapPut("/users/{id}", (int id, User updated) =>
{
    var existing = users.FirstOrDefault(Users => Users.Id == id); //Finds user with a specific ID
    if (string.IsNullOrWhiteSpace(existing?.FirstName)) //check if the user exists and has a valid ID
    {
        return Results.NotFound();
    }

    existing.FirstName = updated.FirstName;
    existing.LastName = updated.LastName;
    existing.Email = updated.Email;

    return Results.Ok(existing);
});

app.MapDelete("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(Users => Users.Id == id);
    if (string.IsNullOrWhiteSpace(user?.FirstName)) //Checks if the user exists and has an ID
    {
        return Results.NotFound();
    }

    users.Remove(user); //removes the user from the list
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