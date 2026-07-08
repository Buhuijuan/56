package com.mycampus.backend.api;

import com.mycampus.backend.api.dto.ActivityDtos;
import com.mycampus.backend.service.CampusAiService;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
@RequestMapping("/api/campus")
public class CampusAiController {

    private final CampusAiService campusAiService;

    public CampusAiController(CampusAiService campusAiService) {
        this.campusAiService = campusAiService;
    }

    @PostMapping("/qa")
    public Map<String, Object> qa(@RequestBody ActivityDtos.CampusQaRequest request) {
        return campusAiService.answer(request);
    }
}
