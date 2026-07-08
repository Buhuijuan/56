package com.mycampus.backend;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.mycampus.backend.auth.repository.AccountRepository;
import com.mycampus.backend.auth.repository.EmailVerificationCodeRepository;
import com.mycampus.backend.school.repository.SchoolRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
class AuthFlowIntegrationTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private AccountRepository accountRepository;

    @Autowired
    private EmailVerificationCodeRepository emailVerificationCodeRepository;

    @Autowired
    private SchoolRepository schoolRepository;

    @Test
    void shouldSendCodeRegisterAndLoginWithoutExposingCode() throws Exception {
        String mailbox = "course-mvp@example.com";

        String sendCodeResponse = mockMvc.perform(post("/api/auth/send-code")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "mailbox": "%s"
                                }
                                """.formatted(mailbox)))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode sendCodeJson = objectMapper.readTree(sendCodeResponse);
        assertThat(sendCodeJson.get("data").has("verificationCode")).isFalse();
        assertThat(sendCodeJson.get("data").get("sent").asBoolean()).isTrue();

        String verificationCode = latestVerificationCode(mailbox);
        assertThat(verificationCode).isNotBlank();

        String registerResponse = mockMvc.perform(post("/api/auth/register")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "mailbox": "%s",
                                  "password": "Password123",
                                  "verificationCode": "%s"
                                }
                                """.formatted(mailbox, verificationCode)))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode registerJson = objectMapper.readTree(registerResponse);
        assertThat(registerJson.get("data").get("token").asText()).isNotBlank();

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

        JsonNode loginJson = objectMapper.readTree(loginResponse);
        assertThat(loginJson.get("data").get("token").asText()).isNotBlank();
    }

    @Test
    void shouldListSchoolsAndCreateRoleBySchoolId() throws Exception {
        String mailbox = "school-role@example.com";
        createAccount(mailbox);
        String token = login(mailbox);

        String schoolsResponse = mockMvc.perform(get("/api/schools")
                        .header("Authorization", "Bearer " + token))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode schools = objectMapper.readTree(schoolsResponse).get("data");
        assertThat(schools.isArray()).isTrue();
        assertThat(schools.size()).isGreaterThan(0);

        Long schoolId = schoolRepository.findByStatusOrderByIdAsc(1).get(0).getId();

        String createRoleResponse = mockMvc.perform(post("/api/player/roles")
                        .header("Authorization", "Bearer " + token)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "schoolId": %d,
                                  "nickName": "SchoolRoleTester",
                                  "characterId": 1
                                }
                                """.formatted(schoolId)))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode roleJson = objectMapper.readTree(createRoleResponse).get("data");
        assertThat(roleJson.get("schoolId").asLong()).isEqualTo(schoolId);
        assertThat(roleJson.get("slotNo").asInt()).isEqualTo(1);
    }

    private void createAccount(String mailbox) throws Exception {
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
    }

    private String login(String mailbox) throws Exception {
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
        return objectMapper.readTree(loginResponse).get("data").get("token").asText();
    }

    private String latestVerificationCode(String mailbox) {
        return emailVerificationCodeRepository.findFirstByMailboxAndIsUsedOrderByCreatedAtDesc(mailbox, 0)
                .orElseThrow()
                .getVerificationCode();
    }
}
