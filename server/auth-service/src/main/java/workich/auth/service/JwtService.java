package workich.auth.service;

import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.function.Function;

import org.springframework.security.core.userdetails.UserDetails;
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

    public String generateAccessToken(UserDetails userDetails) {
        Map<String, Object> claims = new HashMap<>();
        if (userDetails instanceof User user) {
            claims.put("id", user.getId());
            claims.put("type", user.getUserType());
        }
        return Jwts
        .builder()
        .claims(claims)
        .subject(userDetails.getUsername())
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

    public String extractUsername(String token) {
        return extractClaim(token, Claims::getSubject);
    }

    public Date extractExpiration(String token) {
        return extractClaim(token, Claims::getExpiration);
    }

    public boolean isTokenValid(String token, UserDetails userDetails) {
        final String username = extractUsername(token);

        if (tokenBlacklistService.isBlacklisted(token)) {
            return false;
        }
        return (username.equals(userDetails.getUsername())) && !isTokenExpired(token);
    }

    private boolean isTokenExpired(String token) {
        return extractExpiration(token).before(new Date());
    }

    public void invalidateToken(String token) {
        tokenBlacklistService.addToBlacklist(token);
    }
}