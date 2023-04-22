using System.Diagnostics;
using System.Text.Json;

namespace FiorSearchService.Interfaces;

/// <summary>
/// Class converted item from 1C 8.3
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public record class ItemPosition {
    public Guid Guid { get; init; }
    public string Article { get; init; }
    public string FullName { get; init; }
    public string Description { get; init; }
    public Dictionary<string, string> AdditionalInfo { get; set; } //Rus localization ಥ_ಥ

    public ItemPosition(Guid guid, string article, string fullName, string description, Dictionary<string, string> additionalInfo) {
        Guid = guid; Article = article; FullName = fullName; Description = description; AdditionalInfo = additionalInfo;
    }

    private string GetDebuggerDisplay() {
        return JsonSerializer.Serialize(this);
    }
}