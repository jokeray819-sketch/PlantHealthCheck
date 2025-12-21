CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `users` (
    `id` bigint NOT NULL AUTO_INCREMENT,
    `username` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `password_hash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `phone` varchar(20) CHARACTER SET utf8mb4 NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NULL,
    `is_active` tinyint(1) NOT NULL,
    CONSTRAINT `PK_users` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_users_email` ON `users` (`email`);

CREATE UNIQUE INDEX `IX_users_username` ON `users` (`username`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251221075111_InitialCreate', '8.0.11');

COMMIT;

