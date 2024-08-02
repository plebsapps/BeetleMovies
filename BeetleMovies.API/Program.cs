using BeetleMovies.API;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BeetleMovieContext>( 
    o => o.UseSqlite ( builder.Configuration["ConnectionStrings:BeetleMovieStr"] )
);   
var app = builder.Build();

app.MapGet("/", () => "Application ist Start now!");

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
