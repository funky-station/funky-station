using System.Collections.Frozen;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._Funkystation.Genetics.Systems;

public sealed class GeneticShuffleSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private const int ActiveBlocks = 150;
    private static ReadOnlySpan<char> Bases => "ATGC";
    private static ReadOnlySpan<char> Pairs => "TACG";
    private EntityUid _shuffledMutationsSingleton;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
    }

    private void OnRoundStarting(RoundStartingEvent _)
    {
        ShuffleMutations();
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New is GameRunLevel.PreRoundLobby or GameRunLevel.PostRound)
            ClearMutation();
    }

    private void ClearMutation()
    {
        if (EntityQuery<GeneticShuffleSingletonComponent>(true).FirstOrDefault() is { } comp)
        {
            comp.Clear();
            _entityManager.Dirty(comp.Owner, comp);
        }
    }

    private void ShuffleMutations()
    {
        var prototypes = _prototypeManager
            .EnumeratePrototypes<GeneticMutationPrototype>()
            .Select(p => p.ID)
            .ToList();

        if (prototypes.Count == 0)
            return;

        _random.Shuffle(prototypes);

        var blocks = Enumerable.Range(1, ActiveBlocks).ToList();
        _random.Shuffle(blocks);

        var mutation = new Dictionary<string, GeneticBlock>(prototypes.Count);
        var usedSequences = new HashSet<string>(prototypes.Count);

        for (var i = 0; i < prototypes.Count; i++)
        {
            var id = prototypes[i];
            var block = i < blocks.Count ? blocks[i] : -1;
            var sequence = GenerateUniqueSequence(usedSequences);
            mutation[id] = new GeneticBlock(block, sequence);
        }

        _shuffledMutationsSingleton = Spawn();

        var singleton = EnsureComp<GeneticShuffleSingletonComponent>(_shuffledMutationsSingleton);
        singleton.SetMutation(mutation);
        _entityManager.Dirty(singleton.Owner, singleton);
    }

    private string GenerateUniqueSequence(HashSet<string> used)
    {
        Span<char> buffer = stackalloc char[32];

        string seq;
        do
        {
            for (int i = 0; i < 32; i += 2)
            {
                var idx = _random.Next(4);
                buffer[i] = Bases[idx];
                buffer[i + 1] = Pairs[idx];
            }
            seq = new string(buffer);
        } while (!used.Add(seq));

        return seq;
    }



    public bool TryGetSlot(string mutationId, out GeneticBlock slot)
    {
        slot = GeneticBlock.Invalid;

        if (!TryComp<GeneticShuffleSingletonComponent>(_shuffledMutationsSingleton, out var comp))
            return false;

        return comp.Mutation.TryGetValue(mutationId, out slot);
    }

    public GeneticBlock GetSlot(string mutationId)
        => TryGetSlot(mutationId, out var slot) ? slot : GeneticBlock.Invalid;

    public int GetBlock(string mutationId)
        => TryGetSlot(mutationId, out var slot) ? slot.Block : -1;

    public string GetSequence(string mutationId)
        => TryGetSlot(mutationId, out var slot) ? slot.Sequence : string.Empty;

    public IReadOnlyDictionary<string, GeneticBlock> CurrentMutation()
    {
        if (TryComp<GeneticShuffleSingletonComponent>(_shuffledMutationsSingleton, out var comp))
            return comp.Mutation;

        return FrozenDictionary<string, GeneticBlock>.Empty;
    }
}
