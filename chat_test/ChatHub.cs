using Microsoft.AspNetCore.SignalR;


namespace chat_test
{
    public class ChatHub : Hub
    {

        public async Task Send(string message, string to)
        {
            // получение текущего пользователя, который отправил сообщение
            //var userName = Context.UserIdentifier;
            if (Context.UserIdentifier is string userName)
            {
                //создание объекта message
                //отправка сообщения в бд
                int msgid = message.GetHashCode();
                
                await Clients.Users(to, userName).SendAsync("Receive", message, userName, msgid);
            }
        }


        public async Task Answer(string message, string to, string answer_text, int answer_id)
        {
            // получение текущего пользователя, который отправил сообщение
            //var userName = Context.UserIdentifier;
            if (Context.UserIdentifier is string userName)
            {
                //создание объекта message
                //answer_id - id сообщения на которое отвечаем, добавляем в объект message, при загрузке из бд сможем показать что это ответ
                //отправка сообщения в бд
                int msgid = message.GetHashCode();
                if (answer_text.Length > 30)
                    answer_text = answer_text.Substring(0, 27) + "...";

                await Clients.Users(to, userName).SendAsync("ReceiveAnswer", message, userName, msgid, answer_text);
            }
        }



        public async Task Change(string message, string to, int msgid)
        {
            if (Context.UserIdentifier is string userName)
            {
                //создание объекта message
                //изменение сообщения в бд
                

                await Clients.Users(to, userName).SendAsync("ReceiveChange", message, userName, msgid);
            }
        }

        public async Task DeleteForAll(string to, int msgid)
        {
            // получение текущего пользователя, который отправил сообщение
            //var userName = Context.UserIdentifier;
            if (Context.UserIdentifier is string userName)
            {
                //удаление объекта message из бд
                //обновление бд
                

                await Clients.Users(to, userName).SendAsync("ReceiveDeleteForAll", userName, msgid);
            }
        }




        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("Notify", $"Приветствуем {Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }



















        //public async Task Enter(string username, string groupName)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //    await Clients.Group(groupName).SendAsync("Notify", $"{username} вошел в чат в группу {groupName}");
        //}
        //public async Task Send(string message, string userName, string groupName)
        //{
        //    Console.WriteLine($"{userName}: {message} id: {Clients.Caller}");
        //    await Clients.Group(groupName).SendAsync("Receive", message, userName);
        //    await Clients.User("admin").SendAsync("Receive", message, userName);
        //}
    }
}
