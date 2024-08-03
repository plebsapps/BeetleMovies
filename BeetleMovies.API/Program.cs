using AutoMapper;
using BeetleMovies.API;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BeetleMovieContext>( 
    o => o.UseSqlite ( builder.Configuration["ConnectionStrings:BeetleMovieStr"] )
);   

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.MapGet("/", () => "Application is start now!");

//Delete async ../movie/[Id] 
app.MapDelete("/movie/{id:int}", async Task<Results<NotFound, NoContent>>(
    BeetleMovieContext context, 
    int id,
    [FromBody] MovieForUpdatingDTO movieForUpdateingDTO    
    ) =>
    {
        var movie = await context.Movies.SingleOrDefaultAsync(x => x.Id == id);
        if(movie == null)  
            return TypedResults.NotFound();

        context.Movies.Remove(movie);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    });  


//Put async ../movie/[Id] send movie as ROW JSON 
app.MapPut("/movie/{id:int}", async Task<Results<NotFound, Ok>>(
    BeetleMovieContext context, 
    IMapper mapper,
    int id,
    [FromBody] MovieForUpdatingDTO movieForUpdateingDTO    
    ) =>
    {
        var movie = await context.Movies.SingleOrDefaultAsync(x => x.Id == id);
        if(movie == null)  
            return TypedResults.NotFound();

        mapper.Map(movieForUpdateingDTO, movie);
        await context.SaveChangesAsync();

        return TypedResults.Ok();
    });  


//Post async ../movie send movie as ROW JSON 
app.MapPost("/movie", async (
    BeetleMovieContext context, 
    IMapper mapper,
    [FromBody]MovieForCreatingDTO movieForCreatingDTO) =>
    {
        var movie = mapper.Map<Movie>(movieForCreatingDTO);
        context.Add(movie);
        await context.SaveChangesAsync();

        var movieToReturn = mapper.Map<MovieDTO>(movie);
        
        return TypedResults.CreatedAtRoute(movieToReturn,"GetMovies", new { id = movieToReturn.Id });
    });

//Get async .../movie/[id]/directors the Directors of a movie
app.MapGet("/movie/{movieId:int}/directors", async (
    BeetleMovieContext context, 
    IMapper mapper,
    int movieId) =>
    {
        return mapper.Map<IEnumerable<DirectorDTO>>((await context.Movies
                            .Include(movie => movie.Directors)  
                            .FirstOrDefaultAsync(movie => movie.Id == movieId))?.Directors);
    });

//GET async .../movies  from Header -> movieName?[String]
app.MapGet("/movies", async Task<Results<NoContent, Ok<List<Movie>>>> ( 
    BeetleMovieContext context, 
    [FromHeaderAttribute(Name = "movieName")] string? title
    ) => 
    {
        var movieEntity = await context.Movies
                                       .Where(x => title == null || 
                                              x.Title.ToLower().Contains(title.ToLower()))
                                       .ToListAsync();
        
        if (movieEntity.Count <= 0 || movieEntity == null)
            return TypedResults.NoContent();
        else
            return TypedResults.Ok(movieEntity);        
    }
);  

//GET async .../movie  from Header -> movieName?[String]
//If you don't know the title of the movie. Remember to use Postman to send the request with the title in the Header.
app.MapGet("/movie", async ( 
    BeetleMovieContext context, 
    [FromHeaderAttribute(Name = "movieName")] string title)
     => 
    {
        return await context.Movies.Where(x => x.Title.Contains(title)).ToListAsync();
    }
);  

//GET async .../movie/[int] from URL
//Get the movie from Id write the Id in the URL 
app.MapGet("/movie/{id:int}", async (
    BeetleMovieContext context, 
    IMapper mapper,
    int id) =>
    {
        return mapper.Map<MovieDTO>(await context.Movies.FirstOrDefaultAsync(x => x.Id == id)); 
    }).WithName("GetMovies");

app.Run();
