using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarInsurance.Api.Tests.ServicesTests
{
    public class CarServiceFacts : IDisposable
    {
        private readonly AppDbContext _db;
        private readonly CarService _service;
        public CarServiceFacts()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _db = new AppDbContext(options);
            _db.Database.OpenConnection();
            _db.Database.EnsureCreated();
            SeedData.EnsureSeeded(_db);

            _service = new CarService(_db);
        }

        // Tests for IsInsuranceValidAsync method
        [Fact]
        public async Task IsInsuranceValidAsync_ShouldReturnTrue_WhenInsuranceActive()
        {
            var carId = 1;
            var date = new DateOnly(2025, 1, 10);

            var result = await _service.IsInsuranceValidAsync(carId, date);

            Assert.True(result);
        }

        [Fact]
        public async Task IsInsuranceValidAsync_ShouldReturnFalse_DateBeforeStartDate()
        {
            var carId = 1;
            var date = new DateOnly(2023, 12, 31);

            var result = await _service.IsInsuranceValidAsync(carId, date);

            Assert.False(result);
        }

        [Fact]
        public async Task IsInsuranceValidAsync_ShouldReturnFalse_DateAfterEndDate()
        {
            var carId = 1;
            var date = new DateOnly(2026, 1, 1);

            var result = await _service.IsInsuranceValidAsync(carId, date);

            Assert.False(result);
        }

        [Fact]
        public async Task IsInsuranceValidAsync_ShouldReturnTrue_WhenIsStartDate()
        {
            var carId = 1;
            var date = new DateOnly(2024, 1, 1);

            var result = await _service.IsInsuranceValidAsync(carId, date);

            Assert.True(result);
        }

        [Fact]
        public async Task IsInsuranceValidAsync_ShouldReturnTrue_WhenIsEndDate()
        {
            var carId = 1;
            var date = new DateOnly(2024, 12, 31);

            var result = await _service.IsInsuranceValidAsync(carId, date);

            Assert.True(result);
        }

        // Tests for RegisterClaimAsync method

        [Fact]
        public async Task RegisterClaimAsync_ShouldRegisterClaim_WhenCarExists()
        {
            var carId = 1;
            var claimRequest = new ClaimRequestDto(
                ClaimDate: new DateOnly(2025, 1, 1),
                Description: "Test claim",
                Amount: 1000m
            );

            var result = await _service.RegisterClaimAsync(carId, claimRequest);

            Assert.NotNull(result);
            Assert.Equal(claimRequest.ClaimDate, result.ClaimDate);
            Assert.Equal(claimRequest.Amount, result.Amount);
            Assert.Equal(claimRequest.Description, result.Description);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task RegisterClaimAsync_ShouldThrowKeyNotFoundException_WhenCarNotExist()
        {
            var carId = 50;
            var claimRequest = new ClaimRequestDto(
                ClaimDate: new DateOnly(2025, 1, 1),
                Description: "Test claim",
                Amount: 1000m
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.RegisterClaimAsync(carId, claimRequest));
        }

        [Fact]
        public async Task RegisterClaimAsync_ShouldThrowArgumentException_WhenDateIsInFuture()
        {
            var carId = 1;
            var claimRequest = new ClaimRequestDto(
                ClaimDate: new DateOnly(2500, 1, 1),
                Description: "Test claim",
                Amount: 1000m
            );

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.RegisterClaimAsync(carId, claimRequest));

            Assert.Equal("Claim date cannot be in the future", ex.Message);
        }

        [Fact]
        public async Task RegisterClaimAsync_ShouldThrowArgumentException_WhenAmountIs0()
        {
            var carId = 1;
            var claimRequest = new ClaimRequestDto(
                ClaimDate: new DateOnly(2025, 1, 1),
                Description: "Test claim",
                Amount: 0m
            );

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.RegisterClaimAsync(carId, claimRequest));

            Assert.Equal("Amount cannot be 0 or negative", ex.Message);
        }

        [Fact]
        public async Task RegisterClaimAsync_ShouldThrowArgumentException_WhenAmountIsNegative()
        {
            var carId = 1;
            var claimRequest = new ClaimRequestDto(
                ClaimDate: new DateOnly(2025, 1, 1),
                Description: "Test claim",
                Amount: -50m
            );

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.RegisterClaimAsync(carId, claimRequest));

            Assert.Equal("Amount cannot be 0 or negative", ex.Message);
        }

        // Tests for GetCarHistoryAsync method

        [Fact]
        public async Task GetCarHistoryAsync_ShouldReturnHistory_WhenCarExists()
        {
            var carId = 1;
            var result = await _service.GetCarHistoryAsync(carId);

            Assert.NotNull(result);
            Assert.Equal(carId, result.CarId);
            Assert.NotNull(result.Vin);
            Assert.NotEmpty(result.History);
        }

        [Fact]
        public async Task GetCarHistory_ShouldThrowKeyNotFoundException_WhenCarNotExist()
        {
            var carId = 100;

            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.GetCarHistoryAsync(carId));
        }

        public void Dispose()
        {
            _db.Dispose();
        }

    }
}
