// SPDX-FileCopyrightText: 2021 Julian Giebel <j.giebel@netrocks.info>
// SPDX-FileCopyrightText: 2021 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceNetwork.Components.Devices;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.Interaction;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Payloads;

namespace Content.Server.DeviceNetwork.Systems.Devices
{
    public sealed class ApcNetSwitchSystem : EntitySystem
    {
        [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ApcNetSwitchComponent, InteractHandEvent>(OnInteracted);
            SubscribeLocalEvent<ApcNetSwitchComponent, DeviceNetworkPacketEvent>(OnPackedReceived);
        }

        /// <summary>
        /// Toggles the state of the switch and sents a <see cref="DeviceNetworkConstants.CmdSetState"/> command with the
        /// <see cref="DeviceNetworkConstants"/> value set to state.
        /// </summary>
        private void OnInteracted(Entity<ApcNetSwitchComponent> ent, ref InteractHandEvent args)
        {
            var (uid, component) = ent;
            if (!TryComp(uid, out DeviceNetworkComponent? networkComponent))
                return;

            component.State = !component.State;

            if (networkComponent.TransmitFrequency == null)
                return;

            var payload = new TogglePayload
            {
                Enabled = component.State,
            };

            _deviceNetworkSystem.QueuePacket(uid, null, payload);

            args.Handled = true;
        }

        /// <summary>
        /// Listens to the <see cref="DeviceNetworkConstants.CmdSetState"/> command of other switches to sync state
        /// </summary>
        private void OnPackedReceived(Entity<ApcNetSwitchComponent> ent, ref DeviceNetworkPacketEvent args)
        {
            var (uid, component) = ent;
            if (!TryComp(uid, out DeviceNetworkComponent? networkComponent) || args.SenderAddress == networkComponent.Address) return;
            if (args.Data is not TogglePayload toggle) return;

            component.State = toggle.Enabled;
        }
    }
}
