using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Timer
{
    private readonly Stopwatch _watch = new Stopwatch();

    public Timer()
    {
        Start();
    }

    public void Start()
    {
        _watch.Start();
    }
        
    public void Stop(string msg)
    {
        _watch.Stop();
        Debug.Log(msg + ": " + _watch.Elapsed.TotalMilliseconds);
    }
}