﻿$('body').on('click', '.chatline', function () {
    /*console.log('jquery_' + $(this).text());*/
    var rec = document.getElementById("receiver").value;

    if (rec != $(this).text()) {
        console.log($(this).text());
        rec = $(this).text();

        //при нажатии на строку нужно удалить все блоки div с сообщениями, потом вызвать метод из хаба для получения сообщений

        //var allchat = document.getElementById("chatroom").children;
        //for (let i = 0; i < allchat.length; i++) {
        //    allchat[i].remove();
        //}

        hubConnection.invoke("SendToLoad", $(this).text())
            .catch(error => console.error(error));
    }

    document.getElementById("receiver").value = $(this).text();


});




let token;      // токен
const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chat", { accessTokenFactory: () => token })
    .build();

// аутентификация
document.getElementById("loginBtn").addEventListener("click", async () => {

    const response = await fetch("/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            login: document.getElementById("login").value,
            username: document.getElementById("username").value,
            password: "pass",
            chats: "all admin"
        })
    });

    // если запрос прошел нормально
    if (response.ok === true) {
        // получаем данные
        const data = await response.json();
        token = data.access_token;
        username = data.username;
        let chats = data.chats.toString();


        document.getElementById("loginBtn").disabled = true;

        hubConnection.start()       // начинаем соединение с хабом
            .then(() => document.getElementById("sendBtn").disabled = false)
            .catch(err => console.error(err.toString()));

        const elem = document.getElementById("chatsList");

        var arr = chats.split(' ');

        for (let i = 0; i < arr.length; i++) {
            const e = document.createElement("p");
            e.textContent = arr[i];
            e.className = "chatline";
            elem.appendChild(e);
        }



    }
    else {
        // если произошла ошибка, получаем код статуса
        console.log(`Status: ${response.status}`);
    }
});




// отправка сообщения от простого пользователя
document.getElementById("sendBtn").addEventListener("click", () => {

    const message = document.getElementById("message").value;
    const receiver = document.getElementById("receiver").value;
    document.getElementById("message").value = "";
    hubConnection.invoke("Send", message, receiver)
        .catch(error => console.error(error));
});

// получение сообщения от пользователя
hubConnection.on("Receive", (message, user, msgid, to_group) => {

    if (document.getElementById("receiver").value !== to_group) {
        if (document.getElementById("receiver").value !== user) {
            if (document.getElementById("login").value !== user)
                return;
        }
    }




    const block = document.createElement("div");

    const login_name = document.getElementById("login");

    if (login_name.value === user)
        block.className = "msg_block_me";
    else
        block.className = "msg_block_another";


    const elem = document.createElement("p");

    const name_span = document.createElement("span");
    name_span.className = "name_span";
    const text_span = document.createElement("span");
    text_span.className = "text_span";

    name_span.textContent = `${user}: `;
    text_span.textContent = message;

    let br = document.createElement("br");

    elem.appendChild(name_span);
    elem.appendChild(br);
    elem.appendChild(text_span);


    block.appendChild(elem);

    const msg_id = document.createElement("h3");
    msg_id.className = "id_m";
    msg_id.textContent = msgid;

    block.appendChild(msg_id);

    const scroll = document.getElementById("chatroom");

    document.getElementById("chatroom").appendChild(block);
    scroll.scroll(0, 1000);
});



//hubConnection.on("ReceiveToLoadTest", (list) => {
//    for (let i = 0; i < list.length; i++)
//        console.log(list[0].text)
//});


hubConnection.on("ReceiveToLoad", (list, answers) => {

    let allchat = document.getElementById("chatroom").childElementCount;
    for (let i = 0; i < allchat; i++) {
        document.getElementById("chatroom").removeChild(document.getElementById("chatroom").firstChild);
    }

    for (let i = 0; i < list.length; i++) {

        if (list[i].answerto_id === 0) {
            console.log(list[i].text);
            var block = document.createElement("div");

            var login_name = document.getElementById("login");

            if (login_name.value === list[i].sender)
                block.className = "msg_block_me";
            else
                block.className = "msg_block_another";

            const elem = document.createElement("p");

            const name_span = document.createElement("span");
            name_span.className = "name_span";
            const text_span = document.createElement("span");
            text_span.className = "text_span";

            name_span.textContent = `${list[i].sender}: `;
            text_span.textContent = list[i].text;

            let br = document.createElement("br");

            elem.appendChild(name_span);
            elem.appendChild(br);
            elem.appendChild(text_span);


            block.appendChild(elem);

            var msg_id = document.createElement("h3");
            msg_id.className = "id_m";
            msg_id.textContent = list[i].id;

            block.appendChild(msg_id);

            var scroll = document.getElementById("chatroom");

            document.getElementById("chatroom").appendChild(block);
            scroll.scroll(0, 1000);
        }
        else {
            console.log(list[i].text);
            const block = document.createElement("div");

            const login_name = document.getElementById("login");

            if (login_name.value === list[i].sender)
                block.className = "msg_block_me";
            else
                block.className = "msg_block_another";

            const elem = document.createElement("p");

            const name_span = document.createElement("span");
            name_span.className = "name_span";
            const text_span = document.createElement("span");
            text_span.className = "text_span";

            name_span.textContent = `${list[i].sender}: `;
            text_span.textContent = list[i].text;

            let br = document.createElement("br");

            elem.appendChild(name_span);
            elem.appendChild(br);
            elem.appendChild(text_span);

            var answerto = document.createElement("p");
            answerto.className = "answer_text";

            for (let j = 0; j < answers.length; j++) {

                if (list[i].answerto_id === answers[j].id) {
                    answerto.textContent = "Answer to: " + answers[j].text;
                    break;
                }
            }


            block.appendChild(answerto);

            block.appendChild(elem);

            const msg_id = document.createElement("h3");
            msg_id.className = "id_m";
            msg_id.textContent = list[i].id;

            block.appendChild(msg_id);

            const scroll = document.getElementById("chatroom");

            document.getElementById("chatroom").appendChild(block);
            scroll.scroll(0, 1000);
        }


    }



});





