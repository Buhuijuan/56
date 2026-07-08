package com.mycampus.backend.task.repository;

import com.mycampus.backend.task.entity.BuildingLocation;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface BuildingLocationRepository extends JpaRepository<BuildingLocation, Long> {
    Optional<BuildingLocation> findByBuildingCode(String buildingCode);
    List<BuildingLocation> findBySchoolIdAndStatusOrderByBuildingLocationIdAsc(Long schoolId, Integer status);
}
