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
-- Table structure for table `quiz_event_reward_config`
--

DROP TABLE IF EXISTS `quiz_event_reward_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `quiz_event_reward_config` (
  `quiz_event_reward_id` bigint NOT NULL AUTO_INCREMENT COMMENT '问答奖励配置主键ID',
  `score_min` int NOT NULL COMMENT '积分区间下限',
  `score_max` int NOT NULL COMMENT '积分区间上限',
  `reward_id` bigint NOT NULL COMMENT '奖励物品ID，外键关联reward_item_config.reward_id',
  `reward_amount` int NOT NULL COMMENT '奖励数量',
  `sort_no` int NOT NULL DEFAULT '1' COMMENT '排序号',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态：0停用，1启用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`quiz_event_reward_id`),
  UNIQUE KEY `uk_score_range` (`score_min`,`score_max`),
  KEY `fk_quiz_reward_item` (`reward_id`),
  CONSTRAINT `fk_quiz_reward_item` FOREIGN KEY (`reward_id`) REFERENCES `reward_item_config` (`reward_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='问答积分段奖励配置表（所有周次通用）';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `quiz_event_reward_config`
--

LOCK TABLES `quiz_event_reward_config` WRITE;
/*!40000 ALTER TABLE `quiz_event_reward_config` DISABLE KEYS */;
INSERT INTO `quiz_event_reward_config` VALUES (1,0,200,1,10,1,1,'2026-03-20 15:49:57','2026-03-20 15:49:57'),(2,201,400,1,30,2,1,'2026-03-20 15:49:57','2026-03-20 15:49:57'),(3,401,550,1,50,3,1,'2026-03-20 15:49:57','2026-03-20 15:49:57'),(4,551,700,1,100,4,1,'2026-03-20 15:49:57','2026-03-20 15:49:57');
/*!40000 ALTER TABLE `quiz_event_reward_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:55
