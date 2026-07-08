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
-- Table structure for table `building_location`
--

DROP TABLE IF EXISTS `building_location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `building_location` (
  `building_location_id` bigint NOT NULL,
  `building_code` varchar(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `building_name` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
  `checkin_radius` decimal(10,2) NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `posx` decimal(10,2) NOT NULL,
  `posy` decimal(10,2) NOT NULL,
  `posz` decimal(10,2) NOT NULL,
  `school_id` bigint NOT NULL,
  `status` int NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`building_location_id`),
  UNIQUE KEY `UKqopipagf2ut3krcccwpx21bkc` (`building_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `building_location`
--

LOCK TABLES `building_location` WRITE;
/*!40000 ALTER TABLE `building_location` DISABLE KEYS */;
INSERT INTO `building_location` VALUES (2001,'reception','报到点',80.00,'2026-03-30 18:34:18.169992',40.00,0.00,10.00,1,1,'2026-03-30 18:34:18.170021'),(2002,'dorm_bamboo3','竹苑3号楼',80.00,'2026-03-30 18:34:18.170048',90.00,0.00,35.00,1,1,'2026-03-30 18:34:18.170053'),(2003,'campus_hospital','校医院',80.00,'2026-03-30 18:34:18.170074',130.00,0.00,65.00,1,1,'2026-03-30 18:34:18.170079'),(2004,'jinhu','缙湖',100.00,'2026-03-30 18:34:18.170099',10.00,0.00,5.00,1,1,'2026-03-30 18:34:18.170104'),(2005,'botanical_garden','植物园',100.00,'2026-03-30 18:34:18.170122',70.00,0.00,45.00,1,1,'2026-03-30 18:34:18.170128'),(2006,'library','图书馆',100.00,'2026-03-30 18:34:18.170146',18.00,0.00,12.00,1,1,'2026-03-30 18:34:18.170151'),(2007,'complex_building','综合楼',90.00,'2026-03-30 18:34:18.170171',150.00,0.00,110.00,1,1,'2026-03-30 18:34:18.170433'),(2008,'teaching_building_1','第一教学楼',90.00,'2026-03-30 18:34:18.170476',185.00,0.00,118.00,1,1,'2026-03-30 18:34:18.170482'),(2009,'experiment_building_1','第一实验楼',90.00,'2026-03-30 18:34:18.170502',220.00,0.00,126.00,1,1,'2026-03-30 18:34:18.170507'),(2010,'art_building','艺术楼',90.00,'2026-03-30 18:34:18.170526',255.00,0.00,135.00,1,1,'2026-03-30 18:34:18.170531'),(2011,'clock_tower','钟楼',100.00,'2026-03-30 18:34:18.170750',22.00,0.00,18.00,1,1,'2026-03-30 18:34:18.170766'),(2012,'sunset_point','落日晚霞观景点',100.00,'2026-03-30 18:34:18.170787',290.00,0.00,160.00,1,1,'2026-03-30 18:34:18.170792'),(4001,'bike_station','单车站',80.00,'2026-03-30 18:34:18.170811',110.00,0.00,90.00,1,1,'2026-03-30 18:34:18.170816');
/*!40000 ALTER TABLE `building_location` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:04:40
