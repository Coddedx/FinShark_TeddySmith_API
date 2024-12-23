using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using Microsoft.IdentityModel.Tokens;

namespace api.Service
{
    public class TokenService : ITokenService
    {
        //We will write service to create token outside from identity. we are going to jenerate the jwt on the server and we are going to stuff with the claim (almost like roles but more flexible. key value pairs that what the user does). 
        //we can also send it in the form of jwt so user can have all of these claims into jwt and each time they send a request all of this data is going to be within our jwt. As soon jwt is going to blown away. 
        //you be able acsess through the Http context.
       
        // To summit -> user is going to log in with their email and its going to send their jwt to the server when their authenticated with claims principle and within this claims principle 
        //we wil get acess all of these values (timezone, user name,is paying user? ... (we can specifie it)). And we can use this Http user context all through the app as long is the user loged in  
        
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key; //use to encrypt it an unique way to only specific to our server so that people can not mess with the token
        public TokenService(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"])); //with our key anybody can create token(jwt is hinged on signing key) (the is inside of appsettings.json and it is random string that we created)
        }
        public string CreateToken(AppUser user)
        {
            //these are that u can use to identify the users and express what the user can or cant do (you can put different things other than email,username...)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName)
            };   

            var creds = new SigningCredentials(_key , SecurityAlgorithms.HmacSha512Signature);

            //this is where we actuallly CREATE THE TOKEN
            //we are gona create as object and .net take care of the rest
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"], //we specified this in the appsettings.json
                Audience = _config["JWT:Audience"]
            };
            //tokenHandler the method of creating the actual token (that we created with the token object)
            var tokenHandler = new JwtSecurityTokenHandler();
            //var token = tokenHandler.CreateToken(tokenDescriptor); //teddy böyle yapıyor ama bende kid eklememiz gerekiyor o yüzden alttaki kodu kullan
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor); 


            //kid eklemediğimde terminalde Authentication failed: IDX10503: Signature validation failed. Token does not have a kid. hatası alıyorum çözmek için:
            //kid (Key Identifier) eksikliği genellikle RSA veya diğer asimetrik anahtarlarla ilgili bir sorundur. 
            //Eğer sadece SymmetricSecurityKey kullanıyorsanız, kid gerekmemelidir. Ancak kid talep ediliyorsa, JwtHeader'a manuel olarak ekleyebilirsiniz.
            //SymmetricSecurityKey kullandığınızda doğrulama, doğrudan imzanın ve anahtarın eşleşmesiyle yapılır. Ancak,kid'i yalnızca JWT doğrulama tarafında birden fazla anahtar kullanıyorsanız 
            //(örneğin, bir Key Rotation sistemi (Örneğin, bir public/private key çifti kullanıyor ve anahtarları düzenli olarak değiştiriyorsanız) ) eklemeniz gereklidir. 
            token.Header.Add("kid", "1234568751"); //key i dinamik olarak da değiştirebiliriz. (Bu durumda, appsettings.json veya bir vb gibi bir yerde anahtarlarınızı ve kid bilgilerinizi sakla)

            //we dont want to return the token as an object we retern it form of string ( with this method tokenHandler.WriteToken(token))
            return tokenHandler.WriteToken(token);
        }
    }
}