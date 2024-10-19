namespace Darp.Utils.Dialog.Helper;

using System.ComponentModel;
using System.Linq.Expressions;

internal static class Extensions
{
    public static IObservable<TResult> WhenPropertyChanged<TObject, TProperty, TResult>(
        this TObject changed,
        Expression<Func<TObject, TProperty>> expression,
        Func<TProperty, TResult> selector
    )
        where TObject : INotifyPropertyChanged
    {
        var name = (expression.Body as MemberExpression)?.Member.Name;
        ArgumentNullException.ThrowIfNull(name);
        Func<TObject, TProperty> func = expression.Compile();
        TProperty initialValue = func(changed);
        var simpleSubject = new SimpleBehaviorSubject<TResult>(selector(initialValue));
        changed.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != name || sender is not TObject obj)
            {
                return;
            }
            TProperty value = func(obj);
            simpleSubject.OnNext(selector(value));
        };
        return simpleSubject;
    }

    public static IObservable<TProperty> WhenPropertyChanged<TObject, TProperty>(
        this TObject changed,
        Expression<Func<TObject, TProperty>> expression
    )
        where TObject : INotifyPropertyChanged
    {
        return changed.WhenPropertyChanged<TObject, TProperty, TProperty>(expression, x => x);
    }
}
