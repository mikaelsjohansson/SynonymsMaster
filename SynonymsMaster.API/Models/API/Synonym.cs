using System.ComponentModel.DataAnnotations;

namespace SynonymsMaster.API.Models.API
{
    public record Synonym([Required] string Word, [Required] string To)
    {
    }
}