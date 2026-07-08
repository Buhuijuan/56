package com.mycampus.backend.service;

import com.mycampus.backend.common.AppException;
import com.mycampus.backend.game.config.GameConfigService;
import com.mycampus.backend.player.entity.PlayerStat;
import com.mycampus.backend.player.repository.PlayerStatRepository;
import com.mycampus.backend.player.repository.RoleRepository;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.signin.entity.PlayerSignInState;
import com.mycampus.backend.signin.repository.PlayerSignInStateRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.DayOfWeek;
import java.time.LocalDate;
import java.time.temporal.WeekFields;
import java.util.LinkedHashMap;
import java.util.Locale;
import java.util.List;
import java.util.Map;

@Service
public class SignInService {

    private final PlayerService playerService;
    private final PlayerSignInStateRepository signInRepository;
    private final PlayerStatRepository playerStatRepository;
    private final RoleRepository roleRepository;
    private final GameConfigService gameConfigService;
    private final ProgressionService progressionService;

    public SignInService(PlayerService playerService,
                         PlayerSignInStateRepository signInRepository,
                         PlayerStatRepository playerStatRepository,
                         RoleRepository roleRepository,
                         GameConfigService gameConfigService,
                         ProgressionService progressionService) {
        this.playerService = playerService;
        this.signInRepository = signInRepository;
        this.playerStatRepository = playerStatRepository;
        this.roleRepository = roleRepository;
        this.gameConfigService = gameConfigService;
        this.progressionService = progressionService;
    }

    public Map<String, Object> getStatus(CurrentAccount principal) {
        PlayerSignInState state = stateFor(principal);
        normalize(state);
        signInRepository.save(state);
        return Map.of(
                "state", state,
                "dailyAwards", gameConfigService.dailyAwards(),
                "onlineAwards", gameConfigService.onlineAwards(),
                "totalAwards", gameConfigService.totalAwards()
        );
    }

    @Transactional
    public Map<String, Object> dailySign(CurrentAccount principal) {
        PlayerSignInState state = stateFor(principal);
        normalize(state);
        if (Boolean.TRUE.equals(state.getDailySigned())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "今日已签到");
        }
        LocalDate today = LocalDate.now();
        if (state.getLastSignInDate() != null && state.getLastSignInDate().plusDays(1).equals(today)) {
            state.setContinuousSignDays(state.getContinuousSignDays() + 1);
        } else {
            state.setContinuousSignDays(1);
        }
        state.setLastSignInDate(today);
        state.setDailySigned(true);
        state.setTotalLoginDays(state.getTotalLoginDays() + 1);
        syncLoginDays(state.getRoleId(), state.getTotalLoginDays());
        int weekDay = today.getDayOfWeek() == DayOfWeek.SUNDAY ? 7 : today.getDayOfWeek().getValue();
        state.getDailyRewardClaimed().add(weekDay);
        signInRepository.save(state);

        int addedCoin = gameConfigService.dailyAwards().stream()
                .filter(item -> item.dayIndex() != null && item.dayIndex().equals(weekDay))
                .findFirst()
                .map(this::sumDailyAwardCoins)
                .orElse(0);
        if (addedCoin > 0) {
            progressionService.grantCoins(state.getRoleId(), "SIGN_IN_DAILY", "DAY_" + weekDay, addedCoin);
        }

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("signed", true);
        result.put("continuousSignDays", state.getContinuousSignDays());
        result.put("totalLoginDays", state.getTotalLoginDays());
        result.put("addedCoin", addedCoin);
        return result;
    }

    @Transactional
    public Map<String, Object> claimOnline(CurrentAccount principal, Integer awardId) {
        PlayerSignInState state = stateFor(principal);
        normalize(state);
        var config = gameConfigService.onlineAwards().stream()
                .filter(item -> item.awardID().equals(awardId))
                .findFirst()
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "在线奖励不存在"));
        if (state.getOnlineRewardClaimed().contains(awardId)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "在线奖励已领取");
        }
        if (state.getTodayOnlineSeconds() < config.requiredMinutes() * 60) {
            throw new AppException(HttpStatus.BAD_REQUEST, "在线时长不足");
        }
        state.getOnlineRewardClaimed().add(awardId);
        signInRepository.save(state);
        int addedCoin = sumRewardCoins(config.rewards());
        if (addedCoin > 0) {
            progressionService.grantCoins(state.getRoleId(), "SIGN_IN_ONLINE", String.valueOf(awardId), addedCoin);
        }
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("awardId", awardId);
        result.put("claimed", true);
        result.put("rewards", config.rewards());
        result.put("addedCoin", addedCoin);
        return result;
    }

    @Transactional
    public Map<String, Object> claimTotal(CurrentAccount principal, Integer awardId) {
        PlayerSignInState state = stateFor(principal);
        var config = gameConfigService.totalAward(awardId)
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "累计奖励不存在"));
        if (state.getTotalRewardClaimed().contains(awardId)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "累计奖励已领取");
        }
        if (state.getTotalLoginDays() < config.requiredDays()) {
            throw new AppException(HttpStatus.BAD_REQUEST, "累计签到天数不足");
        }
        state.getTotalRewardClaimed().add(awardId);
        signInRepository.save(state);
        var role = playerService.currentRole(principal);
        role.getUnlockedCharacterIds().add(config.rewardCharacterID());
        roleRepository.save(role);
        return Map.of("awardId", awardId, "claimed", true, "characterUnlocked", config.rewardCharacterID(), "addedCoin", 0);
    }

    private int sumDailyAwardCoins(com.mycampus.backend.game.config.GameConfigs.DailyAwardConfig config) {
        if (config == null) {
            return 0;
        }
        return sumRewardCoin(config.baseReward()) + sumRewardCoin(config.extraReward());
    }

    private int sumRewardCoins(List<com.mycampus.backend.game.config.GameConfigs.RewardRef> rewards) {
        if (rewards == null) {
            return 0;
        }
        return rewards.stream().mapToInt(this::sumRewardCoin).sum();
    }

    private int sumRewardCoin(com.mycampus.backend.game.config.GameConfigs.RewardRef reward) {
        if (reward == null || reward.rewardId() == null || reward.amount() == null) {
            return 0;
        }
        return reward.rewardId() == 1 ? reward.amount() : 0;
    }

    private PlayerSignInState stateFor(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        return signInRepository.findById(roleId).orElseThrow();
    }

    private void normalize(PlayerSignInState state) {
        LocalDate today = LocalDate.now();
        int currentWeek = today.get(WeekFields.of(Locale.getDefault()).weekOfWeekBasedYear());
        if (state.getCurrentWeekIndex() == null || !state.getCurrentWeekIndex().equals(currentWeek)) {
            state.setCurrentWeekIndex(currentWeek);
            state.getDailyRewardClaimed().clear();
            state.setContinuousSignDays(0);
            state.setDailySigned(false);
        }
        if (state.getLastSignInDate() == null || !state.getLastSignInDate().equals(today)) {
            state.setDailySigned(false);
        }
    }

    private void syncLoginDays(Long roleId, Integer totalLoginDays) {
        PlayerStat stat = playerStatRepository.findById(roleId).orElse(null);
        if (stat == null) {
            return;
        }
        stat.setLoginDays(totalLoginDays == null ? 0 : totalLoginDays);
        stat.touch();
        playerStatRepository.save(stat);
    }
}
