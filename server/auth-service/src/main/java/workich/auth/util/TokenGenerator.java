package workich.auth.util;

import lombok.experimental.UtilityClass;
import java.security.SecureRandom;
import java.util.Base64;

@UtilityClass
public class TokenGenerator {
    
    private static final SecureRandom SECURE_RANDOM = new SecureRandom();
    private static final Base64.Encoder BASE64_ENCODER = Base64.getUrlEncoder().withoutPadding();
    
    public static String generateRefreshToken(int length) {
        byte[] randomBytes = new byte[length];
        SECURE_RANDOM.nextBytes(randomBytes);
        return BASE64_ENCODER.encodeToString(randomBytes);
    }
}