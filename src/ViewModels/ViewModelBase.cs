using System.ComponentModel;
using System.Runtime.CompilerServices;
using ArchAnalyzer.Pages;
using Microsoft.AspNetCore.Components;

namespace ArchAnalyzer.ViewModels;

public class ViewModelBase<T> : INotifyPropertyChanged
    where T : ComponentBase
{
    private ViewModelComponentBase<T>? _componentBase;
    public void OnAttach(ViewModelComponentBase<T> componentBase)
    {
        _componentBase = componentBase ?? throw new ArgumentNullException(nameof(componentBase));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RaisePropertyChangeAsync([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                await OnPropertyChangeAsync(propertyName);
                _componentBase?.OnStateChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                // TODO: Better logging.
                Console.WriteLine(ex.ToString());
            }
        });
    }

    protected virtual Task OnPropertyChangeAsync(string propertyName)
    {
        return Task.CompletedTask;
    }
}
