package com.mycampus.backend.service;

import com.mycampus.backend.api.dto.ActivityDtos;
import org.springframework.stereotype.Service;

import java.util.LinkedHashMap;
import java.util.Map;

@Service
public class CampusAiService {

    public Map<String, Object> answer(ActivityDtos.CampusQaRequest request) {
        String question = request.question() == null ? "" : request.question().trim();
        String answer = "欢迎来到校园，有问题可以继续问我。";
        String category = "general";
        String targetScene = "04_Campus";
        Map<String, Object> targetPosition = Map.of("x", 0, "y", 0, "z", 0);

        if (question.contains("图书馆")) {
            answer = "从主教学楼前广场向东走，你会看到带玻璃穹顶的图书馆。";
            category = "navigation";
            targetPosition = Map.of("x", 32, "y", 0, "z", 18);
        } else if (question.contains("食堂")) {
            answer = "食堂在宿舍区和教学区之间，当前时间段可以去三号食堂看看。";
            category = "life";
            targetPosition = Map.of("x", 12, "y", 0, "z", -8);
        } else if (question.contains("活动") || question.contains("问答")) {
            answer = "本周有校园问答、晨光打卡和故事接龙，可以在活动面板查看详情。";
            category = "activity";
        } else if (question.contains("天气")) {
            answer = "天气模块目前以前端表现层为主，后端暂未做动态天气下发。";
            category = "system";
        }

        Map<String, Object> response = new LinkedHashMap<>();
        response.put("code", 0);
        response.put("msg", "ok");
        response.put("request_id", String.valueOf(System.currentTimeMillis()));
        response.put("player_id", request.player_id());
        response.put("data", Map.of(
                "answer", answer,
                "category", category,
                "is_game_related", true,
                "target_scene", targetScene,
                "target_position", targetPosition
        ));
        return response;
    }
}
