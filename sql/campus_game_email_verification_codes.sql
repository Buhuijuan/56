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
-- Table structure for table `email_verification_codes`
--

DROP TABLE IF EXISTS `email_verification_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `email_verification_codes` (
  `email_verification_code_id` bigint NOT NULL AUTO_INCREMENT COMMENT '验证码记录唯一ID',
  `mailbox` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '接收验证码的邮箱',
  `verification_code` varchar(16) NOT NULL COMMENT '邮箱验证码',
  `verification_expires_at` datetime NOT NULL COMMENT '验证码过期时间',
  `is_used` int NOT NULL DEFAULT '0',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '验证码生成时间',
  `purpose` varchar(30) NOT NULL COMMENT '验证码用途，如register/reset_password',
  PRIMARY KEY (`email_verification_code_id`),
  KEY `idx_evc_email` (`mailbox`),
  KEY `idx_evc_email_code` (`mailbox`,`verification_code`),
  KEY `idx_evc_expire_at` (`verification_expires_at`)
) ENGINE=InnoDB AUTO_INCREMENT=60 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='邮箱验证码表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `email_verification_codes`
--

LOCK TABLES `email_verification_codes` WRITE;
/*!40000 ALTER TABLE `email_verification_codes` DISABLE KEYS */;
INSERT INTO `email_verification_codes` VALUES (6,'3970449243@qq.com','8615','2026-03-19 13:49:09',1,'2026-03-19 13:47:09','register'),(7,'123456@qq.com','7572','2026-03-19 16:37:54',0,'2026-03-19 16:35:54','register'),(10,'3124168258@qq.com','4419','2026-03-19 20:04:14',1,'2026-03-19 20:02:14','register'),(11,'3124168258@qq.com','5603','2026-03-19 20:35:22',1,'2026-03-19 20:33:22','register'),(12,'2456154847@qq.com','6947','2026-03-19 21:03:58',1,'2026-03-19 21:01:58','register'),(15,'3124168258@qq.com','7138','2026-03-22 20:40:42',1,'2026-03-22 20:38:42','register'),(16,'3351304843@qq.com','1019','2026-03-22 22:40:25',1,'2026-03-22 22:38:31','register'),(17,'3351304843@qq.com','8314','2026-03-22 22:40:30',1,'2026-03-22 22:38:31','register'),(18,'3351304843@qq.com','5024','2026-03-22 22:40:27',1,'2026-03-22 22:38:31','register'),(19,'3351304843@qq.com','2675','2026-03-22 22:40:25',1,'2026-03-22 22:38:31','register'),(20,'3351304843@qq.com','6439','2026-03-22 22:40:33',1,'2026-03-22 22:38:33','register'),(21,'3351304843@qq.com','2472','2026-03-23 17:22:25',0,'2026-03-23 17:20:25','register'),(22,'876814518@qq.com','7750','2026-03-23 17:24:35',1,'2026-03-23 17:22:34','register'),(23,'3124168258@qq.com','8669','2026-03-23 18:54:13',1,'2026-03-23 18:52:12','register'),(24,'3124168258@qq.com','6464','2026-03-24 10:07:59',1,'2026-03-24 10:05:58','register'),(26,'3124168258@qq.com','599975','2026-03-24 14:54:29',0,'2026-03-24 14:44:29','auth'),(28,'3351304843@qq.com','628668','2026-03-25 08:39:10',0,'2026-03-25 08:29:10','auth'),(29,'3351304843@qq.com','458500','2026-03-25 16:17:38',1,'2026-03-25 16:07:38','auth'),(32,'3351304843@qq.com','353274','2026-03-25 18:07:49',0,'2026-03-25 17:57:49','auth'),(38,'3124168258@qq.com','369098','2026-03-26 12:01:20',0,'2026-03-26 11:51:20','auth'),(39,'3124168258@qq.com','787188','2026-03-26 12:04:35',0,'2026-03-26 11:54:35','auth'),(40,'3124168258@qq.com','832002','2026-03-26 12:06:31',0,'2026-03-26 11:56:31','auth'),(45,'3351304843@qq.com','720450','2026-03-26 21:10:59',1,'2026-03-26 21:00:59','auth'),(48,'2517163619@qq.com','181849','2026-03-27 00:35:43',1,'2026-03-27 00:25:43','auth'),(49,'3124168258@qq.com','913006','2026-03-27 15:23:17',1,'2026-03-27 15:13:17','auth'),(51,'2456154847@qq.com','774610','2026-03-28 15:29:37',1,'2026-03-28 15:19:37','auth'),(52,'2517163619@qq.com','682726','2026-03-30 05:16:43',0,'2026-03-30 05:06:43','auth'),(53,'2517163619@qq.com','243811','2026-03-30 05:17:21',0,'2026-03-30 05:07:21','auth'),(59,'3434243862@qq.com','519853','2026-03-31 11:47:43',1,'2026-03-31 11:37:43','auth');
/*!40000 ALTER TABLE `email_verification_codes` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:44
