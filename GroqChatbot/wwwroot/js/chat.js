"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

// Disable the send button until connection is established.
document.getElementById("submit").disabled = true;

var messageElement;

connection.on("ChatCompletionChunk", function (user, chunk) {
    if (!messageElement) {
        messageElement = document.createElement("div");
        messageElement.style.width = "100%";
        messageElement.style.borderTop = "1px solid #ccc";
        messageElement.style.paddingTop = "5x";
        messageElement.style.paddingBottom = "5x";

        document.getElementById("messages").appendChild(messageElement);
    }

    messageElement.innerHTML += chunk;
});

connection.on("ChatCompletionFinish", function () {
    messageElement = null;
    document.getElementById("message").focus();
});

connection.start().then(function () {
    document.getElementById("submit").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("submit").addEventListener("click", function (event) {
    var messageElement = document.getElementById("message");
    var message = messageElement.value;

    // Invio il messaggio all'hub /chat.
    connection.invoke("ChatCompletion", message).catch(function (err) {
        return console.error(err.toString());
    });

    messageElement.value = "";

    event.preventDefault();
});