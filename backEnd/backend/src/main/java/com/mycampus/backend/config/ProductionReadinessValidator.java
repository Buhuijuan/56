package com.mycampus.backend.config;

import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.core.env.Environment;
import org.springframework.stereotype.Component;

import java.util.Arrays;

@Component
public class ProductionReadinessValidator implements ApplicationRunner {

    private static final String DEFAULT_JWT_SECRET = "change-me-to-a-long-secret-key-for-dev-only-1234567890";

    private final Environment environment;

    public ProductionReadinessValidator(Environment environment) {
        this.environment = environment;
    }

    @Override
    public void run(ApplicationArguments args) {
        if (!Arrays.asList(environment.getActiveProfiles()).contains("prod")) {
            return;
        }

        String jwtSecret = environment.getProperty("app.jwt.secret", "");
        String datasourceUrl = environment.getProperty("spring.datasource.url", "");
        boolean h2ConsoleEnabled = environment.getProperty("spring.h2.console.enabled", Boolean.class, false);

        if (jwtSecret.isBlank() || DEFAULT_JWT_SECRET.equals(jwtSecret)) {
            throw new IllegalStateException("Production startup blocked: APP_JWT_SECRET must be set to a strong secret.");
        }
        if (datasourceUrl.startsWith("jdbc:h2:")) {
            throw new IllegalStateException("Production startup blocked: prod profile cannot use H2.");
        }
        if (h2ConsoleEnabled) {
            throw new IllegalStateException("Production startup blocked: H2 console must remain disabled in prod.");
        }
    }
}
