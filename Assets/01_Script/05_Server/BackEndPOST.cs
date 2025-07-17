using System;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Cysharp.Threading.Tasks;
using BackEnd;

public class BackEndPOST
{
    /// <summary>
    /// 우편 아이템 정보를 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct UPostChartItem
    {
        public int itemID;
        public int itemCount;
    }

    /// <summary>
    /// 우편 정보를 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct UPostItem
    {
        public PostType postType;
        public string title;
        public string content;
        public DateTime expirationDate;
        public DateTime reservationDate;
        public DateTime sentDate;
        public string nickname;
        public string inDate;
        public string author; // 관리자 우편만 존재
        public string rankType; // 랭킹 우편만 존재
        public List<UPostChartItem> items;

        /// <summary>
        /// 만료일까지 남은 시간 (TimeSpan)
        /// </summary>
        public TimeSpan TimeRemaining => expirationDate - DateTime.UtcNow;

        /// <summary>
        /// 우편이 만료되었는지 확인
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= expirationDate;

        /// <summary>
        /// 만료일까지 남은 시간을 문자열로 반환
        /// </summary>
        public string TimeRemainingString
        {
            get
            {
                if (IsExpired)
                {
                    return "Expired";
                }

                var timeSpan = TimeRemaining;

                if (timeSpan.TotalDays >= 1)
                {
                    return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    return $"{timeSpan.Hours}h {timeSpan.Minutes}m";
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
                }
                else
                {
                    return $"{timeSpan.Seconds}s";
                }
            }
        }

        /// <summary>
        /// 만료일까지 남은 시간을 간단한 형식으로 반환
        /// </summary>
        public string TimeRemainingSimple
        {
            get
            {
                if (IsExpired)
                {
                    return "Expired";
                }

                var timeSpan = TimeRemaining;

                if (timeSpan.TotalDays >= 1)
                {
                    return $"{(int)timeSpan.TotalDays}d";
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    return $"{timeSpan.Hours}h";
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    return $"{timeSpan.Minutes}m";
                }
                else
                {
                    return $"{timeSpan.Seconds}s";
                }
            }
        }

        /// <summary>
        /// 우편 아이템들을 RewardManager에서 처리할 수 있는 형태로 변환하여 반환
        /// </summary>
        /// <returns>보상 아이템 리스트</returns>
        public List<St_RewardItemList> GetRewardList()
        {
            var rewardList = new List<St_RewardItemList>();

            if (items == null || items.Count == 0)
            {
                return rewardList;
            }

            foreach (var item in items)
            {
                var rewardItem = new St_RewardItemList
                {
                    _itemid = item.itemID,
                    _itemvalue = item.itemCount
                };
                rewardList.Add(rewardItem);
            }

            return rewardList;
        }


    }

    /// <summary>
    /// 관리자 우편 리스트를 가져오는 메서드 (비동기)
    /// </summary>
    /// <param name="limit">불러올 우편 개수</param>
    /// <returns>우편 리스트</returns>
    public static async UniTask<List<UPostItem>> GetPOSTList(int limit)
    {
        // limit이 10 미만일 경우 10으로 고정
        if (limit < 10) limit = 10;
        if (limit > 100) limit = 100;

        var completionSource = new UniTaskCompletionSource<List<UPostItem>>();

        Backend.UPost.GetPostList(PostType.Admin, limit, bro =>
        {
            if (!bro.IsSuccess())
            {
                Debug.LogError($"관리자 우편 불러오기 실패: {bro.ToString()}");
                completionSource.TrySetResult(new List<UPostItem>());
                return;
            }

            List<UPostItem> postList = ParsePostList(bro, PostType.Admin);
            completionSource.TrySetResult(postList);
        });

        return await completionSource.Task;
    }

