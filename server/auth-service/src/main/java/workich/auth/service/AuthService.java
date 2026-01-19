package workich.auth.service;

import lombok.RequiredArgsConstructor;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import workich.auth.dto.AuthResponse;
import workich.auth.dto.LoginRequest;
import workich.auth.dto.RegisterRequest;
import workich.auth.model.User;
import workich.auth.repository.UserRepository;

import java.util.UUID;

@Service
@RequiredArgsConstructor
public class AuthService {
    
    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;
    private final AuthenticationManager authenticationManager;
    private final JwtService jwtService;
    private final RefreshTokenService refreshTokenService;
    
    @Transactional
    public AuthResponse register(RegisterRequest request) {
        if (userRepository.existsByEmail(request.getEmail())) {
            throw new RuntimeException("User with email " + request.getEmail() + " already exists");
        }
        
        User user = User.builder()
                .email(request.getEmail())
                .password(passwordEncoder.encode(request.getPassword()))
                .userType(request.getUserType())
                .enabled(true)
                .locked(false)
                .build();
        
        user = userRepository.save(user);
        
        return generateAuthResponse(user);
    }
    
    @Transactional
    public AuthResponse login(LoginRequest request) {
        Authentication authentication = authenticationManager.authenticate(
                new UsernamePasswordAuthenticationToken(
                        request.getEmail(),
                        request.getPassword()
                )
        );
        
        SecurityContextHolder.getContext().setAuthentication(authentication);
        
        User user = (User) authentication.getPrincipal();
        
        return generateAuthResponse(user);
    }
    
    @Transactional
    public AuthResponse refreshToken(String refreshToken) {
        UUID userId = refreshTokenService.getUserIdByToken(refreshToken);
        if (userId == null) {
            throw new RuntimeException("Invalid refresh token");
        }
        
        User user = userRepository.findById(userId)
                .orElseThrow(() -> new RuntimeException("User not found"));
        
        refreshTokenService.deleteRefreshToken(refreshToken);
        
        return generateAuthResponse(user);
    }
    
    @Transactional
    public void logout(String accessToken, String refreshToken, UUID userId) {
        jwtService.invalidateToken(accessToken);
        refreshTokenService.deleteRefreshToken(refreshToken);
        SecurityContextHolder.clearContext();
        
    }
    
    @Transactional
    public void logoutAll(UUID userId) {
        refreshTokenService.deleteAllUserTokens(userId);
    }
    
    public boolean validateToken(String token) {
            UUID userId = jwtService.extractId(token);
            User user = userRepository.findById(userId)
                    .orElseThrow(() -> new RuntimeException("User not found"));
            
            return jwtService.isTokenValid(token, user);
    }
    

    public long getUserActiveSessions(UUID userId) {
        return refreshTokenService.getUserActiveSessions(userId);
    }
    
    private AuthResponse generateAuthResponse(User user) {
        String accessToken = jwtService.generateAccessToken(user);
        String refreshToken = refreshTokenService.generateRefreshToken();
        refreshTokenService.saveRefreshToken(user.getId(), refreshToken);
        return buildAuthResponse(user, accessToken, refreshToken);
    }
    
    private AuthResponse buildAuthResponse(User user, String accessToken, String refreshToken) {
        return AuthResponse.builder()
                .accessToken(accessToken)
                .refreshToken(refreshToken)
                .userId(user.getId())
                .email(user.getEmail())
                .userType(user.getUserType())
                .build();
    }
}