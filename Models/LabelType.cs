namespace InventorySharp.Models;

[Flags]
public enum LabelType
{
    QR = 1,
    UHF = 2,
    RFID = 4,
    GS1 = 8,
}
