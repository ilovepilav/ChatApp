'use strict';

const userName = document.querySelector('#Username').textContent;
const convList = document.getElementById('conversationList');
const chatWindow = document.querySelector('.messageArea');
let conversationData;

const connection = new signalR.HubConnectionBuilder()
    .withUrl('/chatHub')
    .build();

connection.on('ClientsRefreshed', function (data) {
    const messageBox = document.getElementById('onlineClients');
    messageBox.innerHTML = '';
    data.forEach(function (item) {
        if (item != userName) {
            let element = document.createElement('li');
            element.className = 'online-client';
            element.setAttribute('onclick', 'ChangeActiveElementChatRoom(this);');
            element.textContent = item;
            element.id = item + '_online';
            messageBox.appendChild(element);
        }
    });
});

connection.on('Conversations', function (data) {
    convList.innerHTML = '';
    conversationData = data;
    data.forEach(function (conversationData) {
        FillConversations(conversationData);
    });
});

connection.on('RefreshChatRooms', function (data) {
    RefreshGroupChatList(data);
});

connection.on('NewConversation', function (data) {
    conversationData.unshift(data);
    FillConversations(data, true);
});

connection.on('NewMessage', function (receiver, messageDto) {
    let span = document.createElement('span');
    span.className =
        'newMessageSpan';
    span.textContent = '1';
    if (userName === messageDto.sender) {
        AddNewMessageToConversation(messageDto);
        AppendToConvData(receiver, messageDto);
        return;
    }
    if (userName === receiver) {
        if (
            document.getElementById(messageDto.sender).classList.contains('active')
        ) {
            AddNewMessageToConversation(messageDto);
            AppendToConvData(messageDto.sender, messageDto);
            return;
        }
        let receiverTab = document.getElementById(messageDto.sender);
        let existingSpan = receiverTab.querySelector('.mediaBody > span');
        if (existingSpan == null) {
            receiverTab.querySelector('.mediaBody').prepend(span);
        } else {
            existingSpan.textContent = Number(existingSpan.textContent) + 1;
        }
        let text = receiverTab.querySelector('.mediaBody');
        text.classList.add('notify');
        AppendToConvData(messageDto.sender, messageDto);
        return;
    }
    if (document.getElementById(receiver).classList.contains('active')) {
        AddNewMessageToConversation(messageDto);
        AppendToConvData(receiver, messageDto);
        return;
    }
    let receiverTab = document.getElementById(receiver);
    let existingSpan = receiverTab.querySelector('.mediaBody > span');
    if (existingSpan == null) {
        receiverTab.querySelector('.mediaBody').prepend(span);
    } else {
        existingSpan.textContent = Number(existingSpan.textContent) + 1;
    }
    let text = receiverTab.querySelector('.mediaBody');
    text.classList.add('notify');
    AppendToConvData(receiver, messageDto);
    return;
});

connection.on('Error', function (data) {
    const toastElement = document.querySelector('.toast');
    toastElement.querySelector('h5').textContent = 'Error';
    toastElement.querySelector('.toast-body').textContent = data;
    toastElement.classList.add('active');
    setTimeout(() => {
        toastElement.classList.add('fadeOut');
    }, 2000);
    toastElement.classList.remove('fadeOut');
});

connection.start();
