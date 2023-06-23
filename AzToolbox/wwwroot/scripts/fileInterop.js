function pickFileAsync() {
    return new Promise(r => {
        const file = document.createElement("input");
        file.type = "file";
        file.onchange = () => r(file.files[0]);
        file.click();
    });
}