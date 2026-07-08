package com.mycampus.backend.school;

import com.mycampus.backend.common.AppException;
import com.mycampus.backend.school.entity.School;
import com.mycampus.backend.school.repository.SchoolRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class SchoolService {

    private final SchoolRepository schoolRepository;

    public SchoolService(SchoolRepository schoolRepository) {
        this.schoolRepository = schoolRepository;
    }

    public List<School> listActiveSchools() {
        return schoolRepository.findByStatusOrderByIdAsc(1);
    }

    public School requireActiveSchool(Long schoolId) {
        return schoolRepository.findByIdAndStatus(schoolId, 1)
                .orElseThrow(() -> new AppException(HttpStatus.BAD_REQUEST, "学校不存在或不可用"));
    }
}
