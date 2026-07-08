package com.mycampus.backend.common;

import com.mycampus.backend.security.CurrentAccount;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import java.io.IOException;

@Component
public class ProtectedRequestLoggingFilter extends OncePerRequestFilter {

    private static final Logger log = LoggerFactory.getLogger(ProtectedRequestLoggingFilter.class);

    @Override
    protected boolean shouldNotFilter(HttpServletRequest request) {
        String path = request.getRequestURI();
        return !(path.startsWith("/api/player")
                || path.startsWith("/api/game")
                || path.startsWith("/api/signin")
                || path.startsWith("/api/tasks")
                || path.startsWith("/api/growth")
                || path.startsWith("/api/activities"));
    }

    @Override
    protected void doFilterInternal(HttpServletRequest request,
                                    HttpServletResponse response,
                                    FilterChain filterChain) throws ServletException, IOException {
        long start = System.currentTimeMillis();
        filterChain.doFilter(request, response);

        Authentication authentication = SecurityContextHolder.getContext().getAuthentication();
        String accountInfo = "anonymous";
        if (authentication != null && authentication.getPrincipal() instanceof CurrentAccount currentAccount) {
            accountInfo = currentAccount.id() + "/" + currentAccount.mailbox();
        }

        log.info("API {} {} -> {} account={} cost={}ms",
                request.getMethod(),
                request.getRequestURI(),
                response.getStatus(),
                accountInfo,
                System.currentTimeMillis() - start);
    }
}
