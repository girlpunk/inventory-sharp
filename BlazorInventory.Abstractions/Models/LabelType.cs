namespace BlazorInventory.Abstractions.Models;

[Flags]
public enum LabelType
{
    None = 0,
    QR = 1,
    UHF = 2,
    RFID = 4,
    GS1 = 8,
}
