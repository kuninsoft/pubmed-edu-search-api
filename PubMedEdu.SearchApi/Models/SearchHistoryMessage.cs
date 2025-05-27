using System.ComponentModel.DataAnnotations;

namespace PubMedEdu.SearchApi.Models;

public class SearchHistoryMessage
{
    public string UserId { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public SearchResult Results { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}