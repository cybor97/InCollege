using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SG.Algoritma;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
                else context.Response = new HttpResponse(HttpResponseCode.MethodNotAllowed, "Ошибка! Неверный запрос.", false);
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
                    return new HttpResponse(HttpResponseCode.Ok, token.accountJSON, true);
                else return new HttpResponse(HttpResponseCode.Forbidden, "Токен невалидный. Проверьте правильность или запросите новый.", false);
            }
            else return new HttpResponse(HttpResponseCode.Forbidden, "Не удалось получить данные об аккаунте. Нужен токен!", false);
        }

        private static IHttpResponse SignIn(IHttpHeaders query)
        {
            if (query.TryGetByName("UserName", out string userName) &&
                query.TryGetByName("Password", out string password))
            {
                var rows = DBHolderSQL.GetRange("Account", null, 0, 1, true, false, false, false,
                    ("UserName", userName),
                    ("Password", password)).Rows;

                if (rows.Count == 1)
                    return new HttpResponse(HttpResponseCode.Ok, CreateToken(int.Parse(rows[0]["ID"].ToString()), userName, password), true);
                else if (rows.Count > 1)
                {
                    DBHolderSQL.Log($"[КОНФЛИКТ] Конфликт аккаунтов {userName}.",
                        $"Попытка входа при наличии более одного аккаунта с одинаковым именем пользователя ({userName}).\n" +
                        $"Измените имя пользователя для одного из аккаунтов.");
                    return new HttpResponse(HttpResponseCode.InternalServerError, "Ошибка! Найдено более 1 аккаунта. Обратитесь к администратору.", false);
                }
                else
                {
                    DBHolderSQL.Log($"[НЕВЕРНЫЙ ВВОД] Ошибка авторизации пользователя {userName}.",
                                    $"Пользователь ввел неверные данные. Осторожно! Это может означать попытку взлома \"Грубой силой\"(BruteForce)");
                    return new HttpResponse(HttpResponseCode.Forbidden, "Ошибка! Пользователь с таким именем пользователя и паролем не найден.", false);
                }
            }
            else return new HttpResponse(HttpResponseCode.Forbidden, "Укажите 'UserName' и 'Password'!", false);
        }

        private static IHttpResponse SignUp(IHttpHeaders query)
        {
            if (query.TryGetByName("UserName", out string userName) &&
                query.TryGetByName("Password", out string password) &&
                query.TryGetByName("AccountType", out byte accountType) &&
                query.TryGetByName("BirthDate", out string birthDateString) &&
                DateTime.TryParseExact(birthDateString, Core.CommonVariables.DateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthDate) &&

                query.TryGetByName("FullName", out string fullName))
            {

                var validationResult = Account.Validate(userName, password, birthDate, fullName);
                if (validationResult == AccountValidationResult.OK)
                {
                    var rows = DBHolderSQL.GetRange("Account", null, 0, 1, true, false, false, false, ("UserName", userName)).Rows;
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
                            ("Approved", false),
                            ("IsLocal", true),
                            ("ID", -1)),
                            userName, password), true);
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
        static string CreateToken(long id, string userName, string password)
        {
            var handler = new JwtSecurityTokenHandler();

            return Cipher.Encrypt(handler.WriteToken(handler
                        .CreateJwtSecurityToken(
                        issuer: "InCollege_Auth",
                        audience: "InCollege_Auth",
                        subject: new ClaimsIdentity(new Claim[]
                        {
                            new Claim("ID",id.ToString(),typeof(int).Name,"InCollege_Auth","InCollege_Auth"),
                            new Claim("UserName",userName??"_default_",typeof(string).Name,"InCollege_Auth","InCollege_Auth"),
                            new Claim("Password",password??"_default_",typeof(string).Name,"InCollege_Auth","InCollege_Auth"),
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

            if (data.id == -1 || string.IsNullOrWhiteSpace(data.userName))
                return (false, null);

            DataTable table = DBHolderSQL.GetRange("Account", null, 0, 1, true, false, false, false,
                ("ID", data.id),
                ("UserName", data.userName == "_default_" ? string.Empty : data.userName),
                ("Password", data.password == "_default_" ? string.Empty : data.password));

            if (table.Rows.Count == 1)
            {
                if (wipePassword)
                    table.Rows[0]["Password"] = null;
                return (true, deSerializeAccount ?
                    (object)new Account
                    {
                        ID = (int)(long)table.Rows[0]["ID"],
                        AccountType = (AccountType)(int)(long)table.Rows[0]["AccountType"],
                        Approved = ((long)table.Rows[0]["Approved"] == 1),
                        LastAction = table.Rows[0]["LastAction"] == DBNull.Value ? null : (DateTime?)table.Rows[0]["LastAction"],
                        Password = table.Rows[0]["Password"] == DBNull.Value ? null : (string)table.Rows[0]["Password"],
                    } :
                    JsonConvert.SerializeObject(table, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            }
            else if (table.Rows.Count > 1)
                return (false, null);
            else
                return (false, null);
        }

        static JwtSecurityTokenHandler TokenHandler = new JwtSecurityTokenHandler();
        static Dictionary<string, JwtSecurityToken> TokensCache = new Dictionary<string, JwtSecurityToken>();
        const int TOKENS_CACHE_SIZE_LIMIT = 100;

        //TODO:Implement "expires" checking.
        public static (int id, string userName, string password) GetToken(string tokenString)
        {
            try
            {
                JwtSecurityToken token;
                if (TokensCache.ContainsKey(tokenString))
                    token = TokensCache[tokenString];
                else
                {
                    token = TokenHandler.ReadJwtToken(Cipher.Decrypt(tokenString.Replace(' ', '+'), EncryptionKey));
                    if (TokensCache.Count >= TOKENS_CACHE_SIZE_LIMIT)
                        foreach (var current in TokensCache.Keys)
                        {
                            TokensCache.Remove(current);
                            break;
                        }
                    TokensCache.Add(tokenString, token);
                }

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