using Content.Shared._Funkystation.Medical.SmartFridge;
using Content.Shared.FixedPoint;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._Funkystation.Medical.SmartFridge.UI;

public sealed class SmartFridgeItemUIController : UIController
{
    public void SendSmartFridgeEjectMessage(EntityUid uid, string item, FixedPoint2 itemsToEject)
    {
        EntityManager.RaisePredictiveEvent(new EjectItemMessage(EntityManager.GetNetEntity(uid), item, itemsToEject));
    }
}
