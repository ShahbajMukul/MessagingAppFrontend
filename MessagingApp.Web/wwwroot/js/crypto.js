async function generateRSAKeys() {
    console.log("Attempting to generate RSA keys via JS...");
    try {
        const keyPair = await window.crypto.subtle.generateKey(
            {
                name: "RSA-OAEP",
                modulusLength: 2048,
                publicExponent: new Uint8Array([1, 0, 1]),
                hash: "SHA-256",
            },
            true,
            ["encrypt", "decrypt"]
        );

        const privateKeyJwk = await window.crypto.subtle.exportKey("jwk", keyPair.privateKey);
        console.log("Keys generated and exported as JWK.");

        // Return structure matching C# class (JWK components)
        return {
            n: privateKeyJwk.n,  // Modulus (Base64Url)
            e: privateKeyJwk.e,  // Exponent (Base64Url)
            d: privateKeyJwk.d,  // Private Exponent (Base64Url)
            p: privateKeyJwk.p,  // First prime factor (Base64Url)
            q: privateKeyJwk.q,  // Second prime factor (Base64Url)
            dp: privateKeyJwk.dp, // First factor CRT exponent (Base64Url)
            dq: privateKeyJwk.dq, // Second factor CRT exponent (Base64Url)
            qi: privateKeyJwk.qi  // First CRT coefficient (InverseQ) (Base64Url)
        };
    } catch (error) {
        console.error("Error generating RSA keys in JS:", error);
        throw error;
    }
}