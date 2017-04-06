using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Headers;
using static CyborTools.StringEncryptor;

namespace InCollege.Server
{
    class AuthorizationHandler : IHttpRequestHandler
    {
        const string EncryptionKey = "Devil's deal. Joke for hebrew. Gruesome fate for devil.";

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
                    return new HttpResponse(HttpResponseCode.Ok, CreateToken((int)rows[0]["ID"], userName, password), false);
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
                        return new HttpResponse(HttpResponseCode.Ok, CreateToken(DBHolderSQL.Save("Account",
                            ("UserName", userName),
                            ("Password", password),
                            ("AccountType", accountType),
                            ("BirthDate", birthDate),
                            ("ProfileImage", profileImage),
                            ("FullName", fullName)),
                            userName, password), false);
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
        static string CreateToken(int id, string userName, string password)
        {
            var handler = new JwtSecurityTokenHandler();

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(PackData(Encrypt(handler.WriteToken(handler
                        .CreateJwtSecurityToken(
                        issuer: "InCollege_Auth",
                        audience: "InCollege_Auth",
                        subject: new ClaimsIdentity(new Claim[]
                        {
                            new Claim("ID",id.ToString(),typeof(int).Name,"InCollege_Auth","InCollege_Auth"),
                            new Claim("UserName",userName,typeof(string).Name,"InCollege_Auth","InCollege_Auth"),
                            new Claim("Password",password,typeof(string).Name,"InCollege_Auth","InCollege_Auth"),
                        }),
                        notBefore: DateTime.Now.AddDays(1),
                        expires: DateTime.Now.AddMonths(1),
                        signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
                            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(EncryptionKey)),
                            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256))), GetKey(EncryptionKey)))));
        }

        public static (int id, string userName, string password) GetToken(string tokenString)
        {
            JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(
                Decrypt(
                    GetData(
                        Encoding.UTF8.GetString(Convert.FromBase64String(tokenString))), GetKey(EncryptionKey)));

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
    }
}
