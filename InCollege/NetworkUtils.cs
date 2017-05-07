using InCollege.Client;
using InCollege.Client.UI;
using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using InCollege.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace InCollege
{
    public enum DataAction
    {
        Save,
        Remove
    }

    public class NetworkUtils
    {
        public static async Task<Account> WhoAmI()
        {
            HttpResponseMessage response;
            if ((response = (await new HttpClient()
            .PostAsync(ClientConfiguration.AuthHandlerPath,
            new StringContent(
            $"Action=WhoAmI&" +
            $"token={App.Token}")))).StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<IList<Account>>(await response.Content.ReadAsStringAsync())[0];
            else
                MessageBox.Show(await response.Content.ReadAsStringAsync());
            return null;
        }

        public static async Task<List<T>> RequestData<T>(Window context, params (string name, object value)[] whereParams) where T : DBRecord
        {
            try
            {
                HttpResponseMessage response;

                var whereString = string.Join("&", whereParams.Select(c => $"where{c.name}=" + WebUtility.UrlEncode(((c.value is byte[]) ? Convert.ToBase64String((byte[])c.value) :
                                                        (c.value is bool) ? ((bool)c.value ? 1 : 0) :
                                                        (c.value is Enum) ? (byte)c.value :
                                                        (c.value is DateTime) ? ((DateTime)c.value).ToString("yyyy-MM-dd") :
                                                        c.value).ToString())));

                if ((response = (await new HttpClient()
                      .PostAsync(ClientConfiguration.DataHandlerPath,
                      new StringContent(
                      $"Action=GetRange&" +
                      $"table={typeof(T).Name}&" +
                      $"token={App.Token}" +
                      //Not an error! Little more attension, & is  ' here
                      (!string.IsNullOrWhiteSpace(whereString) ? $"&{whereString}" : ""))))).StatusCode == HttpStatusCode.OK)
                    return JsonConvert
                        .DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync());
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                    if (response.StatusCode == HttpStatusCode.Forbidden) context?.Close();
                }
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return null;
        }

        public static async Task<string> ExecuteDataAction<T>(IUpdatable context, DBRecord dataObject, DataAction action) where T : DBRecord
        {
            return await ExecuteDataAction(typeof(T).Name, context, dataObject, action);
        }

        public static async Task<string> ExecuteDataAction(string tableName, IUpdatable context, DBRecord dataObject, DataAction action)
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await new HttpClient()
                      .PostAsync(ClientConfiguration.DataHandlerPath,
                      new StringContent(
                      $"Action={Enum.GetName(typeof(DataAction), action)}&" +
                      $"table={tableName}&" +
                      $"token={App.Token}&" +
                      (action == DataAction.Save ? dataObject.POSTSerialized : $"id={dataObject.ID}"))))).StatusCode == HttpStatusCode.OK)
                {
                    if (context != null)
                        await context.UpdateData();
                }
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return "-1";
        }

    }
}
