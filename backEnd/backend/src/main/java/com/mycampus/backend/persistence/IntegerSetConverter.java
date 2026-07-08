package com.mycampus.backend.persistence;

import com.mycampus.backend.common.JsonUtils;
import jakarta.persistence.AttributeConverter;
import jakarta.persistence.Converter;

import java.util.LinkedHashSet;
import java.util.Set;

@Converter
public class IntegerSetConverter implements AttributeConverter<Set<Integer>, String> {

    @Override
    public String convertToDatabaseColumn(Set<Integer> attribute) {
        return JsonUtils.toJson(attribute == null ? Set.of() : attribute);
    }

    @Override
    public Set<Integer> convertToEntityAttribute(String dbData) {
        return new LinkedHashSet<>(JsonUtils.integerSet(dbData));
    }
}
