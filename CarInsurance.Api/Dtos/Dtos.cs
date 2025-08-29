using CarInsurance.Api.Models;

namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);

public record ClaimRequestDto(DateOnly ClaimDate, string? Description, decimal Amount);
public record ClaimResponseDto(long Id, DateOnly ClaimDate, string? Description, decimal Amount);

public record CarHistoryEntryDto(long Id, string Type, DateOnly Date, string? Description, decimal? Amount, string? Provider, DateOnly? EndDate);

public record CarHistoryDto(long CarId, string Vin, string? Model, int YearOfmanufacture, List<CarHistoryEntryDto> History);