// получени общего уведомления
hubConnection.on("Notify", message => {

    const elem = document.createElement("p");
    elem.textContent = message;


    document.getElementById("chatroom").appendChild(elem);
});


//скрипт для изменения сообщения
$('body').on('click', '.msg_block_me', function () {
    /*console.log('jquery_' + $(this).text());*/
    var id = $(this).children("h3")[0].textContent;
    var text = $(this).find(".text_span")[0].textContent;
    document.getElementById("msg_id").textContent = id;
    document.getElementById("change_text").value = text;
    document.getElementById("changes_block").style.visibility = "visible";
    document.getElementById("answer_btn").disabled = true;
    document.getElementById("answer_btn_private").disabled = true;
});

$('body').on('click', '.msg_block_another', function () {
    /*console.log('jquery_' + $(this).text());*/
    document.getElementById("changes_block").style.visibility = "visible";
    document.getElementById("change_btn").disabled = true;
    document.getElementById("delete_btn_me").disabled = true;
    document.getElementById("delete_btn_all").disabled = true;
    var text = $(this).find(".text_span")[0].textContent;
    var id = $(this).children("h3")[0].textContent;
    var from = $(this).find(".name_span")[0].textContent;
    from = from.substring(0, from.length - 2);
    document.getElementById("answer_txt").textContent = text;
    document.getElementById("answer_from").textContent = from;
    document.getElementById("msg_id").textContent = id;
});




document.getElementById("back_btn").addEventListener("click", () => {
    document.getElementById("change_btn").disabled = false;
    document.getElementById("delete_btn_me").disabled = false;
    document.getElementById("delete_btn_all").disabled = false;
    document.getElementById("answer_btn").disabled = false;
    document.getElementById("answer_btn_private").disabled = false;
    document.getElementById("change_text").value = "";
    document.getElementById("changes_block").style.visibility = "hidden";
});

document.getElementById("change_btn").addEventListener("click", () => {
    let id = parseInt(document.getElementById("msg_id").textContent);
    var rec = document.getElementById("receiver").value;
    var text = document.getElementById("change_text").value;
    hubConnection.invoke("Change", text, rec, id)
        .catch(error => console.error(error));
});






hubConnection.on("ReceiveChange", (message, user, msgid, to_group) => {

    console.log("login: " + document.getElementById("login").value);

    if (document.getElementById("receiver").value !== to_group) {
        if (document.getElementById("receiver").value !== user) {
            if (document.getElementById("login").value !== user)
                return;
        }
    }

    var ids = document.getElementsByClassName("id_m");
    for (let i = 0; i < ids.length; i++) {
        if (parseInt(ids[i].textContent) === msgid) {
            document.getElementsByClassName("text_span")[i].textContent = message;
        }
    }

});


hubConnection.on("ReceiveNewChat", (receiver, sender) => {
    var user = document.getElementById("login").value;
    if (user === receiver) {
        var elem = document.getElementById("chatsList");
        var e = document.createElement("p");
        e.textContent = sender;
        e.className = "chatline";
        elem.appendChild(e);
    }
    else if (user === sender) {
        var elem = document.getElementById("chatsList");
        var e = document.createElement("p");
        e.textContent = receiver;
        e.className = "chatline";
        elem.appendChild(e);
        document.getElementById("receiver").value = receiver;
        var allchat = document.getElementById("chatroom").children;
        for (let i = 0; i < allchat.length; i++) {
            allchat[i].remove();
        }
    }

});




