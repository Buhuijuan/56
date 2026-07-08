package com.mycampus.backend.service;

import com.mycampus.backend.api.dto.PlayerDtos;
import com.mycampus.backend.auth.entity.Account;
import com.mycampus.backend.auth.repository.AccountRepository;
import com.mycampus.backend.auth.repository.EmailVerificationCodeRepository;
import com.mycampus.backend.activity.repository.PlayerClockInStateRepository;
import com.mycampus.backend.activity.repository.PlayerQuizStateRepository;
import com.mycampus.backend.activity.repository.PlayerStoryStateRepository;
import com.mycampus.backend.activity.repository.StoryRecordRepository;
import com.mycampus.backend.common.AppException;
import com.mycampus.backend.player.repository.PlayerProfileRepository;
import com.mycampus.backend.player.repository.PlayerStatRepository;
import com.mycampus.backend.player.repository.PlayerTitleRepository;
import com.mycampus.backend.player.entity.Role;
import com.mycampus.backend.player.repository.RoleRepository;
import com.mycampus.backend.progression.repository.PlayerGrowthStateRepository;
import com.mycampus.backend.progression.repository.PlayerLevelStateRepository;
import com.mycampus.backend.progression.repository.PlayerTitleStateRepository;
import com.mycampus.backend.school.entity.School;
import com.mycampus.backend.school.repository.SchoolRepository;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.signin.entity.PlayerSignInState;
import com.mycampus.backend.signin.repository.PlayerSignInStateRepository;
import com.mycampus.backend.task.repository.PlayerCheckinRecordRepository;
import com.mycampus.backend.task.repository.PlayerTaskRepository;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpStatus;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.InvalidPathException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.UUID;
import java.util.concurrent.ThreadLocalRandom;

@Service
public class PlayerService {

    private static final long MAX_AVATAR_SIZE_BYTES = 2L * 1024 * 1024;
    private static final DateTimeFormatter AVATAR_TIMESTAMP = DateTimeFormatter.ofPattern("yyyyMMddHHmmss");
    private static final Set<String> ALLOWED_AVATAR_EXTENSIONS = Set.of("jpg", "jpeg", "png");

    private final AccountRepository accountRepository;
    private final RoleRepository roleRepository;
    private final SchoolRepository schoolRepository;
    private final PasswordEncoder passwordEncoder;
    private final PlayerStateInitializer playerStateInitializer;
    private final PlayerSignInStateRepository signInRepository;
    private final PlayerProfileRepository profileRepository;
    private final PlayerStatRepository statRepository;
    private final PlayerTitleRepository playerTitleRepository;
    private final PlayerLevelStateRepository levelStateRepository;
    private final PlayerGrowthStateRepository growthStateRepository;
    private final PlayerTitleStateRepository titleStateRepository;
    private final PlayerQuizStateRepository quizStateRepository;
    private final PlayerClockInStateRepository clockInStateRepository;
    private final PlayerStoryStateRepository storyStateRepository;
    private final PlayerTaskRepository playerTaskRepository;
    private final PlayerCheckinRecordRepository playerCheckinRecordRepository;
    private final StoryRecordRepository storyRecordRepository;
    private final EmailVerificationCodeRepository emailVerificationCodeRepository;
    private final Path avatarStoragePath;
    private final String avatarPublicPath;

    public PlayerService(AccountRepository accountRepository,
                         RoleRepository roleRepository,
                         SchoolRepository schoolRepository,
                         PasswordEncoder passwordEncoder,
                         PlayerStateInitializer playerStateInitializer,
                         PlayerSignInStateRepository signInRepository,
                         PlayerProfileRepository profileRepository,
                         PlayerStatRepository statRepository,
                         PlayerTitleRepository playerTitleRepository,
                         PlayerLevelStateRepository levelStateRepository,
                         PlayerGrowthStateRepository growthStateRepository,
                         PlayerTitleStateRepository titleStateRepository,
                         PlayerQuizStateRepository quizStateRepository,
                         PlayerClockInStateRepository clockInStateRepository,
                         PlayerStoryStateRepository storyStateRepository,
                         PlayerTaskRepository playerTaskRepository,
                         PlayerCheckinRecordRepository playerCheckinRecordRepository,
                         StoryRecordRepository storyRecordRepository,
                         EmailVerificationCodeRepository emailVerificationCodeRepository,
                         @Value("${app.avatar.storage-path:uploads/avatars}") String avatarStoragePath,
                         @Value("${app.avatar.public-path:/uploads/avatars}") String avatarPublicPath) {
        this.accountRepository = accountRepository;
        this.roleRepository = roleRepository;
        this.schoolRepository = schoolRepository;
        this.passwordEncoder = passwordEncoder;
        this.playerStateInitializer = playerStateInitializer;
        this.signInRepository = signInRepository;
        this.profileRepository = profileRepository;
        this.statRepository = statRepository;
        this.playerTitleRepository = playerTitleRepository;
        this.levelStateRepository = levelStateRepository;
        this.growthStateRepository = growthStateRepository;
        this.titleStateRepository = titleStateRepository;
        this.quizStateRepository = quizStateRepository;
        this.clockInStateRepository = clockInStateRepository;
        this.storyStateRepository = storyStateRepository;
        this.playerTaskRepository = playerTaskRepository;
        this.playerCheckinRecordRepository = playerCheckinRecordRepository;
        this.storyRecordRepository = storyRecordRepository;
        this.emailVerificationCodeRepository = emailVerificationCodeRepository;
        this.avatarStoragePath = Paths.get(avatarStoragePath).toAbsolutePath().normalize();
        this.avatarPublicPath = normalizeAvatarPublicPath(avatarPublicPath);
    }

