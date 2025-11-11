namespace BlazorInventory.Client.Pages;

public partial class Index
{
    /// <inheritdoc />
    protected override async Task<Counts> ComputeState(CancellationToken cancellationToken) =>
        new()
        {
            Items = await ItemService.Count(cancellationToken),
            Labels = await LabelService.Count(cancellationToken),
            Scans = await ScanService.Count(cancellationToken),
            //var momentsAgo = await Time.GetMomentsAgo(changeTime);
            // return count; // $"{count}, changed {momentsAgo}";
        };

    /// <summary>
    /// Counts for each type of item
    /// </summary>
    public class Counts
    {
        /// <summary>
        /// Number of Items
        /// </summary>
        public int Items { get; init; }

        /// <summary>
        /// Number of Labels
        /// </summary>
        public int Labels { get; init; }

        /// <summary>
        /// Number of Scans
        /// </summary>
        public int Scans { get; init; }
    }
}
