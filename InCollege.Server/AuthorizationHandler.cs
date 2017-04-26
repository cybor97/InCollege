using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SG.Algoritma;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Headers;

namespace InCollege.Server
{
    class AuthorizationHandler : IHttpRequestHandler
    {
        const string EncryptionKey = "Devil's deal. Joke for hebrew. Gruesome fate for devil. Conclusion? Don't make deals with hebrews!";

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            var request = context.Request;
            if (request.Method == HttpMethods.Post)
                if (request.Post.Parsed.TryGetByName("Action", out string action))
                    context.Response = Actions[action]?.Invoke(request.Post.Parsed);
                else context.Response = new HttpResponse(HttpResponseCode.MethodNotAllowed, "Ошибка! Неверный запрос.", true);
            return Task.Factory.GetCompleted();
        }

        static Dictionary<string, Func<IHttpHeaders, IHttpResponse>> Actions = new Dictionary<string, Func<IHttpHeaders, IHttpResponse>>()
        {
            { "SignIn", SignIn },
            { "SignUp", SignUp },
            { "ValidateToken", ValidateToken },
            { "WhoAmI", WhoAmI }
        };

        private static IHttpResponse WhoAmI(IHttpHeaders arg)
        {
            if (arg.TryGetByName("token", out string tokenString))
            {
                //Nope! I won't send password with account info!
                var token = VerifyTokenString(tokenString, true);
                if (token.valid)
                    return new HttpResponse(HttpResponseCode.Ok, token.accountJSON, false);
                else return new HttpResponse(HttpResponseCode.Forbidden, "Токен невалидный. Проверьте правильность или запросите новый.", false);
            }
            else return new HttpResponse(HttpResponseCode.Forbidden, "Не удалось получить данные об аккаунте. Нужен токен!", false);
        }

        private static IHttpResponse SignIn(IHttpHeaders query)
        {
            if (query.TryGetByName("UserName", out string userName) &&
                query.TryGetByName("Password", out string password))
            {
                var rows = DBHolderSQL.GetRange("Account", null, 0, 1, true,
                    ("UserName", userName),
                    ("Password", password)).Rows;

                if (rows.Count == 1)
                    return new HttpResponse(HttpResponseCode.Ok, CreateToken(int.Parse(rows[0]["ID"].ToString()), userName, password), false);
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
                query.TryGetByName("AccountType", out byte accountType) &&
                query.TryGetByName("BirthDate", out DateTime birthDate) &&
                query.TryGetByName("FullName", out string fullName))
            {
                var validationResult = Account.Validate(userName, password, birthDate, fullName);
                if (validationResult == AccountValidationResult.OK)
                {
                    var rows = DBHolderSQL.GetRange("Account", null, 0, 1, true, ("UserName", userName)).Rows;
                    if (rows.Count == 0)
                    {
                        query.TryGetByName("ProfileImage", out byte[] profileImage);
                        return new HttpResponse(HttpResponseCode.Ok, CreateToken(DBHolderSQL.Save("Account",
                            ("UserName", userName),
                            ("Password", password),
                            ("AccountType", accountType),
                            ("BirthDate", birthDate),
                            ("ProfileImage", profileImage),
                            ("FullName", fullName),
                            ("AccountDataID", -1),
                            ("Approved", false),
                            ("IsLocal", true),
                            ("ID", -1)
                            ),
                            userName, password), false);
                    }
                    else return new HttpResponse(HttpResponseCode.BadRequest, "Ошибка! Регистрация невозможна, т.к. пользователь с этим именем пользователя уже зарегистирован в системе!", false);
                }
                else return new HttpResponse(HttpResponseCode.BadRequest, ErrorMessages[validationResult], false);
            }
            return null;
        }

