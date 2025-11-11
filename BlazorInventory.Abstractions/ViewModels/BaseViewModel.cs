namespace BlazorInventory.Abstractions.ViewModels;

public abstract record BaseViewModel
{
    /// <summary>
    /// Unique Identifier
    /// </summary>
    public Guid Id { get; set; }
}
