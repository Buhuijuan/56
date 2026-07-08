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
-- Table structure for table `clockin_event_location_config`
--

DROP TABLE IF EXISTS `clockin_event_location_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clockin_event_location_config` (
  `clockin_event_location_id` bigint NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `clockin_event_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '活动ID',
  `location_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '地点编码',
  `location_name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '地点名称',
  `pos_x` decimal(10,2) NOT NULL COMMENT 'X坐标',
  `pos_y` decimal(10,2) NOT NULL COMMENT 'Y坐标',
  `pos_z` decimal(10,2) NOT NULL COMMENT 'Z坐标',
  `checkin_radius` decimal(10,2) NOT NULL DEFAULT '8.00' COMMENT '打卡半径',
  `sort_no` int NOT NULL DEFAULT '1' COMMENT '排序',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`clockin_event_location_id`),
  KEY `fk_clockin_event_location_event` (`clockin_event_id`),
  CONSTRAINT `fk_clockin_event_location_event` FOREIGN KEY (`clockin_event_id`) REFERENCES `clockin_event_config` (`clockin_event_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='晨光打卡活动地点配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clockin_event_location_config`
--

LOCK TABLES `clockin_event_location_config` WRITE;
/*!40000 ALTER TABLE `clockin_event_location_config` DISABLE KEYS */;
INSERT INTO `clockin_event_location_config` VALUES (1,'clockin_2026_03_12','loc_teaching_building_1','第一教学楼',10.00,0.00,20.00,8.00,1,'2026-03-20 11:33:14'),(2,'clockin_2026_03_12','loc_library','图书馆',-5.00,0.00,35.00,8.00,2,'2026-03-20 11:33:14'),(3,'clockin_2026_03_14','loc_art_square','艺术楼前广场',-15.00,0.00,20.00,8.00,1,'2026-03-20 11:33:14'),(4,'clockin_2026_03_14','loc_sunset_point','日落观景点',12.00,0.00,42.00,8.00,2,'2026-03-20 11:33:14');
/*!40000 ALTER TABLE `clockin_event_location_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:23
