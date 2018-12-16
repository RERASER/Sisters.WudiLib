﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib.Api.Responses;
using Sisters.WudiLib.Responses;

namespace Sisters.WudiLib
{
    partial class HttpApiClient
    {
        private const string PrivatePath = "send_private_msg";
        private const string GroupPath = "send_group_msg";
        private const string DiscussPath = "send_discuss_msg";
        private const string MessagePath = "send_msg";
        private const string KickGroupMemberPath = "set_group_kick";
        private const string RecallPath = "delete_msg";
        private const string BanGroupMemberPath = "set_group_ban";
        private const string SetGroupCardPath = "set_group_card";
        private const string LoginInfoPath = "get_login_info";
        private const string GroupMemberInfoPath = "get_group_member_info";
        private const string GroupMemberListPath = "get_group_member_list";
        private const string CleanPath = "clean_data_dir";

        private string PrivateUrl => _apiAddress + PrivatePath;
        private string GroupUrl => _apiAddress + GroupPath;
        private string DiscussUrl => _apiAddress + DiscussPath;
        private string MessageUrl => _apiAddress + MessagePath;
        private string KickGroupMemberUrl => _apiAddress + KickGroupMemberPath;
        private string RecallUrl => _apiAddress + RecallPath;
        private string BanGroupMemberUrl => _apiAddress + BanGroupMemberPath;
        private string SetGroupCardUrl => _apiAddress + SetGroupCardPath;
        private string LoginInfoUrl => _apiAddress + LoginInfoPath;
        private string GroupMemberInfoUrl => _apiAddress + GroupMemberInfoPath;
        private string GroupMemberListUrl => _apiAddress + GroupMemberListPath;
        private string CleanUrl => _apiAddress + CleanPath;

        /// <summary>
        /// API 访问 token。请详见插件文档。
        /// </summary>
        public static string AccessToken { get; set; }
    }

    /// <summary>
    /// 通过酷Q HTTP API实现QQ功能。
    /// </summary>
    public partial class HttpApiClient
    {
        private int _isReadyToCleanData;

        /// <summary>
        /// 是否已设置定期清理图片缓存。
        /// </summary>
        public bool IsCleaningData => _isReadyToCleanData != 0;

        private string _apiAddress;

        /// <summary>
        /// 获取或设置 HTTP API 的监听地址
        /// </summary>
        public string ApiAddress
        {
            get => _apiAddress;
            set
            {
                if (value.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    _apiAddress = value;
                }
                else
                {
                    _apiAddress = value + "/";
                }
            }
        }

        /// <summary>
        /// 开始定期访问清理图片的 API。
        /// </summary>
        /// <param name="intervalMinutes">间隔的毫秒数。</param>
        /// <returns>成功开始则为 <c>true</c>，如果之前已经开始过，则为 <c>false</c>。</returns>
        public bool StartClean(int intervalMinutes)
        {
            if (Interlocked.CompareExchange(ref _isReadyToCleanData, 1, 0) == 0)
            {
                var task = new Task(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            await this.CleanImageData();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        await Task.Delay(60000 * intervalMinutes);
                    }
                }, TaskCreationOptions.LongRunning);
                task.Start();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 发送私聊消息。
        /// </summary>
        /// <param name="userId">对方 QQ 号。</param>
        /// <param name="message">要发送的内容（文本）。</param>
        /// <returns>包含消息 ID 的响应数据。</returns>
        public async Task<SendPrivateMessageResponseData> SendPrivateMessageAsync(long userId, string message)
        {
            var data = new
            {
                user_id = userId,
                message,
                auto_escape = true,
            };
            var result = await PostAsync<SendPrivateMessageResponseData>(PrivateUrl, data);
            return result;
        }

        /// <summary>
        /// 发送私聊消息。
        /// </summary>
        /// <param name="qq">对方 QQ 号。</param>
        /// <param name="message">要发送的内容。</param>
        /// <returns>包含消息 ID 的响应数据。</returns>
        public async Task<SendPrivateMessageResponseData> SendPrivateMessageAsync(long qq, Message message)
        {
            var data = new
            {
                user_id = qq,
                message = message.Serializing,
            };
            var result = await PostAsync<SendPrivateMessageResponseData>(PrivateUrl, data);
            return result;
        }

