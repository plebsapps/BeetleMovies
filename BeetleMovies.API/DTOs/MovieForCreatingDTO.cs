using System.ComponentModel.DataAnnotations;

namespace BeetleMovies.API;

public class MovieForCreatingDTO
{
  //Validation
  [Required]
  [StringLength(100, MinimumLength = 3)]
  public required string Title { get; set; }
  public int Year { get; set; }
  public double Rating { get; set; }
}
