const url = "/longpol";
var cancelPol = true;

tableUpdate();
call();

async function sendRequest() {
    const response = await fetch(url);
    if (response.status == 200) {
        const object = await response.json();
        return (object);
    }
    return null;
}

async function call() {
    while (cancelPol) {
        var object = await sendRequest();
        if (object === null) 
            console.log("No object was sent");
        else 
            entityUpdate(object);  
    }
}

onbeforeunload = (event) => {
    cancelPol = false;
};