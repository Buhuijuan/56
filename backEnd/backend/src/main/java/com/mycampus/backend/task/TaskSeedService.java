package com.mycampus.backend.task;

import com.mycampus.backend.task.entity.BuildingLocation;
import com.mycampus.backend.task.entity.TaskConfigEntity;
import com.mycampus.backend.task.repository.BuildingLocationRepository;
import com.mycampus.backend.task.repository.TaskConfigRepository;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.stereotype.Component;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;

@Component
public class TaskSeedService implements ApplicationRunner {

    public static final long SCHOOL_ID = 1L;
    private static final String DEFAULT_ELF_NAME = "AI小精灵";
    private static final String DEFAULT_ELF_AVATAR = "elf_default";

    public static final long NPC_WELCOME_VOLUNTEER = 1001L;
    public static final long NPC_RECEPTION_VOLUNTEER = 1002L;
    public static final long NPC_DORM_MANAGER = 1003L;
    public static final long NPC_NURSE = 1004L;

    public static final long BUILDING_RECEPTION = 2001L;
    public static final long BUILDING_DORM = 2002L;
    public static final long BUILDING_HOSPITAL = 2003L;
    public static final long BUILDING_JINHU = 2004L;
    public static final long BUILDING_BOTANICAL_GARDEN = 2005L;
    public static final long BUILDING_LIBRARY = 2006L;
    public static final long BUILDING_COMPLEX = 2007L;
    public static final long BUILDING_FIRST_TEACHING = 2008L;
    public static final long BUILDING_FIRST_LAB = 2009L;
    public static final long BUILDING_ART = 2010L;
    public static final long BUILDING_CLOCK_TOWER = 2011L;
    public static final long BUILDING_SUNSET_POINT = 2012L;
    public static final long BUILDING_BIKE_STATION = 4001L;

    private final TaskConfigRepository taskConfigRepository;
    private final BuildingLocationRepository buildingLocationRepository;

    public TaskSeedService(TaskConfigRepository taskConfigRepository,
                           BuildingLocationRepository buildingLocationRepository) {
        this.taskConfigRepository = taskConfigRepository;
        this.buildingLocationRepository = buildingLocationRepository;
    }

    @Override
    public void run(ApplicationArguments args) {
        seedTaskConfigs();
        seedBuildingLocations();
    }

