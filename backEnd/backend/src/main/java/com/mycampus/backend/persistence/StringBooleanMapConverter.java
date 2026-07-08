package com.mycampus.backend.persistence;

import com.mycampus.backend.common.JsonUtils;
import jakarta.persistence.AttributeConverter;
import jakarta.persistence.Converter;

import java.util.LinkedHashMap;
import java.util.Map;

@Converter
public class StringBooleanMapConverter implements AttributeConverter<Map<String, Boolean>, String> {

    @Override
    public String convertToDatabaseColumn(Map<String, Boolean> attribute) {
        return JsonUtils.toJson(attribute == null ? Map.of() : attribute);
    }

    @Override
    public Map<String, Boolean> convertToEntityAttribute(String dbData) {
        return new LinkedHashMap<>(JsonUtils.stringBooleanMap(dbData));
    }
}
