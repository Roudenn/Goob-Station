// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceNetwork.Systems;
using Content.Server.Power.Components;
using Content.Shared.DeviceNetwork.Events;

namespace Content.Server.SensorMonitoring;

public sealed class BatterySensorSystem : EntitySystem
{
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BatterySensorComponent, DeviceNetworkPacketEvent>(PacketReceived);
    }

    private void PacketReceived(Entity<BatterySensorComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        switch (args.Data)
        {
            case BatterySensorRequestPayload:
                var battery = Comp<BatteryComponent>(ent);
                var netBattery = Comp<PowerNetworkBatteryComponent>(ent);

                var payload = new BatterySensorSyncPayload
                {
                    Data = new BatterySensorData(
                        battery.CurrentCharge,
                        battery.MaxCharge,
                        netBattery.CurrentReceiving,
                        netBattery.MaxChargeRate,
                        netBattery.CurrentSupply,
                        netBattery.MaxSupply),
                };

                _deviceNetwork.QueuePacket(ent, args.SenderAddress, payload);
                break;
        }
    }
}
