using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DiscordBotMK1
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        const string MARK_DOWN_3 = "```";
        const string MARK_DOWN_1 = "`";

        // ~say hello -> hello
        [Command("say")]
        [Summary("Echos a message.")]
        public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
        {
            // ReplyAsync is a method on ModuleBase
            await ReplyAsync(echo);
        }

        [Command("주사위")]
        public async Task RandomAsync()
        {
            // ReplyAsync is a method on ModuleBase
            await Context.Channel.SendMessageAsync("주사위 결과 : " + new Random().Next(1, 100).ToString());
        }

        [Command("수집품")]
        [Alias("수", "ㅅ")]
        public async Task CollectionAsync(string data)
        {
            Regex reg = new Regex("[\\d]{1,5}");
            Match match = reg.Match(data);
            if (match.Success)
            {
                int number = Convert.ToInt32(data);
                if (Data.Collection.ContainsKey(number))
                    await ReplyAsync(string.Format(MARK_DOWN_1 + "{0:00000}번 : {1}"+ MARK_DOWN_1, number, Data.Collection[number]));
                else
                    await ReplyAsync(string.Format("미등록 수집품"));
            }
            else
            {
                if (Data.Collection.FirstOrDefault(x => x.Value.Replace(" ", "") == data.Replace(" ", "")).Key != null)
                {
                    int key = Data.Collection.FirstOrDefault(x => x.Value.Replace(" ", "") == data.Replace(" ", "")).Key;
                    await ReplyAsync(string.Format(MARK_DOWN_1 + "{0:00000}번 : {1}" + MARK_DOWN_1, key, Data.Collection[key]));
                }
                else
                    await ReplyAsync(string.Format("미등록 수집품"));
            }
            // ReplyAsync is a method on ModuleBase\
        }

        [Command("레시피")]
        [Alias("레", "ㄹ")]
        public async Task RecipeAsync(string data)
        {
            Regex reg = new Regex("[\\d]{1,5}");
            Match match = reg.Match(data);
            if (match.Success)
            {
                int number = Convert.ToInt32(data);
                if (Data.Collection.ContainsKey(number))
                {
                    var matches = Data.Recipes.Where(pair => pair.Value.Contains(number.ToString("0000#")))
                  .Select(pair => "[" +pair.Key + "] " + pair.Value);
                    if (string.Join("", matches).Length <= 0)
                        await ReplyAsync("합성정보가 없습니다.");
                    else
                    {
                        StringBuilder resultsb = new StringBuilder();
                        resultsb.Append(string.Format("총 {0}건 검색됨\r\n{1}", matches.Count(), MARK_DOWN_3));

                        foreach (string matching in matches)
                        {
                            if (resultsb.Length + matching.Length + 3 > 2000)
                            {
                                resultsb.Append(MARK_DOWN_3);
                                await ReplyAsync(resultsb.ToString());
                                resultsb.Clear();
                                resultsb.Append(MARK_DOWN_3);
                            }
                            else
                            {
                                resultsb.Append(matching + "\r\n");
                            }
                        }
                        resultsb.Append(MARK_DOWN_3);
                        await ReplyAsync(resultsb.ToString());                      
                    }
                }
                else
                    await ReplyAsync(string.Format("미등록 수집품"));
            }
            else
            {
                if (Data.Collection.FirstOrDefault(x => x.Value.Replace(" ", "") == data.Replace(" ", "")).Key != null)
                {
                    int key = Data.Collection.FirstOrDefault(x => x.Value.Replace(" ", "") == data.Replace(" ", "")).Key;

                    if (Data.Collection.ContainsKey(key))
                    {
                        var matches = Data.Recipes.Where(pair => pair.Value.Contains(key.ToString("0000#")))
                  .Select(pair => "[" + pair.Key + "] " + pair.Value);
                        if (string.Join("", matches).Length <= 0)
                            await ReplyAsync("합성정보가 없습니다.");
                        else
                        {
                            StringBuilder resultsb = new StringBuilder();
                            resultsb.Append(string.Format("총 {0}건 검색됨\r\n{1}", matches.Count(), MARK_DOWN_3));

                            foreach (string matching in matches)
                            {
                                if (resultsb.Length + matching.Length + 3 > 2000)
                                {
                                    resultsb.Append(MARK_DOWN_3);
                                    await ReplyAsync(resultsb.ToString());
                                    resultsb.Clear();
                                    resultsb.Append(MARK_DOWN_3);
                                }
                                else
                                {
                                    resultsb.Append(matching + "\r\n");
                                }
                            }
                            resultsb.Append(MARK_DOWN_3);
                            await ReplyAsync(resultsb.ToString());
                        }
                    }
                    else
                        await ReplyAsync(string.Format("미등록 수집품"));
                }
                // ReplyAsync is a method on ModuleBase
            }
        }

        [Command("명령어")]
        [Alias("명", "ㅁㄹㅇ")]
        public async Task CommandInfoAsync()
        {
            await ReplyAsync(@"명령어 목록
```!주사위
!수집품(수,ㅅ) [번호or이름]
!알림(ㅇㄹ) [시|시분|시분초|분|분초|초] [숫자(시)] [숫자(분)] [숫자(초)] [메모]
!레시피(레,ㄹ) [번호or이름]
!선택(ㅅㅌ) [선택1] [선택2] ... (최소 2개 이상)
%주의사항 : 알람은 서버가 종료되거나 업데이트되면 사라집니다.```
                ");
        }
        
        [Group("알림")]
        [Alias("ㅇㄹ")]
        public class Alarm : ModuleBase<SocketCommandContext>
        {
            [Command("시분초")]
            [Alias("ㅅㅂㅊ")]
            public async Task AlarmHourMinuteSecondAsync(int hour, int minute, int second, string memo ="")
            {
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + string.Format(" {0}시간 {1}분 {2}초 후 알람. 메모 : {3}", hour, minute, second, memo));
                System.Threading.Thread.Sleep(((hour * 60 * 60) + (minute * 60) + second) * 1000);
                await Context.User.SendMessageAsync( DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + Context.User.Mention + " 메모 :" + memo);
            }

            [Command("시분")]
            [Alias("ㅅㅂ")]
            public async Task AlarmHourMinuteAsync(int hour, int minute, string memo = "")
            {
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + string.Format(" {0}시간 {1}분 후 알람. 메모 : {2}", hour, minute, memo));
                System.Threading.Thread.Sleep(( hour * 60 * 60 ) + ( minute * 60 ) * 1000);
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + Context.User.Mention + " 메모 :" + memo);
            }

            [Command("시")]
            [Alias("ㅅ")]
            public async Task AlarmHourAsync(int hour, string memo = "")
            {
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + string.Format(" {0}시간 후 알람. 메모 : {1}", hour, memo));
                System.Threading.Thread.Sleep(hour * 60 * 60 * 1000);
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + Context.User.Mention + " 메모 :" + memo);
            }

            [Command("분초")]
            [Alias("ㅂㅊ")]
            public async Task AlarmMinuteSecondAsync(int minute, int second, string memo = "")
            {
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + string.Format(" {0}분 {1}초 후 알람. 메모 : {1}", minute, second, memo));
                System.Threading.Thread.Sleep(( minute * 60 + second) * 1000);
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + Context.User.Mention + " 메모 :" + memo);
            }

            [Command("분")]
            [Alias("ㅂ")]
            public async Task AlarmMinuteAsync(int minute, string memo = "")
            {
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + string.Format(" {0}분 후 알람. 메모 : {1}", minute, memo));
                System.Threading.Thread.Sleep(minute * 60 * 1000);
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + Context.User.Mention + " 메모 :" + memo);
            }

            [Command("초")]
            [Alias("ㅊ")]
            public async Task AlarmSecondAsync(int second, string memo = "")
            {
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + string.Format(" {0}초 후 알람. 메모 : {1}", second, memo));
                System.Threading.Thread.Sleep(second * 1000);
                await Context.User.SendMessageAsync(DateTime.UtcNow.AddHours(9).ToString("[HH:mm:ss]") + Context.User.Mention + " 메모 :" + memo);
            }
        }

        [Command("랜덤게임")]
        [Alias("ㄹㄷㄱㅇ")]
        public async Task RandomGameAsync()
        {
            var users = Context.Channel.GetUsersAsync();
            int index = new Random().Next(0, await users.Count());
            var user = await users.ElementAt(index) as IUser;
            await Context.Channel.SendMessageAsync(user.Mention);
        }

        [Command("선택")]
        [Alias("ㅅㅌ")]
        public async Task ChooseOneAsync(params string[] args)
        {
            if(args.Length > 1)
                await ReplyAsync(args[new Random().Next(0,args.Length)]);
            else
                await ReplyAsync("선택지는 최소 2개!");

        }

        // Create a module with the 'sample' prefix
        [Group("sample")]
        public class Sample : ModuleBase<SocketCommandContext>
        {
            // ~sample square 20 -> 400
            [Command("square")]
            [Summary("Squares a number.")]
            public async Task SquareAsync([Summary("The number to square.")] int num)
            {
                // We can also access the channel from the Command Context.
                await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
            }

            // ~sample userinfo --> foxbot#0282
            // ~sample userinfo @Khionu --> Khionu#8708
            // ~sample userinfo Khionu#8708 --> Khionu#8708
            // ~sample userinfo Khionu --> Khionu#8708
            // ~sample userinfo 96642168176807936 --> Khionu#8708
            // ~sample whois 96642168176807936 --> Khionu#8708
            [Command("userinfo")]
            [Summary("Returns info about the current user, or the user parameter, if one passed.")]
            [Alias("user", "whois")]
            public async Task UserInfoAsync([Summary("The (optional) user to get info for")] SocketUser user = null)
            {
                var userInfo = user ?? Context.Client.CurrentUser;
                await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
            }
        }

        /*
        [Group("admin")]
        public class AdminModule : ModuleBase<SocketCommandContext>
        {
            [Group("clean")]
            public class CleanModule : ModuleBase<SocketCommandContext>
            {
                // ~admin clean 15
                [Command]
                public async Task Default(int count = 10) => Messages(count);

                // ~admin clean messages 15
                [Command("messages")]
                public async Task Messages(int count = 10) { }
            }
            // ~admin ban foxbot#0282
            [Command("ban")]
            public async Task Ban(IGuildUser user) { }
        }*/

        [Command("클리어")]
        [Alias("clear","ㅋㄹㅇ")]
        [Summary("메시지 삭제")]
        public async Task ClearMessages()
        {
            try
            {
                IReadOnlyCollection<IMessage> temp1 = await Context.Channel.GetMessagesAsync(100, CacheMode.AllowDownload, RequestOptions.Default).Last();
                IReadOnlyCollection<IMessage> temp2 = await Context.Channel.GetMessagesAsync(100, CacheMode.CacheOnly, RequestOptions.Default).Last();
                Debug.WriteLine(string.Format("{0} / {1}", temp1.Count(), temp2.Count()));
                List<IMessage> msgs = new List<IMessage>();
                foreach (IMessage msg in temp1)
                {
                    if (msg.Author.Id.Equals(Context.Client.CurrentUser.Id) 
                        && msg.Timestamp > DateTimeOffset.UtcNow.AddDays(-14) )
                        msgs.Add(msg);
                }

                foreach (IMessage msg in temp2)
                {
                    if (msg.Author.Id.Equals(Context.Client.CurrentUser.Id)
                        && msg.Timestamp > DateTimeOffset.UtcNow.AddDays(-14))
                        msgs.Add(msg);
                }

                await Context.Channel.DeleteMessagesAsync(msgs);
                foreach (IMessage msg in msgs)
                {
                    Debug.WriteLine(msg.Content);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
