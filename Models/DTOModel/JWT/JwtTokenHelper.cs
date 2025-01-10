using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Models.DBModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Models.DTOModel.Users;
using Utility;

namespace Models.DTOModel.JWT
{
    public class JwtTokenHelper
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
  
        public JwtTokenHelper(IConfiguration configuration)
        {
            _key = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }
        public string GenerateToken(User user)
        {
            var issuer = SharedUtilityConnection.AppConfiguration["JWT:ValidIssuer"];
            var sceretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SharedUtilityConnection.AppConfiguration["JWT:Secret"]));
            var signingCredentials = new SigningCredentials(sceretKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                           issuer: issuer,
                           audience: issuer,
                           claims: MapClaims(user),
                           expires: DateTime.Now.AddDays(1),
                           signingCredentials: signingCredentials);

            var jwttoken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwttoken;
        }
        List<Claim> MapClaims(User userDto)
        {
            List<Claim> claim = new List<Claim>();
            claim.Add(new Claim("Id", userDto.Id.ToString()));
            claim.Add(new Claim("FirstName", userDto.FirstName));
            claim.Add(new Claim("Email", userDto.Email));
            claim.Add(new Claim("LastName", userDto.LastName));
            return claim;
        }
    }

}