    private void seedTaskConfigs() {
        List<TaskConfigEntity> configs = List.of(
                task(101L, "M_1_1", "认识你的AI伙伴", "MAIN", 1, 1, "guide", "auto_after_role_created",
                        null, "ai_dialogue", NPC_WELCOME_VOLUNTEER, 1, 25, 0, null,
                        "第一次和校园 AI 小精灵对话，开启你的报到日引导。",
                        promptJson("你好呀！欢迎来到此间校园！我是你的校园AI伙伴。",
                                "先和前面的迎新志愿者打个招呼吧，他会告诉你一些重要的事情～"),
                        promptJson("点击左上角的头像就能和我聊天。",
                                "以后有任何问题都可以来问我，比如“食堂在哪”“教学楼怎么走”。"),
                        promptJson("太好了！现在我们是一起探索的伙伴啦！",
                                "接下来，记得在任务面板中开启\"前往报到点\"的任务哦，迎新接待处就在前方主路上。")),

                task(102L, "M_1_2", "前往报到点", "MAIN", 1, 2, "explore", "auto_after_task_completed",
                        "M_1_1", "arrive_building", BUILDING_RECEPTION, 1, 25, 0, null,
                        "前往新生报到处，完成入校报到。",
                        promptJson("我们已经互相认识啦！",
                                "现在该去完成正式的报到手续了。",
                                "迎新接待处就在前方主路上，我们过去吧！"),
                        promptJson("看，迎新接待处就在前面。",
                                "请和迎新接待处的志愿者打个招呼吧，他会帮你确认信息，并告诉你宿舍安排。"),
                        promptJson("报到完成啦！你正式成为这里的一员了。",
                                "别忘了在任务面板中开启下一个任务，我们一起去宿舍安顿下来吧！")),

                task(103L, "M_1_3", "找到你的宿舍楼", "MAIN", 1, 3, "npc", "auto_after_task_completed",
                        "M_1_2", "npc_dialogue", NPC_DORM_MANAGER, 1, 25, 0, null,
                        "前往竹苑3号楼，与宿管老师对话确认宿舍信息。",
                        promptJson("报到完成了，接下来我们该去你的新家了。",
                                "根据宿舍分配单，你住在竹苑3号楼205室。",
                                "穿过小广场就到竹苑了。"),
                        promptJson("这就是竹苑3号楼啦！",
                                "和楼管阿姨打个招呼吧，她会帮你激活门禁，并告诉你入住注意事项。"),
                        promptJson("门禁激活成功！这里就是你在校园里的家了。",
                                "接下来，记得在任务面板中开启新任务，我们还要去校医院完成新生体检呢。")),

                task(104L, "M_1_4", "前往校医院体检", "MAIN", 1, 4, "npc", "auto_after_task_completed",
                        "M_1_3", "npc_dialogue", NPC_NURSE, 1, 25, 0, null,
                        "前往校医院，与护士对话完成新生体检。",
                        promptJson("新生入学还需要完成一项重要事项。",
                                "校医院今天开放新生体检通道，我们现在就过去吧。"),
                        promptJson("到体检报到点啦！",
                                "和护士打个招呼，出示校园卡，按流程完成检查就好。"),
                        promptJson("体检顺利完成！健康是探索校园的本钱哦。",
                                "第一章的重要事项都完成啦！新章节会自动开启，期待明天的校园探索吧！")),

                task(201L, "M_2_1", "探访缙湖", "MAIN", 2, 1, "explore", "manual_after_chapter_completed",
                        "M_1_4", "arrive_building", BUILDING_JINHU, 1, 40, 0, null,
                        "前往缙湖，开启第二章校园探索。",
                        promptJson("早上好！今天天气真好。",
                                "我们先去校园里最有名的缙湖走走吧。"),
                        promptJson("沿着这条小路往前走就能看到湖了。",
                                "缙湖是校园的灵魂景观，也是热门打卡地。"),
                        promptJson("真美啊！以后心情不好的时候，可以随时来这里走走。",
                                "记得在任务面板中开启下一站，我们继续探索植物园吧！")),

                task(202L, "M_2_2", "走近植物园", "MAIN", 2, 2, "explore", "auto_after_task_completed",
                        "M_2_1", "arrive_building", BUILDING_BOTANICAL_GARDEN, 1, 40, 0, null,
                        "前往植物园，感受校园自然景观。",
                        promptJson("接下来我们去植物园看看吧。",
                                "那里是校园里的绿色宝藏，有很多珍稀植物。"),
                        promptJson("从湖边往南走就能看到植物园入口啦。",
                                "每个区域都有自己的特色，慢慢欣赏吧。"),
                        promptJson("植物园不仅是学习基地，也是放松的好地方。",
                                "探索完记得去任务面板开启新任务，接下来我们要去图书馆啦！")),

                task(203L, "M_2_3", "初识图书馆", "MAIN", 2, 3, "explore", "auto_after_task_completed",
                        "M_2_2", "arrive_building", BUILDING_LIBRARY, 1, 100, 0, null,
                        "到达图书馆广场，完成第二章探索。",
                        promptJson("最后我们去图书馆看看吧。",
                                "那里是知识的宝库，也是未来你会经常来的地方。"),
                        promptJson("图书馆就在前面那栋高大的建筑里。",
                                "以后这里会是你自习、查资料的重要地点。"),
                        promptJson("今天走了三个地方，是不是对校园更熟悉了一些？",
                                "明天我们就要开始上课啦！任务面板中会开启第三章，好好休息～")),

                task(301L, "M_3_1", "初识综合楼", "MAIN", 3, 1, "explore", "manual_after_chapter_completed",
                        "M_2_3", "arrive_building", BUILDING_COMPLEX, 1, 50, 0, null,
                        "前往综合楼，开启第三章教学区探索。",
                        promptJson("今天是你大学的第一堂课。",
                                "我们先去教学楼区域熟悉一下环境吧。"),
                        promptJson("看，那栋白色外墙、有弧形楼梯的建筑就是综合楼。",
                                "你的第一节课就在这里。"),
                        promptJson("综合楼的位置记住啦！以后上课前记得提前来这里签到哦。",
                                "别忘了在任务面板中开启下一个任务，我们继续认识其他教学楼吧。")),

                task(302L, "M_3_2", "认识第一教学楼", "MAIN", 3, 2, "explore", "auto_after_task_completed",
                        "M_3_1", "arrive_building", BUILDING_FIRST_TEACHING, 1, 30, 0, null,
                        "前往第一教学楼，熟悉日常上课区域。",
                        promptJson("教学楼区域还有几栋常用的楼。",
                                "我们先去最近的第一教学楼看看吧。"),
                        promptJson("第一教学楼主要是各专业的专业课教室。",
                                "很多学院的学生都会来这里上课。"),
                        promptJson("第一教学楼也记住啦！以后上专业课就不会找不到地方了。",
                                "任务面板中新任务已开启，接下来我们去第一实验楼看看！")),

                task(303L, "M_3_3", "走近第一实验楼", "MAIN", 3, 3, "explore", "auto_after_task_completed",
                        "M_3_2", "arrive_building", BUILDING_FIRST_LAB, 1, 30, 0, null,
                        "前往第一实验楼，认识校园实验空间。",
                        promptJson("接下来我们去第一实验楼看看。",
                                "那里是进行实验教学的地方。"),
                        promptJson("第一实验楼是理工科同学常来的地方。",
                                "实验室一般需要提前预约，记得遵守安全规定。"),
                        promptJson("实验楼也认识啦！如果你以后有实验课，记得来这里上课哦。",
                                "还有最后一站，记得去任务面板开启探索艺术楼的任务！")),

                task(304L, "M_3_4", "探访艺术楼", "MAIN", 3, 4, "explore", "auto_after_task_completed",
                        "M_3_3", "arrive_building", BUILDING_ART, 1, 30, 0, null,
                        "到达艺术楼，完成第三章探索。",
                        promptJson("最后我们去艺术楼看看。",
                                "那里是艺术类专业的教学场所，建筑风格也很特别。"),
                        promptJson("艺术楼里经常有展览、音乐会和演出。",
                                "路过时偶尔还能听到琴声或歌声哦。"),
                        promptJson("教学楼区域的主要建筑都认识啦！以后上课、做实验、看展览都不会迷路了。",
                                "第三章探索完成！可以自由探索校园啦，任务面板中还有支线任务等你发现哦！")),

                task(1101L, "B_1_1", "校园打卡纪念", "BRANCH", 11, 1, "collect", "first_landmark_visit_after_task",
                        "M_1_3", "photo_checkin_distinct_location", null, 3, 50, 0, null,
                        "在三个不同地标完成成功拍照打卡。",
                        promptJson("哇，这里真美！",
                                "不如我们拍张照片记录下来吧？"),
                        promptJson("再去找两个不同的地标拍照吧。",
                                "这样你的校园相册会更丰富。"),
                        promptJson("太棒了！你已经为校园留下了三份美好的视觉记忆。",
                                "支线任务完成啦！去看看任务面板，还有新的支线任务在等着你哦！")),

                task(1201L, "B_2_1", "校园单车初体验", "BRANCH", 12, 1, "explore", "first_bike_station_visit_after_task",
                        "M_1_1", "bike_trial_distance", BUILDING_BIKE_STATION, 50, 70, 0, null,
                        "完成一段短距离单车试骑，解锁校园单车功能。",
                        promptJson("咦，那边好像有校园单车可以使用！",
                                "我们过去看看怎么用吧？"),
                        promptJson("骑车真方便！",
                                "围着单车点试骑一小圈试试看。"),
                        promptJson("恭喜你解锁校园单车功能！以后去远一点的地方就轻松多啦。",
                                "继续探索校园吧，任务面板中还有更多精彩任务等你开启！"))
        );

        configs.forEach(entity -> {
            TaskConfigEntity persisted = taskConfigRepository.findByTaskCode(entity.getTaskCode())
                    .orElseGet(TaskConfigEntity::new);
            if (persisted.getTaskId() == null) {
                persisted.setTaskId(entity.getTaskId());
            }
            copyTask(entity, persisted);
            taskConfigRepository.save(persisted);
        });
    }

