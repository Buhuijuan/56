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
-- Table structure for table `campus_landmark_config`
--

DROP TABLE IF EXISTS `campus_landmark_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `campus_landmark_config` (
  `landmark_id` bigint NOT NULL AUTO_INCREMENT COMMENT '地标ID',
  `landmark_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '地标编码，程序/任务中使用，如 JINHU、LIBRARY',
  `landmark_name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '地标名称，如 缙湖、图书馆',
  `task_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '关联任务编码，对应task_config.task_code',
  `card_title` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '介绍卡片标题',
  `detail_intro` text COLLATE utf8mb4_unicode_ci COMMENT '详细介绍，用于完整卡片正文',
  `visit_tip` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '参观提示/使用提示',
  `pos_x` decimal(10,2) DEFAULT NULL COMMENT '场景坐标X',
  `pos_y` decimal(10,2) DEFAULT NULL COMMENT '场景坐标Y',
  `pos_z` decimal(10,2) DEFAULT NULL COMMENT '场景坐标Z',
  `is_task_related` tinyint NOT NULL DEFAULT '1' COMMENT '是否用于任务相关介绍卡：1是 0否',
  `sort_no` int NOT NULL DEFAULT '0' COMMENT '排序号',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '状态：1启用 0禁用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`landmark_id`),
  UNIQUE KEY `landmark_code` (`landmark_code`),
  KEY `task_code_idx` (`task_code`),
  CONSTRAINT `task_code` FOREIGN KEY (`task_code`) REFERENCES `task_config` (`task_code`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='校园建筑/地标介绍卡配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `campus_landmark_config`
--

LOCK TABLES `campus_landmark_config` WRITE;
/*!40000 ALTER TABLE `campus_landmark_config` DISABLE KEYS */;
INSERT INTO `campus_landmark_config` VALUES (1,'JINHU','缙湖','M_2_1','缙湖','缙湖之名源于远处的缙云山。建校之初，校园设计师引山泉汇成此湖，取“缙云山下，一脉相承”之意，寓意校园与山川同呼吸、共生长。\n\n环湖一周约800米，铺着青石板路，沿途设有多处观景点与休憩空间。不少同学喜欢傍晚绕湖慢跑，一边锻炼一边看天色渐暗、灯火初上。\n\n这里既是校园最具代表性的景观之一，也是许多人放松心情、独处思考的地方。','最佳观赏时间：清晨6:30-7:30（湖面晨雾未散时）\n最佳拍摄角度：从东岸望向西岸，可将湖心与远山同框',NULL,NULL,NULL,1,1,1,'2026-03-21 23:29:25','2026-03-21 23:31:42'),(2,'BOTANICAL_GARDEN','植物园','M_2_2','植物园','植物园是校园中一处充满生机的绿色空间，分布着多种植物景观区域，如花卉区、草本区和水生植物区等。\n\n不同季节来到这里，都能看到不一样的景色：春有繁花，夏有绿荫，秋有层林尽染。许多同学会在这里散步、拍照或短暂休息，让身心慢下来。\n\n这里不仅是自然课堂，也是校园中最治愈的一角。','最佳观赏时间：春秋季上午或傍晚（光线柔和，植物状态最佳）\n最佳拍摄角度：沿主步道取景，可利用树荫形成自然前景',NULL,NULL,NULL,1,2,1,'2026-03-21 23:29:25','2026-03-21 23:31:42'),(3,'LIBRARY','图书馆','M_2_3','图书馆','图书馆是校园中最安静、也是最专注的地方之一。这里不仅提供图书借阅服务，还设有自习区、电子阅览区和研讨空间。\n\n许多同学会在这里度过大量学习时间，从清晨到深夜，灯光常亮，见证着努力与成长。\n\n当你需要专心做一件事时，这里总能给你一个安静的角落。','进入馆内请保持安静，提前了解开放时间与借阅规则。',NULL,NULL,NULL,1,3,1,'2026-03-21 23:29:25','2026-03-21 23:31:42'),(4,'TEACHING_BUILDING_1','第一教学楼','M_3_2','第一教学楼','第一教学楼是教学区的重要组成部分，内设多个教室与阶梯教室，是高频上课地点之一。\n\n许多专业课程和公共课程都会在这里进行，是学生日常往返最多的教学楼之一。\n\n熟悉这栋楼的位置，有助于更快适应校园节奏。','可留意教室安排与课表信息，合理规划上课时间。',NULL,NULL,NULL,1,4,1,'2026-03-21 23:29:25','2026-03-21 23:31:56'),(5,'LAB_BUILDING_1','第一实验楼','M_3_3','第一实验楼','第一实验楼主要用于实验课程与实践教学，是动手学习的重要场所。\n\n在这里，你将从理论走向实践，亲自参与实验操作与项目训练。\n\n这里不仅是学习知识的地方，也是培养能力的起点。','进入实验室请遵守安全规范，按要求进行操作。',NULL,NULL,NULL,1,5,1,'2026-03-21 23:29:25','2026-03-21 23:31:56'),(6,'ART_BUILDING','艺术楼','M_3_4','艺术楼','艺术楼是校园中充满创意与灵感的地方，常常可以看到作品展示或艺术活动。\n\n这里不仅用于教学，也承担文化展示与交流的功能，是校园中氛围最独特的空间之一。\n\n偶尔驻足，你可能会遇见一场展览或一次表演。','可关注展览信息，体验校园艺术氛围。',NULL,NULL,NULL,1,6,1,'2026-03-21 23:29:25','2026-03-21 23:31:57');
/*!40000 ALTER TABLE `campus_landmark_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:04:54
