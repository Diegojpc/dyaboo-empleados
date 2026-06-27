using MediatR;

namespace Dyaboo.Application.Features.WMS.GetWarehouseStatus;

public record GetWarehouseStatusQuery : IRequest<WarehouseStatusResult>;

public record WarehouseStatusResult(
    int TotalLocations,
    int ActiveLocations,
    int OccupiedLocations,
    int TotalCapacity,
    int TotalStockUnits,
    double OccupancyPercentage,
    IReadOnlyList<AisleStatus> Aisles);

public record AisleStatus(
    string AisleCode,
    int LocationCount,
    int TotalCapacity,
    int CurrentStock,
    double OccupancyPercentage,
    IReadOnlyList<LocationStatus> Locations);

public record LocationStatus(
    string LocationCode,
    int Shelf,
    int Level,
    int Capacity,
    int CurrentStock,
    int AvailableSpace,
    double OccupancyPercentage,
    IReadOnlyList<string> SkusPresent);
