﻿using InCollege.Client;
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
            Client.DefaultRequestHeaders.Add("Keep-Alive", "timeout=10000");
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
            return await RequestData<T>(context, true, false, false, null, whereParams);
        }

        public static async Task<List<T>> RequestData<T>(Window context, string column, params (string name, object value)[] whereParams) where T : DBRecord
        {
            return await RequestData<T>(context, true, false, false, column, whereParams);
        }

        public static async Task<List<T>> RequestData<T>(Window context, bool preserveContext, string column, params (string name, object value)[] whereParams) where T : DBRecord
        {
            return await RequestData<T>(context, true, false, preserveContext, column, whereParams);
        }

        public static async Task<List<T>> RequestData<T>(Window context, bool strict, bool orAll, bool preserveContext, string column, params (string name, object value)[] whereParams) where T : DBRecord
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
                      $"orAll={(orAll ? 1 : 0)}&" +
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

        public static async Task<string> RemoveWhere<T>(IUpdatable context, params (string name, object value)[] whereParams)
        {
            try
            {
                var whereString = string.Join("&", whereParams.Select(c => $"where{c.name}=" + WebUtility.UrlEncode(((c.value is byte[]) ? Convert.ToBase64String((byte[])c.value) :
                                        (c.value is bool) ? ((bool)c.value ? 1 : 0) :
                                        (c.value is Enum) ? (byte)c.value :
                                        (c.value is DateTime) ? ((DateTime)c.value).ToString(Core.CommonVariables.DateFormatString) :
                                        c.value).ToString())));

                HttpResponseMessage response;
                if ((response = (await Client
                      .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
                      new StringContent(
                      $"Action=RemoveWhere&" +
                      $"table={typeof(T).Name}&" +
                      $"token={App.Token}" +
                      //Not an error! Little more attension, & is  ' here
                      (!string.IsNullOrWhiteSpace(whereString) ? $"&{whereString}" : ""))))).StatusCode == HttpStatusCode.OK)
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

        public static async Task<int> CheckMessages(int partnerID = -1)
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await Client
                .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
                new StringContent(
                $"Action=Chat&" +
                $"token={App.Token}&" +
                $"mode={(byte)ChatRequestMode.CheckMessages}" +
                //Here               '
                (partnerID != -1 ? $"&partnerID={partnerID}" : ""))))).StatusCode == HttpStatusCode.OK)
                    return int.Parse(await response.Content.ReadAsStringAsync());
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return -1;
        }

        public static async Task<List<StatementResult>> RequestStudyResults()
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await Client
                .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
                new StringContent($"Action=GetStudyResults&token={App.Token}")))).StatusCode == HttpStatusCode.OK)
                    return JsonConvert.DeserializeObject<List<StatementResult>>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return null;
        }

        public static async Task<List<Message>> RequestConversation(int partnerID)
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await Client
                .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
                new StringContent(
                $"Action=Chat&" +
                $"token={App.Token}&" +
                $"mode={(byte)ChatRequestMode.Conversation}&" +
                $"partnerID={partnerID}")))).StatusCode == HttpStatusCode.OK)
                    return JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return null;
        }

        public static async Task<string> CheckVersion()
        {
            try
            {
                return await (await Client.GetAsync($"http://{ClientConfiguration.Instance.HostName}:{ClientConfiguration.Instance.Port}/?CheckVersion=1")).Content.ReadAsStringAsync();
            }
            catch (HttpRequestException exc)
            {
                return $"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}";
            }
        }

        public static async Task<List<Account>> RequestFriendList()
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await Client
                .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
                new StringContent(
                $"Action=Chat&" +
                $"token={App.Token}&" +
                $"mode={(byte)ChatRequestMode.Friends}")))).StatusCode == HttpStatusCode.OK)
                    return JsonConvert.DeserializeObject<List<Account>>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return null;
        }

        public static async Task<bool> SendMessage(int partnerID, Message message)
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await Client
                .PostAsync(ClientConfiguration.Instance.DataHandlerPath,
                new StringContent(
                $"Action=Chat&" +
                $"token={App.Token}&" +
                $"mode={(byte)ChatRequestMode.Send}&" +
                $"partnerID={partnerID}&" +
                message.POSTSerializedClear)))).StatusCode == HttpStatusCode.OK)
                    return true;
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return false;
        }

        public static async Task Disconnect()
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            await Client.PostAsync(ClientConfiguration.Instance.DataHandlerPath, new StringContent($"Action=Disconnect"));
        }
    }
}
