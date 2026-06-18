// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 vulppine <vulppine@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.Atmos.Monitor;
using Content.Shared.Atmos.Monitor.Components;
using Content.Shared.DeviceNetwork;

namespace Content.Server.Atmos.Monitor.Systems;

/// <summary>
///     Generic device network commands useful for atmos devices,
///     as well as some helper commands.
/// </summary>
public sealed class AtmosDeviceNetworkSystem : EntitySystem
{
    [Dependency] private readonly DeviceNetworkSystem _deviceNet = default!;

    public void Register(EntityUid uid, string? address)
    {
        var registerPayload = new AtmosRegisterDevicePayload();
        _deviceNet.QueuePacket(uid, address, registerPayload);
    }

    public void Deregister(EntityUid uid, string? address)
    {
        var deregisterPayload = new AtmosDeregisterDevicePayload();
        _deviceNet.QueuePacket(uid, address, deregisterPayload);
    }

    public void Sync(EntityUid uid, string? address)
    {
        var syncPayload = new AtmosSyncDevicePayload();
        _deviceNet.QueuePacket(uid, address, syncPayload);
    }

    public void SetDeviceState(EntityUid uid, string address, AtmosDeviceDataPayload dataPayload)
    {
        var payload = new AtmosDeviceSetDataPayload
        {
            Data = dataPayload,
        };

        _deviceNet.QueuePacket(uid, address, payload);
    }
}
