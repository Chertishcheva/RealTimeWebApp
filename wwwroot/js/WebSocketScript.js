const url = "/ws";

var webSocket = new WebSocket(url);

webSocket.onmessage = function (message) {
    tableUpdate();
    webSocket.onmessage = (message) => {
        let object = JSON.parse(message.data);
        entityUpdate(object);
    }
}

onbeforeunload = (event) => {
    webSocket.close();   
};
