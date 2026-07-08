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
-- Table structure for table `growth_stage_reward_config`
--

DROP TABLE IF EXISTS `growth_stage_reward_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `growth_stage_reward_config` (
  `stage_reward_id` bigint NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `stage_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '阶段ID',
  `reward_id` bigint NOT NULL COMMENT '奖励物品ID',
  `amount` int NOT NULL COMMENT '奖励数量',
  `sort_no` int NOT NULL DEFAULT '1' COMMENT '排序',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`stage_reward_id`),
  KEY `fk_growth_stage_reward_stage` (`stage_id`),
  KEY `fk_growth_stage_reward_item` (`reward_id`),
  CONSTRAINT `fk_growth_stage_reward_item` FOREIGN KEY (`reward_id`) REFERENCES `reward_item_config` (`reward_id`),
  CONSTRAINT `fk_growth_stage_reward_stage` FOREIGN KEY (`stage_id`) REFERENCES `growth_stage_config` (`stage_id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='成长阶段奖励配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `growth_stage_reward_config`
--

LOCK TABLES `growth_stage_reward_config` WRITE;
/*!40000 ALTER TABLE `growth_stage_reward_config` DISABLE KEYS */;
INSERT INTO `growth_stage_reward_config` VALUES (1,'ST1',1,10,1,'2026-03-20 11:29:49'),(2,'ST1',1,10,2,'2026-03-20 11:29:49'),(3,'ST2',1,10,1,'2026-03-20 11:29:49'),(4,'ST2',1,10,2,'2026-03-20 11:29:49'),(5,'ST3',1,10,1,'2026-03-20 11:29:49'),(6,'ST3',1,10,2,'2026-03-20 11:29:49'),(7,'ST4',1,10,1,'2026-03-20 11:29:49'),(8,'ST4',1,10,2,'2026-03-20 11:29:49'),(9,'ST5',1,10,1,'2026-03-20 11:29:49'),(10,'ST5',1,10,2,'2026-03-20 11:29:49');
/*!40000 ALTER TABLE `growth_stage_reward_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:04:47
