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
-- Table structure for table `player_quiz_answer`
--

DROP TABLE IF EXISTS `player_quiz_answer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_quiz_answer` (
  `player_quiz_answer_id` bigint NOT NULL AUTO_INCREMENT COMMENT '答题记录ID',
  `role_id` bigint NOT NULL COMMENT '角色ID',
  `quiz_event_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '答题活动ID',
  `question_id` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '题目ID',
  `selected_index` int NOT NULL COMMENT '玩家选择的选项下标',
  `is_correct` tinyint NOT NULL COMMENT '是否答对：0否，1是',
  `answered_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '作答时间',
  PRIMARY KEY (`player_quiz_answer_id`),
  UNIQUE KEY `uk_role_question` (`role_id`,`question_id`),
  KEY `fk_player_quiz_event` (`quiz_event_id`),
  KEY `fk_player_quiz_question` (`question_id`),
  CONSTRAINT `fk_player_quiz_event` FOREIGN KEY (`quiz_event_id`) REFERENCES `quiz_event_config` (`quiz_event_id`),
  CONSTRAINT `fk_player_quiz_question` FOREIGN KEY (`question_id`) REFERENCES `quiz_question_config` (`question_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='角色答题记录表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player_quiz_answer`
--

LOCK TABLES `player_quiz_answer` WRITE;
/*!40000 ALTER TABLE `player_quiz_answer` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_quiz_answer` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:05:37
