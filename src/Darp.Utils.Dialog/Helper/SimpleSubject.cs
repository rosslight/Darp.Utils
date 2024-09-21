namespace Darp.Utils.Dialog.Helper;

internal sealed class SimpleSubject<T> : IObservable<T>
{
    private readonly List<IObserver<T>> _observers = [];

    public void OnNext(T next)
    {
        // Use reversed for loop to account for the fact that observers might be removed on next
        for (var index = _observers.Count - 1; index >= 0; index--)
        {
            IObserver<T> observer = _observers[index];
            observer.OnNext(next);
        }
    }

    public void OnError(Exception error)
    {
        // Use reversed for loop to account for the fact that observers might be removed on error
        for (var index = _observers.Count - 1; index >= 0; index--)
        {
            IObserver<T> observer = _observers[index];
            observer.OnError(error);
        }
    }

    public void OnCompleted()
    {
        // Use reversed for loop to account for the fact that observers might be removed on completion
        for (var index = _observers.Count - 1; index >= 0; index--)
        {
            IObserver<T> observer = _observers[index];
            observer.OnCompleted();
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        return new CallbackDisposable<(SimpleSubject<T>, IObserver<T>)>((this, observer), subject => subject.Item1._observers.Remove(subject.Item2));
    }
}
