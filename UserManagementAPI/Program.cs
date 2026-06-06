using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

const string ApiToken = "super-secret-token123";


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Token-based security
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-API-TOKEN", out var token))
    {
        Console.WriteLine($"[TOKEN RECEIVED] {token}");
    }
    else
    {
        Console.WriteLine("[TOKEN MISSING]");
    }
    if (!context.Request.Headers.TryGetValue("X-API-TOKEN", out token) || token != ApiToken)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new {error = "Unauthorised: Missing or Invalid API token"});
        return;
    }

    await next();
});

//Global request/response logging
app.Use(async (context, next) =>
{
    Console.WriteLine($"[REQUEST] {context.Request.Method} {context.Request.Path} at {DateTime.Now}");
    await next();
    Console.WriteLine($"[RESPONSE] {context.Response.StatusCode} for {context.Request.Method} {context.Request.Path}");
});

//global error handling, removes need for the try-catch block
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex.Message}");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occured.", detail = ex.Message });
    }
});

var users = new List<User>();
var idCounter = 0;
{
    users.Add(new User { Id = idCounter++, FirstName = "Chris", LastName = "Justice", Email = "chris.justice@example.com" });
    users.Add(new User { Id = idCounter++, FirstName = "Nikki", LastName = "Foster", Email = "nikki.Foster@example.com" });
}

//Helper methods go here, before ANY endpoints are implemented
bool IsInvalid(User userDetails) => 
string.IsNullOrWhiteSpace(userDetails.FirstName) || string.IsNullOrWhiteSpace (userDetails.LastName) || string.IsNullOrWhiteSpace (userDetails.Email);

bool EmailExists(string email) =>
users.Any(existingUser => existingUser.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

User? FindUser(int id) =>
users.FirstOrDefault(userId => userId.Id == id);

//Helper method ends

app.MapGet("/", () => "Welcome to your User Management Program!");

//Create the new user
app.MapPost("/users", (User user) =>
{
    //Validate required fields - You cannot leave whitespace or blank.
    if (IsInvalid(user))
    return Results.BadRequest("First name, last name and email cannot be empty; please enter a name and email.");

    if (EmailExists(user.Email))
    return Results.BadRequest("First name, last name and email cannot be empty; please enter a name and email.");

    int newId = Interlocked.Increment(ref idCounter); //Stops duplicate IDs
    user.Id = newId; 
    users.Add(user);
    Console.WriteLine($"User created: {user.FirstName} {user.LastName} {user.Email} {user.Id}");
    return Results.Created($"/users/{user.Id}", user);
});

// Get all users
app.MapGet("/users", () =>
{
    Console.WriteLine("Here are the current users: ");

    foreach (var user in users)
    {
        Console.WriteLine($" {user.Id}: {user.FirstName} {user.LastName} ({user.Email})"); //For console purposes, the API will still return the full list.
    }
    return Results.Ok(users);
});

// Get user by ID
app.MapGet("/users/{id}", (int id) =>
{
    var user = FindUser(id);
    if (user is null)
    {
        return Results.NotFound("User does not exist, please check the ID and try again"); //I added this, thought a helpful message would be good.
    }
    else
    {
        Console.WriteLine($"User found: {user.FirstName} {user.LastName} {user.Email} {user.Id}"); //added for console purposes, the API will still return what's expected.
        return Results.Ok(user);
    }
});

//Update a user
app.MapPut("/users/{id}", (int id, User updated) =>
{
    var existing = FindUser(id);
    {
        if (existing is null) //check if the user exists and has a valid ID
        return Results.NotFound("ID does not exist, please check and try again"); //helpful message to the error.
    }

    if (IsInvalid(updated))
    return Results.BadRequest("First name, last name and email cannot be empty; please enter a name and email.");

    existing.FirstName = updated.FirstName;
    existing.LastName = updated.LastName;
    existing.Email = updated.Email;

    Console.WriteLine($"User Updated: {existing.FirstName} {existing.LastName} {existing.Email} {existing.Id}"); //Tells you what it's been updated to
    return Results.Ok(existing);
});

// Delete a user
app.MapDelete("/users/{id}", (int id) =>
{
    var user = FindUser(id);
        
        if (user is null)
        return Results.NotFound("User not found");
        
        users.Remove(user); //removes the user from the list
        Console.WriteLine($"User deleted: {user.FirstName} {user.LastName} {user.Email} {user.Id}");
        return Results.Ok(new { message = "User Deleted"}); 
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