using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace supportsapi.labgenomics.com.Services
{
    public class ManageJwtToken
    {
        const string Secret = "5o1tSA7SLy+nFhcZ0/ZJO0ygtrn0YTl8ZD2QeIWScRhZINRsI0oqu1S3E29WE457Eks6009oOopNtw5Y3F8+sA==";
        public static string GenerateToken(string user, string aud)
        {
            byte[] key = Convert.FromBase64String(Secret);
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(key);
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                //발행자
                Issuer = "Labgenomics",
                //제목
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user) }),
                //대상
                Audience = aud,
                //Expires = DateTime.UtcNow.AddMinutes(525600),
                Expires = DateTime.UtcNow.AddMinutes(60 * 24 * 7), //유효기간 7일
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }

        public static ClaimsPrincipal VerifyToken(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                byte[] key = Convert.FromBase64String(Secret);
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken securityToken);
                return principal;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("IDX10223"))
                {
                    Console.Write(ex.Message);
                }
                return null;
            }
        }
    }
}