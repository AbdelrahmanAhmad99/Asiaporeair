using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AsiaporeairDbTesting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aircraft_type",
                columns: table => new
                {
                    type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    manufacturer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    range_km = table.Column<int>(type: "int", nullable: true),
                    max_seats = table.Column<int>(type: "int", nullable: true),
                    cargo_capacity = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    cruising_velocity = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_type", x => x.type_id);
                });

            migrationBuilder.CreateTable(
                name: "ancillary_product",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    base_cost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    unit_of_measure = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ancillary_product", x => x.product_id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    LastName = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    Address = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastLogin = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UserType = table.Column<string>(type: "NVARCHAR(50)", maxLength: 50, nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contextual_pricing_attributes",
                columns: table => new
                {
                    attribute_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    time_until_departure = table.Column<int>(type: "int", nullable: true),
                    length_of_stay = table.Column<int>(type: "int", nullable: true),
                    competitor_fares = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    willingness_to_pay = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contextual_pricing_attributes", x => x.attribute_id);
                });

            migrationBuilder.CreateTable(
                name: "country",
                columns: table => new
                {
                    iso_code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    continent_fk = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_country", x => x.iso_code);
                });

            migrationBuilder.CreateTable(
                name: "fare_basis_code",
                columns: table => new
                {
                    code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rules = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fare_basis_code", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "frequent_flyer",
                columns: table => new
                {
                    flyer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    card_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    award_points = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frequent_flyer", x => x.flyer_id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    date_of_hire = table.Column<DateTime>(type: "DATE", nullable: true),
                    salary = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    shift_preference_fk = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_employee_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "airport",
                columns: table => new
                {
                    iata_code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    icao_code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    country_fk = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    altitude = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airport", x => x.iata_code);
                    table.ForeignKey(
                        name: "FK_airport_country_country_fk",
                        column: x => x.country_fk,
                        principalTable: "country",
                        principalColumn: "iso_code");
                });

            migrationBuilder.CreateTable(
                name: "price_offer_log",
                columns: table => new
                {
                    offer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_fk = table.Column<int>(type: "int", nullable: true),
                    offer_price_quote = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    context_attributes_fk = table.Column<int>(type: "int", nullable: false),
                    fare_fk = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ancillary_fk = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_offer_log", x => x.offer_id);
                    table.ForeignKey(
                        name: "FK_price_offer_log_ancillary_product_ancillary_fk",
                        column: x => x.ancillary_fk,
                        principalTable: "ancillary_product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_price_offer_log_contextual_pricing_attributes_context_attributes_fk",
                        column: x => x.context_attributes_fk,
                        principalTable: "contextual_pricing_attributes",
                        principalColumn: "attribute_id");
                    table.ForeignKey(
                        name: "FK_price_offer_log_fare_basis_code_fare_fk",
                        column: x => x.fare_fk,
                        principalTable: "fare_basis_code",
                        principalColumn: "code");
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    frequent_flyer_fk = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    kris_flyer_tier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_frequent_flyer_frequent_flyer_fk",
                        column: x => x.frequent_flyer_fk,
                        principalTable: "frequent_flyer",
                        principalColumn: "flyer_id");
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AddedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AppUserId);
                    table.ForeignKey(
                        name: "FK_Admins_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Admins_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Admins_employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employee",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "SuperAdmins",
                columns: table => new
                {
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdmins", x => x.AppUserId);
                    table.ForeignKey(
                        name: "FK_SuperAdmins_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuperAdmins_employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employee",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "Supervisors",
                columns: table => new
                {
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AddedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ManagedArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supervisors", x => x.AppUserId);
                    table.ForeignKey(
                        name: "FK_Supervisors_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Supervisors_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Supervisors_employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employee",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "airline",
                columns: table => new
                {
                    iata_code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    callsign = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    operating_region = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    base_airport_fk = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airline", x => x.iata_code);
                    table.ForeignKey(
                        name: "FK_airline_airport_base_airport_fk",
                        column: x => x.base_airport_fk,
                        principalTable: "airport",
                        principalColumn: "iata_code");
                });

            migrationBuilder.CreateTable(
                name: "crew_member",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    crew_base_airport_fk = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    position = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_member", x => x.employee_id);
                    table.ForeignKey(
                        name: "FK_crew_member_airport_crew_base_airport_fk",
                        column: x => x.crew_base_airport_fk,
                        principalTable: "airport",
                        principalColumn: "iata_code");
                    table.ForeignKey(
                        name: "FK_crew_member_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "route",
                columns: table => new
                {
                    route_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    origin_airport_fk = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    destination_airport_fk = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    distance_km = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route", x => x.route_id);
                    table.ForeignKey(
                        name: "FK_route_airport_destination_airport_fk",
                        column: x => x.destination_airport_fk,
                        principalTable: "airport",
                        principalColumn: "iata_code");
                    table.ForeignKey(
                        name: "FK_route_airport_origin_airport_fk",
                        column: x => x.origin_airport_fk,
                        principalTable: "airport",
                        principalColumn: "iata_code");
                });

            migrationBuilder.CreateTable(
                name: "passenger",
                columns: table => new
                {
                    passenger_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_fk = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    passport_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passenger", x => x.passenger_id);
                    table.ForeignKey(
                        name: "FK_passenger_user_User_fk",
                        column: x => x.User_fk,
                        principalTable: "user",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "aircraft",
                columns: table => new
                {
                    tail_number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    airline_fk = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    aircraft_type_fk = table.Column<int>(type: "int", nullable: false),
                    total_flight_hours = table.Column<int>(type: "int", nullable: true),
                    acquisition_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft", x => x.tail_number);
                    table.ForeignKey(
                        name: "FK_aircraft_aircraft_type_aircraft_type_fk",
                        column: x => x.aircraft_type_fk,
                        principalTable: "aircraft_type",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aircraft_airline_airline_fk",
                        column: x => x.airline_fk,
                        principalTable: "airline",
                        principalColumn: "iata_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attendant",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendant", x => x.employee_id);
                    table.ForeignKey(
                        name: "FK_attendant_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_attendant_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_attendant_crew_member_employee_id",
                        column: x => x.employee_id,
                        principalTable: "crew_member",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "certification",
                columns: table => new
                {
                    cert_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    crew_member_fk = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    issue_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certification", x => x.cert_id);
                    table.ForeignKey(
                        name: "FK_certification_crew_member_crew_member_fk",
                        column: x => x.crew_member_fk,
                        principalTable: "crew_member",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "pilot",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    license_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    total_flight_hours = table.Column<int>(type: "int", nullable: true),
                    type_rating_fk = table.Column<int>(type: "int", nullable: false),
                    last_sim_check_date = table.Column<DateTime>(type: "DATE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AircraftTypeTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pilot", x => x.employee_id);
                    table.ForeignKey(
                        name: "FK_pilot_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_pilot_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_pilot_aircraft_type_AircraftTypeTypeId",
                        column: x => x.AircraftTypeTypeId,
                        principalTable: "aircraft_type",
                        principalColumn: "type_id");
                    table.ForeignKey(
                        name: "FK_pilot_aircraft_type_type_rating_fk",
                        column: x => x.type_rating_fk,
                        principalTable: "aircraft_type",
                        principalColumn: "type_id");
                    table.ForeignKey(
                        name: "FK_pilot_crew_member_employee_id",
                        column: x => x.employee_id,
                        principalTable: "crew_member",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "flight_schedule",
                columns: table => new
                {
                    schedule_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    flight_no = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    route_fk = table.Column<int>(type: "int", nullable: false),
                    airline_fk = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    aircraft_type_fk = table.Column<int>(type: "int", nullable: false),
                    departure_time_scheduled = table.Column<DateTime>(type: "datetime2", nullable: false),
                    arrival_time_scheduled = table.Column<DateTime>(type: "datetime2", nullable: false),
                    days_of_week = table.Column<byte>(type: "tinyint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_schedule", x => x.schedule_id);
                    table.ForeignKey(
                        name: "FK_flight_schedule_aircraft_type_aircraft_type_fk",
                        column: x => x.aircraft_type_fk,
                        principalTable: "aircraft_type",
                        principalColumn: "type_id");
                    table.ForeignKey(
                        name: "FK_flight_schedule_airline_airline_fk",
                        column: x => x.airline_fk,
                        principalTable: "airline",
                        principalColumn: "iata_code");
                    table.ForeignKey(
                        name: "FK_flight_schedule_route_route_fk",
                        column: x => x.route_fk,
                        principalTable: "route",
                        principalColumn: "route_id");
                });

            migrationBuilder.CreateTable(
                name: "route_operator",
                columns: table => new
                {
                    route_fk = table.Column<int>(type: "int", nullable: false),
                    airline_fk = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    codeshare_status = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_operator", x => new { x.route_fk, x.airline_fk });
                    table.ForeignKey(
                        name: "FK_route_operator_airline_airline_fk",
                        column: x => x.airline_fk,
                        principalTable: "airline",
                        principalColumn: "iata_code");
                    table.ForeignKey(
                        name: "FK_route_operator_route_route_fk",
                        column: x => x.route_fk,
                        principalTable: "route",
                        principalColumn: "route_id");
                });

            migrationBuilder.CreateTable(
                name: "aircraft_config",
                columns: table => new
                {
                    config_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    aircraft_fk = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    configuration_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    total_seats_count = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_config", x => x.config_id);
                    table.ForeignKey(
                        name: "FK_aircraft_config_aircraft_aircraft_fk",
                        column: x => x.aircraft_fk,
                        principalTable: "aircraft",
                        principalColumn: "tail_number");
                });

            migrationBuilder.CreateTable(
                name: "flight_instance",
                columns: table => new
                {
                    instance_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_fk = table.Column<int>(type: "int", nullable: false),
                    aircraft_fk = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    scheduled_dep_ts = table.Column<DateTime>(type: "datetime2", nullable: false),
                    actual_dep_ts = table.Column<DateTime>(type: "datetime2", nullable: true),
                    scheduled_arr_ts = table.Column<DateTime>(type: "datetime2", nullable: false),
                    actual_arr_ts = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_instance", x => x.instance_id);
                    table.ForeignKey(
                        name: "FK_flight_instance_aircraft_aircraft_fk",
                        column: x => x.aircraft_fk,
                        principalTable: "aircraft",
                        principalColumn: "tail_number");
                    table.ForeignKey(
                        name: "FK_flight_instance_flight_schedule_schedule_fk",
                        column: x => x.schedule_fk,
                        principalTable: "flight_schedule",
                        principalColumn: "schedule_id");
                });

            migrationBuilder.CreateTable(
                name: "flight_leg_def",
                columns: table => new
                {
                    leg_def_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_fk = table.Column<int>(type: "int", nullable: false),
                    segment_number = table.Column<int>(type: "int", nullable: false),
                    departure_airport_fk = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    arrival_airport_fk = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_leg_def", x => x.leg_def_id);
                    table.ForeignKey(
                        name: "FK_flight_leg_def_airport_arrival_airport_fk",
                        column: x => x.arrival_airport_fk,
                        principalTable: "airport",
                        principalColumn: "iata_code");
                    table.ForeignKey(
                        name: "FK_flight_leg_def_airport_departure_airport_fk",
                        column: x => x.departure_airport_fk,
                        principalTable: "airport",
                        principalColumn: "iata_code");
                    table.ForeignKey(
                        name: "FK_flight_leg_def_flight_schedule_schedule_fk",
                        column: x => x.schedule_fk,
                        principalTable: "flight_schedule",
                        principalColumn: "schedule_id");
                });

            migrationBuilder.CreateTable(
                name: "cabin_class",
                columns: table => new
                {
                    cabin_class_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    config_fk = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cabin_class", x => x.cabin_class_id);
                    table.ForeignKey(
                        name: "FK_cabin_class_aircraft_config_config_fk",
                        column: x => x.config_fk,
                        principalTable: "aircraft_config",
                        principalColumn: "config_id");
                });

            migrationBuilder.CreateTable(
                name: "booking",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_fk = table.Column<int>(type: "int", nullable: false),
                    flight_instance_fk = table.Column<int>(type: "int", nullable: false),
                    booking_ref = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    booking_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    price_total = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    payment_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    fare_basis_code_fk = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PointsAwarded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_booking_fare_basis_code_fare_basis_code_fk",
                        column: x => x.fare_basis_code_fk,
                        principalTable: "fare_basis_code",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "FK_booking_flight_instance_flight_instance_fk",
                        column: x => x.flight_instance_fk,
                        principalTable: "flight_instance",
                        principalColumn: "instance_id");
                    table.ForeignKey(
                        name: "FK_booking_user_User_fk",
                        column: x => x.User_fk,
                        principalTable: "user",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "flight_crew",
                columns: table => new
                {
                    flight_instance_fk = table.Column<int>(type: "int", nullable: false),
                    crew_member_fk = table.Column<int>(type: "int", nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_crew", x => new { x.flight_instance_fk, x.crew_member_fk });
                    table.ForeignKey(
                        name: "FK_flight_crew_crew_member_crew_member_fk",
                        column: x => x.crew_member_fk,
                        principalTable: "crew_member",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK_flight_crew_flight_instance_flight_instance_fk",
                        column: x => x.flight_instance_fk,
                        principalTable: "flight_instance",
                        principalColumn: "instance_id");
                });

            migrationBuilder.CreateTable(
                name: "seat",
                columns: table => new
                {
                    seat_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    aircraft_fk = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    seat_number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    cabin_class_fk = table.Column<int>(type: "int", nullable: false),
                    is_window = table.Column<bool>(type: "bit", nullable: true),
                    is_exit_row = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seat", x => x.seat_id);
                    table.ForeignKey(
                        name: "FK_seat_aircraft_aircraft_fk",
                        column: x => x.aircraft_fk,
                        principalTable: "aircraft",
                        principalColumn: "tail_number");
                    table.ForeignKey(
                        name: "FK_seat_cabin_class_cabin_class_fk",
                        column: x => x.cabin_class_fk,
                        principalTable: "cabin_class",
                        principalColumn: "cabin_class_id");
                });

            migrationBuilder.CreateTable(
                name: "ancillary_sale",
                columns: table => new
                {
                    sale_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_fk = table.Column<int>(type: "int", nullable: false),
                    product_fk = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    price_paid = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    segment_fk = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ancillary_sale", x => x.sale_id);
                    table.ForeignKey(
                        name: "FK_ancillary_sale_ancillary_product_product_fk",
                        column: x => x.product_fk,
                        principalTable: "ancillary_product",
                        principalColumn: "product_id");
                    table.ForeignKey(
                        name: "FK_ancillary_sale_booking_booking_fk",
                        column: x => x.booking_fk,
                        principalTable: "booking",
                        principalColumn: "booking_id");
                    table.ForeignKey(
                        name: "FK_ancillary_sale_flight_leg_def_segment_fk",
                        column: x => x.segment_fk,
                        principalTable: "flight_leg_def",
                        principalColumn: "leg_def_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_fk = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    transaction_datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_payment_booking_booking_fk",
                        column: x => x.booking_fk,
                        principalTable: "booking",
                        principalColumn: "booking_id");
                });

            migrationBuilder.CreateTable(
                name: "booking_passenger",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    passenger_id = table.Column<int>(type: "int", nullable: false),
                    seat_assignment_fk = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_passenger", x => new { x.booking_id, x.passenger_id });
                    table.ForeignKey(
                        name: "FK_booking_passenger_booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "booking",
                        principalColumn: "booking_id");
                    table.ForeignKey(
                        name: "FK_booking_passenger_passenger_passenger_id",
                        column: x => x.passenger_id,
                        principalTable: "passenger",
                        principalColumn: "passenger_id");
                    table.ForeignKey(
                        name: "FK_booking_passenger_seat_seat_assignment_fk",
                        column: x => x.seat_assignment_fk,
                        principalTable: "seat",
                        principalColumn: "seat_id");
                });

            migrationBuilder.CreateTable(
                name: "ticket",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PassengerId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    FlightInstanceId = table.Column<int>(type: "int", nullable: false),
                    SeatId = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    FrequentFlyerId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_ticket_booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_flight_instance_FlightInstanceId",
                        column: x => x.FlightInstanceId,
                        principalTable: "flight_instance",
                        principalColumn: "instance_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ticket_frequent_flyer_FrequentFlyerId",
                        column: x => x.FrequentFlyerId,
                        principalTable: "frequent_flyer",
                        principalColumn: "flyer_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ticket_passenger_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "passenger",
                        principalColumn: "passenger_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ticket_seat_SeatId",
                        column: x => x.SeatId,
                        principalTable: "seat",
                        principalColumn: "seat_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "boarding_pass",
                columns: table => new
                {
                    pass_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_passenger_booking_id = table.Column<int>(type: "int", nullable: false),
                    booking_passenger_passenger_id = table.Column<int>(type: "int", nullable: false),
                    seat_fk = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    boarding_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    precheck_status = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boarding_pass", x => x.pass_id);
                    table.ForeignKey(
                        name: "FK_boarding_pass_booking_passenger_booking_passenger_booking_id_booking_passenger_passenger_id",
                        columns: x => new { x.booking_passenger_booking_id, x.booking_passenger_passenger_id },
                        principalTable: "booking_passenger",
                        principalColumns: new[] { "booking_id", "passenger_id" });
                    table.ForeignKey(
                        name: "FK_boarding_pass_seat_seat_fk",
                        column: x => x.seat_fk,
                        principalTable: "seat",
                        principalColumn: "seat_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_AddedById",
                table: "Admins",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_EmployeeId",
                table: "Admins",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_aircraft_type_fk",
                table: "aircraft",
                column: "aircraft_type_fk");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_airline_fk",
                table: "aircraft",
                column: "airline_fk");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_config_aircraft_fk",
                table: "aircraft_config",
                column: "aircraft_fk");

            migrationBuilder.CreateIndex(
                name: "IX_airline_base_airport_fk",
                table: "airline",
                column: "base_airport_fk");

            migrationBuilder.CreateIndex(
                name: "IX_airport_country_fk",
                table: "airport",
                column: "country_fk");

            migrationBuilder.CreateIndex(
                name: "IX_airport_icao_code",
                table: "airport",
                column: "icao_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ancillary_sale_booking_fk",
                table: "ancillary_sale",
                column: "booking_fk");

            migrationBuilder.CreateIndex(
                name: "IX_ancillary_sale_product_fk",
                table: "ancillary_sale",
                column: "product_fk");

            migrationBuilder.CreateIndex(
                name: "IX_ancillary_sale_segment_fk",
                table: "ancillary_sale",
                column: "segment_fk");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_attendant_AddedById",
                table: "attendant",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_attendant_AppUserId",
                table: "attendant",
                column: "AppUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_boarding_pass_booking_passenger_booking_id_booking_passenger_passenger_id",
                table: "boarding_pass",
                columns: new[] { "booking_passenger_booking_id", "booking_passenger_passenger_id" });

            migrationBuilder.CreateIndex(
                name: "IX_boarding_pass_seat_fk",
                table: "boarding_pass",
                column: "seat_fk");

            migrationBuilder.CreateIndex(
                name: "IX_booking_fare_basis_code_fk",
                table: "booking",
                column: "fare_basis_code_fk");

            migrationBuilder.CreateIndex(
                name: "IX_booking_flight_instance_fk",
                table: "booking",
                column: "flight_instance_fk");

            migrationBuilder.CreateIndex(
                name: "IX_booking_User_fk",
                table: "booking",
                column: "User_fk");

            migrationBuilder.CreateIndex(
                name: "IX_booking_passenger_passenger_id",
                table: "booking_passenger",
                column: "passenger_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_passenger_seat_assignment_fk",
                table: "booking_passenger",
                column: "seat_assignment_fk");

            migrationBuilder.CreateIndex(
                name: "IX_cabin_class_config_fk",
                table: "cabin_class",
                column: "config_fk");

            migrationBuilder.CreateIndex(
                name: "IX_certification_crew_member_fk",
                table: "certification",
                column: "crew_member_fk");

            migrationBuilder.CreateIndex(
                name: "IX_crew_member_crew_base_airport_fk",
                table: "crew_member",
                column: "crew_base_airport_fk");

            migrationBuilder.CreateIndex(
                name: "IX_employee_AppUserId",
                table: "employee",
                column: "AppUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flight_crew_crew_member_fk",
                table: "flight_crew",
                column: "crew_member_fk");

            migrationBuilder.CreateIndex(
                name: "IX_flight_instance_aircraft_fk",
                table: "flight_instance",
                column: "aircraft_fk");

            migrationBuilder.CreateIndex(
                name: "IX_flight_instance_schedule_fk",
                table: "flight_instance",
                column: "schedule_fk");

            migrationBuilder.CreateIndex(
                name: "IX_flight_leg_def_arrival_airport_fk",
                table: "flight_leg_def",
                column: "arrival_airport_fk");

            migrationBuilder.CreateIndex(
                name: "IX_flight_leg_def_departure_airport_fk",
                table: "flight_leg_def",
                column: "departure_airport_fk");

            migrationBuilder.CreateIndex(
                name: "IX_flight_leg_def_schedule_fk",
                table: "flight_leg_def",
                column: "schedule_fk");

            migrationBuilder.CreateIndex(
                name: "IX_flight_schedule_aircraft_type_fk",
                table: "flight_schedule",
                column: "aircraft_type_fk");

            migrationBuilder.CreateIndex(
                name: "IX_flight_schedule_airline_fk",
                table: "flight_schedule",
                column: "airline_fk");

            migrationBuilder.CreateIndex(
                name: "IX_flight_schedule_route_fk",
                table: "flight_schedule",
                column: "route_fk");

            migrationBuilder.CreateIndex(
                name: "IX_passenger_User_fk",
                table: "passenger",
                column: "User_fk");

            migrationBuilder.CreateIndex(
                name: "IX_payment_booking_fk",
                table: "payment",
                column: "booking_fk");

            migrationBuilder.CreateIndex(
                name: "IX_pilot_AddedById",
                table: "pilot",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_pilot_AircraftTypeTypeId",
                table: "pilot",
                column: "AircraftTypeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_pilot_AppUserId",
                table: "pilot",
                column: "AppUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pilot_type_rating_fk",
                table: "pilot",
                column: "type_rating_fk");

            migrationBuilder.CreateIndex(
                name: "IX_price_offer_log_ancillary_fk",
                table: "price_offer_log",
                column: "ancillary_fk");

            migrationBuilder.CreateIndex(
                name: "IX_price_offer_log_context_attributes_fk",
                table: "price_offer_log",
                column: "context_attributes_fk");

            migrationBuilder.CreateIndex(
                name: "IX_price_offer_log_fare_fk",
                table: "price_offer_log",
                column: "fare_fk");

            migrationBuilder.CreateIndex(
                name: "IX_route_destination_airport_fk",
                table: "route",
                column: "destination_airport_fk");

            migrationBuilder.CreateIndex(
                name: "IX_route_origin_airport_fk",
                table: "route",
                column: "origin_airport_fk");

            migrationBuilder.CreateIndex(
                name: "IX_route_operator_airline_fk",
                table: "route_operator",
                column: "airline_fk");

            migrationBuilder.CreateIndex(
                name: "IX_seat_aircraft_fk",
                table: "seat",
                column: "aircraft_fk");

            migrationBuilder.CreateIndex(
                name: "IX_seat_cabin_class_fk",
                table: "seat",
                column: "cabin_class_fk");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdmins_EmployeeId",
                table: "SuperAdmins",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_AddedById",
                table: "Supervisors",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_EmployeeId",
                table: "Supervisors",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticket_BookingId",
                table: "ticket",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_FlightInstanceId",
                table: "ticket",
                column: "FlightInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_FrequentFlyerId",
                table: "ticket",
                column: "FrequentFlyerId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_PassengerId",
                table: "ticket",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_SeatId",
                table: "ticket",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_TicketCode",
                table: "ticket",
                column: "TicketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_AppUserId",
                table: "user",
                column: "AppUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_frequent_flyer_fk",
                table: "user",
                column: "frequent_flyer_fk");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "ancillary_sale");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "attendant");

            migrationBuilder.DropTable(
                name: "boarding_pass");

            migrationBuilder.DropTable(
                name: "certification");

            migrationBuilder.DropTable(
                name: "flight_crew");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "pilot");

            migrationBuilder.DropTable(
                name: "price_offer_log");

            migrationBuilder.DropTable(
                name: "route_operator");

            migrationBuilder.DropTable(
                name: "SuperAdmins");

            migrationBuilder.DropTable(
                name: "Supervisors");

            migrationBuilder.DropTable(
                name: "ticket");

            migrationBuilder.DropTable(
                name: "flight_leg_def");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "booking_passenger");

            migrationBuilder.DropTable(
                name: "crew_member");

            migrationBuilder.DropTable(
                name: "ancillary_product");

            migrationBuilder.DropTable(
                name: "contextual_pricing_attributes");

            migrationBuilder.DropTable(
                name: "booking");

            migrationBuilder.DropTable(
                name: "passenger");

            migrationBuilder.DropTable(
                name: "seat");

            migrationBuilder.DropTable(
                name: "employee");

            migrationBuilder.DropTable(
                name: "fare_basis_code");

            migrationBuilder.DropTable(
                name: "flight_instance");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "cabin_class");

            migrationBuilder.DropTable(
                name: "flight_schedule");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "frequent_flyer");

            migrationBuilder.DropTable(
                name: "aircraft_config");

            migrationBuilder.DropTable(
                name: "route");

            migrationBuilder.DropTable(
                name: "aircraft");

            migrationBuilder.DropTable(
                name: "aircraft_type");

            migrationBuilder.DropTable(
                name: "airline");

            migrationBuilder.DropTable(
                name: "airport");

            migrationBuilder.DropTable(
                name: "country");
        }
    }
}
