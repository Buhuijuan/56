package com.mycampus.backend;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.mycampus.backend.activity.entity.PlayerQuizState;
import com.mycampus.backend.activity.entity.PlayerStoryState;
import com.mycampus.backend.activity.entity.StoryRecordEntity;
import com.mycampus.backend.activity.repository.PlayerQuizStateRepository;
import com.mycampus.backend.activity.repository.PlayerStoryStateRepository;
import com.mycampus.backend.activity.repository.StoryRecordRepository;
import com.mycampus.backend.auth.repository.AccountRepository;
import com.mycampus.backend.auth.repository.EmailVerificationCodeRepository;
import com.mycampus.backend.player.entity.PlayerProfile;
import com.mycampus.backend.player.repository.PlayerProfileRepository;
import com.mycampus.backend.task.entity.BuildingLocation;
import com.mycampus.backend.task.repository.BuildingLocationRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.context.TestConfiguration;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Primary;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import java.time.Clock;
import java.time.Instant;
import java.time.ZoneId;
import java.util.List;
import java.util.Map;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
class ActivityIntegrationTest {

    @TestConfiguration
    static class FixedClockConfig {
        @Bean
        @Primary
        Clock fixedClock() {
            return Clock.fixed(Instant.parse("2026-03-21T01:00:00Z"), ZoneId.of("Asia/Shanghai"));
        }
    }

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private PlayerProfileRepository profileRepository;

    @Autowired
    private PlayerQuizStateRepository quizStateRepository;

    @Autowired
    private PlayerStoryStateRepository storyStateRepository;

    @Autowired
    private StoryRecordRepository storyRecordRepository;

    @Autowired
    private BuildingLocationRepository buildingLocationRepository;

    @Autowired
    private AccountRepository accountRepository;

    @Autowired
    private EmailVerificationCodeRepository emailVerificationCodeRepository;

    private static final Map<String, String> CLOCK_IN_BUILDING_CODES = Map.of(
            "loc_teaching_building_1", "teaching_building_1",
            "loc_library", "library",
            "loc_clock_tower", "clock_tower",
            "loc_art_square", "art_building",
            "loc_jin_lake", "jinhu",
            "loc_sunset_point", "sunset_point"
    );

