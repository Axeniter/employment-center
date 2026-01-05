package workich.auth.service;

import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.data.redis.core.RedisTemplate;
import org.springframework.stereotype.Service;

import java.util.concurrent.TimeUnit;

@Service
@RequiredArgsConstructor
public class TokenBlacklistService {
    
    private final RedisTemplate<String, String> redisTemplate;
    
    @Value("${token.blacklist.ttl}")
    private long blacklistTtl;
    
    private static final String BLACKLIST_KEY_PREFIX = "blacklist:token:";
    
    public void addToBlacklist(String token) {
        String key = BLACKLIST_KEY_PREFIX + token;
        redisTemplate.opsForValue().set(key, "blacklisted", blacklistTtl, TimeUnit.MILLISECONDS);
    }
    
    public boolean isBlacklisted(String token) {
        String key = BLACKLIST_KEY_PREFIX + token;
        return Boolean.TRUE.equals(redisTemplate.hasKey(key));
    }
    
    public void removeFromBlacklist(String token) {
        String key = BLACKLIST_KEY_PREFIX + token;
        redisTemplate.delete(key);
    }
    
    public long getBlacklistSize() {
        String pattern = BLACKLIST_KEY_PREFIX + "*";
        var keys = redisTemplate.keys(pattern);
        return keys != null ? keys.size() : 0;
    }
}