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
-- Table structure for table `growth_stage_goal_config`
--

DROP TABLE IF EXISTS `growth_stage_goal_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `growth_stage_goal_config` (
  `stage_goal_id` bigint NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `stage_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '阶段ID',
  `goal_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '阶段目标编码',
  `goal_description` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '阶段目标描述',
  `sort_no` int NOT NULL DEFAULT '1' COMMENT '排序',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`stage_goal_id`),
  UNIQUE KEY `uk_stage_goal_code` (`goal_code`),
  KEY `fk_growth_stage_goal_stage` (`stage_id`),
  CONSTRAINT `fk_growth_stage_goal_stage` FOREIGN KEY (`stage_id`) REFERENCES `growth_stage_config` (`stage_id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='成长阶段目标配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `growth_stage_goal_config`
--

LOCK TABLES `growth_stage_goal_config` WRITE;
/*!40000 ALTER TABLE `growth_stage_goal_config` DISABLE KEYS */;
INSERT INTO `growth_stage_goal_config` VALUES (1,'ST1','ST1_GT1','完成“报到日”系列任务',1,'2026-03-20 11:29:49'),(2,'ST1','ST1_GT2','成功抵达3个核心建筑',2,'2026-03-20 11:29:49'),(3,'ST1','ST1_GT3','累计登录天数 ≥ 3天',3,'2026-03-20 11:29:49'),(4,'ST2','ST2_GT1','完成“报到日”系列任务',1,'2026-03-20 11:29:49'),(5,'ST2','ST2_GT2','成功抵达3个核心建筑',2,'2026-03-20 11:29:49'),(6,'ST2','ST2_GT3','累计登录天数 ≥ 3天',3,'2026-03-20 11:29:49'),(7,'ST3','ST3_GT1','完成“报到日”系列任务',1,'2026-03-20 11:29:49'),(8,'ST3','ST3_GT2','成功抵达3个核心建筑',2,'2026-03-20 11:29:49'),(9,'ST3','ST3_GT3','累计登录天数 ≥ 3天',3,'2026-03-20 11:29:49'),(10,'ST4','ST4_GT1','完成“报到日”系列任务',1,'2026-03-20 11:29:49'),(11,'ST4','ST4_GT2','成功抵达3个核心建筑',2,'2026-03-20 11:29:49'),(12,'ST4','ST4_GT3','累计登录天数 ≥ 3天',3,'2026-03-20 11:29:49'),(13,'ST5','ST5_GT1','完成“报到日”系列任务',1,'2026-03-20 11:29:49'),(14,'ST5','ST5_GT2','成功抵达3个核心建筑',2,'2026-03-20 11:29:49'),(15,'ST5','ST5_GT3','累计登录天数 ≥ 3天',3,'2026-03-20 11:29:49');
/*!40000 ALTER TABLE `growth_stage_goal_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:09
