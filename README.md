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
#### Create *EndpointRouteBuilderExtensions* class
This are some stacic Methode of EndpointRouteBuilderExtensions Class

#### Create *DirectorsHandlers* class 
There you can find all static Methoden of DirectorsHandlers class
GetDirectorsAsync... 

#### Create *MoviesHandlers* class
This section includes all static methods of the MoviesHandlers class
    moviesGroups.MapGet(...
    moviesGroups.MapPost(...
    moviesGroupsWithId.MapGet(...
    moviesGroupsWithId.MapPut(...
    moviesGroupsWithId.MapDelete(...

### Errors 
To see the errors, you need to set "ASPNETCORE_ENVIRONMENT": "Development" in the launchSettings.json file. Normally, you set "ASPNETCORE_ENVIRONMENT" to "Production".

```csharp
  "profiles": {    
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5000;http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
```

```csharp
    ....
      "ASPNETCORE_ENVIRONMENT": "Production"
```

### Logging
In this method, you can see that the logger is active (ILogger<MovieDTO> logger). You can write messages like Debug:
*logger.LogDebug($"Movie not found. Param: {title}");* or like this: *logger.LogDebug($"Movie not found. Param: {title}");*

Here the method:
```csharp
public static async Task<Results<NoContent, Ok<IEnumerable<MovieDTO>>>> GetMoviesAsync(
    BeetleMovieContext context,
    IMapper mapper,
    ILogger<MovieDTO> logger,
    [FromQuery(Name = "movieName")] string? title)
{
    var movieEntity = await context.Movies
                                  .Where(x => title == null ||
                                          x.Title.ToLower().Contains(title.ToLower()))
                                  .ToListAsync();

    if (movieEntity == null || movieEntity.Count <= 0)
    {
        logger.LogDebug($"Movie not found. Param: {title}");
        return TypedResults.NoContent();
    }
    else
    {
        logger.LogInformation($"Movie found. Return: {movieEntity[0].Title}");
        return TypedResults.Ok(mapper.Map<IEnumerable<MovieDTO>>(movieEntity));
    }
}
```

### Filter 
This is an example of a filter. The example is not a perfect filter, but it clarifies how to use the filter. You need to add a new class and edit the RouteBuilder, as shown below.

**Class PerfectMoviesAreLockedFilter**
```csharp
public class PerfectMoviesAreLockedFilter : IEndpointFilter
{
    public readonly int _lockedPerfectMoviesId;

    public PerfectMoviesAreLockedFilter(int lockedPerfectMoviesId)
    {
      _lockedPerfectMoviesId = lockedPerfectMoviesId;
    }
    
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        int moviesId;

        if (context.HttpContext.Request.Method == "PUT")
        {
          moviesId = context.GetArgument<int>(2);
        }
        else if (context.HttpContext.Request.Method == "DELETE")
        {
          moviesId = context.GetArgument<int>(1);
        }
        else
        {
          throw new NotSupportedException("This filter is not supported for this scenario.");
        }

        var toyStoryId = _lockedPerfectMoviesId;

        if (moviesId == toyStoryId)
        {
          return TypedResults.Problem(new()
          {
            Status = 400,
            Title = "Movie is perfect! You should not change or delete it!",
            Detail = "You can not modify or delete what's already perfect!"
          });
        }

        var result = await next.Invoke(context);
        return result;
    }
}
```

```csharp
    moviesGroupsWithId.MapPut("", MoviesHandlers.UpdateMoviesAsync)
      .AddEndpointFilter(new PerfectMoviesAreLockedFilter(2))
      .AddEndpointFilter(new PerfectMoviesAreLockedFilter(5));

    moviesGroupsWithId.MapDelete("", MoviesHandlers.DeleteMoviesAsync)
      .AddEndpointFilter(new PerfectMoviesAreLockedFilter(2))
      .AddEndpointFilter(new PerfectMoviesAreLockedFilter(5));
```

#### Filter Grouping
A better approach is to use the filter in different places by setting the filter in the group like this:

```csharp
    var moviesGroupsWithIdFilters = entpointRouteBuilder.MapGroup("/movies/{moviesId:int}") 
      .AddEndpointFilter(new PerfectMoviesAreLockedFilter(2))
      .AddEndpointFilter(new PerfectMoviesAreLockedFilter(5));
```

#### NotFoundResponse Logging Filter

```csharp

public class LogNotFoundResponseFilter(ILogger<LogNotFoundResponseFilter> logger) : IEndpointFilter
{
  public readonly ILogger<LogNotFoundResponseFilter> _logger = logger;
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    var result = await next(context);
    var actualResults = (result is INestedHttpResult result1) ? result1.Result : (IResult)result;

    if (actualResults is IStatusCodeHttpResult { StatusCode: (int)HttpStatusCode.NotFound })
    {
      _logger.LogInformation($"Resource {context.HttpContext.Request.Path} was not found.");
    }

    return result;
  }
}
```
```csharp
    var moviesGroupsWithIdFilters = entpointRouteBuilder.MapGroup("/movies/{moviesId:int}") 
      .AddEndpointFilter(new PerfectMoviesAreLockedFilter(2))
      .AddEndpointFilter(new PerfectMoviesAreLockedFilter(5));
```

You can see in Terminal the Info:

info: BeetleMovies.API.LogNotFoundResponseFilter[0]
      Resource /movies/55 was not found.

### Validation

See https://github.com/DamianEdwards/MiniValidation

Add in BeetleMovies.API.csproj
```csharp
       <PackageReference Include="MiniValidation" Version="0.9.1" />
```

```csharp
public class MovieForCreatingDTO
{
  //Validation
  [Required]
  [StringLength(100, MinimumLength = 3)]
  public required string Title { get; set; }
  public int Year { get; set; }
  public double Rating { get; set; }
}
```

Class ValidateAnnotationFilter
```csharp
public class ValidateAnnotationFilter : IEndpointFilter
{
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    var movieForCreatingDTO = context.GetArgument<MovieForCreatingDTO>(2);

    if (!MiniValidator.TryValidate(movieForCreatingDTO, out var validationError))
    {
      return TypedResults.ValidationProblem(validationError);
    }

    return await next(context);
  }
}
```

Edit EndpointRouteBuilderExtensions class
```csharp
    moviesGroups.MapPost("", MoviesHandlers.CreateMoviesAsync)
      .AddEndpointFilter<ValidateAnnotationFilter>();

```

This error will show:
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Title": [
            "The field Title must be a string with a minimum length of 3 and a maximum length of 100."
        ]
    }
}


### Adding Swagger

Add in BeetleMovies.API.csproj
```csharp
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
```

Add in Program.cs
```csharp
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();     //Add Swagger
builder.Services.AddSwaggerGen();               //Add Swagger

var app = builder.Build();

app.UseSwagger();       //Add Swagger
app.UseSwaggerUI();     //Add Swagger

```

Add in launchSettings.json
```csharp
    ...
      "launchBrowser": true,
    "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5000;http://localhost:5001",
    ...
```

### Authorization and Authentication

And See Video 69 - 73 
And Read https://learn.microsoft.com/de-de/aspnet/identity/overview/getting-started/introduction-to-aspnet-identity

Coming Sone

### Others 

Minimal APIs overview
See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview?view=aspnetcore-8.0