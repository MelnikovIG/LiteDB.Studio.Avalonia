namespace LiteDb.Studio.Avalonia.Infra;

public class Res<T>
{
    public T Value { get; }
    public Exception Error { get; }
    public bool IsValid => Error == null;

    public Res(T value, Exception error)
    {
        Value = value;
        Error = error;
    }
}

public static class Rop
{
    public static Res<T> Ok<T>(T value) =>
        new Res<T>(value, null);

    public static Res<T> Failed<T>(Exception exc) =>
        new Res<T>(default!, exc);

    public static Res<U> Bind<T, U>(Func<T, Res<U>> fn, Res<T> res)
    {
        if (!res.IsValid)
            return Failed<U>(res.Error);

        try
        {
            return fn(res.Value);
        }
        catch (Exception ex)
        {
            return Failed<U>(ex);
        }
    }

    public static Res<T> Run1<S, T>(Func<S, T> fn, S arg1)
    {
        try
        {
            return Ok(fn(arg1));
        }
        catch (Exception ex)
        {
            return Failed<T>(ex);
        }
    }

    public static Res<T> Run<T>(Func<T> fn) =>
        Run1(_ => fn(), default(object));

    public static Res<T> Run2<A, B, T>(Func<A, B, T> fn, A arg1, B arg2) =>
        Run1((ValueTuple<A, B> ab) => fn(ab.Item1, ab.Item2), (arg1, arg2));

    public static Res<T> Run3<A, B, C, T>(Func<A, B, C, T> fn, A arg1, B arg2, C arg3) =>
        Run1((ValueTuple<A, B, C> abc) => fn(abc.Item1, abc.Item2, abc.Item3), (arg1, arg2, arg3));

    public static Res<T> Inspect<T>(Action<T> onOk, Action<Exception> onFailed, Res<T> res)
    {
        try
        {
            if (res.IsValid)
                onOk(res.Value);
            else
                onFailed(res.Error);

            return res;
        }
        catch (Exception ex)
        {
            return Failed<T>(ex);
        }
    }

    public static Res<T> Log<T>(Action<T> onOk, Action<Exception> onFailed, Res<T> res) =>
        Inspect(onOk, onFailed, res);

    public static Res<U> Map<T, U>(Func<T, U> fn, Res<T> res)
    {
        if (!res.IsValid)
            return Failed<U>(res.Error);

        try
        {
            return Ok(fn(res.Value));
        }
        catch (Exception ex)
        {
            return Failed<U>(ex);
        }
    }

    /// <summary>
    /// Если ошибка — пробуем трансформировать Exception в результат.
    /// Если успех — возвращаем исходное значение.
    /// </summary>
    public static Res<T> TryMapErr<T>(Func<Exception, T> fn, Res<T> res)
    {
        if (res.IsValid)
            return res;

        try
        {
            return Ok(fn(res.Error));
        }
        catch (Exception ex)
        {
            return Failed<T>(ex);
        }
    }

    public static void Finish<T>(Action<T> f, Res<T> res)
    {
        if (res.IsValid)
        {
            try
            {
                f(res.Value);
            }
            catch
            {
                // Игнорируем, как в F#
            }
        }
    }
}