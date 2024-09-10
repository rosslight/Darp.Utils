namespace Darp.Utils.Dialog.Helper;

internal sealed class SimpleSubject<T> : IObservable<T>
{
    private readonly List<IObserver<T>> _observers = [];

    public void OnNext(T next)
    {
        foreach (IObserver<T> observer in _observers)
        {
            observer.OnNext(next);
        }
    }

    public void OnError(Exception error)
    {
        foreach (IObserver<T> observer in _observers)
        {
            observer.OnError(error);
        }
    }

    public void OnCompleted()
    {
        foreach (IObserver<T> observer in _observers)
        {
            observer.OnCompleted();
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        return new CallbackDisposable<(SimpleSubject<T>, IObserver<T>)>((this, observer), subject =>
        {
            subject.Item1._observers.Remove(subject.Item2);
        });
    }
}