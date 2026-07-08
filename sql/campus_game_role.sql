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
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `role_id` bigint NOT NULL AUTO_INCREMENT COMMENT '角色唯一ID',
  `account_code` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `school_id` bigint NOT NULL COMMENT '所属学校ID',
  `slot_no` int NOT NULL COMMENT '该用户下第几个角色槽位',
  `nickname` varchar(100) NOT NULL COMMENT '角色名称',
  `avatar_url` varchar(255) DEFAULT NULL COMMENT '头像图片地址',
  `status` int NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `current_character_id` int NOT NULL DEFAULT '1',
  `unlocked_character_ids` tinytext NOT NULL,
  `campus_name` varchar(128) DEFAULT NULL,
  `role_code` varchar(32) DEFAULT NULL,
  PRIMARY KEY (`role_id`),
  UNIQUE KEY `uk_role_user_slot` (`account_code`,`slot_no`),
  UNIQUE KEY `uk_role_code` (`role_code`),
  KEY `idx_role_user_id` (`account_code`),
  KEY `idx_role_school_id` (`school_id`),
  CONSTRAINT `fk_role_account_code` FOREIGN KEY (`account_code`) REFERENCES `account` (`account_code`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_role_school` FOREIGN KEY (`school_id`) REFERENCES `school` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=59 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='角色表/游戏角色存档表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES (45,'137030',1,1,'ada',NULL,1,'2026-03-28 15:26:59','2026-03-28 15:26:59',1,'[1,2]','安心大学','R452037'),(51,'275706',1,1,'番茄',NULL,1,'2026-03-30 05:09:01','2026-03-30 05:09:01',1,'[1,2]','安心大学','R650357'),(52,'867469',1,1,'谢小杉',NULL,1,'2026-03-30 14:14:44','2026-03-30 14:14:44',2,'[1,2]','安心大学','R286284'),(57,'853624',1,1,'卜',NULL,1,'2026-03-31 11:09:01','2026-03-31 11:09:01',1,'[1,2]','安心大学','R407318'),(58,'126909',1,1,'shin','/uploads/avatars/role_58_20260331114108_234e1e53.jpg',1,'2026-03-31 11:39:28','2026-03-31 11:41:09',1,'[1,2]','安心大学','R882925');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:16
