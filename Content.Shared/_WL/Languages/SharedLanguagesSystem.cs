using Content.Shared._WL.Languages.Components;
using Content.Shared._WL.Languages.Components;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Languages;

public abstract class SharedLanguagesSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager _prototype = default!;
    [Dependency] protected readonly SharedGameTicker _ticker = default!;
    [Dependency] private readonly IEntityManager _ent = default!;

    public LanguagePrototype? GetLanguagePrototype(ProtoId<LanguagePrototype> id)
    {
        _prototype.TryIndex(id, out var proto);
        return proto;
    }

    public string ObfuscateMessage(string message, ProtoId<LanguagePrototype> language)
    {
        var proto = GetLanguagePrototype(language);

        if (proto == null)
        {
            return message;
        }
        else
        {
            var obfus = proto.Obfuscation.Obfuscate(message, _ticker.RoundId);
            return obfus;
        }
    }

    public bool TryChangeLanguage(NetEntity netEnt, ProtoId<LanguagePrototype> protoid)
    {
        if (!_ent.TryGetEntity(netEnt, out var ent))
            return false;

        if (!TryComp<LanguagesSpeekingComponent>(ent, out var comp))
            return false;
        Logger.Debug("TryChangeLanguage");
        if (!comp.UnderstandingLanguages.Contains(protoid))
            return false;

        var ev = new LanguageChangeEvent(netEnt, protoid);
        RaiseNetworkEvent(ev);
        RaiseLocalEvent(ent.Value, ev);
        Logger.Debug("Ev 1");

        var ev2 = new LanguagesInfoEvent(netEnt, (string)protoid, comp.SpeekingLanguages, comp.UnderstandingLanguages);
        RaiseNetworkEvent(ev2);
        Logger.Debug("Ev 2");

        return true;
    }

    public void OnLanguageChange(EntityUid entity, string language)
    {
        if (!TryComp<LanguagesSpeekingComponent>(entity, out var component))
            return;

        component.CurrentLanguage = language;
        Dirty(entity, component);

        var netEntity = GetNetEntity(entity);
        var ev = new LanguagesInfoEvent(netEntity, language, component.SpeekingLanguages, component.UnderstandingLanguages);
        RaiseNetworkEvent(ev);
    }

    [Serializable, NetSerializable]
    public sealed class LanguagesInfoEvent : EntityEventArgs
    {
        public readonly NetEntity NetEntity;
        public readonly string CurrentLanguage;
        public readonly List<ProtoId<LanguagePrototype>> SpeekingLanguages;
        public readonly List<ProtoId<LanguagePrototype>> UnderstandingLanguages;

        public LanguagesInfoEvent(NetEntity netEntity, string current, List<ProtoId<LanguagePrototype>> speeking, List<ProtoId<LanguagePrototype>> understand)
        {
            NetEntity = netEntity;
            CurrentLanguage = current;
            SpeekingLanguages = speeking;
            UnderstandingLanguages = understand;
        }
    }
}
