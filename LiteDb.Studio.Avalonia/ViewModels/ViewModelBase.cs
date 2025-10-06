using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class ViewModelBase : ReactiveObject
{
    // Optional: helper similar to the commented F# RaiseAndSet
    // public void RaiseAndSet<T>(ref T oldValue, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    // {
    //     this.RaiseAndSetIfChanged(ref oldValue, newValue, propertyName);
    // }
}