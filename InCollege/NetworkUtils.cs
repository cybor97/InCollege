using InCollege.Client;
using InCollege.Client.UI;
using InCollege.Core.Data;
using InCollege.Core.Data.Base;
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
        static HttpClient Client;
        static NetworkUtils()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            Client.DefaultRequestHeaders.Add("Keep-Alive", "timeout=600");
        }

        public static async Task<Account> WhoAmI()
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await Client
                .PostAsync(ClientConfiguration.Instance.AuthHandlerPath,
                new StringContent(
                $"Action=WhoAmI&" +
                $"token={App.Token}")))).StatusCode == HttpStatusCode.OK)
                    return JsonConvert.DeserializeObject<IList<Account>>(await response.Content.ReadAsStringAsync())[0];
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return null;

        }

        public static async Task<int> GetCount<T>(Window context, params (string name, object value)[] whereParams) where T : DBRecord
        {
            return await GetCount<T>(context, false, whereParams);
        }

        public static async Task<int> GetCount<T>(Window context, bool preserveContext = false, params (string name, object value)[] whereParams) where T : DBRecord
        {
            try
            {
                HttpResponseMessage response;

                var whereString = string.Join("&", whereParams.Select(c => $"where{c.name}=" + WebUtility.UrlEncode(((c.value is byte[]) ? Convert.ToBase64String((byte[])c.value) :
                                                        (c.value is bool) ? ((bool)c.value ? 1 : 0) :
                                                        (c.value is Enum) ? (byte)c.value :
                                                        (c.value is DateTime) ? ((DateTime)c.value).ToString(Core.CommonVariables.DateFormatString) :
                                                        c.value).ToString())));

                if ((response = (await Client
                      .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
                      new StringContent(
                      $"Action=GetRange&" +
                      $"table={typeof(T).Name}&" +
                      $"justCount=1&" +
                      $"token={App.Token}&" +
                      $"fixedString=1" +
                      //Not an error! Little more attension, & is  ' here
                      (!string.IsNullOrWhiteSpace(whereString) ? $"&{whereString}" : ""))))).StatusCode == HttpStatusCode.OK)
                    return int.TryParse(await response.Content.ReadAsStringAsync(), out int result) ? result : -1;
                else if (response.StatusCode == HttpStatusCode.Forbidden) context?.Close();
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
                if (!preserveContext)
                    context?.Close();
            }
            return -1;
        }

        public static async Task<List<T>> RequestData<T>(Window context, params (string name, object value)[] whereParams) where T : DBRecord
        {
            return await RequestData<T>(context, true, false, null, whereParams);
        }

        public static async Task<List<T>> RequestData<T>(Window context, string column, params (string name, object value)[] whereParams) where T : DBRecord
        {
            return await RequestData<T>(context, true, false, column, whereParams);
        }

        public static async Task<List<T>> RequestData<T>(Window context, bool preserveContext, string column, params (string name, object value)[] whereParams) where T : DBRecord
        {
            return await RequestData<T>(context, true, preserveContext, column, whereParams);
        }

        public static async Task<List<T>> RequestData<T>(Window context, bool strict, bool preserveContext, string column, params (string name, object value)[] whereParams) where T : DBRecord
        {
            try
            {
                HttpResponseMessage response;

                var whereString = string.Join("&", whereParams.Select(c => $"where{c.name}=" + WebUtility.UrlEncode(((c.value is byte[]) ? Convert.ToBase64String((byte[])c.value) :
                                                        (c.value is bool) ? ((bool)c.value ? 1 : 0) :
                                                        (c.value is Enum) ? (byte)c.value :
                                                        (c.value is DateTime) ? ((DateTime)c.value).ToString(Core.CommonVariables.DateFormatString) :
                                                        c.value).ToString())));

                if ((response = (await Client
                      .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
                      new StringContent(
                      $"Action=GetRange&" +
                      $"table={typeof(T).Name}&" +
                      $"token={App.Token}&" +
                      (!string.IsNullOrWhiteSpace(column) ? $"column={column}&" : "") +
                      $"fixedString={(strict ? 1 : 0)}" +
                      //Not an error! Little more attension, & is  ' here
                      (!string.IsNullOrWhiteSpace(whereString) ? $"&{whereString}" : "")
                      )))).StatusCode == HttpStatusCode.OK)
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
                if (!preserveContext)
                    context?.Close();
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
                if ((response = (await Client
                      .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
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