    @Test
    void shouldCheckClockInWithinRangeAndRejectDuplicate() throws Exception {
        String token = createRoleAndLoginToken();
        Long roleId = currentRoleId(token);
        int coinBefore = profileRepository.findById(roleId).map(PlayerProfile::getCoin).orElseThrow();

        String currentResponse = mockMvc.perform(get("/api/activities/clockin/current")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode currentJson = objectMapper.readTree(currentResponse).get("data");
        JsonNode firstLocation = currentJson.get("config").get("locations").get(0);
        String locationId = firstLocation.get("locationId").asText();
        BuildingLocation location = buildingLocationRepository.findByBuildingCode(
                        CLOCK_IN_BUILDING_CODES.get(locationId))
                .orElseThrow();
        double x = location.getPosX().doubleValue();
        double y = location.getPosY().doubleValue();
        double z = location.getPosZ().doubleValue();

        String checkResponse = mockMvc.perform(post("/api/activities/clockin/" + locationId + "/check")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "currentPosX": %s,
                                  "currentPosY": %s,
                                  "currentPosZ": %s
                                }
                                """.formatted(x, y, z)))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode checkJson = objectMapper.readTree(checkResponse).get("data");
        assertThat(checkJson.get("locationId").asText()).isEqualTo(locationId);
        assertThat(checkJson.get("addedCoin").asInt()).isEqualTo(10);

        int coinAfter = profileRepository.findById(roleId).map(PlayerProfile::getCoin).orElseThrow();
        assertThat(coinAfter).isEqualTo(coinBefore + 10);

        mockMvc.perform(post("/api/activities/clockin/" + locationId + "/check")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "currentPosX": %s,
                                  "currentPosY": %s,
                                  "currentPosZ": %s
                                }
                                """.formatted(x, y, z)))
                .andExpect(status().isBadRequest());
    }

    @Test
    void shouldStartQuizSubmitAnswersAndRejectSecondStartSameDay() throws Exception {
        String token = createRoleAndLoginToken();
        Long roleId = currentRoleId(token);

        String startResponse = mockMvc.perform(post("/api/activities/quiz/start")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode startJson = objectMapper.readTree(startResponse).get("data");
        JsonNode questions = startJson.get("questions");
        assertThat(questions.isArray()).isTrue();
        assertThat(questions.size()).isGreaterThan(0);

        String answersBody = buildQuizAnswersBody(questions.size(), 0);
        String submitResponse = mockMvc.perform(post("/api/activities/quiz/submit")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(answersBody))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode submitJson = objectMapper.readTree(submitResponse).get("data");
        assertThat(submitJson.get("score").asInt()).isBetween(0, questions.size() * 10);

        PlayerQuizState state = quizStateRepository.findById(roleId).orElseThrow();
        assertThat(state.getWeeklyScore()).isEqualTo(submitJson.get("weeklyScore").asInt());
        assertThat(state.getHasPlayedToday()).isTrue();

        mockMvc.perform(post("/api/activities/quiz/start")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());
    }

    @Test
    void shouldStartStoryChooseSaveAndUpload() throws Exception {
        String token = createRoleAndLoginToken();
        Long roleId = currentRoleId(token);

        String startResponse = mockMvc.perform(post("/api/activities/story/start")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode startJson = objectMapper.readTree(startResponse).get("data");
        assertThat(startJson.get("segments").size()).isEqualTo(1);

        String choiceOneResponse = mockMvc.perform(post("/api/activities/story/choice")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "choice": "Start from the canteen"
                                }
                                """))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        JsonNode choiceOneJson = objectMapper.readTree(choiceOneResponse).get("data");
        assertThat(choiceOneJson.get("finished").asBoolean()).isFalse();

        String choiceTwoResponse = mockMvc.perform(post("/api/activities/story/choice")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "choice": "Keep exploring"
                                }
                                """))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        JsonNode choiceTwoJson = objectMapper.readTree(choiceTwoResponse).get("data");
        assertThat(choiceTwoJson.get("finished").asBoolean()).isTrue();

        String saveResponse = mockMvc.perform(post("/api/activities/story/save")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "finalText": "一段测试校园故事。"
                                }
                                """))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode saveJson = objectMapper.readTree(saveResponse).get("data");
        Long storyId = saveJson.get("storyId").asLong();
        assertThat(saveJson.get("saved").asBoolean()).isTrue();

        PlayerStoryState state = storyStateRepository.findById(roleId).orElseThrow();
        assertThat(state.getHasFinished()).isTrue();
        assertThat(state.getRewardClaimed()).isTrue();

        mockMvc.perform(post("/api/activities/story/upload")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());

        StoryRecordEntity story = storyRecordRepository.findById(storyId).orElseThrow();
        assertThat(story.getUploaded()).isTrue();

        List<StoryRecordEntity> uploadedStories = storyRecordRepository.findByUploadedTrueOrderByCreatedAtDesc();
        assertThat(uploadedStories.stream().map(StoryRecordEntity::getId).toList()).contains(storyId);
    }

    private String buildQuizAnswersBody(int count, int answerValue) {
        StringBuilder builder = new StringBuilder();
        builder.append("{\"answers\":[");
        for (int i = 0; i < count; i++) {
            if (i > 0) {
                builder.append(',');
            }
            builder.append(answerValue);
        }
        builder.append("]}");
        return builder.toString();
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

    private String createRoleAndLoginToken() throws Exception {
        String mailbox = "activity-" + UUID.randomUUID() + "@example.com";

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
                                  "campusName": "Campus",
                                  "nickName": "ActivityTester",
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
