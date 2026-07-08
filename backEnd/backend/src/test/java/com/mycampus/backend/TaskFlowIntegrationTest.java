package com.mycampus.backend;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.mycampus.backend.auth.repository.AccountRepository;
import com.mycampus.backend.auth.repository.EmailVerificationCodeRepository;
import com.mycampus.backend.player.repository.PlayerTitleRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
class TaskFlowIntegrationTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private PlayerTitleRepository playerTitleRepository;

    @Autowired
    private AccountRepository accountRepository;

    @Autowired
    private EmailVerificationCodeRepository emailVerificationCodeRepository;

    @Test
    void shouldAutoStartFirstMainTaskAndUnlockNextTaskAfterClaim() throws Exception {
        String token = createRoleAndLoginToken();

        String currentTaskResponse = mockMvc.perform(get("/api/tasks/current/main")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode currentTaskJson = objectMapper.readTree(currentTaskResponse);
        assertThat(currentTaskJson.get("data").get("task").get("taskCode").asText()).isEqualTo("M_1_1");
        assertThat(currentTaskJson.get("data").get("task").get("status").asText()).isEqualTo("IN_PROGRESS");

        String eventBody = """
                {
                  "eventType": "AI_DIALOGUE",
                  "success": true
                }
                """;

        String eventResponse = mockMvc.perform(post("/api/tasks/events")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(eventBody))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode eventJson = objectMapper.readTree(eventResponse);
        assertThat(eventJson.get("data").get("progressedTasks").get(0).get("completed").asBoolean()).isTrue();

        String claimResponse = mockMvc.perform(post("/api/tasks/M_1_1/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode claimJson = objectMapper.readTree(claimResponse);
        assertThat(claimJson.get("data").get("rewards").get("exp").get("addedExp").asInt()).isEqualTo(25);
        assertThat(claimJson.get("data").get("rewards").get("title").get("titleId").asLong()).isEqualTo(2L);

        String nextTaskResponse = mockMvc.perform(get("/api/tasks/current/main")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode nextTaskJson = objectMapper.readTree(nextTaskResponse);
        assertThat(nextTaskJson.get("data").get("task").get("taskCode").asText()).isEqualTo("M_1_2");
    }

    @Test
    void shouldTriggerBranchTasksAndUnlockBikeFeature() throws Exception {
        String token = createRoleAndLoginToken();

        completeAndClaim(token, "M_1_1", """
                {
                  "eventType": "AI_DIALOGUE",
                  "success": true
                }
                """);

        completeAndClaim(token, "M_1_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetType": "arrive_building",
                  "targetId": 2001,
                  "success": true
                }
                """);

        completeAndClaim(token, "M_1_3", """
                {
                  "eventType": "NPC_DIALOGUE",
                  "targetType": "npc_dialogue",
                  "targetId": 1003,
                  "success": true
                }
                """);

        String landmarkTriggerResponse = mockMvc.perform(post("/api/tasks/events")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "eventType": "LANDMARK_VISIT",
                                  "targetType": "arrive_building",
                                  "targetId": 2004,
                                  "success": true
                                }
                                """))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode landmarkTriggerJson = objectMapper.readTree(landmarkTriggerResponse);
        assertThat(landmarkTriggerJson.get("data").get("triggeredTasks").get(0).asText()).isEqualTo("B_1_1");

        sendPhotoEvent(token, 2004);
        sendPhotoEvent(token, 2004);
        sendPhotoEvent(token, 2006);
        sendPhotoEvent(token, 2011);

        String branchTaskResponse = mockMvc.perform(get("/api/tasks/B_1_1")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        JsonNode branchTaskJson = objectMapper.readTree(branchTaskResponse);
        assertThat(branchTaskJson.get("data").get("status").asText()).isEqualTo("COMPLETED");
        assertThat(branchTaskJson.get("data").get("progressCurrent").asInt()).isEqualTo(3);

        String branchClaimResponse = mockMvc.perform(post("/api/tasks/B_1_1/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        JsonNode branchClaimJson = objectMapper.readTree(branchClaimResponse);
        assertThat(branchClaimJson.get("data").get("rewards").get("exp").get("addedExp").asInt()).isEqualTo(50);

        String bikeTriggerResponse = mockMvc.perform(post("/api/tasks/events")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "eventType": "BIKE_STATION_VISIT",
                                  "targetId": 4001,
                                  "success": true
                                }
                                """))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        JsonNode bikeTriggerJson = objectMapper.readTree(bikeTriggerResponse);
        assertThat(bikeTriggerJson.get("data").get("triggeredTasks").get(0).asText()).isEqualTo("B_2_1");

        String bikeProgressBody = """
                {
                  "eventType": "BIKE_TRIAL_DISTANCE",
                  "targetId": 4001,
                  "increment": 50,
                  "success": true
                }
                """;
        mockMvc.perform(post("/api/tasks/events")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(bikeProgressBody))
                .andExpect(status().isOk());

        String bikeClaimResponse = mockMvc.perform(post("/api/tasks/B_2_1/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        JsonNode bikeClaimJson = objectMapper.readTree(bikeClaimResponse);
        assertThat(bikeClaimJson.get("data").get("rewards").get("feature").get("bikeUnlocked").asBoolean()).isTrue();

        String homeResponse = mockMvc.perform(get("/api/game/home")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        JsonNode homeJson = objectMapper.readTree(homeResponse);
        assertThat(homeJson.get("data").get("profile").get("bikeUnlocked").asInt()).isEqualTo(1);
    }

    @Test
    void shouldReturnNullCurrentMainTaskAfterAllMainTasksAreClaimed() throws Exception {
        String token = createRoleAndLoginToken();

        completeAndClaim(token, "M_1_1", """
                {
                  "eventType": "AI_DIALOGUE",
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2001,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_3", """
                {
                  "eventType": "NPC_DIALOGUE",
                  "targetId": 1003,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_4", """
                {
                  "eventType": "NPC_DIALOGUE",
                  "targetId": 1004,
                  "success": true
                }
                """);

        mockMvc.perform(post("/api/tasks/M_2_1/accept")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());
        completeAndClaim(token, "M_2_1", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2004,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_2_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2005,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_2_3", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2006,
                  "success": true
                }
                """);

        mockMvc.perform(post("/api/tasks/M_3_1/accept")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());
        completeAndClaim(token, "M_3_1", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2007,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2008,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_3", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2009,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_4", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2010,
                  "success": true
                }
                """);

        String response = mockMvc.perform(get("/api/tasks/current/main")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode json = objectMapper.readTree(response);
        assertThat(json.get("data").get("task").isNull()).isTrue();
    }

    @Test
    void shouldRejectPhotoCheckinWhenOutsideBuildingRadius() throws Exception {
        String token = createRoleAndLoginToken();

        completeAndClaim(token, "M_1_1", """
                {
                  "eventType": "AI_DIALOGUE",
                  "success": true
                }
                """);

        completeAndClaim(token, "M_1_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2001,
                  "success": true
                }
                """);

        completeAndClaim(token, "M_1_3", """
                {
                  "eventType": "NPC_DIALOGUE",
                  "targetId": 1003,
                  "success": true
                }
                """);

        mockMvc.perform(post("/api/tasks/events")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "eventType": "LANDMARK_VISIT",
                                  "targetId": 2004,
                                  "success": true
                                }
                                """))
                .andExpect(status().isOk());

        mockMvc.perform(post("/api/tasks/events")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "eventType": "PHOTO_CHECKIN",
                                  "targetId": 2004,
                                  "currentPosX": 500.0,
                                  "currentPosY": 0.0,
                                  "currentPosZ": 0.0
                                }
                                """))
                .andExpect(status().isBadRequest());
    }

    @Test
    void shouldRejectDuplicateClaimForSameTask() throws Exception {
        String token = createRoleAndLoginToken();

        completeAndClaim(token, "M_1_1", """
                {
                  "eventType": "AI_DIALOGUE",
                  "success": true
                }
                """);

        mockMvc.perform(post("/api/tasks/M_1_1/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());
    }

    @Test
    void shouldRejectManualAcceptWhenPrerequisiteNotSatisfied() throws Exception {
        String token = createRoleAndLoginToken();

        mockMvc.perform(post("/api/tasks/M_3_1/accept")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());
    }

    @Test
    void shouldGrantTaskTitlesForKeyMainTasks() throws Exception {
        String token = createRoleAndLoginToken();
        Long roleId = currentRoleId(token);

        completeAndClaim(token, "M_1_1", """
                {
                  "eventType": "AI_DIALOGUE",
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2001,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_3", """
                {
                  "eventType": "NPC_DIALOGUE",
                  "targetId": 1003,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_4", """
                {
                  "eventType": "NPC_DIALOGUE",
                  "targetId": 1004,
                  "success": true
                }
                """);

        mockMvc.perform(post("/api/tasks/M_2_1/accept")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());
        completeAndClaim(token, "M_2_1", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2004,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_2_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2005,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_2_3", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2006,
                  "success": true
                }
                """);

        mockMvc.perform(post("/api/tasks/M_3_1/accept")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());
        completeAndClaim(token, "M_3_1", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2007,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2008,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_3", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2009,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_4", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2010,
                  "success": true
                }
                """);

        var unlockedTitleIds = playerTitleRepository.findByRoleIdOrderByTitleIdAsc(roleId)
                .stream()
                .map(title -> title.getTitleId().intValue())
                .toList();

        assertThat(unlockedTitleIds).contains(2, 3, 4, 5, 6);
    }

    @Test
    void shouldGrantChapterTitlesAfterEachChapterCompletion() throws Exception {
        String token = createRoleAndLoginToken();
        Long roleId = currentRoleId(token);

        completeAndClaim(token, "M_1_1", """
                {
                  "eventType": "AI_DIALOGUE",
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2001,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_3", """
                {
                  "eventType": "NPC_DIALOGUE",
                  "targetId": 1003,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_1_4", """
                {
                  "eventType": "NPC_DIALOGUE",
                  "targetId": 1004,
                  "success": true
                }
                """);
        assertThat(unlockedTitleIds(roleId)).contains(7);

        mockMvc.perform(post("/api/tasks/M_2_1/accept")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());
        completeAndClaim(token, "M_2_1", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2004,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_2_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2005,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_2_3", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2006,
                  "success": true
                }
                """);
        assertThat(unlockedTitleIds(roleId)).contains(8);

        mockMvc.perform(post("/api/tasks/M_3_1/accept")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());
        completeAndClaim(token, "M_3_1", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2007,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_2", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2008,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_3", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2009,
                  "success": true
                }
                """);
        completeAndClaim(token, "M_3_4", """
                {
                  "eventType": "ARRIVE_BUILDING",
                  "targetId": 2010,
                  "success": true
                }
                """);
        assertThat(unlockedTitleIds(roleId)).contains(9);
    }

    private void sendPhotoEvent(String token, long targetId) throws Exception {
        String body = """
                {
                  "eventType": "PHOTO_CHECKIN",
                  "targetId": %d,
                  "currentPosX": 10.0,
                  "currentPosY": 0.0,
                  "currentPosZ": 5.0,
                  "distanceToTarget": 2.0,
                  "success": true
                }
                """.formatted(targetId);
        mockMvc.perform(post("/api/tasks/events")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(body))
                .andExpect(status().isOk());
    }

    private void completeAndClaim(String token, String taskCode, String eventBody) throws Exception {
        mockMvc.perform(post("/api/tasks/events")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(eventBody))
                .andExpect(status().isOk());
        mockMvc.perform(post("/api/tasks/%s/claim".formatted(taskCode))
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());
    }

    private Long currentRoleId(String token) throws Exception {
        String meResponse = mockMvc.perform(get("/api/player/me")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        JsonNode meJson = objectMapper.readTree(meResponse);
        return meJson.get("data").get("currentRoleId").asLong();
    }

    private java.util.List<Integer> unlockedTitleIds(Long roleId) {
        return playerTitleRepository.findByRoleIdOrderByTitleIdAsc(roleId)
                .stream()
                .map(title -> title.getTitleId().intValue())
                .toList();
    }

    private String createRoleAndLoginToken() throws Exception {
        String mailbox = "task-" + UUID.randomUUID() + "@example.com";

        mockMvc.perform(post("/api/auth/send-code")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "mailbox": "%s"
                                }
                                """.formatted(mailbox)))
                .andExpect(status().isOk());

        String verificationCode = latestVerificationCode(mailbox);

        mockMvc.perform(post("/api/auth/register")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "mailbox": "%s",
                                  "password": "Password123",
                                  "verificationCode": "%s"
                                }
                                """.formatted(mailbox, verificationCode)))
                .andExpect(status().isOk());

        String loginResponse = mockMvc.perform(post("/api/auth/login")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "mailbox": "%s",
                                  "password": "Password123"
                                }
                                """.formatted(mailbox)))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        String token = objectMapper.readTree(loginResponse).get("data").get("token").asText();

        mockMvc.perform(post("/api/player/roles")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "campusName": "此间校园",
                                  "nickName": "测试角色",
                                  "characterId": 1
                                }
                                """))
                .andExpect(status().isOk());

        return token;
    }

    private String latestVerificationCode(String mailbox) {
        return emailVerificationCodeRepository.findFirstByMailboxAndIsUsedOrderByCreatedAtDesc(mailbox, 0)
                .orElseThrow()
                .getVerificationCode();
    }
}
