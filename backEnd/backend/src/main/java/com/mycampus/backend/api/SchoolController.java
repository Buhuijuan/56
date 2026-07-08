package com.mycampus.backend.api;

import com.mycampus.backend.common.ApiResponse;
import com.mycampus.backend.school.SchoolService;
import com.mycampus.backend.school.entity.School;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
@RequestMapping("/api/schools")
public class SchoolController {

    private final SchoolService schoolService;

    public SchoolController(SchoolService schoolService) {
        this.schoolService = schoolService;
    }

    @GetMapping
    public ApiResponse<List<School>> listSchools() {
        return ApiResponse.ok(schoolService.listActiveSchools());
    }
}
