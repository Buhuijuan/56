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
-- Table structure for table `title_config`
--

DROP TABLE IF EXISTS `title_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `title_config` (
  `title_id` bigint NOT NULL COMMENT '称号唯一ID',
  `title_name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '称号名称',
  `title_category` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '称号大类：EXPLORE / SOCIAL / FUNCTION / COLLECT / NONE',
  `title_source` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '称号来源：DEFAULT / LEVEL / TASK / STAGE / SOCIAL / FUNCTION / COLLECT',
  `unlock_condition` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '解锁条件说明',
  `reward_exp` int NOT NULL DEFAULT '0' COMMENT '解锁该称号获得的经验值，无则为0',
  `sort_no` int NOT NULL DEFAULT '0' COMMENT '排序号',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态：0停用，1启用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`title_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='称号配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `title_config`
--

LOCK TABLES `title_config` WRITE;
/*!40000 ALTER TABLE `title_config` DISABLE KEYS */;
INSERT INTO `title_config` VALUES (1,'不使用称号','NONE','DEFAULT','默认称号，不佩戴任何称号时使用',0,1,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(2,'青芽·初萌','EXPLORE','LEVEL','完成角色创建，初次进入校园',0,2,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(3,'新叶·吐绿','EXPLORE','LEVEL','等级达到2级',0,3,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(4,'向阳·伸展','EXPLORE','LEVEL','等级达到3级',0,4,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(5,'含苞·待放','EXPLORE','LEVEL','等级达到4级',0,5,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(6,'初绽·吐蕊','EXPLORE','LEVEL','等级达到5级',0,6,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(7,'繁花·满枝','EXPLORE','LEVEL','等级达到6级',0,7,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(8,'成荫·叠翠','EXPLORE','LEVEL','等级达到7级',0,8,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(9,'硕果·盈枝','EXPLORE','LEVEL','等级达到8级',0,9,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(10,'扎根·深远','EXPLORE','LEVEL','等级达到9级',0,10,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(11,'参天·成木','EXPLORE','LEVEL','等级达到10级',0,11,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(12,'寻伴启程','EXPLORE','TASK','主线任务1-1完成后获得',0,12,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(13,'归家旅人','EXPLORE','TASK','主线任务1-3完成后获得',0,13,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(14,'健康萌新','EXPLORE','TASK','主线任务1-4完成后获得',0,14,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(15,'初探书海','EXPLORE','TASK','主线任务2-3完成后获得',0,15,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(16,'学途启航','EXPLORE','TASK','主线任务3-4完成后获得',0,16,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(17,'初入校园','EXPLORE','TASK','主线第一章完成后获得',0,17,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(18,'校园漫步者','EXPLORE','TASK','主线第二章完成后获得',0,18,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(19,'教学楼常客','EXPLORE','TASK','主线第三章完成后获得',0,19,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(20,'初临·雾中客','EXPLORE','STAGE','完成阶段1任务',30,20,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(21,'探路·识途者','EXPLORE','STAGE','完成阶段2任务',40,21,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(22,'漫游·发现家','EXPLORE','STAGE','完成阶段3任务',50,22,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(23,'归属·此间人','EXPLORE','STAGE','完成阶段4任务',60,23,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(24,'熟知·百事通','EXPLORE','STAGE','完成阶段5任务',60,24,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(25,'友善问候','SOCIAL','SOCIAL','与10个不同NPC对话',40,25,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(26,'知心伙伴·一','SOCIAL','SOCIAL','与同一NPC对话累计10次（第1个NPC）',40,26,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(27,'知心伙伴·二','SOCIAL','SOCIAL','与同一NPC对话累计10次（第2个NPC）',40,27,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(28,'知心伙伴·三','SOCIAL','SOCIAL','与同一NPC对话累计10次（第3个NPC）',40,28,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(29,'小动物之友','SOCIAL','SOCIAL','与校园猫/狗互动累计20次',40,29,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(30,'精灵伙伴','FUNCTION','FUNCTION','向小精灵提问并获答30次',40,30,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(31,'光影捕手','FUNCTION','FUNCTION','拍摄并保存30张不同照片',40,31,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(32,'骑行达人','FUNCTION','FUNCTION','累计骑行次数超过15次',30,32,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(33,'问答新秀','FUNCTION','FUNCTION','在校园问答中累计答对50题',40,33,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(34,'晨曦守护者','FUNCTION','FUNCTION','累计晨光打卡30天',40,34,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(35,'校园说书人','FUNCTION','FUNCTION','完成故事接龙作品10个',30,35,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(36,'小有名气','COLLECT','COLLECT','解锁10个不同的称号',30,36,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(37,'名声在外','COLLECT','COLLECT','解锁20个不同的称号',40,37,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(38,'名扬校园','COLLECT','COLLECT','解锁30个不同的称号',40,38,1,'2026-03-20 20:34:36','2026-03-20 20:34:36'),(39,'收藏大师','COLLECT','COLLECT','达成“小有名气”“名声在外”“名扬校园”三个称号',40,39,1,'2026-03-20 20:34:36','2026-03-20 20:34:36');
/*!40000 ALTER TABLE `title_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:33
