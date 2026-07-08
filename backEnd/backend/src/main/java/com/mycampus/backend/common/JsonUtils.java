package com.mycampus.backend.common;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.util.Collections;
import java.util.Map;
import java.util.Set;

public final class JsonUtils {

    private static final ObjectMapper OBJECT_MAPPER = new ObjectMapper()
            .findAndRegisterModules()
            .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);

    private JsonUtils() {
    }

    public static ObjectMapper mapper() {
        return OBJECT_MAPPER;
    }

    public static String toJson(Object value) {
        try {
            return OBJECT_MAPPER.writeValueAsString(value);
        } catch (JsonProcessingException e) {
            throw new IllegalStateException("Failed to serialize json", e);
        }
    }

    public static <T> T fromJson(String value, Class<T> type) {
        try {
            return OBJECT_MAPPER.readValue(value, type);
        } catch (Exception e) {
            throw new IllegalStateException("Failed to deserialize json", e);
        }
    }

    public static <T> T fromJson(String value, TypeReference<T> typeReference) {
        try {
            return OBJECT_MAPPER.readValue(value, typeReference);
        } catch (Exception e) {
            throw new IllegalStateException("Failed to deserialize json", e);
        }
    }

    public static Set<Integer> integerSet(String value) {
        if (value == null || value.isBlank()) {
            return Collections.emptySet();
        }
        return fromJson(value, new TypeReference<>() {
        });
    }

    public static Set<String> stringSet(String value) {
        if (value == null || value.isBlank()) {
            return Collections.emptySet();
        }
        return fromJson(value, new TypeReference<>() {
        });
    }

    public static Map<String, Boolean> stringBooleanMap(String value) {
        if (value == null || value.isBlank()) {
            return Collections.emptyMap();
        }
        return fromJson(value, new TypeReference<>() {
        });
    }
}
