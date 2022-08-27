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
                await Clients.Users(to, userName).SendAsync("Receive", message, userName);
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
