package com.mycampus.backend;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
class CampusAiControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @Test
    void shouldKeepUnityCompatibleResponseShape() throws Exception {
        mockMvc.perform(post("/api/campus/qa")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "question": "图书馆怎么走",
                                  "player_id": "unity_player_001",
                                  "scene_name": "04_Campus"
                                }
                                """))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(0))
                .andExpect(jsonPath("$.data.answer").isNotEmpty())
                .andExpect(jsonPath("$.data.target_scene").value("04_Campus"))
                .andExpect(jsonPath("$.data.target_position.x").exists());
    }
}
