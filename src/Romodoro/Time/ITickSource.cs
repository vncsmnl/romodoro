namespace Romodoro.Time;

public interface ITickSource
{
    event EventHandler? Tick;
    void Start();
    void StopTicking();
}
