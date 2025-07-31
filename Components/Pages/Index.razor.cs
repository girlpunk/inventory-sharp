namespace InventorySharp.Components.Pages;

public partial class Index
{
    protected override async Task<int> ComputeState(CancellationToken cancellationToken)
    {
        var count = await ItemService.Count();
        //var momentsAgo = await Time.GetMomentsAgo(changeTime);
        return count; // $"{count}, changed {momentsAgo}";
    }
}
