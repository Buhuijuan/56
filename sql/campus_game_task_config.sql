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
-- Table structure for table `task_config`
--

DROP TABLE IF EXISTS `task_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `task_config` (
  `task_id` bigint NOT NULL AUTO_INCREMENT COMMENT '任务配置主键ID',
  `task_code` varchar(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `task_name` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
  `task_type` varchar(32) COLLATE utf8mb4_unicode_ci NOT NULL,
  `chapter_code` varchar(64) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'MAIN',
  `chapter_title` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '第一章',
  `chapter_no` int NOT NULL,
  `branch_no` int DEFAULT NULL COMMENT '支线编号，支线任务使用',
  `step_no` int NOT NULL COMMENT '章节内步骤序号',
  `trigger_type` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `trigger_condition` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '任务触发条件说明',
  `pre_task_code` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `target_type` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `target_id` bigint DEFAULT NULL,
  `target_count` int NOT NULL DEFAULT '1' COMMENT '目标完成次数',
  `reward_exp` int NOT NULL DEFAULT '0' COMMENT '奖励经验值',
  `reward_id` bigint DEFAULT NULL COMMENT '奖励物品ID，外键关联reward_item_config.reward_id',
  `reward_amount` int NOT NULL DEFAULT '0' COMMENT '奖励物品数量',
  `reward_title_id` bigint DEFAULT NULL COMMENT '奖励称号ID，关联title_config.title_id',
  `reward_unlock_feature` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `description` longtext COLLATE utf8mb4_unicode_ci,
  `status` int NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `category` varchar(32) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `reward_coin` int NOT NULL,
  `elf_npc_name` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `elf_avatar_key` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `elf_start_prompt_json` longtext COLLATE utf8mb4_unicode_ci,
  `elf_progress_prompt_json` longtext COLLATE utf8mb4_unicode_ci,
  `elf_complete_prompt_json` longtext COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`task_id`),
  UNIQUE KEY `task_code` (`task_code`),
  KEY `fk_task_reward` (`reward_id`),
  KEY `fk_task_reward_title` (`reward_title_id`),
  KEY `fk_task_pre_task` (`pre_task_code`),
  CONSTRAINT `fk_task_pre_task` FOREIGN KEY (`pre_task_code`) REFERENCES `task_config` (`task_code`),
  CONSTRAINT `fk_task_reward` FOREIGN KEY (`reward_id`) REFERENCES `reward_item_config` (`reward_id`),
  CONSTRAINT `fk_task_reward_title` FOREIGN KEY (`reward_title_id`) REFERENCES `title_config` (`title_id`)
) ENGINE=InnoDB AUTO_INCREMENT=102 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='任务配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `task_config`
--

LOCK TABLES `task_config` WRITE;
/*!40000 ALTER TABLE `task_config` DISABLE KEYS */;
INSERT INTO `task_config` VALUES (1,'M_1_1','认识你的AI伙伴','MAIN','CH01','初入校园 · 报到日',1,NULL,1,'auto_after_role_created','首次登录游戏并完成角色创建后自动触发',NULL,'ai_dialogue',1001,1,25,NULL,0,12,NULL,'第一次和校园 AI 小精灵对话，开启你的报到日引导。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','guide',0,'AI小精灵','elf_default','[\"你好呀！欢迎来到此间校园！我是你的校园AI伙伴。\",\"先和前面的迎新志愿者打个招呼吧，他会告诉你一些重要的事情～\"]','[\"点击左上角的头像就能和我聊天。\",\"以后有任何问题都可以来问我，比如“食堂在哪”“教学楼怎么走”。\"]','[\"太好了！现在我们是一起探索的伙伴啦！\",\"接下来，记得在任务面板中开启\\\"前往报到点\\\"的任务哦，迎新接待处就在前方主路上。\"]'),(2,'M_1_2','前往报到点','MAIN','CH01','初入校园 · 报到日',1,NULL,2,'auto_after_task_completed','完成任务 M_1_1 后自动接取','M_1_1','arrive_building',2001,1,25,NULL,0,NULL,NULL,'前往新生报到处，完成入校报到。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','explore',0,'AI小精灵','elf_default','[\"我们已经互相认识啦！\",\"现在该去完成正式的报到手续了。\",\"迎新接待处就在前方主路上，我们过去吧！\"]','[\"看，迎新接待处就在前面。\",\"请和迎新接待处的志愿者打个招呼吧，他会帮你确认信息，并告诉你宿舍安排。\"]','[\"报到完成啦！你正式成为这里的一员了。\",\"别忘了在任务面板中开启下一个任务，我们一起去宿舍安顿下来吧！\"]'),(3,'M_1_3','找到你的宿舍楼','MAIN','CH01','初入校园 · 报到日',1,NULL,3,'auto_after_task_completed','完成任务 M_1_2 后自动接取','M_1_2','npc_dialogue',1003,1,25,NULL,0,13,NULL,'前往竹苑3号楼，与宿管老师对话确认宿舍信息。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','npc',0,'AI小精灵','elf_default','[\"报到完成了，接下来我们该去你的新家了。\",\"根据宿舍分配单，你住在竹苑3号楼205室。\",\"穿过小广场就到竹苑了。\"]','[\"这就是竹苑3号楼啦！\",\"和楼管阿姨打个招呼吧，她会帮你激活门禁，并告诉你入住注意事项。\"]','[\"门禁激活成功！这里就是你在校园里的家了。\",\"接下来，记得在任务面板中开启新任务，我们还要去校医院完成新生体检呢。\"]'),(4,'M_1_4','前往校医院体检','MAIN','CH01','初入校园 · 报到日',1,NULL,4,'auto_after_task_completed','完成任务 M_1_3 后自动接取','M_1_3','npc_dialogue',1004,1,25,NULL,0,14,NULL,'前往校医院，与护士对话完成新生体检。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','npc',0,'AI小精灵','elf_default','[\"新生入学还需要完成一项重要事项。\",\"校医院今天开放新生体检通道，我们现在就过去吧。\"]','[\"到体检报到点啦！\",\"和护士打个招呼，出示校园卡，按流程完成检查就好。\"]','[\"体检顺利完成！健康是探索校园的本钱哦。\",\"第一章的重要事项都完成啦！新章节会自动开启，期待明天的校园探索吧！\"]'),(5,'M_2_1','探访缙湖','MAIN','CH02','校园初识 · 漫游时光',2,NULL,1,'manual_after_chapter_completed','第一章完成后开放','M_1_4','arrive_building',2004,1,40,NULL,0,NULL,NULL,'前往缙湖，开启第二章校园探索。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','explore',0,'AI小精灵','elf_default','[\"早上好！今天天气真好。\",\"我们先去校园里最有名的缙湖走走吧。\"]','[\"沿着这条小路往前走就能看到湖了。\",\"缙湖是校园的灵魂景观，也是热门打卡地。\"]','[\"真美啊！以后心情不好的时候，可以随时来这里走走。\",\"记得在任务面板中开启下一站，我们继续探索植物园吧！\"]'),(6,'M_2_2','走近植物园','MAIN','CH02','校园初识 · 漫游时光',2,NULL,2,'auto_after_task_completed','完成任务 M_2_1 后自动接取','M_2_1','arrive_building',2005,1,40,NULL,0,NULL,NULL,'前往植物园，感受校园自然景观。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','explore',0,'AI小精灵','elf_default','[\"接下来我们去植物园看看吧。\",\"那里是校园里的绿色宝藏，有很多珍稀植物。\"]','[\"从湖边往南走就能看到植物园入口啦。\",\"每个区域都有自己的特色，慢慢欣赏吧。\"]','[\"植物园不仅是学习基地，也是放松的好地方。\",\"探索完记得去任务面板开启新任务，接下来我们要去图书馆啦！\"]'),(7,'M_2_3','初识图书馆','MAIN','CH02','校园初识 · 漫游时光',2,NULL,3,'auto_after_task_completed','完成任务 M_2_2 后自动接取','M_2_2','arrive_building',2006,1,100,NULL,0,15,NULL,'到达图书馆广场，完成第二章探索。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','explore',0,'AI小精灵','elf_default','[\"最后我们去图书馆看看吧。\",\"那里是知识的宝库，也是未来你会经常来的地方。\"]','[\"图书馆就在前面那栋高大的建筑里。\",\"以后这里会是你自习、查资料的重要地点。\"]','[\"今天走了三个地方，是不是对校园更熟悉了一些？\",\"明天我们就要开始上课啦！任务面板中会开启第三章，好好休息～\"]'),(8,'M_3_1','初识综合楼','MAIN','CH03','学习体验 · 第一堂课',3,NULL,1,'manual_after_chapter_completed','第二章完成后开放','M_2_3','arrive_building',2007,1,50,NULL,0,NULL,NULL,'前往综合楼，开启第三章教学区探索。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','explore',0,'AI小精灵','elf_default','[\"今天是你大学的第一堂课。\",\"我们先去教学楼区域熟悉一下环境吧。\"]','[\"看，那栋白色外墙、有弧形楼梯的建筑就是综合楼。\",\"你的第一节课就在这里。\"]','[\"综合楼的位置记住啦！以后上课前记得提前来这里签到哦。\",\"别忘了在任务面板中开启下一个任务，我们继续认识其他教学楼吧。\"]'),(9,'M_3_2','认识第一教学楼','MAIN','CH03','学习体验 · 第一堂课',3,NULL,2,'auto_after_task_completed','完成任务 M_3_1 后自动接取','M_3_1','arrive_building',2008,1,30,NULL,0,NULL,NULL,'前往第一教学楼，熟悉日常上课区域。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','explore',0,'AI小精灵','elf_default','[\"教学楼区域还有几栋常用的楼。\",\"我们先去最近的第一教学楼看看吧。\"]','[\"第一教学楼主要是各专业的专业课教室。\",\"很多学院的学生都会来这里上课。\"]','[\"第一教学楼也记住啦！以后上专业课就不会找不到地方了。\",\"任务面板中新任务已开启，接下来我们去第一实验楼看看！\"]'),(10,'M_3_3','走近第一实验楼','MAIN','CH03','学习体验 · 第一堂课',3,NULL,3,'auto_after_task_completed','完成任务 M_3_2 后自动接取','M_3_2','arrive_building',2009,1,30,NULL,0,NULL,NULL,'前往第一实验楼，认识校园实验空间。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','explore',0,'AI小精灵','elf_default','[\"接下来我们去第一实验楼看看。\",\"那里是进行实验教学的地方。\"]','[\"第一实验楼是理工科同学常来的地方。\",\"实验室一般需要提前预约，记得遵守安全规定。\"]','[\"实验楼也认识啦！如果你以后有实验课，记得来这里上课哦。\",\"还有最后一站，记得去任务面板开启探索艺术楼的任务！\"]'),(11,'M_3_4','探访艺术楼','MAIN','CH03','学习体验 · 第一堂课',3,NULL,4,'auto_after_task_completed','完成任务 M_3_3 后自动接取','M_3_3','arrive_building',2010,1,30,NULL,0,16,NULL,'到达艺术楼，完成第三章探索。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','explore',0,'AI小精灵','elf_default','[\"最后我们去艺术楼看看。\",\"那里是艺术类专业的教学场所，建筑风格也很特别。\"]','[\"艺术楼里经常有展览、音乐会和演出。\",\"路过时偶尔还能听到琴声或歌声哦。\"]','[\"教学楼区域的主要建筑都认识啦！以后上课、做实验、看展览都不会迷路了。\",\"第三章探索完成！可以自由探索校园啦，任务面板中还有支线任务等你发现哦！\"]'),(12,'B_1_1','校园打卡纪念','BRANCH','CH11','校园打卡纪念',11,1,1,'first_landmark_visit_after_task','完成主线 M_1_3 后首次访问任意地标触发','M_1_3','photo_checkin_distinct_location',NULL,3,50,NULL,0,NULL,NULL,'在三个不同地标完成成功拍照打卡。',1,'2026-03-20 16:53:20','2026-03-29 12:54:36','collect',0,'AI小精灵','elf_default','[\"哇，这里真美！\",\"不如我们拍张照片记录下来吧？\"]','[\"再去找两个不同的地标拍照吧。\",\"这样你的校园相册会更丰富。\"]','[\"太棒了！你已经为校园留下了三份美好的视觉记忆。\",\"支线任务完成啦！去看看任务面板，还有新的支线任务在等着你哦！\"]'),(13,'B_2_1','校园单车初体验','BRANCH','CH12','校园单车初体验',12,2,1,'first_bike_station_visit_after_task','完成支线 B_1_1 后首次经过校园单车停放点触发','M_1_1','bike_trial_distance',4001,50,70,NULL,0,NULL,NULL,'完成一段短距离单车试骑，解锁校园单车功能。',1,'2026-03-20 16:53:20','2026-03-29 13:28:49','explore',0,'AI小精灵','elf_default','[\"咦，那边好像有校园单车可以使用！\",\"我们过去看看怎么用吧？\"]','[\"骑车真方便！\",\"围着单车点试骑一小圈试试看。\"]','[\"恭喜你解锁校园单车功能！以后去远一点的地方就轻松多啦。\",\"继续探索校园吧，任务面板中还有更多精彩任务等你开启！\"]');
/*!40000 ALTER TABLE `task_config` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-31 17:06:25
