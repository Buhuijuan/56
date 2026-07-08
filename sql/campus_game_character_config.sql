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
-- Table structure for table `character_config`
--

DROP TABLE IF EXISTS `character_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `character_config` (
  `character_id` bigint NOT NULL COMMENT '角色配置ID',
  `character_name` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '角色名称，可后续补充',
  `character_image_path` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '角色立绘路径',
  `character_head_path` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '角色头像路径',
  `character_video_path` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '角色展示视频路径',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态：0禁用，1启用',
  `sort_no` int NOT NULL DEFAULT '0' COMMENT '排序',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`character_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='角色配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `character_config`
--

LOCK TABLES `character_config` WRITE;
/*!40000 ALTER TABLE `character_config` DISABLE KEYS */;
INSERT INTO `character_config` VALUES (1,'角色1','Sprites/CharacterImages/Image1','Sprites/CharacterHeads/Head1','Videos/Character01',1,1,'2026-03-20 11:21:11','2026-03-20 11:21:11'),(2,'角色2','Sprites/CharacterImages/Image2','Sprites/CharacterHeads/Head2','Videos/Character01',1,2,'2026-03-20 11:21:11','2026-03-20 11:21:11'),(3,'角色3','Sprites/CharacterImages/Image1','Sprites/CharacterHeads/Head1','Videos/Character01',1,3,'2026-03-20 11:21:11','2026-03-20 11:21:11'),(4,'角色4','Sprites/CharacterImages/Image2','Sprites/CharacterHeads/Head2','Videos/Character01',1,4,'2026-03-20 11:21:11','2026-03-20 11:21:11'),(5,'角色5','Sprites/CharacterImages/Image1','Sprites/CharacterHeads/Head1','Videos/Character01',1,5,'2026-03-20 11:21:11','2026-03-20 11:21:11');
/*!40000 ALTER TABLE `character_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:08
