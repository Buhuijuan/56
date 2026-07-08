package com.mycampus.backend.api;

import com.mycampus.backend.api.dto.AssetDtos;
import com.mycampus.backend.common.ApiResponse;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.service.AssetService;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
public class AssetController {

    private final AssetService assetService;

    public AssetController(AssetService assetService) {
        this.assetService = assetService;
    }

    @GetMapping("/api/assets")
    public ApiResponse<AssetDtos.AssetSnapshot> assets(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(assetService.snapshot(principal));
    }

    @GetMapping("/api/assets/wallet")
    public ApiResponse<AssetDtos.WalletSummary> wallet(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(assetService.wallet(principal));
    }

    @GetMapping("/api/assets/titles")
    public ApiResponse<AssetDtos.TitleSummary> titles(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(assetService.titles(principal));
    }

    @GetMapping("/api/assets/features")
    public ApiResponse<AssetDtos.FeatureSummary> features(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(assetService.features(principal));
    }

    @GetMapping("/api/assets/inventory")
    public ApiResponse<List<AssetDtos.InventoryItem>> inventory(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(assetService.inventory(principal));
    }

    @GetMapping("/api/assets/ledger")
    public ApiResponse<List<AssetDtos.RewardLedgerItem>> ledger(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(assetService.ledger(principal));
    }
}
