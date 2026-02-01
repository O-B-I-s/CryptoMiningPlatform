using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CryptoMining.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_Users_UserId",
                table: "Deposits");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMiningPlans_MiningPlans_MiningPlanId",
                table: "UserMiningPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMiningPlans_Users_UserId",
                table: "UserMiningPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Withdrawals_Users_UserId",
                table: "Withdrawals");

            migrationBuilder.DropColumn(
                name: "DailyReturnPercentage",
                table: "MiningPlans");

            migrationBuilder.RenameColumn(
                name: "RelatedMiningPlanId",
                table: "Transactions",
                newName: "RelatedEntityId");

            migrationBuilder.RenameColumn(
                name: "DurationDays",
                table: "MiningPlans",
                newName: "DurationValue");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Withdrawals",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "Transactions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceBefore",
                table: "Transactions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PerformedBy",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationUnit",
                table: "MiningPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnPercentage",
                table: "MiningPlans",
                type: "decimal(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MiningPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Deposits",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "MiningPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "DurationUnit", "DurationValue", "HashRate", "MaxDeposit", "MinDeposit", "Name", "ReturnPercentage", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 8, 55, 54, 246, DateTimeKind.Utc).AddTicks(9685), "Fast returns! Get 1% every 10 minutes for 2 hours.", 1, 10, 5m, 500m, 50m, "Quick Starter", 1.0m, null });

            migrationBuilder.UpdateData(
                table: "MiningPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "DurationUnit", "DurationValue", "HashRate", "MaxDeposit", "MinDeposit", "Name", "ReturnPercentage", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 8, 55, 54, 246, DateTimeKind.Utc).AddTicks(9693), "Earn 2.5% every hour for 24 hours.", 2, 1, 15m, 2000m, 100m, "Hourly Miner", 2.5m, null });

            migrationBuilder.UpdateData(
                table: "MiningPlans",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "DurationUnit", "DurationValue", "HashRate", "MaxDeposit", "MinDeposit", "Name", "ReturnPercentage", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 8, 55, 54, 246, DateTimeKind.Utc).AddTicks(9699), "Steady 5% daily returns for 30 days.", 3, 1, 50m, 10000m, 500m, "Daily Grind", 5.0m, null });

            migrationBuilder.InsertData(
                table: "MiningPlans",
                columns: new[] { "Id", "CreatedAt", "Description", "DurationUnit", "DurationValue", "HashRate", "IsActive", "MaxDeposit", "MinDeposit", "Name", "ReturnPercentage", "UpdatedAt" },
                values: new object[,]
                {
                    { 4, new DateTime(2026, 1, 28, 8, 55, 54, 246, DateTimeKind.Utc).AddTicks(9704), "15% weekly returns for 4 weeks.", 4, 1, 100m, true, 50000m, 1000m, "Weekly Wealth", 15.0m, null },
                    { 5, new DateTime(2026, 1, 28, 8, 55, 54, 246, DateTimeKind.Utc).AddTicks(9709), "Premium plan with 50% monthly returns for 3 months.", 5, 1, 500m, true, 100000m, 5000m, "Monthly Master", 50.0m, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "IsAdmin", "LastLoginAt", "PasswordHash", "UpdatedAt", "Username", "WalletBalance" },
                values: new object[] { 1, new DateTime(2026, 1, 28, 8, 55, 54, 246, DateTimeKind.Utc).AddTicks(8595), "admin@cryptomining.com", true, true, null, "$2a$11$GgVJ.2TIr2sgvnNqZvBSteiUwnbe/p.76i0zZum2jy2QoWh3JhN.C", null, "admin", 0m });

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_Users_UserId",
                table: "Deposits",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMiningPlans_MiningPlans_MiningPlanId",
                table: "UserMiningPlans",
                column: "MiningPlanId",
                principalTable: "MiningPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMiningPlans_Users_UserId",
                table: "UserMiningPlans",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Withdrawals_Users_UserId",
                table: "Withdrawals",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_Users_UserId",
                table: "Deposits");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMiningPlans_MiningPlans_MiningPlanId",
                table: "UserMiningPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMiningPlans_Users_UserId",
                table: "UserMiningPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Withdrawals_Users_UserId",
                table: "Withdrawals");

            migrationBuilder.DeleteData(
                table: "MiningPlans",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MiningPlans",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BalanceBefore",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PerformedBy",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DurationUnit",
                table: "MiningPlans");

            migrationBuilder.DropColumn(
                name: "ReturnPercentage",
                table: "MiningPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MiningPlans");

            migrationBuilder.RenameColumn(
                name: "RelatedEntityId",
                table: "Transactions",
                newName: "RelatedMiningPlanId");

            migrationBuilder.RenameColumn(
                name: "DurationValue",
                table: "MiningPlans",
                newName: "DurationDays");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Withdrawals",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyReturnPercentage",
                table: "MiningPlans",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Deposits",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.UpdateData(
                table: "MiningPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DailyReturnPercentage", "Description", "DurationDays", "HashRate", "MaxDeposit", "MinDeposit", "Name" },
                values: new object[] { new DateTime(2026, 1, 28, 5, 58, 44, 543, DateTimeKind.Utc).AddTicks(5130), 1.5m, "Perfect for beginners. Start mining with minimal investment.", 30, 10m, 999m, 100m, "Starter" });

            migrationBuilder.UpdateData(
                table: "MiningPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DailyReturnPercentage", "Description", "DurationDays", "HashRate", "MaxDeposit", "MinDeposit", "Name" },
                values: new object[] { new DateTime(2026, 1, 28, 5, 58, 44, 543, DateTimeKind.Utc).AddTicks(5146), 2.5m, "For serious miners looking for better returns.", 60, 50m, 9999m, 1000m, "Professional" });

            migrationBuilder.UpdateData(
                table: "MiningPlans",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DailyReturnPercentage", "Description", "DurationDays", "HashRate", "MaxDeposit", "MinDeposit", "Name" },
                values: new object[] { new DateTime(2026, 1, 28, 5, 58, 44, 543, DateTimeKind.Utc).AddTicks(5151), 4.0m, "Maximum mining power for maximum profits.", 90, 200m, 100000m, 10000m, "Enterprise" });

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_Users_UserId",
                table: "Deposits",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMiningPlans_MiningPlans_MiningPlanId",
                table: "UserMiningPlans",
                column: "MiningPlanId",
                principalTable: "MiningPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMiningPlans_Users_UserId",
                table: "UserMiningPlans",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Withdrawals_Users_UserId",
                table: "Withdrawals",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
