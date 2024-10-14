ImportAesKey = async function (key, name) {
    return await window.crypto.subtle.importKey(
        "raw",
        key,
        {
            name: name,
        },
        false,
        ["encrypt", "decrypt"]
    )
}

ImportPbkdf2Key = async function (key) {
    return await window.crypto.subtle.importKey(
        "raw",
        key,
        {
            name: "PBKDF2",
        },
        false,
        ["deriveKey", "deriveBits"]
    )
}

ConcatBytes = function (iv, salt, data) {
    const totalLength = iv.byteLength + salt.byteLength + data.byteLength;
    const result = new Uint8Array(totalLength);

    result.set(iv, 0);
    result.set(salt, iv.byteLength);
    result.set(data, iv.byteLength + salt.byteLength);

    return result;
}

DeriveKey = async function (key, name, salt, len, iter) {
    return await window.crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            salt: salt,
            iterations: iter,
            hash: { name: "SHA-256" },
        },
        key,
        {
            name: name,
            length: len,
        },
        false,
        ["encrypt", "decrypt"]
    );
}

window.AesEncrypt = async function (k, data, name, len, iter = 5000) {
    var iv = window.crypto.getRandomValues(new Uint8Array(16));
    var keySalt = window.crypto.getRandomValues(new Uint8Array(16));

    if (iter == 0) {
        var key = await ImportAesKey(k, name);
    } else {
        var key = await DeriveKey(await ImportPbkdf2Key(k), name, keySalt, len, iter);
    }

    var param = (name == "AES-CTR") ?
        {
            name: name,
            counter: iv,
            length: 128
        } :
        {
            name: name,
            iv: iv,
        };

    var encrypted = await window.crypto.subtle.encrypt(
        param,
        key,
        data
    );
    var result = ConcatBytes(iv, keySalt, new Uint8Array(encrypted));
    return result;
}

window.AesDecrypt = async function (k, data, name, len, iter = 5000) {
    var iv = data.slice(0, 16);
    var keySalt = data.slice(16, 32);
    var encrypted = data.slice(32, data.byteLength);

    if (iter == 0) {
        var key = await ImportAesKey(k, name);
    } else {
        var key = await DeriveKey(await ImportPbkdf2Key(k), name, keySalt, len, iter);
    }
    var param = (name == "AES-CTR") ?
        {
            name: name,
            counter: iv,
            length: 128
        } :
        {
            name: name,
            iv: iv,
        };

    var decrypted = await window.crypto.subtle.decrypt(
        param,
        key,
        encrypted
    );
    return new Uint8Array(decrypted);
}