        /// <summary>
        /// 发送群消息。
        /// </summary>
        /// <param name="groupId">群号。</param>
        /// <param name="message">要发送的内容（文本）。</param>
        /// <returns>包含消息 ID 的响应数据。</returns>
        public async Task<SendGroupMessageResponseData> SendGroupMessageAsync(long groupId, string message)
        {
            var data = new
            {
                group_id = groupId,
                message,
                auto_escape = true,
            };
            var result = await PostAsync<SendGroupMessageResponseData>(GroupUrl, data);
            return result;
        }

        /// <summary>
        /// 发送群消息。
        /// </summary>
        /// <param name="groupId">群号。</param>
        /// <param name="message">要发送的内容。</param>
        /// <returns>包含消息 ID 的响应数据。</returns>
        public async Task<SendGroupMessageResponseData> SendGroupMessageAsync(long groupId, Message message)
        {
            var data = new
            {
                group_id = groupId,
                message = message.Serializing,
            };
            var result = await PostAsync<SendGroupMessageResponseData>(GroupUrl, data);
            return result;
        }

        /// <summary>
        /// 发送讨论组消息。
        /// </summary>
        /// <param name="discussId">讨论组 ID。</param>
        /// <param name="message">要发送的内容（文本）。</param>
        /// <returns>包含消息 ID 的响应数据。</returns>
        public async Task<SendDiscussMessageResponseData> SendDiscussMessageAsync(long discussId, string message)
        {
            var data = new
            {
                discuss_id = discussId,
                message,
                auto_escape = true,
            };
            var result = await PostAsync<SendDiscussMessageResponseData>(DiscussUrl, data);
            return result;
        }

