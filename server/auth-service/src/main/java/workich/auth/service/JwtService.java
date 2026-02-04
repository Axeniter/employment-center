package workich.auth.service;

import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;
import java.util.function.Function;

import org.springframework.stereotype.Service;

import io.jsonwebtoken.Claims;
import io.jsonwebtoken.Jwts;
import lombok.RequiredArgsConstructor;
import workich.auth.config.JwtConfig;
import workich.auth.model.User;

@Service
@RequiredArgsConstructor
public class JwtService {
    private final JwtConfig jwtConfig;
    private final TokenBlacklistService tokenBlacklistService;

    public String generateAccessToken(User user) {
        Map<String, Object> claims = new HashMap<>();
        claims.put("email", user.getEmail());
        claims.put("type", user.getUserType());
        return Jwts
            .builder()
            .claims(claims)
            .subject(user.getId().toString())
            .issuedAt(new Date(System.currentTimeMillis()))
            .expiration(new Date(System.currentTimeMillis() + jwtConfig.getAccessExpiration()))
            .signWith(jwtConfig.getSecretKey(), Jwts.SIG.HS256)
            .compact();
    }

    private Claims extractAllClaims(String token) {
        return Jwts.parser().verifyWith(jwtConfig.getSecretKey()).build().parseSignedClaims(token)
                .getPayload();
    }

    public <T> T extractClaim(String token, Function<Claims, T> claimsResolver) {
        final Claims claims = extractAllClaims(token);
        return claimsResolver.apply(claims);
    }

    public UUID extractId(String token) {
        String subject = extractClaim(token, Claims::getSubject);
        return UUID.fromString(subject);
    }

    public Date extractExpiration(String token) {
        return extractClaim(token, Claims::getExpiration);
    }

    public boolean isTokenValid(String token, User user) {
        UUID id = extractId(token);

        if (tokenBlacklistService.isBlacklisted(token)) {
            return false;
        }
        return (id.equals(user.getId())) && !isTokenExpired(token);
    }

    private boolean isTokenExpired(String token) {
        return extractExpiration(token).before(new Date());
    }

    public void invalidateToken(String token) {
        tokenBlacklistService.addToBlacklist(token);
    }
}