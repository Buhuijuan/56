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
-- Table structure for table `player_checkin_record`
--

DROP TABLE IF EXISTS `player_checkin_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_checkin_record` (
  `player_checkin_record_id` bigint NOT NULL AUTO_INCREMENT COMMENT '拍照打卡记录ID',
  `role_id` bigint NOT NULL COMMENT '角色ID',
  `task_code` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '关联任务编码，可为空',
  `building_code` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '命中的建筑/打卡点编码',
  `current_pos_x` decimal(10,2) NOT NULL COMMENT '当前X坐标',
  `current_pos_y` decimal(10,2) NOT NULL COMMENT '当前Y坐标',
  `current_pos_z` decimal(10,2) NOT NULL COMMENT '当前Z坐标',
  `distance_to_target` decimal(10,2) DEFAULT NULL COMMENT '与目标点距离',
  `is_success` int NOT NULL,
  `trigger_type` varchar(32) COLLATE utf8mb4_unicode_ci NOT NULL,
  `checked_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '打卡时间',
  `building_location_id` bigint DEFAULT NULL,
  `task_id` bigint DEFAULT NULL,
  PRIMARY KEY (`player_checkin_record_id`),
  KEY `fk_player_checkin_task_code_idx` (`task_id`),
  CONSTRAINT `fk_player_checkin_task_id` FOREIGN KEY (`task_id`) REFERENCES `task_config` (`task_id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='角色拍照打卡记录表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player_checkin_record`
--

LOCK TABLES `player_checkin_record` WRITE;
/*!40000 ALTER TABLE `player_checkin_record` DISABLE KEYS */;
INSERT INTO `player_checkin_record` VALUES (4,7,NULL,NULL,-333.12,-184.21,-111.47,53.43,1,'photo_checkin','2026-03-25 15:02:54',2005,12),(5,7,NULL,NULL,-320.22,-184.23,134.16,6.45,1,'photo_checkin','2026-03-25 15:25:13',2004,12),(7,7,NULL,NULL,-509.85,-184.23,30.66,89.70,1,'photo_checkin','2026-03-25 15:31:11',2010,12),(20,51,NULL,NULL,-370.88,-184.15,-148.62,53.02,1,'photo_checkin','2026-03-30 05:16:18',2005,12),(21,51,NULL,NULL,-323.05,-184.15,88.51,39.85,1,'photo_checkin','2026-03-30 05:17:00',2004,12),(22,51,NULL,NULL,-493.72,-184.15,150.71,14.40,1,'photo_checkin','2026-03-30 05:19:12',2006,12);
/*!40000 ALTER TABLE `player_checkin_record` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:44
