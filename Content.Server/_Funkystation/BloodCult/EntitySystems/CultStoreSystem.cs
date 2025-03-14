using System.Threading;
using Content.Shared.FixedPoint;
using Content.Server.Store.Systems;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Server.BloodCult.Components;

namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class CultStoreSystem : EntitySystem
{
	[Dependency] private readonly StoreSystem _storeSystem = default!;

	[ValidatePrototypeId<CurrencyPrototype>]
    private const string CultMoney = "CultPoint";

	private readonly ReaderWriterLockSlim _pointsChange = new();

	public override void Initialize()
	{
		base.Initialize();
	}

	public void RefreshStores(float frameTime)
	{
		var stores = GetStores();
		foreach (var store in stores)
		{
			store.Comp1.TimeElapsed += frameTime;
			if (store.Comp1.TimeElapsed >= store.Comp1.TimeUntilRecharge)
			{
				RechargeStore(store, 1);
				store.Comp1.TimeElapsed = 0.0f;
			}
		}
	}

	private List<Entity<CultStoreComponent, StoreComponent>> GetStores()
	{
		var storesList = new List<Entity<CultStoreComponent, StoreComponent>>();
        var stores = AllEntityQuery<CultStoreComponent, StoreComponent>();
        while (stores.MoveNext(out var uid, out var cultStoreComp, out var storeComp))
        {
			if (storeComp.Balance[CultMoney] < cultStoreComp.MaximumEnergy)
				storesList.Add((uid, cultStoreComp, storeComp));
        }
        return storesList;
	}

	private bool RechargeStore(Entity<CultStoreComponent, StoreComponent> storeEnt, FixedPoint2 amount)
	{
		if (!_pointsChange.TryEnterWriteLock(1000))
            return false;

		if (_storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>
                {
                    { CultMoney, amount }
                },
                storeEnt,
                storeEnt.Comp2))
        {
            //UpdateAllAlerts(store);

            _pointsChange.ExitWriteLock();
            return true;
        }

		_pointsChange.ExitWriteLock();
		return false;
	}
}