    public Account currentAccount(CurrentAccount principal) {
        return accountRepository.findById(principal.id())
                .orElseThrow(() -> new AppException(HttpStatus.UNAUTHORIZED, "账号不存在"));
    }

    public Role currentRole(CurrentAccount principal) {
        Account account = currentAccount(principal);
        if (account.getCurrentRoleId() == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "当前账号尚未创建角色");
        }
        return roleRepository.findByIdAndAccountCode(account.getCurrentRoleId(), account.getAccountCode())
                .orElseThrow(() -> new AppException(HttpStatus.BAD_REQUEST, "当前角色不存在"));
    }

    public Map<String, Object> getMe(CurrentAccount principal) {
        Account account = currentAccount(principal);
        List<Role> roles = roleRepository.findByAccountCodeOrderBySlotNoAscIdAsc(account.getAccountCode());
        Map<String, Object> response = new LinkedHashMap<>();
        response.put("accountId", account.getId());
        response.put("accountCode", account.getAccountCode());
        response.put("mailbox", account.getMailbox());
        response.put("currentRoleId", account.getCurrentRoleId());
        response.put("roles", roles);
        return response;
    }

    @Transactional
    public Role createRole(CurrentAccount principal, PlayerDtos.CreateRoleRequest request) {
        Account account = currentAccount(principal);
        School school = resolveSchool(request);
        if (school == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "请选择学校");
        }

        Role role = new Role();
        role.setAccountCode(account.getAccountCode());
        role.setSchoolId(school.getId());
        role.setCampusName(school.getSchoolName());
        role.setSlotNo(nextAvailableSlot(account.getAccountCode()));
        role.setRoleCode(generateUniqueRoleCode());
        role.setNickName(request.nickName());
        role.setStatus(1);
        role.setCurrentCharacterId(request.characterId());
        role.getUnlockedCharacterIds().add(1);
        role.getUnlockedCharacterIds().add(2);
        role.getUnlockedCharacterIds().add(request.characterId());
        role = roleRepository.save(role);
        role.setCampusName(school.getSchoolName());

        playerStateInitializer.initialize(role.getId());
        if (account.getCurrentRoleId() == null) {
            account.setCurrentRoleId(role.getId());
            accountRepository.save(account);
        }
        return role;
    }

    @Transactional
    public Map<String, Object> switchRole(CurrentAccount principal, PlayerDtos.SwitchRoleRequest request) {
        Account account = currentAccount(principal);
        Role role = roleRepository.findByIdAndAccountCode(request.roleId(), account.getAccountCode())
                .orElseThrow(() -> new AppException(HttpStatus.BAD_REQUEST, "角色不存在"));
        account.setCurrentRoleId(role.getId());
        accountRepository.save(account);
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("currentRoleId", role.getId());
        result.put("roleCode", role.getRoleCode());
        result.put("slotNo", role.getSlotNo());
        return result;
    }

    @Transactional
    public Map<String, Object> updateMailbox(CurrentAccount principal, PlayerDtos.UpdateMailboxRequest request) {
        if (accountRepository.existsByMailbox(request.mailbox())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "该邮箱已被占用");
        }
        Account account = currentAccount(principal);
        account.setMailbox(request.mailbox());
        accountRepository.save(account);
        return Map.of("mailbox", account.getMailbox());
    }

    @Transactional
    public Map<String, Object> updatePassword(CurrentAccount principal, PlayerDtos.UpdatePasswordRequest request) {
        Account account = currentAccount(principal);
        account.setPasswordHash(passwordEncoder.encode(request.newPassword()));
        accountRepository.save(account);
        return Map.of("updated", true);
    }

    @Transactional
    public Map<String, Object> uploadCurrentRoleAvatar(CurrentAccount principal, MultipartFile file) {
        if (file == null || file.isEmpty()) {
            throw new AppException(HttpStatus.BAD_REQUEST, "请先选择头像图片");
        }
        if (file.getSize() > MAX_AVATAR_SIZE_BYTES) {
            throw new AppException(HttpStatus.BAD_REQUEST, "头像图片不能超过 2MB");
        }

        String extension = resolveAvatarExtension(file);
        Role role = currentRole(principal);
        String fileName = "role_" + role.getId() + "_" + LocalDateTime.now().format(AVATAR_TIMESTAMP)
                + "_" + UUID.randomUUID().toString().replace("-", "").substring(0, 8) + "." + extension;
        Path avatarFilePath = avatarStoragePath.resolve(fileName).normalize();
        ensureInsideAvatarStorage(avatarFilePath);

        try {
            Files.createDirectories(avatarStoragePath);
            try (InputStream inputStream = file.getInputStream()) {
                Files.copy(inputStream, avatarFilePath, StandardCopyOption.REPLACE_EXISTING);
            }
        } catch (IOException e) {
            throw new AppException(HttpStatus.INTERNAL_SERVER_ERROR, "头像保存失败，请稍后再试");
        }

        String avatarUrl = avatarPublicPath + "/" + fileName;
        deletePreviousAvatarIfManaged(role.getAvatarUrl(), avatarUrl);
        role.setAvatarUrl(avatarUrl);
        roleRepository.save(role);

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("roleId", role.getId());
        result.put("avatarUrl", avatarUrl);
        return result;
    }

    @Transactional
    public Map<String, Object> deleteRole(CurrentAccount principal, Long roleId) {
        if (roleId == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "角色参数不能为空");
        }

        Account account = currentAccount(principal);
        Role role = roleRepository.findByIdAndAccountCode(roleId, account.getAccountCode())
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "角色不存在"));

        deleteAvatarIfManaged(role.getAvatarUrl());
        deleteRoleState(role.getId());
        roleRepository.delete(role);

        List<Role> remainingRoles = roleRepository.findByAccountCodeOrderBySlotNoAscIdAsc(account.getAccountCode());
        Long nextCurrentRoleId = remainingRoles.isEmpty() ? null : remainingRoles.get(0).getId();
        account.setCurrentRoleId(nextCurrentRoleId);
        accountRepository.save(account);

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("deletedRoleId", roleId);
        result.put("currentRoleId", nextCurrentRoleId);
        result.put("remainingRoleCount", remainingRoles.size());
        return result;
    }

    @Transactional
    public Map<String, Object> deleteAccount(CurrentAccount principal) {
        Account account = currentAccount(principal);
        List<Role> roles = roleRepository.findByAccountCode(account.getAccountCode());
        int deletedRoleCount = roles.size();
        for (Role role : roles) {
            deleteAvatarIfManaged(role.getAvatarUrl());
            deleteRoleState(role.getId());
        }
        roleRepository.deleteAll(roles);
        emailVerificationCodeRepository.deleteByMailbox(account.getMailbox());
        accountRepository.delete(account);

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("deletedAccountId", account.getId());
        result.put("deletedRoleCount", deletedRoleCount);
        result.put("deleted", true);
        return result;
    }

    @Transactional
    public Map<String, Object> heartbeat(CurrentAccount principal, PlayerDtos.HeartbeatRequest request) {
        Role role = currentRole(principal);
        PlayerSignInState signIn = signInRepository.findById(role.getId())
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "签到状态不存在"));
        int elapsed = request.elapsedSeconds() == null ? 30 : Math.max(0, Math.min(120, request.elapsedSeconds()));
        signIn.setTodayOnlineSeconds(signIn.getTodayOnlineSeconds() + elapsed);
        signIn.setLastHeartbeatAt(LocalDateTime.now());
        signInRepository.save(signIn);
        return Map.of(
                "todayOnlineSeconds", signIn.getTodayOnlineSeconds(),
                "todayOnlineMinutes", signIn.getTodayOnlineSeconds() / 60
        );
    }

    private School resolveSchool(PlayerDtos.CreateRoleRequest request) {
        if (request.schoolId() != null) {
            return schoolRepository.findByIdAndStatus(request.schoolId(), 1)
                    .orElseThrow(() -> new AppException(HttpStatus.BAD_REQUEST, "学校不存在或不可用"));
        }
        if (request.campusName() == null || request.campusName().isBlank()) {
            return null;
        }
        return schoolRepository.findBySchoolNameIgnoreCaseAndStatus(request.campusName().trim(), 1)
                .orElse(null);
    }

    private Integer nextAvailableSlot(String accountCode) {
        List<Role> roles = roleRepository.findByAccountCodeOrderBySlotNoAscIdAsc(accountCode);
        int expected = 1;
        for (Role role : roles) {
            Integer slotNo = role.getSlotNo();
            if (slotNo == null || slotNo < expected) {
                continue;
            }
            if (slotNo > expected) {
                break;
            }
            expected++;
        }
        return expected;
    }

    private String generateUniqueRoleCode() {
        for (int attempt = 0; attempt < 20; attempt++) {
            String candidate = "R" + ThreadLocalRandom.current().nextInt(100000, 999999);
            if (!roleRepository.existsByRoleCode(candidate)) {
                return candidate;
            }
        }
        throw new AppException(HttpStatus.INTERNAL_SERVER_ERROR, "角色编码生成失败，请稍后重试");
    }

    private String resolveAvatarExtension(MultipartFile file) {
        String originalName = file.getOriginalFilename();
        if (originalName != null) {
            int dotIndex = originalName.lastIndexOf('.');
            if (dotIndex >= 0 && dotIndex < originalName.length() - 1) {
                String extension = originalName.substring(dotIndex + 1).toLowerCase();
                if (ALLOWED_AVATAR_EXTENSIONS.contains(extension)) {
                    return extension;
                }
            }
        }

        String contentType = file.getContentType();
        if ("image/jpeg".equalsIgnoreCase(contentType) || "image/jpg".equalsIgnoreCase(contentType)) {
            return "jpg";
        }
        if ("image/png".equalsIgnoreCase(contentType)) {
            return "png";
        }
        throw new AppException(HttpStatus.BAD_REQUEST, "头像仅支持 jpg、jpeg、png 格式");
    }

    private void ensureInsideAvatarStorage(Path avatarFilePath) {
        if (!avatarFilePath.startsWith(avatarStoragePath)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "头像文件路径不合法");
        }
    }

    private void deletePreviousAvatarIfManaged(String previousAvatarUrl, String currentAvatarUrl) {
        if (previousAvatarUrl == null || previousAvatarUrl.isBlank()) {
            return;
        }
        if (previousAvatarUrl.equals(currentAvatarUrl)) {
            return;
        }
        String prefix = avatarPublicPath + "/";
        if (!previousAvatarUrl.startsWith(prefix)) {
            return;
        }

        String fileName = previousAvatarUrl.substring(prefix.length());
        if (fileName.isBlank()) {
            return;
        }

        try {
            Path oldFilePath = avatarStoragePath.resolve(fileName).normalize();
            ensureInsideAvatarStorage(oldFilePath);
            Files.deleteIfExists(oldFilePath);
        } catch (IOException | InvalidPathException ignored) {
            // Ignore cleanup failures and keep the newly uploaded avatar.
        }
    }

    private void deleteAvatarIfManaged(String avatarUrl) {
        deletePreviousAvatarIfManaged(avatarUrl, null);
    }

    private void deleteRoleState(Long roleId) {
        playerCheckinRecordRepository.deleteByRoleId(roleId);
        playerTaskRepository.deleteByRoleId(roleId);
        playerTitleRepository.deleteByRoleId(roleId);
        storyRecordRepository.deleteByRoleId(roleId);
        quizStateRepository.deleteAllByIdInBatch(List.of(roleId));
        clockInStateRepository.deleteAllByIdInBatch(List.of(roleId));
        storyStateRepository.deleteAllByIdInBatch(List.of(roleId));
        signInRepository.deleteAllByIdInBatch(List.of(roleId));
        levelStateRepository.deleteAllByIdInBatch(List.of(roleId));
        growthStateRepository.deleteAllByIdInBatch(List.of(roleId));
        titleStateRepository.deleteAllByIdInBatch(List.of(roleId));
        profileRepository.deleteAllByIdInBatch(List.of(roleId));
        statRepository.deleteAllByIdInBatch(List.of(roleId));
    }

    private String normalizeAvatarPublicPath(String rawPath) {
        if (rawPath == null || rawPath.isBlank()) {
            return "/uploads/avatars";
        }
        String result = rawPath.startsWith("/") ? rawPath : "/" + rawPath;
        return result.endsWith("/") ? result.substring(0, result.length() - 1) : result;
    }
}
