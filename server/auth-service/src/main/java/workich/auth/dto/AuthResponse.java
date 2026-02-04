package workich.auth.dto;

import java.util.UUID;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import workich.auth.model.UserType;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class AuthResponse {
    @Builder.Default
    private String tokenType = "Bearer";

    private String accessToken;
    private String refreshToken;
    private UUID userId;
    private String email;
    private UserType userType;
}