document.getElementById("delete_btn_all").addEventListener("click", () => {
    let id = parseInt(document.getElementById("msg_id").textContent);
    var rec = document.getElementById("receiver").value;
    hubConnection.invoke("DeleteForAll", rec, id)
        .catch(error => console.error(error));
});

hubConnection.on("ReceiveDeleteForAll", (user, msgid, to_group) => {

    if (document.getElementById("receiver").value !== to_group) {
        if (document.getElementById("receiver").value !== user) {
            if (document.getElementById("login").value !== user)
                return;
        }
    }

    var ids = document.getElementsByClassName("id_m");
    for (let i = 0; i < ids.length; i++) {
        if (parseInt(ids[i].textContent) === msgid) {
            let temp = document.getElementById("chatroom");
            temp.children[i].remove();
        }
    }
});

document.getElementById("delete_btn_me").addEventListener("click", () => {
    let id = parseInt(document.getElementById("msg_id").textContent);
    var rec = document.getElementById("receiver").value;
    hubConnection.invoke("DeleteForMe", rec, id)
        .catch(error => console.error(error));
});

hubConnection.on("ReceiveDeleteForMe", (user, msgid) => {

    var ids = document.getElementsByClassName("id_m");
    for (let i = 0; i < ids.length; i++) {
        if (parseInt(ids[i].textContent) === msgid) {
            let temp = document.getElementById("chatroom");
            temp.children[i].remove();
        }
    }
});






document.getElementById("answer_btn").addEventListener("click", () => {
    let id = parseInt(document.getElementById("msg_id").textContent);
    let message = document.getElementById("change_text").value;
    let receiver = document.getElementById("receiver").value;
    let ans_text = document.getElementById("answer_txt").textContent;
    hubConnection.invoke("Answer", message, receiver, ans_text, id)
        .catch(error => console.error(error));
});




hubConnection.on("ReceiveAnswer", (message, user, msgid, answer_text, to_group) => {

    if (document.getElementById("receiver").value !== to_group) {
        if (document.getElementById("receiver").value !== user) {
            if (document.getElementById("login").value !== user)
                return;
        }
    }

    const block = document.createElement("div");

    const login_name = document.getElementById("login");

    if (login_name.value === user)
        block.className = "msg_block_me";
    else
        block.className = "msg_block_another";

    const elem = document.createElement("p");

    const name_span = document.createElement("span");
    name_span.className = "name_span";
    const text_span = document.createElement("span");
    text_span.className = "text_span";

    name_span.textContent = `${user}: `;
    text_span.textContent = message;

    let br = document.createElement("br");

    elem.appendChild(name_span);
    elem.appendChild(br);
    elem.appendChild(text_span);

    const answerto = document.createElement("p");
    answerto.className = "answer_text";
    answerto.textContent = "Answer to: " + answer_text;

    block.appendChild(answerto);

    block.appendChild(elem);

    const msg_id = document.createElement("h3");
    msg_id.className = "id_m";
    msg_id.textContent = msgid;

    block.appendChild(msg_id);

    const scroll = document.getElementById("chatroom");

    document.getElementById("chatroom").appendChild(block);
    scroll.scroll(0, 1000);
});


document.getElementById("answer_btn_private").addEventListener("click", () => {
    let id = parseInt(document.getElementById("msg_id").textContent);
    let message = document.getElementById("change_text").value;
    let receiver = document.getElementById("answer_from").textContent;
    let ans_text = document.getElementById("answer_txt").textContent;

    hubConnection.invoke("AnswerPrivate", message, receiver, ans_text, id)
        .catch(error => console.error(error));
});


hubConnection.on("ReceiveAnswerPrivate", (message, user, msgid, answer_text) => {

    if (document.getElementById("receiver").value !== user) {
        if (document.getElementById("login").value !== user)
            return;
    }

    const block = document.createElement("div");

    const login_name = document.getElementById("login");

    if (login_name.value === user)
        block.className = "msg_block_me";
    else
        block.className = "msg_block_another";

    const elem = document.createElement("p");

    const name_span = document.createElement("span");
    name_span.className = "name_span";
    const text_span = document.createElement("span");
    text_span.className = "text_span";

    name_span.textContent = `${user}: `;
    text_span.textContent = message;

    let br = document.createElement("br");

    elem.appendChild(name_span);
    elem.appendChild(br);
    elem.appendChild(text_span);

    const answerto = document.createElement("p");
    answerto.className = "answer_text";
    answerto.textContent = "Answer to: " + answer_text;

    block.appendChild(answerto);

    block.appendChild(elem);

    const msg_id = document.createElement("h3");
    msg_id.className = "id_m";
    msg_id.textContent = msgid;

    block.appendChild(msg_id);

    const scroll = document.getElementById("chatroom");

    document.getElementById("chatroom").appendChild(block);
    scroll.scroll(0, 1000);
});