function blazorFileDownload(filename, contentType, content) {
    const file = new File([content], filename, { type: contentType });
    const exportUrl = URL.createObjectURL(file);
    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = "_self";
    a.click();
    URL.revokeObjectURL(exportUrl);
    document.body.removeChild(a);
}

function fallbackCopyTextToClipboard(text) {
    var textArea = document.createElement("textarea");
    textArea.value = text;

    textArea.style.top = "0";
    textArea.style.left = "0";
    textArea.style.position = "fixed";

    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        var successful = document.execCommand('copy');
        if (successful) {
            return "Success";
        }
        else {
            return "Failed";
        }
    } catch (err) {
        return err;
    }

    document.body.removeChild(textArea);
}

function copyTextToClipboard(text) {
    if (!navigator.clipboard) {
        return fallbackCopyTextToClipboard(text);
    }
    navigator.clipboard.writeText(text);
    return "Success";
}