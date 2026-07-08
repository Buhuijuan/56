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
-- Table structure for table `player_stat`
--

DROP TABLE IF EXISTS `player_stat`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_stat` (
  `role_id` bigint NOT NULL COMMENT '角色ID',
  `npc_distinct_talk_count` int NOT NULL DEFAULT '0' COMMENT '不同NPC对话数',
  `animal_interact_count` int NOT NULL DEFAULT '0' COMMENT '小动物互动次数',
  `elf_ask_count` int NOT NULL DEFAULT '0' COMMENT '向小精灵提问次数',
  `photo_count` int NOT NULL DEFAULT '0' COMMENT '拍照总次数',
  `distinct_photo_location_count` int NOT NULL DEFAULT '0' COMMENT '不同地点拍照数',
  `bike_ride_count` int NOT NULL DEFAULT '0' COMMENT '骑行次数',
  `quiz_correct_count` int NOT NULL DEFAULT '0' COMMENT '累计答对题数',
  `morning_checkin_days` int NOT NULL DEFAULT '0' COMMENT '累计晨光打卡天数',
  `story_completed_count` int NOT NULL DEFAULT '0' COMMENT '完成故事接龙数量',
  `title_unlocked_count` int NOT NULL DEFAULT '0' COMMENT '累计解锁称号数',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `login_days` int NOT NULL DEFAULT '0' COMMENT '累计登录天数',
  `core_building_reached_count` int NOT NULL DEFAULT '0' COMMENT '累计到达核心建筑数量',
  PRIMARY KEY (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='角色行为统计表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player_stat`
--

LOCK TABLES `player_stat` WRITE;
/*!40000 ALTER TABLE `player_stat` DISABLE KEYS */;
INSERT INTO `player_stat` VALUES (24,0,0,0,0,0,0,0,0,0,1,'2026-03-27 15:35:11',0,0),(45,0,0,1,0,0,0,3,0,0,2,'2026-03-30 18:56:41',0,0),(51,0,0,1,3,3,9,7,1,0,8,'2026-03-30 18:04:03',1,2),(52,0,0,1,0,0,0,6,0,0,2,'2026-03-31 01:53:11',1,0),(57,0,0,0,0,0,0,0,0,0,1,'2026-03-31 15:39:16',0,0),(58,0,0,1,0,0,0,0,0,0,2,'2026-03-31 14:46:59',0,0);
/*!40000 ALTER TABLE `player_stat` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:40
