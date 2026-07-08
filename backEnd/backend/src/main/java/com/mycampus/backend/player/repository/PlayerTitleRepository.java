package com.mycampus.backend.player.repository;

import com.mycampus.backend.player.entity.PlayerTitle;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface PlayerTitleRepository extends JpaRepository<PlayerTitle, Long> {
    List<PlayerTitle> findByRoleIdOrderByTitleIdAsc(Long roleId);
    Optional<PlayerTitle> findByRoleIdAndTitleId(Long roleId, Long titleId);
    void deleteByRoleId(Long roleId);
}
