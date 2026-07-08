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
-- Table structure for table `player_signin_state`
--

DROP TABLE IF EXISTS `player_signin_state`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_signin_state` (
  `role_id` bigint NOT NULL,
  `continuous_sign_days` int NOT NULL,
  `current_week_index` int NOT NULL,
  `daily_reward_claimed` tinytext COLLATE utf8mb4_unicode_ci NOT NULL,
  `daily_signed` bit(1) NOT NULL,
  `last_heartbeat_at` datetime(6) DEFAULT NULL,
  `last_sign_in_date` date DEFAULT NULL,
  `online_reward_claimed` tinytext COLLATE utf8mb4_unicode_ci NOT NULL,
  `today_online_seconds` int NOT NULL,
  `total_login_days` int NOT NULL,
  `total_reward_claimed` tinytext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player_signin_state`
--

LOCK TABLES `player_signin_state` WRITE;
/*!40000 ALTER TABLE `player_signin_state` DISABLE KEYS */;
INSERT INTO `player_signin_state` VALUES (4,1,13,'[1]',_binary '\0','2026-03-23 23:58:07.653624','2026-03-23','[1]',7770,1,'[]'),(5,0,13,'[]',_binary '\0',NULL,NULL,'[]',0,0,'[]'),(8,1,13,'[3]',_binary '','2026-03-25 20:34:38.107479','2026-03-25','[1]',3150,1,'[]'),(24,0,13,'[]',_binary '\0','2026-03-27 15:36:40.817335',NULL,'[]',690,0,'[]'),(45,0,14,'[]',_binary '\0','2026-03-30 18:56:18.269993',NULL,'[]',720,0,'[]'),(51,1,14,'[1]',_binary '','2026-03-30 18:05:31.450305','2026-03-30','[1]',1320,1,'[]'),(52,1,14,'[1]',_binary '\0','2026-03-31 01:56:10.511301','2026-03-30','[1]',1560,1,'[]'),(57,0,14,'[]',_binary '\0','2026-03-31 15:39:45.057916',NULL,'[]',60,0,'[]'),(58,0,14,'[]',_binary '\0','2026-03-31 14:51:58.615352',NULL,'[]',870,0,'[]');
/*!40000 ALTER TABLE `player_signin_state` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:04:51
