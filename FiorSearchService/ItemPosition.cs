using System.Diagnostics;
using System.Text.Json;

namespace FiorSearchService;

/// <summary>
/// Class converted item from 1C 8.3
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public record class ItemPosition {
    public Guid Guid { get; init; }
    public String Article { get; init; }
    public String FullName { get; init; }
    public String Description { get; init; }
    public Dictionary<String, String> AdditionalInfo { get; set; } //Rus localization ಥ_ಥ

    public ItemPosition(Guid guid, string article, string fullName, string description, Dictionary<String,String> additionalInfo) {
        this.Guid = guid; this.Article = article; this.FullName = fullName; this.Description = description; this.AdditionalInfo = additionalInfo;
    }

    private string GetDebuggerDisplay() {
        return JsonSerializer.Serialize(this);
    }
}