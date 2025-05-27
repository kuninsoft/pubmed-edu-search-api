using System.ComponentModel.DataAnnotations;

namespace PubMedEdu.SearchApi.Models;

public class SearchRequest
{
    [Required]
    public string Prompt { get; set; } = null!;
    
    public int MaxResults { get; set; } = 5;
}