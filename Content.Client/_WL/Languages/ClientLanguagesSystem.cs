using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Languages;

public sealed partial class ClientLanguagesSystem : SharedLanguagesSystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<LanguagesInfoEvent>(OnLanguagesInfoEvent);
        SubscribeNetworkEvent<LanguageChangeEvent>(OnGlobalLanguageChange);

        SubscribeLocalEvent<LanguagesSpeekingComponent, LanguageChangeEvent>(OnLocalLanguageChange);
    }

    public event Action<LanguagesData>? OnLanguagesUpdate;

    public List<LanguagePrototype>? GetSpeekingLanguages(EntityUid entity)
    {
        if (!TryComp<LanguagesSpeekingComponent>(entity, out var comp))
            return null;

        var prototypes = new List<LanguagePrototype>();
        foreach (ProtoId<LanguagePrototype>protoid in comp.SpeekingLanguages)
        {
            var proto = GetLanguagePrototype(protoid);
            if (proto == null)
                continue;
            prototypes.Add(proto);
        }

        if (prototypes.Count == 0)
            return null;
        return prototypes;
    }

    public void OnLocalLanguageChange(EntityUid entity, LanguagesSpeekingComponent comp, ref LanguageChangeEvent args)
    {
        OnLanguageChange(entity, (string)args.Language);
    }

    public void OnGlobalLanguageChange(LanguageChangeEvent msg, EntitySessionEventArgs args)
    {
        var entity = GetEntity(msg.Entity);
        OnLanguageChange(entity, (string)msg.Language);
    }

    private void OnLanguagesInfoEvent(LanguagesInfoEvent msg, EntitySessionEventArgs args)
    {
        var entity = GetEntity(msg.NetEntity);
        var data = new LanguagesData(entity, msg.CurrentLanguage, msg.SpeekingLanguages, msg.UnderstandingLanguages);

        OnLanguagesUpdate?.Invoke(data);
    }
}

public readonly record struct LanguagesData(
    EntityUid Entity,
    string CurrentLanguage,
    List<ProtoId<LanguagePrototype>> SpeekingLanguages,
    List<ProtoId<LanguagePrototype>> UnderstandingLanguages
);
