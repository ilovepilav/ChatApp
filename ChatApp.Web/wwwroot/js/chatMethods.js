function FillConversations(item, appendtop = false) {
  let child = document.createElement('div');
  child.className = 'conversation-item';
  child.id = item.receipent;
  child.setAttribute('onclick', `FillMessages(this);return false;`);
  child.innerHTML = `<div class="mediaHead">
                      <h4 class="receipentName"></h4>
                      <small></small>
                      </div>
                      <div class="mediaBody">
                      <p class="messageContent">
                      </p>
                      </div>`;
  child.querySelector('h4').textContent = item.receipent;
  if (item.privateChat) {
    child.setAttribute('data-PrivateChat', 'true');
  } else {
    child.setAttribute('data-PrivateChat', 'false');
  }
  if (item.messages.length > 0) {
    child.querySelector('small').textContent = moment(
      item.messages[item.messages.length - 1].date
    ).calendar();
    child.querySelector('p').textContent =
      item.messages[item.messages.length - 1].content;
  } else {
    child.querySelector('small').textContent = '';

    child.querySelector('p').textContent = 'Wow! Such Empty.';
  }
  if (appendtop) return convList.prepend(child);
  convList.appendChild(child);
}

function FillMessages(element) {
  ChangeActiveElement(element);
  element.classList.add('active');
  conversationData.forEach(function (item) {
    if (item.receipent === element.id) {
      chatWindow.innerHTML = '';
      item.messages.forEach(function (msg) {
        let msgElement = document.createElement('div');
        if (msg.sender != userName) {
            msgElement.className = 'message received';
            if (item.privateChat == false) {
                let senderName = document.createElement('small');
                senderName.textContent = msg.sender;
                msgElement.appendChild(senderName);
            }
          let messageContent = document.createElement('p');
          messageContent.textContent = msg.content;
          msgElement.appendChild(messageContent);
        } else {
          msgElement.className = 'message sent';
          let messageContent = document.createElement('p');
          messageContent.textContent = msg.content;
          msgElement.appendChild(messageContent);
        }
        chatWindow.appendChild(msgElement);
      });
    }
  });
    ScrollToBottom();
    const menuElement = document.querySelector(".chatLeft");
    if (menuElement.classList.contains('mobile-active')) {
        menuElement.classList.remove('mobile-active');
        return;
    }
}

function ChangeActiveElement(element) {
  const parentElement = element.parentElement;
  const spanElement = element.querySelector('span');
  if (spanElement != null) {
    spanElement.parentElement.removeChild(spanElement);
  }
  parentElement.childNodes.forEach(function (node) {
    node.classList.remove('active');
  });
    element.classList.add('active');
    element.querySelector('.mediaBody').classList.remove('notify');

}

function RefreshGroupChatList(data) {
  let chatList = document.querySelector('#groupChatList');
  chatList.innerHTML = '';
  data.forEach(function (item) {
    let ele = document.createElement('li');
    ele.className = 'chat-room';
    ele.setAttribute(
      'onclick',
      `ChangeActiveElementChatRoom(this);return false;`
    );
    ele.textContent = item;
    chatList.appendChild(ele);
  });
}

function ChangeActiveElementChatRoom(element) {
  const parentElement = element.parentElement;
  parentElement.childNodes.forEach(function (node) {
    node.classList.remove('active');
  });
  element.classList.add('active');
}

function AddNewMessageToConversation(messageDto) {
  let msgElement = document.createElement('div');
  if (messageDto.sender != userName) {
    msgElement.className = 'message received';
    msgElement.innerHTML = `<p></p>`
    msgElement.querySelector('p').textContent = messageDto.content;
    
  } else {
      msgElement.className = 'message sent';
      msgElement.innerHTML = `<p></p>`
      msgElement.querySelector('p').textContent = messageDto.content;

  }
  chatWindow.appendChild(msgElement);
}

function AppendToConvData(receipent, messageDto) {
  for (let i = 0; i < conversationData.length; i++) {
    if (conversationData[i].receipent === receipent) {
      conversationData[i].messages.push(messageDto);
      const temp = conversationData[i];
      conversationData.splice(i, 1);
      conversationData.unshift(temp);
      break;
    }
  }
  document.getElementById(receipent).querySelector('p').textContent =
    messageDto.content;
  document.getElementsByTagName('small').textContent = moment(
    messageDto.date
  ).calendar();
  const lastReceipent = document.getElementById(receipent);
  ReArrangeConversations(lastReceipent);
}

function ReArrangeConversations(lastReceipent) {
  if (convList.firstChild != lastReceipent) {
    const temp = convList.removeChild(lastReceipent);
    convList.prepend(temp);
  }
}

async function ScrollToBottom() {
  const height = chatWindow.scrollHeight;
  chatWindow.scrollTop = height;
}
