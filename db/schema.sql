-- Schema of the LWT database, as created by the old .NET API's EF Core migrations.
-- The MySQL service in docker-compose.yaml loads it into a fresh database.

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `language_settings` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CreatedDate` datetime(6) NOT NULL,
  `LastModifiedDate` datetime(6) NOT NULL,
  `DictionaryLanguageCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LanguageCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserSettingId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_language_settings_UserSettingId` (`UserSettingId`),
  CONSTRAINT `FK_language_settings_user_settings_UserSettingId` FOREIGN KEY (`UserSettingId`) REFERENCES `user_settings` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logs` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Event` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedDate` datetime(6) NOT NULL,
  `LastModifiedDate` datetime(6) NOT NULL,
  `EntityId` int DEFAULT NULL,
  `EntityType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role_claims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` int NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_role_claims_RoleId` (`RoleId`),
  CONSTRAINT `FK_role_claims_roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(256) DEFAULT NULL,
  `NormalizedName` varchar(256) DEFAULT NULL,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `terms` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CreatedDate` datetime(6) NOT NULL,
  `LastModifiedDate` datetime(6) NOT NULL,
  `Content` varchar(95) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LanguageCode` varchar(95) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Meaning` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `UserId` int NOT NULL DEFAULT '0',
  `LearningLevel` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_terms_Content_LanguageCode_UserId` (`Content`,`LanguageCode`,`UserId`),
  KEY `IX_terms_UserId` (`UserId`),
  CONSTRAINT `FK_terms_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `text_terms` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CreatedDate` datetime(6) NOT NULL,
  `LastModifiedDate` datetime(6) NOT NULL,
  `TextId` int NOT NULL,
  `TermId` int DEFAULT NULL,
  `Content` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `Index` bigint NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_text_terms_TermId` (`TermId`),
  KEY `IX_text_terms_TextId` (`TextId`),
  KEY `IX_text_terms_TextId_TermId` (`TextId`,`TermId`),
  KEY `index_text_term_content` ((cast(lower(`Content`) as char(256) charset utf8mb4))),
  CONSTRAINT `FK_text_terms_terms_TermId` FOREIGN KEY (`TermId`) REFERENCES `terms` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_text_terms_texts_TextId` FOREIGN KEY (`TextId`) REFERENCES `texts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `texts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CreatedDate` datetime(6) NOT NULL,
  `LastModifiedDate` datetime(6) NOT NULL,
  `UserId` int NOT NULL DEFAULT '0',
  `Bookmark` bigint unsigned DEFAULT NULL,
  `Title` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `Content` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `LanguageCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `TermCount` bigint NOT NULL DEFAULT '0',
  `ProcessedTermCount` bigint NOT NULL,
  `LastReadAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  PRIMARY KEY (`Id`),
  KEY `IX_texts_UserId` (`UserId`),
  KEY `IX_texts_ProcessedTermCount` (`ProcessedTermCount`),
  CONSTRAINT `FK_texts_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_claims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_user_claims_UserId` (`UserId`),
  CONSTRAINT `FK_user_claims_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_logins` (
  `LoginProvider` varchar(95) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderKey` varchar(95) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderDisplayName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `UserId` int NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`),
  KEY `IX_user_logins_UserId` (`UserId`),
  CONSTRAINT `FK_user_logins_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_roles` (
  `UserId` int NOT NULL,
  `RoleId` int NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IX_user_roles_RoleId` (`RoleId`),
  CONSTRAINT `FK_user_roles_roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_user_roles_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_settings` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CreatedDate` datetime(6) NOT NULL,
  `LastModifiedDate` datetime(6) NOT NULL,
  `UserId` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_user_settings_UserId` (`UserId`),
  CONSTRAINT `FK_user_settings_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_tokens` (
  `UserId` int NOT NULL,
  `LoginProvider` varchar(95) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(95) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`UserId`,`LoginProvider`,`Name`),
  CONSTRAINT `FK_user_tokens_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserName` varchar(256) DEFAULT NULL,
  `NormalizedUserName` varchar(256) DEFAULT NULL,
  `Email` varchar(256) DEFAULT NULL,
  `NormalizedEmail` varchar(256) DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `PasswordHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `SecurityStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEnd` datetime(6) DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int NOT NULL,
  `CurrentLanguageId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
  KEY `EmailIndex` (`NormalizedEmail`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;


INSERT INTO `__EFMigrationsHistory` VALUES ('20200825020613_InitialCreate','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20200825022943_AddTermFKUser','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20200825024405_AddTextFKUser','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20200825025009_AddUserSettingFKUser','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20200825075531_AddTextTermContentIndex','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20200825083715_AddCombineUniqueIndexForTerm','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20200826183524_ChangeColumnCollation','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20200831160501_AddTermCount','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20200901154854_AddProcessTermCountField','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20201115131804_AddLastReadAt','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20201120135938_AddProcessedTermCountIndex','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20201120143131_ProcessedTermCountLong','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20201121113845_AddIndexForTextIdTermId','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20211203173006_AddLogs','6.0.0');
INSERT INTO `__EFMigrationsHistory` VALUES ('20211204075943_AddEntityInfoInLogs','6.0.0');
