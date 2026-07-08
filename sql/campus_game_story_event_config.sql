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
-- Table structure for table `story_event_config`
--

DROP TABLE IF EXISTS `story_event_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `story_event_config` (
  `story_event_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '故事接龙活动ID',
  `theme` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '活动主题',
  `theme_description` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '主题描述',
  `start_time` datetime NOT NULL COMMENT '开始时间',
  `duration_days` int NOT NULL COMMENT '持续天数',
  `round_count` int NOT NULL DEFAULT '3' COMMENT '接龙轮数',
  `reward_id` bigint NOT NULL COMMENT '奖励物品ID，外键关联reward_item_config.reward_id',
  `reward_amount` int NOT NULL COMMENT '完成一个故事可获得的奖励数量',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态：0停用，1启用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`story_event_id`),
  KEY `fk_story_event_reward` (`reward_id`),
  CONSTRAINT `fk_story_event_reward` FOREIGN KEY (`reward_id`) REFERENCES `reward_item_config` (`reward_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='故事接龙活动配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `story_event_config`
--

LOCK TABLES `story_event_config` WRITE;
/*!40000 ALTER TABLE `story_event_config` DISABLE KEYS */;
INSERT INTO `story_event_config` VALUES ('story_2026_w10','食堂阿姨的秘密配方','每个人心中都有一碗忘不掉的食堂汤。','2026-03-12 00:00:00',7,3,1,10,1,'2026-03-20 16:01:52','2026-03-20 16:01:52'),('story_2026_w11','图书馆的午夜传说','据说在闭馆后的图书馆，会出现奇怪的脚步声。','2026-03-19 00:00:00',7,3,1,10,1,'2026-03-20 16:01:52','2026-03-20 16:01:52');
/*!40000 ALTER TABLE `story_event_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:15
