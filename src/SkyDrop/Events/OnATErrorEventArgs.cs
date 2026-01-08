using FishyFlip.Models;

namespace SkyDrop.Events;

public class OnATErrorEventArgs : EventArgs
{
    public OnATErrorEventArgs(string message, ATError exception)
    {
        this.Message = message;
        this.Exception = exception;
    }

    public string Message { get; }

    public ATError Exception { get; }
}