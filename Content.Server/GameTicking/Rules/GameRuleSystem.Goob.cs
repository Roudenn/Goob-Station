using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Maps;
using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server.GameTicking.Rules;

public abstract partial class GameRuleSystem<T> where T: IComponent
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    protected bool TryFindRandomTileOnStation(Entity<StationDataComponent> station,
        out Vector2i tile,
        out EntityUid targetGrid,
        out EntityCoordinates targetCoords)
    {
        tile = default;
        targetCoords = EntityCoordinates.Invalid;
        targetGrid = EntityUid.Invalid;

        if (GetStationMainGrid(station.Comp) is not { } grid)
            return false;

        targetGrid = grid.Owner;
        return TryFindTileOnGrid(grid, out tile, out targetCoords);
    }

    protected Entity<MapGridComponent>? GetStationMainGrid(StationDataComponent station)
    {
        if ((station.Grids.FirstOrNull(HasComp<BecomesStationComponent>) ?? _station.GetLargestGrid(station.Owner)) is not //todo goobstation station.owner obsolete patchup
            { } grid || !TryComp(grid, out MapGridComponent? gridComp))
            return null;

        return (grid, gridComp);
    }

    protected bool TryFindTileOnGrid(Entity<MapGridComponent> grid,
        out Vector2i tile,
        out EntityCoordinates targetCoords,
        int tries = 10)
    {
        tile = default;
        targetCoords = EntityCoordinates.Invalid;

        var aabb = grid.Comp.LocalAABB;

        for (var i = 0; i < tries; i++)
        {
            var randomX = RobustRandom.Next((int) aabb.Left, (int) aabb.Right);
            var randomY = RobustRandom.Next((int) aabb.Bottom, (int) aabb.Top);

            tile = new Vector2i(randomX, randomY);

            if (!_map.TryGetTile(grid.Comp, tile, out var selectedTile) || selectedTile.IsEmpty ||
                _turf.IsSpace(selectedTile))
                continue;

            if (_atmosphere.IsTileSpace(grid.Owner, Transform(grid.Owner).MapUid, tile)
                || _atmosphere.IsTileAirBlocked(grid.Owner, tile, mapGridComp: grid.Comp))
                continue;

            targetCoords = _map.GridTileToLocal(grid.Owner, grid.Comp, tile);
            return true;
        }

        return false;
    }

    protected bool CheckStationMember(EntityUid? grid, EntityUid? station, bool isGlobal = false)
    {
        return TryComp(grid, out StationMemberComponent? stationMemberComp)
               && (stationMemberComp.EventsEnabled || isGlobal)
               && stationMemberComp.Station == station;
    }
}
