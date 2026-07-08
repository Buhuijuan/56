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
-- Table structure for table `clockin_event_config`
--

DROP TABLE IF EXISTS `clockin_event_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clockin_event_config` (
  `clockin_event_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '晨光打卡活动ID',
  `event_date` date NOT NULL COMMENT '活动日期',
  `refresh_time` time NOT NULL COMMENT '刷新时间',
  `start_time` time NOT NULL DEFAULT '06:00:00' COMMENT '开放开始时间',
  `end_time` time NOT NULL DEFAULT '09:00:00' COMMENT '开放结束时间',
  `reward_coin` int NOT NULL DEFAULT '10' COMMENT '单次奖励纪念币',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`clockin_event_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='晨光打卡活动配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clockin_event_config`
--

LOCK TABLES `clockin_event_config` WRITE;
/*!40000 ALTER TABLE `clockin_event_config` DISABLE KEYS */;
INSERT INTO `clockin_event_config` VALUES ('clockin_2026_03_12','2026-03-12','05:00:00','06:00:00','09:00:00',10,1,'2026-03-20 11:33:14','2026-03-20 11:33:14'),('clockin_2026_03_14','2026-03-14','05:00:00','06:00:00','09:00:00',10,1,'2026-03-20 11:33:14','2026-03-20 11:33:14');
/*!40000 ALTER TABLE `clockin_event_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:05
