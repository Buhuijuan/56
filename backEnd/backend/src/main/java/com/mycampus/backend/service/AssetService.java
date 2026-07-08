package com.mycampus.backend.service;

import com.mycampus.backend.api.dto.AssetDtos;
import com.mycampus.backend.game.config.GameConfigService;
import com.mycampus.backend.player.entity.PlayerProfile;
import com.mycampus.backend.player.entity.PlayerTitle;
import com.mycampus.backend.player.repository.PlayerProfileRepository;
import com.mycampus.backend.player.repository.PlayerTitleRepository;
import com.mycampus.backend.progression.entity.PlayerLevelState;
import com.mycampus.backend.progression.entity.PlayerTitleState;
import com.mycampus.backend.progression.repository.PlayerLevelStateRepository;
import com.mycampus.backend.progression.repository.PlayerTitleStateRepository;
import com.mycampus.backend.security.CurrentAccount;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;

@Service
public class AssetService {

    private final PlayerService playerService;
    private final PlayerProfileRepository profileRepository;
    private final PlayerLevelStateRepository levelRepository;
    private final PlayerTitleStateRepository titleStateRepository;
    private final PlayerTitleRepository playerTitleRepository;
    private final GameConfigService gameConfigService;

    public AssetService(PlayerService playerService,
                        PlayerProfileRepository profileRepository,
                        PlayerLevelStateRepository levelRepository,
                        PlayerTitleStateRepository titleStateRepository,
                        PlayerTitleRepository playerTitleRepository,
                        GameConfigService gameConfigService) {
        this.playerService = playerService;
        this.profileRepository = profileRepository;
        this.levelRepository = levelRepository;
        this.titleStateRepository = titleStateRepository;
        this.playerTitleRepository = playerTitleRepository;
        this.gameConfigService = gameConfigService;
    }

    public AssetDtos.AssetSnapshot snapshot(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        PlayerLevelState level = levelRepository.findById(roleId).orElseThrow();
        PlayerTitleState titleState = titleStateRepository.findById(roleId).orElseThrow();

        return new AssetDtos.AssetSnapshot(
                new AssetDtos.ProfileSummary(roleId, profile.getNickname()),
                toWallet(level, profile),
                toTitles(profile, titleState),
                new AssetDtos.FeatureSummary(profile.getBikeUnlocked() != null && profile.getBikeUnlocked() == 1),
                buildInventory(roleId),
                buildLedger(roleId)
        );
    }

    public AssetDtos.WalletSummary wallet(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        PlayerLevelState level = levelRepository.findById(roleId).orElseThrow();
        return toWallet(level, profile);
    }

    public AssetDtos.TitleSummary titles(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        PlayerTitleState titleState = titleStateRepository.findById(roleId).orElseThrow();
        return toTitles(profile, titleState);
    }

    public AssetDtos.FeatureSummary features(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        return new AssetDtos.FeatureSummary(profile.getBikeUnlocked() != null && profile.getBikeUnlocked() == 1);
    }

    public List<AssetDtos.InventoryItem> inventory(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        return buildInventory(roleId);
    }

    public List<AssetDtos.RewardLedgerItem> ledger(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        return buildLedger(roleId);
    }

    private AssetDtos.WalletSummary toWallet(PlayerLevelState level, PlayerProfile profile) {
        return new AssetDtos.WalletSummary(level.getLevel(), level.getExp(), profile.getCoin());
    }

    private AssetDtos.TitleSummary toTitles(PlayerProfile profile, PlayerTitleState titleState) {
        List<Integer> unlocked = new ArrayList<>(titleState.getUnlockedTitleIds());
        unlocked.sort(Integer::compareTo);
        Long currentTitleId = profile.getCurrentTitleId();
        if (currentTitleId == null && titleState.getEquippedTitleId() != null) {
            currentTitleId = titleState.getEquippedTitleId().longValue();
        }
        return new AssetDtos.TitleSummary(currentTitleId, unlocked);
    }

    private List<AssetDtos.InventoryItem> buildInventory(Long roleId) {
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        List<AssetDtos.InventoryItem> items = new ArrayList<>();
        items.add(new AssetDtos.InventoryItem(1, "纪念币", profile.getCoin()));

        List<PlayerTitle> titles = playerTitleRepository.findByRoleIdOrderByTitleIdAsc(roleId);
        if (!titles.isEmpty()) {
            items.add(new AssetDtos.InventoryItem(9001, "已解锁称号数", titles.size()));
        }
        return items;
    }

    private List<AssetDtos.RewardLedgerItem> buildLedger(Long roleId) {
        List<PlayerTitle> titles = playerTitleRepository.findByRoleIdOrderByTitleIdAsc(roleId);
        return titles.stream()
                .sorted(Comparator.comparing(PlayerTitle::getUnlockedAt, Comparator.nullsLast(Comparator.naturalOrder())).reversed())
                .limit(20)
                .map(title -> new AssetDtos.RewardLedgerItem(
                        title.getSourceType() == null ? "TITLE" : title.getSourceType(),
                        title.getTitleId() == null ? "" : String.valueOf(title.getTitleId()),
                        "TITLE",
                        title.getSourceRefId(),
                        1,
                        title.getUnlockedAt() == null ? "" : title.getUnlockedAt().toString()
                ))
                .toList();
    }
}
