using System.ComponentModel;
using ArchAnalyzer.ViewModels;
using Microsoft.AspNetCore.Components;

namespace ArchAnalyzer.Pages;

public abstract class ViewModelComponentBase<T> : ComponentBase, IDisposable
    where T : ComponentBase
{
    [Inject]
    public ViewModelBase<T> ViewModel { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.PropertyChanged += OnStateChanged;
        ViewModel.OnAttach(this);
    }

    public virtual void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        ViewModel.PropertyChanged -= OnStateChanged;
        OnDispose();
    }

    protected virtual void OnDispose()
    {
    }
}