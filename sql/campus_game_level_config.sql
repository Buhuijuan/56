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
-- Table structure for table `level_config`
--

DROP TABLE IF EXISTS `level_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `level_config` (
  `level` int NOT NULL COMMENT '等级',
  `need_exp` int DEFAULT NULL COMMENT '升到下一级所需经验，满级可为空',
  `total_exp` int NOT NULL COMMENT '到达本级所需累计经验',
  `reward_id` bigint NOT NULL COMMENT '升级奖励物品ID，外键关联reward_item_config.reward_id',
  `reward_amount` int NOT NULL DEFAULT '0' COMMENT '达到该等级时获得的奖励数量',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态：0停用，1启用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`level`),
  KEY `fk_level_reward` (`reward_id`),
  CONSTRAINT `fk_level_reward` FOREIGN KEY (`reward_id`) REFERENCES `reward_item_config` (`reward_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='等级配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `level_config`
--

LOCK TABLES `level_config` WRITE;
/*!40000 ALTER TABLE `level_config` DISABLE KEYS */;
INSERT INTO `level_config` VALUES (1,100,0,1,10,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(2,150,100,1,20,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(3,200,250,1,30,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(4,250,450,1,40,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(5,300,700,1,50,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(6,350,1000,1,60,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(7,400,1350,1,70,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(8,450,1750,1,80,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(9,500,2200,1,90,1,'2026-03-20 16:10:13','2026-03-20 16:10:13'),(10,NULL,2700,1,100,1,'2026-03-20 16:10:13','2026-03-20 16:10:13');
/*!40000 ALTER TABLE `level_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:40
