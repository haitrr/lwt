namespace Lwt.Models;

public record TermEditedEventData(TermSnapShot Before, TermSnapShot After)
{
    public TermSnapShot Before = Before;
    public TermSnapShot After = After;
}