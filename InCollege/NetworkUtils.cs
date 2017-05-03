using InCollege.Client;
using InCollege.Core.Data.Base;
using InCollege.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public static async Task<IEnumerable<T>> RequestData<T>(Window context) where T : DBRecord
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await new HttpClient()
                      .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                      new StringContent(
                      $"Action=GetRange&" +
                      $"table={typeof(T).Name}&" +
                      $"token={App.Token}")))).StatusCode == HttpStatusCode.OK)
                    return JsonConvert
                        .DeserializeObject<IEnumerable<T>>(await response.Content.ReadAsStringAsync());
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

        public static async Task<string> ExecuteDataAction<T>(IUpdatable context, DBRecord dataObject, DataAction action)
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await new HttpClient()
                      .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                      new StringContent(
                      $"Action={Enum.GetName(typeof(DataAction), action)}&" +
                      $"table={typeof(T).Name}&" +
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
