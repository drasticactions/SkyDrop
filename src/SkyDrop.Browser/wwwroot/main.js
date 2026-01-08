import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

// CORS proxy for loading external images (Bluesky CDN doesn't support CORS)
// For production, consider hosting your own proxy
const CORS_PROXY = "https://corsproxy.io/?";

// Fetch image as bytes, using CORS proxy if needed
globalThis.fetchImageAsBytes = async function(url) {
    try {
        // First try direct fetch (for same-origin or CORS-enabled URLs)
        let response = await fetch(url);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const blob = await response.blob();
        const arrayBuffer = await blob.arrayBuffer();
        return new Uint8Array(arrayBuffer);
    } catch (e) {
        // If direct fetch fails (likely CORS), try with proxy
        try {
            const proxyUrl = CORS_PROXY + encodeURIComponent(url);
            const response = await fetch(proxyUrl);

            if (!response.ok) {
                console.warn(`Image fetch failed: ${url}`);
                return null;
            }

            const blob = await response.blob();
            const arrayBuffer = await blob.arrayBuffer();
            return new Uint8Array(arrayBuffer);
        } catch (proxyError) {
            console.warn(`Image fetch with proxy failed: ${url}`, proxyError);
            return null;
        }
    }
};

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = dotnetRuntime.getConfig();

await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);
