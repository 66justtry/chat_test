using chat_test.Models;
using Microsoft.AspNetCore.SignalR;


namespace chat_test
{
    public class ChatHub : Hub
    {
        public record class AnswersList(int id, string text);
        public async Task Send(string message, string to)
        {
            // получение текущего пользователя, который отправил сообщение
            //var userName = Context.UserIdentifier;
            if (Context.UserIdentifier is string userName)
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    if (db.Users.Any(u => u.login == to))
                    {
                        Message msg = new Message(userName, to, message);
                        db.Messages.Add(msg);
                        db.SaveChanges();
                        int msgid = db.Messages.Where(u => u.sender == userName).Max(u => u.id);



                        //если это первое сообщение в переписке с получателем, добавляем получателя в список чатов
                        var sndr = db.Users.Where(u => u.login == userName).First();

                        //по умолчанию пишем только существующим пользователям!!


                        string[] chats = sndr.chats.Split(' ');

                        if (!chats.Contains(to))
                        {
                            sndr.chats += $" {to}";
                            db.Users.Update(sndr);

                            var rcvr = db.Users.Where(u => u.login == to).First();
                            chats = rcvr.chats.Split(' ');

                            if (!chats.Contains(to))
                            {
                                rcvr.chats += $" {userName}";
                                db.Users.Update(rcvr);
                                db.SaveChanges();
                                await Clients.Users(to, userName).SendAsync("ReceiveNewChat", to, userName);
                            }
                            else
                            {
                                await Clients.Users(userName).SendAsync("ReceiveNewChat", to, userName);
                            }

                            //доделать открытие у отправителя!!
                        }



                        await Clients.Users(to, userName).SendAsync("Receive", message, userName, msgid, "");
                    }
                    else //если сообщение для группы
                    {
                        Message msg = new Message(userName, to, message);
                        db.Messages.Add(msg);
                        db.SaveChanges();
                        int msgid = db.Messages.Where(u => u.sender == userName).Max(u => u.id);

                        var sndr = db.Users.Where(u => u.login == userName).First();
                        string[] chats = sndr.chats.Split(' '); //список чатов отправителя
                        if (!chats.Contains(to)) //если такого чата еще не было в списке
                        {
                            sndr.chats += $" {to}";
                            db.Users.Update(sndr);
                            await Clients.Users(userName).SendAsync("ReceiveNewChat", to, userName);
                        }


                        await Groups.AddToGroupAsync(Context.ConnectionId, to);

                        await Clients.Group(to).SendAsync("Receive", message, userName, msgid, to);
                    }



                }



            }
        }

        public async Task SendToLoad(string to)
        {
            if (Context.UserIdentifier is string userName)
            {

                //добавить проверку на удаленные сообщения!!!!!


                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    var answers = new List<AnswersList>(); //массив сообщений-ответов



                    if (db.Users.Any(u => u.login == to)) //если это личная переписка с человеком
                    {


                        //получаем 20 последних сообщений -- добавить
                        var list = db.Messages.Where(m =>
                            ((m.sender == userName && m.receiver == to) || (m.receiver == userName && m.sender == to))).ToList<Message>();
                        foreach (var message in list)
                        {
                            if (message.answerto_id != 0)
                            {
                                var text_to_add = db.Messages.Where(m => m.id == message.answerto_id).First().text;
                                answers.Add(new AnswersList(message.answerto_id, text_to_add));
                            }
                        }

                        await Clients.User(userName).SendAsync("ReceiveToLoad", list, answers);


                    }
                    else
                    {
                        //добавить -- загрузка первых 20 сообщений
                        var list = db.Messages.Where(m => m.receiver == to).ToList<Message>();
                        foreach (var message in list)
                        {
                            if (message.answerto_id != 0)
                            {
                                var text_to_add = db.Messages.Where(m => m.id == message.answerto_id).First().text;
                                answers.Add(new AnswersList(message.answerto_id, text_to_add));
                            }
                        }

                        await Groups.AddToGroupAsync(Context.ConnectionId, to);

                        //await Groups.AddToGroupAsync(userName, to);
                        await Clients.User(userName).SendAsync("ReceiveToLoad", list, answers);

                    }


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

                    //ответ сюда, не важно, чат или приватное сообщение


                    if (answer_text.Length > 30)
                        answer_text = answer_text.Substring(0, 27) + "...";


                    if (db.Users.Any(u => u.login == to)) //если это личная переписка с человеком
                    {
                        await Clients.Users(to, userName).SendAsync("ReceiveAnswer", message, userName, msgid, answer_text, "");
                    }
                    else
                    {
                        await Clients.Group(to).SendAsync("ReceiveAnswer", message, userName, msgid, answer_text, to);
                    }

                        




                }



            }
        }

        public async Task AnswerPrivate(string message, string to, string answer_text, int answer_id)
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

                    //отправляем в личные сообщения тому, кто написал


                    if (answer_text.Length > 30)
                        answer_text = answer_text.Substring(0, 27) + "...";

                    await Clients.Users(to, userName).SendAsync("ReceiveAnswerPrivate", message, userName, msgid, answer_text);


                }

            }
        }





        public async Task Change(string message, string to, int msgid)
        {

            if (Context.UserIdentifier is string userName)
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {

                    var msg = db.Messages.Where(m => m.id == msgid).First();
                    msg.text = message;
                    db.Messages.Update(msg);
                    db.SaveChanges();

                    if (db.Users.Any(u => u.login == to)) //если личный чат с пользователем
                    {
                        await Clients.Users(to, userName).SendAsync("ReceiveChange", message, userName, msgid, "");
                    }
                    else
                    {
                        await Clients.Group(to).SendAsync("ReceiveChange", message, userName, msgid, to);
                    }

                }

            }
        }

        public async Task DeleteForAll(string to, int msgid)
        {
            //полное удаление сообщения из бд

            if (Context.UserIdentifier is string userName)
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    var msg = db.Messages.Where(m => m.id == msgid).First();
                    db.Messages.Remove(msg);
                    db.SaveChanges();

                    if (db.Users.Any(u => u.login == to))
                    {
                        await Clients.Users(to, userName).SendAsync("ReceiveDeleteForAll", userName, msgid, "");
                    }
                    else
                    {
                        await Clients.Group(to).SendAsync("ReceiveDeleteForAll", userName, msgid, to);
                    }
                }



            }

        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Notify", $"Приветствуем, {Context.UserIdentifier}. Выберите чат.");
            await base.OnConnectedAsync();
        }
    }
}



