    private void seedBuildingLocations() {
        List<BuildingLocation> locations = List.of(
                building(BUILDING_RECEPTION, "reception", "报到点", 40, 0, 10, 80),
                building(BUILDING_DORM, "dorm_bamboo3", "竹苑3号楼", 90, 0, 35, 80),
                building(BUILDING_HOSPITAL, "campus_hospital", "校医院", 130, 0, 65, 80),
                building(BUILDING_JINHU, "jinhu", "缙湖", 10, 0, 5, 100),
                building(BUILDING_BOTANICAL_GARDEN, "botanical_garden", "植物园", 70, 0, 45, 100),
                building(BUILDING_LIBRARY, "library", "图书馆", 18, 0, 12, 100),
                building(BUILDING_COMPLEX, "complex_building", "综合楼", 150, 0, 110, 90),
                building(BUILDING_FIRST_TEACHING, "teaching_building_1", "第一教学楼", 185, 0, 118, 90),
                building(BUILDING_FIRST_LAB, "experiment_building_1", "第一实验楼", 220, 0, 126, 90),
                building(BUILDING_ART, "art_building", "艺术楼", 255, 0, 135, 90),
                building(BUILDING_CLOCK_TOWER, "clock_tower", "钟楼", 22, 0, 18, 100),
                building(BUILDING_SUNSET_POINT, "sunset_point", "落日晚霞观景点", 290, 0, 160, 100),
                building(BUILDING_BIKE_STATION, "bike_station", "单车站", 110, 0, 90, 80)
        );

        locations.forEach(entity -> {
            BuildingLocation persisted = buildingLocationRepository.findById(entity.getBuildingLocationId())
                    .orElseGet(BuildingLocation::new);
            copyBuilding(entity, persisted);
            buildingLocationRepository.save(persisted);
        });
    }