        /// <summary>
        /// 发送讨论组消息。
        /// </summary>
        /// <param name="discussId">讨论组 ID。</param>
        /// <param name="message">要发送的内容。</param>
        /// <returns>包含消息 ID 的响应数据。</returns>
        public async Task<SendDiscussMessageResponseData> SendDiscussMessageAsync(long discussId, Message message)
        {
            var data = new
            {
                discuss_id = discussId,
                message = message.Serializing,
            };
            var result = await PostAsync<SendDiscussMessageResponseData>(DiscussUrl, data);
            return result;
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="endpoint">要发送到的终结点。</param>
        /// <param name="message">要发送的消息。</param>
        /// <returns>包含消息 ID 的响应数据。</returns>
        public async Task<SendMessageResponseData> SendMessageAsync(Posts.Endpoint endpoint, Message message)
        {
            var data = JObject.FromObject(endpoint);
            data["message"] = JToken.FromObject(message.Serializing);
            var result = await PostAsync<SendMessageResponseData>(MessageUrl, data);
            return result;
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="endpoint">要发送到的终结点。</param>
        /// <param name="message">要发送的消息（文本）。</param>
        /// <returns>包含消息 ID 的响应数据。</returns>
        public async Task<SendMessageResponseData> SendMessageAsync(Posts.Endpoint endpoint, string message)
        {
            var data = JObject.FromObject(endpoint);
            data["message"] = JToken.FromObject(message);
            data["auto_escape"] = true;
            var result = await PostAsync<SendMessageResponseData>(MessageUrl, data);
            return result;
        }

        /// <summary>
        /// 群组踢人。
        /// </summary>
        /// <param name="groupId">群号。</param>
        /// <param name="userId">要踢的 QQ 号。</param>
        /// <returns>是否成功。注意：酷 Q 未处理错误，所以无论是否成功都会返回<c>true</c>。</returns>
        public async Task<bool> KickGroupMemberAsync(long groupId, long userId)
        {
            var data = new
            {
                group_id = groupId,
                user_id = userId,
            };
            var success = await PostAsync(KickGroupMemberUrl, data);
            return success;
        }

        /// <summary>
        /// 撤回消息（需要Pro）。
        /// </summary>
        /// <param name="message">消息返回值。</param>
        /// <returns>是否成功。</returns>
        public async Task<bool> RecallMessageAsync(SendMessageResponseData message)
        {
            return await RecallMessageAsync(message.MessageId);
        }

        /// <summary>
        /// 撤回消息（需要Pro）
        /// </summary>
        /// <param name="messageId">消息 ID。</param>
        /// <returns>是否成功。</returns>
        public async Task<bool> RecallMessageAsync(int messageId)
        {
            var data = new { message_id = messageId };
            var success = await PostAsync(RecallUrl, data);
            return success;
        }

        /// <summary>
        /// 群组单人禁言。
        /// </summary>
        /// <param name="groupId">群号。</param>
        /// <param name="userId">要禁言的 QQ 号。</param>
        /// <param name="duration">禁言时长，单位秒，0 表示取消禁言。</param>
        /// <exception cref="ApiAccessException"></exception>
        /// <returns>如果操作成功，返回 <c>true</c>。</returns>
        public async Task<bool> BanGroupMember(long groupId, long userId, int duration)
        {
            var data = new
            {
                group_id = groupId,
                user_id = userId,
                duration,
            };
            return await PostAsync(BanGroupMemberUrl, data);
        }

        /// <summary>
        /// 设置群名片。
        /// </summary>
        /// <param name="groupId">群号。</param>
        /// <param name="userId">要设置的 QQ 号。</param>
        /// <param name="card">群名片内容，不填或空字符串表示删除群名片。</param>
        /// <returns>是否成功。</returns>
        public async Task<bool> SetGroupCard(long groupId, long userId, string card)
        {
            var data = new
            {
                group_id = groupId,
                user_id = userId,
                card,
            };
            return await PostAsync(SetGroupCardUrl, data);
        }

        /// <summary>
        /// 获取登录信息。
        /// </summary>
        /// <returns>登录信息。</returns>
        public async Task<LoginInfo> GetLoginInfoAsync()
        {
            var data = new object();
            var result = await PostAsync<LoginInfo>(LoginInfoUrl, data);
            return result;
        }

        /// <summary>
        /// 获取群成员信息。
        /// </summary>
        /// <param name="group">群号。</param>
        /// <param name="qq">QQ 号（不可以是登录号）。</param>
        /// <returns>获取到的成员信息。</returns>
        public async Task<GroupMemberInfo> GetGroupMemberInfoAsync(long group, long qq)
        {
            var data = new
            {
                group_id = group,
                user_id = qq,
                no_cache = true,
            };
            var result = await PostAsync<GroupMemberInfo>(GroupMemberInfoUrl, data);
            return result;
        }

        /// <summary>
        /// 获取群成员列表。
        /// </summary>
        /// <param name="group">群号。</param>
        /// <returns>响应内容为数组，每个元素的内容和上面的 GetGroupMemberInfoAsync() 方法相同，但对于同一个群组的同一个成员，获取列表时和获取单独的成员信息时，某些字段可能有所不同，例如 area、title 等字段在获取列表时无法获得，具体应以单独的成员信息为准。</returns>
        public async Task<GroupMemberInfo[]> GetGroupMemberListAsync(long group)
        {
            var data = new
            {
                group_id = group,
            };
            var result = await PostAsync<GroupMemberInfo[]>(GroupMemberListUrl, data);
            return result;
        }

        /// <summary>
        /// 清理数据目录中的图片。
        /// </summary>
        /// <returns></returns>
        public async Task CleanImageData()
            => await PostAsync(CleanUrl, new { data_dir = "image" });
        #region Utilities

        private static async Task<CqHttpApiResponse<T>> PostApiAsync<T>(string url, object data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data), "data不能为null");
            try
            {
                string json = JsonConvert.SerializeObject(data);
                using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var http = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(HttpApiClient.AccessToken))
                    {
                        //content.Headers.Add("Authorization", "Token " + HttpApiClient.AccessToken);
                        http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Token " + HttpApiClient.AccessToken);
                    }
                    using (var response = (await http.PostAsync(url, content)).EnsureSuccessStatusCode())
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<CqHttpApiResponse<T>>(responseContent);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                throw new ApiAccessException("访问 API 时出现错误。", e);
            }
        }

        /// <summary>
        /// 通过 POST 请求访问API，返回数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="url">API请求地址</param>
        /// <param name="data">请求参数</param>
        /// <returns>从 HTTP API 返回的数据</returns>
        private static async Task<T> PostAsync<T>(string url, object data)
        {
            var response = await PostApiAsync<T>(url, data);
            return response.Retcode == CqHttpApiResponse.RetcodeOK ? response.Data : default(T);
        }

        /// <exception cref="ApiAccessException">网络错误等。</exception>
        private static async Task<bool> PostAsync(string url, object data)
        {
            try
            {
                var response = await PostApiAsync<object>(url, data);
                return response.Retcode == CqHttpApiResponse.RetcodeOK;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        #endregion
    }
}
