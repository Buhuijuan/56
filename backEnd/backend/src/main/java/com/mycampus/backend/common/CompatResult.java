package com.mycampus.backend.common;

public class CompatResult<T> {

    private Integer code;
    private String message;
    private T data;

    public static <T> CompatResult<T> success(T data) {
        CompatResult<T> result = new CompatResult<>();
        result.setCode(200);
        result.setMessage("success");
        result.setData(data);
        return result;
    }

    public static <T> CompatResult<T> success(String message, T data) {
        CompatResult<T> result = new CompatResult<>();
        result.setCode(200);
        result.setMessage(message);
        result.setData(data);
        return result;
    }

    public static <T> CompatResult<T> fail(String message) {
        CompatResult<T> result = new CompatResult<>();
        result.setCode(500);
        result.setMessage(message);
        result.setData(null);
        return result;
    }

    public Integer getCode() {
        return code;
    }

    public void setCode(Integer code) {
        this.code = code;
    }

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    public T getData() {
        return data;
    }

    public void setData(T data) {
        this.data = data;
    }
}
