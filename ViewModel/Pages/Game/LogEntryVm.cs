namespace MyriaRPG.ViewModel.Pages.Game
{
    /// <summary>A single entry in the room log panel. Errors are rendered in red.</summary>
    public record LogEntryVm(string Message, bool IsError = false);
}
