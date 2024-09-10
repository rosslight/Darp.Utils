namespace Darp.Utils.Dialog.Helper;

using System.ComponentModel;
using System.Linq.Expressions;

internal static class Extensions
{
    public static IObservable<TResult> WhenPropertyChanged<TObject, TProperty, TResult>(this TObject changed,
        Expression<Func<TObject, TProperty>> expression,
        Func<TProperty, TResult> selector)
        where TObject : INotifyPropertyChanged
    {
        Func<TObject, TProperty> func = expression.Compile();
        var simpleSubject = new SimpleSubject<TResult>();
        changed.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != expression.Name || sender is not TObject obj)
            {
                return;
            }
            TProperty value = func(obj);
            simpleSubject.OnNext(selector(value));
        };
        return simpleSubject;
    }
}
