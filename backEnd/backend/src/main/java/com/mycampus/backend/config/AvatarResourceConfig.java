package com.mycampus.backend.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.servlet.config.annotation.ResourceHandlerRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

import java.nio.file.Path;
import java.nio.file.Paths;

@Configuration
public class AvatarResourceConfig implements WebMvcConfigurer {

    @Value("${app.avatar.public-path:/uploads/avatars}")
    private String publicPath;

    @Value("${app.avatar.storage-path:uploads/avatars}")
    private String storagePath;

    @Override
    public void addResourceHandlers(ResourceHandlerRegistry registry) {
        String normalizedPublicPath = normalizePublicPath(publicPath);
        Path absoluteStoragePath = Paths.get(storagePath).toAbsolutePath().normalize();
        registry.addResourceHandler(normalizedPublicPath + "/**")
                .addResourceLocations(absoluteStoragePath.toUri().toString());
    }

    private String normalizePublicPath(String rawPath) {
        if (rawPath == null || rawPath.isBlank()) {
            return "/uploads/avatars";
        }
        String result = rawPath.startsWith("/") ? rawPath : "/" + rawPath;
        return result.endsWith("/") ? result.substring(0, result.length() - 1) : result;
    }
}
