package com.mycampus.backend;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

import java.time.Clock;

@SpringBootApplication
public class MyCampusBackendApplication {

    public static void main(String[] args) {
        SpringApplication.run(MyCampusBackendApplication.class, args);
    }

    @Bean
    public Clock systemClock() {
        return Clock.systemDefaultZone();
    }
}
