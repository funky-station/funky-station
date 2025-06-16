using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.Payouts;

public sealed class CharacterBalanceManager : IPostInjectInit
{
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    public void PostInject()
    {
        // _userDb.AddOnLoadPlayer(OnPlayerConnected);
        // _userDb.AddOnPlayerDisconnect(OnPlayerDisconnected);
    }

    // private Task OnPlayerConnected(ICommonSession session, CancellationToken cancel)
    // {
    //
    // }
    //
    // private Task OnPlayerDisconnected(ICommonSession session)
    // {
    //
    // }
}
