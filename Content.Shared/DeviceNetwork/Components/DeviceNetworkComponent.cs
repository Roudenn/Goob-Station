// SPDX-FileCopyrightText: 2021 Julian Giebel <j.giebel@netrocks.info>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Zachary Yona <58833995+Magicalus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Zachary Yona <magicalusf@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 PJB3005 <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeviceNetwork.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
//Goobstation - sync device lists / modules had to nuke explicit access
public sealed partial class DeviceNetworkComponent : Component
{
    [DataField("deviceNetId")]
    public DeviceNetIdDefaults NetIdEnum { get; set; }

    public int DeviceNetId => (int) NetIdEnum;

    /// <summary>
    ///     The frequency that this device is listening on.
    /// </summary>
    [DataField]
    public uint? ReceiveFrequency;

    /// <summary>
    ///     frequency prototype. Used to select a default frequency to listen to on. Used when the map is
    ///     initialized.
    /// </summary>
    [DataField]
    public ProtoId<DeviceFrequencyPrototype>? ReceiveFrequencyId;

    /// <summary>
    ///     The frequency that this device going to try transmit on.
    /// </summary>
    [DataField]
    public uint? TransmitFrequency;

    /// <summary>
    ///     frequency prototype. Used to select a default frequency to transmit on. Used when the map is
    ///     initialized.
    /// </summary>
    [DataField]
    public ProtoId<DeviceFrequencyPrototype>? TransmitFrequencyId;

    /// <summary>
    ///     The address of the device, either on the network it is currently connected to or whatever address it
    ///     most recently used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Address = string.Empty;

    /// <summary>
    ///     If true, the address was customized and should be preserved across networks. If false, a randomly
    ///     generated address will be created whenever this device connects to a network.
    /// </summary>
    [DataField]
    public bool CustomAddress;

    /// <summary>
    ///     Prefix to prepend to any automatically generated addresses. Helps players to identify devices. This gets
    ///     localized.
    /// </summary>
    [DataField]
    public string? Prefix;

    /// <summary>
    ///     Whether the device should listen for all device messages, regardless of the intended recipient.
    /// </summary>
    [DataField]
    public bool ReceiveAll;

    /// <summary>
    ///     If the device should show its address upon an examine. Useful for devices
    ///     that do not have a visible UI.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ExaminableAddress;

    /// <summary>
    ///     Whether the device should attempt to join the network on map init.
    /// </summary>
    [DataField]
    public bool AutoConnect = true;

    /// <summary>
    ///     Whether to send the broadcast recipients list to the sender so it can be filtered.
    /// <see cref="DeviceListSystem"/>
    /// </summary>
    [DataField]
    public bool SendBroadcastAttemptEvent;

    /// <summary>
    ///     Whether this device's address can be saved to device-lists
    /// </summary>
    [DataField]
    public bool SavableAddress = true;

    /// <summary>
    ///     A list of device-lists that this device is on.
    /// </summary>
    [DataField]
    // had to nuke explicit access for goob modules
    public HashSet<EntityUid> DeviceLists = new();

    /// <summary>
    ///     A list of configurators that this device is on.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> Configurators = new();
}
