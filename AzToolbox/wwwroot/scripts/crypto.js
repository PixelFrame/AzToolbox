ImportAesKey = async function (k) {
    return await window.crypto.subtle.importKey(
        "jwk",
        {
            kty: "oct",
            k: k,
            alg: "A256CBC",
            ext: true,
        },
        {
            name: "AES-CBC",
        },
        false,
        ["encrypt", "decrypt"]
    )
}

PrependIV = function (iv, data) {
    const totalLength = iv.byteLength + data.byteLength;
    const result = new Uint8Array(totalLength);

    result.set(iv, 0);
    result.set(data, iv.byteLength);

    return result;
}


window.AesEncrypt = async function (k, data) {
    var key = await ImportAesKey(k);
    var iv = window.crypto.getRandomValues(new Uint8Array(16));
    var encrypted = await window.crypto.subtle.encrypt(
        {
            name: "AES-CBC",
            iv: iv,
        },
        key,
        data
    );
    var result = PrependIV(iv, new Uint8Array(encrypted));
    return result;
}

window.AesDecrypt = async function (k, data) {
    var key = await ImportAesKey(k);
    var iv = data.slice(0, 16);
    var encrypted = data.slice(16, data.byteLength);
    var decrypted = await window.crypto.subtle.decrypt(
        {
            name: "AES-CBC",
            iv: iv,
        },
        key,
        encrypted
    );
    return new Uint8Array(decrypted);
}