    private TaskConfigEntity task(Long taskId, String taskCode, String taskName, String taskType, int chapterNo,
                                  int stepNo, String category, String triggerType, String preTaskCode,
                                  String targetType, Long targetId, int targetCount, int rewardExp,
                                  int rewardCoin, String rewardUnlockFeature, String description,
                                  String elfStartPromptJson, String elfProgressPromptJson, String elfCompletePromptJson) {
        TaskConfigEntity entity = new TaskConfigEntity();
        entity.setTaskId(taskId);
        entity.setTaskCode(taskCode);
        entity.setTaskName(taskName);
        entity.setTaskType(taskType);
        entity.setChapterNo(chapterNo);
        entity.setStepNo(stepNo);
        entity.setCategory(category);
        entity.setTriggerType(triggerType);
        entity.setPreTaskCode(preTaskCode);
        entity.setTargetType(targetType);
        entity.setTargetId(targetId);
        entity.setTargetCount(targetCount);
        entity.setRewardExp(rewardExp);
        entity.setRewardCoin(rewardCoin);
        entity.setRewardUnlockFeature(rewardUnlockFeature);
        entity.setDescription(description);
        entity.setElfNpcName(DEFAULT_ELF_NAME);
        entity.setElfAvatarKey(DEFAULT_ELF_AVATAR);
        entity.setElfStartPromptJson(elfStartPromptJson);
        entity.setElfProgressPromptJson(elfProgressPromptJson);
        entity.setElfCompletePromptJson(elfCompletePromptJson);
        entity.setStatus(1);
        return entity;
    }

    private void copyTask(TaskConfigEntity source, TaskConfigEntity target) {
        target.setTaskCode(source.getTaskCode());
        target.setTaskName(source.getTaskName());
        target.setTaskType(source.getTaskType());
        target.setChapterNo(source.getChapterNo());
        target.setStepNo(source.getStepNo());
        target.setCategory(source.getCategory());
        target.setTriggerType(source.getTriggerType());
        target.setPreTaskCode(source.getPreTaskCode());
        target.setTargetType(source.getTargetType());
        target.setTargetId(source.getTargetId());
        target.setTargetCount(source.getTargetCount());
        target.setRewardExp(source.getRewardExp());
        target.setRewardCoin(source.getRewardCoin());
        target.setRewardUnlockFeature(source.getRewardUnlockFeature());
        target.setDescription(source.getDescription());
        target.setElfNpcName(source.getElfNpcName());
        target.setElfAvatarKey(source.getElfAvatarKey());
        target.setElfStartPromptJson(source.getElfStartPromptJson());
        target.setElfProgressPromptJson(source.getElfProgressPromptJson());
        target.setElfCompletePromptJson(source.getElfCompletePromptJson());
        target.setStatus(source.getStatus());
    }

    private void copyBuilding(BuildingLocation source, BuildingLocation target) {
        target.setBuildingLocationId(source.getBuildingLocationId());
        target.setSchoolId(source.getSchoolId());
        target.setBuildingCode(source.getBuildingCode());
        target.setBuildingName(source.getBuildingName());
        target.setPosX(source.getPosX());
        target.setPosY(source.getPosY());
        target.setPosZ(source.getPosZ());
        target.setCheckinRadius(source.getCheckinRadius());
        target.setStatus(source.getStatus());
        target.setCreatedAt(source.getCreatedAt());
        target.setUpdatedAt(source.getUpdatedAt());
    }

    private String promptJson(String... lines) {
        StringBuilder builder = new StringBuilder("[");
        for (int i = 0; i < lines.length; i++) {
            if (i > 0) {
                builder.append(',');
            }
            builder.append('"').append(escapeJson(lines[i])).append('"');
        }
        builder.append(']');
        return builder.toString();
    }

    private String escapeJson(String value) {
        return value
                .replace("\\", "\\\\")
                .replace("\"", "\\\"");
    }

    private BuildingLocation building(Long id, String code, String name,
                                      double posX, double posY, double posZ,
                                      double checkinRadius) {
        BuildingLocation entity = new BuildingLocation();
        entity.setBuildingLocationId(id);
        entity.setSchoolId(SCHOOL_ID);
        entity.setBuildingCode(code);
        entity.setBuildingName(name);
        entity.setPosX(BigDecimal.valueOf(posX));
        entity.setPosY(BigDecimal.valueOf(posY));
        entity.setPosZ(BigDecimal.valueOf(posZ));
        entity.setCheckinRadius(BigDecimal.valueOf(checkinRadius));
        entity.setStatus(1);
        entity.setCreatedAt(LocalDateTime.now());
        entity.setUpdatedAt(LocalDateTime.now());
        return entity;
    }
}
