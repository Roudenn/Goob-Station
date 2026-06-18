using Content.Shared.Configurable;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.DeviceNetwork.Systems;
using Content.Shared.Disposal.Components;
using Content.Shared.Disposal.Unit;
using Content.Shared.Disposal.Unit.Events;
using Content.Shared.Interaction;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Player;

namespace Content.Shared.Disposal.Mailing;

public abstract class SharedMailingUnitSystem : EntitySystem
{
    [Dependency] private readonly SharedDeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UserInterfaceSystem = default!;

    private const string MailTag = "mail";
    private const string TagConfigurationKey = "tag";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MailingUnitComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<MailingUnitComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<MailingUnitComponent, BeforeDisposalFlushEvent>(OnBeforeFlush);
        SubscribeLocalEvent<MailingUnitComponent, ConfigurationUpdatedEvent>(OnConfigurationUpdated);
        SubscribeLocalEvent<MailingUnitComponent, ActivateInWorldEvent>(HandleActivate, before: new[] { typeof(SharedDisposalUnitSystem) });
        SubscribeLocalEvent<MailingUnitComponent, TargetSelectedMessage>(OnTargetSelected);
    }

    private void OnComponentInit(EntityUid uid, MailingUnitComponent component, ComponentInit args)
    {
        UpdateTargetList(uid, component);
    }

    private void OnPacketReceived(EntityUid uid, MailingUnitComponent component, ref DeviceNetworkPacketEvent args)
    {
        if (!_power.IsPowered(uid))
            return;

        switch (args.Data)
        {
            case MailRequestTagPayload:
                SendTagRequestResponse(uid, args, component.Tag);
                break;
            case MailTagPayload payload:
                //Add the received tag request response to the list of targets
                component.TargetList.Add(payload.Tag);
                Dirty(uid, component);
                break;
        }
    }

    /// <summary>
    /// Sends the given tag as a response to a <see cref="MailRequestTagPayload"/> if it's not null
    /// </summary>
    private void SendTagRequestResponse(EntityUid uid, DeviceNetworkPacketEvent args, string? tag)
    {
        if (tag == null)
            return;

        var payload = new MailTagPayload
        {
            Tag = tag,
        };

        _deviceNetworkSystem.QueuePacket(uid, args.Address, payload, args.Frequency);
    }

    /// <summary>
    /// Prevents the unit from flushing if no target is selected
    /// </summary>
    private void OnBeforeFlush(EntityUid uid, MailingUnitComponent component, BeforeDisposalFlushEvent args)
    {
        if (string.IsNullOrEmpty(component.Target))
        {
            args.Cancel();
            return;
        }

        Dirty(uid, component);
        args.Tags.Add(MailTag);
        args.Tags.Add(component.Target);

        BroadcastSentMessage(uid, component);
    }

    /// <summary>
    /// Broadcast that a mail was sent including the src and target tags
    /// </summary>
    private void BroadcastSentMessage(EntityUid uid, MailingUnitComponent component, DeviceNetworkComponent? device = null)
    {
        if (string.IsNullOrEmpty(component.Tag) || string.IsNullOrEmpty(component.Target) || !Resolve(uid, ref device))
            return;

        var payload = new MailSendPayload
        {
            Tag = component.Tag,
            Target = component.Target
        };

        _deviceNetworkSystem.QueuePacket(uid, null, payload, null, null, device);
    }

    /// <summary>
    /// Clears the units target list and broadcasts a <see cref="MailRequestTagPayload"/>.
    /// The target list will then get populated with <see cref="MailTagPayload"/> responses from all active mailing units on the same grid
    /// </summary>
    private void UpdateTargetList(EntityUid uid, MailingUnitComponent component, DeviceNetworkComponent? device = null)
    {
        if (!Resolve(uid, ref device, false))
            return;

        var payload = new MailRequestTagPayload();
        component.TargetList.Clear();
        _deviceNetworkSystem.QueuePacket(uid, null, payload, null, null, device);
    }

    /// <summary>
    /// Gets called when the units tag got updated
    /// </summary>
    private void OnConfigurationUpdated(EntityUid uid, MailingUnitComponent component, ConfigurationUpdatedEvent args)
    {
        var configuration = args.Configuration.Config;
        if (!configuration.ContainsKey(TagConfigurationKey) || configuration[TagConfigurationKey] == string.Empty)
        {
            component.Tag = null;
            return;
        }

        component.Tag = configuration[TagConfigurationKey];
        Dirty(uid, component);
    }

    private void HandleActivate(EntityUid uid, MailingUnitComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (!TryComp(args.User, out ActorComponent? actor))
        {
            return;
        }

        args.Handled = true;
        UpdateTargetList(uid, component);
        UserInterfaceSystem.OpenUi(uid, MailingUnitUiKey.Key, actor.PlayerSession);
    }

    private void OnTargetSelected(EntityUid uid, MailingUnitComponent component, TargetSelectedMessage args)
    {
        component.Target = args.Target;
        Dirty(uid, component);
    }
}
