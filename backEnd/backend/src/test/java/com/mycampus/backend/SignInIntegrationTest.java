package com.mycampus.backend;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.mycampus.backend.auth.repository.AccountRepository;
import com.mycampus.backend.auth.repository.EmailVerificationCodeRepository;
import com.mycampus.backend.player.entity.Role;
import com.mycampus.backend.player.repository.RoleRepository;
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
class SignInIntegrationTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private PlayerSignInStateRepository signInRepository;

    @Autowired
    private RoleRepository roleRepository;

    @Autowired
    private AccountRepository accountRepository;

    @Autowired
    private EmailVerificationCodeRepository emailVerificationCodeRepository;

    @Test
    void shouldRejectDuplicateDailySignAndResetWhenWeekChanges() throws Exception {
        String token = createRoleAndLoginToken();

        mockMvc.perform(post("/api/signin/daily")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());

        mockMvc.perform(post("/api/signin/daily")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());

        Long roleId = currentRoleId(token);
        PlayerSignInState state = signInRepository.findById(roleId).orElseThrow();
        state.setCurrentWeekIndex(0);
        state.setDailySigned(true);
        state.setContinuousSignDays(5);
        signInRepository.save(state);

        String statusResponse = mockMvc.perform(get("/api/signin")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode stateJson = objectMapper.readTree(statusResponse).get("data").get("state");
        assertThat(stateJson.get("dailySigned").asBoolean()).isFalse();
        assertThat(stateJson.get("continuousSignDays").asInt()).isEqualTo(0);
        assertThat(stateJson.get("dailyRewardClaimed").size()).isEqualTo(0);
    }

    @Test
    void shouldRejectOnlineAwardBeforeEnoughMinutesAndDuplicateAfterClaim() throws Exception {
        String token = createRoleAndLoginToken();

        JsonNode signInJson = signInState(token);
        JsonNode award = signInJson.get("onlineAwards").get(0);
        int awardId = award.get("awardID").asInt();
        int requiredMinutes = award.get("requiredMinutes").asInt();

        mockMvc.perform(post("/api/signin/online/" + awardId + "/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());

        int requiredSeconds = requiredMinutes * 60;
        while (requiredSeconds > 0) {
            int step = Math.min(120, requiredSeconds);
            mockMvc.perform(post("/api/player/heartbeat")
                            .header("Authorization", "Bearer " + token)
                            .contentType(MediaType.APPLICATION_JSON)
                            .content("""
                                    {
                                      "elapsedSeconds": %d
                                    }
                                    """.formatted(step)))
                    .andExpect(status().isOk());
            requiredSeconds -= step;
        }

        mockMvc.perform(post("/api/signin/online/" + awardId + "/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());

        mockMvc.perform(post("/api/signin/online/" + awardId + "/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());
    }

    @Test
    void shouldRejectTotalAwardBeforeEnoughDaysAndDuplicateAfterClaim() throws Exception {
        String token = createRoleAndLoginToken();
        Long roleId = currentRoleId(token);

        JsonNode signInJson = signInState(token);
        JsonNode award = signInJson.get("totalAwards").get(0);
        int awardId = award.get("awardID").asInt();
        int requiredDays = award.get("requiredDays").asInt();
        int rewardCharacterId = award.get("rewardCharacterID").asInt();

        mockMvc.perform(post("/api/signin/total/" + awardId + "/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());

        PlayerSignInState state = signInRepository.findById(roleId).orElseThrow();
        state.setTotalLoginDays(requiredDays);
        state.setLastSignInDate(LocalDate.now());
        signInRepository.save(state);

        mockMvc.perform(post("/api/signin/total/" + awardId + "/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk());

        mockMvc.perform(post("/api/signin/total/" + awardId + "/claim")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isBadRequest());

        Role role = roleRepository.findById(roleId).orElseThrow();
        assertThat(role.getUnlockedCharacterIds()).contains(rewardCharacterId);
    }

    private JsonNode signInState(String token) throws Exception {
        String response = mockMvc.perform(get("/api/signin")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();
        return objectMapper.readTree(response).get("data");
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
        String mailbox = "signin-" + UUID.randomUUID() + "@example.com";

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
                                  "nickName": "SignInTester",
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
