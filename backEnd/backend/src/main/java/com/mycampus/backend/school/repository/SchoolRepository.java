package com.mycampus.backend.school.repository;

import com.mycampus.backend.school.entity.School;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface SchoolRepository extends JpaRepository<School, Long> {
    List<School> findByStatusOrderByIdAsc(Integer status);
    Optional<School> findByIdAndStatus(Long id, Integer status);
    Optional<School> findBySchoolNameIgnoreCaseAndStatus(String schoolName, Integer status);
    Optional<School> findBySchoolNameIgnoreCase(String schoolName);
    Optional<School> findBySchoolCode(String schoolCode);
    boolean existsBySchoolCode(String schoolCode);
}
