using chat_test.Models;
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
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    Message msg = new Message(userName, to, message);
                    db.Messages.Add(msg);
                    db.SaveChanges();
                    int msgid = db.Messages.Where(u => u.sender == userName).Max(u => u.id);

                    
                    
                    //если это первое сообщение в переписке с получателем, добавляем получателя в список чатов
                    var sndr = db.Users.Where(u => u.login == userName).First();

                    //по умолчанию пишем только существующим пользователям!!
                    //если пользователь не существует, считаем что пишем в чат -- добавить!!!!!!
                    
                    string[] chats = sndr.chats.Split(' ');

                    if (!chats.Contains(to))
                    {
                        sndr.chats += $" {to}";
                        db.Users.Update(sndr);

                        var rcvr = db.Users.Where(u => u.login == to).First();
                        rcvr.chats += $" {userName}";
                        db.Users.Update(rcvr);
                        db.SaveChanges();
                        //добавить правильное добавление нового чата - вызов метода у себя и получателя для обновления списка чатов
                        //у себя - сразу открыть этот чат
                    }

                    



                    await Clients.Users(to, userName).SendAsync("Receive", message, userName, msgid);


                }
                    //создание объекта message
                    //отправка сообщения в бд
                    
                
                
            }
        }

        public async Task SendToLoad(string to)
        {
            if (Context.UserIdentifier is string userName)
            {
                //добавить проверку на чат!!!!!!!
                //добавить проверку на удаленные сообщения!!!!!
                //проверка на ответы

                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    //получаем 20 последних сообщений -- добавить
                    var list = db.Messages.Where(m =>
                        ((m.sender == userName && m.receiver == to) || (m.receiver == userName && m.sender == to))).ToList<Message>();

                    //await Clients.Caller.SendAsync("ReceiveToLoad", list);
                    await Clients.User(userName).SendAsync("ReceiveToLoad", list);

                }

            }
        }



        public async Task Answer(string message, string to, string answer_text, int answer_id)
        {
            
            if (Context.UserIdentifier is string userName)
            {

                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    Message msg = new Message(userName, to, message);
                    msg.answerto_id = answer_id;
                    db.Messages.Add(msg);
                    db.SaveChanges();
                    int msgid = db.Messages.Where(u => u.sender == userName).Max(u => u.id);

                    //добавить работу с чатами!!
                    //сейчас работает только с ответами в личные сообщения одному пользователю


                    if (answer_text.Length > 30)
                        answer_text = answer_text.Substring(0, 27) + "...";

                    await Clients.Users(to, userName).SendAsync("ReceiveAnswer", message, userName, msgid, answer_text);




                }



            }
        }



        public async Task Change(string message, string to, int msgid)
        {
            //добавить работу с группой!!!

            if (Context.UserIdentifier is string userName)
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    var msg = db.Messages.Where(m => m.id == msgid).First();
                    msg.text = message;
                    db.Messages.Update(msg);
                    db.SaveChanges();


                    await Clients.Users(to, userName).SendAsync("ReceiveChange", message, userName, msgid);
                }
                    
            }
        }

        public async Task DeleteForAll(string to, int msgid)
        {
            //полное удаление сообщения из бд

            //добавить для удаления у пользователей группы!!!
            if (Context.UserIdentifier is string userName)
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    var msg = db.Messages.Where(m => m.id == msgid).First();
                    db.Messages.Remove(msg);
                    db.SaveChanges();


                    await Clients.Users(to, userName).SendAsync("ReceiveDeleteForAll", userName, msgid);
                }

            }
        }




        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Notify", $"Приветствуем, {Context.UserIdentifier}\n Выберите чат для начала общения");
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
