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
-- Table structure for table `daily_award_config`
--

DROP TABLE IF EXISTS `daily_award_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `daily_award_config` (
  `base_award_id` bigint NOT NULL COMMENT '签到奖励配置ID，对应JSON中的baseAwardID',
  `day_index` int NOT NULL COMMENT '签到第几天，对应JSON中的dayIndex',
  `reward_id` bigint NOT NULL COMMENT '奖励物品ID，对应JSON中的rewardId',
  `amount` int NOT NULL COMMENT '奖励数量，对应JSON中的amount',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态：0停用，1启用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`base_award_id`),
  UNIQUE KEY `uk_day_index` (`day_index`),
  KEY `fk_daily_award_reward` (`reward_id`),
  CONSTRAINT `fk_daily_award_reward` FOREIGN KEY (`reward_id`) REFERENCES `reward_item_config` (`reward_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='每日签到奖励配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `daily_award_config`
--

LOCK TABLES `daily_award_config` WRITE;
/*!40000 ALTER TABLE `daily_award_config` DISABLE KEYS */;
INSERT INTO `daily_award_config` VALUES (1,1,1,10,1,'2026-03-20 14:32:41','2026-03-20 14:32:41'),(2,2,1,10,1,'2026-03-20 14:32:41','2026-03-20 14:32:41'),(3,3,1,10,1,'2026-03-20 14:32:41','2026-03-20 14:32:41'),(4,4,1,10,1,'2026-03-20 14:32:41','2026-03-20 14:32:41'),(5,5,1,10,1,'2026-03-20 14:32:41','2026-03-20 14:32:41'),(6,6,1,10,1,'2026-03-20 14:32:41','2026-03-20 14:32:41'),(7,7,1,20,1,'2026-03-20 14:32:41','2026-03-20 14:32:41');
/*!40000 ALTER TABLE `daily_award_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:47
