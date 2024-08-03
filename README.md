# Creating Micro-Services APIs with ASP.NET Core

## Overview
This repository provides an example of how to create APIs using ASP.NET Core. **This is a working Repo for learnig ASP.net.** The API approach simplifies the process of building web APIs by reducing the boilerplate code and focusing on the essentials.

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


### Async & Await
```csharp
//Async & Await
app.MapGet("/movie/{number:int}", async (BeetleMovieContext context, int number) => {
    return await context.Movies.FirstOrDefaultAsync(x => x.Id == number);
});

app.MapGet("/movies", async (BeetleMovieContext context) => {
    return await context.Movies.ToListAsync();
});
````

### With Where and Contains
```csharp
//Use this asynchronously if you don't know the title of the movie. Remember to use Postman to send the request with the title in the Header.
app.MapGet("/movie", async ( 
    BeetleMovieContext context, 
    [FromHeaderAttribute(Name = "movieName")] string title)
     => 
    {
        return await context.Movies.Where(x => x.Title.Contains(title)).ToListAsync();
    }
);  
````

### With HTTP Result
```csharp
//If the result is OK, send an HTTP 200 OK response along with the entity. If the result is null or zero, send an "HTTP 204 No Content" response.
app.MapGet("/movies", async Task<Results<NoContent, Ok<List<Movie>>>> ( 
    BeetleMovieContext context, 
    [FromHeaderAttribute(Name = "movieName")] string title)
     => 
    {
        var movieEntity = await context.Movies
                                       .Where(x => title == null || x.Title.Contains(title))
                                       .ToListAsync();
        
        if (movieEntity.Count <= 0 || movieEntity == null)
            return TypedResults.NoContent();
        else
            return TypedResults.Ok(movieEntity);        
    }
);  
````

### DTO files
In ASP.NET, DTO files (Data Transfer Objects) are special classes used to transfer data between different layers of an application.

```csharp

public class DirectorDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int MovieId { get; set; }
}

public class MovieDTO
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int Year { get; set; }
    public double Rating { get; set; }
}

public class MovieForCreatingDTO
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Title { get; set; }
    public int Year { get; set; }
    public double Rating { get; set; }
}

public class MovieForUpdatingDTO
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int Year { get; set; }
    public double Rating { get; set; }
}
```

### Profiles
In ASP.NET, "Profiles" refer to a feature that allows storing and managing user-specific data. This functionality is mainly used in connection with user profiles in web applications.


```csharp
.... 
builder.Services.AddDbContext<BeetleMovieContext>( 
    o => o.UseSqlite ( builder.Configuration["ConnectionStrings:BeetleMovieStr"] )
);   

//It is very Importen to add this betwen this lines in Programm.cs the AutoMapper dont found .. Profile in class BeetleMovieProfile 
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

```

```csharp
using AutoMapper;

namespace BeetleMovies.API;

public class BeetleMovieProfile : Profile
{
  public BeetleMovieProfile()
  {
    CreateMap<Movie, MovieDTO>().ReverseMap();
    CreateMap<Movie, MovieForCreatingDTO>().ReverseMap();
    CreateMap<Movie, MovieForUpdatingDTO>().ReverseMap();
    CreateMap<Director, DirectorDTO>()
      .ForMember(d => d.MovieId,
                 o => o.MapFrom(d => d.Movies.First().Id));
  }
}
```

### Add AutoMapper.Extensions.Microsoft.DependencyInjection
 
 ```csharp
    //Add this Line in BeetleMovies.API.csproj
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
 ```
Or add it with NuGet 

### Post Data to Database
 ```csharp
app.MapPost("/movies", async (
    BeetleMovieContext context, 
    IMapper mapper,
    [FromBody]MovieForCreatingDTO movieForCreatingDTO) =>
    {
        var movie = mapper.Map<Movie>(movieForCreatingDTO);
        context.Add(movie);
        await context.SaveChangesAsync();

        var movieToReturn = mapper.Map<MovieDTO>(movie);
        return TypedResults.Ok(movieToReturn);
    });  

app.Run();
 ```

### Post Data to Database with return URL (link to the new Movie) in Header
 ```csharp
//Post async ../movie send movie as ROW JSON 
app.MapPost("/movie", async (
    BeetleMovieContext context, 
    LinkGenerator linkGenerator,
    HttpContext httpContext,
    IMapper mapper,
    [FromBody]MovieForCreatingDTO movieForCreatingDTO) =>
    {
        var movie = mapper.Map<Movie>(movieForCreatingDTO);
        context.Add(movie);
        await context.SaveChangesAsync();

        var movieToReturn = mapper.Map<MovieDTO>(movie);
        var linkToReturn = linkGenerator.GetUriByName(httpContext, "GetMovies", new{ id = movieToReturn.Id});
        
        return TypedResults.Created(linkToReturn, movieToReturn);
    });  

//VERY Importen a funktion with Name ""GetMovies""

//GET async .../movie/[int] from URL
//Get the movie from Id write the Id in the URL 
app.MapGet("/movie/{id:int}", async (
    BeetleMovieContext context, 
    IMapper mapper,
    int id) =>
    {
        return mapper.Map<MovieDTO>(await context.Movies.FirstOrDefaultAsync(x => x.Id == id)); 
    }).WithName("GetMovies");

 ```

### Post Data to Database with return URL in Header using "CreatedAtRoute"
This will be the better way because smallerCode are better Code *KISS*

The KISS principle in coding stands for "Keep It Simple, Stupid." It means that when you're writing code, you should strive for simplicity and avoid unnecessary complexity. Your code should be as straightforward and easy to understand as possible
```csharp
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
```

### PUT Update Data in Database with return OK or NotFound

```csharp
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
            return  TypedResults.NotFound();

        mapper.Map(movieForUpdateingDTO, movie);
        await context.SaveChangesAsync();

        return TypedResults.Ok();
    });  
```

### Delete Data in Database with return NoContent or NotFound

```csharp
//Delete async ../movie/[Id] 
app.MapDelete("/movie/{id:int}", async Task<Results<NotFound, NoContent>>(
    BeetleMovieContext context, 
    int id    
    ) =>
    {
        var movie = await context.Movies.FirstOrDefaultAsync(x => x.Id == id);
        if(movie == null)  
            return TypedResults.NotFound();

        context.Movies.Remove(movie);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    });  
```
### Get List of the Directors from a Movie

```csharp
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
```

## Route Handlers in Minimal API apps
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers?view=aspnetcore-8.0

### Movie Group
```csharp
var movieGroup = app.MapGroup("/movie");
var moviesGroup = app.MapGroup("/movies");
var movieGroupWithId = movieGroup.MapGroup("/{id:int}");
var directorsGroup = movieGroupWithId.MapGroup("/directors");
```

And all **app.MapDelete("/movie/{id:int}"....**  to **movieGroupWithId.MapDelete(""....**

### Simplify *Program.cs*
```csharp
app.RegisterMoviesEndpoints();
app.RegisterDirectorsEndpoints();
```
## Create *EndpointRouteBuilderExtensions* class
This are some stacic Methode of EndpointRouteBuilderExtensions Class

## Create *DirectorsHandlers* class 
There you can find all static Methoden of DirectorsHandlers class
GetDirectorsAsync... 

## Create *MoviesHandlers* class
This section includes all static methods of the MoviesHandlers class
    moviesGroups.MapGet(...
    moviesGroups.MapPost(...
    moviesGroupsWithId.MapGet(...
    moviesGroupsWithId.MapPut(...
    moviesGroupsWithId.MapDelete(...


