-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: campus_game
-- ------------------------------------------------------
-- Server version	8.0.45-0ubuntu0.22.04.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `player_profile`
--

DROP TABLE IF EXISTS `player_profile`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_profile` (
  `role_id` bigint NOT NULL,
  `nickname` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `level` int NOT NULL,
  `exp` int NOT NULL,
  `coin` int NOT NULL,
  `current_title_id` bigint DEFAULT NULL,
  `bike_unlocked` int NOT NULL DEFAULT '0',
  `first_login_at` timestamp NULL DEFAULT NULL,
  `last_login_at` timestamp NULL DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`role_id`),
  KEY `fk_player_profile_current_title` (`current_title_id`),
  CONSTRAINT `fk_player_profile_current_title` FOREIGN KEY (`current_title_id`) REFERENCES `title_config` (`title_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player_profile`
--

LOCK TABLES `player_profile` WRITE;
/*!40000 ALTER TABLE `player_profile` DISABLE KEYS */;
INSERT INTO `player_profile` VALUES (24,'shin',1,0,0,1,0,'2026-03-27 07:21:18','2026-03-27 07:21:18','2026-03-27 07:21:18','2026-03-27 07:21:18'),(45,'ada',1,80,0,1,1,'2026-03-28 07:26:59','2026-03-28 07:26:59','2026-03-28 07:26:59','2026-03-30 10:56:41'),(51,'番茄',3,370,90,23,1,'2026-03-29 21:09:01','2026-03-29 21:09:01','2026-03-29 21:09:01','2026-03-29 21:28:07'),(52,'谢小杉',1,85,20,1,1,'2026-03-30 06:14:45','2026-03-30 06:14:45','2026-03-30 06:14:45','2026-03-30 09:57:22'),(57,'卜',1,0,0,1,0,'2026-03-31 03:09:01','2026-03-31 03:09:01','2026-03-31 03:09:01','2026-03-31 03:09:01'),(58,'shin',1,25,0,1,1,'2026-03-31 03:39:28','2026-03-31 03:39:28','2026-03-31 03:39:28','2026-03-31 05:39:44');
/*!40000 ALTER TABLE `player_profile` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:30
