package com.mycampus.backend.api;

import com.mycampus.backend.common.ApiResponse;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.service.GameHomeService;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.Map;

@RestController
@RequestMapping("/api/game")
public class GameController {

    private final GameHomeService gameHomeService;

    public GameController(GameHomeService gameHomeService) {
        this.gameHomeService = gameHomeService;
    }

    @GetMapping("/home")
    public ApiResponse<Map<String, Object>> home(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(gameHomeService.home(principal));
    }
}
