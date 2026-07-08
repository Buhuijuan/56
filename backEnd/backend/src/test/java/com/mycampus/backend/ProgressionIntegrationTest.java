package com.mycampus.backend;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.mycampus.backend.auth.repository.AccountRepository;
import com.mycampus.backend.auth.repository.EmailVerificationCodeRepository;
import com.mycampus.backend.player.entity.PlayerProfile;
import com.mycampus.backend.player.repository.PlayerProfileRepository;
import com.mycampus.backend.player.repository.PlayerTitleRepository;
import com.mycampus.backend.progression.entity.PlayerTitleState;
import com.mycampus.backend.progression.repository.PlayerTitleStateRepository;
import com.mycampus.backend.signin.entity.PlayerSignInState;
import com.mycampus.backend.signin.repository.PlayerSignInStateRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import java.time.LocalDate;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
class ProgressionIntegrationTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private PlayerSignInStateRepository signInRepository;

    @Autowired
    private PlayerProfileRepository profileRepository;

    @Autowired
    private PlayerTitleStateRepository titleStateRepository;

    @Autowired
    private PlayerTitleRepository playerTitleRepository;

    @Autowired
    private AccountRepository accountRepository;

    @Autowired
    private EmailVerificationCodeRepository emailVerificationCodeRepository;

    @Test
    void shouldRefreshGrowthStateAfterMainlineAndLoginProgress() throws Exception {
        String token = createRoleAndLoginToken();
        Long roleId = currentRoleId(token);

        completeGrowthRequirements(token);

        PlayerSignInState signIn = signInRepository.findById(roleId).orElseThrow();
        signIn.setTotalLoginDays(3);
        signIn.setLastSignInDate(LocalDate.now());
        signInRepository.save(signIn);

        String refreshResponse = mockMvc.perform(post("/api/growth/refresh")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode refreshJson = objectMapper.readTree(refreshResponse);
        assertThat(refreshJson.get("data").get("updated").asBoolean()).isTrue();

        String growthResponse = mockMvc.perform(get("/api/growth")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode growthJson = objectMapper.readTree(growthResponse).get("data");
        JsonNode growthState = growthJson.get("growthState");

        assertThat(growthState.get("taskCompleted").toString())
                .contains("ST1_GT1", "ST1_GT2", "ST1_GT3");
        assertThat(growthState.get("stageCompleted").toString())
                .contains("ST1", "ST2", "ST3", "ST4", "ST5");
    }

    @Test
    void shouldClaimGrowthRewardGrantCoinUnlockTitleAndAllowEquip() throws Exception {
        String token = createRoleAndLoginToken();
        Long roleId = currentRoleId(token);

        completeGrowthRequirements(token);
        PlayerSignInState signIn = signInRepository.findById(roleId).orElseThrow();
        signIn.setTotalLoginDays(3);
        signIn.setLastSignInDate(LocalDate.now());
        signInRepository.save(signIn);

        mockMvc.perform(post("/api/growth/refresh")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());

        int coinBefore = profileRepository.findById(roleId).map(PlayerProfile::getCoin).orElseThrow();

        String claimResponse = mockMvc.perform(post("/api/growth/ST1/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode claimJson = objectMapper.readTree(claimResponse).get("data");
        assertThat(claimJson.get("claimed").asBoolean()).isTrue();
        assertThat(claimJson.get("addedCoin").asInt()).isEqualTo(10);
        assertThat(claimJson.get("titleUnlocked").asBoolean()).isTrue();
        assertThat(claimJson.get("titleId").asInt()).isEqualTo(31);

        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        assertThat(profile.getCoin()).isEqualTo(coinBefore + 10);

        PlayerTitleState titleState = titleStateRepository.findById(roleId).orElseThrow();
        assertThat(titleState.getUnlockedTitleIds()).contains(31);
        assertThat(playerTitleRepository.findByRoleIdOrderByTitleIdAsc(roleId)
                .stream()
                .map(record -> record.getTitleId().intValue())
                .toList()).contains(31);

        mockMvc.perform(post("/api/growth/ST1/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());

        mockMvc.perform(post("/api/titles/31/equip")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());

        PlayerProfile equippedProfile = profileRepository.findById(roleId).orElseThrow();
        assertThat(equippedProfile.getCurrentTitleId()).isEqualTo(31L);
    }

    @Test
    void shouldRejectGrowthClaimBeforeStageCompleted() throws Exception {
        String token = createRoleAndLoginToken();

        mockMvc.perform(post("/api/growth/ST1/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());
    }

    private void completeChapterOne(String token) throws Exception {
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
    }

    private void completeGrowthRequirements(String token) throws Exception {
        completeChapterOne(token);
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

    private String createRoleAndLoginToken() throws Exception {
        String mailbox = "growth-" + UUID.randomUUID() + "@example.com";

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
                                  "nickName": "GrowthTester",
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
