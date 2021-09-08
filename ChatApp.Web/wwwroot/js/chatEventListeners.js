document.getElementById('sendButton').addEventListener('click', function (e) {
  e.preventDefault();
    const message = document.getElementById('message').value;
    if (message.trim() == "") {
        return;
    }
  const receiver = document
    .querySelector('#conversationList > .active')
    .querySelector('h4').textContent;
  const isPrivate = document
    .querySelector('#conversationList > .active')
    .getAttribute('data-privatechat');
  connection.invoke('SendMessage', receiver, message, JSON.parse(isPrivate));
  document.getElementById('message').value = '';
    setTimeout(() => {
        ScrollToBottom();
    }, 100);
});

document.querySelector('#createRoom').addEventListener('click', (e) => {
  e.preventDefault();
    const roomName = document.querySelector('.chatRoomName');
    if (roomName.value.trim() == "") {
        return;
    }
  connection.invoke('CreateChatRoom', roomName.value);
    roomName.value = '';
    const menuElement = document.querySelector(".groupChatArea");
    if (menuElement.classList.contains('mobile-active')) {
        menuElement.classList.remove('mobile-active');
        return;
    }
});

document.querySelector('#joinRoom').addEventListener('click', function (e) {
  e.preventDefault();
  const chatRoomName = document.querySelector(
    '#groupChatList > .active'
  ).textContent;
    connection.invoke('JoinChatRoom', chatRoomName);
    const menuElement = document.querySelector(".groupChatArea");
    if (menuElement.classList.contains('mobile-active')) {
        menuElement.classList.remove('mobile-active');
        return;
    }
});

document.querySelector('#createChat').addEventListener('click', (e) => {
  e.preventDefault();
  const receipent = document.querySelector(
    '#onlineClients > .active'
    ).textContent;
    
    connection.invoke('CreateChat', receipent);

    if (menuElement.classList.contains('mobile-active')) {
        menuElement.classList.remove('mobile-active');
        return;
    }
});

document.querySelector("#mobile-online-users-menu").addEventListener('click', (e) => {
    e.preventDefault();
    const menuElement = document.querySelector(".onlineArea");
    if (menuElement.classList.contains('mobile-active')) {
        menuElement.classList.remove('mobile-active');
        return;
    } else {
        menuElement.classList.add('mobile-active');
        document.querySelector(".chatLeft").classList.remove('mobile-active');
        document.querySelector(".groupChatArea").classList.remove('mobile-active');
    }
    
});

document.querySelector("#mobile-conversations-menu").addEventListener('click', (e) => {
    e.preventDefault();
    const menuElement = document.querySelector(".chatLeft");
    if (menuElement.classList.contains('mobile-active')) {
        menuElement.classList.remove('mobile-active');
        return;
    } else {
        menuElement.classList.add('mobile-active');
        document.querySelector(".onlineArea").classList.remove('mobile-active');
        document.querySelector(".groupChatArea").classList.remove('mobile-active');
    }

});

document.querySelector("#mobile-chatrooms-menu").addEventListener('click', (e) => {
    e.preventDefault();
    const menuElement = document.querySelector(".groupChatArea");
    if (menuElement.classList.contains('mobile-active')) {
        menuElement.classList.remove('mobile-active');
        return;
    } else {
        menuElement.classList.add('mobile-active');
        document.querySelector(".onlineArea").classList.remove('mobile-active');
        document.querySelector(".chatLeft").classList.remove('mobile-active');
    }

});

document.querySelector('.menu-buttons img').addEventListener('click', () => {
    const sidebar = document.querySelector('.nav-items');
    if (sidebar.classList.contains('mobile-active')) {
        sidebar.classList.remove('mobile-active')
        return;
    }
    sidebar.classList.add('mobile-active');

});