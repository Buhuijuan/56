package com.mycampus.backend.persistence;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.ApplicationRunner;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Component;

@Component
public class SchemaCompatibilityInitializer implements ApplicationRunner {

    private static final Logger log = LoggerFactory.getLogger(SchemaCompatibilityInitializer.class);

    private final JdbcTemplate jdbcTemplate;

    public SchemaCompatibilityInitializer(JdbcTemplate jdbcTemplate) {
        this.jdbcTemplate = jdbcTemplate;
    }

    @Override
    public void run(org.springframework.boot.ApplicationArguments args) {
        ensureTaskConfigColumns();
        ensureQuizStateColumns();
    }

    private void ensureTaskConfigColumns() {
        if (!tableExists("task_config")) {
            return;
        }

        ensureNullableColumn("task_config", "elf_npc_name", "varchar(64)");
        ensureNullableColumn("task_config", "elf_avatar_key", "varchar(64)");
        ensureNullableColumn("task_config", "elf_start_prompt_json", "longtext");
        ensureNullableColumn("task_config", "elf_progress_prompt_json", "longtext");
        ensureNullableColumn("task_config", "elf_complete_prompt_json", "longtext");
    }

    private void ensureQuizStateColumns() {
        if (!tableExists("player_quiz_state")) {
            return;
        }
        ensureNullableColumn("player_quiz_state", "weekly_reward_claimed", "bit(1)");
    }

    private void ensureNullableColumn(String tableName, String columnName, String definition) {
        if (columnExists(tableName, columnName)) {
            return;
        }
        log.info("Adding missing column {}.{}", tableName, columnName);
        jdbcTemplate.execute("alter table " + tableName + " add column " + columnName + " " + definition);
    }

    private boolean tableExists(String tableName) {
        Integer count = jdbcTemplate.queryForObject(
                "select count(*) from information_schema.tables where table_schema = database() and lower(table_name) = lower(?)",
                Integer.class,
                tableName
        );
        return count != null && count > 0;
    }

    private boolean columnExists(String tableName, String columnName) {
        Integer count = jdbcTemplate.queryForObject(
                "select count(*) from information_schema.columns where table_schema = database() and lower(table_name) = lower(?) and lower(column_name) = lower(?)",
                Integer.class,
                tableName,
                columnName
        );
        return count != null && count > 0;
    }
}
