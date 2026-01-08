namespace SkyDrop.Events;

public class OnExceptionEventArgs : EventArgs
{
    public OnExceptionEventArgs(string message, Exception exception)
    {
        this.Message = message;
        this.Exception = exception;
    }

    public string Message { get; }

    public Exception Exception { get; }
}