    /// <summary>
    /// 우편 하나를 수령하는 메서드 (비동기)
    /// </summary>
    /// <param name="postInDate">수령할 우편의 inDate</param>
    /// <returns>수령 성공 여부</returns>
    public static async UniTask<bool> RemovePost(string postInDate)
    {
        var completionSource = new UniTaskCompletionSource<bool>();

        Backend.UPost.ReceivePostItem(PostType.Admin, postInDate, bro =>
        {
            if (!bro.IsSuccess())
            {
                Debug.LogError($"우편 수령 실패: {bro.ToString()}");
                BackEndLog.WriteLog(LogType.POST, $"우편ID:{postInDate} 수령 실패");
                completionSource.TrySetResult(false);
                return;
            }

            Debug.Log($"우편ID:{postInDate} 수령 성공");
            BackEndLog.WriteLog(LogType.POST, $"우편ID:{postInDate} 수령 성공");
            completionSource.TrySetResult(true);
        });

        return await completionSource.Task;
    }

    /// <summary>
    /// 모든 우편을 수령하는 메서드 (비동기)
    /// </summary>
    /// <returns>수령 성공 여부</returns>
    public static async UniTask<bool> RemoveAllPost()
    {
        var completionSource = new UniTaskCompletionSource<bool>();

        Backend.UPost.ReceivePostItemAll(PostType.Admin, bro =>
        {
            if (!bro.IsSuccess())
            {
                Debug.LogError($"모든 우편 수령 실패: {bro.ToString()}");
                completionSource.TrySetResult(false);
                return;
            }

            Debug.Log("모든 우편 수령 성공");
            completionSource.TrySetResult(true);
        });

        var success = await completionSource.Task;
        if (success)
        {
            BackEndLog.WriteLog(LogType.POST, $"모든 우편 수령 성공");
        }
        else
        {
            BackEndLog.WriteLog(LogType.POST, $"모든 우편 수령 실패");
        }

        return success;
    }

    /// <summary>
    /// BackendReturnObject에서 우편 리스트를 파싱하는 메서드
    /// </summary>
    /// <param name="bro">뒤끝 응답 객체</param>
    /// <param name="postType">우편 타입</param>
    /// <returns>파싱된 우편 리스트</returns>
    private static List<UPostItem> ParsePostList(BackendReturnObject bro, PostType postType)
    {
        List<UPostItem> postItemList = new List<UPostItem>();

        JsonData postListJson = bro.GetReturnValuetoJSON()["postList"];

        for (int i = 0; i < postListJson.Count; i++)
        {
            UPostItem postItem = new UPostItem();

            postItem.inDate = postListJson[i]["inDate"].ToString();
            postItem.title = postListJson[i]["title"].ToString();
            postItem.postType = postType;

            if (postType == PostType.Admin || postType == PostType.Rank)
            {
                postItem.content = postListJson[i]["content"].ToString();
                postItem.expirationDate = DateTime.Parse(postListJson[i]["expirationDate"].ToString());
                postItem.reservationDate = DateTime.Parse(postListJson[i]["reservationDate"].ToString());
                postItem.nickname = postListJson[i]["nickname"]?.ToString();
                postItem.sentDate = DateTime.Parse(postListJson[i]["sentDate"].ToString());

                if (postListJson[i].ContainsKey("author"))
                {
                    postItem.author = postListJson[i]["author"].ToString();
                }

                if (postListJson[i].ContainsKey("rankType"))
                {
                    postItem.rankType = postListJson[i]["rankType"].ToString();
                }
            }

            // 아이템 파싱
            postItem.items = new List<UPostChartItem>();
            if (postListJson[i]["items"].Count > 0)
            {
                for (int itemNum = 0; itemNum < postListJson[i]["items"].Count; itemNum++)
                {
                    UPostChartItem item = new UPostChartItem();
                    item.itemCount = int.Parse(postListJson[i]["items"][itemNum]["itemCount"].ToString());
                    int.TryParse(postListJson[i]["items"][itemNum]["item"]["ItemID"].ToString(), out var itemid);
                    if (itemid > 0)
                    {
                        item.itemID = itemid;
                    }
                    postItem.items.Add(item);
                }
            }

            postItemList.Add(postItem);
        }

        return postItemList;
    }
}