        static Dictionary<AccountValidationResult, string> ErrorMessages = new Dictionary<AccountValidationResult, string>()
        {
            { AccountValidationResult.FullNameEmpty, "Ваше имя не может быть пустым." },
            { AccountValidationResult.FullNameIncorrectFormat, "ФИО не соответствует формату Имя Фамилия Отчество. Проверьте правильность введенных данных." },
            { AccountValidationResult.UserNameEmpty, "Имя пользователя не может быть пустым." },
            { AccountValidationResult.UserNameTooShort,"Имя пользователя должно содержать не менее 5 символов." },
            { AccountValidationResult.PasswordEmpty, "Пароль не может быть пустым." },
            { AccountValidationResult.PasswordTooShort,  "Пароль должен содержать не менее 5 символов." },
            { AccountValidationResult.BirthDateUndefined, "Необходимо указать дату рождения." },
            { AccountValidationResult.BirthDateAfterNow, "Дата рождения не может быть больше текущей." },
            { AccountValidationResult.AgeTooBig,"ПО разработано для особей вида Homo Sapiens, т.е. людей.<br>На момент разработки ПО и в перспективе люди столько не живут."  },
        };

        private static IHttpResponse ValidateToken(IHttpHeaders query)
        {
            if (query.TryGetByName("token", out string token))
                return new HttpResponse(VerifyToken(token, false, true).valid ? HttpResponseCode.Ok : HttpResponseCode.NotAcceptable, string.Empty, false);
            else return new HttpResponse(HttpResponseCode.BadRequest, string.Empty, false);
        }


        //TODO:Implement device-specific "secret" to ensure token isn't stolen
        static string CreateToken(int id, string userName, string password)
        {
            var handler = new JwtSecurityTokenHandler();

            return Cipher.Encrypt(handler.WriteToken(handler
                        .CreateJwtSecurityToken(
                        issuer: "InCollege_Auth",
                        audience: "InCollege_Auth",
                        subject: new ClaimsIdentity(new Claim[]
                        {
                            new Claim("ID",id.ToString(),typeof(int).Name,"InCollege_Auth","InCollege_Auth"),
                            new Claim("UserName",userName,typeof(string).Name,"InCollege_Auth","InCollege_Auth"),
                            new Claim("Password",password,typeof(string).Name,"InCollege_Auth","InCollege_Auth"),
                        }),
                        notBefore: DateTime.Now,
                        expires: DateTime.Now.AddMonths(1),
                        signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
                            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(EncryptionKey)),
                            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256))), EncryptionKey);
        }

        public static (bool valid, Account account) VerifyToken(string tokenString, bool wipePassword)
        {
            var token = VerifyToken(tokenString, true, wipePassword);
            return (token.valid, (Account)token.account);
        }

        public static (bool valid, string accountJSON) VerifyTokenString(string tokenString, bool wipePassword)
        {
            var token = VerifyToken(tokenString, false, wipePassword);
            return (token.valid, (string)token.account);
        }

        public static (bool valid, object account) VerifyToken(string tokenString, bool deSerializeAccount, bool wipePassword)
        {
            var data = GetToken(tokenString);

            if (data.id == -1 || string.IsNullOrWhiteSpace(data.userName) || string.IsNullOrWhiteSpace(data.password))
                return (false, null);

            DataTable table = DBHolderSQL.GetRange("Account", null, 0, 1, true,
                ("ID", data.id),
                ("UserName", data.userName),
                ("Password", data.password));

            if (table.Rows.Count == 1)
            {
                if (wipePassword)
                    table.Rows[0]["Password"] = null;
                var accountString = JsonConvert.SerializeObject(table, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                return (true, deSerializeAccount ? (object)JsonConvert.DeserializeObject<IList<Account>>(accountString)[0] : accountString);
            }
            else if (table.Rows.Count > 1)
                return (false, null);
            else
                return (false, null);
        }

        //TODO:Implement "expires" checking.
        public static (int id, string userName, string password) GetToken(string tokenString)
        {
            try
            {
                JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(Cipher.Decrypt(tokenString.Replace(' ', '+'), EncryptionKey));

                int id = -1;
                string userName = null, password = null;

                foreach (var current in token.Claims)
                    if (current.Type.Equals("ID"))
                        int.TryParse(current.Value, out id);
                    else if (current.Type.Equals("UserName"))
                        userName = current.Value;
                    else if (current.Type.Equals("Password"))
                        password = current.Value;

                return (id, userName, password);
            }
            catch (FormatException)
            {
                return (-1, null, null);
            }
        }
    }
}