package workich.auth.service;

import java.util.Set;
import java.util.UUID;
import java.util.concurrent.TimeUnit;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.data.redis.core.RedisTemplate;
import org.springframework.stereotype.Service;

import lombok.RequiredArgsConstructor;
import workich.auth.util.TokenGenerator;

@Service
@RequiredArgsConstructor
public class RefreshTokenService {
    private final RedisTemplate<String, String> redisTemplate;
    private static final String REFRESH_TOKEN_PREFIX = "refresh:token:";
    private static final String USER_TOKENS_PREFIX = "user:tokens:";

    @Value("$token.refresh-length")
    private int refreshTokenLength;

    @Value("$token.refresh-expiration")
    private long refreshTokenExpiration;

    public String generateRefreshToken() {
        return TokenGenerator.generateRefreshToken(refreshTokenLength);
    }

    public void saveRefreshToken(UUID userId, String refreshToken) {
        String tokenKey = REFRESH_TOKEN_PREFIX + refreshToken;
        redisTemplate.opsForValue().set(
            tokenKey, 
            userId.toString(),
            refreshTokenExpiration, 
            TimeUnit.MILLISECONDS
        );
        
        String userKey = USER_TOKENS_PREFIX + userId;
        redisTemplate.opsForSet().add(userKey, refreshToken);
        redisTemplate.expire(userKey, refreshTokenExpiration, TimeUnit.MILLISECONDS);
    }

    public UUID getUserIdByToken(String refreshToken)
    {
        String key = REFRESH_TOKEN_PREFIX + refreshToken;
        String idStr = redisTemplate.opsForValue().get(key);
        return UUID.fromString(idStr);
    }

    public boolean isTokenValid(String refreshToken) {
        String key = REFRESH_TOKEN_PREFIX + refreshToken;
        return Boolean.TRUE.equals(redisTemplate.hasKey(key));
    }

    public void deleteRefreshToken(String refreshToken) {
        String key = REFRESH_TOKEN_PREFIX + refreshToken;
        redisTemplate.delete(key);
    }

    public void deleteAllUserTokens(UUID userId) {
        String userKey = USER_TOKENS_PREFIX + userId;
        Set<String> tokens = redisTemplate.opsForSet().members(userKey);
        
        if (tokens != null && !tokens.isEmpty()) {
            tokens.forEach(token -> {
                String tokenKey = REFRESH_TOKEN_PREFIX + token;
                redisTemplate.delete(tokenKey);
            });
        }
        redisTemplate.delete(userKey);
    }

    public long getUserActiveSessions(UUID userId) {
        String userKey = USER_TOKENS_PREFIX + userId;
        Long size = redisTemplate.opsForSet().size(userKey);
        return size != null ? size : 0;
    }

    public Set<String> getUserRefreshTokens(UUID userId) {
        String userKey = USER_TOKENS_PREFIX + userId;
        return redisTemplate.opsForSet().members(userKey);
    }
}
