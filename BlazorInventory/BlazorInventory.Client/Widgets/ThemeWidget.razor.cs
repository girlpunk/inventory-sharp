namespace BlazorInventory.Client.Widgets;

public partial class ThemeWidget
{
    private void ChangeTheme(string value)
    {
        ThemeService.SetTheme(value);
    }

    private void ChangeRightToLeft(bool value)
    {
        ThemeService.SetRightToLeft(value);
    }

    private void ChangeWcag(bool value)
    {
        ThemeService.SetWcag(value);
    }
}
