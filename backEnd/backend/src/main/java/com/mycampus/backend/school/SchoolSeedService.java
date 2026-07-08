package com.mycampus.backend.school;

import com.mycampus.backend.school.entity.School;
import com.mycampus.backend.school.repository.SchoolRepository;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.stereotype.Component;

import java.util.List;

@Component
public class SchoolSeedService implements ApplicationRunner {

    private final SchoolRepository schoolRepository;

    public SchoolSeedService(SchoolRepository schoolRepository) {
        this.schoolRepository = schoolRepository;
    }

    @Override
    public void run(ApplicationArguments args) {
        List<School> schools = List.of(
                build("mycampus", "安心大学"),
                build("library-campus", "书香校区"),
                build("science-campus", "科创校区"),
                build("main-campus", "主校区")
        );
        schools.forEach(source -> {
            School target = schoolRepository.findBySchoolCode(source.getSchoolCode())
                    .or(() -> schoolRepository.findBySchoolNameIgnoreCase(source.getSchoolName()))
                    .orElseGet(School::new);
            target.setSchoolCode(source.getSchoolCode());
            target.setSchoolName(source.getSchoolName());
            target.setStatus(source.getStatus());
            schoolRepository.save(target);
        });
    }

    private School build(String code, String name) {
        School school = new School();
        school.setSchoolCode(code);
        school.setSchoolName(name);
        school.setStatus(1);
        return school;
    }
}
