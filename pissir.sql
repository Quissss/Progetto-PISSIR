PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);
INSERT INTO __EFMigrationsHistory VALUES('20241016215525_FixDb','8.0.8');
INSERT INTO __EFMigrationsHistory VALUES('20241021225623_Add_IR_IsBeingHandledFlag','8.0.8');
INSERT INTO __EFMigrationsHistory VALUES('20241022224449_Charge_MwBotID_MadeNullable','8.0.8');
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL
);
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "TelegramUsername" TEXT NULL,
    "TelegramChatId" INTEGER NULL,
    "IsTelegramNotificationEnabled" INTEGER NOT NULL,
    "TelegramVerificationCode" TEXT NULL,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL
);
INSERT INTO AspNetUsers VALUES('88915ece-4ff3-4d44-a9a6-309356017dae','akaantox',128173490,0,NULL,'admin01@admin.it','ADMIN01@ADMIN.IT','admin01@admin.it','ADMIN01@ADMIN.IT',1,'AQAAAAIAAYagAAAAEPoucshsHg5jfoikaWwBzhImHYgKaEVGtwFVc9V216zBnP8uU2JUGMZO6cvMzxXIqA==','M3MQGQI43LRVNZXMY2DGZYSUD7CWZNXN','6ff02818-4a60-4e57-a475-b5d1f5319ea9',NULL,0,0,NULL,1,0);
INSERT INTO AspNetUsers VALUES('73f0886f-315d-4b5c-8147-2de4ba878fca','akaantox',128173490,1,'67a79fa0-4c30-4173-8589-fc2abb7620eb','admin@admin.it','ADMIN@ADMIN.IT','admin@admin.it','ADMIN@ADMIN.IT',1,'AQAAAAIAAYagAAAAEHj4bx3tclhLCh8P5kb8Zb85ytT12nsWPWQnFQ+9eeErZlesEgeFzz+c3drvPdieDg==','T2E3SLQHETLUPGDRBIFGQICFFE7W2ERF','04514906-e967-4fa5-bc6c-d531d0cf9928',NULL,0,0,NULL,1,0);
INSERT INTO AspNetUsers VALUES('6f1945b5-d8d0-40d9-9c2c-30acbb26f566','akaantox',128173490,0,NULL,'utente01@utente.com','UTENTE01@UTENTE.COM','utente01@utente.com','UTENTE01@UTENTE.COM',1,'AQAAAAIAAYagAAAAEI6Jjvzcy4QjxY8RhwfSO8zizAWCwVEyACr36ZOZ20XayzI+fwHhM75iOq/vH/qTAw==','4W66SDXVUA7D4U3N4S7MAZ5NXBJM5VDJ','d525d9bf-c740-44b6-ad77-1e7716fb4329',NULL,0,0,NULL,1,0);
INSERT INTO AspNetUsers VALUES('d909bdc8-6a4c-48e8-b803-323946324b32','akaantox',128173490,0,NULL,'utentepremium01@upremium.com','UTENTEPREMIUM01@UPREMIUM.COM','utentepremium01@upremium.com','UTENTEPREMIUM01@UPREMIUM.COM',1,'AQAAAAIAAYagAAAAENxAvWrIgtCJfWNEO7s3xHqDxMUTJL8E/43ofqgcCGa4f19pPz9soWPEC0k4gGvPaQ==','NSJCPHMPJCDW52ZMWZSL5O3S2EYEB6UL','4df77124-b81c-4810-933d-4120240abea8',NULL,0,0,NULL,1,0);
CREATE TABLE IF NOT EXISTS "Parkings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parkings" PRIMARY KEY AUTOINCREMENT,
    "EnergyCostPerKw" decimal(5, 2) NOT NULL,
    "StopCostPerMinute" decimal(5, 2) NOT NULL,
    "Name" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "City" TEXT NOT NULL,
    "Province" TEXT NOT NULL,
    "PostalCode" TEXT NOT NULL,
    "Country" TEXT NOT NULL
);
INSERT INTO Parkings VALUES(1,1,3,'Colosseo','Viale Roma 20','Novara','NO','28100','Italy');
INSERT INTO Parkings VALUES(5,1,1,'Anto''s place','Via Anton 2','Santhia','VC','13048','IT');
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
INSERT INTO AspNetUserClaims VALUES(1,'88915ece-4ff3-4d44-a9a6-309356017dae','Role','0');
INSERT INTO AspNetUserClaims VALUES(2,'73f0886f-315d-4b5c-8147-2de4ba878fca','Role','0');
INSERT INTO AspNetUserClaims VALUES(3,'6f1945b5-d8d0-40d9-9c2c-30acbb26f566','Role','2');
INSERT INTO AspNetUserClaims VALUES(4,'d909bdc8-6a4c-48e8-b803-323946324b32','Role','1');
INSERT INTO AspNetUserClaims VALUES(5,'e493d0d7-234e-4df8-afee-5a4a0c26696b','Role','2');
INSERT INTO AspNetUserClaims VALUES(6,'79a28ec1-e712-4201-95e0-e534fe7e4888','Role','2');
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "Cars" (
    "Plate" TEXT NOT NULL CONSTRAINT "PK_Cars" PRIMARY KEY,
    "Brand" TEXT NOT NULL,
    "Model" TEXT NOT NULL,
    "IsElectric" INTEGER NOT NULL DEFAULT 1,
    "Status" INTEGER NOT NULL,
    "ParkingId" INTEGER NULL,
    "ParkingSlotId" INTEGER NULL,
    "OwnerId" TEXT NOT NULL,
    CONSTRAINT "FK_Cars_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Cars_Parkings_ParkingId" FOREIGN KEY ("ParkingId") REFERENCES "Parkings" ("Id")
);
INSERT INTO Cars VALUES('AA123AA','Tesla','Model 3',1,5,5,7,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO Cars VALUES('AA234AA','Tesla','Model 5',1,5,1,2,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO Cars VALUES('AA345AA','Tesla','Model Zio',1,0,NULL,NULL,'73f0886f-315d-4b5c-8147-2de4ba878fca');
CREATE TABLE IF NOT EXISTS "MwBots" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MwBots" PRIMARY KEY AUTOINCREMENT,
    "BatteryPercentage" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "ParkingId" INTEGER NULL,
    "LatestLocation" INTEGER NULL,
    CONSTRAINT "FK_MwBots_Parkings_ParkingId" FOREIGN KEY ("ParkingId") REFERENCES "Parkings" ("Id")
);
INSERT INTO MwBots VALUES(1,'93.0',0,1,0);
INSERT INTO MwBots VALUES(2,'92.5',0,1,1);
INSERT INTO MwBots VALUES(3,'97.0',0,1,1);
INSERT INTO MwBots VALUES(4,'84.5',0,1,1);
INSERT INTO MwBots VALUES(5,'100.0',1,5,0);
INSERT INTO MwBots VALUES(6,'100.0',1,5,0);
CREATE TABLE IF NOT EXISTS "ParkingSlots" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ParkingSlots" PRIMARY KEY AUTOINCREMENT,
    "Number" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "ParkingId" INTEGER NOT NULL,
    CONSTRAINT "FK_ParkingSlots_Parkings_ParkingId" FOREIGN KEY ("ParkingId") REFERENCES "Parkings" ("Id") ON DELETE CASCADE
);
INSERT INTO ParkingSlots VALUES(2,2,1,1);
INSERT INTO ParkingSlots VALUES(3,3,0,1);
INSERT INTO ParkingSlots VALUES(4,4,0,1);
INSERT INTO ParkingSlots VALUES(5,5,0,1);
INSERT INTO ParkingSlots VALUES(6,1,0,1);
INSERT INTO ParkingSlots VALUES(7,1,1,5);
INSERT INTO ParkingSlots VALUES(8,2,0,5);
INSERT INTO ParkingSlots VALUES(9,3,0,5);
CREATE TABLE IF NOT EXISTS "Reservations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Reservations" PRIMARY KEY AUTOINCREMENT,
    "ReservationTime" TEXT NOT NULL,
    "RequestDate" TEXT NOT NULL,
    "RequestedChargeLevel" TEXT NOT NULL,
    "CarPlate" TEXT NULL,
    "ParkingId" INTEGER NOT NULL,
    "UserId" TEXT NOT NULL,
    "CarIsInside" INTEGER NOT NULL,
    CONSTRAINT "FK_Reservations_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reservations_Cars_CarPlate" FOREIGN KEY ("CarPlate") REFERENCES "Cars" ("Plate") ON DELETE CASCADE,
    CONSTRAINT "FK_Reservations_Parkings_ParkingId" FOREIGN KEY ("ParkingId") REFERENCES "Parkings" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "ImmediateRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ImmediateRequests" PRIMARY KEY AUTOINCREMENT,
    "RequestDate" TEXT NOT NULL,
    "RequestedChargeLevel" TEXT NOT NULL,
    "CarPlate" TEXT NULL,
    "ParkingId" INTEGER NOT NULL,
    "ParkingSlotId" INTEGER NULL,
    "UserId" TEXT NOT NULL,
    "FromReservation" INTEGER NOT NULL, "IsBeingHandled" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT "FK_ImmediateRequests_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ImmediateRequests_Cars_CarPlate" FOREIGN KEY ("CarPlate") REFERENCES "Cars" ("Plate"),
    CONSTRAINT "FK_ImmediateRequests_ParkingSlots_ParkingSlotId" FOREIGN KEY ("ParkingSlotId") REFERENCES "ParkingSlots" ("Id"),
    CONSTRAINT "FK_ImmediateRequests_Parkings_ParkingId" FOREIGN KEY ("ParkingId") REFERENCES "Parkings" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "CurrentlyCharging" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CurrentlyCharging" PRIMARY KEY AUTOINCREMENT,
    "CarPlate" TEXT NULL,
    "CurrentChargePercentage" decimal(5, 2) NULL,
    "EndChargingTime" TEXT NULL,
    "EnergyConsumed" decimal(5, 2) NOT NULL DEFAULT '0.0',
    "ImmediateRequestId" INTEGER NULL,
    "MwBotId" INTEGER NULL,
    "ParkingSlotId" INTEGER NULL,
    "StartChargePercentage" decimal(5, 2) NULL,
    "StartChargingTime" TEXT NOT NULL DEFAULT '2024-10-23 00:44:46.323007',
    "TargetChargePercentage" decimal(5, 2) NULL,
    "ToPay" INTEGER NOT NULL DEFAULT 0,
    "TotalCost" decimal(5, 2) NOT NULL DEFAULT '0.0',
    "UserId" TEXT NULL,
    CONSTRAINT "FK_CurrentlyCharging_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id"),
    CONSTRAINT "FK_CurrentlyCharging_Cars_CarPlate" FOREIGN KEY ("CarPlate") REFERENCES "Cars" ("Plate"),
    CONSTRAINT "FK_CurrentlyCharging_ImmediateRequests_ImmediateRequestId" FOREIGN KEY ("ImmediateRequestId") REFERENCES "ImmediateRequests" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CurrentlyCharging_MwBots_MwBotId" FOREIGN KEY ("MwBotId") REFERENCES "MwBots" ("Id"),
    CONSTRAINT "FK_CurrentlyCharging_ParkingSlots_ParkingSlotId" FOREIGN KEY ("ParkingSlotId") REFERENCES "ParkingSlots" ("Id")
);
INSERT INTO CurrentlyCharging VALUES(5,'AA123AA',100,'2024-10-22 20:15:38.7354724',0.0186592000000000007,NULL,1,2,33,'2024-10-22 20:08:06.8815491',100,1,22.6099999999999994,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(6,'AA123AA',100,'2024-10-22 20:39:26.8431281',0.0266559999999999991,NULL,1,2,4,'2024-10-22 20:21:06.3373244',100,1,55.0499999999999971,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(7,'AA234AA',100,'2024-10-22 21:38:14.0502335',0.0228241999999999993,NULL,1,2,18,'2024-10-22 21:26:31.4532643',100,1,35.1499999999999985,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(8,'AA345AA',100,'2024-10-22 21:44:01.7398471',0.00724710000000000024,NULL,1,3,74,'2024-10-22 21:42:28.1385429',100,1,4.67999999999999971,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(9,'AA123AA',100,'2024-10-22 22:03:36.9599022',0.0119951999999999992,NULL,1,2,57,'2024-10-22 22:00:24.1096185',100,1,9.65000000000000035,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(10,'AA234AA',85.0999999999999943,'2024-10-22 22:21:34.6022942',0.0214080999999999993,NULL,1,3,8,'2024-10-22 22:06:46.7741628',100,1,44.4099999999999965,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(11,'AA123AA',71.2000000000000028,'2024-10-22 22:21:40.5346953',0.00533120000000000034,NULL,1,2,52,'2024-10-22 22:20:02.8807589',100,1,4.87999999999999989,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(12,'AA123AA',100,'2024-10-22 23:40:04.9836427',0.0147440999999999995,NULL,1,2,47,'2024-10-22 23:37:00.9704035',100,1,9.21000000000000085,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(13,'AA234AA',100,'2024-10-23 00:51:30.3634436',0.0255731000000000013,NULL,3,2,8,'2024-10-22 23:43:24.1320037',100,1,204.330000000000012,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(14,'AA123AA',100,'2024-10-23 16:52:07.4036628',0.0178262000000000003,NULL,3,3,36,'2024-10-23 00:52:23.8371469',100,1,2879.19000000000005,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(15,'AA123AA',100,'2024-10-23 17:03:59.05277',0.0230741000000000001,NULL,3,2,17,'2024-10-23 16:53:40.3381144',100,1,30.9600000000000008,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(16,'AA123AA',100,'2024-10-23 22:17:52.1496319',0.0149940000000000003,NULL,4,2,46,'2024-10-23 17:13:41.8640928',100,1,912.519999999999981,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(17,'AA234AA',100,'2024-10-24 00:33:56.908093',0.0144942000000000004,NULL,2,2,48,'2024-10-23 22:20:16.8929982',100,1,401.00999999999999,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(18,'AA123AA',100,'2024-10-24 23:33:32.000406',0.0189091000000000016,NULL,5,7,32,'2024-10-24 00:39:34.3385241',100,1,1373.98000000000001,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO CurrentlyCharging VALUES(19,'AA234AA',100,'2024-10-25 12:40:58.3678816',0.0239071000000000005,NULL,4,2,14,'2024-10-24 00:40:12.3253848',100,1,6482.3199999999997,'73f0886f-315d-4b5c-8147-2de4ba878fca');
CREATE TABLE IF NOT EXISTS "Stopover" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Stopover" PRIMARY KEY AUTOINCREMENT,
    "CarPlate" TEXT NULL,
    "EndStopoverTime" TEXT NULL,
    "ParkingSlotId" INTEGER NULL,
    "StartStopoverTime" TEXT NOT NULL DEFAULT '2024-10-23 00:44:46.3263079',
    "ToPay" INTEGER NOT NULL DEFAULT 0,
    "TotalCost" decimal(5, 2) NOT NULL,
    "UserId" TEXT NULL,
    CONSTRAINT "FK_Stopover_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id"),
    CONSTRAINT "FK_Stopover_Cars_CarPlate" FOREIGN KEY ("CarPlate") REFERENCES "Cars" ("Plate"),
    CONSTRAINT "FK_Stopover_ParkingSlots_ParkingSlotId" FOREIGN KEY ("ParkingSlotId") REFERENCES "ParkingSlots" ("Id")
);
INSERT INTO Stopover VALUES(2,'AA234AA','2024-10-22 01:18:09.6049284',3,'2024-10-22 01:07:25.1248292',1,32.2199999999999988,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO Stopover VALUES(3,'AA345AA','2024-10-22 01:18:14.5051612',4,'2024-10-22 01:08:57.0039912',1,27.879999999999999,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO Stopover VALUES(5,'AA123AA','2024-10-22 01:32:23.4417954',2,'2024-10-22 01:30:10.5997664',1,6.63999999999999968,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO Stopover VALUES(10,'AA123AA','2024-10-22 01:43:58.6504986',2,'2024-10-22 01:43:57.2939647',1,0.0700000000000000066,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO Stopover VALUES(11,'AA234AA','2024-10-22 20:22:36.1726086',2,'2024-10-22 20:22:25.8917498',1,0.510000000000000008,'73f0886f-315d-4b5c-8147-2de4ba878fca');
CREATE TABLE IF NOT EXISTS "PaymentHistory" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PaymentHistory" PRIMARY KEY AUTOINCREMENT,
    "CarPlate" TEXT NOT NULL,
    "EndChargePercentage" TEXT NULL,
    "EndTime" TEXT NOT NULL,
    "EnergyConsumed" TEXT NULL,
    "IsCharge" INTEGER NOT NULL,
    "PaymentDate" TEXT NOT NULL DEFAULT '2024-10-23 00:44:46.3254883',
    "StartChargePercentage" TEXT NULL,
    "StartTime" TEXT NOT NULL,
    "TotalCost" decimal(5, 2) NOT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_PaymentHistory_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PaymentHistory_Cars_CarPlate" FOREIGN KEY ("CarPlate") REFERENCES "Cars" ("Plate")
);
INSERT INTO PaymentHistory VALUES(1,'AA123AA',NULL,'2024-10-22 01:41:54.8364777',NULL,0,'2024-10-22 00:56:20.2895939',NULL,'2024-10-22 01:41:51.750776',0.149999999999999994,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO PaymentHistory VALUES(2,'AA123AA',NULL,'2024-10-22 01:18:00.086059',NULL,0,'2024-10-22 00:56:20.2895939',NULL,'2024-10-22 01:05:18.3159577',38.0900000000000034,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO PaymentHistory VALUES(3,'AA123AA',NULL,'2024-10-22 01:41:44.2144114',NULL,0,'2024-10-22 00:56:20.2895939',NULL,'2024-10-22 01:39:22.5854175',7.08000000000000007,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO PaymentHistory VALUES(4,'AA234AA',NULL,'2024-10-22 01:39:13.9504908',NULL,0,'2024-10-22 00:56:20.2895939',NULL,'2024-10-22 01:36:45.5831799',7.41999999999999992,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO PaymentHistory VALUES(5,'AA234AA',NULL,'2024-10-22 01:36:12.9489653',NULL,0,'2024-10-22 00:56:20.2895939',NULL,'2024-10-22 01:35:38.419052',1.72999999999999998,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO PaymentHistory VALUES(6,'AA123AA','100.0','2024-10-22 01:44:46.2147374','0.0029988',1,'2024-10-22 00:56:20.2895939','70.0','2024-10-22 01:44:04.3726172',2.08999999999999985,'73f0886f-315d-4b5c-8147-2de4ba878fca');
INSERT INTO PaymentHistory VALUES(7,'AA345AA',NULL,'2024-10-22 01:29:11.4011579',NULL,0,'2024-10-22 00:56:20.2895939',NULL,'2024-10-22 01:19:15.3172203',29.8000000000000007,'73f0886f-315d-4b5c-8147-2de4ba878fca');
DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('AspNetUserClaims',6);
INSERT INTO sqlite_sequence VALUES('Parkings',5);
INSERT INTO sqlite_sequence VALUES('ParkingSlots',9);
INSERT INTO sqlite_sequence VALUES('MwBots',6);
INSERT INTO sqlite_sequence VALUES('ImmediateRequests',17);
INSERT INTO sqlite_sequence VALUES('Reservations',13);
INSERT INTO sqlite_sequence VALUES('CurrentlyCharging',19);
INSERT INTO sqlite_sequence VALUES('Stopover',11);
INSERT INTO sqlite_sequence VALUES('PaymentHistory',7);
CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
CREATE INDEX "IX_Cars_OwnerId" ON "Cars" ("OwnerId");
CREATE INDEX "IX_Cars_ParkingId" ON "Cars" ("ParkingId");
CREATE INDEX "IX_ImmediateRequests_CarPlate" ON "ImmediateRequests" ("CarPlate");
CREATE INDEX "IX_ImmediateRequests_ParkingId" ON "ImmediateRequests" ("ParkingId");
CREATE INDEX "IX_ImmediateRequests_ParkingSlotId" ON "ImmediateRequests" ("ParkingSlotId");
CREATE INDEX "IX_ImmediateRequests_UserId" ON "ImmediateRequests" ("UserId");
CREATE INDEX "IX_MwBots_ParkingId" ON "MwBots" ("ParkingId");
CREATE INDEX "IX_ParkingSlots_ParkingId" ON "ParkingSlots" ("ParkingId");
CREATE INDEX "IX_Reservations_CarPlate" ON "Reservations" ("CarPlate");
CREATE INDEX "IX_Reservations_ParkingId" ON "Reservations" ("ParkingId");
CREATE INDEX "IX_Reservations_UserId" ON "Reservations" ("UserId");
CREATE INDEX "IX_CurrentlyCharging_CarPlate" ON "CurrentlyCharging" ("CarPlate");
CREATE INDEX "IX_CurrentlyCharging_ImmediateRequestId" ON "CurrentlyCharging" ("ImmediateRequestId");
CREATE INDEX "IX_CurrentlyCharging_MwBotId" ON "CurrentlyCharging" ("MwBotId");
CREATE INDEX "IX_CurrentlyCharging_ParkingSlotId" ON "CurrentlyCharging" ("ParkingSlotId");
CREATE INDEX "IX_CurrentlyCharging_UserId" ON "CurrentlyCharging" ("UserId");
CREATE INDEX "IX_Stopover_CarPlate" ON "Stopover" ("CarPlate");
CREATE INDEX "IX_Stopover_ParkingSlotId" ON "Stopover" ("ParkingSlotId");
CREATE INDEX "IX_Stopover_UserId" ON "Stopover" ("UserId");
CREATE INDEX "IX_PaymentHistory_CarPlate" ON "PaymentHistory" ("CarPlate");
CREATE INDEX "IX_PaymentHistory_UserId" ON "PaymentHistory" ("UserId");
COMMIT;
