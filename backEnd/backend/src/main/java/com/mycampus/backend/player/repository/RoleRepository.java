package com.mycampus.backend.player.repository;

import com.mycampus.backend.player.entity.Role;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface RoleRepository extends JpaRepository<Role, Long> {
    List<Role> findByAccountCodeOrderBySlotNoAscIdAsc(String accountCode);
    List<Role> findByAccountCode(String accountCode);
    Optional<Role> findByIdAndAccountCode(Long id, String accountCode);
    boolean existsByAccountCodeAndSlotNo(String accountCode, Integer slotNo);
    boolean existsByRoleCode(String roleCode);
}
