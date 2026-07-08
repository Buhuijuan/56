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
-- Table structure for table `player_story_record`
--

DROP TABLE IF EXISTS `player_story_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_story_record` (
  `player_story_record_id` bigint NOT NULL AUTO_INCREMENT COMMENT '故事接龙记录ID',
  `role_id` bigint NOT NULL COMMENT '角色ID',
  `story_event_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '故事活动ID',
  `submit_content` text COLLATE utf8mb4_unicode_ci COMMENT '提交内容，可选',
  `is_completed` tinyint NOT NULL DEFAULT '1' COMMENT '是否完成：0否，1是',
  `rewarded_at` datetime DEFAULT NULL COMMENT '奖励发放时间',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`player_story_record_id`),
  UNIQUE KEY `uk_role_story_event` (`role_id`,`story_event_id`),
  KEY `fk_player_story_event` (`story_event_id`),
  CONSTRAINT `fk_player_story_event` FOREIGN KEY (`story_event_id`) REFERENCES `story_event_config` (`story_event_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='角色故事接龙记录表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player_story_record`
--

LOCK TABLES `player_story_record` WRITE;
/*!40000 ALTER TABLE `player_story_record` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_story_record` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:29
