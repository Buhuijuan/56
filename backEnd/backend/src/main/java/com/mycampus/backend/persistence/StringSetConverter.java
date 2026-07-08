package com.mycampus.backend.persistence;

import com.mycampus.backend.common.JsonUtils;
import jakarta.persistence.AttributeConverter;
import jakarta.persistence.Converter;

import java.util.LinkedHashSet;
import java.util.Set;

@Converter
public class StringSetConverter implements AttributeConverter<Set<String>, String> {

    @Override
    public String convertToDatabaseColumn(Set<String> attribute) {
        return JsonUtils.toJson(attribute == null ? Set.of() : attribute);
    }

    @Override
    public Set<String> convertToEntityAttribute(String dbData) {
        return new LinkedHashSet<>(JsonUtils.stringSet(dbData));
    }
}
