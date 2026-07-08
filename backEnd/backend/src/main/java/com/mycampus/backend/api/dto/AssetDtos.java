package com.mycampus.backend.api.dto;

import java.util.List;

public final class AssetDtos {

    private AssetDtos() {
    }

    public record AssetSnapshot(
            ProfileSummary profile,
            WalletSummary wallet,
            TitleSummary titles,
            FeatureSummary features,
            List<InventoryItem> inventory,
            List<RewardLedgerItem> ledger
    ) {
    }

    public record ProfileSummary(
            Long roleId,
            String nickname
    ) {
    }

    public record WalletSummary(
            Integer level,
            Integer exp,
            Integer coin
    ) {
    }

    public record TitleSummary(
            Long currentTitleId,
            List<Integer> unlockedTitleIds
    ) {
    }

    public record FeatureSummary(
            boolean bikeUnlocked
    ) {
    }

    public record InventoryItem(
            Integer rewardId,
            String rewardName,
            Integer amount
    ) {
    }

    public record RewardLedgerItem(
            String sourceType,
            String sourceId,
            String rewardType,
            Long refId,
            Integer amount,
            String createdAt
    ) {
    }
}
