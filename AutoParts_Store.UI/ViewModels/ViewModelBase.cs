using AutoPartsStore.Data;
using Avalonia.Controls;
using ReactiveUI;

namespace AutoParts_Store.UI.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected static AutoPartsStoreQueries _queriesService;
    protected static AutoPartsStoreTables _tablesService;

    public ViewModelBase()
    {
        if (!Design.IsDesignMode && _queriesService == null)
        {
            throw new System.Exception("Сначала необходимо инициализировать базу данных");
        }
    }

    public static void Initialize(AutoPartsStoreQueries queriesRealization, AutoPartsStoreTables tablesService)
    {
        _queriesService = queriesRealization;
        _tablesService = tablesService;
    }
}
