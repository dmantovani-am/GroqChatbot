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

    var model = document.getElementById("model").value;
    var temperature = parseFloat(document.getElementById("temperature").value);
    var maxTokens = parseInt(document.getElementById("maxTokens").value, 10);

    // Invio il messaggio all'hub /chat.
    connection.invoke("ChatCompletion", message, model, temperature, maxTokens).catch(function (err) {
        return console.error(err.toString());
    });

    messageElement.value = "";

    event.preventDefault();
});

document.getElementById("clear").addEventListener("click", function (event) {
    connection.invoke("Clear").catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById("messages").innerHTML = "";

    event.preventDefault();
});