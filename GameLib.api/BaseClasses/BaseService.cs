using GameLib.api.Infrastructure.Session;
using Supabase;

namespace GameLib.api.BaseClasses;

internal class BaseService
{
    #region Variables

    protected readonly Client _client;

    #endregion

    #region Constructor

    protected BaseService(SupabaseClientService supabaseService)
    {
        supabaseService.InitializeAsync().GetAwaiter().GetResult();
        supabaseService.AddAuthStateChangedListener();

        _client = supabaseService.Client;
    }

    #endregion
}