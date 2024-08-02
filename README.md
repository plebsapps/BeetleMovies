# ASP.NET - Creating Micro-Services APIs with ASP.NET Core

## Overview
This repository provides an example of how to create APIs using ASP.NET Core. The API approach simplifies the process of building web APIs by reducing the boilerplate code and focusing on the essentials.

## Starting an ASP.NET App in the Terminal in Visual Studio Code
In the Terminal, navigate to the API directory, for example, C:\Dev\.net_ASP\BeetleMovies\BeetleMovies.API>, and type "dotnet watch run"
While dotnet watch is running, you can force the app to rebuild and restart by pressing Ctrl+R in the command shell. This feature is available only while the app is running. For example, if you run dotnet watch on a console app that ends before you press Ctrl+R, pressing Ctrl+R will have no effect.

## Example Code

### Introduction to ASP.NET
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Application is starting now!");

app.Run();
````

### Queries with Parameters in the URL
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BeetleMovieContext>( 
    o => o.UseSqlite ( builder.Configuration["ConnectionStrings:BeetleMovieStr"] )
);   
var app = builder.Build();

app.MapGet("/movie/{number:int}", (BeetleMovieContext context, int number) => {
    return context.Movies.FirstOrDefault(x => x.Id == number);
});

app.MapGet("/movie/{title}", (BeetleMovieContext context, string title) => {

    Console.WriteLine("Das ist Title in Lowerletter: " + title);    
    return context.Movies.FirstOrDefault(x => x.Title.ToLower() == title.ToLower());    
});

app.MapGet("/movies", (BeetleMovieContext context) => {
    return context.Movies;
});

app.Run();
````

### Introduction to ASP.NET
```csharp
//You need to use this with Postman and include the title in the Header.
app.MapGet("/movie", 
    (BeetleMovieContext context, [FromHeaderAttribute(Name = "X-CUSTOM_TITEL")] string title)
     => 
    {
        return context.Movies.Where(x => x.Title == title).ToList();
    }
);  
````
#### See link: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-8.0

#### Postman app
![Postman](https://github.com/plebsapps/BeetleMovies/blob/main/postman.png)
