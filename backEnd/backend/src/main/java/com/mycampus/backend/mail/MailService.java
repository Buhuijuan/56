package com.mycampus.backend.mail;

public interface MailService {

    void sendVerificationCode(String mailbox, String verificationCode);
}
