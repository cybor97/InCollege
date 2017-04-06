using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Headers;

namespace InCollege.Server
{
    class AuthorizationHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            var request = context.Request;
            //FIXME:Change to POST
            if (request.Method == HttpMethods.Get)
                if (request.QueryString.TryGetByName("action", out string action))
                    context.Response = Actions[action]?.Invoke(request.QueryString);
                else context.Response = new HttpResponse(HttpResponseCode.MethodNotAllowed, "Ошибка! Неверный запрос.", true);
            return Task.Factory.GetCompleted();
        }

        static Dictionary<string, Func<IHttpHeaders, IHttpResponse>> Actions = new Dictionary<string, Func<IHttpHeaders, IHttpResponse>>()
        {
            { "SignIn", SignIn },
            { "SignUp", SignUp }
        };

        private static IHttpResponse SignIn(IHttpHeaders query)
        {
            if (query.TryGetByName("UserName", out string userName) &&
                query.TryGetByName("Password", out string password))
            {
                var rows = DBHolderSQL.GetRange("Account", null, 0, 1, true,
                    new Tuple<string, object>("UserName", userName),
                    new Tuple<string, object>("Password", password)).Rows;

                if (rows.Count == 1)
                {
                    return new HttpResponse(HttpResponseCode.Ok, new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                        issuer: "InCollege_Auth",
                        audience: "InCollege_Auth",
                        claims: new List<Claim>
                        {
                            new Claim("ID",rows[0]["ID"].ToString()),
                            new Claim("UserName",userName),
                            new Claim("Password",password),
                        },
                        notBefore: DateTime.Now.AddDays(1),
                        expires: DateTime.Now.AddMonths(1),
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Devil's deal. Joke for hebrew. Gruesome fate for devil.")),
                            SecurityAlgorithms.HmacSha256))), false);
                }
                else if (rows.Count > 1)
                    return new HttpResponse(HttpResponseCode.InternalServerError, "Ошибка! Найдено более 1 аккаунта. Обратитесь к администратору.", false);
                else
                    return new HttpResponse(HttpResponseCode.Forbidden, "Ошибка! Пользователь с таким именем пользователя и паролем не найден.", false);
            }
            else return new HttpResponse(HttpResponseCode.Forbidden, "Укажите 'UserName' и 'Password'!", false);
        }

        private static IHttpResponse SignUp(IHttpHeaders query)
        {
            if (query.TryGetByName("UserName", out string userName) &&
                query.TryGetByName("Password", out string password) &&
                query.TryGetByName("AccountType", out AccountType accountType) &&
                query.TryGetByName("BirthDate", out DateTime birthDate) &&
                query.TryGetByName("ProfileImage", out string profileImage) &&
                query.TryGetByName("FullName", out string fullName))
            {
                switch (Account.Validate(userName, password, birthDate, fullName))
                {
                    case AccountValidationResult.OK:
                        break;
                    case AccountValidationResult.UserNameEmpty:
                        return new HttpResponse(HttpResponseCode.BadRequest, "Имя пользователя не может быть пустым.", false);
                    case AccountValidationResult.UserNameTooShort:
                        return new HttpResponse(HttpResponseCode.BadRequest, "Имя пользователя должно содержать не менее 5 символов.", false);
                    case AccountValidationResult.PasswordEmpty:
                        return new HttpResponse(HttpResponseCode.BadRequest, "Пароль не может быть пустым.", false);
                    case AccountValidationResult.PasswordTooShort:
                        return new HttpResponse(HttpResponseCode.BadRequest, "Пароль должен содержать не менее 5 символов.", false);
                    case AccountValidationResult.BirthDateUndefined:
                        return new HttpResponse(HttpResponseCode.BadRequest, "Необходимо указать дату рождения.", false);
                    case AccountValidationResult.BirthDateAfterNow:
                        return new HttpResponse(HttpResponseCode.BadRequest, "Дата рождения не может быть больше текущей.", false);
                    case AccountValidationResult.AgeTooBig:
                        return new HttpResponse(HttpResponseCode.BadRequest, "ПО разработано для особей вида Homo Sapiens, т.е. людей.<br>" +
                                                                             "На момент разработки ПО и в перспективе люди столько не живут.", false);
                }
            }
            return null;
        }

        public enum JwtHashAlgorithm
        {
            RS256,
            HS384,
            HS512
        }

        public class JsonWebToken
        {
            private static Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>> HashAlgorithms;

            static JsonWebToken()
            {
                HashAlgorithms = new Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>>
            {
                { JwtHashAlgorithm.RS256, (key, value) => { using (var sha = new HMACSHA256(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS384, (key, value) => { using (var sha = new HMACSHA384(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS512, (key, value) => { using (var sha = new HMACSHA512(key)) { return sha.ComputeHash(value); } } }
            };
            }

            public static string Encode(object payload, string key, JwtHashAlgorithm algorithm)
            {
                return Encode(payload, Encoding.UTF8.GetBytes(key), algorithm);
            }

            public static string Encode(object payload, byte[] keyBytes, JwtHashAlgorithm algorithm)
            {
                var segments = new List<string>();
                var header = new { alg = algorithm.ToString(), typ = "JWT" };

                byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header, Formatting.None));
                byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, Formatting.None));
                //byte[] payloadBytes = Encoding.UTF8.GetBytes(@"{"iss":"761326798069-r5mljlln1rd4lrbhg75efgigp36m78j5@developer.gserviceaccount.com","scope":"https://www.googleapis.com/auth/prediction","aud":"https://accounts.google.com/o/oauth2/token","exp":1328554385,"iat":1328550785}");

                segments.Add(Base64UrlEncode(headerBytes));
                segments.Add(Base64UrlEncode(payloadBytes));

                var stringToSign = string.Join(".", segments.ToArray());

                var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

                byte[] signature = HashAlgorithms[algorithm](keyBytes, bytesToSign);
                segments.Add(Base64UrlEncode(signature));

                return string.Join(".", segments.ToArray());
            }

            public static string Decode(string token, string key)
            {
                return Decode(token, key, true);
            }

            public static string Decode(string token, string key, bool verify)
            {
                var parts = token.Split('.');
                var header = parts[0];
                var payload = parts[1];
                byte[] crypto = Base64UrlDecode(parts[2]);

                var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
                var headerData = JObject.Parse(headerJson);
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
                var payloadData = JObject.Parse(payloadJson);

                if (verify)
                {
                    var bytesToSign = Encoding.UTF8.GetBytes(string.Concat(header, ".", payload));
                    var keyBytes = Encoding.UTF8.GetBytes(key);
                    var algorithm = (string)headerData["alg"];

                    var signature = HashAlgorithms[GetHashAlgorithm(algorithm)](keyBytes, bytesToSign);
                    var decodedCrypto = Convert.ToBase64String(crypto);
                    var decodedSignature = Convert.ToBase64String(signature);

                    if (decodedCrypto != decodedSignature)
                    {
                        throw new ApplicationException(string.Format("Invalid signature. Expected {0} got {1}", decodedCrypto, decodedSignature));
                    }
                }

                return payloadData.ToString();
            }

            private static JwtHashAlgorithm GetHashAlgorithm(string algorithm)
            {
                switch (algorithm)
                {
                    case "RS256": return JwtHashAlgorithm.RS256;
                    case "HS384": return JwtHashAlgorithm.HS384;
                    case "HS512": return JwtHashAlgorithm.HS512;
                    default: throw new InvalidOperationException("Algorithm not supported.");
                }
            }

            // from JWT spec
            private static string Base64UrlEncode(byte[] input)
            {
                var output = Convert.ToBase64String(input);
                output = output.Split('=')[0]; // Remove any trailing '='s
                output = output.Replace('+', '-'); // 62nd char of encoding
                output = output.Replace('/', '_'); // 63rd char of encoding
                return output;
            }

            // from JWT spec
            private static byte[] Base64UrlDecode(string input)
            {
                var output = input;
                output = output.Replace('-', '+'); // 62nd char of encoding
                output = output.Replace('_', '/'); // 63rd char of encoding
                switch (output.Length % 4) // Pad with trailing '='s
                {
                    case 0: break; // No pad chars in this case
                    case 2: output += "=="; break; // Two pad chars
                    case 3: output += "="; break; // One pad char
                    default: throw new System.Exception("Illegal base64url string!");
                }
                var converted = Convert.FromBase64String(output); // Standard base64 decoder
                return converted;
            }
        }
    }
}
