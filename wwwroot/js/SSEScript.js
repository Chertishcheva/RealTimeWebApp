url = "/sse";

var eventSource = new EventSource(url);

eventSource.onmessage = (message) => {
    tableUpdate();

    eventSource.onmessage = (message) => {
        let object = JSON.parse(message.data);
        entityUpdate(object);
    }
};
onbeforeunload = (event) => {
    eventSource.close();
};