using System.Reflection.Metadata.Ecma335;
using BeetleMovies.API;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BeetleMovieContext>( 
    o => o.UseSqlite ( builder.Configuration["ConnectionStrings:BeetleMovieStr"] )
);   
var app = builder.Build();

app.MapGet("/", () => "Application ist start now!");

//If the result is OK, send an HTTP 200 OK response along with the entity. If the result is null or zero, send an "HTTP 204 No Content" response.
app.MapGet("/movie", async ( 
    BeetleMovieContext context, 
    [FromHeaderAttribute(Name = "movieName")] string title)
     => 
    {
        var movieEntity = await context.Movies
                                       .Where(x => x.Title.Contains(title))
                                       .ToListAsync();
        
        if (movieEntity.Count <= 0 || movieEntity == null)
            return Results.NoContent();
        else
            return Results.Ok(movieEntity);        
    }
);  

/*
//Use this asynchronously if you don't know the title of the movie. Remember to use Postman to send the request with the title in the Header.
app.MapGet("/movie", async ( 
    BeetleMovieContext context, 
    [FromHeaderAttribute(Name = "movieName")] string title)
     => 
    {
        return await context.Movies.Where(x => x.Title.Contains(title)).ToListAsync();
    }
);  
*/

/*
//Async & Await
app.MapGet("/movie/{number:int}", async (BeetleMovieContext context, int number) => {
    return await context.Movies.FirstOrDefaultAsync(x => x.Id == number);
});

app.MapGet("/movies", async (BeetleMovieContext context) => {
    return await context.Movies.ToListAsync();
});
*/

/*
//You need to use this with Postman and include the title in the Header.
app.MapGet("/movie", 
    (BeetleMovieContext context, [FromHeaderAttribute(Name = "X-CUSTOM_TITEL")] string title)
     => 
    {
        return context.Movies.Where(x => x.Title == title).ToList();
    }
);  
*/

/*
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
*/



app.Run();
