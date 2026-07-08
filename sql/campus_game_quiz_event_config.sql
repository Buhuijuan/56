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
-- Table structure for table `quiz_event_config`
--

DROP TABLE IF EXISTS `quiz_event_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `quiz_event_config` (
  `quiz_event_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '答题活动ID',
  `theme` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '活动主题',
  `start_time` datetime NOT NULL COMMENT '开始时间',
  `duration_days` int NOT NULL COMMENT '持续天数',
  `total_questions` int NOT NULL COMMENT '题目数量',
  `questions_file` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '题库文件名',
  `score_per_correct` int NOT NULL DEFAULT '10' COMMENT '答对一题得分',
  `open_rule` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT '每周一至周日开放' COMMENT '开放规则',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`quiz_event_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='校园问答活动配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `quiz_event_config`
--

LOCK TABLES `quiz_event_config` WRITE;
/*!40000 ALTER TABLE `quiz_event_config` DISABLE KEYS */;
INSERT INTO `quiz_event_config` VALUES ('quiz_2026_w10','校史探秘','2026-03-25 00:00:00',7,10,'QuizQuestions_w10',10,'每周一至周日开放',1,'2026-03-20 11:30:59','2026-03-27 13:04:11'),('quiz_2026_w11','校园风景','2026-04-01 00:00:00',7,10,'QuizQuestions_w11',10,'每周一至周日开放',1,'2026-03-20 11:30:59','2026-03-27 13:04:13');
/*!40000 ALTER TABLE `quiz_event_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:26
