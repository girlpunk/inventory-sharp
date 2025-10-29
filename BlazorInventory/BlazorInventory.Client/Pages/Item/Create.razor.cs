using System.ComponentModel.DataAnnotations;
using ActualLab.CommandR;
using BlazorInventory.Abstractions.Command;
using BlazorInventory.Abstractions.Models;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Pages.Item;

public sealed partial class Create
{
    /// <summary>
    /// Scanner being used at the time of creating the item
    /// </summary>
    [SupplyParameterFromQuery(Name = "scanner")]
    public Guid? ScannerId { get; set; }

    /// <summary>
    /// Label type that was scanned, in URL compatible format
    /// </summary>
    [SupplyParameterFromQuery(Name = "type")]
    public int? LabelTypeRaw
    {
        get => (int?)LabelType;
        set => LabelType = (LabelType?)value;
    }

    /// <summary>
    /// Label type that was scanned
    /// </summary>
    private LabelType? LabelType { get; set; }

    /// <summary>
    /// Identifier from the label that was scanned
    /// </summary>
    [SupplyParameterFromQuery(Name = "identifier")]
    public string? Identifier { get; set; }

    /// <summary>
    /// Latitude from scan
    /// </summary>
    [SupplyParameterFromQuery(Name = "latitude")]
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude from scan
    /// </summary>
    [SupplyParameterFromQuery(Name = "longitude")]
    public double? Longitude { get; set; }

    private CreateModel Model { get; } = new();

    /// <summary>
    /// DTO for the item being created
    /// </summary>
    private sealed class CreateModel
    {
        // Item stuff
        /// <summary>
        /// Name of the item
        /// </summary>
        [MaxLength(120)]
        public string? Name { get; set; }

        /// <summary>
        /// Description of the item
        /// </summary>
        [MaxLength(1024)]
        public string Description { get; set; } = "";

        /// <summary>
        /// ID of the item's parent, or null for no parent
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Identifier shown on the label
        /// </summary>
        [MaxLength(256)]
        public string Identifier { get; set; } = "";

        /// <summary>
        /// What type of label this is
        /// </summary>
        public LabelType? LabelType { get; set; }

        /// <summary>
        /// ID of the foreign server the item belongs to, or null for no foreign server
        /// </summary>
        public Guid? ForeignServerId { get; set; }
    }

    private async Task Submit(CreateModel model)
    {
        //Create Item
        var item = await Commander.Call(new CreateCommand<Abstractions.Models.Item>
        {
            Obj = new Abstractions.Models.Item
            {
                Created = DateTime.Now,
                Description = model.Description,
                Name = model.Name,
                ParentId = model.ParentId,
            }
        }).ConfigureAwait(false);

        //Create Label
        //TODO: Generate Identifier
        if(model.LabelType != null && model.Identifier != null)
        {
            var label = await Commander.Call(new CreateCommand<ItemLabel>
            {
                Obj = new ItemLabel
                {
                    Identifier = model.Identifier,
                    LabelType = model.LabelType.Value,
                    ItemId = item.Id,
                    ForeignServerId = model.ForeignServerId,
                    Created = DateTime.Now,
                }
            }).ConfigureAwait(false);

            //Create Scan
            var scan = await Commander.Call(new ScanLabelCommand
            {
                LabelId = label.Id,
                ScannerId = ScannerId,
                LabelType = model.LabelType.Value,
                //TODO: Make optional
                CreateScanRecord = true,
                ScannerLatitude = Latitude,
                ScannerLongitude = Longitude
            }).ConfigureAwait(false);

            //Go to scan page
            NavigationManager.NavigateTo($"/Scan/{scan.Scan!.Id}");
        }

        //Go to item page
        NavigationManager.NavigateTo($"/Item/{item.Id}");
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync().ConfigureAwait(false);

        if (LabelType != null)
            Model.LabelType = LabelType.Value;

        if (Identifier != null)
        {
            if (Identifier.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                Identifier.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                //https://r.lu.gl/a/A808AZ

                var url = new Uri(Identifier, UriKind.Absolute);

                Model.Identifier = Identifier;

                var foreignServer = await ForeignServerService.Find(url.Host).ConfigureAwait(false);
                if (foreignServer != null)
                {
                    Model.ForeignServerId = foreignServer;
                    Model.Identifier = url.AbsolutePath;
                }
            }
            else
            {
                Model.Identifier = Identifier;
            }
        }
    }

    private async Task ConvertNamespace()
    {
        var url = new Uri(Model.Identifier, UriKind.Absolute);

        var foreignServer = await Commander.Call(new CreateCommand<ForeignServer>
        {
            Obj = new ForeignServer
            {
                Namespace = url.Host
            }
        }).ConfigureAwait(false);

        Model.ForeignServerId = foreignServer.Id;
        Model.Identifier = url.AbsolutePath;
    }
}
