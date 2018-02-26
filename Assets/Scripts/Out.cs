/// <summary>
/// Helper class given to worker threads to notify the parent thread when a return value is ready.
/// The parent thread instantiates this class and gives it to the worker thread which populates its
/// fields.
/// </summary>
/// <typeparam name="T">Type of the return value</typeparam>
public class Out<T>
{
    /// <summary>
    /// Return value produced by the worker thread
    /// </summary>
    public T Value; 
    
    /// <summary>
    /// Thread safe flag indicating the return value is ready to be read
    /// </summary>
    public volatile bool Done = false;
}
